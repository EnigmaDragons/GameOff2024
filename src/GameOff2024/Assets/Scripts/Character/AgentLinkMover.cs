using UnityEngine;
using UnityEngine.AI;

public class AgentLinkMover : MonoBehaviour
{
    NavMeshAgent m_agent;

    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
    }
    private void Start()
    {
        m_agent.autoTraverseOffMeshLink = false;
    }
}
