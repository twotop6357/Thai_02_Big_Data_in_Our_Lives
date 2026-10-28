using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Exhibition.Blocks
{
    public static class LocalBlockColoring
    {
        public static async UniTask<Texture2D> RenderAsync(BlockColoringTemplate template, Color32 primary,
            Color32 secondary, CancellationToken token)
        {
            Texture2D source = null, mask = null, result = null;
            try
            {
                source = await LoadPngAsync(template.baseImage, token);
                mask = await LoadPngAsync(template.maskImage, token);
                if (source.width != mask.width || source.height != mask.height)
                    throw new InvalidDataException("Base image and mask dimensions differ: " + template.baseImage);
                var pixels = source.GetPixels32();
                var regions = mask.GetPixels32();
                var colored = await UniTask.RunOnThreadPool(() => BlockColorizer.Colorize(pixels, regions, primary, secondary, token),
                    cancellationToken: token);
                token.ThrowIfCancellationRequested();
                result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                result.name = "Colored_" + template.groupId + "_" + template.resolvedProductId;
                result.SetPixels32(colored);
                result.Apply(false, true);
                var owned = result;
                result = null;
                return owned;
            }
            finally
            {
                // Cancellation can arrive immediately after the worker-thread switch.
                await UniTask.SwitchToMainThread();
                if (source) UnityEngine.Object.Destroy(source);
                if (mask) UnityEngine.Object.Destroy(mask);
                if (result) UnityEngine.Object.Destroy(result);
            }
        }

        private static async UniTask<Texture2D> LoadPngAsync(string file, CancellationToken token)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "BlockGeneration", file);
            byte[] bytes;
            using (var request = UnityWebRequest.Get(new Uri(path).AbsoluteUri))
            {
                request.timeout = 15;
                try { await request.SendWebRequest().ToUniTask(cancellationToken: token); }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { throw new IOException("Could not read coloring PNG: " + file, ex); }
                bytes = request.downloadHandler.data;
            }
            // Bound allocations before Unity decodes uncompressed PNG pixels.
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (bytes == null || bytes.Length < 33 || bytes.Length > 16 * 1024 * 1024)
                throw new InvalidDataException("Invalid coloring PNG size: " + file);
            for (int i = 0; i < signature.Length; i++)
                if (bytes[i] != signature[i]) throw new InvalidDataException("Coloring image must be PNG: " + file);
            uint width = ReadUInt32(bytes, 16), height = ReadUInt32(bytes, 20);
            if (width < 1 || height < 1 || width > 2048 || height > 2048)
                throw new InvalidDataException("Coloring PNG dimensions must be 1..2048: " + file);
            token.ThrowIfCancellationRequested();
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, bytes, false)) throw new InvalidDataException("Could not decode coloring PNG: " + file);
                return texture;
            }
            catch { UnityEngine.Object.Destroy(texture); throw; }
        }

        private static uint ReadUInt32(byte[] bytes, int offset) =>
            ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) | ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
