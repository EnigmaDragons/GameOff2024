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
    private float lastCollsionTime = 0f;
    //The time at which we consider a subsequent impact to be the start of a sliding motion and not an impact
    private static readonly float timerThresholdForSlide = 0.05f;
    private Rigidbody rb;
    private static float slideTimeBuffer = 0.2f;
    private bool slideSFXPlaying = false;
    private EventInstance PhysicsSlideEventInstance;
    private float lastVelocity = 0f;
    private bool collisionEnabled = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionEnabled)
        {
            CollsionHandler(collision);
        }        
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collisionEnabled)
        {
            CollsionHandler(collision);
        }            
    }

    public void ToggleCollision(bool value)
    {
        collisionEnabled = value;
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
        PhysicsSlideEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(collision.GetContact(0).point));
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
    }
}
