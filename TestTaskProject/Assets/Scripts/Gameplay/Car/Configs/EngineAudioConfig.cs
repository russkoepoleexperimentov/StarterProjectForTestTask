using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/EngineAudioConfig")]
    public class EngineAudioConfig : ScriptableObject
    {
        [field: SerializeField] public AudioClip[] Sounds { get; private set; }
        [field: SerializeField] public float FirstSoundRPM { get; private set; } = 1000;
        [field: SerializeField] public float SoundRPMStep { get; private set; } = 600;
        [field: SerializeField] [field: Range(0f, 1f)] public float MasterVolume { get; private set; } = 1f;
        
        [field: SerializeField]
        [field: Tooltip("Ширина гауссова окна относительно SoundRPMStep. Меньше = резче разделение клипов.")]
        public float WindowWidthFactor { get; private set; } = 0.75f;

        [field: SerializeField]
        [field: Tooltip("Громкость от педали газа: 0 = закрыт газ (overrun), 1 = полный газ")]
        public AnimationCurve ThrottleVolumeCurve { get; private set; } = AnimationCurve.Linear(0f, 0.4f, 1f, 1f);

        [field: SerializeField]
        [field: Tooltip("Скорость сглаживания громкости, чем больше — тем быстрее реакция")]
        public float VolumeSmoothingSpeed { get; private set; } = 12f;
    }
}