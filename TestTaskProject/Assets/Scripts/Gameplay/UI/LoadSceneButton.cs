using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    [SerializeField] private string _loadingDescription;

    private Button _button;

    [Inject]
    private LoadingService _service;

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

    private void HandleClick() 
    {
        _service.AppendOperation(new SceneLoadingOperation(_sceneName, _loadingDescription));
    }
}
