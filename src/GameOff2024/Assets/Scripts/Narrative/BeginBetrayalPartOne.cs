using FMODUnity;
using UnityEngine;

public class BeginBetrayalPartOne : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private FadeInImage fadeIn;
    [SerializeField] private FadeOutImage fadeOut;
    [SerializeField] private float fadeCloseTime = 1.5f;
    [SerializeField] private StudioEventEmitter knockoutSound;
    [SerializeField] private float delayBetweenSounds = 0.5f;
    [SerializeField] private StudioEventEmitter briefcaseSound;
    [SerializeField] private CS_AudioPlayer handlerVoice;
    [SerializeField] private float delayBetweenHandlerLines = 0.5f;
    [SerializeField] private StudioEventEmitter handlerLineAfterBriefcaseTransfer;

    protected override void AfterEnable()
    {
        handlerVoice.OnCinematicEventEnded += PlayFinalHandlerLines;
    }

    protected override void AfterDisable()
    {
        handlerVoice.OnCinematicEventEnded -= PlayFinalHandlerLines;
    }
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CaughtSpy)
            return;
        
        Log.Info("Betrayal - Part One");
        Log.Info("Betrayal - Part One - Disabling player controls");
        Message.Publish(new StopTheSpy());
        Message.Publish(new UnregisterObjective());
        //Message.Publish(new FadeOutMusic());
        Message.Publish(new DisablePlayerControls());
        FadeToBlack();
    }

    private void FadeToBlack()
    {
        Log.Info("Betrayal - Part One - Starting fade to black");
        fadeIn.StartFade(false, PlaySounds);   
    }

    private void PlaySounds()
    {
        Log.Info("Betrayal - Part One - Playing knockout sequence sounds");
        knockoutSound.Play();
        Message.Publish(new KnockOutTheSpy());
        this.ExecuteAfterDelay(briefcaseSound.Play, delayBetweenSounds);
        this.ExecuteAfterDelay(FadeGameInAndResume, fadeCloseTime);
    }

    private void FadeGameInAndResume()
    {
        Log.Info("Betrayal - Part One - Fading game back in and resuming");
        fadeOut.StartFade(false, () => { 
            Log.Info("Betrayal - Part One - Re-enabling player controls");
            Message.Publish(new EnablePlayerControls());
            Log.Info("Betrayal - Part One - Player is Holding Briefcase");
            Message.Publish(new PlayerHoldBriefcase());
            Log.Info("Betrayal - Part One - Playing handler voice line");
            handlerVoice.TriggerCinematicAudio();
            Message.Publish(new BeginNarrativeSection(NarrativeSection.CarryingBriefcase));
        });
    }

    private void PlayFinalHandlerLines()
    {
        this.ExecuteAfterDelay(() =>
        {
            Log.Info("Betrayal - Part One - Playing Final Handler Lines");
            handlerLineAfterBriefcaseTransfer.Play();
        }, delayBetweenHandlerLines);
    }
}
