using UnityEngine;

public class OnlyActiveWithinRangeOfPlayer : MonoBehaviour
{
    [SerializeField] private GameObject targets;
    [SerializeField] private GameObject[] otherTargets;
    [SerializeField] private float animationDistance = 40f;
    [SerializeField] private float updatePeriod = 0.25f;

    private float nextUpdateTime = 0f;
    
    private void Update()
    {
        if (Time.unscaledTime < nextUpdateTime) return;
        nextUpdateTime = Time.unscaledTime + updatePeriod;

        if (CurrentGameState.ReadOnly.playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, CurrentGameState.ReadOnly.playerTransform.position);
        bool shouldBeInRange = distanceToPlayer <= animationDistance;

        targets.SetActive(shouldBeInRange);
        foreach (GameObject otherTarget in otherTargets)
        {
            otherTarget.SetActive(shouldBeInRange);
        }
    }
}
