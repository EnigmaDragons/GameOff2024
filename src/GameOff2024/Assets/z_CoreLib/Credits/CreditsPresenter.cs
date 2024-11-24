using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CreditsPresenter : MonoBehaviour
{
    [SerializeField] private FloatReference delayBeforeStart = new FloatReference(4f);
    [SerializeField] private FloatReference delayBetween = new FloatReference(2.4f);
    [SerializeField] private Vector2 basePosition = new Vector2(960, 540);
    [SerializeField] private Vector2 positionVarianceRange = new Vector2(180, 240);
    [SerializeField] private AllCredits allCredits;
    [SerializeField] private CreditPresenter creditPresenter;
    [SerializeField] private FloatReference maxLifetimeOfCredit;
    [SerializeField] private UnityEvent onStart;
    [SerializeField] private UnityEvent onFinished;
    
    private void Start()
    {
        StartCoroutine(ShowNext());
    }
    
    private IEnumerator ShowNext()
    {
        onStart.Invoke();
        yield return new WaitForSeconds(delayBeforeStart);
        
        for (var i = 0; i < allCredits.Credits.Length; i++)
        {
            var offsetX = Random.Range(-positionVarianceRange.x, positionVarianceRange.x);
            var offsetY = Random.Range(-positionVarianceRange.y, positionVarianceRange.y);
            var position = new Vector3(offsetX, offsetY, 0) + new Vector3(960, 540, 0);
            var presenter = Instantiate(creditPresenter, position, Quaternion.identity, transform).Initialized(allCredits.Credits[i]);
            Destroy(presenter.gameObject, maxLifetimeOfCredit);
            yield return new WaitForSeconds(delayBetween);
        }

        onFinished.Invoke();
    }
}
