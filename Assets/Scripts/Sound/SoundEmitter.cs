using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Audio;

namespace NFramework
{
    public class SoundEmitter : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Tween _fadeTween;
        private bool _isApplicationPause;

        public string Guid { get; private set; }
        public bool IsBgm { get; set; }
        public AudioClip AudioClip => _audioSource.clip;
        private Action OnStop { get; set; }

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
        
        private void OnApplicationPause(bool pauseStatus) => _isApplicationPause = pauseStatus;

        private void Update()
        {
            if (!_isApplicationPause && !AudioListener.pause && !_audioSource.isPlaying)
                Stop();
        }

        /// <summary>
        /// Only call from SoundManager
        /// </summary>
        public void Play(string guid, AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f, bool ignoreListenerPause = false, float fadeTime = 0f, Action onStop = null)
        {
            if (enabled)
            {
                NLogger.LogWarning("Sound emitter is playing", this);
                return;
            }

            Guid = guid;
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.loop = loop;
            _audioSource.ignoreListenerPause = ignoreListenerPause;
            _audioSource.pitch = pitch;
            enabled = true;

            if (fadeTime <= 0f)
            {
                _audioSource.volume = volume;
            }
            else
            {
                _audioSource.volume = 0f;
                var fadeTimeTemp = Mathf.Min(fadeTime, clip.length);
                _fadeTween = Tween.Custom(0f, volume, fadeTimeTemp, value =>
                {
                    _audioSource.volume = value;
                }, Ease.Linear, useUnscaledTime: _audioSource.ignoreListenerPause);
            }

            OnStop = onStop;
            _audioSource.Play();
        }

        public void Stop(float fadeTime, Action onCompleteStop = null)
        {
            if (fadeTime > 0f)
            {
                OnStop += onCompleteStop;
                _fadeTween.Complete();
                _fadeTween = Tween.Custom(_audioSource.volume, 0f, fadeTime, value =>
                {
                    _audioSource.volume = value;
                }, Ease.Linear, useUnscaledTime: _audioSource.ignoreListenerPause).OnComplete(Stop);
            }
            else
            {
                Stop();
            }
        }

        public void Stop()
        {
            _fadeTween.Complete();
            _audioSource.Stop();
            OnStop?.Invoke();
            OnStop = null;
            enabled = false;
            
            if (!IsBgm)
                SoundManager.ReturnSoundEmitter(this, _audioSource.clip);
            
            _audioSource.clip = null;
        }
        
        public void SetAudioMixerGroup(AudioMixerGroup mixerGroup) => _audioSource.outputAudioMixerGroup = mixerGroup;
    }
}
