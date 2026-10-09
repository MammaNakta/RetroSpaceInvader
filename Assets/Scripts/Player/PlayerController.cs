using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Audio;
using RetroSpaceInvader.UI;

namespace RetroSpaceInvader.Player
{
    /// <summary>
    /// 플레이어 우주선 컨트롤러
    /// - 좌우 이동 (화면 경계 클램프)
    /// - 스페이스바로 미사일 발사 (최대 2발 동시 존재 제한)
    /// - 피격 시 1초 무적 점멸 및 목숨 감소
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject missilePrefab;

        private int _lives = GameConstants.PlayerMaxLives;
        private float _invincibleTimer = 0f;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        public int Lives => _lives;
        public bool IsInvincible => _invincibleTimer > 0f;
        public int Level { get; private set; } = 1;

        public static bool GodMode { get; set; } = false;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
        }

        public void ResetPlayer(int initialLives = GameConstants.PlayerMaxLives, int initialLevel = 1)
        {
            _lives = initialLives;
            Level = Mathf.Clamp(initialLevel, 1, GameConstants.MaxPlayerLevel);
            _invincibleTimer = 0f;
            transform.position = new Vector3(0f, GameConstants.PlayerStartY, 0f);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                Color c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }
        }

        public void SetLevel(int newLevel)
        {
            int clamped = Mathf.Clamp(newLevel, 1, GameConstants.MaxPlayerLevel);
            if (Level != clamped)
            {
                Level = clamped;
                RetroSpaceInvader.UI.UIManager.Instance?.ShowLevelUpBanner(Level);
            }
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;

            HandleMovement();
            HandleShooting();
            HandleInvincibility();
        }

        private float GetCurrentSpeed()
        {
            // Lv1~2: 300, Lv3~4: 350, Lv5: 380
            if (Level >= 5) return 380f;
            if (Level >= 3) return 350f;
            return GameConstants.PlayerSpeed;
        }

        private int GetMaxAllowedMissiles()
        {
            switch (Level)
            {
                case 1: return 5;
                case 2: return 6;
                case 3: return 8;
                case 4: return 9;
                default: return 10;
            }
        }

        private void HandleMovement()
        {
            float inputX = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) inputX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) inputX += 1f;

            Vector3 pos = transform.position;
            pos.x += inputX * GetCurrentSpeed() * Time.deltaTime;

            // 화면 좌우 이탈 방지
            float limitX = GameConstants.HalfWidth - (GameConstants.PlayerWidth * 0.5f) - 10f;
            pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
            transform.position = pos;
        }

        private void HandleShooting()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int currentMissiles = FindObjectsByType<PlayerMissile>(FindObjectsSortMode.None).Length;
                if (currentMissiles < GetMaxAllowedMissiles() && missilePrefab != null)
                {
                    FireMissiles();
                    AudioManager.Instance?.PlayShoot();
                }
            }
        }

        private void FireMissiles()
        {
            float missileSpeed = (Level >= 3) ? 640f : GameConstants.MissileSpeed;
            int pierceCount = (Level >= 5) ? 2 : 1;
            float topY = transform.position.y + (GameConstants.PlayerHeight * 0.5f);

            switch (Level)
            {
                case 1: // 단발 중앙 발사
                    SpawnMissile(new Vector3(transform.position.x, topY, 0f), Vector2.up, missileSpeed, pierceCount);
                    break;

                case 2: // 트윈 캐논 (좌우 2발)
                case 3: // 고속 트윈 캐논
                    SpawnMissile(new Vector3(transform.position.x - 14f, topY, 0f), Vector2.up, missileSpeed, pierceCount);
                    SpawnMissile(new Vector3(transform.position.x + 14f, topY, 0f), Vector2.up, missileSpeed, pierceCount);
                    break;

                case 4: // 트리플 스프레드 (정면 1발 + 좌우 12도 부채꼴 2발)
                case 5: // 플라즈마 관통탄 (트리플 스프레드 + 2회 관통)
                    SpawnMissile(new Vector3(transform.position.x, topY, 0f), Vector2.up, missileSpeed, pierceCount);
                    SpawnMissile(new Vector3(transform.position.x - 14f, topY, 0f), new Vector2(-0.21f, 0.98f), missileSpeed, pierceCount);
                    SpawnMissile(new Vector3(transform.position.x + 14f, topY, 0f), new Vector2(0.21f, 0.98f), missileSpeed, pierceCount);
                    break;
            }
        }

        private void SpawnMissile(Vector3 pos, Vector2 dir, float speed, int pierce)
        {
            GameObject obj = Instantiate(missilePrefab, pos, Quaternion.identity);
            PlayerMissile missile = obj.GetComponent<PlayerMissile>();
            if (missile != null)
            {
                missile.Init(dir, speed, pierce);
            }
        }

        private void HandleInvincibility()
        {
            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.deltaTime;
                // 깜빡임 점멸 효과 (0.1초 단위)
                if (_spriteRenderer != null)
                {
                    bool visible = Mathf.FloorToInt(_invincibleTimer * 10f) % 2 == 0;
                    _spriteRenderer.enabled = visible;
                }
                if (_invincibleTimer <= 0f && _spriteRenderer != null)
                {
                    _spriteRenderer.enabled = true;
                }
            }
        }

        public bool TakeHit()
        {
            if (GodMode || IsInvincible) return false;

            _lives--;
            _invincibleTimer = GameConstants.PlayerInvincibleDuration;

            // 피격 페널티: 레벨 1단계 다운
            if (Level > 1)
            {
                SetLevel(Level - 1);
            }

            AudioManager.Instance?.PlayPlayerHit();
            GameManager.Instance.OnPlayerHit(_lives);

            if (_lives <= 0)
            {
                GameManager.Instance.TriggerGameOver();
            }

            return true;
        }
    }
}
