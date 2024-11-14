using DunGen;
using DunGen.Adapters;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpyController : OnMessage<GameStateChanged>
{
    public enum TraversalLinkTypes
    {
        running,
        climbing,
        sliding,
        jumping,
        jumpingDown
    }
    [SerializeField] float spyBaseSpeedRunning;
    [SerializeField] float spyBaseSpeedClimbing;
    [SerializeField] float spyBaseSpeedSliding;
    [SerializeField] float spyBaseSpeedJumping;
    [SerializeField] float spyBaseSpeedFalling;

    private float spyBaseSpeed;
    [SerializeField] float spySpeedMultiplierMinimum;
    [SerializeField] float spySpeedMultiplierMaximum;
    float spySpeed;
    NavMeshAgent navMeshAgent;


    // Distance from player is tracked, move speed is lerped between 
    float currentDistance;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;

    [SerializeField] Transform playerCharacterTransform;
    [SerializeField] Transform destinationTransform;

    private NavMeshPath pathToPlayer;

    private bool _playerFound = false;
    private bool _destinationFound = false;
    private bool _waypointsSet = false;

    // the interval between calculating the distance to the player
    [SerializeField] float playerDistanceCalcInterval;
    float playerDistanceCalcTimer = 0f;

    [Header("Tagging the spy")]
    [SerializeField] SphereCollider playerTagTrigger;
    [SerializeField] float playerTagTriggerRadius;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
            Log.Error("Missing NavMeshAgent");
        navMeshAgent.autoTraverseOffMeshLink = false;
        navMeshAgent.enabled = false;
    }
    
    private void Start()
    {
        pathToPlayer = new NavMeshPath();
        SetSpeed(TraversalLinkTypes.running);
        if (UnityNavMeshAdapter.instance != null)
            UnityNavMeshAdapter.instance.OnNavmeshBaked += Instance_OnNavmeshBaked;
        playerTagTrigger.radius = playerTagTriggerRadius;
    }

    private void Instance_OnNavmeshBaked(object sender, System.EventArgs e)
    {
        navMeshAgent.enabled = true;
    }

    private void FixedUpdate()
    {
        if(!navMeshAgent.enabled) return;
        if (_playerFound && _destinationFound)
        {
            playerDistanceCalcTimer -= Time.deltaTime;
            if (playerDistanceCalcTimer <= 0f)
            {
                if (navMeshAgent.CalculatePath(playerCharacterTransform.position, pathToPlayer))
                {
                    currentDistance = Mathf.Clamp(CalculatePathDistance(), minDistance, maxDistance);
                    spySpeed = Mathf.Lerp(spyBaseSpeed*spySpeedMultiplierMaximum, spyBaseSpeed*spySpeedMultiplierMinimum, (currentDistance - minDistance) / (maxDistance - minDistance));
                    navMeshAgent.speed = spySpeed;
                    playerDistanceCalcTimer = playerDistanceCalcInterval;
                }
            }
        }
        if (navMeshAgent.isOnOffMeshLink)
        {
            OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;

            //calculate the final point of the link
            Vector3 endPos = data.endPos + Vector3.up * navMeshAgent.baseOffset;

            //Move the navMeshAgent to the end point
            navMeshAgent.transform.position = Vector3.MoveTowards(navMeshAgent.transform.position, endPos, navMeshAgent.speed * Time.deltaTime);

            //when the navMeshAgent reach the end point you should tell it, and the navMeshAgent will "exit" the link and work normally after that
            if (navMeshAgent.transform.position == endPos)
            {
                navMeshAgent.CompleteOffMeshLink();
            }
        }
    }
    public void SetSpeed(TraversalLinkTypes linkType)
    {
        switch(linkType)
        {
            case TraversalLinkTypes.running:
                spyBaseSpeed = spyBaseSpeedRunning;
                break;
            case TraversalLinkTypes.sliding:
                spyBaseSpeed = spyBaseSpeedSliding;
                break;
            case TraversalLinkTypes.climbing:
                spyBaseSpeed = spyBaseSpeedClimbing;
                break;
            case TraversalLinkTypes.jumping:
                spyBaseSpeed = spyBaseSpeedJumping;
                break;
            case TraversalLinkTypes.jumpingDown:
                spyBaseSpeed = spyBaseSpeedFalling;
                break;
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

        }
        destinationTransform = msg.State.spyDestination;
        if(destinationTransform != null)
        {
            _destinationFound = true;
            StartCoroutine("SetDestination");//navMeshAgent.SetDestination(destinationTransform.position);
            //StartCoroutine(CalculatePathWithClosestPortals(destinationTransform.position));
        }
    }

    public IEnumerator SetDestination()
    {
        while (navMeshAgent.enabled == false)
        {
            yield return null;
        }
        navMeshAgent.SetDestination(destinationTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CurrentGameState.UpdateState(gs =>
            {
                gs.gameWon = true;
            });
        }
    }
}
