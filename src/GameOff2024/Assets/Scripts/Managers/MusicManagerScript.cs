using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicManagerScript : OnMessage<BeginNarrativeSection>
{
    public EventReference mxEvent;
    EventInstance mxEventInstance;


    private Dictionary<NarrativeSection, string> sectionMapping = new()
    {
        { NarrativeSection.Intro, "Unconscious" },
        { NarrativeSection.IntroOpenEyes, "Tutorial" },
        { NarrativeSection.IntroPlayerFullControl, "TutorialSectionA" },
        { NarrativeSection.IntroHalfwayThrough, "TutorialSectionB" },
        { NarrativeSection.ChasingSpy, "SectionA" },
        { NarrativeSection.CaughtSpy, "SectionB" },
        { NarrativeSection.CarriedBriefcase, "SectionC" },
        { NarrativeSection.ChasingHandler, "SectionD" },
        { NarrativeSection.CaughtHandler, "SectionC" }
    };

    void OnDestroy()
    {
        Log.Info("Music Manager - OnDestroy");
        mxEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        mxEventInstance.release();
    }

    protected override void Execute(BeginNarrativeSection msg)
    {
        
        if(msg.Section == NarrativeSection.IntroOpenEyes)
        {
            mxEventInstance = RuntimeManager.CreateInstance(mxEvent);
            mxEventInstance.start();
        }        

        if (sectionMapping.TryGetValue(msg.Section, out var musicParam))
        {
            Log.Info($"Music Manager - param {musicParam} for section {msg.Section}");
            mxEventInstance.setParameterByNameWithLabel("MusicControl", musicParam);
        }
        else
        {
            Log.Warn($"Music Manager - No Music Param known for {msg.Section}");
        }
    }
}
