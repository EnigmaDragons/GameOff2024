using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEyeVignette : OnMessage<HideEyes, OpenEyes>
{
    [Header("Effect Settings")] [SerializeField]
    private float openDuration = 2f;

    [SerializeField] private AnimationCurve openingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool startClosed = true;
    [SerializeField] private float delaySeconds = 0f;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Color eyelidColor = Color.black;
    
    private bool isAnimating = false;
    private bool isOpen = false;
    private bool isEnabled = true;
    private float currentEyeAmount = 1f;
    private Vignette vignette;

    private void Awake()
    {
        Debug.Log("CameraEyeVignette: Awake called");
        
        Debug.Log($"CameraEyeVignette: Successfully found Camera component on {gameObject.name}");

        // Get vignette from post process volume
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out Vignette vig))
        {
            vignette = vig;
        }

        if (startClosed)
        {
            isOpen = false;
            isEnabled = true;
            if (vignette != null) vignette.intensity.Override(1f);
            Debug.Log("CameraEyeVignette: Starting closed");
        }
        else
        {
            isOpen = true;
            if (vignette != null) vignette.intensity.Override(0.2f);
            Debug.Log("CameraEyeVignette: Starting open");
        }
    }

    public void DisableEyeEffect()
    {
        isEnabled = false;
    }

    public void TriggerOpenAnimation()
    {
        Debug.Log($"CameraEyeVignette: TriggerOpenAnimation called (isAnimating: {isAnimating})");
        if (!isAnimating)
        {
            isEnabled = true;
            this.ExecuteAfterDelay(() => StartCoroutine(OpenEyes()), delaySeconds);
        }
    }

    private IEnumerator OpenEyes()
    {
        Debug.Log("CameraEyeVignette: Starting OpenEyes coroutine");
        isAnimating = true;
        float startTime = Time.time;
        float elapsedTime = 0f;

        float startValue = isOpen ? 0f : 1f;
        float endValue = isOpen ? 1f : 0f;
        float startVignette = isOpen ? 0.2f : 1f;
        float endVignette = isOpen ? 1f : 0.2f;

        while (elapsedTime < openDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / openDuration;
            float curveValue = openingCurve.Evaluate(normalizedTime);

            currentEyeAmount = Mathf.Lerp(startValue, endValue, curveValue);
            if (vignette != null)
            {
                vignette.intensity.Override(Mathf.Lerp(startVignette, endVignette, curveValue));
            }

            yield return null;
        }

        currentEyeAmount = endValue;
        if (vignette != null) vignette.intensity.Override(endVignette);
        isOpen = !isOpen;
        isAnimating = false;

        if (isOpen)
        {
            isEnabled = false;
        }
    }

    public bool ShouldRender() => isEnabled && (!isOpen || isAnimating);

    public void DrawToCommandBuffer(CommandBuffer cmd, Material material)
    {
        float amount = currentEyeAmount;

        // Set material properties with full alpha
        Color colorWithAlpha = eyelidColor;
        colorWithAlpha.a = 1f; // Ensure full opacity
        material.SetColor("_Color", colorWithAlpha); // Try both _Color and _BaseColor
        material.SetColor("_BaseColor", colorWithAlpha);

        Debug.Log($"Drawing vignette with amount: {amount}, color: {colorWithAlpha}");

        float screenHeight = Screen.height;
        float screenWidth = Screen.width;

        // Draw top eyelid
        float topY = Mathf.Lerp(screenHeight / 2, -screenHeight * 0.1f, amount);
        Matrix4x4 topMatrix = Matrix4x4.TRS(
            new Vector3(screenWidth / 2, topY + (screenHeight - topY) / 2, 0),
            Quaternion.identity,
            new Vector3(screenWidth, screenHeight - topY, 1)
        );
        cmd.DrawMesh(RenderingUtils.fullscreenMesh, topMatrix, material);

        // Draw bottom eyelid
        float bottomY = Mathf.Lerp(screenHeight / 2, screenHeight * 1.1f, amount);
        Matrix4x4 bottomMatrix = Matrix4x4.TRS(
            new Vector3(screenWidth / 2, bottomY / 2, 0),
            Quaternion.identity,
            new Vector3(screenWidth, bottomY, 1)
        );
        cmd.DrawMesh(RenderingUtils.fullscreenMesh, bottomMatrix, material);
    }

    protected override void Execute(HideEyes msg)
    {
        DisableEyeEffect();
    }

    protected override void Execute(OpenEyes msg)
    {
        TriggerOpenAnimation();
    }

    private void OnGUI()
    {
        if (Debug.isDebugBuild && false)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"Eye State: {(isOpen ? "Open" : "Closed")}");
            GUILayout.Label($"Is Animating: {isAnimating}");
            GUILayout.Label($"Is Enabled: {isEnabled}");
            GUILayout.Label($"Current Amount: {currentEyeAmount}");
            GUILayout.EndArea();
        }
    }
}