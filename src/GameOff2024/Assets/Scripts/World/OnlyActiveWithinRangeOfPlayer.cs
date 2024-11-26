using UnityEngine;

public class OnlyActiveWithinRangeOfPlayer : MonoBehaviour
{
    [SerializeField] SkinnedNpc npcChild;
    [SerializeField] private float animationDistance = 40f;
    [SerializeField] private float updatePeriod = 0.25f;

    private float nextUpdateTime = 0f;
    
    private void Update()
    {
        if (Time.unscaledTime < nextUpdateTime) return;
        nextUpdateTime = Time.unscaledTime + updatePeriod;

        if (CurrentGameState.ReadOnly.playerTransform == null) return;

        float distanceToPlayer = Vector3.SqrMagnitude(transform.position-CurrentGameState.ReadOnly.playerTransform.position);
        bool shouldBeInRange = distanceToPlayer <= animationDistance*animationDistance;

        npcChild.SetShouldAnimate(shouldBeInRange);
    }
}
