using UnityEngine;

public class SkinnedNpc : MonoBehaviour
{
    [SerializeField] private Animator[] animators;
    
    private Animator activeAnimator;

    private void Awake()
    {
        if (animators == null || animators.Length == 0)
        {
            Debug.LogError("No animators assigned to SkinnedNpc");
            return;
        }

        // Pick a random animator
        int randomIndex = Random.Range(0, animators.Length);
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

    public void SetTrigger(string triggerName)
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetTrigger(triggerName);
        }
    }

    public void SetBool(string paramName, bool value)
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetBool(paramName, value);
        }
    }

    public void SetFloat(string paramName, float value)
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetFloat(paramName, value);
        }
    }

    public void SetInteger(string paramName, int value)
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetInteger(paramName, value);
        }
    }
}
