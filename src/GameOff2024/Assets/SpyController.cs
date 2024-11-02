using UnityEngine;
using UnityEngine.AI;

public class SpyController : MonoBehaviour
{
    [SerializeField] float spySpeedMinimum;
    [SerializeField] float spySpeedMaximum;
    float spySpeed;
    NavMeshAgent navMeshAgent;

    [SerializeField] float maxDistance;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        
    }
}
