using System.Linq;
using Gameplay.Car.Configs;
using Gameplay.Car.Model;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.View
{
    public class CarAudioView : MonoBehaviour
    {
        private CarAudioConfig _audioConfig;
        private CarWheelEffectsConfig _wheelEffectsConfig;

        private AudioSource _accelLowSource;
        private AudioSource _decelLowSource;

        private AudioSource _accelHighSource;
        private AudioSource _decelHighSource;

        private AudioSource _skidSource;

        private CarView _view;
        
        [Inject]
        public void Init(CarAudioConfig audioConfig, CarWheelEffectsConfig wheelEffectsConfig, CarView view)
        {
            _audioConfig = audioConfig;
            _wheelEffectsConfig = wheelEffectsConfig;

            _accelLowSource = CreateAudioSource(_audioConfig.AccelerationLow);
            _decelLowSource = CreateAudioSource(_audioConfig.DecelerationLow);
            _accelHighSource = CreateAudioSource(_audioConfig.AccelerationHigh);
            _decelHighSource = CreateAudioSource(_audioConfig.DecelerationHigh);

            _skidSource = CreateAudioSource(_wheelEffectsConfig.SkidmarksClip);

            _view = view;
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

        public void ApplySkidmarksSound()
        {
            var slip = _view.Axles.Max(axle => Mathf.Max(axle.LeftWheel.GetSlip(), 
                axle.RightWheel.GetSlip()));
            
            var pitch = _wheelEffectsConfig.SkidmarksStepPerMeterSlip * 
                        Mathf.Max(0, slip - _wheelEffectsConfig.SkidmarksMinSlip);
            
            _skidSource.pitch = Mathf.Clamp(pitch, _wheelEffectsConfig.SkidmarksMinPitch, _wheelEffectsConfig.SkidmarksMaxPitch);
            _skidSource.volume = Mathf.Clamp01(pitch);
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