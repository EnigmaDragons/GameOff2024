using UnityEngine;

public class BeginChasingHandler : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private SpyController handlerPrefab;
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.ChasingHandler)
            return;

        var gs = CurrentGameState.ReadOnly;
        Log.Info("Begin Chasing Handler - Opening Security Room Door");
        gs.handlerFinalFightRoomDoor.gameObject.SetActive(false);
        Log.Info("Begin Chasing Handler - Spawning handler");
        var handler = Instantiate(handlerPrefab, gs.handlerSpawnPoint.position, gs.handlerSpawnPoint.rotation);
        handler.InitDestinationAndPlayer(gs.playerTransform, gs.handlerFinalFightRoom);
        CurrentGameState.UpdateState(g => g.objectiveTransform = handler.transform);
    }
}
