using System;

public class CarStateModel {
    public float RPM { get; private set; }
    public float SpeedKph { get; private set; }
    public int GearIndex { get; private set; }

    public event Action<CarStateModel> Changed;

    public void Set(float rpm, float speedKph, int gearIndex)
    {
        if(RPM == rpm && SpeedKph == speedKph && GearIndex == gearIndex) return;

        RPM = rpm;
        SpeedKph = speedKph;
        GearIndex = gearIndex;
        Changed?.Invoke(this);
    }
}
