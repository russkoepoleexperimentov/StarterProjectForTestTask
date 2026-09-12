using System;
using UnityEngine;

namespace Gameplay.Car.View
{
    [RequireComponent(typeof(WheelCollider))]
    public class CarWheelView : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        
        public WheelCollider Collider => _wheelCollider;
        
        private WheelCollider _wheelCollider;

        private void Awake()
        {
            _wheelCollider = GetComponent<WheelCollider>();
        }

        public void ApplyVisual()
        {
            _wheelCollider.GetWorldPose(out var position, out var rotation);
            _visual.SetPositionAndRotation(position, rotation);
        }
    }
}