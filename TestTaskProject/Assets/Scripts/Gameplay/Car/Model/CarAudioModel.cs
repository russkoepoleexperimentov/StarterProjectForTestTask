using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    public class CarAudioModel
    {
        private readonly CarAudioConfig _audioConfig;
        private readonly EngineConfig _engineConfig;

        private float _throttle;
        
        private const float THROTTLE_SMOOTH_TIME = 0.7f;

        public CarAudioModel(CarAudioConfig audioConfig, EngineConfig engineConfig)
        {
            _audioConfig = audioConfig;
            _engineConfig = engineConfig;
        }
        
        public EngineAudioParametersModel CalculateEngineAudio(float engineRpm, float throttle, float deltaTime)
        {
            _throttle = Mathf.MoveTowards(_throttle, throttle, deltaTime * THROTTLE_SMOOTH_TIME);

            var lowPitch = engineRpm / _audioConfig.LowRpmReference;
            var highPitch = engineRpm / _audioConfig.HighRpmReference;
            lowPitch = Mathf.Clamp(lowPitch, _audioConfig.MinPitch, _audioConfig.MaxPitch);
            highPitch = Mathf.Clamp(highPitch, _audioConfig.MinPitch, _audioConfig.MaxPitch);
            
            var accelFade = _throttle;
            var decelFade = 1 - _throttle;
            
            var highFade = Mathf.InverseLerp(_audioConfig.LowRpmReference, _audioConfig.HighRpmReference, engineRpm);
            var lowFade = 1 - highFade;

            accelFade = Magic(accelFade);
            decelFade = Magic(decelFade);
            highFade = Magic(highFade);
            lowFade = Magic(lowFade);

            return new EngineAudioParametersModel(
                accelFade * lowFade, 
                accelFade * highFade, 
                decelFade * lowFade, 
                decelFade * highFade,
                lowPitch, highPitch);

        }
        
        

        private float Magic(float x) => 1 - ((1 - x) * (1 - x));
    }
}