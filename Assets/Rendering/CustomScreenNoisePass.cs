using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class NoiseRenderFeature : ScriptableRendererFeature
{
    public class NoiseRenderPass: ScriptableRenderPass
    {
        private RenderPassVariables<ScreenNoiseProfile, ScreenNoiseDataStruct> _passState = new("Noise");
        private CustomRenderFeatureVariables<ScreenNoiseProfile, ScreenNoiseDataStruct> _featureState;

        public NoiseRenderPass(CustomRenderFeatureVariables<ScreenNoiseProfile, ScreenNoiseDataStruct> featureState)
        {
            _featureState = featureState;
            renderPassEvent = featureState.InjectionPoint;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!RenderPassHelper.ShouldRenderPostProcessingInSceneView(_featureState, frameData))
            {
                return;
            }

            var cameraData = frameData.Get<UniversalCameraData>();
            PreRender(cameraData);

            RenderPassHelper.RecordRenderGraph(_featureState, _passState, renderGraph, frameData);
        }

        public void PreRender(UniversalCameraData cameraData)
        {
            // Get dimensions of viewport to ensure noise UV scales to counter it (for uniform sizing)
            #if UNITY_EDITOR

                if (cameraData.isSceneViewCamera) 
                {
                    // Scene Viewport

                    Rect position = SceneView.lastActiveSceneView.position;
                
                    _featureState.Profile.Data._ViewportDimensions.x = position.width;
                    _featureState.Profile.Data._ViewportDimensions.y = position.height;
                }
                else 
                {
                    // Game Viewport

                    Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
                    MethodInfo getSizeMethod = gameViewType.GetMethod(
                        "GetSizeOfMainGameView",
                        BindingFlags.NonPublic | BindingFlags.Static
                    );

                    var size = (Vector2)getSizeMethod.Invoke(null, null);

                    _featureState.Profile.Data._ViewportDimensions.x = size.x;
                    _featureState.Profile.Data._ViewportDimensions.y = size.y;
                }

            #else

                // Build Viewport

                _featureState.Profile.Data._ViewportDimensions.x = Screen.width;
                _featureState.Profile.Data._ViewportDimensions.y = Screen.height;
            
            #endif


            // Calculate random UV offset
            _featureState.Profile.Data._NoiseRandomUvOffset = new Vector2(
                UnityEngine.Random.Range(0, _featureState.Profile.NoiseRandomOffsetScale),
                UnityEngine.Random.Range(0, _featureState.Profile.NoiseRandomOffsetScale)
            );
        }
    }




    public CustomRenderFeatureVariables<ScreenNoiseProfile, ScreenNoiseDataStruct> State = new();
    private NoiseRenderPass _renderPass;

    public override void Create()
    {
        RenderFeatureHelper.Setup(State);
        _renderPass = new(State);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        RenderFeatureHelper.AddRenderPasses(State, renderer, _renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        RenderFeatureHelper.Dispose(State, _renderPass);
    }
}