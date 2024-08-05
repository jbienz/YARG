using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DepthFadeFeature : ScriptableRendererFeature
{
    DepthFadePass pass;

    public override void Create()
    {
        pass = new DepthFadePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}