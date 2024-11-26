using UnityEngine;

public class SkinnedNpc : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] renderers;
    [SerializeField] private float animationDistance = 36f;

    [SerializeField] private Animator activeAnimator;
    private SkinnedMeshRenderer activeRenderer;
    private bool isInRange;

    public SkinnedMeshRenderer Renderer => activeRenderer; 

    public void SetShouldAnimate(bool shouldAnimate)
    {
        isInRange = shouldAnimate;
        activeAnimator.enabled = shouldAnimate;
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
