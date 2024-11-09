using UnityEngine;

public class SpyControllerBreadcrumb : OnMessage<GameStateChanged>
{
    [SerializeField] float spySpeedMinimum = 6f;
    [SerializeField] float spySpeedMaximum = 18f;    
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 40f;
    [SerializeField] float playerDistanceCalcInterval = 0.5f;

    Transform playerCharacterTransform;
    Transform finalDestinationTransform;
    Transform currentDestinationTransform;

    float spySpeed;
    float currentDistance;

    bool _playerFound = false;
    bool _destinationFound = false;

    float playerDistanceCalcTimer = 0f;
    
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        // Initialization
        if (!_playerFound || !_destinationFound) {
            Log.Info("Spy - Player or destination not found, skipping update");
            return;
        }

        // Adjusting Speed
        playerDistanceCalcTimer -= Time.deltaTime;
        if (playerDistanceCalcTimer <= 0f)
        {
            currentDistance = Mathf.Clamp(GetDistanceFromPlayer(), minDistance, maxDistance);
            spySpeed = Mathf.Lerp(spySpeedMaximum, spySpeedMinimum, (currentDistance - minDistance) / (maxDistance - minDistance));
            playerDistanceCalcTimer = playerDistanceCalcInterval;
        }

        // Walking
        if (currentDestinationTransform != null)
        {
            Vector3 direction = (currentDestinationTransform.position - transform.position).normalized;
            Log.Info($"Spy - {direction} - Dest - {currentDestinationTransform.position}");
            _rb.MovePosition(transform.position + direction * (spySpeed * Time.fixedDeltaTime));
        }
    }

    private float GetDistanceFromPlayer()
    {
        return Vector3.Distance(transform.position, playerCharacterTransform.position);
    }

    protected override void Execute(GameStateChanged msg)
    {
        playerCharacterTransform = msg.State.playerTransform;
        if(playerCharacterTransform != null)
            _playerFound = true;
        finalDestinationTransform = msg.State.spyDestination;
        if(finalDestinationTransform != null) {
            currentDestinationTransform = finalDestinationTransform;
            _destinationFound = true;
        }
    }
}
