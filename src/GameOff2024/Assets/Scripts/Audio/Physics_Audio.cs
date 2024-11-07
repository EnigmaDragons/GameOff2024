using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Physics_Audio : MonoBehaviour
{
    public enum SurfaceTypeAudio
    {
        Concrete,
        Glass,
        Metal,
        Wood
    }
    
    public SurfaceTypeAudio SurfaceType;
    public EventReference PhysicsImpactEvent;
    public EventReference PhysicsSlideEvent;
    float collisionThreshold = 0.01f;
    float lastCollsionTime = 0f;
    //The time at which we consider a subsequent impact to be the start of a sliding motion and not an impact
    float timerThresholdForSlide = 0.1f;
    //a collision large enough while sliding to deserve another impact sound
    float slideBumpThreshold = 20f;
    Rigidbody rb;
    //a buffer for storing previous angular velocities
    List<float> angularVelBuffer = new List<float> ();
    int maxCollsion = 10;
    int collisionCounter = 0;
    private float slideTimeBuffer = 0.2f;
    private bool slideSFXPlaying = false;
    EventInstance PhysicsSlideEventInstance;
    float lastVelocity = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > collisionThreshold)
        {
            CollsionHandler(collision);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.relativeVelocity.sqrMagnitude > collisionThreshold)
        {
            CollsionHandler(collision);
        }
    }

    void CollsionHandler(Collision collision)
    {
        //is an impact
        if (Time.time - lastCollsionTime > timerThresholdForSlide)
        {
            TriggerImpactAudio(collision);
        }
        else
        {
            HandleSlideSound(collision);
        }
        lastCollsionTime = Time.time;
    }

    private void Update()
    {
        if(Time.time - lastCollsionTime > slideTimeBuffer && slideSFXPlaying)
        {
            StopSlidingSound();
        }

    }

    private void StopSlidingSound()
    {
        slideSFXPlaying = false;
        PhysicsSlideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        PhysicsSlideEventInstance.release();        
    }

    private void HandleSlideSound(Collision collision)
    {
        
        //stores angular velocities in a buffer so we can consider changes over a number of collsions
        if(angularVelBuffer.Count < maxCollsion)
        {
            angularVelBuffer.Add(rb.angularVelocity.sqrMagnitude);
        }
        else
        {
            angularVelBuffer[collisionCounter % maxCollsion] = rb.angularVelocity.sqrMagnitude;
        }

        collisionCounter++;

        //has there been a large change in angular velocity over the last 10 collsions and or a big impact to deserve another impact sound
        if (angularVelBuffer.Max() - rb.angularVelocity.sqrMagnitude > 4f)
        {
            TriggerImpactAudio(collision);
        }
        else if (collision.relativeVelocity.sqrMagnitude > slideBumpThreshold)
        {
            TriggerImpactAudio(collision);
        }

        if (!slideSFXPlaying)
        {
            StartSlidingSound(collision);
            slideSFXPlaying = true;
        }
        else
        {
            UpdateSlidingSound(collision);
        }
    }

    private void UpdateSlidingSound(Collision collision)
    {
        if (lastVelocity != collision.relativeVelocity.sqrMagnitude)
        {
            PhysicsSlideEventInstance.setParameterByName("Phy_Impact_Velocity", collision.relativeVelocity.sqrMagnitude);
            PhysicsSlideEventInstance.setParameterByName("Phy_Angular_Velocity", rb.angularVelocity.sqrMagnitude);
        }
        lastVelocity = collision.relativeVelocity.sqrMagnitude;
    }

    private void StartSlidingSound(Collision collision)
    {
        PhysicsSlideEventInstance = RuntimeManager.CreateInstance(PhysicsSlideEvent);
        RuntimeManager.AttachInstanceToGameObject(PhysicsSlideEventInstance, gameObject);
        PhysicsSlideEventInstance.setParameterByName("Phy_Impact_Velocity", collision.relativeVelocity.sqrMagnitude);
        PhysicsSlideEventInstance.setParameterByName("Phy_Angular_Velocity", rb.angularVelocity.sqrMagnitude);
        PhysicsSlideEventInstance.start();
    }

    void TriggerImpactAudio(Collision collision)
    {
        EventInstance PhysicsImpactEventInstance = RuntimeManager.CreateInstance(PhysicsImpactEvent);
        RuntimeManager.AttachInstanceToGameObject(PhysicsImpactEventInstance, gameObject);
        PhysicsImpactEventInstance.setParameterByName("Phy_Impact_Velocity", collision.relativeVelocity.sqrMagnitude);
        PhysicsImpactEventInstance.start();
        PhysicsImpactEventInstance.release();
        //clear the buffer ready for ther next impact evaluation
        angularVelBuffer.Clear();
    }
}
