using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RetroSpaceInvader.Ranking
{
    /// <summary>
    /// JSON 기반 리더보드 영구 저장 및 순위 관리
    /// (Pygame ranking_manager.py와 1:1 대응)
    /// </summary>
    public class RankingManager
    {
        private static RankingManager _instance;
        public static RankingManager Instance => _instance ??= new RankingManager();

        private readonly string _filePath;
        private readonly List<RankingEntry> _entries = new List<RankingEntry>();
        private const int MaxEntries = 10;

        public RankingManager()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
            Load();
        }

        public void Load()
        {
            _entries.Clear();
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    RankingDataWrapper wrapper = JsonUtility.FromJson<RankingDataWrapper>(json);
                    if (wrapper != null && wrapper.items != null)
                    {
                        foreach (RankingEntry item in wrapper.items)
                        {
                            if (item != null)
                            {
                                item.name = string.IsNullOrWhiteSpace(item.name) ? "UNKNOWN" : item.name.Trim().ToUpper();
                                item.score = Mathf.Max(0, item.score);
                                if (string.IsNullOrEmpty(item.date)) item.date = DateTime.Now.ToString("yyyy-MM-dd");
                                _entries.Add(item);
                            }
                        }
                        SortEntries();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[RankingManager] Failed to load {_filePath}: {e.Message}. Initializing defaults.");
                }
            }

            InitDefaults();
            Save();
        }

        private void InitDefaults()
        {
            _entries.Clear();
            _entries.Add(new RankingEntry("ACE", 1000, "2026-09-01"));
            _entries.Add(new RankingEntry("TOP", 800,  "2026-09-02"));
            _entries.Add(new RankingEntry("NEO", 600,  "2026-09-03"));
            _entries.Add(new RankingEntry("SKY", 400,  "2026-09-04"));
            _entries.Add(new RankingEntry("INV", 200,  "2026-09-05"));
        }

        public void Save()
        {
            try
            {
                SortEntries();
                RankingDataWrapper wrapper = new RankingDataWrapper { items = _entries.ToArray() };
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RankingManager] Failed to save ranking: {e.Message}");
            }
        }

        private void SortEntries()
        {
            _entries.Sort((a, b) => b.score.CompareTo(a.score));
            if (_entries.Count > MaxEntries)
            {
                _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
            }
        }

        public int AddScore(string name, int score)
        {
            string sanitizedName = string.IsNullOrWhiteSpace(name) ? "UNKNOWN" : name.Trim().ToUpper();
            int sanitizedScore = Mathf.Max(0, score);
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            RankingEntry newEntry = new RankingEntry(sanitizedName, sanitizedScore, dateStr);
            _entries.Add(newEntry);
            SortEntries();
            Save();

            int rank = _entries.IndexOf(newEntry) + 1;
            return rank > 0 ? rank : 0;
        }

        public int GetHighScore()
        {
            return _entries.Count > 0 ? _entries[0].score : 0;
        }

        public List<RankingEntry> GetEntries()
        {
            return new List<RankingEntry>(_entries);
        }
    }
}
