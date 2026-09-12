using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/Gearbox Usage Config")]
    public class GearboxUsageConfig : ScriptableObject
    {
        [field: SerializeField] public float ShiftTimeSeconds { get; private set; } = 0.3f;
        [field: SerializeField] public float MinTimeInGearSeconds { get; private set; } = 1f;
        
        [field: SerializeField] public float ShiftDownRPM { get; private set; } = 1000;
        [field: SerializeField] public float ShiftUpRPM { get; private set; } = 2500;
    }
}