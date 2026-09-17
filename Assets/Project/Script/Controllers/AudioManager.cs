using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    /// <summary>
    /// Small presentation-only audio service for overlapping SFX and one looping track.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX")]
        [SerializeField] private AudioClip _swap;
        [SerializeField] private AudioClip _invalidSwap;
        [SerializeField] private AudioClip _landing;
        [SerializeField] private AudioClip[] _matchPops;
        [SerializeField] private AudioClip _specialCreated;
        [SerializeField] private AudioClip _specialActivated;
        [SerializeField] private AudioClip _scoreGain;
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField, Range(0.0f, 1.0f)] private float _sfxVolume = 0.65f;

        [Header("Music")]
        [SerializeField] private AudioClip _gameplayMusic;
        [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.25f;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private int _nextPopIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _sfxSource = CreateSource("SFX", false, _sfxVolume);
            _musicSource = CreateSource("Music", true, _musicVolume);
            PlayMusic(_gameplayMusic);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlaySwap() => PlaySfx(_swap);

        public void PlayInvalidSwap() => PlaySfx(_invalidSwap);

        public void PlayLanding() => PlaySfx(_landing);

        public void PlayMatchPop()
        {
            if (_matchPops == null || _matchPops.Length == 0)
            {
                return;
            }

            AudioClip clip = _matchPops[_nextPopIndex % _matchPops.Length];
            _nextPopIndex++;
            PlaySfx(clip);
        }

        public void PlaySpecialCreated() => PlaySfx(_specialCreated);

        public void PlaySpecialActivated() => PlaySfx(_specialActivated);

        public void PlayScoreGain(float pitch = 1.0f) => PlaySfx(_scoreGain, 1.0f, pitch);

        public void PlayButtonClick() => PlaySfx(_buttonClick);

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || _musicSource == null || _musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        private void PlaySfx(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null || _sfxSource == null)
            {
                return;
            }

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, _sfxVolume * volume);
            _sfxSource.pitch = 1.0f;
        }

        private AudioSource CreateSource(string sourceName, bool loop, float volume)
        {
            GameObject sourceObject = new(sourceName, typeof(AudioSource));
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            return source;
        }
    }
}
