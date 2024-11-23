using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class IntoxicationEffectSettings
{
    [Range(0, 10)]
    public float blurAmount = 1.0f;
    [Range(0, 10)]
    public float swayAmount = 1.0f;
    [Range(0, 2)]
    public float swaySpeed = 1.0f;
    [Range(0, 1)]
    public float chromaticAberration = 0.2f;
    [Range(0, 2)]
    public float saturation = 1.2f;
    public bool animate = true;
    public float pulseSpeed = 1.0f;
    public float pulseAmount = 0.2f;
}

public class IntoxicationEffectRenderFeature : ScriptableRendererFeature
{
    public class IntoxicationEffectPass : ScriptableRenderPass
    {
        private Material effectMaterial;
        private RTHandle tempTexture;
        private RTHandle sourceHandle;
        public IntoxicationEffectSettings settings;
        private float startTime;
        private bool wasSetupCalled = false;
        private bool isFirstFrame = true;

        public IntoxicationEffectPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            tempTexture = RTHandles.Alloc("_TempTexture");
            startTime = Time.time;
        }

        public void Initialize(Material material)
        {
            effectMaterial = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            wasSetupCalled = true;
            
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempTexture");
            
            sourceHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (sourceHandle == null)
            {
                sourceHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Intoxication Effect");

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            // Update shader parameters
            float time = Time.time - startTime;
            float animatedBlur = settings.animate ? 
                settings.blurAmount * (1 + Mathf.Sin(time * settings.pulseSpeed) * settings.pulseAmount) : 
                settings.blurAmount;

            effectMaterial.SetFloat("_BlurAmount", animatedBlur);
            effectMaterial.SetFloat("_SwayAmount", settings.swayAmount);
            effectMaterial.SetFloat("_SwaySpeed", settings.swaySpeed);
            effectMaterial.SetFloat("_ChromaticAberration", settings.chromaticAberration);
            effectMaterial.SetFloat("_Saturation", settings.saturation);
            effectMaterial.SetFloat("_TimeOffset", time);

            cmd.GetTemporaryRT(Shader.PropertyToID(tempTexture.name), descriptor);
            
            // Blit with the effect
            cmd.Blit(sourceHandle, tempTexture, effectMaterial);
            cmd.Blit(tempTexture, sourceHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (effectMaterial == null)
            {
                Debug.LogError("Effect material is null!");
                return;
            }

            var cameraData = frameData.Get<UniversalCameraData>();
            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // Import the source texture
            TextureHandle sourceTexture;
            if (sourceHandle != null)
            {
                sourceTexture = renderGraph.ImportBackbuffer(sourceHandle);
            }
            else
            {
                sourceTexture = renderGraph.CreateTexture(new TextureDesc(desc)
                {
                    clearBuffer = false,
                    name = "Source Intoxication"
                });
            }

            TextureHandle tempTarget = renderGraph.CreateTexture(new TextureDesc(desc)
            {
                clearBuffer = false,
                name = "Temp Intoxication"
            });

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Intoxication Effect", out var passData))
            {
                // Setup pass data
                passData.effectMaterial = effectMaterial;
                passData.settings = settings;
                passData.startTime = startTime;

                // Declare texture usage
                builder.UseTexture(sourceTexture, AccessFlags.Read);
                builder.SetRenderAttachment(tempTarget, 0, AccessFlags.Write);
                builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);

                // Set render function
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    float time = Time.time - data.startTime;
                    float animatedBlur = data.settings.animate ? 
                        data.settings.blurAmount * (1 + Mathf.Sin(time * data.settings.pulseSpeed) * data.settings.pulseAmount) : 
                        data.settings.blurAmount;

                    data.effectMaterial.SetFloat("_BlurAmount", animatedBlur);
                    data.effectMaterial.SetFloat("_SwayAmount", data.settings.swayAmount);
                    data.effectMaterial.SetFloat("_SwaySpeed", data.settings.swaySpeed);
                    data.effectMaterial.SetFloat("_ChromaticAberration", data.settings.chromaticAberration);
                    data.effectMaterial.SetFloat("_Saturation", data.settings.saturation);
                    data.effectMaterial.SetFloat("_TimeOffset", time);

                    //Blitter.BlitCameraTexture(context.cmd, sourceTexture., tempTarget, data.effectMaterial, 0);
                    //Blitter.BlitCameraTexture(context.cmd, tempTarget, sourceTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.effectMaterial, 0);
                });
            }
        }

        private class PassData
        {
            public Material effectMaterial;
            public IntoxicationEffectSettings settings;
            public float startTime;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(tempTexture.name));
        }
    }

    public IntoxicationEffectSettings settings = new IntoxicationEffectSettings();
    private IntoxicationEffectPass effectPass;
    private Material effectMaterial;

    public override void Create()
    {
        if (effectMaterial == null)
        {
            var shader = Shader.Find("Hidden/Universal Render Pipeline/IntoxicationEffect");
            if (shader == null)
            {
                Debug.LogError("Failed to find IntoxicationEffect shader!");
                return;
            }
            effectMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        effectPass = new IntoxicationEffectPass();
        effectPass.Initialize(effectMaterial);
        effectPass.settings = settings;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (effectMaterial == null || !effectMaterial.shader.isSupported)
        {
            Debug.LogWarning("IntoxicationEffect cannot run: shader not supported or material missing");
            return;
        }

        renderer.EnqueuePass(effectPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(effectMaterial);
    }
}
