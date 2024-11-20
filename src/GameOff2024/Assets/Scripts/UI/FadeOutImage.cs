using UnityEngine;
using UnityEngine.UI;

public class FadeOutImage : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float fadeSeconds = 2.8f;
    [SerializeField] private float fadeDelay = 7.2f;

    private float currentTime;
    private float fadeTime;
    private float startAlpha;
    private bool isFading;

    private void OnEnable()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target image not assigned on FadeInImage component", this);
            enabled = false;
            return;
        }

        startAlpha = targetImage.color.a;
        currentTime = 0f;
        fadeTime = 0f;
        isFading = false;
        targetImage.enabled = true;
    }

    private void Update()
    {
        currentTime += Time.unscaledDeltaTime;
        
        if (!isFading)
        {
            if (currentTime >= fadeDelay)
            {
                isFading = true;
            }
            return;
        }

        fadeTime += Time.unscaledDeltaTime;
        if (fadeTime < fadeSeconds)
        {
            float normalizedTime = fadeTime / fadeSeconds;
            float currentAlpha = Mathf.Lerp(startAlpha, 0f, normalizedTime);
            
            Color color = targetImage.color;
            color.a = currentAlpha;
            targetImage.color = color;

            if (fadeTime >= fadeSeconds)
            {
                enabled = false;
            }
        }
    }
}