using System;
using UnityEngine;

public class CS_EndingSequence : MonoBehaviour
{
    [SerializeField] private bool beginImmediately;
    [SerializeField] private CS_AudioPlayer CS_script;
    
    private int MarkerInc = 0;

    private Action onLightsOff = () => { };
    private Action onLightsOn = () => { };
    private Action onPlayerResume = () => { };
    
    private void OnEnable()
    {
        CS_script = GetComponent<CS_AudioPlayer>();
        CS_script.OnCinematicMarkerHit += CS_script_OnCinematicMarkerHit;
        if (beginImmediately)
            CS_script.TriggerCinematicAudio();
    }

    private void OnDisable()
    {
        CS_script.OnCinematicMarkerHit -= CS_script_OnCinematicMarkerHit;
    }

    public void TriggerCinematicAudio(Action lightsOff, Action lightsOn, Action playerResume)
    {
        onLightsOff = lightsOff;
        onLightsOn = lightsOn;
        onPlayerResume = playerResume;
        MarkerInc = 0;
        
        CS_script.TriggerCinematicAudio();
    }

    private void CS_script_OnCinematicMarkerHit()
    {
        //light switch off
        if (MarkerInc == 0)
        {
            Debug.Log("Light Switch off");
            onLightsOff();
        }
        //light switch on
        else if (MarkerInc == 1)
        {
            Debug.Log("Light Switch on");
            onLightsOn();
        }
        //resume player control
        else if (MarkerInc == 2)
        {
            Debug.Log("Player resume control");
            onPlayerResume();
        }
        MarkerInc++;
    }
}
