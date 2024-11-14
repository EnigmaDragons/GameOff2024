
using UnityEngine;

public class RecordAsSpyDestinationOnStart : MonoBehaviour
{
    [SerializeField] private Transform spyDestination;

    void Start()
    {
        CurrentGameState.UpdateState(gs =>
        {
            gs.spyDestination = spyDestination;
        });
    }
}
