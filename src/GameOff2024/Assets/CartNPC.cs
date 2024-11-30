using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class CartNPC : MonoBehaviour
{
    [SerializeField] private SkinnedNpc character;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;
    Vector3 targetPosition;

    [SerializeField] private float topSpeed = 3f;
    [SerializeField] private float drivingForce = 20f;
    private float decelForce;
    float stopDistance = 5f;
    Vector3 targetVelocity;
    Rigidbody rb;

    int direction = 1;

    private void OnEnable()
    {
        rb = GetComponentInChildren<Rigidbody>();
        stopDistance = Vector3.Distance(startPosition.position, endPosition.position) / 4;
        targetPosition = endPosition.position;
        decelForce = -topSpeed*topSpeed / (2 * stopDistance);
        Debug.Log(decelForce);
    }

    private void FixedUpdate()
    {
        float distanceToTarget = Vector3.Distance(rb.position, targetPosition);

        // Apply force forward or backward based on the distance
        if (distanceToTarget > stopDistance)
        {
            rb.AddForce(direction * drivingForce * transform.forward, ForceMode.Acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, topSpeed);
            character.SetBool("isPushingCart", direction > 0);
            character.SetBool("isPullingCart", direction < 0);

        }
        else if (distanceToTarget < 0.1f)
        {
            // Switch target and reset state
            direction = -1 * direction;
            rb.MovePosition(targetPosition);
            targetPosition = direction > 0 ? endPosition.position : startPosition.position;
            rb.linearVelocity = Vector3.zero; // Stop completely
        }
        else
        {
            rb.AddForce(decelForce * direction * transform.forward * Time.deltaTime, ForceMode.Acceleration);
            character.SetBool("isPushingCart", direction < 0);
            character.SetBool("isPullingCart", direction > 0);
        }


        // Check if we've reached the target

    }
}
