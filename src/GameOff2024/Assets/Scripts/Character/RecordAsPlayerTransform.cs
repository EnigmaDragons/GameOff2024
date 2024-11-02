using UnityEngine;

public class RecordAsPlayerTransformOnStart : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        CurrentGameState.UpdateState(gs =>
        {
            gs.playerTransform = playerTransform;
        });
    }
}
