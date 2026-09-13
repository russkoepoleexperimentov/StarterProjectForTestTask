using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Car.View
{
    public class CarUIView : MonoBehaviour 
    {
        [SerializeField] private TMP_Text _gearText;
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _rpmText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input indicators")]
        [SerializeField] private Slider _throttleSlider;
        [SerializeField] private Slider _brakeSlider;
        [SerializeField] private Slider _steerSlider;
        [SerializeField] private Slider _clutchSlider;

        private void Awake()
        {
            SetupIndicator(_throttleSlider, 0f, 1f);
            SetupIndicator(_brakeSlider, 0f, 1f);
            SetupIndicator(_steerSlider, -1f, 1f);
            SetupIndicator(_clutchSlider, 0f, 1f);
        }

        public void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1 : 0;
        }

        public void SetGear(string gear)
        {
            _gearText.text = gear;
        }

        public void SetSpeed(int speedKph)
        {
            _speedText.text = $"kph: {speedKph}";
        }

        public void SetRpm(int rpm)
        {
            _rpmText.text = $"rpm: {rpm}";
        }

        public void SetInput(float throttle, float brake, float steering, float clutchEngagement)
        {
            SetIndicator(_throttleSlider, throttle);
            SetIndicator(_brakeSlider, brake);
            SetIndicator(_steerSlider, steering);
            SetIndicator(_clutchSlider, clutchEngagement);
        }

        private static void SetupIndicator(Slider slider, float min, float max)
        {
            if (slider == null) return;

            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.interactable = false;
        }

        private static void SetIndicator(Slider slider, float value)
        {
            if (slider == null) return;

            slider.value = value;
        }
    }
}
