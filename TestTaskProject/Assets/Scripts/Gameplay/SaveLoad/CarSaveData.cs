using System;
using UnityEngine;

namespace Gameplay.SaveLoad
{
    [Serializable]
    public struct CarSaveData
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;

        public CarSaveData(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
        }
    }
}
