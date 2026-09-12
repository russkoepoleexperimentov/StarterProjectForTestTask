using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/Car systems config")]
    public class CarSystemsConfig : ScriptableObject
    {
        [field: SerializeField] public float MaxBrakeTorque { get; set; } = 3000;
        [field: SerializeField] public float MaxHandBrakeTorque { get; set; } = 1500;
        [field: SerializeField] public float MaxSteerAngle { get; set; } = 30f;
    }
}