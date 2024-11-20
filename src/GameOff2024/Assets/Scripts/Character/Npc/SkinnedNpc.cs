using UnityEngine;

public class SkinnedNpc : MonoBehaviour
{
    [SerializeField] private Animator[] animators;
    [SerializeField] private SkinnedMeshRenderer[] renderers;
    [SerializeField] private float animationDistance = 36f;

    private int _selectedIndex;
    private Animator activeAnimator;
    private SkinnedMeshRenderer activeRenderer;
    private bool isInRange;

    public SkinnedMeshRenderer Renderer => activeRenderer; 
    
    private void Awake()
    {
        if (animators == null || animators.Length == 0)
        {
            Debug.LogError("No animators assigned to SkinnedNpc");
            return;
        }

        // Pick a random animator
        int randomIndex = Random.Range(0, animators.Length);
        _selectedIndex = randomIndex;
        activeAnimator = animators[randomIndex];

        // Set active state for all animator GameObjects in one pass
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].gameObject.SetActive(i == randomIndex);
            }
        }
    }

    private void Update()
    {
        if (activeAnimator == null || CurrentGameState.ReadOnly.playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, CurrentGameState.ReadOnly.playerTransform.position);
        bool shouldBeInRange = distanceToPlayer <= animationDistance;

        if (shouldBeInRange != isInRange)
        {
            isInRange = shouldBeInRange;
            activeAnimator.enabled = isInRange;
        }
    }

    public void SetTrigger(string triggerName)
    {
        if (activeAnimator != null && isInRange)
        {
            activeAnimator.SetTrigger(triggerName);
        }
    }

    public void SetBool(string paramName, bool value)
    {
        if (activeAnimator != null && isInRange)
        {
            activeAnimator.SetBool(paramName, value);
        }
    }

    public void SetIsTexting(bool value)
    {
        SetBool("isTexting", value);
    }
    
    public void SetFloat(string paramName, float value)
    {
        if (activeAnimator != null && isInRange)
        {
            activeAnimator.SetFloat(paramName, value);
        }
    }

    public void SetInteger(string paramName, int value)
    {
        if (activeAnimator != null && isInRange)
        {
            activeAnimator.SetInteger(paramName, value);
        }
    }
}
