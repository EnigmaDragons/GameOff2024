
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
        Debug.Log("Spy destination is " + spyDestination.position);
    }
}
