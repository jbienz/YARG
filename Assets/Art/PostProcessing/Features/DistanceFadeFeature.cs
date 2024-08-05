using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DistanceFadeFeature : ScriptableRendererFeature
{
    DistanceFadePass pass;

    public override void Create()
    {
        pass = new DistanceFadePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}