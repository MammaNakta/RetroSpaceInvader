using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.UI;
using RetroSpaceInvader.Player;
using RetroSpaceInvader.Enemy;

namespace RetroSpaceInvader.Editor.Harness
{
    /// <summary>
    /// Tier 1: 에디터 퀵 진단 하네스 (Editor Sanity Check)
    /// 씬 내 누락된 스크립트(Missing Component), 필수 싱글톤 및 레퍼런스 연결, 프리팹 무결성을 1초 만에 검증합니다.
    /// </summary>
    public static class EditorSanityCheckHarness
    {
        [MenuItem("Tools/Harness/Run Quick Sanity Check %#s", priority = 10)]
        public static void RunSanityCheck()
        {
            Debug.Log("<color=cyan><b>[Harness: Tier 1] Starting Editor Quick Sanity Check...</b></color>");

            int issuesFound = 0;

            // 1. 활성 씬의 Missing Script(Missing Component) 검사
            issuesFound += CheckMissingScriptsInScene();

            // 2. 필수 매니저 및 씬 오브젝트 레퍼런스 검사
            issuesFound += CheckCoreSceneReferences();

            // 3. 주요 프리팹 에셋 존재 여부 검사
            issuesFound += CheckRequiredPrefabs();

            if (issuesFound == 0)
            {
                Debug.Log("<color=lime><b>[Harness: Tier 1 PASS] All scene components, references, and prefabs are valid!</b></color>");
                EditorUtility.DisplayDialog("Sanity Check Passed", "모든 씬 컴포넌트, 참조 및 프리팹이 정상입니다.", "확인");
            }
            else
            {
                Debug.LogWarning($"<color=orange><b>[Harness: Tier 1 WARN] Sanity check completed with {issuesFound} potential issue(s). Check Console for details.</b></color>");
                EditorUtility.DisplayDialog("Sanity Check Warning", $"{issuesFound}개의 잠재적 문제가 발견되었습니다. 콘솔 창을 확인하세요.", "확인");
            }
        }

        private static int CheckMissingScriptsInScene()
        {
            int missingCount = 0;
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var go in allObjects)
            {
                Component[] components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        Debug.LogError($"[Missing Script] '{go.name}' 게임 오브젝트에 누락된 스크립트(Missing Component)가 있습니다!", go);
                        missingCount++;
                    }
                }
            }

            if (missingCount == 0)
            {
                Debug.Log("<color=lightblue>[Check] Missing Components in scene: None (OK)</color>");
            }

            return missingCount;
        }

        private static int CheckCoreSceneReferences()
        {
            int issueCount = 0;

            var gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("[Check] Scene에 GameManager가 배치되지 않았습니다!");
                issueCount++;
            }
            else
            {
                if (gameManager.player == null)
                {
                    Debug.LogWarning("[Check] GameManager의 Player 레퍼런스가 비어 있습니다.", gameManager);
                }
                if (gameManager.fleetManager == null)
                {
                    Debug.LogWarning("[Check] GameManager의 FleetManager 레퍼런스가 비어 있습니다.", gameManager);
                }
            }

            var uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[Check] Scene에 UIManager가 배치되지 않았습니다!");
                issueCount++;
            }

            return issueCount;
        }

        private static int CheckRequiredPrefabs()
        {
            int missingPrefabs = 0;
            string[] requiredPaths = new string[]
            {
                "Assets/Prefabs/PlayerMissile.prefab",
                "Assets/Prefabs/EnemyLaser.prefab",
                "Assets/Prefabs/ExplosionEffect.prefab",
                "Assets/Prefabs/AlienUnit.prefab"
            };

            foreach (string path in requiredPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning($"[Check] 필수 프리팹 누락: {path}");
                    missingPrefabs++;
                }
            }

            if (missingPrefabs == 0)
            {
                Debug.Log("<color=lightblue>[Check] Core Prefabs: All exist (OK)</color>");
            }

            return missingPrefabs;
        }
    }
}
