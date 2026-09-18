using System;
using UnityEngine;

namespace Gameplay.SaveLoad
{
    [Serializable]
    public struct CarSaveData
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        // без флага старый сейв (где поля нет) прочитался бы как "двигатель мёртв"
        [SerializeField] private bool _hasDamage;
        [SerializeField] private float _engineHealth;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
        public bool HasDamage => _hasDamage;
        public float EngineHealth => _engineHealth;

        public CarSaveData(Vector3 position, Quaternion rotation, float engineHealth)
        {
            _position = position;
            _rotation = rotation;
            _hasDamage = true;
            _engineHealth = engineHealth;
        }
    }
}
