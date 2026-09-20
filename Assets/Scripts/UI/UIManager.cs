using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Ranking;

namespace RetroSpaceInvader.UI
{
    /// <summary>
    /// HUD (점수, 최고기록, 목숨 하트) 및
    /// 시작 화면, 스테이지 클리어, 게임오버(3글자 입력 및 TOP 5 랭킹) UI 제어
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Elements")]
        public Text scoreText;
        public Text highScoreText;
        public Image[] heartImages;
        public Sprite heartFullSprite;
        public Sprite heartEmptySprite;

        [Header("Panels")]
        public GameObject startPanel;
        public GameObject stageClearPanel;
        public GameObject gameOverPanel;

        [Header("Stage Clear Texts")]
        public Text stageClearScoreText;

        [Header("Game Over / Ranking Elements")]
        public Text gameOverFinalScoreText;
        public Text[] initialSlotTexts;     // 3글자 이니셜 슬롯 (A A A)
        public Image[] initialSlotBorders;  // 테두리 하이라이트
        public Text initialGuideText;
        public Text registeredInfoText;

        [Header("Hall of Fame Table (Top 5)")]
        public Text[] rankRowsText;   // 5개 행 텍스트

        private string _playerInitials = "";
        private bool _rankingSubmitted = false;
        private int _currentRank = 0;
        private float _cursorBlinkTimer = 0f;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateHUD(int score, int highScore, int lives)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score:D6}";
            if (highScoreText != null) highScoreText.text = $"HIGH: {highScore:D6}";

            if (heartImages != null)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] != null)
                    {
                        heartImages[i].sprite = (i < lives) ? heartFullSprite : heartEmptySprite;
                        heartImages[i].color = (i < lives) ? new Color(1f, 0.2f, 0.35f) : new Color(0.35f, 0.35f, 0.45f);
                    }
                }
            }
        }

        public void ShowStartScreen(bool show)
        {
            if (startPanel != null) startPanel.SetActive(show);
        }

        public void ShowStageClearScreen(bool show, int score)
        {
            if (stageClearPanel != null) stageClearPanel.SetActive(show);
            if (show && stageClearScoreText != null)
            {
                stageClearScoreText.text = $"외계인 편대 격파 완료! (현재 점수: {score}점)";
            }
        }

        public void ShowGameOverScreen(bool show, int finalScore)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(show);
            if (!show) return;

            _playerInitials = "";
            _rankingSubmitted = false;
            _currentRank = 0;
            _cursorBlinkTimer = 0f;

            if (gameOverFinalScoreText != null)
            {
                gameOverFinalScoreText.text = $"최종 획득 점수 : {finalScore:D6} PTS";
            }

            if (registeredInfoText != null) registeredInfoText.gameObject.SetActive(false);
            if (initialGuideText != null) initialGuideText.gameObject.SetActive(true);

            UpdateInitialSlots();
            RefreshLeaderboardTable();
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.GameOver)
                return;

            _cursorBlinkTimer += Time.deltaTime;

            if (!_rankingSubmitted)
            {
                HandleInitialInput();
                UpdateInitialSlots();
            }
        }

        private void HandleInitialInput()
        {
            // Backspace: 이전 글자 삭제
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_playerInitials.Length > 0)
                {
                    _playerInitials = _playerInitials.Substring(0, _playerInitials.Length - 1);
                }
            }
            // Enter: 랭킹 등록 확정
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                string name = string.IsNullOrEmpty(_playerInitials) ? "AAA" : _playerInitials.PadRight(3, 'A').Substring(0, 3);
                _playerInitials = name;
                _currentRank = RankingManager.Instance.AddScore(name, GameManager.Instance.Score);
                _rankingSubmitted = true;

                if (registeredInfoText != null)
                {
                    registeredInfoText.gameObject.SetActive(true);
                    registeredInfoText.text = (_currentRank > 0) 
                        ? $"★ 랭킹 등록 완료! (순위: {_currentRank}위) ★" 
                        : "★ 랭킹 등록 완료! ★";
                }
                if (initialGuideText != null) initialGuideText.gameObject.SetActive(false);

                RefreshLeaderboardTable();
            }
            // 영문 알파벳 A-Z 입력
            else
            {
                foreach (char c in Input.inputString)
                {
                    if (char.IsLetter(c) && _playerInitials.Length < 3)
                    {
                        _playerInitials += char.ToUpper(c);
                    }
                }
            }
        }

        private void UpdateInitialSlots()
        {
            if (initialSlotTexts == null || initialSlotTexts.Length < 3) return;

            bool cursorVisible = Mathf.FloorToInt(_cursorBlinkTimer * 3f) % 2 == 0;

            for (int i = 0; i < 3; i++)
            {
                if (i < _playerInitials.Length)
                {
                    initialSlotTexts[i].text = _playerInitials[i].ToString();
                }
                else if (i == _playerInitials.Length && !_rankingSubmitted && cursorVisible)
                {
                    initialSlotTexts[i].text = "_";
                }
                else
                {
                    initialSlotTexts[i].text = "";
                }

                if (initialSlotBorders != null && i < initialSlotBorders.Length && initialSlotBorders[i] != null)
                {
                    bool isCurrent = (i == _playerInitials.Length);
                    initialSlotBorders[i].color = isCurrent ? GameConstants.ColorPlayer : new Color(0.3f, 0.3f, 0.45f);
                }
            }
        }

        public void RefreshLeaderboardTable()
        {
            if (rankRowsText == null) return;

            List<RankingEntry> entries = RankingManager.Instance.GetEntries();
            string[] badges = { "1ST", "2ND", "3RD", "4TH", "5TH" };

            for (int i = 0; i < rankRowsText.Length && i < 5; i++)
            {
                if (rankRowsText[i] == null) continue;

                if (i < entries.Count)
                {
                    RankingEntry e = entries[i];
                    int rankNum = i + 1;
                    string badge = (i < badges.Length) ? badges[i] : $"{rankNum}TH";

                    // 방금 등록한 플레이어 강조
                    bool isNew = (_rankingSubmitted && rankNum == _currentRank);
                    string colorTag = isNew ? "#39FF14" : (rankNum == 1 ? "#FFD700" : "#FFFFFF");

                    rankRowsText[i].text = $"<color={colorTag}>{badge, -4}   {e.name, -5}   {e.score, 6:D6} PTS   {e.date}</color>";
                }
                else
                {
                    rankRowsText[i].text = $"<color=#666677>---     ---      ------ PTS   ----.--.--</color>";
                }
            }
        }
    }
}
