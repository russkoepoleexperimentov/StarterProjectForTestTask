using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/GearboxConfig")]
    public class GearboxConfig : ScriptableObject
    {
        [field: Header("Gear ratios")]
        [field: SerializeField] public float[] BackwardGearRatios { get; private set; } = new float[] { -3.66f };
        [field: SerializeField] public float[] ForwardGearRatios { get; private set; } = new float[] { 3.66f, 2.5f, 1.73f, 1 };
        [field: SerializeField] public float FinalDriveRatio { get; private set; } = 3.23f;

        [field: Header("Shift timings")]
        [field: SerializeField] public float ShiftTimeSeconds { get; private set; } = 0.3f;
        [field: SerializeField] public float MinTimeInGearSeconds { get; private set; } = 1f;
    }
}
