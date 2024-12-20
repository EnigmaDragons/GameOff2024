using System;
using UnityEngine;

[Serializable]
public sealed class GameState
{
    // Should consist of only serializable primitives.
    // Any logic or non-trivial data should be enriched in CurrentGameState.
    // Except for Save/Load Systems, everything should use CurrentGameState,
    // instead of this pure data structure.
    
    // All enums used in this class should have specified integer values.
    // This is necessary to preserve backwards save compatibility.

    public bool shouldShowIntroCutscene;

    public Transform playerTransform;
    public Transform spyDestination;
    public Transform coverDestination;
    public Transform coverLookPoint;
    public Transform droppedBriefcase;
    public Transform objectiveTransform;
    public Transform handlerFinalFightRoom;
    public Transform handlerSpawnPoint;
    public Transform handlerFinalFightRoomDoor;
    public Transform handlerWatchRunningPoint;
    public Transform maintenanceHangarTeleportPoint;

    public Transform handlerFinalFightDoorTeleportPoint;
    public bool gameWon;
    public bool gameLost;
}
