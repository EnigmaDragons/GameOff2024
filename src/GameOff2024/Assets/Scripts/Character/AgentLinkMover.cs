using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class AgentLinkMover : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 1f;  // Max units per second
    SpyController controller;
    private NavMeshAgent m_agent;
    NavMeshHit m_hit;
    [SerializeField] private GameObject visuals;
    Animator m_animator;

    private NavmeshJumpPath activeJumpPath;

    private bool isJumping;
    private bool isClimbing;
    private bool isSliding;
    [SerializeField] float jumpSpeed = 2;
    [SerializeField] float slideSpeed = 5;
    [SerializeField] float climbSpeed = 5;

    [SerializeField] float groundedCheckRadius = .5f;
    [SerializeField] LayerMask groundedCheckMask;
    bool isGrounded;
    [SerializeField] Transform groundedCheckPosition;

    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = visuals.GetComponent<Animator>();
        controller = GetComponent<SpyController>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        GroundedCheck();
        if (isJumping)
        {
            JumpBehaviour();
        }
        else if (isSliding)
        {
            SlidingBehaviour();
        }
        else if (isClimbing)
        {
            ClimbingBehaviour();
        }
        else if (m_agent.isOnOffMeshLink)
        {
            m_agent.SamplePathPosition(NavMesh.AllAreas, 0f, out m_hit);

            if (m_hit.mask == 1<<NavMesh.GetAreaFromName("Slide"))
            {
                m_animator.SetBool("Slide", true);
                isSliding = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.sliding);
            }
            else if (m_hit.mask == 1<<NavMesh.GetAreaFromName("Climb"))
            {
                m_animator.SetBool("Climb", true);
                m_animator.SetFloat("YVelocity", m_agent.desiredVelocity.y);
                isClimbing = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.climbing);

            }

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
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);

        }
        // Update the visuals position
        visuals.transform.position = new Vector3(
            visuals.transform.position.x,
            newY,
            visuals.transform.position.z
        );

        // Check if we have reached the target height when there’s no active jump path

    }
    private void SlidingBehaviour()
    {
        m_agent.SamplePathPosition(NavMesh.AllAreas, 0f, out m_hit);
        if (m_hit.mask != 1 << NavMesh.GetAreaFromName("Slide"))
        {
            m_animator.SetBool("Slide", false);
            isSliding = false;
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);
        }
    }
    private void ClimbingBehaviour()
    {
        m_agent.SamplePathPosition(NavMesh.AllAreas, 0f, out m_hit);
        if (m_hit.mask != 1 << NavMesh.GetAreaFromName("Climb"))
        {
            m_animator.SetBool("Climb", false);
            isClimbing = false;
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NavmeshJumpPath navmeshJumpPath))
        {
            if (activeJumpPath == null)
            {
                activeJumpPath = navmeshJumpPath;
                isJumping = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.jumping);
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
