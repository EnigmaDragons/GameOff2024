using UnityEngine;

[CreateAssetMenu]
public class EventPublisher : ScriptableObject
{
    public void TriggerSpyCaughtNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CaughtSpy));
    public void TriggerBriefcaseCarriedNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CarriedBriefcase));
    public void TriggerHandlerCaughtNarrative() => Message.Publish(new BeginNarrativeSection(NarrativeSection.CaughtHandler));
    public void TriggerTutorialSecondHalf() => Message.Publish(new BeginNarrativeSection(NarrativeSection.IntroHalfwayThrough));
    public void TeleportToMainenanceHangar() => Message.Publish(new TeleportPlayer(CurrentGameState.ReadOnly.maintenanceHangarTeleportPoint.position));
    public void TeleportToHandlerFinalFightDoor() => Message.Publish(new TeleportPlayer(CurrentGameState.ReadOnly.handlerFinalFightDoorTeleportPoint.position));
}
