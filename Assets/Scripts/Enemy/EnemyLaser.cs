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
        private void Update()
        {
            transform.position += Vector3.down * (GameConstants.EnemyLaserSpeed * Time.deltaTime);

            // 화면 하단 벗어나면 소멸
            if (transform.position.y < -GameConstants.HalfHeight - 20f)
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
