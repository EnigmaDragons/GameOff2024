using UnityEngine;

public class BeginHandlerCaughtNarrative : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private CS_EndingSequence audioSequence;
    [SerializeField] private FadeInImage fadeIn;
    [SerializeField] private FadeOutImage fadeOut;
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CaughtHandler)
            return;
        
        Message.Publish(new DisablePlayerControls());
        audioSequence.TriggerCinematicAudio(OnSwitchLightsOff, OnSwitchLightsOn, OnPlayerResumeControl);
    }

    private void OnSwitchLightsOff()
    {
        fadeIn.StartFade(false);
    }

    private void OnSwitchLightsOn()
    {
    }

    private void OnPlayerResumeControl()
    {
        fadeOut.StartFade(false);
        Message.Publish(new EnablePlayerControls());
    }
}
