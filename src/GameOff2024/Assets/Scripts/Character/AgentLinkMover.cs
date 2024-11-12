using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class AgentLinkMover : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 1f;  // Max units per second

    private NavMeshAgent m_agent;
    NavMeshHit m_hit;
    [SerializeField] private GameObject visuals;
    Animator m_animator;

    private NavmeshJumpPath activeJumpPath;

    private bool isJumping;
    float agentBaseSpeed;
    [SerializeField] float jumpSpeed = 2;

    [SerializeField] float groundedCheckRadius = .5f;
    [SerializeField] LayerMask groundedCheckMask;
    bool isGrounded;
    [SerializeField] Transform groundedCheckPosition;

    private int slideMask = -1;
    private int climbMask = -1;

    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = visuals.GetComponent<Animator>();
    }

    private void Start()
    {
        agentBaseSpeed = m_agent.speed;
        slideMask = NavMesh.GetAreaFromName("Slide");
        climbMask = NavMesh.GetAreaFromName("Climb");
        Debug.Log($"Spy - Climb is {climbMask}, Slide is {slideMask}");
    }

    private void Update()
    {
        if (isJumping)
        {
            JumpBehaviour();
        }
        if (m_agent.isOnOffMeshLink)
        {
            Debug.Log($"Spy Movement is {m_hit.mask}");
            m_agent.SamplePathPosition(NavMesh.AllAreas, 0f, out m_hit);
            if (m_hit.mask == slideMask)
            {
                Debug.Log($"Spy - Started Slide");
                m_animator.SetBool("Slide", true);
            }
            else
            {
                m_animator.SetBool("Slide", false);
            }
            if (m_hit.mask == climbMask)
            {
                m_animator.SetBool("Climb", true);
                m_animator.SetFloat("YVelocity", m_agent.desiredVelocity.y);
            }
            else
            {
                m_animator.SetBool("Climb", false);
            }
        }
        else
        {
            m_animator.SetBool("Climb", false);
            m_animator.SetBool("Slide", false);
        }
    }
    void GroundedCheck()
    {
        isGrounded = Physics.CheckSphere(groundedCheckPosition.position, groundedCheckRadius);
        m_animator.SetBool("IsGrounded", isGrounded);
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

        if (isGrounded)
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

        // Check if we have reached the target height when there�s no active jump path

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundedCheckPosition.position, groundedCheckRadius);
    }
}
