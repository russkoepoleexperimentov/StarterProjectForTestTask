using UnityEngine;

[CreateAssetMenu(menuName = "Car/GearboxConfig")]
public class GearboxConfig : ScriptableObject
{
    public enum GearboxType { Manual, Automatic }

    [field: SerializeField] public float[] BackwardGearRatios { get; private set; } = new float[] { -3.66f };
    [field: SerializeField] public float[] ForwardGearRatios { get; private set; } = new float[] { 3.66f, 2.5f, 1.73f, 1 };
    [field: SerializeField] public float FinalDriveRatio = 3.23f;
    [field: SerializeField] public GearboxType Type { get; private set; } = GearboxType.Manual;
}
