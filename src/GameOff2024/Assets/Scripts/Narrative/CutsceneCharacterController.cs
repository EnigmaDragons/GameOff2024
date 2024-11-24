using System;
using NeoCC;
using NeoFPS.CharacterMotion;
using UnityEngine;

public class CutsceneCharacterController : OnMessage<ForceMovePlayer, ForceLookPlayer, EnablePlayerControls, PlayerHoldBriefcase, PlayerReleaseBriefcase>
{
    [SerializeField] private NeoCharacterController characterController;
    [SerializeField] private MotionController motionController;
    [SerializeField] private Transform briefcaseObject;
    
    private Vector3 cutsceneDestination;
    private bool cutsceneActive = false;
    private bool forceLookActive = false;
    
    private Action onReachedDestination;
    private Action onLookComplete;
    private Vector3 lookTarget;

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
        else if (forceLookActive)
        {
            moveVector = Vector3.zero;
            
            // Calculate direction to look target
            Vector3 directionToTarget = (lookTarget - transform.position).normalized;
            directionToTarget.y = 0; // Keep look direction level with ground
            
            // Get target rotation
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Smoothly interpolate rotation
            float turnSpeed = 180f; // Degrees per second
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );
            
            // Apply to character controller
            characterController.Teleport(transform.position, transform.rotation, false);
            
            // Check if we're looking at target
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                onLookComplete?.Invoke();
                forceLookActive = false;
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
        if (!cutsceneActive && !forceLookActive)
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

    protected override void Execute(ForceLookPlayer msg)
    {
        characterController.enabled = true;
        forceLookActive = true;
        lookTarget = msg.Target;
        onLookComplete = msg.OnFinished;
        
        characterController.SetMoveCallback(CutsceneMoveCallback, OnCutsceneMoved);
        characterController.collisionsEnabled = false;
        characterController.ignoreExternalForces = true;
    }

    protected override void Execute(EnablePlayerControls msg)
    {
        cutsceneActive = false;
        forceLookActive = false;
        characterController.collisionsEnabled = true;
        characterController.ignoreExternalForces = false;
        motionController.Reconnect();
    }

    protected override void Execute(PlayerHoldBriefcase msg)
    {
        briefcaseObject.gameObject.SetActive(true);
    }

    protected override void Execute(PlayerReleaseBriefcase msg)
    {
        briefcaseObject.gameObject.SetActive(false);
    }
}
