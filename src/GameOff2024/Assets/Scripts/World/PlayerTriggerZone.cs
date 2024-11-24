using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerZone : MonoBehaviour
{
    [SerializeField] private UnityEvent onPlayerEnter;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;
            
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            onPlayerEnter?.Invoke();
        }
    }
}
