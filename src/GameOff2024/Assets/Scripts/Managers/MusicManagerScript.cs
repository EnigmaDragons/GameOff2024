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
            mxEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    protected override void Execute(FadeInMusic msg)
    {
        if (mxEventInstance.isValid())
        {
            mxEventInstance.start();
        }
    }
}

