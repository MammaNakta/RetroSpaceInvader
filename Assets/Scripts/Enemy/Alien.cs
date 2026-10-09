using UnityEngine;
using RetroSpaceInvader.Core;
using RetroSpaceInvader.Audio;
using RetroSpaceInvader.Player;

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
        public AlienTier tier = AlienTier.Tier1_Grunt;
        public int maxHp = 1;
        public int currentHp = 1;

        [Header("Sprites")]
        public Sprite frame1;
        public Sprite frame2;

        public GameObject explosionPrefab;

        private SpriteRenderer _sr;
        private bool _isFrame1 = true;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Init(int r, int c, int score, Sprite f1, Sprite f2, GameObject explosion, AlienTier alienTier = AlienTier.Tier1_Grunt, int hp = 1)
        {
            row = r;
            col = c;
            scoreValue = score;
            frame1 = f1;
            frame2 = f2;
            explosionPrefab = explosion;
            tier = alienTier;
            maxHp = hp;
            currentHp = hp;

            if (_sr != null && frame1 != null)
            {
                _sr.sprite = frame1;
                _sr.color = Color.white;
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
            currentHp--;

            if (currentHp > 0)
            {
                // 아직 체력이 남음: 피격 사운드 및 피격 플래시 연출
                AudioManager.Instance?.PlayPlayerHit(); // 찰진 타격음 공유
                if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
                _flashCoroutine = StartCoroutine(HitFlashRoutine());
                return;
            }

            // 체력 0 이하: 격파
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

        public bool IsDiving { get; private set; } = false;
        public Vector3 LocalFormationPos { get; set; }

        private Coroutine _diveCoroutine;

        public void StartDiveAttack(Vector3 targetPlayerPos, GameObject laserPrefab = null)
        {
            if (IsDiving) return;
            if (_diveCoroutine != null) StopCoroutine(_diveCoroutine);
            _diveCoroutine = StartCoroutine(DiveRoutine(targetPlayerPos, laserPrefab));
        }

        private System.Collections.IEnumerator DiveRoutine(Vector3 targetPlayerPos, GameObject laserPrefab)
        {
            IsDiving = true;
            Vector3 startPos = transform.position;
            // 플레이어를 향해 바깥쪽으로 부드럽게 휘어지는 제어점
            float sideOffset = (startPos.x < targetPlayerPos.x) ? -60f : 60f;
            Vector3 controlPoint = new Vector3((startPos.x + targetPlayerPos.x) * 0.5f + sideOffset, (startPos.y + targetPlayerPos.y) * 0.5f, 0f);
            Vector3 bottomExit = new Vector3(targetPlayerPos.x, -GameConstants.HalfHeight - 60f, 0f);

            // 1단계: 플레이어를 향해 2차 베지어 곡선으로 급강하 (약 1.3초)
            float elapsed = 0f;
            float diveDuration = 1.3f;
            bool firedLaser = false;

            while (elapsed < diveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / diveDuration);

                // 2차 베지어 곡선: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                Vector3 p = Mathf.Pow(1 - t, 2) * startPos + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * bottomExit;
                transform.position = p;

                // 절반 지점에서 플레이어 향해 레이저 1발 기습 투하
                if (!firedLaser && t > 0.45f && laserPrefab != null)
                {
                    firedLaser = true;
                    GameObject laserObj = Instantiate(laserPrefab, transform.position + Vector3.down * 12f, Quaternion.identity);
                    EnemyLaser el = laserObj.GetComponent<EnemyLaser>();
                    if (el != null) el.SetDirection(Vector2.down);
                }

                yield return null;
            }

            // 2단계: 편대의 현재 실시간 슬롯 위치 X 좌표 상단에서 재진입 (약 0.8초)
            Vector3 currentTargetWorld = (transform.parent != null)
                ? transform.parent.TransformPoint(LocalFormationPos)
                : LocalFormationPos;

            Vector3 topReentry = new Vector3(currentTargetWorld.x, GameConstants.HalfHeight + 40f, 0f);
            transform.position = topReentry;

            elapsed = 0f;
            float returnDuration = 0.8f;

            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnDuration);

                // 부모의 현재 로컬 편대 위치를 실시간 월드 좌표로 계산하여 복귀
                Vector3 targetWorldPos = (transform.parent != null)
                    ? transform.parent.TransformPoint(LocalFormationPos)
                    : LocalFormationPos;

                transform.position = Vector3.Lerp(topReentry, targetWorldPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            // 복귀 완료: 로컬 슬롯 좌표에 완벽 동기화
            if (transform.parent != null)
            {
                transform.localPosition = LocalFormationPos;
            }
            IsDiving = false;
            _diveCoroutine = null;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 급강하 다이브 중 플레이어와 직접 충돌 시 데미지 처리
            if (IsDiving)
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeHit();
                    TakeHit(); // 외계인 자신도 폭발
                }
            }
        }

        private System.Collections.IEnumerator HitFlashRoutine()
        {
            if (_sr != null)
            {
                Color original = Color.white;
                _sr.color = new Color(1f, 0.4f, 0.4f, 1f); // 붉은색 플래시
                yield return new WaitForSeconds(0.1f);
                if (_sr != null)
                {
                    _sr.color = original;
                }
            }
            _flashCoroutine = null;
        }
    }
}
