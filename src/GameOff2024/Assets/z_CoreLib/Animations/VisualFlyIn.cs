using System.Collections;
using DG.Tweening;
using UnityEngine;

public class VisualFlyIn : MonoBehaviour
{
    [SerializeField] private float flyInDuration = 1.8f; 
    [SerializeField] private bool shouldGlide = true;
    [SerializeField] private float glideDuration = 1.8f;
    [SerializeField] private float glideDistance = 200;
    [SerializeField] private bool shouldFlyOut = true;
    [SerializeField] private float flyOutDuration = 1.8f;

    private Vector3 _initialPosition;
    private bool _isAnimating;
    private bool _fromRight;

    private void Awake()
    {
        _initialPosition = transform.position;
        _fromRight = _initialPosition.x > 0;
    }

    private void OnEnable() => StartCoroutine(Animate());

    private IEnumerator Animate()
    {
        if (_isAnimating)
            yield break;

        _isAnimating = true;
        transform.position = _initialPosition;
        var initialTarget = new Vector3(0, _initialPosition.y, _initialPosition.z);
        var glideOffset = (glideDistance / (shouldFlyOut ? 2 : 1)) * (_fromRight ? 1 : -1);
        if (shouldGlide)
            initialTarget += new Vector3(glideOffset, 0, 0);

        Sequence sequence = DOTween.Sequence();
        
        // Fly in with easing
        sequence.Append(transform.DOLocalMoveX(initialTarget.x, flyInDuration)
            .SetEase(Ease.InQuad));

        if (shouldGlide)
        {
            // Glide with matching easing
            sequence.Append(transform.DOLocalMoveX(-glideOffset, glideDuration)
                .SetEase(Ease.Linear));
        }

        if (shouldFlyOut)
        {
            // Fly out with matching easing
            sequence.Append(transform.DOLocalMoveX(-(_initialPosition.x + (_fromRight ? 1600 : -1600)), flyOutDuration)
                .SetEase(Ease.OutQuad));
        }

        yield return sequence.WaitForCompletion();
        Finish();
    }

    private void Finish()
    {
        _isAnimating = false;
    }
}
