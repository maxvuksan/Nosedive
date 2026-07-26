using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlaydeadVelocityFeature : ScriptableRendererFeature
{
    public enum NeighborMaxSupport { TileSize10, TileSize20, TileSize40 }

    [System.Serializable]
    public class Settings
    {
        public Shader velocityShader;
        public bool neighborMaxGen = false;
        public NeighborMaxSupport neighborMaxSupport = NeighborMaxSupport.TileSize20;
    }

    public Settings settings = new Settings();
    private PlaydeadVelocityPass m_VelocityPass;

    public override void Create()
    {
        m_VelocityPass = new(settings)
        {
            // Inject right after opaque geometry finishes rendering
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.velocityShader == null) return;
        renderer.EnqueuePass(m_VelocityPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_VelocityPass?.Dispose();
    }



    /// <summary>
    /// This TRAA solution is ported from Playdead's TRAA implementation for their game INSIDE
    /// The original implementation was written for the built in render pipeline and has been modified to fit URP render feature workflow
    /// </summary>
    private class PlaydeadVelocityPass : ScriptableRenderPass
    {
        private Settings settings;
        private Material velocityMaterial;
        
        private RTHandle velocityBufferTarget;
        private RTHandle neighborMaxTarget;

        // Historical Matrix Tracking (Handles what the old Camera component used to track)
        private bool paramInitialized = false;
        private Matrix4x4 paramCurrV;
        private Matrix4x4 paramCurrVP;
        private Matrix4x4 paramPrevVP;
        private Matrix4x4 paramPrevVP_NoFlip;

        public PlaydeadVelocityPass(Settings settings)
        {
            this.settings = settings;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Forces URP to make sure the native depth texture is active and readable
            ConfigureInput(ScriptableRenderPassInput.Depth);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 16;
            
#if UNITY_PS4
            desc.colorFormat = RenderTextureFormat.RGHalf;
#else
            desc.colorFormat = RenderTextureFormat.RGFloat;
#endif

            RenderingUtils.ReAllocateIfNeeded(ref velocityBufferTarget, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_PlaydeadVelocityBuffer");
            
            if (settings.neighborMaxGen)
            {
                var tileDesc = desc;
                int divide = settings.neighborMaxSupport == NeighborMaxSupport.TileSize10 ? 10 : (settings.neighborMaxSupport == NeighborMaxSupport.TileSize20 ? 20 : 40);
                tileDesc.width = Mathf.Max(1, desc.width / divide);
                tileDesc.height = Mathf.Max(1, desc.height / divide);
                RenderingUtils.ReAllocateIfNeeded(ref neighborMaxTarget, tileDesc, FilterMode.Point, TextureWrapMode.Clamp, name: "_PlaydeadNeighborMax");
            }

            ConfigureTarget(velocityBufferTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (velocityMaterial == null || velocityMaterial.shader != settings.velocityShader)
            {
                if (velocityMaterial != null) CoreUtils.Destroy(velocityMaterial);
                velocityMaterial = CoreUtils.CreateEngineMaterial(settings.velocityShader);
            }

            if (velocityMaterial == null) return;

            Camera camera = renderingData.cameraData.camera;
            CommandBuffer cmd = CommandBufferPool.Get("PlaydeadVelocityBuffer");

            // --- Historical Matrix Calculations ---
            Matrix4x4 currV = camera.worldToCameraMatrix;
            Matrix4x4 currP = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            Matrix4x4 currP_NoFlip = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            Matrix4x4 prevV = paramInitialized ? paramCurrV : currV;

            paramInitialized = true;
            paramCurrV = currV;
            paramCurrVP = currP * currV;
            paramPrevVP = currP * prevV;
            paramPrevVP_NoFlip = currP_NoFlip * prevV;

            // --- Update Keywords & Uniforms ---
            CoreUtils.SetKeyword(velocityMaterial, "CAMERA_PERSPECTIVE", !camera.orthographic);
            CoreUtils.SetKeyword(velocityMaterial, "CAMERA_ORTHOGRAPHIC", camera.orthographic);
            CoreUtils.SetKeyword(velocityMaterial, "TILESIZE_10", settings.neighborMaxSupport == NeighborMaxSupport.TileSize10);
            CoreUtils.SetKeyword(velocityMaterial, "TILESIZE_20", settings.neighborMaxSupport == NeighborMaxSupport.TileSize20);
            CoreUtils.SetKeyword(velocityMaterial, "TILESIZE_40", settings.neighborMaxSupport == NeighborMaxSupport.TileSize40);

            // Fetch projection extents manually without legacy camera extensions
            float tanHalfFov = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float aspect = camera.aspect;
            float top = tanHalfFov * camera.nearClipPlane;
            float right = top * aspect;
            Vector4 projectionExtents = new Vector4(-right, right, -top, top);

            velocityMaterial.SetVector("_ProjectionExtents", projectionExtents);
            velocityMaterial.SetMatrix("_CurrV", paramCurrV);
            velocityMaterial.SetMatrix("_CurrVP", paramCurrVP);
            velocityMaterial.SetMatrix("_PrevVP", paramPrevVP);
            velocityMaterial.SetMatrix("_PrevVP_NoFlip", paramPrevVP_NoFlip);

            // Clear buffer to black
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.black);

            // Pass 0: Prepass (Static Scene Camera Motion Vectors via Depth)
            Blitter.BlitTexture(cmd, velocityBufferTarget, new Vector4(1, 1, 0, 0), velocityMaterial, 0);

            // Passes 1 & 2: Dynamic / Skinned objects using VelocityBufferTag
            var obs = VelocityBufferTag.activeObjects; 
            for (int i = 0; i < obs.Count; i++)
            {
                var ob = obs[i];
                if (ob != null && ob.rendering && ob.mesh != null)
                {
                    cmd.SetGlobalMatrix("_CurrM", ob.localToWorldCurr);
                    cmd.SetGlobalMatrix("_PrevM", ob.localToWorldPrev);
                    
                    int passIndex = ob.meshSmrActive ? 2 : 1;
                    for (int j = 0; j < ob.mesh.subMeshCount; j++)
                    {
                        cmd.DrawMesh(ob.mesh, Matrix4x4.identity, velocityMaterial, j, passIndex);
                    }
                }
            }

            // Passes 3 & 4: TileMax & NeighborMax logic
            if (settings.neighborMaxGen)
            {
                // Pass 3: TileMax 
                Blitter.BlitTexture(cmd, neighborMaxTarget, new Vector4(1, 1, 0, 0), velocityMaterial, 3);
                // Pass 4: NeighborMax (Writing back out to main velocity)
                Blitter.BlitTexture(cmd, velocityBufferTarget, new Vector4(1, 1, 0, 0), velocityMaterial, 4);
            }

            // Bind globally so your TRAA shader can grab it using Tex2D(_PlaydeadVelocityTexture)
            cmd.SetGlobalTexture("_PlaydeadVelocityTexture", velocityBufferTarget);
            if (settings.neighborMaxGen)
            {
                cmd.SetGlobalTexture("_PlaydeadNeighborMaxTexture", neighborMaxTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            velocityBufferTarget?.Release();
            neighborMaxTarget?.Release();
            if (velocityMaterial != null) CoreUtils.Destroy(velocityMaterial);
        }
    }
}
