using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exhibition.Blocks
{
    [Serializable]
    public sealed class BlockColoringTemplate
    {
        public string groupId, productId, resolvedProductId, baseImage, maskImage;
    }

    [Serializable]
    public sealed class BlockColoringColor
    {
        public string id, hex;
    }

    [Serializable]
    public sealed class BlockColoringCatalog
    {
        public int schemaVersion;
        public BlockColoringTemplate[] templates;
        public BlockColoringColor[] colors;

        public BlockColoringTemplate Find(string group, string product)
        {
            foreach (var item in templates)
                if (item.groupId == group && item.productId == product) return item;
            throw new ArgumentException("No coloring template for " + group + "/" + product + ".");
        }

        public Color32 GetColor(string id)
        {
            foreach (var item in colors)
                if (item.id == id && ColorUtility.TryParseHtmlString(item.hex, out var value)) return value;
            throw new ArgumentException("Invalid coloring palette ID: " + id);
        }

        public void Validate(BlockCatalog blocks)
        {
            if (schemaVersion != 1 || templates == null || colors == null)
                throw new ArgumentException("Invalid coloring catalog schema.");
            var keys = new HashSet<string>();
            foreach (var item in templates)
            {
                if (item == null) throw new ArgumentException("Null coloring template.");
                blocks.Find(BlockSlot.Group, item.groupId);
                var product = blocks.Find(BlockSlot.Product, item.productId);
                if (!keys.Add(item.groupId + "/" + item.productId)) throw new ArgumentException("Duplicate coloring template.");
                bool resolved = item.resolvedProductId == item.productId;
                if (product.id == "equipment")
                {
                    resolved = false;
                    foreach (var variant in blocks.equipmentVariants)
                        if (variant.id == item.resolvedProductId) resolved = true;
                }
                if (!resolved) throw new ArgumentException("Invalid resolved coloring product.");
                ValidatePath(item.baseImage);
                ValidatePath(item.maskImage);
            }
            foreach (var group in blocks.groups)
            foreach (var product in blocks.products) Find(group.id, product.id);
            keys.Clear();
            foreach (var item in colors)
            {
                if (item == null || !keys.Add(item.id)) throw new ArgumentException("Invalid or duplicate coloring color.");
                blocks.Find(BlockSlot.Color1, item.id);
                if (item.hex == null || item.hex.Length != 7 || item.hex[0] != '#' ||
                    !ColorUtility.TryParseHtmlString(item.hex, out _)) throw new ArgumentException("Use #RRGGBB palette colors.");
            }
            foreach (var color in blocks.colors) GetColor(color.id);
        }

        private static void ValidatePath(string path)
        {
            // Paths are relative to BlockGeneration; never allow a server URL or parent traversal.
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("Coloring/", StringComparison.Ordinal) ||
                !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.Contains("..") ||
                path.Contains(":") || path.Contains("\\") || path.Contains("?") || path.Contains("#"))
                throw new ArgumentException("Invalid local coloring image path: " + path);
        }
    }
}
