using System;
using Gameplay.Car.Configs;
using UnityEngine;

namespace Gameplay.Car.Model
{
    public class CarAudioModel
    {
        private readonly EngineAudioConfig _audio;
        private readonly CarWheelEffectsConfig _wheelEffectsConfig;
        
        private readonly int _clipCount;
        private readonly float[] _smoothedVolumes;

        private bool _handbrakePulled;
        
        private const float VOLUME_THRESHOLD = 0.01f;

        public CarAudioModel(EngineConfig engineConfig,
            CarWheelEffectsConfig wheelEffectsConfig)
        {
            _audio = engineConfig.AudioConfig;
            _clipCount = _audio.Sounds.Length;
            _wheelEffectsConfig = wheelEffectsConfig;
            
            _smoothedVolumes = new float[_clipCount];
        }

        public bool ConsumeHandbrakePull(float handbrake)
        {
            var pulled = handbrake >= _wheelEffectsConfig.HandbrakePullThreshold;
            var isPull = pulled && !_handbrakePulled;

            _handbrakePulled = pulled;

            return isPull;
        }

        public void UpdateClips(float engineRpm, float throttle, float deltaTime,
            float[] pitchesOut, float[] volumesOut, bool[] activeOut)
        {
            if (_audio.SoundRPMStep <= 0f)
                throw new InvalidOperationException("SoundRPMStep must be > 0");

            float windowWidth = _audio.SoundRPMStep * _audio.WindowWidthFactor;

            // 1. Считаем сырые (ненормированные) веса по гауссиане для каждого клипа
            Span<float> rawWeights = stackalloc float[_clipCount];
            float weightSum = 0f;

            for (int i = 0; i < _clipCount; i++)
            {
                float preferredRpm = _audio.FirstSoundRPM + _audio.SoundRPMStep * i;
                float diff = engineRpm - preferredRpm;

                // Гауссово окно: плавный спад без резких границ
                float w = Mathf.Exp(-(diff * diff) / (2f * windowWidth * windowWidth));
                rawWeights[i] = w;
                weightSum += w;
            }

            // Защита от деления на 0 (все веса ~0, RPM далеко за пределами диапазона клипов)
            if (weightSum < 1e-6f)
                weightSum = 1e-6f;

            // 2. Модуляция от газа: реалистичная громкость при overrun / полном газе
            float throttleGain = _audio.ThrottleVolumeCurve.Evaluate(Mathf.Clamp01(throttle));

            for (int i = 0; i < _clipCount; i++)
            {
                // 3. Нормализация -> сумма вкладов всегда ~1, что убирает резонансный
                //    суммарный всплеск при наложении соседних клипов
                float normalizedWeight = rawWeights[i] / weightSum;

                // Equal-power преобразование для более ровного восприятия микса
                float targetVolume = Mathf.Sqrt(normalizedWeight) * _audio.MasterVolume * throttleGain;

                // 4. Сглаживание во времени, чтобы избежать резких скачков/дребезга
                _smoothedVolumes[i] = Mathf.Lerp(_smoothedVolumes[i], targetVolume,
                    1f - Mathf.Exp(-_audio.VolumeSmoothingSpeed * deltaTime));

                float preferredRpm = _audio.FirstSoundRPM + _audio.SoundRPMStep * i;
                pitchesOut[i] = preferredRpm > 0f
                    ? Mathf.Clamp(engineRpm / preferredRpm, 0.5f, 2f)
                    : 1f;

                volumesOut[i] = _smoothedVolumes[i];
                activeOut[i] = _smoothedVolumes[i] > VOLUME_THRESHOLD && engineRpm > 100;
            }
        }
    }
}
