using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using DunGen;
using Unity.AI.Navigation;
using Random = UnityEngine.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private int minSpawns = 7;
    [SerializeField] private int maxSpawns = 33;
    [SerializeField] private ObstacleSpawnPool spawnPool;
    [SerializeField] private BoxCollider safeZone;

    private Tile _currentTile;
    private NavMeshSurface _navMeshSurface;
    private LayerMask _collisionMask;
    private readonly Collider[] _overlapResults = new Collider[8];

    private void Start()
    {
        _collisionMask = LayerMask.GetMask("Obstacle", "EnvironmentRough", "EnvironmentDetail");
        
        _currentTile = GetComponent<Tile>();
        if (_currentTile == null)
        {
            Debug.LogError("EnemyTileSpawner must be attached to a Tile object.");
            return;
        }

        _navMeshSurface = GetComponentInChildren<NavMeshSurface>();
        if (_navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface not found in the scene.");
            return;
        }

        // Wait a frame to ensure NavMesh is built
        StartCoroutine(SpawnNextFrame());
    }

    private IEnumerator SpawnNextFrame()
    {
        yield return null; // Wait one frame
        
        var start = Time.timeSinceLevelLoad;
        Spawn();
        var elapsedMs = (Time.timeSinceLevelLoad - start) * 1000;
        Debug.Log($"Obstacle spawning took {elapsedMs}ms");
    }

    private void Spawn()
    {
        if (!NavMesh.SamplePosition(transform.position, out _, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogError("No NavMesh found at spawn point - has the NavMesh been built?");
            return;
        }

        var count = Rng.Int(minSpawns, maxSpawns + 1);
        var toSpawn = new Queue<ObstacleSpawnRule>(spawnPool.GetRandomObstacles(count));
        
        int successfulPlacements = 0;
        int failedPlacements = 0;
        int maxRetries = 3; // Number of retries per obstacle

        while (toSpawn.Count > 0)
        {
            var obs = toSpawn.Dequeue();
            if (obs.prefab == null || obs.prefab.NavMeshObstacle == null)
            {
                Debug.LogError($"Invalid obstacle prefab or missing NavMeshObstacle component");
                failedPlacements++;
                continue;
            }

            bool placed = false;
            for (int retry = 0; retry < maxRetries && !placed; retry++)
            {
                if (TrySpawnObstacle(obs))
                {
                    successfulPlacements++;
                    placed = true;
                }
            }
            
            if (!placed)
            {
                failedPlacements++;
            }
        }
        
        Debug.Log($"Placed {successfulPlacements} obstacles, failed to place {failedPlacements}");
    }

    private bool TrySpawnObstacle(ObstacleSpawnRule obs)
    {
        var tileBounds = _currentTile.Bounds;
        var objBounds = obs.prefab.PlacementCollider.bounds;
        
        // Get rotation based on settings - use 90-degree increments for better alignment
        float yRotation = obs.useRandomCardinalRotation ? Random.Range(0, 4) * 90f : 0;
        Quaternion rotation = Quaternion.Euler(0, yRotation + transform.eulerAngles.y, 0);

        // Try more direct random placement first
        for (int quickAttempt = 0; quickAttempt < 10; quickAttempt++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(tileBounds.min.x, tileBounds.max.x) + _currentTile.transform.position.x,
                transform.position.y,
                Random.Range(tileBounds.min.z, tileBounds.max.z) + _currentTile.transform.position.z
            );

            // Sample nearest point on NavMesh
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                randomPosition = hit.position;
                
                // Final collision check
                Vector3 checkSize = objBounds.size * 1.1f; // 10% buffer
                int numColliders = Physics.OverlapBoxNonAlloc(randomPosition, checkSize/2f, _overlapResults, rotation, _collisionMask);
                
                if (numColliders == 0 && !IsInSafeZone(randomPosition))
                {
                    var obstacle = Instantiate(obs.prefab, randomPosition, rotation, transform);
                    obstacle.GetComponent<Collider>().enabled = false;
                    return true;
                }
            }
        }
        
        // If quick attempts fail, fall back to more thorough Poisson disk sampling
        float minDistance = Mathf.Max(objBounds.size.x, objBounds.size.z) * 1.2f; // Slightly reduced spacing
        int gridSize = Mathf.CeilToInt(Mathf.Max(tileBounds.size.x, tileBounds.size.z) / (minDistance / Mathf.Sqrt(2)));
        
        Vector2?[,] grid = new Vector2?[gridSize, gridSize];
        List<Vector2> activePoints = new List<Vector2>();
        List<Vector2> points = new List<Vector2>();

        // Start with point near center with smaller edge buffer
        float edgeBuffer = 0.1f; // Reduced buffer from edges
        Vector2 firstPoint = new Vector2(
            Random.Range(tileBounds.min.x + tileBounds.size.x * edgeBuffer, tileBounds.max.x - tileBounds.size.x * edgeBuffer),
            Random.Range(tileBounds.min.z + tileBounds.size.z * edgeBuffer, tileBounds.max.z - tileBounds.size.z * edgeBuffer)
        );
        
        activePoints.Add(firstPoint);
        points.Add(firstPoint);
        
        int cellX = Mathf.FloorToInt((firstPoint.x - tileBounds.min.x) * gridSize / tileBounds.size.x);
        int cellY = Mathf.FloorToInt((firstPoint.y - tileBounds.min.z) * gridSize / tileBounds.size.z);
        grid[cellX, cellY] = firstPoint;

        while (activePoints.Count > 0 && points.Count < 50) // Increased point limit
        {
            int randomIndex = Random.Range(0, activePoints.Count);
            Vector2 point = activePoints[randomIndex];
            bool foundValidPoint = false;

            for (int k = 0; k < 30; k++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float distance = Random.Range(minDistance, 2f * minDistance);
                Vector2 candidatePoint = point + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );

                if (!IsPointInBounds(candidatePoint, tileBounds))
                    continue;

                if (IsValidPoint(candidatePoint, points, minDistance, grid, gridSize, tileBounds))
                {
                    points.Add(candidatePoint);
                    activePoints.Add(candidatePoint);
                    
                    cellX = Mathf.FloorToInt((candidatePoint.x - tileBounds.min.x) * gridSize / tileBounds.size.x);
                    cellY = Mathf.FloorToInt((candidatePoint.y - tileBounds.min.z) * gridSize / tileBounds.size.z);
                    grid[cellX, cellY] = candidatePoint;
                    
                    foundValidPoint = true;
                    break;
                }
            }

            if (!foundValidPoint)
            {
                activePoints.RemoveAt(randomIndex);
            }
        }

        // Shuffle points to try them in random order
        for (int i = points.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = points[i];
            points[i] = points[j];
            points[j] = temp;
        }

        foreach (Vector2 point in points)
        {
            Vector3 position = new Vector3(
                point.x + _currentTile.transform.position.x,
                transform.position.y,
                point.y + _currentTile.transform.position.z
            );

            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                position = hit.position;

                Vector3 checkSize = objBounds.size * 1.1f;
                int numColliders = Physics.OverlapBoxNonAlloc(position, checkSize/2f, _overlapResults, rotation, _collisionMask);
                
                if (numColliders == 0 && !IsInSafeZone(position))
                {
                    var obstacle = Instantiate(obs.prefab, position, rotation, transform);
                    obstacle.GetComponent<Collider>().enabled = false;
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsPointInBounds(Vector2 point, Bounds bounds)
    {
        return point.x >= bounds.min.x && point.x <= bounds.max.x &&
               point.y >= bounds.min.z && point.y <= bounds.max.z;
    }

    private bool IsValidPoint(Vector2 candidate, List<Vector2> points, float minDist, Vector2?[,] grid, int gridSize, Bounds bounds)
    {
        int cellX = Mathf.FloorToInt((candidate.x - bounds.min.x) * gridSize / bounds.size.x);
        int cellY = Mathf.FloorToInt((candidate.y - bounds.min.z) * gridSize / bounds.size.z);

        // Check nearby cells only
        int searchRadius = 2;
        for (int i = -searchRadius; i <= searchRadius; i++)
        {
            for (int j = -searchRadius; j <= searchRadius; j++)
            {
                int neighborX = cellX + i;
                int neighborY = cellY + j;

                if (neighborX >= 0 && neighborX < gridSize && 
                    neighborY >= 0 && neighborY < gridSize && 
                    grid[neighborX, neighborY].HasValue)
                {
                    Vector2 neighbor = grid[neighborX, neighborY].Value;
                    if (Vector2.Distance(candidate, neighbor) < minDist)
                        return false;
                }
            }
        }
        return true;
    }
    
    private bool IsInSafeZone(Vector3 position)
    {
        if (safeZone != null)
        {
            return safeZone.bounds.Contains(position);
        }
        return false;
    }
}
