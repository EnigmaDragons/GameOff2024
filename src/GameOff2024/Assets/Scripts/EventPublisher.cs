using UnityEngine;

[CreateAssetMenu]
public class EventPublisher : ScriptableObject
{
    public void TriggerSpyCaughtNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CaughtSpy));
    public void TriggerBriefcaseCarriedNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CarriedBriefcase));
    public void TriggerHandlerCaughtNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CaughtHandler));
}
