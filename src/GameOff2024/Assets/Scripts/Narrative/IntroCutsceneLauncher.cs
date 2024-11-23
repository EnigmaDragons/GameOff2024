using UnityEngine;
using UnityEngine.Events;

public class IntroCutsceneLauncher : MonoBehaviour
{
    [SerializeField] private float delayBeforeActivateSeconds = 0.2f;
    [SerializeField] private CS_AudioPlayer csAudio;
    [SerializeField] private UnityEvent fadeToGameplay;
    [SerializeField] private UnityEvent audioEnded;

    private void Start()
    {
        this.ExecuteAfterDelay(InvokeCutsceneCheck, delayBeforeActivateSeconds);
    }
    
    private void InvokeCutsceneCheck()
    {
        var shouldShow = CurrentGameState.ReadOnly.shouldShowIntroCutscene;
        Log.Info($"Intro Cutscene - Showing {shouldShow}");
        if (shouldShow)
            BeginCutscene();
        else 
            Message.Publish(new HideEyes());
    }

    private void BeginCutscene()
    {
        Message.Publish(new FadeOutMusic());
        csAudio.OnCinematicMarkerHit += () => fadeToGameplay.Invoke();
        csAudio.OnCinematicEventEnded += () => audioEnded.Invoke();
        csAudio.TriggerCinematicAudio();
    }
}
