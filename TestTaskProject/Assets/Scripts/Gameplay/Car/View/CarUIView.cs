using Gameplay.Car.Presenter;
using Gameplay.Car.Services;
using Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.Car.View
{
    public class CarUIView : ScreenView
    {
        [Header("Drive UI")]
        [SerializeField] private TMP_Text _gearText;
        [SerializeField] private NeedleView _speedNeedle;
        [SerializeField] private NeedleView _rpmNeedle;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input indicators")]
        [SerializeField] private Slider _throttleSlider;
        [SerializeField] private Slider _brakeSlider;
        [SerializeField] private Slider _steerSlider;
        [SerializeField] private Slider _clutchSlider;

        [Inject] private PlayerCarRegistry _registry;

        public override ScreenController Construct(EventManager eventManager)
        {
            return new CarUIScreenController(this, eventManager, _registry);
        }

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

        public void SetSpeed(int speedKph) => _speedNeedle.DisplayValue(speedKph);

        public void SetRpm(int rpm) => _rpmNeedle.DisplayValue(rpm);

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
