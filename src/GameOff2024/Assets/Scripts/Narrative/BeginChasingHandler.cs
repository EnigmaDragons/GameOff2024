using UnityEngine;

public class BeginChasingHandler : OnMessage<BeginNarrativeSection, SpawnHandler>
{
    [SerializeField] private SpyController handlerPrefab;

    private SpyController handler;
    
    protected override void Execute(BeginNarrativeSection msg)
    {
        if (msg.Section != NarrativeSection.ChasingHandler || handler == null)
            return;

        Log.Info("Begin Chasing Handler - Go!!!");
        CurrentGameState.UpdateState(g => g.objectiveTransform = handler.transform);
        handler.AllowFullSpeed();
    }

    protected override void Execute(SpawnHandler msg)
    {
        var gs = CurrentGameState.ReadOnly;
        Log.Info("Begin Chasing Handler - Opening Security Room Door");
        gs.handlerFinalFightRoomDoor.gameObject.SetActive(false);
        
        Log.Info("Begin Chasing Handler - Spawning handler");
        handler = Instantiate(handlerPrefab, gs.handlerSpawnPoint.position, gs.handlerSpawnPoint.rotation);
        handler.InitDestinationAndPlayerAndBeginRunning(gs.playerTransform, gs.handlerFinalFightRoom);
        handler.KeepSpeedAtZero();
    }
}
