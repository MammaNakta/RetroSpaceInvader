using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Audio;

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

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
        }

        public void ResetPlayer(int initialLives = GameConstants.PlayerMaxLives)
        {
            _lives = initialLives;
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

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;

            HandleMovement();
            HandleShooting();
            HandleInvincibility();
        }

        private void HandleMovement()
        {
            float inputX = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) inputX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) inputX += 1f;

            Vector3 pos = transform.position;
            pos.x += inputX * GameConstants.PlayerSpeed * Time.deltaTime;

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
                if (currentMissiles < GameConstants.PlayerMaxMissiles)
                {
                    if (missilePrefab != null)
                    {
                        Vector3 spawnPos = transform.position + Vector3.up * (GameConstants.PlayerHeight * 0.5f);
                        Instantiate(missilePrefab, spawnPos, Quaternion.identity);
                        AudioManager.Instance?.PlayShoot();
                    }
                }
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
            if (IsInvincible) return false;

            _lives--;
            _invincibleTimer = GameConstants.PlayerInvincibleDuration;
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
