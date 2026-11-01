using System;

namespace Exhibition.Blocks
{
    public enum BlockSlot { Group, Product, Color1, Color2 }

    [Serializable]
    public sealed class BlockAgeVariant
    {
        public string groupId, label, prompt, primaryPart, secondaryPart;
    }

    [Serializable]
    public sealed class BlockOption
    {
        public string id, label, prompt, primaryPart, secondaryPart, negativePrompt, hex;
        public string poseGuide;
        public float poseStrength, poseEndPercent;
        public float poseLowThreshold = .02f, poseHighThreshold = .06f;
        public BlockAgeVariant[] ageVariants;

        public BlockAgeVariant ForGroup(string groupId)
        {
            if (ageVariants == null || ageVariants.Length == 0) return null;
            foreach (var variant in ageVariants)
                if (variant.groupId == groupId) return variant;
            throw new ArgumentException("Missing product design for audience: " + id + "/" + groupId);
        }

        public string ColorReference
        {
            get
            {
                if (!UnityEngine.ColorUtility.TryParseHtmlString(hex, out var color))
                    throw new ArgumentException("Invalid block resource color: " + id);
                UnityEngine.Color32 rgb = color;
                return hex.ToUpperInvariant() + " RGB(" + rgb.r + ", " + rgb.g + ", " + rgb.b + ")";
            }
        }
    }

    [Serializable]
    public sealed class BlockCatalog
    {
        public int schemaVersion;
        public string stylePrefix, styleSuffix, negativePrompt;
        public string promptSeparator;
        public bool numericColorPrompt;
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
            if (promptSeparator != null && promptSeparator != ", " && promptSeparator != ". ")
                throw new ArgumentException("Invalid prompt separator.");
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
            {
                if (string.IsNullOrWhiteSpace(option.primaryPart) || string.IsNullOrWhiteSpace(option.secondaryPart))
                    throw new ArgumentException("Missing product color parts.");
                if (option.ageVariants != null && option.ageVariants.Length > 0)
                {
                    var audiences = new System.Collections.Generic.HashSet<string>();
                    foreach (var variant in option.ageVariants)
                    {
                        if (variant == null || string.IsNullOrWhiteSpace(variant.groupId) ||
                            string.IsNullOrWhiteSpace(variant.label) || string.IsNullOrWhiteSpace(variant.prompt) ||
                            string.IsNullOrWhiteSpace(variant.primaryPart) || string.IsNullOrWhiteSpace(variant.secondaryPart) ||
                            !audiences.Add(variant.groupId))
                            throw new ArgumentException("Invalid or duplicate product age design: " + option.id);
                        Find(BlockSlot.Group, variant.groupId);
                    }
                    if (audiences.Count != groups.Length)
                        throw new ArgumentException("Incomplete product age designs: " + option.id);
                }
                if (!string.IsNullOrEmpty(option.poseGuide) &&
                    (!option.poseGuide.StartsWith("guide_", StringComparison.Ordinal) ||
                     !option.poseGuide.EndsWith(".png", StringComparison.Ordinal) || option.poseGuide.Contains("..") ||
                     option.poseGuide.Contains("/") || option.poseGuide.Contains("\\") || option.poseGuide.Contains(":") ||
                     !(option.poseStrength > 0 && option.poseStrength <= .8f) || !(option.poseEndPercent > 0 && option.poseEndPercent <= .8f) ||
                     !(option.poseLowThreshold >= 0 && option.poseLowThreshold < option.poseHighThreshold && option.poseHighThreshold <= 1)))
                    throw new ArgumentException("Invalid limited product pose reference.");
            }
            foreach (var option in colors)
                if (option.hex == null || option.hex.Length != 7 || option.hex[0] != '#' ||
                    !UnityEngine.ColorUtility.TryParseHtmlString(option.hex, out _))
                    throw new ArgumentException("Use #RRGGBB sampled from the color block circle: " + option.id);
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
            var ageDesign = product.ForGroup(group.id);
            string subject = variant != null ? variant.prompt : ageDesign != null ? ageDesign.prompt : product.prompt;
            string primaryPart = ageDesign != null ? ageDesign.primaryPart : product.primaryPart;
            string secondaryPart = ageDesign != null ? ageDesign.secondaryPart : product.secondaryPart;
            // The reference model's language encoder understands numeric colors; legacy CLIP keeps color words.
            string primary = catalog.numericColorPrompt ? "Main color " + first.hex.ToUpperInvariant() + " (" + first.prompt + ") on " + primaryPart : first.prompt + " " + primaryPart;
            string secondary = catalog.numericColorPrompt ? "Accent color " + second.hex.ToUpperInvariant() + " (" + second.prompt + ") on " + secondaryPart : second.prompt + " " + secondaryPart;
            return new BlockPrompt {
                groupId = group.id, productId = product.id, color1 = first.id, color2 = second.id,
                primaryHex = first.hex, secondaryHex = second.hex,
                primaryReference = first.ColorReference, secondaryReference = second.ColorReference,
                poseGuide = product.poseGuide, poseStrength = product.poseStrength, poseEndPercent = product.poseEndPercent,
                poseLowThreshold = product.poseLowThreshold, poseHighThreshold = product.poseHighThreshold,
                resolvedProduct = variant != null ? variant.id : product.id,
                audienceFragment = group.prompt, productFragment = subject,
                primaryFragment = primary, secondaryFragment = secondary,
                positive = string.Join(catalog.promptSeparator ?? ", ", subject, primary, secondary, group.prompt, catalog.stylePrefix, catalog.styleSuffix),
                negative = string.IsNullOrWhiteSpace(product.negativePrompt) ? catalog.negativePrompt :
                    catalog.negativePrompt + ", " + product.negativePrompt
            };
        }
    }

    [Serializable]
    public sealed class BlockPrompt
    {
        public string groupId, productId, color1, color2, resolvedProduct;
        public string primaryHex, secondaryHex;
        public string primaryReference, secondaryReference;
        public string poseGuide;
        public float poseStrength, poseEndPercent;
        public float poseLowThreshold, poseHighThreshold;
        public string audienceFragment, productFragment, primaryFragment, secondaryFragment, positive, negative;
    }
}
