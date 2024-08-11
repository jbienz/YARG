using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("YARG/DistanceFade", typeof(UniversalRenderPipeline))]
public class DistanceFadeComponent : VolumeComponent, IPostProcessComponent
{
    // Define the parameters for the component
    public ClampedFloatParameter nearClip = new ClampedFloatParameter(value: 0.1f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farClip = new ClampedFloatParameter(value: 0.3f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter nearFade = new ClampedFloatParameter(value: 0.01f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farFade = new ClampedFloatParameter(value: 0.1f, min: 0, max: 1, overrideState: true);
    public BoolParameter previewMask = new BoolParameter(value: false);

    /// <summary>
    /// Gets whether our effect should be rendered.
    /// </summary>
    public bool IsActive() => (farClip.value < 1.0 | nearClip.value > 0.0);

    /// <summary>
    /// Gets whether it can run on-tile.
    /// </summary>
    /// <remarks>
    /// Unclear what this does. Unity docs say "if it can run on-tile", which may have something to
    /// do with tile-based rendering but not sure. Note that this property is marked Obsolete for
    /// Unity 2023 onwards.
    /// </remarks>
    public bool IsTileCompatible() => true;
}