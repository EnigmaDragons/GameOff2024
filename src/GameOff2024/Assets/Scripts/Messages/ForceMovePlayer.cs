using System;
using UnityEngine;

public class ForceMovePlayer
{
    public Vector3 Destination { get; }
    public Action OnReached { get; }
    
    public ForceMovePlayer(Vector3 destination, Action onFinished)
    {
        Destination = destination;
        OnReached = onFinished;
    }
}
