using UnityEngine;

public class RecordTransformMarker : MonoBehaviour
{
    [SerializeField] private Transform marker;
    [SerializeField] private TransformMarker markerType;
    [SerializeField] private bool isObjective = false;

    private void OnEnable()
    {
        if (markerType == TransformMarker.Player)
            CurrentGameState.UpdateState(gs => gs.playerTransform = marker);
        if (markerType == TransformMarker.SpyDestination)
            CurrentGameState.UpdateState(gs => gs.spyDestination = marker);
        if (markerType == TransformMarker.HandlerCoverDestination)
            CurrentGameState.UpdateState(gs => gs.coverDestination = marker);
        if (markerType == TransformMarker.HandlerCoverLookPoint)
            CurrentGameState.UpdateState(gs => gs.coverLookPoint = marker);
        if (markerType == TransformMarker.DroppedBriefcase)
            CurrentGameState.UpdateState(gs => gs.droppedBriefcase = marker);
        if (markerType == TransformMarker.HandlerFinalFightRoom)
            CurrentGameState.UpdateState(gs => gs.handlerFinalFightRoom = marker);
        if (markerType == TransformMarker.HandlerSpawnPoint)
            CurrentGameState.UpdateState(gs => gs.handlerSpawnPoint = marker);
        if (markerType == TransformMarker.HandlerFinalFightRoomDoor)
            CurrentGameState.UpdateState(gs => gs.handlerFinalFightRoomDoor = marker);
        if (markerType == TransformMarker.HandlerWatchRunningPoint)
            CurrentGameState.UpdateState(gs => gs.handlerWatchRunningPoint = marker);
        if (markerType == TransformMarker.MaintenanceHangarTeleportPoint)
            CurrentGameState.UpdateState(gs => gs.maintenanceHangarTeleportPoint = marker);
        if (markerType == TransformMarker.HandlerFinalFightDoorTeleportPoint)
            CurrentGameState.UpdateState(gs => gs.handlerFinalFightDoorTeleportPoint = marker);
        if (isObjective || markerType == TransformMarker.Objective)
            CurrentGameState.UpdateState(gs => gs.objectiveTransform = marker);
    }
}
