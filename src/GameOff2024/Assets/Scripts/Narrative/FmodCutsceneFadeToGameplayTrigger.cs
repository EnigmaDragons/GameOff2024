using System;
using System.Runtime.InteropServices;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class FmodFadeToGameplayListener : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter eventEmitter;
    [SerializeField] private UnityEvent fadeToGameplayAction;
    
    void Start()
    {
        if (eventEmitter != null && eventEmitter.EventInstance.isValid())
        {
            // Set up callback for timeline markers/cues
            eventEmitter.EventInstance.setCallback(OnTimelineCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        }
    }

    private FMOD.RESULT OnTimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            // Convert the parameters to a TIMELINE_MARKER_PROPERTIES struct
            var props = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(TIMELINE_MARKER_PROPERTIES));
            
            // Check if this is our FadeToGameplay marker
            if (props.name == "FadeToGameplay")
            {
                Debug.Log($"Hit FadeToGameplay marker at position: {props.position}");
                fadeToGameplayAction.Invoke();
                // Add your fade to gameplay logic here
            }
            else
            {
                Debug.Log($"FadeToGameplay Props {props}");
            }
        }
        
        return FMOD.RESULT.OK;
    }

    void OnDestroy()
    {
        if (eventEmitter != null && eventEmitter.EventInstance.isValid())
        {
            eventEmitter.EventInstance.setCallback(null);
        }
    }
}
