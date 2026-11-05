using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Exhibition.Blocks
{
    [Serializable]
    public sealed class RfidConfiguration
    {
        public int version = 2;
        public bool enabled = true;
        public int listenPort = 10123;
        public RfidWireFormat format = new RfidWireFormat();
        public RfidEndpoint[] readers = {
            new RfidEndpoint { slot = 0, ip = "192.168.0.121" },
            new RfidEndpoint { slot = 1, ip = "192.168.0.122" },
            new RfidEndpoint { slot = 2, ip = "192.168.0.123" },
            new RfidEndpoint { slot = 3, ip = "192.168.0.124" }
        };
        public RfidCardMapping[] cards = Array.Empty<RfidCardMapping>();

        public void Validate()
        {
            if (version != 2) throw new ArgumentException("Unsupported RFID configuration version.");
            if (listenPort < 1 || listenPort > 65535) throw new ArgumentException("Invalid RFID listen port.");
            if (format == null) throw new ArgumentException("RFID wire format is missing.");
            format.Validate();
            if (readers == null || readers.Length != 4) throw new ArgumentException("Exactly four RFID readers are required.");
            var slots = new HashSet<int>(); var addresses = new HashSet<string>();
            foreach (var reader in readers)
            {
                if (reader == null || reader.slot < 0 || reader.slot > 3 || !slots.Add(reader.slot) ||
                    !IPAddress.TryParse(reader.ip, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    !addresses.Add(address.ToString()))
                    throw new ArgumentException("Invalid or duplicate RFID reader slot/address.");
            }
            if (cards == null || cards.Length > 4096) throw new ArgumentException("Invalid RFID card mapping list.");
            var uids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var card in cards)
            {
                if (card == null) throw new ArgumentException("RFID card mapping is null.");
                card.uid = RfidHex.Normalize(card.uid);
                if (!uids.Add(card.uid) || format.IsRemoval(card.uid) ||
                    !Enum.TryParse(card.kind, false, out BlockKind kind) || !Enum.IsDefined(typeof(BlockKind), kind) ||
                    string.IsNullOrWhiteSpace(card.id))
                    throw new ArgumentException("Invalid or duplicate RFID card mapping.");
            }
        }

        public RfidCardMapping Find(string uid)
        {
            foreach (var card in cards) if (card.uid == uid) return card;
            return null;
        }

        /// <summary>A connecting reader's source IP selects its physical slot. Source ports are ephemeral.</summary>
        public RfidEndpoint FindReader(IPAddress address)
        {
            foreach (var reader in readers)
                if (IPAddress.TryParse(reader.ip, out var configured) && configured.Equals(address)) return reader;
            return null;
        }
    }

    /// <summary>Readers cannot report that a card stays in place, so repeated reports are sampled to infer it.</summary>
    [Serializable] public sealed class RfidPresenceSettings
    {
        public int checkIntervalMilliseconds = 1000;

        public void Validate()
        {
            if (checkIntervalMilliseconds < 100 || checkIntervalMilliseconds > 60000)
                throw new ArgumentException("RFID check interval must be 100 to 60000 milliseconds.");
        }
    }

    [Serializable] public sealed class RfidEndpoint
    {
        public int slot;
        public string ip;
    }

    [Serializable] public sealed class RfidCardMapping
    {
        public string uid;
        public string kind;
        public string id;
    }

    [Serializable] public sealed class RfidWireFormat
    {
        // LAN-754 passes through serial bytes. A TCP read is neither a UID nor a reader report.
        public string framing = "UidBurst";
        public int idleGapMilliseconds = 100;
        public int fixedFrameBytes = 16;
        public string delimiterHex = "0D0A";
        public bool asciiHex;
        public int uidOffsetBytes;
        public int uidLengthBytes;
        public string[] removedValues = Array.Empty<string>();

        public void Validate()
        {
            if (framing != "UidBurst" && framing != "IdleGap" && framing != "FixedLength" && framing != "Delimiter")
                throw new ArgumentException("RFID framing must be UidBurst, IdleGap, FixedLength or Delimiter.");
            if (idleGapMilliseconds < 10 || idleGapMilliseconds > 5000 ||
                (framing == "FixedLength" && (fixedFrameBytes < 1 || fixedFrameBytes > 512)) ||
                uidOffsetBytes < 0 || uidOffsetBytes > 511 || uidLengthBytes < 0 || uidLengthBytes > 512)
                throw new ArgumentException("Invalid RFID frame/UID bounds.");
            if (framing == "Delimiter" && RfidHex.Bytes(delimiterHex).Length > 8)
                throw new ArgumentException("RFID delimiter is too long.");
            if (removedValues == null) throw new ArgumentException("RFID removal values are missing.");
            for (int i = 0; i < removedValues.Length; i++) removedValues[i] = RfidHex.Normalize(removedValues[i]);
        }

        public string Extract(byte[] frame) => Extract(frame, out _);

        public string Extract(byte[] frame, out int reports)
        {
            byte[] bytes = asciiHex ? RfidHex.Bytes(Encoding.ASCII.GetString(frame)) : frame;
            reports = 1;
            if (framing == "UidBurst") bytes = SingleRepeatedValue(bytes, out reports);
            int length = uidLengthBytes == 0 ? bytes.Length - uidOffsetBytes : uidLengthBytes;
            if (length < 1 || uidOffsetBytes + length > bytes.Length) throw new FormatException("RFID frame is shorter than the configured UID bounds.");
            return BitConverter.ToString(bytes, uidOffsetBytes, length).Replace("-", "");
        }

        /// <summary>One complete quiet-delimited burst can contain several identical UID reports.</summary>
        public static byte[] SingleRepeatedValue(byte[] burst, out int reports)
        {
            if (burst == null || burst.Length == 0) throw new FormatException("RFID burst is empty.");
            reports = 1;
            for (int size = 1; size <= burst.Length / 2; size++)
            {
                if (burst.Length % size != 0) continue;
                bool repeated = true;
                for (int i = size; i < burst.Length; i++)
                    if (burst[i] != burst[i % size]) { repeated = false; break; }
                if (!repeated) continue;
                reports = burst.Length / size;
                var value = new byte[size]; Array.Copy(burst, value, size); return value;
            }
            return burst;
        }

        public bool IsRemoval(string uid) => Array.IndexOf(removedValues, uid) >= 0;
    }

    /// <summary>Do not expose an unconfirmed fragment as a new card in burst mode.</summary>
    public sealed class RfidUidStability
    {
        private string _candidate;
        private int _reports;
        public bool Observe(string uid, int reports)
        {
            if (uid != _candidate) { _candidate = uid; _reports = 0; }
            _reports = Math.Min(2, _reports + Math.Min(2, reports));
            return _reports >= 2;
        }
        public void Reset() { _candidate = null; _reports = 0; }
    }

    public static class RfidHex
    {
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new FormatException("RFID HEX value is empty.");
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (char.IsWhiteSpace(c) || c == ':' || c == '-') continue;
                char upper = char.ToUpperInvariant(c);
                if (!(upper >= '0' && upper <= '9') && !(upper >= 'A' && upper <= 'F'))
                    throw new FormatException("RFID value must contain hexadecimal bytes only.");
                builder.Append(upper);
            }
            if (builder.Length == 0 || builder.Length % 2 != 0 || builder.Length > 1024)
                throw new FormatException("RFID HEX value must contain 1 to 512 complete bytes.");
            return builder.ToString();
        }

        public static byte[] Bytes(string value)
        {
            string hex = Normalize(value); var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
    }

    /// <summary>TCP reads are not packets. Retains partial frames and splits coalesced frames.</summary>
    public sealed class RfidFrameDecoder
    {
        private readonly RfidWireFormat _format;
        private readonly byte[] _delimiter;
        private readonly List<byte> _buffer = new List<byte>(520);
        private bool _overflow;
        private long _lastByte;
        public RfidFrameDecoder(RfidWireFormat format)
        {
            format.Validate(); _format = format;
            _delimiter = format.framing == "Delimiter" ? RfidHex.Bytes(format.delimiterHex) : Array.Empty<byte>();
        }
        public void Feed(byte[] bytes, int count, long now, Action<byte[]> emit)
        {
            FlushIdle(now, emit);
            for (int i = 0; i < count; i++)
            {
                _buffer.Add(bytes[i]); _lastByte = now;
                if (_format.framing == "FixedLength" && _buffer.Count == _format.fixedFrameBytes) Emit(emit, 0);
                else if (_delimiter.Length > 0 && EndsWithDelimiter()) Emit(emit, _delimiter.Length);
                else if (_buffer.Count > 512 + _delimiter.Length)
                {
                    _overflow = true;
                    // Keep a suffix so a subsequent delimiter can still terminate a bad frame.
                    _buffer.RemoveAt(0);
                }
            }
        }
        public void FlushIdle(long now, Action<byte[]> emit)
        {
            if ((_format.framing == "IdleGap" || _format.framing == "UidBurst") && _buffer.Count > 0 && now - _lastByte >= _format.idleGapMilliseconds)
                Emit(emit, 0);
        }
        private bool EndsWithDelimiter()
        {
            if (_buffer.Count < _delimiter.Length) return false;
            for (int i = 0; i < _delimiter.Length; i++)
                if (_buffer[_buffer.Count - _delimiter.Length + i] != _delimiter[i]) return false;
            return true;
        }
        private void Emit(Action<byte[]> emit, int trim)
        {
            int length = _buffer.Count - trim;
            byte[] frame = !_overflow && length > 0 && length <= 512 ? _buffer.GetRange(0, length).ToArray() : null;
            _buffer.Clear(); _overflow = false;
            if (frame != null) emit(frame);
        }
    }
}
