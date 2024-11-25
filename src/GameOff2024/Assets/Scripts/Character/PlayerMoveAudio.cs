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
    float walkStrideLength = 2f;
    float runStrideLength = 3f;
    float walkVelocity = 1f;
    Vector3 lastPosition;
    float totalDistance = 0f;
    StudioEventEmitter emitter;
    public EventReference fsEvent;
    public EventReference fsLandEvent;
    public EventReference foleyEvent;
    public EventReference jumpFoleyEvent;
    public EventReference foleySlideEvent;
    public EventReference fsSlideEvent;
    EventInstance foleySlideEventInstance;
    EventInstance fsSlideEventInstance;
    INeoCharacterController characterController;
    bool triggerLandingFS = false;
    float charHeight = 2f;
    bool waitForNextStep = false;
    MotionController mc;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mc = GetComponent<MotionController>();
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        characterController = GetComponent<INeoCharacterController>();
        characterController.onHeightChanged += CharacterController_onHeightChanged;
        //characterController.onControllerHit += CharacterController_onControllerHit;
    }

    //private void CharacterController_onControllerHit(NeoCharacterControllerHit hit)
    //{
   //     Debug.Log("Player Hit");
 //   }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(mc.currentState.name);

        if (rb)
        {
            
            if (characterController.isGrounded)
            {
                if (rb.linearVelocity.sqrMagnitude >= 0.1f && !triggerLandingFS && (charHeight != 1))
                {
                    if (rb.linearVelocity.sqrMagnitude >= walkVelocity * walkVelocity)
                    {
                        //running
                        if (totalDistance > runStrideLength + UnityEngine.Random.Range(-0.1f, 0.1f))
                        {
                            TriggerFootstep(rb.linearVelocity.magnitude);
                        }
                    }
                    else
                    {
                        //walking
                        if (totalDistance > walkStrideLength + UnityEngine.Random.Range(-0.1f, 0.1f))
                        {
                            TriggerFootstep(rb.linearVelocity.magnitude);
                        }
                    }
                    if (totalDistance < runStrideLength && !waitForNextStep)
                    {
                        TriggerFoley(rb.linearVelocity.magnitude);
                        waitForNextStep = true;
                    }
                    if(charHeight == 1)
                    {
                        foleySlideEventInstance.setParameterByName("Char_Speed", rb.linearVelocity.magnitude);
                        fsSlideEventInstance.setParameterByName("Char_Speed", rb.linearVelocity.magnitude);
                    }
                    totalDistance += Mathf.Abs((lastPosition - transform.position).magnitude);
                    lastPosition = transform.position;
                }
                else if(triggerLandingFS)
                {
                    TriggerLandingFootStep();
                }
            }
            else
            {
                if(!triggerLandingFS)
                {
                    TriggerJumpSound();
                }
                triggerLandingFS = true;
            }
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

    private void CharacterController_onHeightChanged(float newHeight, float rootOffset)
    {
        charHeight = newHeight;
        if (charHeight == 1) { TriggerSlidingSound(rb.linearVelocity.magnitude); }
        else { StopSlidingSound(); }
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
        triggerLandingFS = false;
        EventInstance fsLandEventInstance = RuntimeManager.CreateInstance(fsLandEvent);
        RuntimeManager.AttachInstanceToGameObject(fsLandEventInstance, gameObject);
        fsLandEventInstance.start();
        fsLandEventInstance.release();
    }

    void TriggerFootstep(float charSpeed)
    {
        //reset total distance so we start tracking the distance until the next stride
        totalDistance = 0f;
        waitForNextStep = false;
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
