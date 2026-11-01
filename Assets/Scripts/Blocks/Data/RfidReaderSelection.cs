using System;

namespace Exhibition.Blocks
{
    public enum BlockKind { Group, Product, Color }

    public readonly struct RfidBlock : IEquatable<RfidBlock>
    {
        public BlockKind Kind { get; }
        public string Id { get; }
        public RfidBlock(BlockKind kind, string id)
        {
            if (kind < BlockKind.Group || kind > BlockKind.Color || string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Invalid RFID block kind or ID.");
            Kind = kind; Id = id;
        }
        public bool Equals(RfidBlock other) => Kind == other.Kind && Id == other.Id;
        public override bool Equals(object obj) => obj is RfidBlock other && Equals(other);
        public override int GetHashCode() => ((int)Kind * 397) ^ (Id == null ? 0 : Id.GetHashCode());
        public static bool operator ==(RfidBlock a, RfidBlock b) => a.Equals(b);
        public static bool operator !=(RfidBlock a, RfidBlock b) => !a.Equals(b);
    }

    /// <summary>Four physical readers in top-left, top-right, bottom-left, bottom-right order.</summary>
    public sealed class RfidReaderSelection
    {
        private readonly RfidBlock?[] _readers = new RfidBlock?[4];
        private readonly int[] _counts = new int[3];
        public int Revision { get; private set; }
        public RfidBlock? this[int reader] { get { ValidateReader(reader); return _readers[reader]; } }
        public int GroupCount => _counts[0];
        public int ProductCount => _counts[1];
        public int ColorCount => _counts[2];
        public int OccupiedCount => GroupCount + ProductCount + ColorCount;
        public bool HasRequiredComposition => GroupCount == 1 && ProductCount == 1 && ColorCount == 2;
        public string GroupId => GroupCount == 1 ? Find(BlockKind.Group, 0) : null;
        public string ProductId => ProductCount == 1 ? Find(BlockKind.Product, 0) : null;
        // Colors are ordered by physical reader number, regardless of where group/product were placed.
        public string Color1Id => Find(BlockKind.Color, 0);
        public string Color2Id => Find(BlockKind.Color, 1);

        public bool Set(int reader, RfidBlock? block)
        {
            ValidateReader(reader);
            if (block.HasValue && string.IsNullOrWhiteSpace(block.Value.Id)) throw new ArgumentException("An RFID block must have an ID.");
            if (Nullable.Equals(_readers[reader], block)) return false;
            Assign(reader, block); Revision++; return true;
        }

        public bool Move(int from, int to)
        {
            ValidateReader(from); ValidateReader(to);
            if (from == to || !_readers[from].HasValue) return false;
            var block = _readers[from];
            Assign(from, null); Assign(to, block); Revision++; return true;
        }

        public bool Clear()
        {
            if (OccupiedCount == 0) return false;
            for (int i = 0; i < 4; i++) _readers[i] = null;
            Array.Clear(_counts, 0, _counts.Length); Revision++; return true;
        }

        private void Assign(int reader, RfidBlock? block)
        {
            if (_readers[reader].HasValue) _counts[(int)_readers[reader].Value.Kind]--;
            _readers[reader] = block;
            if (block.HasValue) _counts[(int)block.Value.Kind]++;
        }

        private string Find(BlockKind kind, int index)
        {
            foreach (var block in _readers)
                if (block.HasValue && block.Value.Kind == kind && index-- == 0) return block.Value.Id;
            return null;
        }

        private static void ValidateReader(int reader)
        {
            if (reader < 0 || reader >= 4) throw new ArgumentOutOfRangeException(nameof(reader), "RFID reader index must be 0 through 3.");
        }
    }
}
