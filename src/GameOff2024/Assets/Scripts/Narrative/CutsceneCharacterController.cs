using System;
using NeoCC;
using UnityEngine;

public class CutsceneCharacterController : MonoBehaviour 
{
    [SerializeField] private NeoCharacterController characterController;
    
    private Vector3 cutsceneDestination;
    private bool cutsceneActive = false;
    
    private Action onReachedDestination;

    public void StartCutsceneAndTravelToDestination(Vector3 destination, Action onReached)
    {
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
            moveVector = direction * Time.fixedDeltaTime * 5f; // Adjust speed as needed
            
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
}