using Core.Visual.UI;
using UnityEngine;

namespace Gameplay.UI
{
    public class NeedleView : FloatDisplayView
    {
        [SerializeField] private float _minAngle = 0;
        [SerializeField] private float _maxAngle = 270;
        [SerializeField] private float _minRange = 0;
        [SerializeField] private float _maxRange = 180;

        [SerializeField] private float _needleAdjustSpeed = 2f; // deg per second
        
        private float _accumulatedAngle;
        private float _displayedValue;

        public override float Displayed => _displayedValue;

        public override void DisplayValue(float value)
        {
            _displayedValue = value;
            var t = Mathf.InverseLerp(_minRange, _maxRange, value);
            var angle = Mathf.LerpUnclamped(_minAngle, _maxAngle, t);

            if (_needleAdjustSpeed > 0)
            {
                _accumulatedAngle = Mathf.MoveTowards(_accumulatedAngle, angle, Time.deltaTime * _needleAdjustSpeed);
            }
            else
            {
                _accumulatedAngle = angle;
            }
            
            transform.localRotation = Quaternion.Euler(0, 0, _accumulatedAngle);
        }
    }
}
