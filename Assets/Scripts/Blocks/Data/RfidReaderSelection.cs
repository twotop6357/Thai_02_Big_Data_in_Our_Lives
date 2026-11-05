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

    /// <summary>
    /// Four physical readers that accept any block, projected onto four purpose slots.
    /// A block's reader no longer decides where it appears: group, product and the two colors have fixed slots.
    /// </summary>
    public sealed class RfidReaderSelection
    {
        public const int GroupSlot = 0, ProductSlot = 1, FirstColorSlot = 2, SecondColorSlot = 3, SlotCount = 4;
        private readonly RfidBlock?[] _readers = new RfidBlock?[4];
        private readonly int[] _counts = new int[3];
        // Reader occupying each purpose slot, or -1. Extra blocks of a kind wait until a slot frees up.
        private readonly int[] _slotReaders = { -1, -1, -1, -1 };
        private int _inputLocks;
        public bool IsInputLocked => _inputLocks > 0;
        public int Revision { get; private set; }
        public bool RequireGroupFirst { get; set; }
        public RfidBlock? this[int reader] { get { ValidateReader(reader); return _readers[reader]; } }
        public int GroupCount => _counts[0];
        public int ProductCount => _counts[1];
        public int ColorCount => _counts[2];
        public int OccupiedCount => GroupCount + ProductCount + ColorCount;
        public bool HasRequiredComposition => GroupCount == 1 && ProductCount == 1 && ColorCount == 2;
        public string GroupId => GroupCount == 1 ? SlotId(GroupSlot) : null;
        public string ProductId => ProductCount == 1 ? SlotId(ProductSlot) : null;
        // Colors follow the visible color slots, so the generated pair matches what the visitor sees.
        public string Color1Id => SlotId(FirstColorSlot) ?? SlotId(SecondColorSlot);
        public string Color2Id => SlotId(FirstColorSlot) != null ? SlotId(SecondColorSlot) : null;

        /// <summary>The block shown in a purpose slot, regardless of which reader holds it.</summary>
        public RfidBlock? DisplayBlock(int slot)
        {
            if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(nameof(slot), "Purpose slot index must be 0 through 3.");
            int reader = _slotReaders[slot];
            return reader < 0 ? null : _readers[reader];
        }

        /// <summary>True when more blocks of this slot's kind are recognized than the design's slots can hold.</summary>
        public bool IsSlotOverfilled(int slot)
        {
            if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(nameof(slot), "Purpose slot index must be 0 through 3.");
            var kind = SlotKind(slot);
            return _counts[(int)kind] > (kind == BlockKind.Color ? 2 : 1);
        }

        private string SlotId(int slot) => DisplayBlock(slot)?.Id;

        public bool Set(int reader, RfidBlock? block)
        {
            ValidateReader(reader);
            if (IsInputLocked) return false;
            if (block.HasValue && !CanRecognize(reader, block.Value)) block = null;
            if (Nullable.Equals(_readers[reader], block)) return false;
            Assign(reader, block); Revision++; return true;
        }

        public bool CanRecognize(int reader, RfidBlock block)
        {
            ValidateReader(reader);
            if (string.IsNullOrWhiteSpace(block.Id)) throw new ArgumentException("An RFID block must have an ID.");
            if (IsInputLocked) return false;
            // A replacement cannot use the group it is about to remove as its prerequisite.
            int remainingGroups = GroupCount - (_readers[reader]?.Kind == BlockKind.Group ? 1 : 0);
            return !RequireGroupFirst || block.Kind == BlockKind.Group || remainingGroups == 1;
        }

        public bool Move(int from, int to)
        {
            ValidateReader(from); ValidateReader(to);
            if (IsInputLocked) return false;
            if (from == to || !_readers[from].HasValue) return false;
            var block = _readers[from];
            if (!CanRecognize(to, block.Value)) block = null;
            Assign(from, null); Assign(to, block); Revision++; return true;
        }

        public bool Clear(bool force = false)
        {
            if (IsInputLocked && !force) return false;
            if (OccupiedCount == 0) return false;
            for (int i = 0; i < 4; i++) { _readers[i] = null; _slotReaders[i] = -1; }
            Array.Clear(_counts, 0, _counts.Length); Revision++; return true;
        }

        /// <summary>Preserves the current reader snapshot; ignored input is never queued for replay.</summary>
        public IDisposable FreezeInput()
        {
            _inputLocks++;
            return new InputLease(this);
        }

        private sealed class InputLease : IDisposable
        {
            private RfidReaderSelection _owner;
            public InputLease(RfidReaderSelection owner) => _owner = owner;
            public void Dispose()
            {
                var owner = _owner; _owner = null;
                if (owner != null) owner._inputLocks--;
            }
        }

        private void Assign(int reader, RfidBlock? block)
        {
            if (_readers[reader].HasValue) _counts[(int)_readers[reader].Value.Kind]--;
            _readers[reader] = block;
            if (block.HasValue) _counts[(int)block.Value.Kind]++;
            UpdateSlots();
        }

        private void UpdateSlots()
        {
            // A block keeps its slot while present, so removing one color never moves the other.
            for (int slot = 0; slot < SlotCount; slot++)
            {
                int reader = _slotReaders[slot];
                if (reader >= 0 && _readers[reader]?.Kind != SlotKind(slot)) _slotReaders[slot] = -1;
            }
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (_slotReaders[slot] >= 0) continue;
                for (int reader = 0; reader < 4; reader++)
                {
                    if (_readers[reader]?.Kind != SlotKind(slot) || IsAssigned(reader)) continue;
                    _slotReaders[slot] = reader;
                    break;
                }
            }
        }

        private bool IsAssigned(int reader)
        {
            for (int slot = 0; slot < SlotCount; slot++)
                if (_slotReaders[slot] == reader) return true;
            return false;
        }

        private static BlockKind SlotKind(int slot) =>
            slot == GroupSlot ? BlockKind.Group : slot == ProductSlot ? BlockKind.Product : BlockKind.Color;

        private static void ValidateReader(int reader)
        {
            if (reader < 0 || reader >= 4) throw new ArgumentOutOfRangeException(nameof(reader), "RFID reader index must be 0 through 3.");
        }
    }
}
