using System;

namespace Exhibition.Blocks
{
    public enum BlockSlot { Group, Product, Color1, Color2 }

    [Serializable]
    public sealed class BlockOption
    {
        public string id, label, prompt, primaryPart, secondaryPart;
    }

    [Serializable]
    public sealed class BlockCatalog
    {
        public int schemaVersion;
        public string stylePrefix, styleSuffix, negativePrompt;
        public BlockOption[] groups, products, colors, equipmentVariants;

        public BlockOption[] Options(BlockSlot slot) => slot == BlockSlot.Group ? groups :
            slot == BlockSlot.Product ? products : colors;

        public BlockOption Find(BlockSlot slot, string id)
        {
            foreach (var option in Options(slot)) if (option.id == id) return option;
            throw new ArgumentException("Unsupported block ID: " + id);
        }

        public void Validate()
        {
            if (schemaVersion != 1 || string.IsNullOrWhiteSpace(styleSuffix) || string.IsNullOrWhiteSpace(negativePrompt))
                throw new ArgumentException("Invalid block catalog schema or prompts.");
            foreach (var options in new[] { groups, products, colors, equipmentVariants })
            {
                if (options == null || options.Length == 0) throw new ArgumentException("Empty block catalog.");
                var ids = new System.Collections.Generic.HashSet<string>();
                foreach (var option in options)
                    if (option == null || string.IsNullOrWhiteSpace(option.id) ||
                        string.IsNullOrWhiteSpace(option.label) || string.IsNullOrWhiteSpace(option.prompt) || !ids.Add(option.id))
                        throw new ArgumentException("Invalid or duplicate block option.");
            }
            foreach (var option in products)
                if (string.IsNullOrWhiteSpace(option.primaryPart) || string.IsNullOrWhiteSpace(option.secondaryPart))
                    throw new ArgumentException("Missing product color parts.");
        }
    }

    public sealed class BlockSelection
    {
        private readonly string[] _ids = new string[4];
        public int Revision { get; private set; }
        public string this[BlockSlot slot] => _ids[(int)slot];
        public bool Complete => _ids[0] != null && _ids[1] != null && _ids[2] != null && _ids[3] != null;

        public void Set(BlockCatalog catalog, BlockSlot slot, string id)
        {
            catalog.Find(slot, id);
            if (_ids[(int)slot] == id) return;
            _ids[(int)slot] = id;
            Revision++;
        }

        public void Remove(BlockSlot slot)
        {
            if (_ids[(int)slot] == null) return;
            _ids[(int)slot] = null;
            Revision++;
        }

        public void Clear()
        {
            for (int i = 0; i < 4; i++) _ids[i] = null;
            Revision++;
        }

        public BlockPrompt CreatePrompt(BlockCatalog catalog, int equipmentIndex)
        {
            if (!Complete) throw new InvalidOperationException("All four blocks are required.");
            var group = catalog.Find(BlockSlot.Group, _ids[0]);
            var product = catalog.Find(BlockSlot.Product, _ids[1]);
            var first = catalog.Find(BlockSlot.Color1, _ids[2]);
            var second = catalog.Find(BlockSlot.Color2, _ids[3]);
            var variant = product.id == "equipment" ? catalog.equipmentVariants[equipmentIndex] : null;
            string subject = variant != null ? variant.prompt : product.prompt;
            string primary = "(" + first.prompt + " " + product.primaryPart + ":1.15)";
            string secondary = "(" + second.prompt + " " + product.secondaryPart + ":1.15)";
            return new BlockPrompt {
                groupId = group.id, productId = product.id, color1 = first.id, color2 = second.id,
                resolvedProduct = variant != null ? variant.id : product.id,
                audienceFragment = group.prompt, productFragment = subject,
                primaryFragment = primary, secondaryFragment = secondary,
                positive = string.Join(", ", catalog.stylePrefix, subject, primary, secondary, group.prompt, catalog.styleSuffix),
                negative = catalog.negativePrompt
            };
        }
    }

    [Serializable]
    public sealed class BlockPrompt
    {
        public string groupId, productId, color1, color2, resolvedProduct;
        public string audienceFragment, productFragment, primaryFragment, secondaryFragment, positive, negative;
    }
}
