using System;
using UnityEngine;

[Serializable]
public class ObstacleSpawnRule
{
    public ObstacleGameObject prefab;
    public int weight = 1;
    public int difficulty = 1;
    public bool useRandomCardinalRotation = false;
}

