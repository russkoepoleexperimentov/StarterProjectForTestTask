using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.View
{
    public class CarAudioView : MonoBehaviour
    {
        private CarAudioConfig _audioConfig;

        private AudioSource _accelLowSource;
        private AudioSource _decelLowSource;

        private AudioSource _accelHighSource;
        private AudioSource _decelHighSource;
        
        [Inject]
        public void Init(CarAudioConfig audioConfig)
        {
            _audioConfig = audioConfig;

            _accelLowSource = CreateAudioSource(_audioConfig.AccelerationLow);
            _decelLowSource = CreateAudioSource(_audioConfig.DecelerationLow);
            _accelHighSource = CreateAudioSource(_audioConfig.AccelerationHigh);
            _decelHighSource = CreateAudioSource(_audioConfig.DecelerationHigh);
        }

        public void ApplyEngineSound(EngineAudioParametersModel model)
        {
            _accelLowSource.volume = model.LowAccelVolume;
            _decelLowSource.volume = model.LowDecelVolume;
            _accelHighSource.volume = model.HighAccelVolume;
            _decelHighSource.volume = model.HighDecelVolume;
            
            _accelLowSource.pitch = _decelLowSource.pitch = model.LowPitch;
            _accelHighSource.pitch = _decelHighSource.pitch = model.HighPitch;
        }

        private AudioSource CreateAudioSource(AudioClip clip)
        {
            var source = gameObject.AddComponent<AudioSource>();
            
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 1f;
            source.volume = 0f;
            source.Play();
                
            return source;
        }
    }
}