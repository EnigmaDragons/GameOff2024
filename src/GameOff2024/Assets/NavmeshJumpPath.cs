using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavmeshJumpPath : MonoBehaviour
{
    [SerializeField] private Transform pathStart;
    [SerializeField] private Transform pathEnd;
    [SerializeField] private float pathHeight = 2.0f;
    [SerializeField] private float pathWidth = 0.1f;
    [SerializeField] private int arcResolution = 20;  // Number of points in the arc for smoother curves
    [SerializeField] private int lineDensity = 5;     // Number of parallel lines to simulate width

    [SerializeField] BoxCollider JumpRegionTrigger;

    Vector3 startToEnd;
    float totalDistance;

    private void Start()
    {
        startToEnd = pathEnd.position - pathStart.position;
        totalDistance = startToEnd.magnitude;
        startToEnd.Normalize();
        JumpRegionTrigger.center = new Vector3(JumpRegionTrigger.center.x, pathHeight, JumpRegionTrigger.center.z);
        JumpRegionTrigger.size = new Vector3(totalDistance, pathHeight, pathWidth);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (pathStart == null || pathEnd == null)
            return;

        // Calculate arc points
        Vector3[] arcPoints = CalculateArcPoints(pathStart.position, pathEnd.position, pathHeight, arcResolution);

        // Draw multiple parallel lines to simulate width
        Handles.color = new Color(0, 0.5f, 1f, 0.5f);  // Transparent blue for the arc
        float widthOffset = pathWidth / lineDensity;

        for (int i = -lineDensity / 2; i <= lineDensity / 2; i++)
        {
            Vector3 offset = Vector3.Cross((pathEnd.position - pathStart.position).normalized, Vector3.up) * i * widthOffset;
            Vector3[] offsetArcPoints = new Vector3[arcPoints.Length];
            for (int j = 0; j < arcPoints.Length; j++)
            {
                offsetArcPoints[j] = arcPoints[j] + offset;
            }
            Handles.DrawAAPolyLine(offsetArcPoints);
        }
#endif
    }

    private Vector3[] CalculateArcPoints(Vector3 start, Vector3 end, float height, int resolution)
    {
        Vector3[] points = new Vector3[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            points[i] = Vector3.Lerp(start, end, t) + Vector3.up * CalculateParabolaPoint(height, t);
        }
        return points;
    }

    private float CalculateParabolaPoint(float height, float t)
    {
        return Mathf.Sin(t * Mathf.PI) * height;

    }

    public float GetArcYCoordinate(Vector3 position)
    {
        // Project the position onto the line between start and end to find t
        Vector3 startToPosition = position - pathStart.position;
        float projection = Vector3.Dot(startToPosition, startToEnd.normalized);
        
        float t = Mathf.Clamp01(projection / totalDistance);

        // Get the y-coordinate using CalculateParabolaPoint
        return CalculateParabolaPoint(pathHeight, t);
    }
}
