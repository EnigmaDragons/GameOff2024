using UnityEngine;

public class CS_ListenerTest : MonoBehaviour
{
    CS_AudioPlayer CS_script;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CS_script = GetComponent<CS_AudioPlayer>();
        CS_script.OnCinematicEventEnded += CS_script_OnCinematicEventEnded;
        CS_script.OnCinematicMarkerHit += CS_script_OnCinematicMarkerHit;
        CS_script.TriggerCinematicAudio();
    }

    private void CS_script_OnCinematicMarkerHit()
    {
        Debug.Log("Marker Hit");
    }

    private void CS_script_OnCinematicEventEnded()
    {
        Debug.Log("CS Event ended");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
