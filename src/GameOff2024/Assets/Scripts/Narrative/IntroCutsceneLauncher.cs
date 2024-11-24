using UnityEngine;
using UnityEngine.Events;

public class IntroCutsceneLauncher : MonoBehaviour
{
    [SerializeField] private float delayBeforeActivateSeconds = 0.2f;
    [SerializeField] private CS_AudioPlayer csAudio;
    [SerializeField] private UnityEvent fadeToGameplay;
    [SerializeField] private UnityEvent audioEnded;
    [SerializeField] private float delayBeforeEnableLookControls = 37f;
    [SerializeField] private float delayBeforeEnableMovementControls = 48f;

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

    private void OnDestroy()
    {
        csAudio.OnCinematicMarkerHit -= fadeToGameplay.Invoke;
        csAudio.OnCinematicEventEnded -= audioEnded.Invoke;
    }

    private void BeginCutscene()
    {
        Message.Publish(new FadeOutMusic());
        Message.Publish(new DisablePlayerControls());
        csAudio.OnCinematicMarkerHit += fadeToGameplay.Invoke;
        csAudio.OnCinematicEventEnded += audioEnded.Invoke;
        csAudio.TriggerCinematicAudio();
        Message.Publish(new SetIntoxicationLevel(1f));
        this.ExecuteAfterDelay(() => Message.Publish(new EnablePlayerLookControls()), delayBeforeEnableLookControls);
        this.ExecuteAfterDelay(() => Message.Publish(new EnablePlayerControls()), delayBeforeEnableMovementControls);
    }
}
