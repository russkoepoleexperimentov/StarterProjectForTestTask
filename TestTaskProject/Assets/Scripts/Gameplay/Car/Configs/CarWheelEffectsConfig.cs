using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/CarWheelEffectsConfig")]
    public class CarWheelEffectsConfig : ScriptableObject
    {
        [field: SerializeField] public AudioClip SkidmarksClip { get; set; }
        [field: SerializeField] public float SkidmarksMinPitch { get; set; } = 0.8f;
        [field: SerializeField] public float SkidmarksMaxPitch { get; set; } = 2f;
        [field: SerializeField] public float SkidmarksMinSlip { get; set; } = 0.4f;
        [field: SerializeField] public float SkidmarksStepPerMeterSlip { get; set; } = 1f;
    }
}
