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

    private NavMeshPath pathToPlayer;

    private bool _playerFound = false;
    private bool _destinationFound = false;

    // the interval between calculating the distance to the player
    [SerializeField] float playerDistanceCalcInterval;
    float playerDistanceCalcTimer = 0f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
            Log.Error("Missing NavMeshAgent");
    }

    private void Start()
    {
        pathToPlayer = new NavMeshPath();
    }

    private void FixedUpdate()
    {
        if (!_playerFound)
        {
            var gObj = GameObject.FindWithTag("Player");
            if (gObj != null)
            {
                playerCharacterTransform = gObj.transform;
                _playerFound = true;
            }
        }

        if (!_destinationFound)
        {
            if (DestinationSingleton.instance != null && DestinationSingleton.instance.transform != null)
            {
                navMeshAgent.SetDestination(DestinationSingleton.instance.transform.position);
                _destinationFound = true;
            }
        }

        if (!_playerFound || !_destinationFound)
        {
            Debug.Log("Player or Destination not found yet");
            return;
        }

        if(playerDistanceCalcTimer <= 0f)
        {
            if (pathToPlayer != null && pathToPlayer.corners != null)
            {
                Debug.Log(pathToPlayer.corners.Length);
            }

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
