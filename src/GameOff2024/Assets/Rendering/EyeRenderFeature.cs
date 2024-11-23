using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EyeRenderFeature : ScriptableRendererFeature
{
    class EyeRenderPass : ScriptableRenderPass
    {
        private Material eyeMaterial;
        private CameraEyeFadeIn eyeFadeEffect;
        
        public EyeRenderPass(Material material)
        {
            eyeMaterial = material;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public void SetEffect(CameraEyeFadeIn effect)
        {
            eyeFadeEffect = effect;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (eyeFadeEffect == null)
            {
                Debug.LogWarning("EyeRenderPass: No eye fade effect found");
                return;
            }
            
            if (!eyeFadeEffect.ShouldRender())
            {
                Debug.Log("EyeRenderPass: Effect should not render");
                return;
            }

            Debug.Log($"EyeRenderPass: Executing render pass");
            CommandBuffer cmd = CommandBufferPool.Get("Eye Overlay");
            
            // Draw fullscreen quad with current eye state
            eyeFadeEffect.DrawToCommandBuffer(cmd, eyeMaterial);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private EyeRenderPass renderPass;
    private Material eyeMaterial;

    public override void Create()
    {
        // Use the same shader as CameraEyeFadeIn for consistency
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        eyeMaterial = new Material(shader);
        eyeMaterial.hideFlags = HideFlags.HideAndDontSave;
        eyeMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        eyeMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        eyeMaterial.SetInt("_Cull", (int)CullMode.Off);
        eyeMaterial.SetInt("_ZWrite", 0);
        eyeMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        
        renderPass = new EyeRenderPass(eyeMaterial);
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var effect = renderingData.cameraData.camera.GetComponent<CameraEyeFadeIn>();
        if (effect != null)
        {
            renderPass.SetEffect(effect);
            renderer.EnqueuePass(renderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (eyeMaterial != null)
        {
            CoreUtils.Destroy(eyeMaterial);
        }
    }
} 