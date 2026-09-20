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
                    // 랭킹 등록 후 재시작 가능
                    if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                    {
                        StartNewGame();
                    }
                    break;
            }
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;

            UIManager.Instance?.ShowStartScreen(newState == GameState.Start);
            UIManager.Instance?.ShowStageClearScreen(newState == GameState.StageClear, Score);
            UIManager.Instance?.ShowGameOverScreen(newState == GameState.GameOver, Score);
            UIManager.Instance?.UpdateHUD(Score, HighScore, player != null ? player.Lives : GameConstants.PlayerMaxLives);
        }

        public void StartNewGame()
        {
            Score = 0;
            Stage = 1;
            HighScore = RankingManager.Instance.GetHighScore();

            if (player != null) player.ResetPlayer(GameConstants.PlayerMaxLives);
            if (fleetManager != null) fleetManager.CreateFleet();

            SetState(GameState.Playing);
        }

        public void AdvanceToNextStage()
        {
            Stage++;
            int savedLives = player != null ? player.Lives : GameConstants.PlayerMaxLives;
            if (player != null) player.ResetPlayer(savedLives);
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
            UIManager.Instance?.UpdateHUD(Score, HighScore, player != null ? player.Lives : 0);
        }

        public void OnPlayerHit(int remainingLives)
        {
            UIManager.Instance?.UpdateHUD(Score, HighScore, remainingLives);
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
