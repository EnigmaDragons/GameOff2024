using UnityEngine;

public class RecordAsCoverPoint : MonoBehaviour
{
    [SerializeField] private Transform pos;

    void Start()
    {
        CurrentGameState.UpdateState(gs =>
        {
            gs.coverDestination = pos;
        });
    }
}
