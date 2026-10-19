using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exhibition.Blocks
{
    // One serialized writer per scene session. Outstanding writes are bounded and observed.
    public sealed class BlockJournal
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly string _directory;
        private readonly int _maxFiles;
        private readonly long _maxBytes;
        private readonly string _session = Guid.NewGuid().ToString("N");
        private string _file;
        private int _pending;
        private long _sequence;

        public BlockJournal(string directory, int maxFiles, int maxMegabytes)
        {
            _directory = directory;
            _maxFiles = Math.Max(2, Math.Min(100, maxFiles));
            _maxBytes = Math.Max(1, Math.Min(16, maxMegabytes)) * 1024L * 1024L;
        }

        public async Task AppendAsync(string kind, JObject data)
        {
            if (Interlocked.Increment(ref _pending) > 128)
            {
                Interlocked.Decrement(ref _pending);
                throw new IOException("Block journal write queue is full.");
            }
            var record = new JObject {
                ["schema_version"] = 1, ["event_id"] = Guid.NewGuid().ToString("N"),
                ["session_id"] = _session, ["sequence"] = Interlocked.Increment(ref _sequence),
                ["timestamp_utc"] = DateTime.UtcNow.ToString("O"), ["event_type"] = kind, ["data"] = data
            };
            string line = record.ToString(Formatting.None) + "\n";
            try
            {
                await _gate.WaitAsync();
                try
                {
                    await Task.Run(() => {
                        Directory.CreateDirectory(_directory);
                        if (_file == null || new FileInfo(_file).Length + Encoding.UTF8.GetByteCount(line) > _maxBytes)
                        {
                            _file = Path.Combine(_directory, "blocks_" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "_" + Guid.NewGuid().ToString("N") + ".jsonl");
                            using (File.Create(_file)) { }
                            var files = new DirectoryInfo(_directory).GetFiles("blocks_*.jsonl").OrderByDescending(f => f.CreationTimeUtc).ToArray();
                            for (int i = _maxFiles; i < files.Length; i++) if (files[i].FullName != _file) files[i].Delete();
                        }
                        File.AppendAllText(_file, line, new UTF8Encoding(false));
                    });
                }
                finally { _gate.Release(); }
            }
            finally { Interlocked.Decrement(ref _pending); }
        }
    }
}
