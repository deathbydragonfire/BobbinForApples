using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class UnderwaterEffectRendererFeature : ScriptableRendererFeature
{
    class UnderwaterEffectPass : ScriptableRenderPass
    {
        private Material material;
        private UnderwaterEffectVolume volumeComponent;
        
        private static readonly int ShallowColorID = Shader.PropertyToID("_ShallowColor");
        private static readonly int DeepColorID = Shader.PropertyToID("_DeepColor");
        private static readonly int TintStrengthID = Shader.PropertyToID("_TintStrength");
        private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
        private static readonly int WaveFrequencyID = Shader.PropertyToID("_WaveFrequency");
        private static readonly int WaveAmplitudeID = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int CausticsSpeedID = Shader.PropertyToID("_CausticsSpeed");
        private static readonly int CausticsScaleID = Shader.PropertyToID("_CausticsScale");
        private static readonly int CausticsIntensityID = Shader.PropertyToID("_CausticsIntensity");
        private static readonly int EffectIntensityID = Shader.PropertyToID("_EffectIntensity");

        public UnderwaterEffectPass(Material mat)
        {
            material = mat;
        }

        private class PassData
        {
            internal Material material;
            internal TextureHandle source;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            VolumeStack volumeStack = VolumeManager.instance.stack;
            volumeComponent = volumeStack.GetComponent<UnderwaterEffectVolume>();

            if (volumeComponent == null || volumeComponent.effectIntensity.value <= 0f)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            material.SetColor(ShallowColorID, volumeComponent.shallowColor.value);
            material.SetColor(DeepColorID, volumeComponent.deepColor.value);
            material.SetFloat(TintStrengthID, volumeComponent.tintStrength.value);
            material.SetFloat(WaveSpeedID, volumeComponent.waveSpeed.value);
            material.SetFloat(WaveFrequencyID, volumeComponent.waveFrequency.value);
            material.SetFloat(WaveAmplitudeID, volumeComponent.waveAmplitude.value);
            material.SetFloat(CausticsSpeedID, volumeComponent.causticsSpeed.value);
            material.SetFloat(CausticsScaleID, volumeComponent.causticsScale.value);
            material.SetFloat(CausticsIntensityID, volumeComponent.causticsIntensity.value);
            material.SetFloat(EffectIntensityID, volumeComponent.effectIntensity.value);

            TextureHandle source = resourceData.activeColorTexture;
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "_UnderwaterEffectTexture", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Underwater Effect Pass", out var passData))
            {
                passData.material = material;
                passData.source = source;
                
                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = destination;
        }
    }

    private UnderwaterEffectPass underwaterPass;
    private Material underwaterMaterial;

    public override void Create()
    {
        Shader shader = Shader.Find("Hidden/UnderwaterEffect");
        
        if (shader == null)
        {
            Debug.LogError("UnderwaterEffect shader not found!");
            return;
        }

        underwaterMaterial = CoreUtils.CreateEngineMaterial(shader);
        underwaterPass = new UnderwaterEffectPass(underwaterMaterial);
        
        underwaterPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (underwaterPass != null && underwaterMaterial != null)
        {
            renderer.EnqueuePass(underwaterPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (underwaterMaterial != null)
        {
            CoreUtils.Destroy(underwaterMaterial);
        }
    }
}
