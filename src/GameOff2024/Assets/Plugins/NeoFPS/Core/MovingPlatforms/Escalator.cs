using UnityEngine;

public class Escalator : MonoBehaviour
{
    [SerializeField] Transform upperStep;
    [SerializeField] Transform lowerStep;
    public float escalatorSpeed;
    public Vector3 escalatorUpDirection;

    // Set to 1 for up and -1 for down
    public int escalatorCurrentDirection = 1;

    private void Awake()
    {
        escalatorUpDirection  = upperStep.position - lowerStep.position;
    }

    public Vector3 GetEscalatorDisplacement()
    {
        return escalatorSpeed*escalatorCurrentDirection*escalatorUpDirection;
    }
}
