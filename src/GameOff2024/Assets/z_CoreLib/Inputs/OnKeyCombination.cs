using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OnKeyCombination : MonoBehaviour
{
    [SerializeField] private KeyCode[] keys;
    [SerializeField] private UnityEvent action;

    private bool triggered;
    private float cooldownTimer;
    private const float COOLDOWN_DURATION = 2.5f;

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (!triggered && cooldownTimer <= 0 && keys.All(Input.GetKey))
        {
            action.Invoke();
            triggered = true;
            cooldownTimer = COOLDOWN_DURATION;
        }
        else if (!keys.All(Input.GetKey))
        {
            triggered = false;
        }
    }
}
