using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class MusicManagerScript : OnMessage<BeginNarrativeSection>
{
    public EventReference mxEvent;
    EventInstance mxEventInstance;

    private bool started = false;
    
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
        StartMusicIfNeeded();
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

    private void StartMusicIfNeeded()
    {
        if (started)
            return;

        started = true;
        mxEventInstance = RuntimeManager.CreateInstance(mxEvent);
        mxEventInstance.start();
    }
}
