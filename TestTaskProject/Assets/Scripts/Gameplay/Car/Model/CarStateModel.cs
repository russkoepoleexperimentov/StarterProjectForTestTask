using System;

namespace Gameplay.Car.Model
{
    public class CarStateModel {
        public float RPM { get; private set; }
        public float SpeedKph { get; private set; }
        public int GearIndex { get; private set; }

        public float Throttle { get; private set; }
        public float Brake { get; private set; }
        public float Steering { get; private set; }
        public float Handbrake { get; private set; }
        public float ClutchEngagement { get; private set; }

        public event Action<CarStateModel> Changed;

        public void Set(float rpm, float speedKph, int gearIndex,
            float throttle, float brake, float steering, float handbrake, float clutchEngagement)
        {
            RPM = rpm;
            SpeedKph = speedKph;
            GearIndex = gearIndex;
            Throttle = throttle;
            Brake = brake;
            Steering = steering;
            Handbrake = handbrake;
            ClutchEngagement = clutchEngagement;
            Changed?.Invoke(this);
        }
    }
}
