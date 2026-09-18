using UnityEngine;

namespace Gameplay.Car.Model
{
    // сырые данные удара, как их отдаёт физика
    public readonly struct CarImpactModel
    {
        public readonly float Impulse;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;

        public CarImpactModel(float impulse, Vector3 point, Vector3 normal)
        {
            Impulse = impulse;
            Point = point;
            Normal = normal;
        }
    }

    // удар, прошедший фильтрацию модели: уже известно, сильный он и насколько
    public readonly struct CarImpactResult
    {
        public readonly bool IsStrong;
        public readonly float Force01;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;

        public CarImpactResult(bool isStrong, float force01, Vector3 point, Vector3 normal)
        {
            IsStrong = isStrong;
            Force01 = force01;
            Point = point;
            Normal = normal;
        }
    }
}
