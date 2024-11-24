using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class IntoxSimple : OnMessage<SetIntoxicationLevel>
{
    [Range(0, 1)]
    public float intoxicationLevel = 0;
    public float transitionSpeed = 1.0f;
    private float currentLevel = 0;

    private Volume volume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;

    void Start()
    {
        volume = GetComponent<Volume>();
        
        // Get or add post-processing effects
        if (!volume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration = volume.profile.Add<ChromaticAberration>();
        }

        if (!volume.profile.TryGet(out lensDistortion))
        {
            lensDistortion = volume.profile.Add<LensDistortion>();
        }

        if (!volume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments = volume.profile.Add<ColorAdjustments>();
        }

        if (!volume.profile.TryGet(out depthOfField))
        {
            depthOfField = volume.profile.Add<DepthOfField>();
        }

        SetupEffectParameters();
    }

    private void SetupEffectParameters()
    {
        // Enable all effects and their parameters
        chromaticAberration.intensity.overrideState = true;
        lensDistortion.intensity.overrideState = true;
        
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.postExposure.overrideState = true;

        depthOfField.mode.overrideState = true;
        depthOfField.focusDistance.overrideState = true;
        depthOfField.gaussianStart.overrideState = true;
        depthOfField.gaussianEnd.overrideState = true;

        EnableEffects();
    }

    private void EnableEffects()
    {
        chromaticAberration.active = true;
        lensDistortion.active = true;
        colorAdjustments.active = true;
        depthOfField.active = true;
    }

    private void DisableEffects()
    {
        // Disable effects
        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorAdjustments.active = false;
        depthOfField.active = false;

        // Disable overrides
        chromaticAberration.intensity.overrideState = false;
        lensDistortion.intensity.overrideState = false;
        
        colorAdjustments.saturation.overrideState = false;
        colorAdjustments.postExposure.overrideState = false;

        depthOfField.mode.overrideState = false;
        depthOfField.focusDistance.overrideState = false;
        depthOfField.gaussianStart.overrideState = false;
        depthOfField.gaussianEnd.overrideState = false;
    }

    void Update()
    {
        currentLevel = Mathf.Lerp(currentLevel, intoxicationLevel, Time.deltaTime * transitionSpeed);

        // Disable all effects if intoxication is 0
        if (Mathf.Approximately(currentLevel, 0f))
        {
            DisableEffects();
            return;
        }

        // Enable effects if they were disabled
        if (!chromaticAberration.active)
        {
            EnableEffects();
        }

        // Update effect intensities based on intoxication level
        chromaticAberration.intensity.value = currentLevel * 1f;
        
        // Subtle swaying effect with lens distortion
        float swayAmount = Mathf.Sin(Time.time * 0.5f) * currentLevel;
        lensDistortion.intensity.value = swayAmount;
        
        // Increase saturation and adjust exposure to match inspector values
        colorAdjustments.saturation.value = currentLevel * 20f;
        colorAdjustments.postExposure.value = currentLevel * 0.5f;
        
        // Add heavy blur effect
        depthOfField.mode.value = DepthOfFieldMode.Gaussian;
        depthOfField.focusDistance.value = 5f;
        depthOfField.gaussianStart.value = 5f - (currentLevel * 10f); 
        depthOfField.gaussianEnd.value = 15f + (currentLevel * 50f);
    }

    protected override void Execute(SetIntoxicationLevel msg)
    {
        intoxicationLevel = msg.Amount;
        Log.Info($"Intoxication Level Target = {msg.Amount}");
    }
}
