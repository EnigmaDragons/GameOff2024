using UnityEngine;

public class CameraEyeFadeIn : MonoBehaviour
{
    [SerializeField] private float openDuration = 2f;
    [SerializeField] private AnimationCurve openingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Material transitionMaterial;
    private float currentTime;
    private bool isOpening;
    private bool isEnabled = true;
    
    private void Awake()
    {
        // Create material that will render the eyelid effect
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        transitionMaterial = new Material(shader);
        transitionMaterial.hideFlags = HideFlags.HideAndDontSave;
        transitionMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transitionMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transitionMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        transitionMaterial.SetInt("_ZWrite", 0);
        transitionMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        
        // Start closed
        currentTime = 0;
        isOpening = false;
    }

    public void DisableEyeEffect()
    {
        isEnabled = false;
    }

    public void TriggerOpenAnimation()
    {
        isEnabled = true;
        isOpening = true;
        currentTime = 0;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!isEnabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (!isOpening)
        {
            RenderEyelidEffect(source, destination, 0); // Fully closed
            return;
        }

        if (currentTime < openDuration)
        {
            currentTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(currentTime / openDuration);
            float openAmount = openingCurve.Evaluate(normalizedTime);
            
            RenderEyelidEffect(source, destination, openAmount);
        }
        else
        {
            isEnabled = false;
            Graphics.Blit(source, destination);
        }
    }

    private void RenderEyelidEffect(RenderTexture source, RenderTexture destination, float openAmount)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, tempRT);
        
        GL.PushMatrix();
        GL.LoadOrtho();
        
        transitionMaterial.SetPass(0);
        
        GL.Begin(GL.TRIANGLES);
        GL.Color(new Color(0, 0, 0, 1));
        
        // Top eyelid
        float topY = Mathf.Lerp(0.5f, -0.1f, openAmount);
        GL.Vertex3(0, topY, 0);
        GL.Vertex3(1, topY, 0);
        GL.Vertex3(0, 1, 0);
        
        GL.Vertex3(1, topY, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        
        // Bottom eyelid
        float bottomY = Mathf.Lerp(0.5f, 1.1f, openAmount);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(0, bottomY, 0);
        
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, bottomY, 0);
        GL.Vertex3(0, bottomY, 0);
        
        GL.End();
        GL.PopMatrix();
        
        Graphics.Blit(tempRT, destination);
        RenderTexture.ReleaseTemporary(tempRT);
    }

    private void OnDestroy()
    {
        if (transitionMaterial != null)
        {
            Destroy(transitionMaterial);
        }
    }
}
