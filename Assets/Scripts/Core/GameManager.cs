using UnityEngine;
using RetroSpaceInvader.Player;
using RetroSpaceInvader.Enemy;
using RetroSpaceInvader.Audio;
using RetroSpaceInvader.Ranking;
using RetroSpaceInvader.UI;

namespace RetroSpaceInvader.Core
{
    /// <summary>
    /// 게임 상태 전환, 점수, 스테이지 관리 및 메인 게임 루프 총괄
    /// (Pygame game.py Game 클래스와 1:1 대응)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Start;
        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public int Stage { get; private set; } = 1;

        [Header("Scene References")]
        public PlayerController player;
        public AlienFleetManager fleetManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 60FPS 고정
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            HighScore = RankingManager.Instance.GetHighScore();
            SetState(GameState.Start);
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case GameState.Start:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        StartNewGame();
                    }
                    break;

                case GameState.StageClear:
                    if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                    {
                        AdvanceToNextStage();
                    }
                    break;

                case GameState.GameOver:
                    // 랭킹 등록 및 메인 화면 복귀는 UIManager에서 제어
                    break;
            }
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;

            UIManager.Instance?.ShowStartScreen(newState == GameState.Start);
            UIManager.Instance?.ShowStageClearScreen(newState == GameState.StageClear, Score);
            UIManager.Instance?.ShowGameOverScreen(newState == GameState.GameOver, Score);
            RefreshHUD();
        }

        public void RefreshHUD()
        {
            int lives = player != null ? player.Lives : GameConstants.PlayerMaxLives;
            int level = player != null ? player.Level : 1;
            UIManager.Instance?.UpdateHUD(Score, HighScore, lives, level, Stage);
        }

        public void StartNewGame()
        {
            ClearProjectiles();
            Score = 0;
            Stage = 1;
            HighScore = RankingManager.Instance.GetHighScore();

            if (player != null) player.ResetPlayer(GameConstants.PlayerMaxLives, 1);
            if (fleetManager != null) fleetManager.CreateFleet();

            SetState(GameState.Playing);
        }

        public void ReturnToTitle()
        {
            ClearProjectiles();
            Score = 0;
            Stage = 1;
            HighScore = RankingManager.Instance.GetHighScore();

            if (player != null) player.ResetPlayer(GameConstants.PlayerMaxLives, 1);
            if (fleetManager != null) fleetManager.ClearFleet();

            SetState(GameState.Start);
        }

        public void ClearProjectiles()
        {
            PlayerMissile[] missiles = FindObjectsOfType<PlayerMissile>();
            for (int i = 0; i < missiles.Length; i++)
            {
                if (missiles[i] != null) Destroy(missiles[i].gameObject);
            }

            EnemyLaser[] lasers = FindObjectsOfType<EnemyLaser>();
            for (int i = 0; i < lasers.Length; i++)
            {
                if (lasers[i] != null) Destroy(lasers[i].gameObject);
            }
        }

        public void AdvanceToNextStage()
        {
            Stage++;
            int savedLives = player != null ? player.Lives : GameConstants.PlayerMaxLives;
            int savedLevel = player != null ? player.Level : 1;
            if (player != null) player.ResetPlayer(savedLives, savedLevel);
            if (fleetManager != null) fleetManager.CreateFleet();

            SetState(GameState.Playing);
        }

        public void AddScore(int points)
        {
            Score += points;
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            // 점수 기반 레벨업 체크
            int targetLevel = GameConstants.GetLevelForScore(Score);
            if (player != null && targetLevel > player.Level)
            {
                player.SetLevel(targetLevel);
            }

            RefreshHUD();
        }

        public void OnPlayerHit(int remainingLives)
        {
            RefreshHUD();
        }

        public void TriggerStageClear()
        {
            SetState(GameState.StageClear);
        }

        public void TriggerGameOver()
        {
            AudioManager.Instance?.PlayGameOver();
            SetState(GameState.GameOver);
        }
    }
}
