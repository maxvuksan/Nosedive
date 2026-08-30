using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// This Custom fog solution acts as a more customizable replacement for the default built in fog
/// </summary>
public class CustomFogPassFeature : ScriptableRendererFeature
{
    class CustomFogRenderPass : ScriptableRenderPass
    {
        private Material _mainFogMaterial;
        private FogProfile _fogProfile;
        private ComputeBuffer _computeBuffer;
        private FogDataStruct[] _dataArray = new FogDataStruct[1];

        public CustomFogRenderPass(Material material, FogProfile profile, ComputeBuffer computeBuffer)
        {
            _mainFogMaterial = material;
            _computeBuffer = computeBuffer;
            _fogProfile = profile;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mainFogMaterial == null || _fogProfile == null) return;

            var cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.isSceneViewCamera && !cameraData.postProcessEnabled) return;

            _dataArray[0] = _fogProfile.Data;
            _computeBuffer.SetData(_dataArray);
            Shader.SetGlobalConstantBuffer(Shader.PropertyToID("FogVariables"), _computeBuffer, 0, _computeBuffer.stride);

            var resourceData = frameData.Get<UniversalResourceData>();

            var destDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            destDesc.name = "_TempFogTarget";
            destDesc.clearBuffer = false;
            TextureHandle tempTarget = renderGraph.CreateTexture(destDesc);

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(
                resourceData.activeColorTexture, tempTarget, _mainFogMaterial, 0),
                "Custom Fog Pass");

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(
                tempTarget, resourceData.activeColorTexture, Blitter.GetBlitMaterial(TextureXR.dimension), 0),
                "Custom Fog Copy Back");
        }
    }


    public Material FogMaterial;
    public FogProfile FogProfile;
    private ComputeBuffer _computeBuffer;

    private CustomFogRenderPass _scriptablePass;

    public override void Create()
    {
        // Create() may run multiple times before Dipose runs, thus if the compute buffer is already allocated we must remember to release it
        _computeBuffer?.Release();
        _computeBuffer = new ComputeBuffer(1, Marshal.SizeOf<FogDataStruct>(), ComputeBufferType.Constant);
        _scriptablePass = new CustomFogRenderPass(FogMaterial, FogProfile, _computeBuffer);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (FogMaterial == null){ 
            return;
        }

        renderer.EnqueuePass(_scriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        if (_computeBuffer != null)
        {
            _computeBuffer.Release();
            _computeBuffer = null;
        }
        _scriptablePass = null;

    }
}
