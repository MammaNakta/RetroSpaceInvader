using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Enemy;

namespace RetroSpaceInvader.Player
{
    /// <summary>
    /// 플레이어 미사일 (상단으로 비행, 외계인 충돌 판정)
    /// </summary>
    public class PlayerMissile : MonoBehaviour
    {
        private void Update()
        {
            transform.position += Vector3.up * (GameConstants.MissileSpeed * Time.deltaTime);

            // 화면 상단을 벗어나면 파괴
            if (transform.position.y > GameConstants.HalfHeight + 20f)
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
                Destroy(gameObject);
            }
        }
    }
}
