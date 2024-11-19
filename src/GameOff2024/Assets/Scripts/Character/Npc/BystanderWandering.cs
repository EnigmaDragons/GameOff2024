using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BystanderWandering : MonoBehaviour
{
    [SerializeField] private SkinnedNpc character;

    private NavMeshAgent agent;
    private float nextActionTime;
    private Vector3 startPosition;
    private int remainingWanderSegments;

    private const float MAX_WANDER_DISTANCE = 25f;
    private const float MIN_ACTION_TIME = 4f;
    private const float MAX_ACTION_TIME = 16f;
    private const float WALK_ACTION_WEIGHT = 0.6f;
    private const float IDLE_ACTION_WEIGHT = 0.2f;
    private const float TEXT_ACTION_WEIGHT = 0.2f;

    private void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        
        // Randomize initial state
        nextActionTime = Time.time + Random.Range(0f, MAX_ACTION_TIME);
        float actionRoll = Random.value;
        if (actionRoll < WALK_ACTION_WEIGHT)
        {
            WalkToRandomPoint();
        }
        else if (actionRoll < WALK_ACTION_WEIGHT + IDLE_ACTION_WEIGHT)
        {
            StopAndIdle();
        }
        else
        {
            StopAndText();
        }
    }

    private void Update()
    {
        if (Time.time >= nextActionTime)
        {
            if (remainingWanderSegments > 0)
            {
                remainingWanderSegments--;
                WalkToRandomPoint();
                nextActionTime = Time.time + Random.Range(MIN_ACTION_TIME, MAX_ACTION_TIME);
            }
            else
            {
                ChooseNextAction();
            }
        }

        character.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    private void ChooseNextAction()
    {
        // Random time until next action
        nextActionTime = Time.time + Random.Range(MIN_ACTION_TIME, MAX_ACTION_TIME);

        // Choose random action based on weights
        float actionRoll = Random.value;
        if (actionRoll < WALK_ACTION_WEIGHT)
        {
            remainingWanderSegments = Random.Range(0, 4); // 0-3 additional segments
            WalkToRandomPoint();
        }
        else if (actionRoll < WALK_ACTION_WEIGHT + IDLE_ACTION_WEIGHT)
        {
            StopAndIdle();
        }
        else
        {
            StopAndText();
        }
    }

    private void WalkToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * MAX_WANDER_DISTANCE;
        randomDirection += startPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, MAX_WANDER_DISTANCE, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void StopAndIdle()
    {
        agent.ResetPath();
        character.SetIsTexting(false);
    }

    private void StopAndText()
    {
        agent.ResetPath();
        character.SetIsTexting(true);
    }
}
