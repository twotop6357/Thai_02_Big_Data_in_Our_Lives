using System;
using UnityEngine;

namespace Exhibition.Blocks
{
    [CreateAssetMenu(menuName = "Exhibition/Block Artwork")]
    public sealed class BlockArtworkCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public BlockKind kind;
            public string id;
            public Sprite sprite;
            public RfidBlock Block => new RfidBlock(kind, id);
        }
        public Entry[] entries;

        public Entry Find(BlockKind kind, string id)
        {
            if (entries != null)
                foreach (var entry in entries)
                    if (entry != null && entry.kind == kind && entry.id == id && entry.sprite) return entry;
            return null;
        }

        public Sprite Find(RfidBlock? block) => block.HasValue ? Find(block.Value.Kind, block.Value.Id)?.sprite : null;
    }
}
