namespace Gameplay.Car.Model
{
    public class EngineAudioParametersModel
    {
        public float LowAccelVolume { get; }
        public float HighAccelVolume { get; }
        public float LowDecelVolume { get; }
        public float HighDecelVolume { get; }
        
        public float LowPitch { get; }
        public float HighPitch { get; }
        
        public EngineAudioParametersModel(float lowAccelVolume, float highAccelVolume, float lowDecelVolume, float highDecelVolume, float lowPitch, float highPitch)
        {
            LowAccelVolume = lowAccelVolume;
            HighAccelVolume = highAccelVolume;
            LowDecelVolume = lowDecelVolume;
            HighDecelVolume = highDecelVolume;
            LowPitch = lowPitch;
            HighPitch = highPitch;
        }
    }
}