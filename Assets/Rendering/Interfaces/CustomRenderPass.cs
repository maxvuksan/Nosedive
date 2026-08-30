using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;



public class RenderPassVariables<TProfile, TProfileDataStructure>
    where TProfile : IProfile<TProfileDataStructure>
    where TProfileDataStructure : struct                              
{   
    public string PassName = "UNTITLED_RENDER_PASS";
    public TProfileDataStructure[] DataArray = new TProfileDataStructure[1];

    public RenderPassVariables(string passName)
    {
        PassName = passName;
    }
}



public static class RenderPassHelper
{
    public static bool ShouldRenderPostProcessingInSceneView<TProfile, TProfileDataStructure>(
        CustomRenderFeatureVariables<TProfile, TProfileDataStructure> featureState, 
        ContextContainer frameData)

        where TProfile : IProfile<TProfileDataStructure>
        where TProfileDataStructure : struct              
    {
        if (featureState.Material == null || featureState.Profile == null) return false;

        // If we are viewing in SceneView and post-processing is disabled, do not render
        var cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.isSceneViewCamera && !cameraData.postProcessEnabled) return false;

        return true;
    }

    public static void RecordRenderGraph<TProfile, TProfileDataStructure>(
        CustomRenderFeatureVariables<TProfile, TProfileDataStructure> featureState, 
        RenderPassVariables<TProfile, TProfileDataStructure> passState, 
        RenderGraph renderGraph, 
        ContextContainer frameData)

        where TProfile : IProfile<TProfileDataStructure>
        where TProfileDataStructure : struct                              
    {
        if (!ShouldRenderPostProcessingInSceneView(featureState, frameData))
        {
            return;
        }

        passState.DataArray[0] = featureState.Profile.GetData();
        featureState.ComputeBuffer.SetData(passState.DataArray);

        string constantBufferName = $"{passState.PassName}Variables";
        Shader.SetGlobalConstantBuffer(Shader.PropertyToID(constantBufferName), featureState.ComputeBuffer, 0, featureState.ComputeBuffer.stride);

        var resourceData = frameData.Get<UniversalResourceData>();

        var destDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
        destDesc.name = $"_Temp{passState.PassName}Target";
        destDesc.clearBuffer = false;
        TextureHandle tempTarget = renderGraph.CreateTexture(destDesc);

        renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(
            resourceData.activeColorTexture, tempTarget, featureState.Material, 0),
            $"{passState.PassName} Pass");

        renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(
            tempTarget, resourceData.activeColorTexture, Blitter.GetBlitMaterial(TextureXR.dimension), 0),
            $"{passState.PassName} Copy Back");

    }

}