using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Player;

namespace RetroSpaceInvader.Enemy
{
    /// <summary>
    /// 외계인이 투하하는 적 레이저 탄환
    /// </summary>
    public class EnemyLaser : MonoBehaviour
    {
        private Vector2 _direction = Vector2.down;
        private float _speed = GameConstants.EnemyLaserSpeed;
        private bool _isParabolic = false;
        private Vector2 _velocity;
        private float _gravity = 420f;

        private void Awake()
        {
            // 탄환 이미지 크기 75%로 슬림화
            transform.localScale = new Vector3(0.75f, 0.75f, 1f);

            // 이미지보다 훨씬 작고 날렵한 중심 코어 피탄 히트박스 적용 (가로 6px, 세로 14px)
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(6f, 14f);
            }
        }

        public void SetDirection(Vector2 dir, float speed = GameConstants.EnemyLaserSpeed)
        {
            _isParabolic = false;
            _direction = dir.normalized;
            _speed = speed;

            // 방향에 맞게 탄환 회전 각도 설정
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void SetParabolicMotion(Vector2 initialVelocity, float gravity = 420f, Color? customColor = null)
        {
            _isParabolic = true;
            _velocity = initialVelocity;
            _gravity = gravity;

            // 곡사탄 시각적 차별화 (황금 주황색)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = customColor ?? new Color(1f, 0.65f, 0.15f, 1f);
            }

            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (_isParabolic)
            {
                _velocity.y -= _gravity * Time.deltaTime;
                transform.position += (Vector3)_velocity * Time.deltaTime;

                // 궤적에 따른 실시간 탄환 회전
                float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg + 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            else
            {
                transform.position += (Vector3)_direction * (_speed * Time.deltaTime);
            }

            // 화면 하단, 상단 또는 좌우를 벗어나면 소멸
            if (transform.position.y < -GameConstants.HalfHeight - 20f ||
                transform.position.y > GameConstants.HalfHeight + 80f ||
                transform.position.x < -GameConstants.HalfWidth - 40f ||
                transform.position.x > GameConstants.HalfWidth + 40f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.TakeHit())
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
