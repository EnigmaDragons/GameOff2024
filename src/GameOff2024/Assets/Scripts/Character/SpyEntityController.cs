using DunGen.Adapters;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SpyController : OnMessage<GameStateChanged, KnockOutTheSpy, StopTheSpy>
{
    public enum TraversalLinkTypes
    {
        running,
        climbing,
        sliding,
        jumping,
        jumpingDown
    }
    bool linkHasCurve = false;
    [SerializeField] AnimationCurve climbSpeedCurveX;
    [SerializeField] AnimationCurve climbSpeedCurveY;
    float climbTime;
    [SerializeField] float climbTimeTotalDuration = 180f;
    [SerializeField] float spyBaseSpeedRunning;
    [SerializeField] float spyBaseSpeedClimbing;
    [SerializeField] float spyBaseSpeedSliding;
    [SerializeField] float spyBaseSpeedJumping;
    [SerializeField] float spyBaseSpeedFalling;
    OffMeshLinkData currentLink;
    private float spyBaseSpeed;
    [SerializeField] float spySpeedMultiplierMinimum;
    [SerializeField] float spySpeedMultiplierMaximum;
    float spySpeed;
    NavMeshAgent navMeshAgent;
    private Rigidbody[] ragdollRigidbodies;
    private Animator animator;
    private bool isKnockedOut = false;
    private bool hasBeenTagged = false;

    // Distance from player is tracked, move speed is lerped between 
    float currentDistance;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;

    [SerializeField] Transform playerCharacterTransform;
    [SerializeField] Transform destinationTransform;
    private NavMeshPath pathToPlayer;

    private bool _playerFound = false;
    private bool _destinationFound = false;

    public event EventHandler<EventArgs> OnDestinationFound;

    // the interval between calculating the distance to the player
    [SerializeField] float playerDistanceCalcInterval;
    float playerDistanceCalcTimer = 0f;

    [Header("Tagging the spy")]
    [SerializeField] SphereCollider playerTagTrigger;
    [SerializeField] float playerTagTriggerRadius;
    [SerializeField] private Transform handBriefcase;

    [Header("Handler Variant")] 
    [SerializeField] private bool startsAsSpy = true;
    [SerializeField] private Animator spyAnimator;
    [SerializeField] private GameObject spyModel;
    [SerializeField] private Animator handlerAnimator;
    [SerializeField] private GameObject handlerModel;

    private bool _shouldStartRunningOnStart = false;
    private bool _keepSpeedAtZero = false;
    private bool _lockDestination = false;
    private bool _isInvincibleToPlayer = false;

    // Stamina degradation
    [SerializeField] float staminaDegradeRate;
    private float staminaDegradeTimer = 0f;
    private float effectiveMaxSpeedFactor = 1f;
    
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
            Log.Error("Missing NavMeshAgent");
        navMeshAgent.autoTraverseOffMeshLink = false;

        animator = startsAsSpy ? spyAnimator : handlerAnimator;
        spyModel.SetActive(startsAsSpy);
        handlerModel.SetActive(!startsAsSpy);
    }
    
    private void Start()
    {
        pathToPlayer = new NavMeshPath();
        SetSpeed(TraversalLinkTypes.running);

        if (UnityNavMeshAdapter.instance != null)
        {
            if (!_shouldStartRunningOnStart)
                navMeshAgent.enabled = false;
            UnityNavMeshAdapter.instance.OnNavmeshBaked += Instance_OnNavmeshBaked;

        }
        playerTagTrigger.radius = playerTagTriggerRadius;
    }

    private void Instance_OnNavmeshBaked(object sender, System.EventArgs e)
    {
        navMeshAgent.enabled = true;
    }

    private void FixedUpdate()
    {
        if(!navMeshAgent.enabled || isKnockedOut) return;
        if (_playerFound && _destinationFound)
        {
            playerDistanceCalcTimer -= Time.deltaTime;
            staminaDegradeTimer += Time.deltaTime;

            // Degrade stamina every 10 seconds
            if (staminaDegradeTimer >= 10f)
            {
                effectiveMaxSpeedFactor = Mathf.Max(0.5f, effectiveMaxSpeedFactor - staminaDegradeRate);
                staminaDegradeTimer = 0f;
            }

            if (playerDistanceCalcTimer <= 0f)
            {
                if (navMeshAgent.CalculatePath(playerCharacterTransform.position, pathToPlayer))
                {
                    currentDistance = Mathf.Clamp(CalculatePathDistance(), minDistance, maxDistance);
                    spySpeed = Mathf.Lerp(spyBaseSpeed * spySpeedMultiplierMaximum * effectiveMaxSpeedFactor, spyBaseSpeed * spySpeedMultiplierMinimum, (currentDistance - minDistance) / (maxDistance - minDistance));
                    navMeshAgent.speed = _keepSpeedAtZero ? 0f : spySpeed;
                    playerDistanceCalcTimer = playerDistanceCalcInterval;
                }
            }
        }
        if (navMeshAgent.isOnOffMeshLink)
        {
            OffMeshLinkData previousLink = currentLink;
            currentLink = navMeshAgent.currentOffMeshLinkData;
            if (linkHasCurve)
            {
                if(!previousLink.Equals(currentLink))
                    StartCoroutine(TraverseClimbLink());
            }
            else
            {
                //calculate the final point of the link
                Vector3 endPos = currentLink.endPos + Vector3.up * navMeshAgent.baseOffset;

                //Move the navMeshAgent to the end point
                navMeshAgent.transform.position = Vector3.MoveTowards(navMeshAgent.transform.position, endPos, navMeshAgent.speed * Time.deltaTime);

                //when the navMeshAgent reach the end point you should tell it, and the navMeshAgent will "exit" the link and work normally after that
                if (navMeshAgent.transform.position == endPos)
                {
                    navMeshAgent.CompleteOffMeshLink();
                }
            }
            
        }
    }
    IEnumerator TraverseClimbLink()
    {
        climbTime = 0f;
        // Normalize time for the climb based on total duration
        float normalizedTime = 0f;
        Vector3 xzStart = new Vector3(currentLink.startPos.x, 0, currentLink.startPos.z);
        Vector3 xzEnd = new Vector3(currentLink.endPos.x, 0, currentLink.endPos.z);
        Vector3 xzPosition;
        Vector3 yStart = currentLink.startPos + Vector3.up * navMeshAgent.baseOffset;
        Vector3 yEnd = currentLink.endPos + Vector3.up * navMeshAgent.baseOffset;
        Debug.Log(yStart.y);
        Debug.Log(yEnd.y);
        while (normalizedTime < 1f)
        {
            climbTime += Time.deltaTime;
            normalizedTime = Mathf.Clamp01(climbTime / climbTimeTotalDuration);

            // Evaluate the curve to get progress along the XZ direction
            float progressXZ = climbSpeedCurveX.Evaluate(normalizedTime);
            float progressY = climbSpeedCurveY.Evaluate(normalizedTime);

            // Calculate the XZ position based on the direction vector

            xzPosition = Vector3.Lerp(xzStart, xzEnd, progressXZ);

            // Interpolate the Y position directly
            float yPosition = Mathf.Lerp(yStart.y, yEnd.y, progressY);

            // Combine XZ and Y into the final position
            navMeshAgent.transform.position = new Vector3(xzPosition.x, yPosition, xzPosition.z);
            yield return null;
        }
        Debug.Log("Completing");
        navMeshAgent.CompleteOffMeshLink();

    }
    public void SetSpeed(TraversalLinkTypes linkType)
    {
        linkHasCurve = false;
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
                linkHasCurve = true;
                climbTime = 0f;
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
            //Debug.Log($"Player found", playerCharacterTransform);
        }

        if (_lockDestination) return;
        destinationTransform = msg.State.spyDestination;
        if(!_destinationFound && destinationTransform != null)
        {
            _destinationFound = true;
            //Debug.Log($"Destination found", destinationTransform);
            StartCoroutine(nameof(SetDestination));
        }
    }

    protected override void Execute(KnockOutTheSpy msg)
    {
        if (isKnockedOut) return;
        
        isKnockedOut = true;
        navMeshAgent.enabled = false;

        if (animator != null)
        {
            animator.SetBool("Sprawl", true);
        }

        // Drop the briefcase when knocked out
        if (handBriefcase != null)
        {
            handBriefcase.gameObject.SetActive(false);
        }
    }

    protected override void Execute(StopTheSpy msg)
    {
        navMeshAgent.enabled = false;
        StopAllCoroutines();
    }

    public IEnumerator SetDestination()
    {
        while (navMeshAgent.enabled == false)
        {
            yield return null;
        }

        if (destinationTransform.position != navMeshAgent.destination)
        {
            Debug.Log($"Setting new destination", destinationTransform);
            if (navMeshAgent.SetDestination(destinationTransform.position))
            {
                transform.rotation = Quaternion.LookRotation(navMeshAgent.path.corners[0]-transform.position, Vector3.up);
                Message.Publish(new SpyNavigationCompleted());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInvincibleToPlayer)
            return;
        
        if (!hasBeenTagged && other.gameObject.CompareTag("Player"))
        {
            hasBeenTagged = true;
            Message.Publish(new BeginNarrativeSection(NarrativeSection.CaughtSpy));
        }
    }

    public void InitDestinationAndPlayerAndBeginRunning(Transform player, Transform destination)
    {
        _shouldStartRunningOnStart = true;
        _playerFound = true;
        playerCharacterTransform = player;
        _destinationFound = true;
        destinationTransform = destination;
        _lockDestination = true;
        _isInvincibleToPlayer = true;
        Debug.Log($"Initializing with player: {playerCharacterTransform}, destination: {destinationTransform}", destinationTransform);
        navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(destinationTransform.position);
    }

    public void KeepSpeedAtZero()
    {
        _keepSpeedAtZero = true;
    }

    public void AllowFullSpeed()
    {
        _keepSpeedAtZero = false;
    }
}

public class SpyNavigationCompleted
{
 
}
