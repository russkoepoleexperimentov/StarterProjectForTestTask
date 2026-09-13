using TMPro;
using UnityEngine;

namespace Gameplay.Car.View
{
    public class CarUIView : MonoBehaviour 
    {
        [SerializeField] private TMP_Text _gearText;
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _rpmText;
        [SerializeField] private CanvasGroup _canvasGroup;

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
    }
}
