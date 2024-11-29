using UnityEngine;

public class ObjectiveIndicatorController : OnMessage<UnregisterObjective>
{
    [SerializeField] private GameObject targetGameObject;

    void Update()
    {
        var gameState = CurrentGameState.ReadOnly;

        if (gameState.objectiveTransform != null)
        {
            targetGameObject.SetActive(true);
            Vector3 direction = gameState.objectiveTransform.position - gameState.playerTransform.position;
            direction.y = 0; // Keep the direction strictly horizontal
            Quaternion rotation = Quaternion.LookRotation(-direction); // Invert the direction to fix the arrow facing backwards
            targetGameObject.transform.rotation = rotation;
        }
        else
        {
            targetGameObject.SetActive(false);
        }
    }

    protected override void Execute(UnregisterObjective msg)
    {
        CurrentGameState.UpdateState(gs => gs.objectiveTransform = null);
    }
}
