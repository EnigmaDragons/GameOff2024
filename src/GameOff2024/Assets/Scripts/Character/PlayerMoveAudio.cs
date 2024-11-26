using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using NeoCC;
using Unity.Mathematics;
using System;
using NeoFPS.CharacterMotion;



public class PlayerMoveAudio : MonoBehaviour
{
    Rigidbody rb;
    public EventReference fsEvent;
    public EventReference fsLandEvent;
    public EventReference foleyEvent;
    public EventReference jumpFoleyEvent;
    public EventReference foleySlideEvent;
    public EventReference fsSlideEvent;
    EventInstance foleySlideEventInstance;
    EventInstance fsSlideEventInstance;
    INeoCharacterController characterController;
    bool waitForNextStep = false;
    MotionController mc;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mc = GetComponent<MotionController>();
        mc.onStep += Mc_onStep;
        mc.onCurrentStateChanged += Mc_onCurrentStateChanged;
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<INeoCharacterController>();
    }

    private void Mc_onCurrentStateChanged()
    {
        Debug.Log(mc.currentState.name);
        switch (mc.currentState.name)
        {
            case "Sprint":
                StopSlidingSound();
                break;
            case "Crouch":
                StopSlidingSound();
                break;
            case "Crouch Slide":
                TriggerSlidingSound(rb.linearVelocity.magnitude);
                break;
            case "Jump":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            case "Landing":
                TriggerLandingFootStep();
                StopSlidingSound();
                break;
            case "Ledge Climb":
                break;
            case "Jump Directional":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            case "Wall Run Across":
                StopSlidingSound();
                break;
            case "Wall Run Up":
                StopSlidingSound();
                break;
            case "Wall Slide Down":
                TriggerSlidingSound(rb.linearVelocity.magnitude);
                break;
            case "Push Off Hard":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            case "Ladder Climb":
                break;
            case "Ladder Push Off":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            case "Push Off Soft":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            default:
                break;
        }
    }

    private void Mc_onStep()
    {
        TriggerFootstep(rb.linearVelocity.magnitude);
        TriggerFoley(rb.linearVelocity.magnitude);
    }

    //private void CharacterController_onControllerHit(NeoCharacterControllerHit hit)
    //{
    //     Debug.Log("Player Hit");
    //   }

    // Update is called once per frame
    void Update()
    {    
        if (rb)
        {
            foleySlideEventInstance.setParameterByName("Char_Speed", rb.linearVelocity.magnitude);
            fsSlideEventInstance.setParameterByName("Char_Speed", rb.linearVelocity.magnitude);
        }
    }

    private void TriggerJumpSound()
    {
        EventInstance jumpFoleyEventInstance = RuntimeManager.CreateInstance(jumpFoleyEvent);
        RuntimeManager.AttachInstanceToGameObject(jumpFoleyEventInstance, gameObject);
        jumpFoleyEventInstance.start();
        jumpFoleyEventInstance.release();
    }

    private void TriggerSlidingSound(float charSpeed)
    {
        foleySlideEventInstance = RuntimeManager.CreateInstance(foleySlideEvent);
        RuntimeManager.AttachInstanceToGameObject(foleySlideEventInstance, gameObject);
        foleySlideEventInstance.setParameterByName("Char_Speed", charSpeed);
        foleySlideEventInstance.start();

        fsSlideEventInstance = RuntimeManager.CreateInstance(fsSlideEvent);
        RuntimeManager.AttachInstanceToGameObject(fsSlideEventInstance, gameObject);
        foleySlideEventInstance.setParameterByName("Char_Speed", charSpeed);
        fsSlideEventInstance.start();
        //Debug.Log("sliding");

    }

    private void StopSlidingSound()
    {
        foleySlideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        foleySlideEventInstance.release();
        fsSlideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        fsSlideEventInstance.release();
    }

    void TriggerLandingFootStep()
    {
        EventInstance fsLandEventInstance = RuntimeManager.CreateInstance(fsLandEvent);
        RuntimeManager.AttachInstanceToGameObject(fsLandEventInstance, gameObject);
        fsLandEventInstance.start();
        fsLandEventInstance.release();
    }

    void TriggerFootstep(float charSpeed)
    {
        EventInstance fsEventInstance = RuntimeManager.CreateInstance(fsEvent);
        RuntimeManager.AttachInstanceToGameObject(fsEventInstance, gameObject);
        fsEventInstance.setParameterByName("Char_Speed", charSpeed);
        fsEventInstance.start();
        fsEventInstance.release();
        //safety check on sliding sound, this sound should have stopped by now, stop it if its valid as it should be invalid
        if (foleySlideEventInstance.isValid()) { StopSlidingSound(); }
    }

    void TriggerFoley(float charSpeed)
    {
        EventInstance foleyEventInstance = RuntimeManager.CreateInstance(foleyEvent);
        RuntimeManager.AttachInstanceToGameObject(foleyEventInstance, gameObject);
        foleyEventInstance.setParameterByName("Char_Speed", charSpeed);
        foleyEventInstance.start();
        foleyEventInstance.release();
    }
}
