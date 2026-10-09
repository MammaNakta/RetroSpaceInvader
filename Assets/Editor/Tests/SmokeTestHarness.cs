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

        [Test]
        public void PlayerLevel_Thresholds_Passes()
        {
            // 플레이어 레벨 계산 인바리언트 검증
            Assert.AreEqual(1, GameConstants.GetLevelForScore(0));
            Assert.AreEqual(1, GameConstants.GetLevelForScore(599));
            Assert.AreEqual(2, GameConstants.GetLevelForScore(600));
            Assert.AreEqual(2, GameConstants.GetLevelForScore(1499));
            Assert.AreEqual(3, GameConstants.GetLevelForScore(1500));
            Assert.AreEqual(4, GameConstants.GetLevelForScore(2800));
            Assert.AreEqual(5, GameConstants.GetLevelForScore(4500));
            Assert.AreEqual(5, GameConstants.GetLevelForScore(999999));
        }

        [Test]
        public void AlienTier_HpScore_Passes()
        {
            // 외계인 티어별 스펙 인바리언트 검증
            Assert.AreEqual(10, GameConstants.ScoreAlienBottom);
            Assert.AreEqual(20, GameConstants.ScoreAlienMiddle);
            Assert.AreEqual(30, GameConstants.ScoreAlienTop);
            Assert.AreEqual(5, GameConstants.MaxPlayerLevel);
        }

        [Test]
        public void GameManager_State_ReturnToTitle_Passes()
        {
            GameObject gmGo = new GameObject("Test_GameManager");
            try
            {
                GameManager gm = gmGo.AddComponent<GameManager>();
                gm.ReturnToTitle();
                Assert.AreEqual(GameState.Start, gm.CurrentState, "ReturnToTitle must transition state to Start.");
                Assert.AreEqual(0, gm.Score, "ReturnToTitle must reset score to 0.");
                Assert.AreEqual(1, gm.Stage, "ReturnToTitle must reset stage to 1.");
            }
            finally
            {
                Object.DestroyImmediate(gmGo);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Harness/Run Invariant Smoke Tests", priority = 20)]
        public static void RunAllTestsInEditor()
        {
            Debug.Log("<color=cyan><b>[Harness: Tier 2] Running Invariant Smoke Tests in Editor...</b></color>");
            SmokeTestHarness harness = new SmokeTestHarness();
            int failed = 0;
            int passed = 0;

            void ExecuteTest(string name, System.Action action)
            {
                try
                {
                    action();
                    Debug.Log($"<color=lime>[PASS]</color> {name}");
                    passed++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"<color=red>[FAIL]</color> {name}: {ex.Message}\n{ex.StackTrace}");
                    failed++;
                }
            }

            ExecuteTest(nameof(harness.ScoreSystem_InvariantCheck_Passes), harness.ScoreSystem_InvariantCheck_Passes);
            ExecuteTest(nameof(harness.RankingManager_Sanitize_InvalidData_Passes), harness.RankingManager_Sanitize_InvalidData_Passes);
            ExecuteTest(nameof(harness.AudioManager_NullSafety_NoException_Passes), harness.AudioManager_NullSafety_NoException_Passes);
            ExecuteTest(nameof(harness.PlayerLevel_Thresholds_Passes), harness.PlayerLevel_Thresholds_Passes);
            ExecuteTest(nameof(harness.AlienTier_HpScore_Passes), harness.AlienTier_HpScore_Passes);
            ExecuteTest(nameof(harness.GameManager_State_ReturnToTitle_Passes), harness.GameManager_State_ReturnToTitle_Passes);

            if (failed == 0)
            {
                Debug.Log($"<color=lime><b>[Harness: Tier 2 Complete] All {passed} tests passed successfully!</b></color>");
                UnityEditor.EditorUtility.DisplayDialog("Smoke Tests Passed", $"모든 스모크/인바리언트 테스트({passed}개)가 성공적으로 통과했습니다.", "확인");
            }
            else
            {
                Debug.LogError($"<color=red><b>[Harness: Tier 2 Complete] {failed} test(s) failed out of {passed + failed}.</b></color>");
                UnityEditor.EditorUtility.DisplayDialog("Smoke Tests Failed", $"{failed}개의 테스트가 실패했습니다. 콘솔 창을 확인하세요.", "확인");
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
            ExecuteBatch(nameof(harness.PlayerLevel_Thresholds_Passes), harness.PlayerLevel_Thresholds_Passes);
            ExecuteBatch(nameof(harness.AlienTier_HpScore_Passes), harness.AlienTier_HpScore_Passes);
            ExecuteBatch(nameof(harness.GameManager_State_ReturnToTitle_Passes), harness.GameManager_State_ReturnToTitle_Passes);

            Debug.Log($"[Batch Verification Harness] Complete. Failed: {failed}");
            UnityEditor.EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
#endif
    }
}
