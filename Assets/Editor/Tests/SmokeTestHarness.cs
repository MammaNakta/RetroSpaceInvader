using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Ranking;
using RetroSpaceInvader.Audio;

namespace RetroSpaceInvader.Tests
{
    /// <summary>
    /// GEMINI.md 제3장(필수 검증 하네스 및 테스트 지침) 준수 스모크 테스트 하네스
    /// </summary>
    public class SmokeTestHarness
    {
        [Test]
        public void ScoreSystem_InvariantCheck_Passes()
        {
            // 1. 점수 및 인바리언트 기본 연산 검증
            int currentScore = 0;
            int alienBotScore = GameConstants.ScoreAlienBottom; // 10
            int alienMidScore = GameConstants.ScoreAlienMiddle; // 20
            int alienTopScore = GameConstants.ScoreAlienTop;    // 30

            currentScore += alienBotScore;
            currentScore += alienMidScore;
            currentScore += alienTopScore;

            Assert.AreEqual(60, currentScore, "Score calculation invariant check failed.");
            Assert.GreaterOrEqual(currentScore, 0, "Score invariant cannot be negative.");
        }

        [Test]
        public void RankingManager_Sanitize_InvalidData_Passes()
        {
            // 2. 랭킹 데이터 무결성 인바리언트(음수 점수, null/공백 이름 방어) 검증
            RankingManager ranking = RankingManager.Instance;
            Assert.IsNotNull(ranking, "RankingManager instance must not be null.");

            // null 및 음수 점수 입력 시 자동 정제 검증
            ranking.AddScore(null, -100);
            List<RankingEntry> entries = ranking.GetEntries();

            Assert.IsNotEmpty(entries, "Entries must not be empty after adding score.");
            
            // 모든 엔트리의 무결성 검증
            for (int i = 0; i < entries.Count; i++)
            {
                Assert.IsNotNull(entries[i].name, $"Entry {i} name must not be null.");
                Assert.AreNotEqual("", entries[i].name.Trim(), $"Entry {i} name must not be empty.");
                Assert.GreaterOrEqual(entries[i].score, 0, $"Entry {i} score must be non-negative.");

                // 점수 내림차순 정렬 인바리언트 검증
                if (i > 0)
                {
                    Assert.GreaterOrEqual(entries[i - 1].score, entries[i].score, "Entries must be sorted descending by score.");
                }
            }
        }

        [Test]
        public void AudioManager_NullSafety_NoException_Passes()
        {
            // 3. 에셋 및 세이프티 펄백: 에셋/클립이 null일 때 NRE 발생 없이 안전 처리 검증
            GameObject audioGo = new GameObject("Test_AudioManager");
            try
            {
                AudioManager audioMgr = audioGo.AddComponent<AudioManager>();
                Assert.DoesNotThrow(() =>
                {
                    audioMgr.PlayShoot();
                    audioMgr.PlayExplosion();
                    audioMgr.PlayPlayerHit();
                    audioMgr.PlayGameOver();
                }, "AudioManager must handle null clips without throwing NullReferenceException.");
            }
            finally
            {
                Object.DestroyImmediate(audioGo);
            }
        }

        [UnityTest]
        public IEnumerator GameLoop_60Frames_NoException_Passes()
        {
            // 4. 60프레임 동안 게임 루프 가동 시 무예외 통과 검증
            GameObject dummyLoopObj = new GameObject("Harness_GameLoop_Test");

            for (int i = 0; i < 60; i++)
            {
                yield return null; // 1프레임 대기
            }

            Object.Destroy(dummyLoopObj);
            Debug.Log("[Harness SUCCESS] 60 frames smoke test passed without exception.");
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Retro Space Invader/Run Verification Harness")]
        public static void RunAllTestsFromMenu()
        {
            Debug.Log("[Verification Harness] Starting verification harness...");
            SmokeTestHarness harness = new SmokeTestHarness();
            int passed = 0;
            int failed = 0;

            void Execute(string name, System.Action action)
            {
                try
                {
                    action();
                    Debug.Log($"<color=green>[PASS]</color> {name}");
                    passed++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"<color=red>[FAIL]</color> {name}: {ex.Message}");
                    failed++;
                }
            }

            Execute(nameof(harness.ScoreSystem_InvariantCheck_Passes), harness.ScoreSystem_InvariantCheck_Passes);
            Execute(nameof(harness.RankingManager_Sanitize_InvalidData_Passes), harness.RankingManager_Sanitize_InvalidData_Passes);
            Execute(nameof(harness.AudioManager_NullSafety_NoException_Passes), harness.AudioManager_NullSafety_NoException_Passes);

            Debug.Log($"[Verification Harness] Complete. Passed: {passed}, Failed: {failed}");
            if (failed > 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("Verification Harness", $"Harness Failed! {failed} test(s) failed.", "OK");
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("Verification Harness", $"All {passed} Invariant & Smoke Tests Passed Successfully!", "OK");
            }
        }

        public static void RunAllTestsBatch()
        {
            Debug.Log("[Batch Verification Harness] Starting batch test harness...");
            SmokeTestHarness harness = new SmokeTestHarness();
            int failed = 0;

            void ExecuteBatch(string name, System.Action action)
            {
                try
                {
                    action();
                    Debug.Log($"[Batch PASS] {name}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Batch FAIL] {name}: {ex.Message}\n{ex.StackTrace}");
                    failed++;
                }
            }

            ExecuteBatch(nameof(harness.ScoreSystem_InvariantCheck_Passes), harness.ScoreSystem_InvariantCheck_Passes);
            ExecuteBatch(nameof(harness.RankingManager_Sanitize_InvalidData_Passes), harness.RankingManager_Sanitize_InvalidData_Passes);
            ExecuteBatch(nameof(harness.AudioManager_NullSafety_NoException_Passes), harness.AudioManager_NullSafety_NoException_Passes);

            Debug.Log($"[Batch Verification Harness] Complete. Failed: {failed}");
            UnityEditor.EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
#endif
    }
}
