

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// State associated with a custom render feature
/// </summary>
/// <typeparam name="TProfile">The profile to pass to the render pass. This should contain configuration settings for the pass</typeparam>
/// <typeparam name="TProfileDataStructure">The data to pass to the shader (through a constant buffer)</typeparam>
[System.Serializable]
public class CustomRenderFeatureVariables<TProfile, TProfileDataStructure>
    where TProfile : IProfile<TProfileDataStructure>
    where TProfileDataStructure : struct                              
{
    /// <summary>
    /// Controls when then RenderPass executes
    /// </summary>
    public RenderPassEvent InjectionPoint;

    /// <summary>
    /// The material to perform the render pass draw with
    /// </summary>
    public Material Material;
    public ComputeBuffer ComputeBuffer;

    public TProfile Profile;
    private TProfileDataStructure Data;
}

/// <summary>
/// Static utility class to aid creating a custom render feature
/// </summary>
public static class RenderFeatureHelper
{
    public static void Setup<TProfile, TData>(CustomRenderFeatureVariables<TProfile, TData> state)
        where TProfile : IProfile<TData>
        where TData : struct 
    {
        if(state.ComputeBuffer != null)
        {
            state.ComputeBuffer.Release();
        }

        state.ComputeBuffer = new ComputeBuffer(1, Marshal.SizeOf<TData>(), ComputeBufferType.Constant);
    }

    public static void AddRenderPasses<TProfile, TData>(CustomRenderFeatureVariables<TProfile, TData> state, ScriptableRenderer renderer, ScriptableRenderPass renderPass)
        where TProfile : IProfile<TData>
        where TData : struct 
    {
        if (state.Material == null){ 
            return;
        }

        renderer.EnqueuePass(renderPass);
    }

    public static void Dispose<TProfile, TData>(CustomRenderFeatureVariables<TProfile, TData> state, ScriptableRenderPass renderPass)
        where TProfile : IProfile<TData>
        where TData : struct 
    {
        if (renderPass != null)
        {
            state.ComputeBuffer.Release();
            state.ComputeBuffer = null;
        }
        renderPass = null;
    }

}