using System.Collections.Generic;
using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.UI;

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
        private float _mortarTimer = 0f;
        private int _direction = 1; // 1: 오른쪽, -1: 왼쪽
        private int _totalInitialAliens = 25;

        public int RemainingCount => _aliens.Count;
        public FleetMovementMode CurrentMovementMode { get; private set; } = FleetMovementMode.GalagaDive;
        private float _diveTimer = 0f;

        // 0: 빈칸, 1: Tier1 Grunt(10점, 1HP), 2: Tier2 Veteran(20점, 1HP, 2점사), 3: Tier3 Commander(30점, 2HP, 확산탄)
        private static readonly int[][,] StagePatterns = new int[][,]
        {
            // Stage 1: 표준 격자 대형 (5x5, 25기)
            new int[,] {
                { 3, 3, 3, 3, 3 },
                { 2, 2, 2, 2, 2 },
                { 2, 2, 2, 2, 2 },
                { 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1 },
            },
            // Stage 2: V-대형 (침공 쐐기 형태, 22기)
            new int[,] {
                { 0, 0, 0, 3, 0, 0, 0 },
                { 0, 0, 3, 2, 3, 0, 0 },
                { 0, 2, 2, 1, 2, 2, 0 },
                { 2, 1, 1, 0, 1, 1, 2 },
                { 1, 1, 0, 0, 0, 1, 1 },
            },
            // Stage 3: Split Twins (양 날개 분할 대형, 24기)
            new int[,] {
                { 3, 3, 3, 0, 0, 3, 3, 3 },
                { 2, 2, 2, 0, 0, 2, 2, 2 },
                { 2, 2, 0, 0, 0, 0, 2, 2 },
                { 1, 1, 1, 0, 0, 1, 1, 1 },
                { 1, 1, 1, 0, 0, 1, 1, 1 },
            },
            // Stage 4: Diamond 호위 대형 (마름모꼴, 26기)
            new int[,] {
                { 0, 0, 0, 3, 0, 0, 0 },
                { 0, 0, 3, 3, 3, 0, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 2, 2, 1, 1, 1, 2, 2 },
                { 0, 1, 1, 1, 1, 1, 0 },
                { 0, 0, 1, 0, 1, 0, 0 },
            }
        };

        private void Awake()
        {
            Instance = this;
        }

        public void SetMovementMode(FleetMovementMode mode)
        {
            CurrentMovementMode = mode;
            _diveTimer = 0f;
            UIManager.Instance?.ShowMovementModeBanner(CurrentMovementMode);
        }

        public void CycleMovementMode()
        {
            FleetMovementMode[] modes = (FleetMovementMode[])System.Enum.GetValues(typeof(FleetMovementMode));
            int next = ((int)CurrentMovementMode + 1) % modes.Length;
            SetMovementMode(modes[next]);
        }

        public void CreateFleet()
        {
            ClearFleet();
            StopAllCoroutines();

            _direction = 1;
            _moveTimer = 0f;
            _shootTimer = 0f;
            _mortarTimer = 0f;
            _diveTimer = 0f;

            int currentStage = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
            int patternIndex = (currentStage - 1) % StagePatterns.Length;
            int[,] pattern = StagePatterns[patternIndex];

            // 4가지 기동 모드를 스테이지마다 무작위 셔플 추첨 (이전과 다르게)
            FleetMovementMode[] allModes = (FleetMovementMode[])System.Enum.GetValues(typeof(FleetMovementMode));
            CurrentMovementMode = allModes[(currentStage - 1) % allModes.Length];
            UIManager.Instance?.ShowMovementModeBanner(CurrentMovementMode);

            int rows = pattern.GetLength(0);
            int cols = pattern.GetLength(1);

            float totalWidth = (cols - 1) * (GameConstants.AlienWidth + GameConstants.AlienSpacingX);
            float startX = -totalWidth * 0.5f;
            float startY = GameConstants.AlienStartY;

            int count = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int type = pattern[r, c];
                    if (type == 0) continue;

                    Sprite spriteToUse;
                    int score;
                    AlienTier tier;
                    int hp;

                    if (type == 3)
                    {
                        spriteToUse = alienTopSprite;
                        score = GameConstants.ScoreAlienTop;
                        tier = AlienTier.Tier3_Commander;
                        hp = 2; // 커맨더는 2 HP
                    }
                    else if (type == 2)
                    {
                        spriteToUse = alienMidSprite;
                        score = GameConstants.ScoreAlienMiddle;
                        tier = AlienTier.Tier2_Veteran;
                        hp = 1;
                    }
                    else
                    {
                        spriteToUse = alienBotSprite;
                        score = GameConstants.ScoreAlienBottom;
                        tier = AlienTier.Tier1_Grunt;
                        hp = 1;
                    }

                    float x = startX + c * (GameConstants.AlienWidth + GameConstants.AlienSpacingX);
                    float y = startY - r * (GameConstants.AlienHeight + GameConstants.AlienSpacingY);

                    GameObject obj = Instantiate(alienPrefab, transform);
                    obj.transform.localPosition = new Vector3(x, y, 0f);
                    obj.name = $"Alien_{r}_{c}_{tier}";

                    Alien alien = obj.GetComponent<Alien>();
                    alien.Init(r, c, score, spriteToUse, null, explosionPrefab, tier, hp);
                    alien.LocalFormationPos = new Vector3(x, y, 0f);
                    _aliens.Add(alien);
                    count++;
                }
            }

            _totalInitialAliens = Mathf.Max(1, count);
        }

        public void ClearFleet()
        {
            StopAllCoroutines();
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
            UpdateSpecialMovementEffects();
            UpdateShooting();
            UpdateMortarShooting();
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
                    if (alien == null || alien.IsDiving) continue;
                    int rowDir = (CurrentMovementMode == FleetMovementMode.AlternatingSweep && alien.row % 2 == 1) ? -_direction : _direction;
                    float nextX = alien.transform.position.x + (GameConstants.AlienStepX * rowDir);
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

                        if (alien.IsDiving)
                        {
                            // 다이브 중인 적도 편대 하강에 맞춰 목표 복귀 위치를 정확히 동기화
                            Vector3 targetPos = alien.LocalFormationPos;
                            targetPos.y -= GameConstants.AlienMoveDownStep;
                            alien.LocalFormationPos = targetPos;
                            continue;
                        }

                        Vector3 pos = alien.transform.position;
                        pos.y -= GameConstants.AlienMoveDownStep;
                        alien.transform.position = pos;
                        alien.LocalFormationPos = alien.transform.localPosition;
                        alien.ToggleFrame();
                    }
                }
                else
                {
                    foreach (Alien alien in _aliens)
                    {
                        if (alien == null) continue;
                        int rowDir = (CurrentMovementMode == FleetMovementMode.AlternatingSweep && alien.row % 2 == 1) ? -_direction : _direction;

                        if (alien.IsDiving)
                        {
                            // 다이브 중인 적도 편대 좌우 이동에 맞춰 목표 복귀 위치를 정확히 동기화
                            Vector3 targetPos = alien.LocalFormationPos;
                            targetPos.x += GameConstants.AlienStepX * rowDir;
                            alien.LocalFormationPos = targetPos;
                            continue;
                        }

                        Vector3 pos = alien.transform.position;
                        pos.x += GameConstants.AlienStepX * rowDir;
                        alien.transform.position = pos;
                        alien.LocalFormationPos = alien.transform.localPosition;
                        alien.ToggleFrame();
                    }
                }

                // 방어선 도달 검사 (플레이어 라인 침공 -> 게임오버)
                foreach (Alien alien in _aliens)
                {
                    if (alien != null && !alien.IsDiving && alien.transform.position.y <= GameConstants.DefenseLineY)
                    {
                        GameManager.Instance.TriggerGameOver();
                        break;
                    }
                }
            }
        }

        private void UpdateSpecialMovementEffects()
        {
            // 1. 갤러그식 급강하 다이브 모드 처리
            if (CurrentMovementMode == FleetMovementMode.GalagaDive)
            {
                // 이미 다이브 중인 외계인이 있다면 복귀할 때까지 대기 (자리 겹침 원천 차단)
                bool anyDiving = _aliens.Exists(x => x != null && x.IsDiving);
                if (anyDiving) return;

                _diveTimer += Time.deltaTime;
                int stage = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
                float diveInterval = Mathf.Max(2.2f, 3.8f - (stage - 1) * 0.3f);

                if (_diveTimer >= diveInterval)
                {
                    _diveTimer = 0f;

                    // 다이브 중이 아닌 외계인 중 1마리 선택 (Tier 2/3 우선)
                    List<Alien> eligible = new List<Alien>();
                    foreach (Alien a in _aliens)
                    {
                        if (a != null && !a.IsDiving) eligible.Add(a);
                    }

                    if (eligible.Count > 0)
                    {
                        // Tier 2, 3 우선 필터
                        List<Alien> highTiers = eligible.FindAll(x => x.tier != AlienTier.Tier1_Grunt);
                        Alien diver = (highTiers.Count > 0)
                            ? highTiers[Random.Range(0, highTiers.Count)]
                            : eligible[Random.Range(0, eligible.Count)];

                        Vector3 targetPlayerPos = (GameManager.Instance != null && GameManager.Instance.player != null)
                            ? GameManager.Instance.player.transform.position
                            : Vector3.zero;

                        diver.StartDiveAttack(targetPlayerPos, enemyLaserPrefab);
                    }
                }
            }
            // 2. 사인파 파도 유영 모드 (상하 실시간 물결)
            else if (CurrentMovementMode == FleetMovementMode.SineWave)
            {
                float time = Time.time * 3.8f;
                foreach (Alien alien in _aliens)
                {
                    if (alien == null || alien.IsDiving) continue;
                    Vector3 basePos = alien.transform.parent != null
                        ? alien.transform.parent.TransformPoint(alien.LocalFormationPos)
                        : alien.LocalFormationPos;

                    float waveY = Mathf.Sin(time + alien.col * 0.5f) * 12f;
                    alien.transform.position = new Vector3(alien.transform.position.x, basePos.y + waveY, 0f);
                }
            }
            // 3. 아코디언 신축 펄스 모드 (좌우 팽창 및 수축)
            else if (CurrentMovementMode == FleetMovementMode.AccordionPulse)
            {
                float pulse = Mathf.Sin(Time.time * 2.8f) * 5.5f;
                foreach (Alien alien in _aliens)
                {
                    if (alien == null || alien.IsDiving) continue;
                    Vector3 basePos = alien.transform.parent != null
                        ? alien.transform.parent.TransformPoint(alien.LocalFormationPos)
                        : alien.LocalFormationPos;

                    float offsetX = (alien.col - 3f) * pulse;
                    alien.transform.position = new Vector3(basePos.x + offsetX, alien.transform.position.y, 0f);
                }
            }
        }

        private void UpdateShooting()
        {
            _shootTimer += Time.deltaTime;
            int stage = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
            float stageShootInterval = Mathf.Max(0.6f, GameConstants.EnemyShootInterval - (stage - 1) * 0.12f);

            if (_shootTimer >= stageShootInterval)
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
                    ExecuteAlienShoot(shooter);
                }
            }
        }

        private void UpdateMortarShooting()
        {
            _mortarTimer += Time.deltaTime;
            int stage = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
            // 스테이지가 올라갈수록 곡사탄 주기 단축 (3.0초 -> 1.8초)
            float mortarInterval = Mathf.Max(1.8f, 3.0f - (stage - 1) * 0.25f);

            if (_mortarTimer >= mortarInterval)
            {
                _mortarTimer = 0f;

                // 최하단 적들 검색
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
                HashSet<Alien> bottomAliens = new HashSet<Alien>(bottomByCol.Values);

                // 최하단이 아닌 후방/상단 적 리스트 추출
                List<Alien> backLineCandidates = new List<Alien>();
                foreach (Alien a in _aliens)
                {
                    if (a != null && !bottomAliens.Contains(a))
                    {
                        backLineCandidates.Add(a);
                    }
                }

                // 후방 적이 있으면 그 중 무작위 1기, 없으면 잔여 적 중 1기 선택
                Alien mortarShooter = null;
                if (backLineCandidates.Count > 0)
                {
                    mortarShooter = backLineCandidates[Random.Range(0, backLineCandidates.Count)];
                }
                else if (_aliens.Count > 0)
                {
                    mortarShooter = _aliens[Random.Range(0, _aliens.Count)];
                }

                if (mortarShooter != null && enemyLaserPrefab != null)
                {
                    ExecuteMortarShoot(mortarShooter);
                }
            }
        }

        private void ExecuteMortarShoot(Alien shooter)
        {
            Vector3 spawnPos = shooter.transform.position + Vector3.up * 8f; // 아군 머리 위로 발사
            float y0 = spawnPos.y;
            float targetY = GameConstants.PlayerStartY;

            float targetX = 0f;
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                targetX = GameManager.Instance.player.transform.position.x;
            }
            // 탄착군 예측 및 미세 오차 부여 (±25px)
            targetX += Random.Range(-25f, 25f);

            float vy = 200f; // 초기 상승 속도
            float gravity = 420f;
            float disc = (vy * vy) - (2f * gravity * (targetY - y0));

            if (disc > 0f)
            {
                float t = (vy + Mathf.Sqrt(disc)) / gravity;
                float vx = (targetX - spawnPos.x) / t;
                vx = Mathf.Clamp(vx, -260f, 260f);

                GameObject obj = Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);
                EnemyLaser laser = obj.GetComponent<EnemyLaser>();
                if (laser != null)
                {
                    laser.SetParabolicMotion(new Vector2(vx, vy), gravity);
                }
            }
        }

        private void ExecuteAlienShoot(Alien shooter)
        {
            if (shooter == null || enemyLaserPrefab == null) return;
            Vector3 spawnPos = shooter.transform.position + Vector3.down * (GameConstants.AlienHeight * 0.5f);

            switch (shooter.tier)
            {
                case AlienTier.Tier1_Grunt:
                    CreateLaser(spawnPos, Vector2.down);
                    break;

                case AlienTier.Tier2_Veteran:
                    StartCoroutine(ShootBurstRoutine(shooter));
                    break;

                case AlienTier.Tier3_Commander:
                    // 시간차를 두고 좌 -> 중앙 -> 우 3방향 순차 발사
                    StartCoroutine(ShootSequentialSpreadRoutine(shooter));
                    break;
            }
        }

        private System.Collections.IEnumerator ShootSequentialSpreadRoutine(Alien shooter)
        {
            if (shooter == null) yield break;

            // 세 방향 벡터 (좌 12도, 중앙, 우 12도)
            List<Vector2> directions = new List<Vector2>
            {
                new Vector2(-0.21f, -0.98f), // 좌측
                Vector2.down,                // 중앙
                new Vector2(0.21f, -0.98f)  // 우측
            };

            // 발사 순서 무작위 셔플 (랜덤 발사 순서)
            for (int i = directions.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                Vector2 temp = directions[i];
                directions[i] = directions[rnd];
                directions[rnd] = temp;
            }

            // 0.3초 간격으로 순차 발사
            for (int i = 0; i < directions.Count; i++)
            {
                if (shooter == null) yield break;

                Vector3 spawnPos = shooter.transform.position + Vector3.down * (GameConstants.AlienHeight * 0.5f);
                CreateLaser(spawnPos, directions[i]);

                if (i < directions.Count - 1)
                {
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        private System.Collections.IEnumerator ShootBurstRoutine(Alien shooter)
        {
            if (shooter == null) yield break;
            Vector3 spawnPos1 = shooter.transform.position + Vector3.down * (GameConstants.AlienHeight * 0.5f);
            CreateLaser(spawnPos1, Vector2.down);

            yield return new WaitForSeconds(0.16f);

            if (shooter != null)
            {
                Vector3 spawnPos2 = shooter.transform.position + Vector3.down * (GameConstants.AlienHeight * 0.5f);
                CreateLaser(spawnPos2, Vector2.down);
            }
        }

        private void CreateLaser(Vector3 position, Vector2 direction)
        {
            if (enemyLaserPrefab == null) return;
            GameObject obj = Instantiate(enemyLaserPrefab, position, Quaternion.identity);
            EnemyLaser laser = obj.GetComponent<EnemyLaser>();
            if (laser != null)
            {
                laser.SetDirection(direction);
            }
        }

        private float GetCurrentMoveInterval()
        {
            if (_aliens.Count <= 0) return GameConstants.AlienBaseMoveInterval;
            int stage = GameManager.Instance != null ? GameManager.Instance.Stage : 1;
            float baseInterval = Mathf.Max(0.22f, GameConstants.AlienBaseMoveInterval - (stage - 1) * 0.04f);

            float ratio = (float)(_aliens.Count - 1) / Mathf.Max(1, _totalInitialAliens - 1);
            return Mathf.Lerp(GameConstants.AlienMinMoveInterval, baseInterval, ratio);
        }

        public void OnAlienKilled(Alien alien)
        {
            _aliens.Remove(alien);
            if (_aliens.Count == 0)
            {
                GameManager.Instance.TriggerStageClear();
            }
        }

        /// <summary>
        /// 디버그 하네스용: 현재 필드의 모든 외계인을 즉시 파괴하여 스테이지 클리어 유도
        /// </summary>
        public void KillAllAliens()
        {
            List<Alien> toKill = new List<Alien>(_aliens);
            foreach (var a in toKill)
            {
                if (a != null)
                {
                    Destroy(a.gameObject);
                }
            }
            _aliens.Clear();
            GameManager.Instance.TriggerStageClear();
        }
    }
}
