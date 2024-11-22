using UnityEngine;

public class CS_EndingSequence : MonoBehaviour
{
    private CS_AudioPlayer CS_script;
    private int MarkerInc = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CS_script = GetComponent<CS_AudioPlayer>();
        CS_script.OnCinematicMarkerHit += CS_script_OnCinematicMarkerHit;
        CS_script.TriggerCinematicAudio();
    }

    private void CS_script_OnCinematicMarkerHit()
    {
        //light switch off
        if (MarkerInc == 0)
        {
            Debug.Log("Light Switch off");
        }
        //light switch on
        else if (MarkerInc == 1)
        {
            Debug.Log("Light Switch on");
        }
        //resume player control
        else if (MarkerInc == 2)
        {
            Debug.Log("Player resume control");
        }
        MarkerInc++;
    }
}
