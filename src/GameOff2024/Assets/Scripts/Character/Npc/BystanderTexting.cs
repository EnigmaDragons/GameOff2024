using UnityEngine;

public class BystanderTexting : MonoBehaviour
{
    public SkinnedNpc skinnedNpc;
    private float nextStateCheck;
    private bool isTexting;
    private const float MIN_INTERVAL = 2f;
    private const float MAX_INTERVAL = 4.5f;
    private const float STATE_CHANGE_CHANCE = 0.12f; // 75% chance to keep current state

    void Start()
    {
        nextStateCheck = Random.Range(MIN_INTERVAL, MAX_INTERVAL);
        isTexting = Random.value > 0.5f;
        UpdateAnimator();
    }

    void Update()
    {
        if (Time.time >= nextStateCheck)
        {
            // Check if we should change state
            if (Random.value < STATE_CHANGE_CHANCE)
            {
                isTexting = !isTexting;
                UpdateAnimator();
            }
            
            nextStateCheck = Time.time + Random.Range(MIN_INTERVAL, MAX_INTERVAL);
        }
    }

    private void UpdateAnimator()
    {
        skinnedNpc.SetBool("isTexting", isTexting);
    }
}
