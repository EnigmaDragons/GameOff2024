using System;
using UnityEngine;

public class ForceLookPlayer
{
    public Vector3 Target { get; }
    public Action OnFinished { get; }
    
    public ForceLookPlayer(Vector3 target, Action onFinished)
    {
        Target = target;
        OnFinished = onFinished;
    }
}
