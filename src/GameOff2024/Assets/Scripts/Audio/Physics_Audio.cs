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
    //defines the threshold when a collision is considered significant enough to play a sound
    public float collisionThreshold = 0.01f;
    private float lastCollsionTime = 0f;
    //The time at which we consider a subsequent impact to be the start of a sliding motion and not an impact
    private static readonly float timerThresholdForSlide = 0.1f;
    //a collision large enough while sliding to deserve another impact sound
    private static readonly float slideBumpThreshold = 30f;
    private Rigidbody rb;
    //a buffer for storing previous angular velocities
    private List<float> angularVelBuffer = new List<float> ();
    private static int maxCollsion = 5;
    private int collisionCounter = 0;
    private static float slideTimeBuffer = 0.2f;
    private bool slideSFXPlaying = false;
    private EventInstance PhysicsSlideEventInstance;
    private float lastVelocity = 0f;
    private bool collsionEnabled = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collsionEnabled)
        {
            if (collision.relativeVelocity.sqrMagnitude > collisionThreshold)
            {
                CollsionHandler(collision);
            }
        }        
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collsionEnabled)
        {
            if (collision.relativeVelocity.sqrMagnitude > collisionThreshold)
            {
                CollsionHandler(collision);
            }
        }            
    }

    //used to optimize the triggering of physics sounds until the player is actually near those objects
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            collsionEnabled = true;
        }        
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            collsionEnabled = false;
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
        if (angularVelBuffer.Max() - rb.angularVelocity.sqrMagnitude > 30f)
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
