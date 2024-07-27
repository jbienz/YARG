using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YARG.Gameplay.Player;
using YARG.Helpers.Extensions;
using YARG.Player;
using YARG.Settings;

namespace YARG.Gameplay.HUD
{
    public class TrackViewManager : GameplayBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _trackViewPrefab;
        [SerializeField]
        private GameObject _vocalHudPrefab;

        [Header("References")]
        [SerializeField]
        private RawImage _vocalImage;
        [SerializeField]
        private Transform _vocalHudParent;

        private readonly List<TrackView> _trackViews = new();

        public TrackView CreateTrackView(TrackPlayer trackPlayer, YargPlayer player)
        {
            // Create a track view
            var trackView = Instantiate(_trackViewPrefab, transform).GetComponent<TrackView>();

            // Set up render texture
            var renderDescriptor = new RenderTextureDescriptor(
                Screen.width, Screen.height,
                RenderTextureFormat.ARGBHalf);
            renderDescriptor.mipCount = 0;
            var renderTexture = new RenderTexture(renderDescriptor);

            // Create a placeholder for the depth texture
            RenderTexture depthTexture = null;

            // Enable depth rendering only if using depth mode for track fade length
            if (SettingsManager.Settings.TrackFadeMode.Value == TrackFadeMode.Depth)
            {
                // Set up depth texture
                var depthDescriptor = new RenderTextureDescriptor(
                    Screen.width, Screen.height,
                    RenderTextureFormat.Depth,
                    24);
                renderDescriptor.mipCount = 0;
                depthTexture = new RenderTexture(depthDescriptor);

                // Enable depth rendering on camera
                trackPlayer.TrackCamera.depthTextureMode = DepthTextureMode.Depth;

                // Tell camera where to store depth map
                trackPlayer.TrackCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
            }
            else
            {
                // Make the camera render on to the texture instead of the screen
                trackPlayer.TrackCamera.targetTexture = renderTexture;
            }

            // Setup track view to show the correct track
            trackView.Initialize(renderTexture, depthTexture, player.CameraPreset, trackPlayer);

            _trackViews.Add(trackView);
            UpdateAllSizing();

            return trackView;
        }

        public void CreateVocalTrackView()
        {
            _vocalImage.gameObject.SetActive(true);

            // Get the aspect ration of the vocal image
            var rect = _vocalImage.rectTransform.ToScreenSpace();
            float ratio = rect.width / rect.height;

            // Apply the vocal track texture
            var rt = GameManager.VocalTrack.InitializeRenderTexture(ratio);
            _vocalImage.texture = rt;
        }

        public VocalsPlayerHUD CreateVocalsPlayerHUD()
        {
            var go = Instantiate(_vocalHudPrefab, _vocalHudParent);
            return go.GetComponent<VocalsPlayerHUD>();
        }

        private void UpdateAllSizing()
        {
            int count = _trackViews.Count;
            foreach (var view in _trackViews)
                view.UpdateSizing(count);
        }

        public void SetAllHUDPositions()
        {
            // The positions of the track view have probably not updated yet at this point
            Canvas.ForceUpdateCanvases();

            foreach (var view in _trackViews)
                view.UpdateHUDPosition();
        }
    }
}