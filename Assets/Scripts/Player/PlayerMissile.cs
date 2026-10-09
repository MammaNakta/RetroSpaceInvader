using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Enemy;

namespace RetroSpaceInvader.Player
{
    /// <summary>
    /// 플레이어 미사일 (상단으로 비행, 관통 및 방향 지원)
    /// </summary>
    public class PlayerMissile : MonoBehaviour
    {
        private Vector2 _direction = Vector2.up;
        private float _speed = GameConstants.MissileSpeed;
        private int _remainingPierce = 1;

        private void Awake()
        {
            transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(8f, 18f);
            }
        }

        public void Init(Vector2 dir, float speed = GameConstants.MissileSpeed, int maxPierce = 1)
        {
            _direction = dir.normalized;
            _speed = speed;
            _remainingPierce = Mathf.Max(1, maxPierce);

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)_direction * (_speed * Time.deltaTime);

            // 화면 밖으로 벗어나면 파괴
            if (transform.position.y > GameConstants.HalfHeight + 20f ||
                transform.position.x < -GameConstants.HalfWidth - 30f ||
                transform.position.x > GameConstants.HalfWidth + 30f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Alien alien = collision.GetComponent<Alien>();
            if (alien != null)
            {
                alien.TakeHit();
                _remainingPierce--;

                if (_remainingPierce <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
