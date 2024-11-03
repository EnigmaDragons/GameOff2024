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

    private void Start()
    {
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

        var start = Time.timeSinceLevelLoad;
        Spawn();
        var elapsedMs = (Time.timeSinceLevelLoad - start) * 1000;
        Debug.Log($"Obstacle spawning took {elapsedMs}ms");
    }

    private void Spawn()
    {
        var count = Rng.Int(minSpawns, maxSpawns + 1);
        var toSpawn = new Queue<ObstacleSpawnRule>(spawnPool.GetRandomObstacles(count));
        
        int successfulPlacements = 0;
        int failedPlacements = 0;

        while (toSpawn.Count > 0)
        {
            var obs = toSpawn.Dequeue();
            var spawnPosition = GetValidSpawnPosition(obs);
            if (spawnPosition != Vector3.zero)
            {
                var obstacle = Instantiate(obs.prefab, spawnPosition, Quaternion.identity, transform);
                successfulPlacements++;
            }
            else
            {
                failedPlacements++;
            }
        }
    }

    private Vector3 GetValidSpawnPosition(ObstacleSpawnRule obs)
    {
        int attempts = 0;

        var tileBounds = _currentTile.Bounds;
        var bounds = obs.prefab.NavMeshObstacle.size;
       
        Debug.Log($"Obstacle bounds: {bounds}", obs.prefab);

        for (attempts = 0; attempts < 30; attempts++)
        {
            Vector3 randomPoint = GetRandomPointInBounds(tileBounds);
            Debug.Log($"Random Spawn point: {randomPoint} - Attempt {attempts}");
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                if (!Physics.CheckBox(hit.position, bounds / 2f, Quaternion.identity, LayerMask.GetMask("Obstacle")) && !IsInSafeZone(hit.position))
                {
                    Debug.Log($"Valid Spawn point: {hit.position} - Attempt {attempts}");
                    return hit.position;
                }
                Debug.Log($"Invalid Spawn point: {hit.position} - Attempt {attempts}");
            }
        }

        return Vector3.zero;
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
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
