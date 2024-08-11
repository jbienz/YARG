using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using YARG.Core.Engine;
using YARG.Core.Game;
using YARG.Gameplay.Player;
using YARG.Settings;

namespace YARG.Gameplay.HUD
{
    public class TrackView : MonoBehaviour
    {
        private static readonly int _curveFactor = Shader.PropertyToID("_CurveFactor");
        private static readonly int _fadeLength = Shader.PropertyToID("_FadeLength");

        [field: SerializeField]
        public RawImage TrackImage { get; private set; }

        [SerializeField]
        private AspectRatioFitter _aspectRatioFitter;
        [SerializeField]
        private RectTransform _topElementContainer;

        [Space]
        [SerializeField]
        private SoloBox _soloBox;
        [SerializeField]
        private TextNotifications _textNotifications;

        private TrackPlayer _trackPlayer;

        private Camera trackCamera;
        private RenderTexture renderTexture;
        private bool depth;

        private void Start()
        {
            _aspectRatioFitter.aspectRatio = (float) Screen.width / Screen.height;
        }

        public void Initialize(RenderTexture renderTexture, CameraPreset cameraPreset, TrackPlayer trackPlayer)
        {
            // Store the render texture as the RawImage main texture
            TrackImage.texture = renderTexture;

            // Clone the shader material since RawImages don't use instanced materials
            var newMaterial = new Material(TrackImage.material);

            // Configure the cloned material
            newMaterial.SetFloat(_curveFactor, cameraPreset.CurveFactor);
            trackCamera = trackPlayer.TrackCamera;
            
            this.renderTexture = renderTexture;

            // Set the cloned material onto the RawImage
            TrackImage.material = newMaterial;

            // Get the post-processing volume applied to the camera
            var volume = trackCamera.GetComponent<Volume>();

            // Clone the profile that comes in from the prefab.
            // This makes it so we can change effects per camera.
            VolumeProfile clonedProfile = Instantiate(volume.profile);

            // Get the DistanceFade component
            DistanceFadeComponent fadeComponent = (DistanceFadeComponent)clonedProfile.components.First(c => c is DistanceFadeComponent);

            // JARED: TODO: The FadeLength in the current system appears to be meters from the camera.
            // However, the new 0 - 1 range appears to be Camera Near Clipping Plane to Far Clipping Plane.
            // We'll need to convert this to depth buffer distance.

            // Set the fade to match the camera profile
            fadeComponent.farClip.Override(cameraPreset.FadeLength);

            // If we're not at infinite distance, also fade out
            if (cameraPreset.FadeLength < 1.0)
            {
                fadeComponent.farFade.Override(0.02f);
            }

            // Set the cloned profile onto the volume
            volume.profile = clonedProfile;

            // Save the player reference
            _trackPlayer = trackPlayer;
        }

        public void UpdateSizing(int trackCount)
        {
            // This equation calculates a good scale for all of the tracks.
            // It was made with experimentation; there's probably a "real" formula for this.
            float scale = Mathf.Max(0.7f * Mathf.Log10(trackCount - 1), 0f);
            scale = 1f - scale;

            TrackImage.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void UpdateHUDPosition()
        {
            var rect = TrackImage.GetComponent<RectTransform>();
            var viewportPos = _trackPlayer.HUDViewportPosition;

            // Caching this is faster
            var rectRect = rect.rect;

            // Adjust the screen's viewport position to the rect's viewport position
            // TODO: I have no idea where this "- 0.5f" comes from. Are these calculations correct?
            var local = new Vector2(
                (viewportPos.x - 0.5f) * rectRect.width,
                viewportPos.y * rectRect.height);
            var screenPos = rect.TransformPoint(local);

            // Now, move the MoveContainer based on this
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform) _topElementContainer.parent,
                screenPos, null, out var localPoint);
            _topElementContainer.localPosition = localPoint;
        }

        public void StartSolo(SoloSection solo)
        {
            _soloBox.StartSolo(solo);

            // No text notifications during the solo
            _textNotifications.gameObject.SetActive(false);
        }

        public void EndSolo(int soloBonus)
        {
            _soloBox.EndSolo(soloBonus, () =>
            {
                // Show text notifications again
                _textNotifications.gameObject.SetActive(true);
            });
        }

        public void UpdateNoteStreak(int streak)
        {
            _textNotifications.UpdateNoteStreak(streak);
        }

        public void ShowNewHighScore()
        {
            _textNotifications.ShowNewHighScore();
        }

        public void ShowFullCombo()
        {
            _textNotifications.ShowFullCombo();
        }

        public void ShowHotStart()
        {
            _textNotifications.ShowHotStart();
        }

        public void ShowBassGroove()
        {
            _textNotifications.ShowBassGroove();
        }

        public void ShowStarPowerReady()
        {
            _textNotifications.ShowStarPowerReady();
        }

        public void ShowStrongFinish()
        {
            _textNotifications.ShowStrongFinish();
        }

        public void ForceReset()
        {
            _textNotifications.gameObject.SetActive(true);

            _soloBox.ForceReset();
            _textNotifications.ForceReset();
        }

        void Update()
        {
            if (depth)
            {
                Graphics.SetRenderTarget(renderTexture);
                // trackCamera.SetTargetBuffers(renderTexture.colorBuffer, renderTexture.depthBuffer);
                trackCamera.Render();
            }
        }
    }
}