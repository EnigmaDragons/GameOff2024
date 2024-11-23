using System;
using NeoCC;
using UnityEngine;

public class CutsceneCharacterController : OnMessage<ForceMovePlayer>
{
    [SerializeField] private NeoCharacterController characterController;
    
    private Vector3 cutsceneDestination;
    private bool cutsceneActive = false;
    
    private Action onReachedDestination;

    public void StartCutsceneAndTravelToDestination(Vector3 destination, Action onReached)
    {
        characterController.enabled = true;
        cutsceneActive = true;
        cutsceneDestination = destination;
        onReachedDestination = onReached;
        
        // Set up movement control
        characterController.SetMoveCallback(CutsceneMoveCallback, OnCutsceneMoved);
        
        // Optional: Disable player control during cutscene
        characterController.collisionsEnabled = false;
        characterController.ignoreExternalForces = true;
    }

    private void CutsceneMoveCallback(out Vector3 moveVector, out bool applyGravity, out bool applyGroundForce)
    {
        if (cutsceneActive)
        {
            Vector3 direction = (cutsceneDestination - transform.position).normalized;
            moveVector = direction * Time.fixedDeltaTime * 10f; // Adjust speed as needed
            
            // Look towards destination using smooth rotation
            Vector3 lookDirection = direction;
            lookDirection.y = 0; // Keep look direction level with ground
            if (lookDirection != Vector3.zero)
            {
                // Get target rotation
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                
                // Smoothly interpolate rotation
                float turnSpeed = 360f; // Degrees per second
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation, 
                    turnSpeed * Time.fixedDeltaTime
                );
                
                // Apply to character controller
                characterController.Teleport(transform.position, transform.rotation, false);
            }

            
            // Check if we've reached the destination
            if (Vector3.Distance(transform.position, cutsceneDestination) < 0.1f)
            {
                onReachedDestination();
                cutsceneActive = false;
                moveVector = Vector3.zero;
            }
        }
        else
        {
            moveVector = Vector3.zero;
        }

        applyGravity = true;
        applyGroundForce = true;
    }

    private void OnCutsceneMoved()
    {
        if (!cutsceneActive)
        {
            // Cutscene complete, restore normal control
            characterController.collisionsEnabled = true;
            characterController.ignoreExternalForces = false;
            // Reset to normal player movement callback
            characterController.SetMoveCallback(null, null);
        }
    }

    protected override void Execute(ForceMovePlayer msg)
    {
        StartCutsceneAndTravelToDestination(msg.Destination, msg.OnReached);
    }
}