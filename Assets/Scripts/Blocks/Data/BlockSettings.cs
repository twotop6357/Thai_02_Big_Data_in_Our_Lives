using System;

namespace Exhibition.Blocks
{
    [Serializable]
    public sealed class BlockSettings
    {
        public int maxJournalFiles = 30;
        public int maxJournalMegabytes = 4;

        public void Validate()
        {
            if (maxJournalFiles < 2 || maxJournalFiles > 100 ||
                maxJournalMegabytes < 1 || maxJournalMegabytes > 16)
                throw new ArgumentException("Invalid block journal limits.");
        }
    }
}
