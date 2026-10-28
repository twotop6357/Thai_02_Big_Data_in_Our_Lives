using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Exhibition.Blocks
{
    public sealed class ImageRetentionReport
    {
        public int RemainingFiles, DeletedFiles, FailedDeletes;
        public long RemainingBytes, DeletedBytes;
        public bool WithinLimits;
        public string FirstError;
    }

    /// <summary>Only the direct Thai02/blocks_GUID_counter_.png files belong to this policy.</summary>
    public static class GeneratedImageRetentionPolicy
    {
        private static readonly Regex OwnedName = new Regex(@"\Ablocks_[0-9a-f]{32}_[0-9]+_\.png\z",
            RegexOptions.CultureInvariant);

        public static bool IsOwnedName(string name) => name != null && OwnedName.IsMatch(name);

        public static string ValidateOutputDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathRooted(directory))
                throw new ArgumentException("ComfyUI output directory must be an absolute path.");
            string normalized = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string root = Path.GetFullPath(normalized).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!string.Equals(Path.GetPathRoot(normalized), Path.GetPathRoot(Path.GetFullPath(normalized)), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Drive-relative output paths are not allowed.");
            if (root.Length == 0 || root == Path.GetPathRoot(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                throw new ArgumentException("A drive root cannot be used as the ComfyUI output directory.");
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("ComfyUI output directory does not exist.");
            CheckNoLinks(root);
            return root;
        }

        private static void CheckNoLinks(string path)
        {
            for (var info = new DirectoryInfo(path); info != null; info = info.Parent)
                if (info.Exists && (info.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Image retention refuses symbolic links or junctions in its directory path.");
        }

        public static ImageRetentionReport Sweep(string outputDirectory, int maxFiles, long maxBytes,
            TimeSpan writeGrace, DateTime nowUtc, string protectedRequestId = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (maxFiles < 1 || maxBytes < 1 || writeGrace < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Image retention limits must be positive.");
            string root = ValidateOutputDirectory(outputDirectory);
            string folder = Path.GetFullPath(Path.Combine(root, "Thai02"));
            // Validate the exact resolved child before any enumeration or deletion.
            if (!string.Equals(Path.GetDirectoryName(folder), root, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Image retention folder escaped the configured output directory.");
            var report = new ImageRetentionReport { WithinLimits = true };
            if (!Directory.Exists(folder)) return report;
            CheckNoLinks(folder);
            var files = new List<FileInfo>();
            foreach (var file in new DirectoryInfo(folder).EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                token.ThrowIfCancellationRequested();
                if (!IsOwnedName(file.Name) || (file.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                files.Add(file);
                report.RemainingFiles++;
                report.RemainingBytes += file.Length;
            }
            files.Sort((a, b) => {
                int order = a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc);
                return order != 0 ? order : string.CompareOrdinal(a.Name, b.Name);
            });
            foreach (var file in files)
            {
                token.ThrowIfCancellationRequested();
                if (report.RemainingFiles <= maxFiles && report.RemainingBytes <= maxBytes) break;
                if (protectedRequestId != null && file.Name.StartsWith("blocks_" + protectedRequestId + "_", StringComparison.Ordinal)) continue;
                if (nowUtc - file.LastWriteTimeUtc < writeGrace) continue;
                long length = file.Length;
                DateTime modified = file.LastWriteTimeUtc;
                try
                {
                    CheckNoLinks(folder);
                    file.Refresh();
                    if (!file.Exists) { report.RemainingFiles--; report.RemainingBytes -= length; continue; }
                    if ((file.Attributes & FileAttributes.ReparsePoint) != 0 || file.Length != length || file.LastWriteTimeUtc != modified)
                        continue; // The file changed since enumeration: never delete an active writer's output.
                    if (!string.Equals(file.DirectoryName, folder, StringComparison.OrdinalIgnoreCase))
                        throw new IOException("Image retention file escaped its project folder.");
                    file.Delete();
                    report.RemainingFiles--;
                    report.RemainingBytes -= length;
                    report.DeletedFiles++;
                    report.DeletedBytes += length;
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    report.FailedDeletes++;
                    if (report.FirstError == null) report.FirstError = ex.Message;
                }
            }
            report.WithinLimits = report.RemainingFiles <= maxFiles && report.RemainingBytes <= maxBytes;
            return report;
        }
    }
}
