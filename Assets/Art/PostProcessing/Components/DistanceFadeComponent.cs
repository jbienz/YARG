using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("YARG/DistanceFade", typeof(UniversalRenderPipeline))]
public class DistanceFadeComponent : VolumeComponent, IPostProcessComponent
{
    // Define the parameters for the component
    // NOTE: These are the defaults that will be used when no overrides are specified per camera.
    // This is why clipping and fading are disabled by default because the entire track is rendered
    // until a fade distance is supplied by the camera profile.
    public ClampedFloatParameter nearClip = new ClampedFloatParameter(value: 0.0f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farClip = new ClampedFloatParameter(value: 0.9f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter nearFade = new ClampedFloatParameter(value: 0.0f, min: 0, max: 1, overrideState: true);
    public ClampedFloatParameter farFade = new ClampedFloatParameter(value: 0.02f, min: 0, max: 1, overrideState: true);
    public BoolParameter previewMask = new BoolParameter(value: false);

    /// <summary>
    /// Gets whether our effect should be rendered.
    /// </summary>
    /// <remarks>
    /// This feature is always active when the feature is present. We need it to ensure that track
    /// cameras render with background transparency even if fading is completely disabled.
    /// </remarks>
    public bool IsActive() => true;

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