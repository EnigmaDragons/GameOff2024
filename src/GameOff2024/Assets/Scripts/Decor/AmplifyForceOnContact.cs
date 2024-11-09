using UnityEngine;

public class AmplifyForceOnContact : MonoBehaviour
{
    private readonly float _kickForce = 3f;
    private readonly float _upwardForce = 6f;

    private const int CharacterPhysicsLayer = 16;
    private const int CharacterControllersLayer = 13;
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a character
        if (collision.gameObject.layer is CharacterPhysicsLayer or CharacterControllersLayer)
        {
            var rb = GetComponent<Rigidbody>();
            
            // Calculate kick direction (away from character)
            var kickDirection = transform.position - collision.gameObject.transform.position;
            kickDirection.y = 0; // Zero out y to get horizontal direction
            kickDirection = kickDirection.normalized;
            
            // Add force in kick direction plus some upward force
            rb.AddForce((kickDirection * _kickForce + Vector3.up * _upwardForce), ForceMode.Impulse);
            Log.Info("Amplified Force!");
        }
    }
}
