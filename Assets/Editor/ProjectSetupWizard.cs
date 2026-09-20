using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Player;
using RetroSpaceInvader.Enemy;
using RetroSpaceInvader.Effects;
using RetroSpaceInvader.Audio;
using RetroSpaceInvader.UI;

namespace RetroSpaceInvader.Editor
{
    /// <summary>
    /// Unity 에디터 및 CLI 배치모드에서 프로젝트 씬, 프리팹, 스프라이트 설정을
    /// 1클릭(또는 무인 자동)으로 완벽하게 구성하는 마법사 스크립트
    /// </summary>
    public static class ProjectSetupWizard
    {
        [MenuItem("Retro Space Invader/Build & Setup Project")]
        public static void BuildProject()
        {
            Debug.Log("[ProjectSetupWizard] Starting complete project configuration...");

            ConfigureTextureAssets();
            AssetDatabase.Refresh();

            CreatePrefabs(out GameObject missilePrefab, out GameObject laserPrefab, out GameObject alienPrefab, out GameObject explosionPrefab);
            AssetDatabase.Refresh();

            BuildMainGameScene(missilePrefab, laserPrefab, alienPrefab, explosionPrefab);
            AssetDatabase.Refresh();

            Debug.Log("[ProjectSetupWizard] Setup complete! MainGameScene is ready to play.");
        }

        public static void ConfigureTextureAssets()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/GameAssets" });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 1f; // 1 Unit = 1 Pixel
                    importer.filterMode = FilterMode.Point; // 8비트 도트 픽셀 유지
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
            Debug.Log($"[ProjectSetupWizard] Configured {textureGuids.Length} sprites with Point filtering & 1 PPU.");
        }

        private static void CreatePrefabs(out GameObject missilePrefab, out GameObject laserPrefab, out GameObject alienPrefab, out GameObject explosionPrefab)
        {
            string prefabDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // 1. Missile Prefab
            Sprite missileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/player/player_bullet.png");
            GameObject missileGo = new GameObject("PlayerMissile");
            SpriteRenderer missileSr = missileGo.AddComponent<SpriteRenderer>();
            missileSr.sprite = missileSprite;
            missileSr.color = GameConstants.ColorPlayer;
            BoxCollider2D missileCol = missileGo.AddComponent<BoxCollider2D>();
            missileCol.isTrigger = true;
            missileCol.size = new Vector2(GameConstants.MissileWidth, GameConstants.MissileHeight);
            Rigidbody2D missileRb = missileGo.AddComponent<Rigidbody2D>();
            missileRb.bodyType = RigidbodyType2D.Kinematic;
            missileGo.AddComponent<PlayerMissile>();
            missilePrefab = PrefabUtility.SaveAsPrefabAsset(missileGo, $"{prefabDir}/PlayerMissile.prefab");
            Object.DestroyImmediate(missileGo);

            // 2. Enemy Laser Prefab
            Sprite laserSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/enemy/enemy_bullet.png");
            GameObject laserGo = new GameObject("EnemyLaser");
            SpriteRenderer laserSr = laserGo.AddComponent<SpriteRenderer>();
            laserSr.sprite = laserSprite;
            laserSr.color = GameConstants.ColorEnemyLaser;
            BoxCollider2D laserCol = laserGo.AddComponent<BoxCollider2D>();
            laserCol.isTrigger = true;
            laserCol.size = new Vector2(GameConstants.EnemyLaserWidth, GameConstants.EnemyLaserHeight);
            Rigidbody2D laserRb = laserGo.AddComponent<Rigidbody2D>();
            laserRb.bodyType = RigidbodyType2D.Kinematic;
            laserGo.AddComponent<EnemyLaser>();
            laserPrefab = PrefabUtility.SaveAsPrefabAsset(laserGo, $"{prefabDir}/EnemyLaser.prefab");
            Object.DestroyImmediate(laserGo);

            // 3. Explosion Prefab
            Sprite explosionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/effect/explosion_fx.png");
            GameObject expGo = new GameObject("ExplosionEffect");
            SpriteRenderer expSr = expGo.AddComponent<SpriteRenderer>();
            expSr.sprite = explosionSprite;
            expGo.AddComponent<ExplosionEffect>();
            explosionPrefab = PrefabUtility.SaveAsPrefabAsset(expGo, $"{prefabDir}/ExplosionEffect.prefab");
            Object.DestroyImmediate(expGo);

            // 4. Alien Base Prefab
            GameObject alienGo = new GameObject("AlienPrefab");
            SpriteRenderer alienSr = alienGo.AddComponent<SpriteRenderer>();
            BoxCollider2D alienCol = alienGo.AddComponent<BoxCollider2D>();
            alienCol.isTrigger = true;
            alienCol.size = new Vector2(GameConstants.AlienWidth, GameConstants.AlienHeight);
            Rigidbody2D alienRb = alienGo.AddComponent<Rigidbody2D>();
            alienRb.bodyType = RigidbodyType2D.Kinematic;
            alienGo.AddComponent<Alien>();
            alienPrefab = PrefabUtility.SaveAsPrefabAsset(alienGo, $"{prefabDir}/AlienPrefab.prefab");
            Object.DestroyImmediate(alienGo);
        }

        private static void BuildMainGameScene(GameObject missilePrefab, GameObject laserPrefab, GameObject alienPrefab, GameObject explosionPrefab)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Main Camera (2D Orthographic Size = 300)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 300f; // 600px Height
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameConstants.ColorBg;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            // 2. Starfield Background
            GameObject starfieldGo = new GameObject("StarfieldScroller");
            starfieldGo.AddComponent<StarfieldScroller>();

            // 3. Audio Manager
            GameObject audioGo = new GameObject("AudioManager");
            AudioManager audioMgr = audioGo.AddComponent<AudioManager>();
            audioMgr.shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/GameAssets/sounds/laser_beep.wav");
            audioMgr.explosionClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/GameAssets/sounds/explosion.wav");
            audioMgr.playerHitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/GameAssets/sounds/player_hit.wav");
            audioMgr.gameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/GameAssets/sounds/game_over.wav");

            // 4. Player
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/player/player_ship.png");
            GameObject playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, GameConstants.PlayerStartY, 0f);
            SpriteRenderer playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = playerSprite;
            BoxCollider2D playerCol = playerGo.AddComponent<BoxCollider2D>();
            playerCol.isTrigger = true;
            playerCol.size = new Vector2(GameConstants.PlayerWidth, GameConstants.PlayerHeight);
            Rigidbody2D playerRb = playerGo.AddComponent<Rigidbody2D>();
            playerRb.bodyType = RigidbodyType2D.Kinematic;
            PlayerController playerCtrl = playerGo.AddComponent<PlayerController>();
            playerCtrl.missilePrefab = missilePrefab;

            // 5. Alien Fleet Manager
            GameObject fleetGo = new GameObject("AlienFleetManager");
            AlienFleetManager fleetMgr = fleetGo.AddComponent<AlienFleetManager>();
            fleetMgr.alienPrefab = alienPrefab;
            fleetMgr.enemyLaserPrefab = laserPrefab;
            fleetMgr.explosionPrefab = explosionPrefab;
            fleetMgr.alienTopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/enemy/alien_top.png");
            fleetMgr.alienMidSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/enemy/alien_mid.png");
            fleetMgr.alienBotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/enemy/alien_bot.png");

            // 6. UI Canvas
            BuildUICanvas(out UIManager uiMgr);

            // 7. Game Manager
            GameObject gmGo = new GameObject("GameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();
            gm.player = playerCtrl;
            gm.fleetManager = fleetMgr;

            // Save Scene
            string scenePath = "Assets/Scenes/MainGameScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in Build Settings
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            Debug.Log($"[ProjectSetupWizard] Scene saved at {scenePath} and added to Build Settings.");
        }

        private static void BuildUICanvas(out UIManager uiMgr)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // --- HUD Panel ---
            GameObject hudGo = new GameObject("HUD", typeof(RectTransform));
            RectTransform hudRt = hudGo.GetComponent<RectTransform>();
            hudRt.SetParent(canvasGo.transform, false);
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;
            hudRt.anchoredPosition = Vector2.zero;

            // Score Text
            GameObject scoreGo = new GameObject("ScoreText");
            scoreGo.transform.SetParent(hudGo.transform, false);
            Text scoreText = scoreGo.AddComponent<Text>();
            scoreText.font = defaultFont;
            scoreText.fontSize = 20;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.alignment = TextAnchor.MiddleLeft;
            scoreText.color = Color.white;
            scoreText.text = "SCORE: 000000";
            RectTransform scoreRt = scoreText.rectTransform;
            scoreRt.anchorMin = new Vector2(0f, 1f);
            scoreRt.anchorMax = new Vector2(0f, 1f);
            scoreRt.pivot = new Vector2(0f, 1f);
            scoreRt.anchoredPosition = new Vector2(25f, -12f);
            scoreRt.sizeDelta = new Vector2(200f, 30f);

            // High Score Text
            GameObject highGo = new GameObject("HighScoreText");
            highGo.transform.SetParent(hudGo.transform, false);
            Text highText = highGo.AddComponent<Text>();
            highText.font = defaultFont;
            highText.fontSize = 20;
            highText.fontStyle = FontStyle.Bold;
            highText.alignment = TextAnchor.MiddleCenter;
            highText.color = new Color(0.7f, 0.7f, 0.8f);
            highText.text = "HIGH: 000000";
            RectTransform highRt = highText.rectTransform;
            highRt.anchorMin = new Vector2(0.5f, 1f);
            highRt.anchorMax = new Vector2(0.5f, 1f);
            highRt.pivot = new Vector2(0.5f, 1f);
            highRt.anchoredPosition = new Vector2(0f, -12f);
            highRt.sizeDelta = new Vector2(200f, 30f);

            // Lives Label & Hearts
            GameObject livesLabelGo = new GameObject("LivesLabel");
            livesLabelGo.transform.SetParent(hudGo.transform, false);
            Text livesLabel = livesLabelGo.AddComponent<Text>();
            livesLabel.font = defaultFont;
            livesLabel.fontSize = 20;
            livesLabel.fontStyle = FontStyle.Bold;
            livesLabel.alignment = TextAnchor.MiddleRight;
            livesLabel.color = new Color(1f, 0.2f, 0.35f);
            livesLabel.text = "LIVES:";
            RectTransform livesLabelRt = livesLabel.rectTransform;
            livesLabelRt.anchorMin = new Vector2(1f, 1f);
            livesLabelRt.anchorMax = new Vector2(1f, 1f);
            livesLabelRt.pivot = new Vector2(1f, 1f);
            livesLabelRt.anchoredPosition = new Vector2(-110f, -12f);
            livesLabelRt.sizeDelta = new Vector2(100f, 30f);

            Image[] heartImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject heartGo = new GameObject($"Heart_{i}");
                heartGo.transform.SetParent(hudGo.transform, false);
                Image img = heartGo.AddComponent<Image>();
                img.color = new Color(1f, 0.2f, 0.35f);
                RectTransform rt = img.rectTransform;
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-20f - (2 - i) * 26f, -16f);
                rt.sizeDelta = new Vector2(20f, 20f);
                heartImages[i] = img;
            }

            // --- Start Panel ---
            GameObject startPanelGo = new GameObject("StartPanel");
            startPanelGo.transform.SetParent(canvasGo.transform, false);
            Image startBg = startPanelGo.AddComponent<Image>();
            startBg.color = new Color(0.04f, 0.04f, 0.06f, 0.92f);
            RectTransform startRt = startPanelGo.GetComponent<RectTransform>();
            startRt.anchorMin = Vector2.zero;
            startRt.anchorMax = Vector2.one;
            startRt.offsetMin = Vector2.zero;
            startRt.offsetMax = Vector2.zero;

            GameObject startTitleGo = new GameObject("StartTitle");
            startTitleGo.transform.SetParent(startPanelGo.transform, false);
            Text startTitle = startTitleGo.AddComponent<Text>();
            startTitle.font = defaultFont;
            startTitle.fontSize = 42;
            startTitle.fontStyle = FontStyle.Bold;
            startTitle.alignment = TextAnchor.MiddleCenter;
            startTitle.color = GameConstants.ColorPlayer;
            startTitle.text = "SPACE INVADERS";
            RectTransform startTitleRt = startTitle.rectTransform;
            startTitleRt.anchoredPosition = new Vector2(0f, 80f);
            startTitleRt.sizeDelta = new Vector2(600f, 60f);

            GameObject startSubGo = new GameObject("StartSub");
            startSubGo.transform.SetParent(startPanelGo.transform, false);
            Text startSub = startSubGo.AddComponent<Text>();
            startSub.font = defaultFont;
            startSub.fontSize = 24;
            startSub.fontStyle = FontStyle.Bold;
            startSub.alignment = TextAnchor.MiddleCenter;
            startSub.color = Color.white;
            startSub.text = "레트로 스페이스 인베이더";
            RectTransform startSubRt = startSub.rectTransform;
            startSubRt.anchoredPosition = new Vector2(0f, 20f);
            startSubRt.sizeDelta = new Vector2(600f, 40f);

            GameObject startPromptGo = new GameObject("StartPrompt");
            startPromptGo.transform.SetParent(startPanelGo.transform, false);
            Text startPrompt = startPromptGo.AddComponent<Text>();
            startPrompt.font = defaultFont;
            startPrompt.fontSize = 22;
            startPrompt.fontStyle = FontStyle.Bold;
            startPrompt.alignment = TextAnchor.MiddleCenter;
            startPrompt.color = GameConstants.ColorGold;
            startPrompt.text = "[ SPACE ] 키를 눌러 게임 시작";
            RectTransform startPromptRt = startPrompt.rectTransform;
            startPromptRt.anchoredPosition = new Vector2(0f, -80f);
            startPromptRt.sizeDelta = new Vector2(600f, 40f);

            // --- Stage Clear Panel ---
            GameObject clearPanelGo = new GameObject("StageClearPanel");
            clearPanelGo.transform.SetParent(canvasGo.transform, false);
            Image clearBg = clearPanelGo.AddComponent<Image>();
            clearBg.color = new Color(0f, 0f, 0f, 0.85f);
            RectTransform clearRt = clearPanelGo.GetComponent<RectTransform>();
            clearRt.anchorMin = Vector2.zero;
            clearRt.anchorMax = Vector2.one;
            clearRt.offsetMin = Vector2.zero;
            clearRt.offsetMax = Vector2.zero;

            GameObject clearTitleGo = new GameObject("ClearTitle");
            clearTitleGo.transform.SetParent(clearPanelGo.transform, false);
            Text clearTitle = clearTitleGo.AddComponent<Text>();
            clearTitle.font = defaultFont;
            clearTitle.fontSize = 38;
            clearTitle.fontStyle = FontStyle.Bold;
            clearTitle.alignment = TextAnchor.MiddleCenter;
            clearTitle.color = GameConstants.ColorCyan;
            clearTitle.text = "★ STAGE CLEAR ★";
            RectTransform clearTitleRt = clearTitle.rectTransform;
            clearTitleRt.anchoredPosition = new Vector2(0f, 50f);
            clearTitleRt.sizeDelta = new Vector2(600f, 50f);

            GameObject clearScoreGo = new GameObject("ClearScore");
            clearScoreGo.transform.SetParent(clearPanelGo.transform, false);
            Text clearScore = clearScoreGo.AddComponent<Text>();
            clearScore.font = defaultFont;
            clearScore.fontSize = 20;
            clearScore.fontStyle = FontStyle.Bold;
            clearScore.alignment = TextAnchor.MiddleCenter;
            clearScore.color = Color.white;
            clearScore.text = "외계인 편대 격파 완료!";
            RectTransform clearScoreRt = clearScore.rectTransform;
            clearScoreRt.anchoredPosition = new Vector2(0f, 0f);
            clearScoreRt.sizeDelta = new Vector2(600f, 30f);

            GameObject clearGuideGo = new GameObject("ClearGuide");
            clearGuideGo.transform.SetParent(clearPanelGo.transform, false);
            Text clearGuide = clearGuideGo.AddComponent<Text>();
            clearGuide.font = defaultFont;
            clearGuide.fontSize = 20;
            clearGuide.fontStyle = FontStyle.Bold;
            clearGuide.alignment = TextAnchor.MiddleCenter;
            clearGuide.color = GameConstants.ColorPlayer;
            clearGuide.text = "[ R ] 또는 [ SPACE ] 키를 눌러 다음 스테이지로 이동";
            RectTransform clearGuideRt = clearGuide.rectTransform;
            clearGuideRt.anchoredPosition = new Vector2(0f, -60f);
            clearGuideRt.sizeDelta = new Vector2(600f, 30f);
            clearPanelGo.SetActive(false);

            // --- Game Over & Hall of Fame Panel ---
            GameObject overPanelGo = new GameObject("GameOverPanel");
            overPanelGo.transform.SetParent(canvasGo.transform, false);
            Image overBg = overPanelGo.AddComponent<Image>();
            overBg.color = new Color(0.03f, 0.03f, 0.06f, 0.94f);
            RectTransform overRt = overPanelGo.GetComponent<RectTransform>();
            overRt.anchorMin = Vector2.zero;
            overRt.anchorMax = Vector2.one;
            overRt.offsetMin = Vector2.zero;
            overRt.offsetMax = Vector2.zero;

            // GameOver Title
            GameObject overTitleGo = new GameObject("OverTitle");
            overTitleGo.transform.SetParent(overPanelGo.transform, false);
            Text overTitle = overTitleGo.AddComponent<Text>();
            overTitle.font = defaultFont;
            overTitle.fontSize = 38;
            overTitle.fontStyle = FontStyle.Bold;
            overTitle.alignment = TextAnchor.MiddleCenter;
            overTitle.color = new Color(1f, 0.25f, 0.25f);
            overTitle.text = "GAME OVER";
            RectTransform overTitleRt = overTitle.rectTransform;
            overTitleRt.anchoredPosition = new Vector2(0f, 240f);
            overTitleRt.sizeDelta = new Vector2(400f, 45f);

            // Final Score Text
            GameObject overScoreGo = new GameObject("FinalScore");
            overScoreGo.transform.SetParent(overPanelGo.transform, false);
            Text overScore = overScoreGo.AddComponent<Text>();
            overScore.font = defaultFont;
            overScore.fontSize = 22;
            overScore.fontStyle = FontStyle.Bold;
            overScore.alignment = TextAnchor.MiddleCenter;
            overScore.color = Color.white;
            overScore.text = "최종 획득 점수 : 000000 PTS";
            RectTransform overScoreRt = overScore.rectTransform;
            overScoreRt.anchoredPosition = new Vector2(0f, 200f);
            overScoreRt.sizeDelta = new Vector2(500f, 35f);

            // 3 Letter Initial Slot Boxes
            Text[] initialTexts = new Text[3];
            Image[] initialBorders = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject boxGo = new GameObject($"Slot_{i}");
                boxGo.transform.SetParent(overPanelGo.transform, false);
                Image boxBg = boxGo.AddComponent<Image>();
                boxBg.color = new Color(0.08f, 0.08f, 0.14f);
                RectTransform boxRt = boxBg.rectTransform;
                boxRt.anchoredPosition = new Vector2(-55f + i * 55f, 145f);
                boxRt.sizeDelta = new Vector2(45f, 50f);
                initialBorders[i] = boxBg;

                GameObject charGo = new GameObject("Char");
                charGo.transform.SetParent(boxGo.transform, false);
                Text charText = charGo.AddComponent<Text>();
                charText.font = defaultFont;
                charText.fontSize = 32;
                charText.fontStyle = FontStyle.Bold;
                charText.alignment = TextAnchor.MiddleCenter;
                charText.color = Color.white;
                charText.text = "";
                RectTransform charRt = charText.rectTransform;
                charRt.anchorMin = Vector2.zero;
                charRt.anchorMax = Vector2.one;
                charRt.offsetMin = Vector2.zero;
                charRt.offsetMax = Vector2.zero;
                initialTexts[i] = charText;
            }

            // Initial Guide Text
            GameObject guideGo = new GameObject("InitialGuide");
            guideGo.transform.SetParent(overPanelGo.transform, false);
            Text guideText = guideGo.AddComponent<Text>();
            guideText.font = defaultFont;
            guideText.fontSize = 16;
            guideText.fontStyle = FontStyle.Bold;
            guideText.alignment = TextAnchor.MiddleCenter;
            guideText.color = new Color(0.7f, 0.7f, 0.8f);
            guideText.text = "알파벳 입력 후 [ENTER] 키를 눌러 랭킹 등록  (지우기: Backspace)";
            RectTransform guideRt = guideText.rectTransform;
            guideRt.anchoredPosition = new Vector2(0f, 100f);
            guideRt.sizeDelta = new Vector2(600f, 25f);

            // Registered Info Text
            GameObject regInfoGo = new GameObject("RegisteredInfo");
            regInfoGo.transform.SetParent(overPanelGo.transform, false);
            Text regInfo = regInfoGo.AddComponent<Text>();
            regInfo.font = defaultFont;
            regInfo.fontSize = 20;
            regInfo.fontStyle = FontStyle.Bold;
            regInfo.alignment = TextAnchor.MiddleCenter;
            regInfo.color = GameConstants.ColorGold;
            regInfo.text = "★ 랭킹 등록 완료! ★";
            RectTransform regInfoRt = regInfo.rectTransform;
            regInfoRt.anchoredPosition = new Vector2(0f, 100f);
            regInfoRt.sizeDelta = new Vector2(600f, 30f);
            regInfoGo.SetActive(false);

            // Hall of Fame Title
            GameObject hofTitleGo = new GameObject("HofTitle");
            hofTitleGo.transform.SetParent(overPanelGo.transform, false);
            Text hofTitle = hofTitleGo.AddComponent<Text>();
            hofTitle.font = defaultFont;
            hofTitle.fontSize = 20;
            hofTitle.fontStyle = FontStyle.Bold;
            hofTitle.alignment = TextAnchor.MiddleCenter;
            hofTitle.color = GameConstants.ColorGold;
            hofTitle.text = "★ TOP 5 HALL OF FAME ★";
            RectTransform hofTitleRt = hofTitle.rectTransform;
            hofTitleRt.anchoredPosition = new Vector2(0f, 55f);
            hofTitleRt.sizeDelta = new Vector2(500f, 30f);

            // Table Rows (5 lines)
            Text[] rows = new Text[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject rowGo = new GameObject($"Row_{i}");
                rowGo.transform.SetParent(overPanelGo.transform, false);
                Text rowText = rowGo.AddComponent<Text>();
                rowText.font = defaultFont;
                rowText.fontSize = 18;
                rowText.fontStyle = FontStyle.Bold;
                rowText.alignment = TextAnchor.MiddleCenter;
                rowText.color = Color.white;
                rowText.text = $"1ST    AAA    001000 PTS    2026-09-01";
                RectTransform rowRt = rowText.rectTransform;
                rowRt.anchoredPosition = new Vector2(0f, 15f - i * 35f);
                rowRt.sizeDelta = new Vector2(600f, 30f);
                rows[i] = rowText;
            }

            // Restart Prompt
            GameObject restartGo = new GameObject("RestartPrompt");
            restartGo.transform.SetParent(overPanelGo.transform, false);
            Text restartText = restartGo.AddComponent<Text>();
            restartText.font = defaultFont;
            restartText.fontSize = 20;
            restartText.fontStyle = FontStyle.Bold;
            restartText.alignment = TextAnchor.MiddleCenter;
            restartText.color = GameConstants.ColorPlayer;
            restartText.text = "[ R ] 또는 [ SPACE ] 키를 눌러 다시 도전";
            RectTransform restartRt = restartText.rectTransform;
            restartRt.anchoredPosition = new Vector2(0f, -205f);
            restartRt.sizeDelta = new Vector2(600f, 30f);

            // Notice
            GameObject noticeGo = new GameObject("Notice");
            noticeGo.transform.SetParent(overPanelGo.transform, false);
            Text noticeText = noticeGo.AddComponent<Text>();
            noticeText.font = defaultFont;
            noticeText.fontSize = 14;
            noticeText.alignment = TextAnchor.MiddleCenter;
            noticeText.color = new Color(0.5f, 0.5f, 0.6f);
            noticeText.text = "※ 랭킹 기록은 게임을 종료 후 재실행해도 영구 보존됩니다.";
            RectTransform noticeRt = noticeText.rectTransform;
            noticeRt.anchoredPosition = new Vector2(0f, -250f);
            noticeRt.sizeDelta = new Vector2(600f, 25f);
            overPanelGo.SetActive(false);

            // --- UIManager Hookup ---
            uiMgr = canvasGo.AddComponent<UIManager>();
            uiMgr.scoreText = scoreText;
            uiMgr.highScoreText = highText;
            uiMgr.heartImages = heartImages;
            uiMgr.startPanel = startPanelGo;
            uiMgr.stageClearPanel = clearPanelGo;
            uiMgr.gameOverPanel = overPanelGo;
            uiMgr.stageClearScoreText = clearScore;
            uiMgr.gameOverFinalScoreText = overScore;
            uiMgr.initialSlotTexts = initialTexts;
            uiMgr.initialSlotBorders = initialBorders;
            uiMgr.initialGuideText = guideText;
            uiMgr.registeredInfoText = regInfo;
            uiMgr.rankRowsText = rows;
        }
    }
}
