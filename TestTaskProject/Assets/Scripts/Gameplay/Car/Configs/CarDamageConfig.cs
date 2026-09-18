using UnityEngine;

namespace Gameplay.Car.Configs
{
    [CreateAssetMenu(menuName = "Car/CarDamageConfig")]
    public class CarDamageConfig : ScriptableObject
    {
        [field: Header("Health")]
        [field: SerializeField] public float MaxEngineHealth { get; private set; } = 100f;
        // удары слабее порога игнорируем: обычная езда и притирания к бордюрам не должны жечь двигатель
        [field: SerializeField] public float MinImpactImpulse { get; private set; } = 2000f;
        [field: SerializeField] public float DamagePerImpulse { get; private set; } = 0.004f;
        [field: SerializeField] public float MaxDamagePerImpact { get; private set; } = 35f;
        // OnCollisionEnter при скольжении вдоль стены срабатывает пачками - режем частоту
        [field: SerializeField] public float ImpactCooldown { get; private set; } = 0.15f;

        [field: Header("Power")]
        [field: SerializeField] public float MinPowerMultiplier { get; private set; } = 0.35f;
        // вход - здоровье [0..1], выход - доля хода от MinPowerMultiplier до 1
        [field: SerializeField] public AnimationCurve PowerLossCurve { get; private set; }
            = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: Header("Smoke")]
        [field: SerializeField] public float SmokeHealthThreshold { get; private set; } = 0.3f;
        [field: SerializeField] public ParticleSystem SmokePrefab { get; private set; }
        [field: SerializeField] public float SmokeMinEmission { get; private set; } = 8f;
        [field: SerializeField] public float SmokeMaxEmission { get; private set; } = 45f;

        [field: Header("Steam audio")]
        // зацикленный шум пара, громкость пропорциональна количеству дыма
        [field: SerializeField] public AudioClip SteamClip { get; private set; }
        [field: SerializeField] public float SteamMinVolume { get; private set; } = 0.1f;
        [field: SerializeField] public float SteamMaxVolume { get; private set; } = 1f;

        [field: Header("Impact audio")]
        [field: SerializeField] public AudioClip[] WeakImpactClips { get; private set; }
        [field: SerializeField] public AudioClip[] StrongImpactClips { get; private set; }
        // импульс, с которого удар считается сильным
        [field: SerializeField] public float StrongImpactImpulse { get; private set; } = 12000f;
        [field: SerializeField] public float MinImpactVolume { get; private set; } = 0.35f;
        [field: SerializeField] public float MaxImpactVolume { get; private set; } = 1f;

        [field: Header("Deformation")]
        [field: SerializeField] public float DeformationRadius { get; private set; } = 0.6f;
        // метры смещения вершины на единицу нормированной силы удара
        [field: SerializeField] public float DeformationStrength { get; private set; } = 0.12f;
        // предел накопленного смещения вершины от исходной позиции
        [field: SerializeField] public float MaxVertexOffset { get; private set; } = 0.25f;
        // вход - нормированное расстояние до точки удара, выход - множитель смещения
        [field: SerializeField] public AnimationCurve DeformationFalloff { get; private set; }
            = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [field: SerializeField] public float DeformationNoise { get; private set; } = 0.02f;
    }
}
