using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ObstacleSpawnPool : ScriptableObject
{
    public ObstacleSpawnRule[] obstacles;
    
    public Queue<ObstacleSpawnRule> GetRandomObstacles(int n)
    {
        var weightedPool = new List<ObstacleSpawnRule>();
        
        // Create a weighted pool with at least 5 times n entries
        var totalEntries = Mathf.Max(5 * n, obstacles.Sum(rule => rule.weight));
        
        foreach (var rule in obstacles)
        {
            var entries = Mathf.RoundToInt((float)rule.weight / obstacles.Sum(r => r.weight) * totalEntries);
            for (var i = 0; i < entries; i++)
            {
                weightedPool.Add(rule);
            }
        }

        // Shuffle the weighted pool
        for (var i = weightedPool.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (weightedPool[i], weightedPool[j]) = (weightedPool[j], weightedPool[i]);
        }

        // Pick n random prefabs from the shuffled pool
        return weightedPool.Take(n).ToQueue();
    }
}
