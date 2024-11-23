// Controller script to manage the effect

using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(UniversalAdditionalCameraData))]
public class IntoxicationEffectController : OnMessage<SetIntoxicationLevel>
{
    [Range(0, 1)]
    public float intoxicationLevel = 0;
    public float transitionSpeed = 1.0f;
    private float currentLevel = 0;
    
    private UniversalAdditionalCameraData cameraData;
    private IntoxicationEffectRenderFeature effectFeature;

    void Start()
    {
        cameraData = GetComponent<UniversalAdditionalCameraData>();
        
        // Find or create the effect feature using reflection since rendererFeatures is protected
        var rendererFeaturesField = cameraData.scriptableRenderer.GetType().GetProperty("rendererFeatures", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var rendererFeatures = (System.Collections.Generic.List<UnityEngine.Rendering.Universal.ScriptableRendererFeature>)
            rendererFeaturesField.GetValue(cameraData.scriptableRenderer);
        effectFeature = (IntoxicationEffectRenderFeature)rendererFeatures.Find(f => f is IntoxicationEffectRenderFeature);

        if (effectFeature == null)
        {
            // Create and add the feature if it doesn't exist
            effectFeature = ScriptableObject.CreateInstance<IntoxicationEffectRenderFeature>();
            rendererFeatures.Add(effectFeature);
            Log.Info("Created new Intoxication Effect Feature");
        }
    }

    void Update()
    {
        if (effectFeature == null)
        {
            Log.Warn("Missing Intoxication Effect Feature");
            return;
        }

        currentLevel = Mathf.Lerp(currentLevel, intoxicationLevel, Time.deltaTime * transitionSpeed);
        
        effectFeature.settings.blurAmount = currentLevel * 2.0f;
        effectFeature.settings.swayAmount = currentLevel * 0.1f;
        effectFeature.settings.chromaticAberration = currentLevel * 0.3f;
        effectFeature.settings.saturation = 1.0f + (currentLevel * 0.4f);
    }

    protected override void Execute(SetIntoxicationLevel msg)
    {
        currentLevel = msg.Amount;
    }
}
