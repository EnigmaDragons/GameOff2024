using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class AgentLinkMover : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 1f;  // Max units per second

    private NavMeshAgent m_agent;
    [SerializeField] private GameObject visuals;
    Animator m_animator;

    private NavmeshJumpPath activeJumpPath;

    private bool isJumping;
    float agentBaseSpeed;
    [SerializeField] float jumpSpeed = 2;

    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = visuals.GetComponent<Animator>();
    }

    private void Start()
    {
        agentBaseSpeed = m_agent.speed;
    }

    private void Update()
    {
        if (isJumping)
        {
            JumpBehaviour();
        }
        if (m_agent.isOnOffMeshLink)
        {
            Debug.Log("Link");
        }
    }

    private void JumpBehaviour()
    {
        float targetY;

        if (activeJumpPath != null)
        {
            // Calculate the target y position along the parabola path
            targetY = activeJumpPath.GetArcYCoordinate(visuals.transform.position);
        }
        else
        {
            // No active jump path; lerp toward ground level (y = 0)
            targetY = 0f;
        }

        // Calculate the current y position of visuals and the max allowed lerp distance
        float currentY = visuals.transform.position.y;
        float maxLerpDistance = lerpSpeed * Time.deltaTime;

        // Lerp y position towards the target with clamping to avoid overshoot
        float newY = Mathf.MoveTowards(currentY, targetY, maxLerpDistance);

        if (activeJumpPath == null && Mathf.Approximately(newY, targetY))
        {
            m_animator.SetTrigger("Land");
            m_animator.ResetTrigger("Jump");
            newY = 0;
            isJumping = false;  // Stop jumping once visuals are at y = 0
            m_agent.speed = agentBaseSpeed;
        }
        // Update the visuals position
        visuals.transform.position = new Vector3(
            visuals.transform.position.x,
            newY,
            visuals.transform.position.z
        );

        // Check if we have reached the target height when there’s no active jump path

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NavmeshJumpPath navmeshJumpPath))
        {
            if (activeJumpPath == null)
            {
                activeJumpPath = navmeshJumpPath;
                isJumping = true;
                m_agent.speed = jumpSpeed;
                m_animator.SetTrigger("Jump");
                //m_animator.SetFloat("Random Jump Float", Random.Range(1, 4));
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(activeJumpPath != null && other.TryGetComponent(out NavmeshJumpPath path) && path == activeJumpPath)
        {
            activeJumpPath = null;
        }
    }
}
