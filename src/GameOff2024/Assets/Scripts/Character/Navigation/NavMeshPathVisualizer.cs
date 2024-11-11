using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathVisualizer : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private bool showPath = true;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private bool showInGame = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        if (!showPath || agent == null) return;
        if (!agent.hasPath)
        {
            return;
        }
        Debug.Log("Path found");


        Vector3[] corners = agent.path.corners;

        // Draw lines between each corner
        for (int i = 0; i < corners.Length - 1; i++)
        {
            // Draw in scene view
            Gizmos.color = pathColor;
            Gizmos.DrawLine(corners[i], corners[i + 1]);

            // Draw in game view if enabled
            if (showInGame)
            {
                Debug.DrawLine(corners[i], corners[i + 1], pathColor, 0, false);
            }

            // Optional: Draw spheres at corner points for better visualization
            Gizmos.DrawWireSphere(corners[i], lineWidth);
        }

        // Draw sphere at the last corner
        if (corners.Length > 0)
        {
            Gizmos.DrawWireSphere(corners[corners.Length - 1], lineWidth);
        }
    }

    private void Update()
    {
        if (!showInGame) return;
        if (!agent.hasPath) return;
        
        Vector3[] corners = agent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Debug.DrawLine(corners[i], corners[i + 1], pathColor);
        }
    }
}