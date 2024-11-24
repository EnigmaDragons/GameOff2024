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
    [SerializeField] private StudioEventEmitter handlerVoice;
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CaughtSpy)
            return;
        
        Log.Info("Begin Betrayal - Part One");
        Log.Info("Disabling player controls");
        Message.Publish(new StopTheSpy());
        Message.Publish(new FadeOutMusic());
        Message.Publish(new DisablePlayerControls());
        FadeToBlack();
    }

    private void FadeToBlack()
    {
        Log.Info("Starting fade to black");
        fadeIn.StartFade(false, PlaySounds);   
    }

    private void PlaySounds()
    {
        Log.Info("Playing knockout sequence sounds");
        Message.Publish(new KnockOutTheSpy());
        knockoutSound.Play();
        this.ExecuteAfterDelay(() => {
            Log.Info("Playing briefcase sound");
            briefcaseSound.Play();
        }, delayBetweenSounds);
        this.ExecuteAfterDelay(() => {
            Log.Info("Starting fade back in");
            FadeGameInAndResume();
        }, fadeCloseTime);
    }

    private void FadeGameInAndResume()
    {
        Log.Info("Fading game back in and resuming");
        fadeOut.StartFade(false, () => { 
            Log.Info("Re-enabling player controls");
            Message.Publish(new EnablePlayerControls());
            Log.Info("Player is Holding Briefcase");
            Message.Publish(new PlayerHoldBriefcase());
            Log.Info("Playing handler voice line");
            handlerVoice.Play();
        });
    }
}
