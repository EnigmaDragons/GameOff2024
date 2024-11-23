using UnityEngine;

public class IntroCutsceneLauncher : MonoBehaviour
{
    [SerializeField] private float delayBeforeActivateSeconds = 0.2f;

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
        Message.Publish(new OpenEyes());
    }
}
