using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class SpyController : MonoBehaviour
{
    [SerializeField] float spySpeedMinimum;
    [SerializeField] float spySpeedMaximum;
    float spySpeed;
    NavMeshAgent navMeshAgent;


    Transform playerCharacterTransform;
    // Distance from player is tracked, move speed is lerped between 
    float currentDistance;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;

    Vector3 destination;

    NavMeshPath pathToPlayer;

    // the interval between calculating the distance to the player
    [SerializeField] float playerDistanceCalcInterval;
    float playerDistanceCalcTimer = 0f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        navMeshAgent.SetDestination(DestinationSingleton.instance.transform.position);
        //playerCharacterTransform = GameObject.FindWithTag("Player").transform;
        pathToPlayer = new NavMeshPath();
    }

    private void FixedUpdate()
    {
        if(playerDistanceCalcTimer <= 0f)
        {
            if(playerCharacterTransform == null)
            {
                playerCharacterTransform = GameObject.FindWithTag("Player").transform;
            }
            Debug.Log(pathToPlayer.corners.Length);

            if (navMeshAgent.CalculatePath(playerCharacterTransform.position, pathToPlayer))
            {
                currentDistance = Mathf.Clamp(CalculatePathDistance(), minDistance, maxDistance);
                spySpeed = Mathf.Lerp(spySpeedMaximum, spySpeedMinimum, (currentDistance - minDistance) / (maxDistance - minDistance));
                navMeshAgent.speed = spySpeed;
                playerDistanceCalcTimer = playerDistanceCalcInterval;
            }

        }
        else
        {
            playerDistanceCalcTimer -= Time.deltaTime;
        }
    }

    private float CalculatePathDistance()
    {
        float distSquared = 0;
        for(int i = 1; i < pathToPlayer.corners.Length; i++)
        {
            distSquared += Vector3.SqrMagnitude(pathToPlayer.corners[i] - pathToPlayer.corners[i - 1]);
        }
        return Mathf.Sqrt(distSquared);
    }
}
