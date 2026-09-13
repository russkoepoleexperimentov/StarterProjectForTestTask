using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    [RequireComponent(typeof(Button))]
    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName => _sceneName;
        public string LoadingDescription => _loadingDescription;

        public event Action<LoadSceneButton> Clicked;

        [SerializeField] private string _sceneName;
        [SerializeField] private string _loadingDescription;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick() => Clicked?.Invoke(this);
    }
}
