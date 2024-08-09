using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DistanceFadeFeature : ScriptableRendererFeature
{
    #region Member Variables
    private Material m_distanceFadeMaterial;
    private DistanceFadePass m_distanceFadePass;
    #endregion Member Variables

    #region Protected Methods

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // Clean up shader materials
        CoreUtils.Destroy(m_distanceFadeMaterial);

        // Pass on to base
        base.Dispose(disposing);
    }

    #endregion Protected Methods

    #region Public Methods

    /// <inheritdoc />
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Only configure and add our pass to game cameras (not preview cameras)
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }

        // Specify that our pass needs color and depth
        m_distanceFadePass.ConfigureInput(ScriptableRenderPassInput.Color);
        m_distanceFadePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderingData.cameraData.requiresDepthTexture = true;

        // Connect color and depth to the renderer
        //m_distanceFadePass.SetTargets(renderer.cameraColorTarget, renderer.cameraDepthTarget);

        // Add our custom passes
        renderer.EnqueuePass(m_distanceFadePass);
    }

    /// <inheritdoc />
    public override void Create()
    {
        // Create shader materials
        m_distanceFadeMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/YARG/DistanceFade"));

        // Create passes
        m_distanceFadePass = new DistanceFadePass(m_distanceFadeMaterial);
    }

    #endregion Public Methods
}