using System;
using NeoCC;
using UnityEngine;

public class BeginBetrayalPartTwo : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private NeoCharacterController playerController;
    [SerializeField] private CS_AudioPlayer handlerAudio;
    [SerializeField] private Navigator navigator;
    
    [SerializeField] private bool doingExtendedEnding = true;
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CarriedBriefcase)
            return;
        
        Log.Info("Begin Betrayal - Part Two. Briefcase Delivered");
        Message.Publish(new FadeOutMusic());
        this.ExecuteAfterDelay(TriggerCutscene, 0.2f);
    }

    private void TriggerCutscene()
    {
        BeginHandlerAudioSection(doingExtendedEnding ? BeginFullEnding : BeginCliffhangerEnding);
    }

    private void BeginHandlerAudioSection(Action onFinished)
    {
        Message.Publish(new DisablePlayerControls());
        handlerAudio.OnCinematicMarkerHit += onFinished;
        handlerAudio.TriggerCinematicAudio();
        
        // NARRATIVE SCRIPT:
        // PLAYER ARRIVES. PLAYER LOSES CONTROL. 
        //
        // SFX: A metallic click of a handgun's hammer echoes behind the player. 
        //
        // (voice right behind player) 
        // “Don’t move.”
        // “Drop the briefcase.” 
        //
        // SFX: Briefcase thuds against the floor. 
        //
        // “Now, toss your gun.” 
        //
        // SFX: Pistol clatters against the floor. 
        //
        // “I’m…I’m sorry for drugging you, Z.”
        // “I can’t trust anyone. I have to reveal the agency’s secrets in that briefcase.” 
        // “Only I can do it. It has to be me.” 
        // “I’ll never forget…your sacrifice.” 
    }

    private void BeginCliffhangerEnding()
    {
        // SCRIPT
        // SFX: Gunshot.
        //
        //     SUDDEN BLACK SCREEN.
        //     TO BE CONTINUED. CREDITS.
        
        navigator.NavigateToCreditsScene();
    }

    private void BeginFullEnding()
    {
        // SCRIPT
        // SFX: Briefcase skids across the floor, slamming into the Handler with a dull thud. Followed instantly by the deafening crack of a pistol.
        //
        // (hit by suitcase) “Agh!” 
        //
        // PC runs towards cover. 
        
        ForceMovePlayerToCover();
        
        //
        //     SFX: Barrage of gunshots crack through the air, pinging off metal as they strike the PC’s cover, until the weapon clicks empty.
        //
        // (angry grunt) “Nrghh!”  
        //
        // PC comes out of cover and sees the Handler running away. 
        //
        //     PLAYER RESUMES CONTROL. 
    }

    private void ForceMovePlayerToCover()
    {
        Log.Info("Force Move To Cover");
        Message.Publish(new ForceMovePlayer(CurrentGameState.ReadOnly.coverDestination.position, () => Log.Info("Force Move Finished")));
    }
}
