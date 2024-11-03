using UnityEngine;
using UnityEngine.AI;

public class SpyController : OnMessage<GameStateChanged>
{
    [SerializeField] float spySpeedMinimum;
    [SerializeField] float spySpeedMaximum;
    float spySpeed;
    NavMeshAgent navMeshAgent;


    // Distance from player is tracked, move speed is lerped between 
    float currentDistance;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;

    Transform playerCharacterTransform;
    Transform destinationTransform;

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
        if (_playerFound && _destinationFound)
        {
            playerDistanceCalcTimer -= Time.deltaTime;
            if (playerDistanceCalcTimer <= 0f)
            {
                if (navMeshAgent.CalculatePath(playerCharacterTransform.position, pathToPlayer))
                {
                    currentDistance = Mathf.Clamp(CalculatePathDistance(), minDistance, maxDistance);
                    spySpeed = Mathf.Lerp(spySpeedMaximum, spySpeedMinimum, (currentDistance - minDistance) / (maxDistance - minDistance));
                    navMeshAgent.speed = spySpeed;
                    playerDistanceCalcTimer = playerDistanceCalcInterval;
                }
            }
        }
    }

    private float CalculatePathDistance()
    {
        float dist = 0;
        for(int i = 1; i < pathToPlayer.corners.Length; i++)
        {
            dist += Vector3.Distance(pathToPlayer.corners[i],pathToPlayer.corners[i - 1]);
        }
        return dist;
    }

    protected override void Execute(GameStateChanged msg)
    {
        playerCharacterTransform = msg.State.playerTransform;
        if(playerCharacterTransform != null)
        {
            _playerFound = true;
            Debug.Log("Player Found");

        }
        destinationTransform = msg.State.spyDestination;
        if(destinationTransform != null)
        {
            _destinationFound = true;
            navMeshAgent.SetDestination(destinationTransform.position);
            Debug.Log("Destination Set");

        }
    }
}
