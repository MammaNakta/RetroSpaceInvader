using UnityEngine;

namespace RetroSpaceInvader.Effects
{
    /// <summary>
    /// 폭발 팽창 이펙트 및 레트로 파편 애니메이션
    /// (Pygame ExplosionEffect 및 ExplosionParticle 대응)
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        public float duration = 0.25f;
        public float maxScaleMultiplier = 1.45f;

        private float _timer = 0f;
        private Vector3 _initialScale;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _initialScale = transform.localScale;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float progress = Mathf.Clamp01(_timer / duration);

            // 순간 팽창 후 페이드아웃
            float scale = Mathf.Lerp(1.0f, maxScaleMultiplier, progress);
            transform.localScale = _initialScale * scale;

            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = Mathf.Lerp(1f, 0f, progress);
                _sr.color = c;
            }

            if (_timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
