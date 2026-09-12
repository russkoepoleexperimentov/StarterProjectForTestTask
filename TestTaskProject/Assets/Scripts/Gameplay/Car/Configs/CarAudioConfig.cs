using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/CarAudioConfig")]
    public class CarAudioConfig : ScriptableObject
    {
        [field: SerializeField] public AudioClip AccelerationHigh { get; set; }
        [field: SerializeField] public AudioClip DecelerationHigh { get; set; }
        
        [field: SerializeField] public AudioClip AccelerationLow { get; set; }
        [field: SerializeField] public AudioClip DecelerationLow { get; set; }

        [field: SerializeField] public float HighRpmReference { get; set; } = 5000;
        [field: SerializeField] public float LowRpmReference { get; set; } = 1000f;
        
        [field: SerializeField] public float MinPitch { get; set; } = 0.8f;
        [field: SerializeField] public float MaxPitch { get; set; } = 1.6f;
    }
}