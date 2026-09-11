using System;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using static EventsProvider;

public class LoadingView : ScreenView 
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _fillTime = 0.5f;

    [Inject]
    private EventManager _eventManager;

    private Coroutine _currentAdjustFillAmountRoutine;

    private void OnEnable()
    {
        _canvasGroup.alpha = 1;
        _eventManager.Subscribe<UpdateLoadingContextEvent>(UpdateContext);
        _eventManager.Subscribe<UpdateLoadingProgressEvent>(UpdateFill);
        _eventManager.Subscribe<StartLoadingScreenFadeEvent>(StartFadeOut);
    }

    private void OnDisable()
    {
        _eventManager.Unsubscribe<UpdateLoadingContextEvent>(UpdateContext);
        _eventManager.Unsubscribe<UpdateLoadingProgressEvent>(UpdateFill);
        _eventManager.Unsubscribe<StartLoadingScreenFadeEvent>(StartFadeOut);
    }

    private void UpdateContext(UpdateLoadingContextEvent context) 
    {
        _descriptionText.text = context.Description;
    }

    private void UpdateFill(UpdateLoadingProgressEvent @event)
    {
        if(_currentAdjustFillAmountRoutine != null)
        {
            StopCoroutine(_currentAdjustFillAmountRoutine);
            _currentAdjustFillAmountRoutine = null;
        }

        if(Mathf.Approximately(@event.Progress, 0))
        {
            _fillImage.fillAmount = 0;
            return;
        }

        _currentAdjustFillAmountRoutine = StartCoroutine(AdjustFillAmount(@event.Progress));
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
    }

    private void StartFadeOut(StartLoadingScreenFadeEvent fadeEvent) => StartCoroutine(FadeOut(fadeEvent));

    private IEnumerator FadeOut(StartLoadingScreenFadeEvent fadeEvent)
    {
        float timeSeconds = fadeEvent.TimeMs / 1000f;

        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime / timeSeconds)
        {
            if(_canvasGroup)
                _canvasGroup.alpha = 1 - t;
            yield return null;
        }
    }
}