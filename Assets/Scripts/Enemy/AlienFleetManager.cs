using System.Collections.Generic;
using UnityEngine;
using RetroSpaceInvader.Core;

namespace RetroSpaceInvader.Enemy
{
    /// <summary>
    /// 5x5 외계인 편대 생성, 일괄 이동 및 가속, 적 탄환 투하 제어
    /// (Pygame game.py의 _update_aliens, _update_enemy_shooting과 1:1 대응)
    /// </summary>
    public class AlienFleetManager : MonoBehaviour
    {
        public static AlienFleetManager Instance { get; private set; }

        [Header("Alien Sprites")]
        public Sprite alienTopSprite;
        public Sprite alienMidSprite;
        public Sprite alienBotSprite;

        [Header("Prefabs")]
        public GameObject alienPrefab;
        public GameObject enemyLaserPrefab;
        public GameObject explosionPrefab;

        private readonly List<Alien> _aliens = new List<Alien>();
        private float _moveTimer = 0f;
        private float _shootTimer = 0f;
        private int _direction = 1; // 1: 오른쪽, -1: 왼쪽
        private int _totalInitialAliens = 25;

        public int RemainingCount => _aliens.Count;

        private void Awake()
        {
            Instance = this;
        }

        public void CreateFleet()
        {
            ClearFleet();
            _direction = 1;
            _moveTimer = 0f;
            _shootTimer = 0f;
            _totalInitialAliens = GameConstants.AlienRows * GameConstants.AlienCols;

            for (int r = 0; r < GameConstants.AlienRows; r++)
            {
                Sprite spriteToUse = (r == 0) ? alienTopSprite : (r <= 2 ? alienMidSprite : alienBotSprite);
                int score = GameConstants.RowScores[r];

                for (int c = 0; c < GameConstants.AlienCols; c++)
                {
                    float x = GameConstants.AlienStartX + c * (GameConstants.AlienWidth + GameConstants.AlienSpacingX);
                    float y = GameConstants.AlienStartY - r * (GameConstants.AlienHeight + GameConstants.AlienSpacingY);

                    GameObject obj = Instantiate(alienPrefab, transform);
                    obj.transform.localPosition = new Vector3(x, y, 0f);
                    obj.name = $"Alien_{r}_{c}";

                    Alien alien = obj.GetComponent<Alien>();
                    alien.Init(r, c, score, spriteToUse, null, explosionPrefab);
                    _aliens.Add(alien);
                }
            }
        }

        public void ClearFleet()
        {
            foreach (Alien a in _aliens)
            {
                if (a != null) Destroy(a.gameObject);
            }
            _aliens.Clear();

            // 필드 위의 모든 적 탄환 제거
            EnemyLaser[] lasers = FindObjectsByType<EnemyLaser>(FindObjectsSortMode.None);
            foreach (EnemyLaser l in lasers) Destroy(l.gameObject);
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (_aliens.Count == 0)
                return;

            UpdateMovement();
            UpdateShooting();
        }

        private void UpdateMovement()
        {
            _moveTimer += Time.deltaTime;
            float currentInterval = GetCurrentMoveInterval();

            if (_moveTimer >= currentInterval)
            {
                _moveTimer = 0f;

                // 벽 충돌 검사
                bool shouldMoveDown = false;
                float boundaryLimit = GameConstants.HalfWidth - 30f;

                foreach (Alien alien in _aliens)
                {
                    if (alien == null) continue;
                    float nextX = alien.transform.position.x + (GameConstants.AlienStepX * _direction);
                    if (nextX < -boundaryLimit || nextX > boundaryLimit)
                    {
                        shouldMoveDown = true;
                        break;
                    }
                }

                if (shouldMoveDown)
                {
                    _direction *= -1;
                    foreach (Alien alien in _aliens)
                    {
                        if (alien == null) continue;
                        Vector3 pos = alien.transform.position;
                        pos.y -= GameConstants.AlienMoveDownStep;
                        alien.transform.position = pos;
                        alien.ToggleFrame();
                    }
                }
                else
                {
                    foreach (Alien alien in _aliens)
                    {
                        if (alien == null) continue;
                        Vector3 pos = alien.transform.position;
                        pos.x += GameConstants.AlienStepX * _direction;
                        alien.transform.position = pos;
                        alien.ToggleFrame();
                    }
                }

                // 방어선 도달 검사 (플레이어 라인 침공 -> 게임오버)
                foreach (Alien alien in _aliens)
                {
                    if (alien != null && alien.transform.position.y <= GameConstants.DefenseLineY)
                    {
                        GameManager.Instance.TriggerGameOver();
                        break;
                    }
                }
            }
        }

        private void UpdateShooting()
        {
            _shootTimer += Time.deltaTime;
            if (_shootTimer >= GameConstants.EnemyShootInterval)
            {
                _shootTimer = 0f;

                // 열별 최하단 외계인 검색
                Dictionary<int, Alien> bottomByCol = new Dictionary<int, Alien>();
                foreach (Alien alien in _aliens)
                {
                    if (alien == null) continue;
                    int col = alien.col;
                    if (!bottomByCol.ContainsKey(col) || alien.transform.position.y < bottomByCol[col].transform.position.y)
                    {
                        bottomByCol[col] = alien;
                    }
                }

                List<Alien> candidates = new List<Alien>(bottomByCol.Values);
                if (candidates.Count > 0 && enemyLaserPrefab != null)
                {
                    Alien shooter = candidates[Random.Range(0, candidates.Count)];
                    Vector3 spawnPos = shooter.transform.position + Vector3.down * (GameConstants.AlienHeight * 0.5f);
                    Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);
                }
            }
        }

        private float GetCurrentMoveInterval()
        {
            if (_aliens.Count <= 0) return GameConstants.AlienBaseMoveInterval;
            float ratio = (float)(_aliens.Count - 1) / Mathf.Max(1, _totalInitialAliens - 1);
            return Mathf.Lerp(GameConstants.AlienMinMoveInterval, GameConstants.AlienBaseMoveInterval, ratio);
        }

        public void OnAlienKilled(Alien alien)
        {
            _aliens.Remove(alien);
            if (_aliens.Count == 0)
            {
                GameManager.Instance.TriggerStageClear();
            }
        }
    }
}
