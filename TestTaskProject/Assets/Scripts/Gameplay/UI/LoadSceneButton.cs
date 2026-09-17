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
        public bool ContinuesFromSave => _continuesFromSave;

        public event Action<LoadSceneButton> Clicked;

        [SerializeField] private string _sceneName;
        [SerializeField] private string _loadingDescription;
        [Tooltip("Кнопка 'Продолжить': загружает сцену и применяет сохранение")]
        [SerializeField] private bool _continuesFromSave;

        private Button _button;

        private Button Button => _button ??= GetComponent<Button>();

        public void SetInteractable(bool interactable) => Button.interactable = interactable;

        private void OnEnable()
        {
            Button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            Button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick() => Clicked?.Invoke(this);
    }
}
