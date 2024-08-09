using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("YARG/DistanceFade", typeof(UniversalRenderPipeline))]
public class DistanceFadeComponent : VolumeComponent, IPostProcessComponent
{
    // For example, an intensity parameter that goes from 0 to 1
    public ClampedFloatParameter nearClip = new ClampedFloatParameter(value: 0.1f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farClip = new ClampedFloatParameter(value: 0.3f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter nearFade = new ClampedFloatParameter(value: 0.01f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farFade = new ClampedFloatParameter(value: 0.1f, min: 0, max: 1, overrideState: true);
    public BoolParameter previewMask = new BoolParameter(value: false);

    // A color that is constant even when the weight changes
    // public NoInterpColorParameter overlayColor = new NoInterpColorParameter(Color.cyan);

    // Other 'Parameter' variables you might have

    // Tells when our effect should be rendered
    public bool IsActive() => (farClip.value < 1.0 | nearClip.value > 0.0);

    // I have no idea what this does yet but I'll update the post once I find an usage
    public bool IsTileCompatible() => true;
}