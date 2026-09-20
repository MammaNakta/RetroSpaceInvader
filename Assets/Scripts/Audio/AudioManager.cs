using UnityEngine;

namespace RetroSpaceInvader.Audio
{
    /// <summary>
    /// 8비트 아케이드 효과음 재생을 총괄하는 오디오 매니저
    /// (Pygame sound_manager.py와 1:1 대응)
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        public AudioClip shootClip;
        public AudioClip explosionClip;
        public AudioClip playerHitClip;
        public AudioClip gameOverClip;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSource();
        }

        private void EnsureAudioSource()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
                _audioSource.playOnAwake = false;
            }
        }

        public void PlayShoot()
        {
            EnsureAudioSource();
            if (_audioSource != null && shootClip != null) _audioSource.PlayOneShot(shootClip, 0.8f);
        }

        public void PlayExplosion()
        {
            EnsureAudioSource();
            if (_audioSource != null && explosionClip != null) _audioSource.PlayOneShot(explosionClip, 0.9f);
        }

        public void PlayPlayerHit()
        {
            EnsureAudioSource();
            if (_audioSource != null && playerHitClip != null) _audioSource.PlayOneShot(playerHitClip, 1.0f);
        }

        public void PlayGameOver()
        {
            EnsureAudioSource();
            if (_audioSource != null && gameOverClip != null) _audioSource.PlayOneShot(gameOverClip, 1.0f);
        }
    }
}
