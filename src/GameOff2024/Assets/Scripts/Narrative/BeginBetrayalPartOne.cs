
using UnityEngine;

public class BeginBetrayalPartOne : OnMessage<BeginNarrativeSection>
{
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CaughtSpy)
            return;
        
        Log.Info("Begin Betrayal - Part One");
    }
}

