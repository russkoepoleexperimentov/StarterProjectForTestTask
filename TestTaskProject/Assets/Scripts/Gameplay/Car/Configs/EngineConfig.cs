using Gameplay.Car.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "Car/EngineConfig")]
public class EngineConfig : ScriptableObject
{
    [field: SerializeField] public float MaxTorqueNm { get; private set; } = 664;
    [field: SerializeField] public float MaxTorqueRPM { get; private set; } = 4000;
    [field: SerializeField] public float MaxPowerWatts { get; private set; } = 317000;
    [field: SerializeField] public float MaxPowerRPM { get; private set; } = 5000;

    [field: SerializeField] public float IdleRPM            { get; private set; } = 800f;
    [field: SerializeField] public float RedlineRPM         { get; private set; } = 6500f;
    [field: SerializeField] public float IdleTorqueFraction { get; private set; } = 0.35f;
    
    [field: SerializeField] public float InertiaKgM { get; private set; } = 0.3f;
    [field: SerializeField] public float BaseFriction { get; private set; } = 25;
    [field: SerializeField] public float RPMFriction { get; private set; } = 0.02f;

    [field: SerializeField] public EngineAudioConfig AudioConfig { get; private set; }
}
