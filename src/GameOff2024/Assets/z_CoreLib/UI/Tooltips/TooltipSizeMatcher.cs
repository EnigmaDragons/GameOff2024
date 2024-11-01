using UnityEngine;

public class TooltipSizeMatcher : MonoBehaviour
{
    [SerializeField] private RectTransform src;
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private void Update() => target.sizeDelta = src.sizeDelta + offset;
}
