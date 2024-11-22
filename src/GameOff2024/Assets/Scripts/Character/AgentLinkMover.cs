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
    float visualsLocalY;
    Animator m_animator;

    private NavmeshJumpPath activeJumpPath;

    private bool isJumping;
    private bool isClimbing;
    private bool isSliding;
    private bool isJumpingDown;
    [SerializeField] float jumpSpeed = 2;
    [SerializeField] float slideSpeed = 5;
    [SerializeField] float climbSpeed = 5;

    [SerializeField] float groundedCheckRadius = .5f;
    [SerializeField] LayerMask groundedCheckMask;
    bool isGrounded;
    [SerializeField] Transform groundedCheckPosition;

    private int jumpMask = -1;
    private int slideMask = -1;
    private int climbMask = -1;
    private int jumpDownMask = -1;

    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = visuals.GetComponent<Animator>();
        controller = GetComponent<SpyController>();
    }

    private void Start()
    {
        jumpMask = NavMesh.GetAreaFromName("Jump");
        slideMask = NavMesh.GetAreaFromName("Slide");
        climbMask = NavMesh.GetAreaFromName("Climb");
        jumpDownMask = NavMesh.GetAreaFromName("Jump Down");
        visualsLocalY = visuals.transform.localPosition.y;
        Debug.Log($"Spy - Climb is {climbMask}, Slide is {slideMask}");
    }

    private void Update()
    {
        if (!m_agent.enabled)
        {
            return;
        }
        GroundedCheck();
        m_agent.SamplePathPosition(NavMesh.AllAreas, 0f, out m_hit);

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
        else if (isJumpingDown)
        {
            JumpingDownBehaviour();
        }
        else if (m_agent.isOnOffMeshLink)
        {
            Debug.Log($"Spy Movement is {m_hit.mask}");
            if (m_hit.mask == 1<<slideMask)
            {
                Debug.Log($"Spy - Started Slide");
                m_animator.SetBool("Slide", true);
                isSliding = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.sliding);
            }
            else if (m_hit.mask == 1<<climbMask)
            {
                m_animator.SetBool("Climb", true);
                isClimbing = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.climbing);
                Debug.Log("Started Climbing");
            }
            else if (m_hit.mask == 1 << jumpDownMask)
            {
                isJumpingDown = true;
                controller.SetSpeed(SpyController.TraversalLinkTypes.jumpingDown);

            }
            else if (m_hit.mask == 1 << jumpMask)
            {
                isJumping = true;
                m_animator.SetTrigger("Jump");
                controller.SetSpeed(SpyController.TraversalLinkTypes.jumping);

            }

        }
    }
    void GroundedCheck()
    {
        isGrounded = Physics.CheckSphere(groundedCheckPosition.position, groundedCheckRadius, groundedCheckMask);
        m_animator.SetBool("IsGrounded", isGrounded);
    }

    private void JumpBehaviour()
    {
        float targetY;

        if (activeJumpPath != null)
        {
            // Calculate the target y position along the parabola path
            targetY = visualsLocalY+0.01f+activeJumpPath.GetArcYCoordinate(transform.position);
        }
        else
        {
            // No active jump path; lerp toward ground level (y = 0)
            targetY = visualsLocalY;
        }

        // Calculate the current y position of visuals and the max allowed lerp distance
        float currentY = visuals.transform.localPosition.y;
        float maxLerpDistance = lerpSpeed * Time.deltaTime;

        // Lerp y position towards the target with clamping to avoid overshoot
        float newY = Mathf.MoveTowards(currentY, targetY, maxLerpDistance);

        if (m_hit.mask != 1 << jumpMask)
        {
            m_animator.ResetTrigger("Jump");
            newY = visualsLocalY;
            isJumping = false;
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);
        }

        // Update the visuals position
        visuals.transform.localPosition = new Vector3(
            0,
            newY,
            0
        );

        // Check if we have reached the target height when there�s no active jump path

    }
    private void SlidingBehaviour()
    {
        if (m_hit.mask != 1 << slideMask)
        {
            m_animator.SetBool("Slide", false);
            isSliding = false;
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);
        }
    }
    private void ClimbingBehaviour()
    {
        if (m_hit.mask != 1 << climbMask)
        {
            m_animator.SetBool("Climb", false);
            isClimbing = false;
            controller.SetSpeed(SpyController.TraversalLinkTypes.running);
        }
    }
    private void JumpingDownBehaviour()
    {
        if (m_hit.mask != 1 << jumpDownMask)
        {
            isJumpingDown = false;
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
                //isJumping = true;
                //controller.SetSpeed(SpyController.TraversalLinkTypes.jumping);
                //m_animator.SetTrigger("Jump");
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
