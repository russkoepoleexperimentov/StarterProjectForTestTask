using System.Linq;
using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.View
{
    public class CarAudioView : MonoBehaviour
    {
        private EngineConfig _engineConfig;
        private CarWheelEffectsConfig _wheelEffectsConfig;
        private CarDamageConfig _damageConfig;

        private AudioSource[] _engineSources;

        private AudioSource _skidSource;
        private AudioSource _steamSource;
        private AudioSource _oneShotSource;

        private CarView _view;
        
        [Inject]
        public void Init(EngineConfig engineConfig, CarWheelEffectsConfig wheelEffectsConfig,
            CarDamageConfig damageConfig, CarView view)
        {
            _engineConfig = engineConfig;
            _wheelEffectsConfig = wheelEffectsConfig;
            _damageConfig = damageConfig;

            var audio = _engineConfig.AudioConfig;
            
            _engineSources = new AudioSource[audio.Sounds.Length];
            for (var i = 0; i < audio.Sounds.Length; i++)
            {
                _engineSources[i] = CreateAudioSource(audio.Sounds[i]);
            }

            _skidSource = CreateAudioSource(_wheelEffectsConfig.SkidmarksClip);
            _steamSource = CreateAudioSource(_damageConfig.SteamClip);
            _oneShotSource = CreateOneShotAudioSource();

            _view = view;
        }

        public void ApplyEngineClipConfiguration(float[] pitches, float[] volumes, bool[] actives)
        {
            for (var i = 0; i < _engineSources.Length; i++)
            {
                var source = _engineSources[i];
                
                source.enabled = actives[i];

                if (source.enabled)
                {
                    if (!source.isPlaying)
                    {
                        source.Play();
                    }
                    
                    source.volume = volumes[i];
                    source.pitch = pitches[i];
                }
                else
                {
                    if (source.isPlaying)
                    {
                        source.Stop();
                    }
                }
            }
        }

        public void ApplySkidmarksSound()
        {
            var slip = _view.Axles.Max(axle => Mathf.Max(axle.LeftWheel.GetSlipVelocity(), 
                axle.RightWheel.GetSlipVelocity()));

            _skidSource.pitch =
                Mathf.Clamp(
                    slip * slip * _wheelEffectsConfig.SkidmarksPitchMultiplier, 0,
                    _wheelEffectsConfig.SkidmarksPitchClamp);
            _skidSource.volume = Mathf.Clamp01(slip * _wheelEffectsConfig.SkidmarksVolumeMultiplier);
            if (_skidSource.volume < _wheelEffectsConfig.SkidmarksVolumeThreshold)
                _skidSource.volume = 0;
        }

        public void PlayHandbrakePull()
        {
            var clip = _wheelEffectsConfig.HandbrakeClip;

            if (clip == null) return;

            _oneShotSource.PlayOneShot(clip);
        }

        // дым идёт - шипит пар, и тем громче, чем его больше
        public void ApplySteamSound(bool active, float intensity01)
        {
            if (_steamSource.clip == null) return;

            _steamSource.volume = active
                ? Mathf.Lerp(_damageConfig.SteamMinVolume, _damageConfig.SteamMaxVolume,
                    Mathf.Clamp01(intensity01))
                : 0f;
        }

        public void PlayImpact(bool strong, float volume)
        {
            var clips = strong ? _damageConfig.StrongImpactClips : _damageConfig.WeakImpactClips;

            if (clips == null || clips.Length == 0) return;

            PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null) return;

            _oneShotSource.PlayOneShot(clip, volume);
        }

        private AudioSource CreateOneShotAudioSource()
        {
            var source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.volume = 1f;
            source.dopplerLevel = 0f;

            return source;
        }

        private AudioSource CreateAudioSource(AudioClip clip)
        {
            var source = gameObject.AddComponent<AudioSource>();
            
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 1f;
            source.volume = 0f;
            source.dopplerLevel = 0f; // чтобы звук не шакалился при тряске камеры 
            source.Play();
                
            return source;
        }
    }
}
