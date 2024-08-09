using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DistanceFadePass : ScriptableRenderPass
{
    #region Constants
    private readonly int PROP_DEPTH_TEXTURE_ID = Shader.PropertyToID("_DepthTex");
    private readonly int PROP_INTENSITY_ID = Shader.PropertyToID("_Intensity");
    private readonly int PROP_OVERLAY_COLOR_ID = Shader.PropertyToID("_OverlayColor");
    private readonly int PROP_TEMP_COLOR_ID = Shader.PropertyToID("_TempColor");
    #endregion Constants

    #region Member Variables
    private RenderTargetIdentifier m_cameraColor;
    private RenderTargetIdentifier m_cameraDepth;
    private RenderTextureDescriptor m_cameraDescriptor;
    private DistanceFadeComponent m_distanceFadeComponent;
    private Material m_distanceFadeMaterial;
    private RenderTargetIdentifier m_shaderDepth;
    private RenderTargetIdentifier m_shaderTempColor;
    #endregion Member Variables

    #region Public Constructors

    public DistanceFadePass(Material distanceFadeMaterial)
    {
        // Validate
        if (distanceFadeMaterial == null)
        {
            throw new ArgumentNullException(nameof(distanceFadeMaterial));
        }

        // Store
        m_distanceFadeMaterial = distanceFadeMaterial;

        // Set the render pass event
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    #endregion Public Constructors

    #region Private Methods

    private void ExecuteDistanceFade(CommandBuffer cmd)
    {
        // Update shader properties
        m_distanceFadeMaterial.SetFloat(PROP_INTENSITY_ID, m_distanceFadeComponent.intensity.value);
        m_distanceFadeMaterial.SetColor(PROP_OVERLAY_COLOR_ID, m_distanceFadeComponent.overlayColor.value);

        // Create temp color buffer
        cmd.GetTemporaryRT(PROP_TEMP_COLOR_ID, m_cameraDescriptor);
        m_shaderTempColor = new RenderTargetIdentifier(PROP_TEMP_COLOR_ID);

        // Use camera depth buffer for shader depth texture
        cmd.SetGlobalTexture(PROP_DEPTH_TEXTURE_ID, m_cameraDepth);
        // m_distanceFadeMaterial.SetTexture(PROP_DEPTH_TEXTURE_ID, Shader.GetGlobalTexture("_MYDEPTH"));

        // Run the first pass of the shader
        Blit(cmd, m_cameraColor, m_shaderTempColor, m_distanceFadeMaterial, 0);
        // Blit(cmd, temporaryBuffer, colorBuffer, mat, 1); // shader pass 1

        // Copy output back to camera !!! IMPORTANT: COULD BE DONE IN ONE STEP ABOVE
        Blit(cmd, m_shaderTempColor, m_cameraColor);
    }

    #endregion Private Methods

    #region Public Methods

    /// <inheritdoc />
    public void Dispose()
    {
        if (m_distanceFadeMaterial != null)
        {
            CoreUtils.Destroy(m_distanceFadeMaterial);
        }
    }

    /// <inheritdoc />
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Get the stack, which holds all the current volumes
        VolumeStack stack = VolumeManager.instance.stack;

        // Get our custom component
        m_distanceFadeComponent = stack.GetComponent<DistanceFadeComponent>();

        // Only process if the effect is active
        if (m_distanceFadeComponent.IsActive())
        {
            // Create a command buffer to execute
            CommandBuffer cmd = CommandBufferPool.Get();

            // Put this in a profiler scope
            using (new ProfilingScope(cmd, new ProfilingSampler("Distance Fade")))
            {
                // The next two lines are a hack for URP 12 to make sure
                // things run in the new profiler scope
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // Execute distance
                ExecuteDistanceFade(cmd);
            }

            // Execute the entire buffer
            // In URP 12 this has to be done again
            // outside the profiler scope
            context.ExecuteCommandBuffer(cmd);

            // Release the buffer back into the pool
            CommandBufferPool.Release(cmd);
        }
    }

    /// <inheritdoc />
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(PROP_DEPTH_TEXTURE_ID);
        cmd.ReleaseTemporaryRT(PROP_TEMP_COLOR_ID);
    }

    /// <inheritdoc />
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Grab the camera target descriptor. We will use this when creating a temporary render texture.
        m_cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        // Hold onto the camera target as the main "source"
        m_cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
        m_cameraDepth = renderingData.cameraData.renderer.cameraDepthTarget;

        /*
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;

        // Create a temporary render texture using the descriptor from above.
        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);
        */
    }

    ///// <summary>
    ///// Sets the render targets for the camera.
    ///// </summary>
    ///// <param name="cameraColorTargetHandle">
    ///// The color target.
    ///// </param>
    ///// <param name="cameraDepthTargetHandle">
    ///// The depth target.
    ///// </param>
    //public void SetTargets(RenderTargetIdentifier cameraColorTargetHandle, RenderTargetIdentifier cameraDepthTargetHandle)
    //{
    //    m_cameraColorTargetHandle = cameraColorTargetHandle;
    //    m_cameraDepthTargetHandle = cameraDepthTargetHandle;
    //}

    #endregion Public Methods
}