using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class MusicManagerScript : MonoBehaviour
{
    StudioEventEmitter studioEventEmitter;
    EventInstance mxEvent;
    Rigidbody rb;
    bool firstMove;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        studioEventEmitter = GetComponent<StudioEventEmitter>();
        rb = GetComponent<Rigidbody>();

        if (studioEventEmitter != null)
        {
            mxEvent = studioEventEmitter.EventInstance;
        }
        
    }

    void FirstMovement()
    {
        if (studioEventEmitter != null)
        {
            mxEvent.setParameterByNameWithLabel("MusicControl", "SectionA");
            firstMove = true;
        }
    }

    void MovementUpdateRunning(float moveValue)
    {
        mxEvent.setParameterByName("MX_Running", Mathf.Lerp(0f, 1f, moveValue));
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

}

