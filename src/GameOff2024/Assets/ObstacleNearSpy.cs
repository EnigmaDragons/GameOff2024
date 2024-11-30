using UnityEngine;
using UnityEngine.AI;

public class ObstacleNearSpy : MonoBehaviour
{
    public Transform spy;
    NavMeshObstacle obstacle;
    private void Start()
    {
        spy = FindAnyObjectByType<SpyController>().transform;
        obstacle = GetComponent<NavMeshObstacle>();
    }

    private void Update()
    {
        if(spy != null)
        {
            obstacle.enabled = Vector3.SqrMagnitude(transform.position - spy.position) < 225;
        }
        else
        {
            spy = FindAnyObjectByType<SpyController>().transform;
        }
    }
}
