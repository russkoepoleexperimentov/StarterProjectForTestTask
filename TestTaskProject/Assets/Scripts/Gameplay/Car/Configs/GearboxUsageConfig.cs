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

        [Header("Clutch")]
        // время полного схватывания сцепления после включения передачи
        [field: SerializeField] public float ClutchEngageTimeSeconds { get; private set; } = 1.2f;
        // скорость, на которой сцепление на первой/задней перестаёт буксовать
        [field: SerializeField] public float ClutchLockSpeedKph { get; private set; } = 15f;
        // минимальная доля момента, которую передаёт буксующее сцепление (иначе не тронуться)
        [field: SerializeField] public float MinClutchEngagement { get; private set; } = 0.15f;
        // ограничение момента на колёсах, пока сцепление буксует
        [field: SerializeField] public float MaxLaunchTorqueNm { get; private set; } = 2500f;
    }
}