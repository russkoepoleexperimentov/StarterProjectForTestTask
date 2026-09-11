using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

public class SceneLoadingOperation : ILoadingOperation
{
    private readonly string _sceneName;
    private readonly string _description;

    /* 
    при 0.9 сцена полностью загружена и готова к активации
    при 1 - сцена уже активна
    поэтому делаем небольшой хак (см. Run())
    */
    private const float SCENE_READY_PROGRESS = 0.9f;
    private const int PROGRESS_UPDATE_DELAY_MS = 10;

    public SceneLoadingOperation(string sceneName, string description = "Загрузка игры")
    {
        _sceneName = sceneName;
        _description = description;
    }

    public async Task Run(Action<float> updateProgress, Action<string> updateContext)
    {
        updateContext(_description);
        updateProgress(0);

        var loadOperation = SceneManager.LoadSceneAsync(_sceneName);
        loadOperation.allowSceneActivation = false;

        while(loadOperation.progress < SCENE_READY_PROGRESS) {
            updateProgress(loadOperation.progress / SCENE_READY_PROGRESS);
            await Task.Delay(PROGRESS_UPDATE_DELAY_MS);
        }

        updateProgress(1);
        loadOperation.allowSceneActivation = true;
    }
}
