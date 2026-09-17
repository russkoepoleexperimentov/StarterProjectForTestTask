using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/DriverConfig")]
    public class DriverConfig : ScriptableObject
    {
        [field: Header("Gearbox")]
        // автомат сам выбирает передачу; выключенный - водитель переключается сам
        [field: SerializeField] public bool EnableAutoGearbox { get; private set; } = true;
        [field: SerializeField] public float ShiftDownRPM { get; private set; } = 1000;
        [field: SerializeField] public float ShiftUpRPM { get; private set; } = 2500;

        [field: Header("Clutch")]
        // ассист сам работает сцеплением: трогание, анти-заглушание, выжим на переключении
        [field: SerializeField] public bool EnableClutchAssist { get; private set; } = true;
        // время полного схватывания сцепления после включения передачи
        [field: SerializeField] public float ClutchEngageTimeSeconds { get; private set; } = 1.2f;
        // скорость, на которой сцепление на первой/задней перестаёт буксовать
        [field: SerializeField] public float ClutchLockSpeedKph { get; private set; } = 15f;
        // минимальная доля момента, которую передаёт буксующее сцепление (иначе не тронуться)
        [field: SerializeField] public float MinClutchEngagement { get; private set; } = 0.15f;

        [field: Header("Throttle")]
        // срезает газ ближе к отсечке, чтобы разгон с клавиатуры был управляемым
        [field: SerializeField] public bool EnableThrottleAssist { get; private set; } = true;

        [field: Header("Steering")]
        // руль слабеет со скоростью и доворачивается плавно
        [field: SerializeField] public bool EnableSteeringAssist { get; private set; } = true;
        [field: SerializeField] public float SteerSpeedFalloffKph { get; private set; } = 60f;
        [field: SerializeField] public float MinSteerFactor { get; private set; } = 0.3f;
        [field: SerializeField] public float SteerAttackRate { get; private set; } = 1f;
        [field: SerializeField] public float SteerReleaseRate { get; private set; } = 3f;

        [field: Header("Brakes")]
        // педаль тормоза набирается и отпускается с ограниченной скоростью
        [field: SerializeField] public bool EnableBrakeAssist { get; private set; } = true;
        [field: SerializeField] public float BrakeRiseRate { get; private set; } = 4f;
        [field: SerializeField] public float BrakeFallRate { get; private set; } = 6f;

        // после торможения машина продолжает придерживаться, пока не дать газ
        [field: SerializeField] public bool EnablePassiveBraking { get; private set; } = true;
        [field: SerializeField] public float PassiveBrakeAmount { get; private set; } = 0.2f;

        // с какого значения педаль считается нажатой
        [field: SerializeField] public float PedalThreshold { get; private set; } = 0.2f;
    }
}
