using System;
using Gameplay.Car.Model;
using UnityEngine;

namespace Gameplay.Car.View
{
    // висит на объекте с Rigidbody (коллизионные колбэки приходят именно туда, а не на
    // дочерний коллайдер). Никакой фильтрации здесь нет - это забота CarDamageModel.
    public class CarCollisionView : MonoBehaviour
    {
        public event Action<CarImpactModel> Impacted;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contactCount == 0) return;

            var point = Vector3.zero;
            for (var i = 0; i < collision.contactCount; i++)
                point += collision.GetContact(i).point;

            point /= collision.contactCount;

            // Normal - нормаль поверхности, направленная в сторону машины;
            // вминать металл нужно против неё
            var normal = collision.GetContact(0).normal;

            Impacted?.Invoke(new CarImpactModel(collision.impulse.magnitude, point, normal));
        }
    }
}
