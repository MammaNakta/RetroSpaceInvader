#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using RetroSpaceInvader.Player;
using RetroSpaceInvader.Enemy;

namespace RetroSpaceInvader.Core
{
    /// <summary>
    /// Tier 3: 런타임 인게임 디버그/치트 하네스
    /// 플레이 모드 및 개발 빌드에서 단축키(F1~F5, F12)를 통해 게임 상태를 즉시 재현 및 검증합니다.
    /// 별도로 씬에 GameObject를 배치하지 않아도 런타임에 자동 초기화됩니다.
    /// </summary>
    public class RuntimeDebugHarness : MonoBehaviour
    {
        private static RuntimeDebugHarness _instance;
        private bool _showDebugGui = true;
        private readonly float[] _timeScales = { 1.0f, 2.0f, 5.0f, 0.5f };
        private int _timeScaleIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance != null) return;

            GameObject harnessGo = new GameObject("[RuntimeDebugHarness]");
            _instance = harnessGo.AddComponent<RuntimeDebugHarness>();
            DontDestroyOnLoad(harnessGo);
        }

        private void Update()
        {
            // F12: 디버그 UI 토글
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _showDebugGui = !_showDebugGui;
            }

            // F1: 무적 모드 토글
            if (Input.GetKeyDown(KeyCode.F1))
            {
                PlayerController.GodMode = !PlayerController.GodMode;
                Debug.Log($"<color=yellow>[Debug Harness] God Mode: {(PlayerController.GodMode ? "ENABLED" : "DISABLED")}</color>");
            }

            // F2: 점수 +1000점
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(1000);
                    Debug.Log("<color=yellow>[Debug Harness] Score +1000 Added</color>");
                }
            }

            // F3: 적 전멸 (스테이지 클리어 테스트)
            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (AlienFleetManager.Instance != null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    AlienFleetManager.Instance.KillAllAliens();
                    Debug.Log("<color=yellow>[Debug Harness] All Aliens Destroyed -> Stage Clear</color>");
                }
            }

            // F4: 플레이어 피격 (데미지 / 게임오버 테스트)
            if (Input.GetKeyDown(KeyCode.F4))
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    bool prevGod = PlayerController.GodMode;
                    PlayerController.GodMode = false;
                    player.TakeHit();
                    PlayerController.GodMode = prevGod;
                    Debug.Log($"<color=yellow>[Debug Harness] Player Hit Triggered (Lives: {player.Lives})</color>");
                }
            }

            // F5: 배속(TimeScale) 토글
            if (Input.GetKeyDown(KeyCode.F5))
            {
                _timeScaleIndex = (_timeScaleIndex + 1) % _timeScales.Length;
                Time.timeScale = _timeScales[_timeScaleIndex];
                Debug.Log($"<color=yellow>[Debug Harness] TimeScale: {Time.timeScale}x</color>");
            }

            // F6: 플레이어 레벨업 (+1 Level)
            if (Input.GetKeyDown(KeyCode.F6))
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.SetLevel(player.Level + 1);
                    GameManager.Instance?.RefreshHUD();
                    Debug.Log($"<color=yellow>[Debug Harness] Player Level UP -> Lv.{player.Level}</color>");
                }
            }

            // F7: 플레이어 레벨다운 (-1 Level)
            if (Input.GetKeyDown(KeyCode.F7))
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.SetLevel(player.Level - 1);
                    GameManager.Instance?.RefreshHUD();
                    Debug.Log($"<color=yellow>[Debug Harness] Player Level DOWN -> Lv.{player.Level}</color>");
                }
            }

            // F8: 다음 스테이지 편대 테스트 (Advance Stage)
            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.AdvanceToNextStage();
                    Debug.Log($"<color=yellow>[Debug Harness] Advanced to Stage {GameManager.Instance.Stage}</color>");
                }
            }

            // F9: 기동 모드 순환 토글 (Movement Trait Cycle)
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (AlienFleetManager.Instance != null)
                {
                    AlienFleetManager.Instance.CycleMovementMode();
                    Debug.Log($"<color=yellow>[Debug Harness] Movement Mode: {AlienFleetManager.Instance.CurrentMovementMode}</color>");
                }
            }
        }

        private void OnGUI()
        {
            if (!_showDebugGui) return;

            var player = FindFirstObjectByType<PlayerController>();
            int lv = player != null ? player.Level : 1;
            int st = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
            string modeName = AlienFleetManager.Instance != null ? AlienFleetManager.Instance.CurrentMovementMode.ToString() : "Standard";

            GUILayout.BeginArea(new Rect(10, 10, 240, 290), GUI.skin.box);
            GUILayout.Label("<b>[Harness: Tier 3 Debug]</b>");

            string godText = PlayerController.GodMode ? "<color=green>ON</color>" : "OFF";
            GUILayout.Label($"[F1] God Mode: {godText}");
            GUILayout.Label("[F2] Add Score (+1000)");
            GUILayout.Label("[F3] Clear Fleet (Win)");
            GUILayout.Label("[F4] Hit Player (Damage)");
            GUILayout.Label($"[F5] Speed: {Time.timeScale:0.0}x");
            GUILayout.Label($"[F6] Level UP (Current: <b>Lv.{lv}</b>)");
            GUILayout.Label("[F7] Level DOWN");
            GUILayout.Label($"[F8] Next Stage (Current: <b>St.{st}</b>)");
            GUILayout.Label($"[F9] Mode: <b>{modeName}</b>");
            GUILayout.Label("[F12] Hide/Show Debug HUD");

            if (GUILayout.Button("F6: Level +1"))
            {
                if (player != null)
                {
                    player.SetLevel(player.Level + 1);
                    GameManager.Instance?.RefreshHUD();
                }
            }

            if (GUILayout.Button("F9: Cycle Movement Trait"))
            {
                AlienFleetManager.Instance?.CycleMovementMode();
            }

            GUILayout.EndArea();
        }
    }
}
#endif
