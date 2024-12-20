﻿using System;
using UnityEngine;

public class BeginBetrayalPartTwo : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private CS_AudioPlayer handlerAudio;
    [SerializeField] private Navigator navigator;

protected override void AfterEnable()
    {
        handlerAudio.OnCinematicMarkerHit += MarkerAction;
        handlerAudio.OnCinematicEventEnded += OnFinishedAction;
    }

    protected override void AfterDisable()
    {
        handlerAudio.OnCinematicMarkerHit -= MarkerAction;
        handlerAudio.OnCinematicEventEnded -= OnFinishedAction;
    }
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CarriedBriefcase)
            return;
        
        Log.Info("Begin Betrayal - Part Two. Briefcase Delivered");
        Message.Publish(new FadeOutMusic());
        Message.Publish(new UnregisterObjective());
        this.ExecuteAfterDelay(BeginHandlerAudioSection, 0.2f);
    }

    private void MarkerAction()
    {
        ForceMovePlayerToCover();
    }

    private void OnFinishedAction()
    {
        BeginFullEnding();
    }
    
    private void BeginHandlerAudioSection()
    {
        Message.Publish(new DisablePlayerControls());
        handlerAudio.TriggerCinematicAudio();
        // NOTE: Spawning Early to begin pathfinding
        Message.Publish(new SpawnHandler());
        this.ExecuteAfterDelay(() => Message.Publish(new PlayerReleaseBriefcase()), 4f);
        
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

    [Obsolete]
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
        //
        //     SFX: Barrage of gunshots crack through the air, pinging off metal as they strike the PC’s cover, until the weapon clicks empty.
        //
        // (angry grunt) “Nrghh!”  
        //
        // PC comes out of cover and sees the Handler running away. 
        //
        //     PLAYER RESUMES CONTROL.         
        
        Message.Publish(new BeginNarrativeSection(NarrativeSection.ChasingHandler));
        this.ExecuteAfterDelay(
            () => Message.Publish(new ForceMovePlayer(CurrentGameState.ReadOnly.handlerWatchRunningPoint.position, 
                ForceLookPlayerAtHandlerAfterExitCover)), 0.2f);
    }

    private void ForceMovePlayerToCover()
    {
        Log.Info("Force Move To Cover");
        Message.Publish(new ForceMovePlayer(CurrentGameState.ReadOnly.coverDestination.position, ForceLookPlayerAtHandler));
    }

    private void ForceLookPlayerAtHandler()
    {
        Log.Info("Force Look At Handler");
        Message.Publish(new ForceLookPlayer(CurrentGameState.ReadOnly.coverLookPoint.position, () => Log.Info("Force Look Finished")));
    }

    private void ForceLookPlayerAtHandlerAfterExitCover()
    {
        Log.Info("Force Look At Handler After Exit Cover");
        Message.Publish(new ForceLookPlayer(CurrentGameState.ReadOnly.handlerSpawnPoint.position, EnablePlayerControls));
    }

    private void EnablePlayerControls()
    {
        Log.Info("Enable Player Controls");
        Message.Publish(new EnablePlayerControls());
    }
}
