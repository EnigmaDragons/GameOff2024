using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using NeoCC;
using Unity.Mathematics;
using System;
using NeoFPS.CharacterMotion;
using System.Collections.Generic;



public class PlayerMoveAudio : MonoBehaviour
{
    Rigidbody rb;
    public EventReference fsEvent;
    public EventReference fsLandEvent;
    public EventReference foleyEvent;
    public EventReference jumpFoleyEvent;
    public EventReference foleySlideEvent;
    public EventReference fsSlideEvent;
    //all of the efforts for our player character
    public EventReference VOXJump;
    public EventReference VOXBump;
    public EventReference VOXBumpTable;
    public EventReference VOXClimb;
    public EventReference VOXKick;
    public EventReference VOXLand;
    public EventReference VOXBreathing;
    public EventInstance VOXBreathingInstance;
    EventInstance foleySlideEventInstance;
    EventInstance fsSlideEventInstance;
    MotionController mc;
    //we'll track how long the PC has been moving and then decide they're getting tired so start playing breathing sounds
    private float movetime = 0f;
    private bool breathingStart = false;
    private bool tiredSet = false;
    private bool exhaustedSet = false;
    private bool recoveringSet = false;
    private bool settlingSet = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mc = GetComponent<MotionController>();
        mc.onStep += Mc_onStep;
        mc.onCurrentStateChanged += Mc_onCurrentStateChanged;
        rb = GetComponent<Rigidbody>();
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
                TriggerClimbSound();
                break;
            case "Jump Directional":
                TriggerJumpSound();
                StopSlidingSound();
                break;
            case "Wall Run Across":
                StopSlidingSound();
                TriggerClimbSound();
                break;
            case "Wall Run Up":
                TriggerClimbSound();
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
                TriggerClimbLadderSound();
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

    private void TriggerClimbSound()
    {
        if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
        {
            EventInstance VOXClimbEventInstance = RuntimeManager.CreateInstance(VOXClimb);
            RuntimeManager.AttachInstanceToGameObject(VOXClimbEventInstance, gameObject);
            VOXClimbEventInstance.start();
            VOXClimbEventInstance.release();
        }            
    }

    private void TriggerClimbLadderSound()
    {
        if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
        {
            EventInstance VOXClimbEventInstance = RuntimeManager.CreateInstance(VOXClimb);
            RuntimeManager.AttachInstanceToGameObject(VOXClimbEventInstance, gameObject);
            VOXClimbEventInstance.start();
            VOXClimbEventInstance.release();
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
            //are we moving? lets start checking how long we've been moving for and if we aren't moving lets reverse the counter
            if(rb.linearVelocity.magnitude > 0.1)
            {
                movetime += Time.deltaTime;
            }
            //we are getting our breath back
            else
            {
                if (movetime > 0)
                {
                    movetime -= (Time.deltaTime * 4);
                }
                if (movetime >10f)
                {
                    if(!recoveringSet && breathingStart)
                    {
                        recoveringSet = true;
                        settlingSet = false;
                        VOXBreathingInstance.setParameterByNameWithLabel("CharTiredness", "Recovering");
                    }
                    
                }
                if(movetime < 10f)
                {
                    if(!settlingSet && breathingStart)
                    {
                        settlingSet = true;
                        recoveringSet = false;
                        VOXBreathingInstance.setParameterByNameWithLabel("CharTiredness", "Settling");
                    }
                    
                }
            }
            //we are getting tired
            if (movetime > 10f)
            {
                if (!breathingStart)
                {
                    breathingStart = true;
                    VOXBreathingInstance = RuntimeManager.CreateInstance(VOXBreathing);
                    VOXBreathingInstance.setParameterByNameWithLabel("CharTiredness", "Start");
                    RuntimeManager.AttachInstanceToGameObject(VOXBreathingInstance,gameObject);
                    VOXBreathingInstance.start();
                }
                if(movetime > 20f && movetime < 30f)
                {
                    if(!tiredSet)
                    {
                        tiredSet = true;
                        exhaustedSet = false;
                        VOXBreathingInstance.setParameterByNameWithLabel("CharTiredness", "Tired");
                    }                    
                }
                else if (movetime > 30f)
                {
                    if(!exhaustedSet)
                    {
                        exhaustedSet = true;
                        tiredSet = false;
                        VOXBreathingInstance.setParameterByNameWithLabel("CharTiredness", "Exhausted");
                    }
                }
            }
            else
            {
                if (breathingStart)
                {
                    breathingStart = false;
                    VOXBreathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    VOXBreathingInstance.release();
                }
            }
        }
    }

    private void TriggerJumpSound()
    {
        EventInstance jumpFoleyEventInstance = RuntimeManager.CreateInstance(jumpFoleyEvent);
        RuntimeManager.AttachInstanceToGameObject(jumpFoleyEventInstance, gameObject);
        jumpFoleyEventInstance.start();
        jumpFoleyEventInstance.release();

        if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
        {
            EventInstance VOXJumpEventInstance = RuntimeManager.CreateInstance(VOXJump);
            RuntimeManager.AttachInstanceToGameObject(VOXJumpEventInstance, gameObject);
            VOXJumpEventInstance.start();
            VOXJumpEventInstance.release();
        }            
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

        if(UnityEngine.Random.Range(0f,1f) > 0.7f)
        {
            EventInstance VOXClimbEventInstance = RuntimeManager.CreateInstance(VOXClimb);
            RuntimeManager.AttachInstanceToGameObject(VOXClimbEventInstance, gameObject);
            VOXClimbEventInstance.start();
            VOXClimbEventInstance.release();
        }      

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

        EventInstance VOXLandEventInstance = RuntimeManager.CreateInstance(VOXLand);
        RuntimeManager.AttachInstanceToGameObject(VOXLandEventInstance, gameObject);
        VOXLandEventInstance.start();
        VOXLandEventInstance.release();
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
