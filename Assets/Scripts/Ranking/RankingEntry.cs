using System;

namespace RetroSpaceInvader.Ranking
{
    [Serializable]
    public class RankingEntry
    {
        public string name = "AAA";
        public int score = 0;
        public string date = "2026-09-01";

        public RankingEntry() { }

        public RankingEntry(string name, int score, string date)
        {
            this.name = name;
            this.score = score;
            this.date = date;
        }
    }

    [Serializable]
    public class RankingDataWrapper
    {
        public RankingEntry[] items;
    }
}
