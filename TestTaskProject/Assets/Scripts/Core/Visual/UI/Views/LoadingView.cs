using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class LoadingView : ScreenView
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _fillTime = 0.5f;
    [SerializeField] private float _fadeTime = 1f;

    [Inject] private LoadingState _state;

    private Coroutine _fillRoutine;
    private Coroutine _completionRoutine;

    public override ScreenController Construct(EventManager eventManager)
    {
        return new LoadingScreenController(this, eventManager, _state);
    }

    public void SetDescription(string description)
    {
        _descriptionText.text = description;
    }

    public void SetProgress(float progress)
    {
        StopFill();

        if (Mathf.Approximately(progress, 0))
        {
            _fillImage.fillAmount = 0;
            return;
        }

        _fillRoutine = StartCoroutine(AdjustFillAmount(progress));
    }

    public void SetOpaque()
    {
        StopCompletion();
        _canvasGroup.alpha = 1;
    }

    public void PlayCompletion(Action onFinished)
    {
        StopCompletion();
        _completionRoutine = StartCoroutine(CompletionRoutine(onFinished));
    }

    private void OnDisable()
    {
        StopFill();
        StopCompletion();
    }

    private void StopFill()
    {
        if (_fillRoutine == null) return;

        StopCoroutine(_fillRoutine);
        _fillRoutine = null;
    }

    private void StopCompletion()
    {
        if (_completionRoutine == null) return;

        StopCoroutine(_completionRoutine);
        _completionRoutine = null;
    }

    private IEnumerator AdjustFillAmount(float desired)
    {
        var start = _fillImage.fillAmount;

        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime / _fillTime)
        {
            _fillImage.fillAmount = Mathf.Lerp(start, desired, t);
            yield return null;
        }

        _fillImage.fillAmount = desired;
        _fillRoutine = null;
    }

    private IEnumerator CompletionRoutine(Action onFinished)
    {
        while (_fillRoutine != null)
            yield return null;

        while (!Mathf.Approximately(_fillImage.fillAmount, 1))
        {
            _fillImage.fillAmount = Mathf.MoveTowards(_fillImage.fillAmount, 1, Time.unscaledDeltaTime / _fillTime);
            yield return null;
        }

        _fillImage.fillAmount = 1;

        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime / _fadeTime)
        {
            _canvasGroup.alpha = 1 - t;
            yield return null;
        }

        _canvasGroup.alpha = 0;
        _completionRoutine = null;
        onFinished?.Invoke();
    }
}
