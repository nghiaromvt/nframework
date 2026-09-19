using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Audio;

namespace NFramework.Sound
{
    public class SoundEmitter : MonoBehaviour, IPoolable, IUpdatable
    {
        [SerializeField] private AudioSource _audioSource;
        
        private Tween _fadeTween;
        private bool _isApplicationPause;
        private bool _isPlaying;

        public int Handle { get; private set; }
        public bool IsBgm { get; set; }
        public AudioClip AudioClip => _audioSource.clip;
        private Action OnStop { get; set; }

        private void OnApplicationPause(bool pauseStatus) => _isApplicationPause = pauseStatus;

        public void OnSpawn()
        {
            _isPlaying = false;
            Handle = SoundManager.INVALID_HANDLE;
            OnStop = null;
        }

        public void OnDespawn()
        {
            if (_isPlaying)
            {
                GameLoop.Unregister(this);
                _isPlaying = false;
            }

            _fadeTween.Stop();
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }

            OnStop = null;
            Handle = SoundManager.INVALID_HANDLE;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_isApplicationPause && !AudioListener.pause && !_audioSource.isPlaying)
                Stop();
        }

        /// <summary>
        /// Only call from SoundManager
        /// </summary>
        public void Play(int handle, AudioClip clip, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (_isPlaying)
            {
                NLogger.LogError("Sound emitter is already playing", this);
                return;
            }

            Handle = handle;
            _audioSource.clip = clip;
            _audioSource.loop = playSettings.loop;
            _audioSource.ignoreListenerPause = playSettings.ignorePause;
            _audioSource.pitch = playSettings.pitch;
            _isPlaying = true;
            GameLoop.Register(this);

            if (playSettings.fadeTime <= 0f)
            {
                _audioSource.volume = playSettings.volume;
            }
            else
            {
                _audioSource.volume = 0f;
                var fadeTimeTemp = Mathf.Min(playSettings.fadeTime, clip.length);
                _fadeTween = Tween.Custom(0f, playSettings.volume, fadeTimeTemp, value =>
                {
                    _audioSource.volume = value;
                }, Ease.Linear, useUnscaledTime: _audioSource.ignoreListenerPause);
            }

            OnStop = onStop;
            _audioSource.Play();
        }

        public void Stop(float fadeTime, Action onCompleteStop = null)
        {
            if (!_isPlaying) return;

            OnStop += onCompleteStop;
            if (fadeTime <= 0f)
            {
                Stop();
                return;
            }

            _fadeTween.Stop();
            _fadeTween = Tween.Custom(_audioSource.volume, 0f, fadeTime, value =>
            {
                _audioSource.volume = value;
            }, Ease.Linear, useUnscaledTime: _audioSource.ignoreListenerPause).OnComplete(Stop);
        }

        public void Stop()
        {
            if (!_isPlaying) return;

            // Stop (not Complete): completing a fade-out tween would re-enter Stop via OnComplete.
            _fadeTween.Stop();
            _audioSource.Stop();
            GameLoop.Unregister(this);
            _isPlaying = false;

            var onStop = OnStop;
            OnStop = null;

            SoundManager.ReturnSoundEmitter(this, _audioSource.clip);

            _audioSource.clip = null;
            onStop?.Invoke();
        }

        public void SetAudioMixerGroup(AudioMixerGroup mixerGroup) => _audioSource.outputAudioMixerGroup = mixerGroup;
    }
}

