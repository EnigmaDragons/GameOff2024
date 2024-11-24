using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public delegate void CSEventHandler(object sender, EventArgs e);

public class CS_AudioPlayer : MonoBehaviour
{
    public EventReference CinematicEvent;
    private EventInstance CinematicEventInstance;
    private EVENT_CALLBACK callback;
    private bool hasValidInstance = false;
    private bool shouldTriggerFinish = false;

    public event Action OnCinematicEventEnded;
    public event Action OnCinematicMarkerHit;

    // trigger Cinematic Event
    public void TriggerCinematicAudio()
    {
        try
        {
            if (!CinematicEvent.IsNull)
            {
                CinematicEventInstance = RuntimeManager.CreateInstance(CinematicEvent);
                hasValidInstance = CinematicEventInstance.isValid();

                if (hasValidInstance)
                {
                    callback = new EVENT_CALLBACK(OnEventStopped);
                    CinematicEventInstance.setCallback(callback, EVENT_CALLBACK_TYPE.STOPPED | EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
                    CinematicEventInstance.start();
                }
                else
                {
                    Debug.LogWarning("Failed to create valid FMOD event instance");
                }
            }
            else
            {
                Debug.LogWarning("No FMOD event reference assigned");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error triggering cinematic audio: {e.Message}");
        }
    }

    void OnDestroy()
    {
        try
        {
            if (hasValidInstance)
            {
                PLAYBACK_STATE state;
                CinematicEventInstance.getPlaybackState(out state);
                if (state != PLAYBACK_STATE.STOPPED)
                {
                    CinematicEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                }
                CinematicEventInstance.release();
                hasValidInstance = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error cleaning up FMOD event: {e.Message}");
        }
    }

    private void Update()
    {
        try
        {
            if (shouldTriggerFinish)
            {
                shouldTriggerFinish = false;
                OnCinematicEventEnded?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error triggering end of Cinematic Audio {e.Message}");
        }
    }
    
    private FMOD.RESULT OnEventStopped(EVENT_CALLBACK_TYPE type, System.IntPtr instance, IntPtr parameterPtr)
    {
        try
        {
            if (type == EVENT_CALLBACK_TYPE.STOPPED)
            {
                shouldTriggerFinish = true;
            }
            else if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                OnCinematicMarkerHit?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in FMOD callback: {e.Message}");
        }

        return FMOD.RESULT.OK;
    }
}
