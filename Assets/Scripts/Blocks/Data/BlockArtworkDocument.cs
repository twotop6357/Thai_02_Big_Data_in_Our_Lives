using System;
using System.Collections.Generic;

namespace Exhibition.Blocks
{
    [Serializable]
    public sealed class BlockArtworkPath
    {
        public string kind, id, image;
        public BlockKind Kind => kind == "group" ? BlockKind.Group : kind == "product" ? BlockKind.Product :
            kind == "color" ? BlockKind.Color : throw new ArgumentException("Unknown block artwork kind: " + kind);
        public string Key => kind + "/" + id;
    }

    [Serializable]
    public sealed class BlockArtworkDocument
    {
        public int schemaVersion;
        public string language;
        public BlockArtworkPath[] blocks;

        public void Validate(string expectedLanguage)
        {
            if (schemaVersion != 1 || language != expectedLanguage || (language != "en" && language != "th"))
                throw new ArgumentException("Invalid block artwork schema or language.");
            if (blocks == null || blocks.Length == 0 || blocks.Length > 64)
                throw new ArgumentException("Block artwork entries must contain 1 through 64 items.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in blocks)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id)) throw new ArgumentException("Block artwork ID is missing.");
                var kind = entry.Kind;
                if (!keys.Add(entry.Key)) throw new ArgumentException("Duplicate block artwork: " + entry.Key);
                ValidateImagePath(entry.image);
            }
        }

        public static void ValidateImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(new[] { ':', '\\', '?', '#' }) >= 0 || path.StartsWith("/"))
                throw new ArgumentException("Block image must be a relative PNG or JPG path.");
            foreach (var part in path.Split('/'))
                if (part == ".." || part == "." || part.Length == 0) throw new ArgumentException("Invalid block image path segment.");
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Block image must be PNG or JPG.");
        }
    }
}
