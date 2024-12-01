using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PropSpawner : MonoBehaviour
{

    [SerializeField] private GameObject[] propPrefabs;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private int minSpawns = 3;
    [SerializeField] private int maxSpawns = 15;
    [SerializeField] private int maxAttempts = 10;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    private const bool ENABLE_LOGGING = false;

    private LayerMask _collisionMask;
    private readonly Collider[] _overlapResults = new Collider[8];

    private void Start()
    {
        spawnArea = GetComponentInChildren<TileTriggerVolume>()?.GetComponent<BoxCollider>();
        spawnParent = transform.Find("Interior");
        _collisionMask = LayerMask.GetMask("Obstacle", "EnvironmentRough", "EnvironmentDetail");
        if (ENABLE_LOGGING) Debug.Log($"Collision mask: {_collisionMask:X}");

        // Subscribe to navmesh baked event
        if (DunGen.Adapters.UnityNavMeshAdapter.instance != null)
        {
            DunGen.Adapters.UnityNavMeshAdapter.instance.OnNavmeshBaked += OnNavMeshBaked;
            if (ENABLE_LOGGING) Debug.Log("Successfully subscribed to NavMesh baked event");
        }
        else
        {
            Debug.LogWarning("UnityNavMeshAdapter instance is null!");
        }
    }

    private void OnDestroy()
    {
        if (DunGen.Adapters.UnityNavMeshAdapter.instance != null)
        {
            DunGen.Adapters.UnityNavMeshAdapter.instance.OnNavmeshBaked -= OnNavMeshBaked;
        }
    }

    private void OnNavMeshBaked(object sender, System.EventArgs e)
    {
        if (ENABLE_LOGGING) Debug.Log("NavMesh baked event received");
        StartCoroutine(SpawnNextFrame());
    }

    private IEnumerator SpawnNextFrame()
    {
        yield return null; // Wait one frame to ensure NavMesh is fully built

        var start = Time.timeSinceLevelLoad;
        SpawnWanderers();
        var elapsedMs = (Time.timeSinceLevelLoad - start) * 1000;
        if (ENABLE_LOGGING) Debug.Log($"Wanderer spawning took {elapsedMs}ms");
    }

    private void SpawnWanderers()
    {
        if (ENABLE_LOGGING) Debug.Log($"Starting spawn at position {transform.position}");
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit initialHit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogError($"No NavMesh found at spawn point. Areas checked: {NavMesh.AllAreas}");
            return;
        }
        if (ENABLE_LOGGING) Debug.Log($"Initial NavMesh sample successful at {initialHit.position}");

        var count = Random.Range(minSpawns, maxSpawns + 1);
        if (ENABLE_LOGGING) Debug.Log($"Attempting to spawn {count} wanderers");
        int successfulPlacements = 0;
        int failedPlacements = 0;

        for (int i = 0; i < count; i++)
        {
            if (TrySpawnWanderer())
            {
                successfulPlacements++;
            }
            else
            {
                failedPlacements++;
            }
        }

        if (ENABLE_LOGGING) Debug.Log($"Placed {successfulPlacements} wanderers, failed to place {failedPlacements}");
    }

    private bool TrySpawnWanderer()
    {
        if (propPrefabs.Length == 0)
        {
            Debug.LogWarning("No Spawn Prefabs Found", gameObject);
            return false;
        }
        
        // Try to find a valid spawn point on the NavMesh
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var prop = propPrefabs.Random();
            // Get random point in a square zone
            Vector3 randomPoint = GetRandomPointInsideCollider(spawnArea);
            randomPoint.y = 0;

            if (ENABLE_LOGGING) Debug.Log($"Attempt {attempt + 1}: Trying position {randomPoint}");

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position;
                if (ENABLE_LOGGING) Debug.Log($"Found NavMesh position at {spawnPos}");

                // Check for collisions
                int numColliders = Physics.OverlapSphereNonAlloc(spawnPos, 1f, _overlapResults, _collisionMask);

                if (numColliders == 0)
                {
                    // Spawn the wanderer
                    GameObject spawned = Instantiate(prop, randomPoint, Quaternion.Euler(0, Random.Range(0, 360), 0), spawnParent);
                    if (ENABLE_LOGGING) Debug.Log($"Successfully spawned wanderer at {spawnPos}");
                    return true;
                }
                else
                {
                    // Check if all colliders are tagged as Spawnable
                    bool allSpawnable = true;
                    for (int i = 0; i < numColliders; i++)
                    {
                        if (!_overlapResults[i].CompareTag("Spawnable"))
                        {
                            allSpawnable = false;
                            break;
                        }
                    }

                    if (allSpawnable)
                    {
                        // Spawn the wanderer since all collisions are with Spawnable objects
                        GameObject spawned = Instantiate(prop, randomPoint, Quaternion.Euler(0, Random.Range(0, 360), 0), spawnParent);
                        if (ENABLE_LOGGING) Debug.Log($"Successfully spawned wanderer at {spawnPos} (all collisions were Spawnable)");
                        return true;
                    }
                    else
                    {
                        if (ENABLE_LOGGING)
                        {
                            Debug.Log($"Position blocked by {numColliders} colliders:");
                            for (int i = 0; i < numColliders; i++)
                            {
                                Debug.Log($"- {_overlapResults[i].gameObject.name} on layer {LayerMask.LayerToName(_overlapResults[i].gameObject.layer)}", _overlapResults[i].gameObject);
                            }
                        }
                    }
                }
            }
            else
            {
                if (ENABLE_LOGGING) Debug.Log($"No NavMesh position found near {randomPoint}");
            }
        }

        Debug.LogWarning("Failed all spawn attempts for this wanderer");
        return false;
    }

    public Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );

        return boxCollider.transform.TransformPoint(point);
    }
}
