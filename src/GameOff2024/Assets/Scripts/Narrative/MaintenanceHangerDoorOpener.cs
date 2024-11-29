using UnityEngine;

public class MaintenanceHangerDoorOpener : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private GameObject maintenanceHangarDoor;

    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.CarryingBriefcase)
            return;
        
        maintenanceHangarDoor.SetActive(false);
        CurrentGameState.UpdateState(gs => gs.objectiveTransform = maintenanceHangarDoor.transform);
        Log.Info("Maintenance Hanger Door Opener - Objective set to Maintenance Hangar Door");
    }
}
