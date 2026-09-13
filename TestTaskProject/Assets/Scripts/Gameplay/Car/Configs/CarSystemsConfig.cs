using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/CarSystemsConfig")]
    public class CarSystemsConfig : ScriptableObject
    {
        [field: SerializeField] public float MaxBrakeTorque { get; set; } = 3000;
        [field: SerializeField] public float MaxHandBrakeTorque { get; set; } = 1500;
        [field: SerializeField] public float MaxSteerAngle { get; set; } = 30f;

        [Header("Steering input filter")]
        // скорость, на которой чувствительность руля падает до минимума
        [field: SerializeField] public float SteerSpeedFalloffKph { get; set; } = 60f;
        // насколько вяло руль работает на максимальной скорости
        [field: SerializeField] public float MinSteerFactor { get; set; } = 0.3f;
        // скорость доворота руля, единиц ввода в секунду
        [field: SerializeField] public float SteerAttackRate { get; set; } = 1f;
        // скорость возврата руля в ноль
        [field: SerializeField] public float SteerReleaseRate { get; set; } = 3f;

        [Header("Brake input filter")]
        // скорость нажатия педали тормоза, единиц ввода в секунду
        [field: SerializeField] public float BrakeRiseRate { get; set; } = 4f;
        // скорость отпускания педали тормоза
        [field: SerializeField] public float BrakeFallRate { get; set; } = 6f;
        // подтормаживание накатом, пока игрок не дал газ
        [field: SerializeField] public float PassiveBrakeAmount { get; set; } = 0.2f;
        // порог, после которого педаль считается нажатой
        [field: SerializeField] public float PedalThreshold { get; set; } = 0.2f;
    }
}
