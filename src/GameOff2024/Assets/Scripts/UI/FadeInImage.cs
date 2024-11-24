using UnityEngine;
using UnityEngine.UI;
using System;

public class FadeInImage : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float fadeSeconds = 2.8f;
    [SerializeField] private float fadeDelay = 7.2f;
    [SerializeField] private bool startOnEnable = true;

    private float fadeTime;
    private bool isFading;
    private Action onFadeComplete;
    private float currentFadeSeconds;

    private void OnEnable()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target image not assigned on FadeInImage component", this);
            enabled = false;
            return;
        }

        fadeTime = 0f;
        isFading = false;
        currentFadeSeconds = fadeSeconds;
        targetImage.enabled = true;

        if (startOnEnable)
            this.ExecuteAfterDelay(() => isFading = true, fadeDelay);
    }

    public void StartFade(bool withDelay, Action onComplete = null, float overrideFadeSeconds = -1)
    {
        enabled = true;
        onFadeComplete = onComplete;
        fadeTime = 0f;
        currentFadeSeconds = overrideFadeSeconds > 0 ? overrideFadeSeconds : fadeSeconds;
        this.ExecuteAfterDelay(() => isFading = true, withDelay ? fadeDelay : 0);
    }

    private void Update()
    {
        if (!isFading)
        {
            return;
        }

        fadeTime += Time.unscaledDeltaTime;
        
        float normalizedTime = fadeTime / currentFadeSeconds;
        float currentAlpha = Mathf.Lerp(0f, 1f, normalizedTime);
        
        Color color = targetImage.color;
        color.a = currentAlpha;
        targetImage.color = color;

        if (fadeTime >= currentFadeSeconds)
        {
            onFadeComplete?.Invoke();
            Log.Info("Fade In - Completed");
            enabled = false;
        }
    }
}