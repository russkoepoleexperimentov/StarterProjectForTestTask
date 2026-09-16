using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/CarWheelEffectsConfig")]
    public class CarWheelEffectsConfig : ScriptableObject
    {
        [field: SerializeField] public AudioClip SkidmarksClip { get; set; }
        [field: SerializeField] public float SkidmarksPitchMultiplier { get; set; } = 0.02f;
        [field: SerializeField] public float SkidmarksPitchClamp { get; set; } = 1.6f;
        [field: SerializeField] public float SkidmarksVolumeMultiplier { get; set; } = 0.00875f;
        [field: SerializeField] public float SkidmarksVolumeThreshold { get; set; } = 0.05f;

        [Header("Handbrake")]
        // одноразовый звук затяжки рычага
        [field: SerializeField] public AudioClip HandbrakeClip { get; set; }
        [field: SerializeField] public float HandbrakePullThreshold { get; set; } = 0.5f;
    }
}
