using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Audio;

namespace RetroSpaceInvader.Enemy
{
    /// <summary>
    /// 개별 외계인 인베이더
    /// - 행(Row), 열(Col), 점수(ScoreValue)
    /// - 2프레임 스텝 애니메이션
    /// - 피격 시 폭발 및 점수 가산
    /// </summary>
    public class Alien : MonoBehaviour
    {
        public int row;
        public int col;
        public int scoreValue;

        [Header("Sprites")]
        public Sprite frame1;
        public Sprite frame2;

        public GameObject explosionPrefab;

        private SpriteRenderer _sr;
        private bool _isFrame1 = true;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Init(int r, int c, int score, Sprite f1, Sprite f2, GameObject explosion)
        {
            row = r;
            col = c;
            scoreValue = score;
            frame1 = f1;
            frame2 = f2;
            explosionPrefab = explosion;

            if (_sr != null && frame1 != null)
            {
                _sr.sprite = frame1;
            }
        }

        public void ToggleFrame()
        {
            _isFrame1 = !_isFrame1;
            if (_sr != null)
            {
                if (frame1 != null && frame2 != null)
                {
                    _sr.sprite = _isFrame1 ? frame1 : frame2;
                }
                else
                {
                    // 텍스처 프레임이 없을 경우 레트로 신축 펄스 효과 (기준 스케일 1.0)
                    transform.localScale = _isFrame1 
                        ? Vector3.one
                        : new Vector3(1.08f, 0.92f, 1f);
                }
            }
        }

        public void TakeHit()
        {
            AudioManager.Instance?.PlayExplosion();
            GameManager.Instance.AddScore(scoreValue);

            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                exp.transform.localScale = Vector3.one;
            }

            AlienFleetManager.Instance?.OnAlienKilled(this);
            Destroy(gameObject);
        }
    }
}
