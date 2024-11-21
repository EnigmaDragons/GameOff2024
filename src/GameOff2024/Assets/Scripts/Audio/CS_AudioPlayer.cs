using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

public delegate void CSEventHandler(object sender, EventArgs e);

public class CS_AudioPlayer : MonoBehaviour
{
    public EventReference CinematicEvent;
    private EventInstance CinematicEventInstance;
    private EVENT_CALLBACK callback;

    public event Action OnCinematicEventEnded;

    // trigger Cinematic Event
    void TriggerCinematicAudio()
    {
        CinematicEventInstance = RuntimeManager.CreateInstance(CinematicEvent);

        if (CinematicEventInstance.isValid())
        {
            callback = new EVENT_CALLBACK(OnEventStopped);
            CinematicEventInstance.setCallback(callback, EVENT_CALLBACK_TYPE.STOPPED);
            CinematicEventInstance.start();
        }
    }

    void OnDestroy()
    { // Clean up the event instance
        CinematicEventInstance.release(); 
    }

    private FMOD.RESULT OnEventStopped(EVENT_CALLBACK_TYPE type, System.IntPtr instance, IntPtr parameterPtr) 
    {
        if (type == EVENT_CALLBACK_TYPE.STOPPED) 
        {
            OnCinematicEventEnded?.Invoke();
        } 
        
        return FMOD.RESULT.OK; 
    }
}

