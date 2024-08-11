using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DistanceFadePass : ScriptableRenderPass
{
    #region Constants
    private readonly int PROP_NEAR_CLIP_ID = Shader.PropertyToID("_NearClip");
    private readonly int PROP_FAR_CLIP_ID = Shader.PropertyToID("_FarClip");
    private readonly int PROP_NEAR_FADE_ID = Shader.PropertyToID("_NearFade");
    private readonly int PROP_FAR_FADE_ID = Shader.PropertyToID("_FarFade");
    private readonly int PROP_PREVIEW_MASK_ID = Shader.PropertyToID("_PreviewMask");
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
        m_distanceFadeMaterial.SetFloat(PROP_NEAR_CLIP_ID, m_distanceFadeComponent.nearClip.value);
        m_distanceFadeMaterial.SetFloat(PROP_FAR_CLIP_ID, m_distanceFadeComponent.farClip.value);
        m_distanceFadeMaterial.SetFloat(PROP_NEAR_FADE_ID, m_distanceFadeComponent.nearFade.value);
        m_distanceFadeMaterial.SetFloat(PROP_PREVIEW_MASK_ID, (m_distanceFadeComponent.previewMask.value ? 1.0f : 0.0f));
        m_distanceFadeMaterial.SetFloat(PROP_FAR_FADE_ID, m_distanceFadeComponent.farFade.value);

        // Create temp color buffer
        cmd.GetTemporaryRT(PROP_TEMP_COLOR_ID, m_cameraDescriptor);
        m_shaderTempColor = new RenderTargetIdentifier(PROP_TEMP_COLOR_ID);

        // Run the first pass of the shader
        Blit(cmd, m_cameraColor, m_shaderTempColor, m_distanceFadeMaterial, 0);

        // Copy output back to camera.
        //
        // TODO: Could this be done in the same step above?
        // I don't think you can write to a texture you're also reading from.
        // E.g. I don't think you can write to a buffer that currently has a
        // SAMPLER2D attached.
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
    }

    #endregion Public Methods
}