using UnityEngine;

public class DestinationDefeatTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<SpyController>() != null)
        {
            CurrentGameState.UpdateState(gs =>
            {
                gs.gameLost = true;
            });
        }
    }
}
