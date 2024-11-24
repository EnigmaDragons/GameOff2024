using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class MusicManagerScript : OnMessage<FadeOutMusic, FadeInMusic>
{
    Rigidbody rb;
    bool firstMove;
    public EventReference mxEvent;
    EventInstance mxEventInstance;
    
    [SerializeField] private float fadeTime = 2f;
    private float currentVolume = 1f;
    private float targetVolume = 1f;
    private float fadeTimer = 0f;
    private bool isFading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mxEventInstance = RuntimeManager.CreateInstance(mxEvent);
        mxEventInstance.start();
    }

    void OnDestroy()
    {
        mxEventInstance.release();
    }

    void FirstMovement()
    {
        if (mxEventInstance.isValid())
        {
            mxEventInstance.setParameterByNameWithLabel("MusicControl", "SectionA");
            firstMove = true;
        }
    }

    void MovementUpdateRunning(float moveValue)
    {
        mxEventInstance.setParameterByName("MX_Running", Mathf.Lerp(0f, 1f, moveValue));
        if (moveValue >= 1f)
        {
            FirstMovement();
        }
    }

    void Update()
    {
        if (!firstMove) 
        {
            if (rb != null)
            {
                if (rb.linearVelocity.magnitude >= 0.1f)
                {
                    MovementUpdateRunning(rb.linearVelocity.magnitude);
                }
            }
        }

        if (isFading && mxEventInstance.isValid())
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeTime);
            currentVolume = Mathf.Lerp(currentVolume, targetVolume, t);
            mxEventInstance.setVolume(currentVolume);

            if (fadeTimer >= fadeTime)
            {
                isFading = false;
            }
        }
    }

    float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float retFloat = Mathf.Clamp(value, 0.1f, 9f);
        return toMin + (retFloat - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    protected override void Execute(FadeOutMusic msg)
    {
        if (mxEventInstance.isValid())
        {
            targetVolume = 0f;
            fadeTimer = 0f;
            isFading = true;
        }
    }

    protected override void Execute(FadeInMusic msg)
    {
        if (mxEventInstance.isValid())
        {
            targetVolume = 1f;
            fadeTimer = 0f;
            isFading = true;
        }
    }
}
