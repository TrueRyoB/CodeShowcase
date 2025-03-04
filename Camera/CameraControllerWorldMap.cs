using UnityEngine;
using Fujin.Player;
using Fujin.Constants;
using System.Collections;

namespace Fujin.Camera
{
    /// <summary>
    /// Child class of CameraController designed for tracking a player on a world map
    /// ...necessary primarily to track the position of a player on a canvas scale.
    /// </summary>
    public sealed class CameraControllerWorldMap : CameraController
    {
        [SerializeField] private PlayerPhysicsWorldMap player;
        [SerializeField] private Vector3 idealOffset;
        [SerializeField] private float zoomTime = 0.2f;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float zoomFOV = 50f;
        private bool isTracking;
        private Vector3 offset;
        
        private UnityEngine.Camera cam;
        private void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
        }
        
        private void Update()
        {
            HandleCameraTracking();
        }
        
        /// <summary>
        /// Is called by LoadingSpotlightTransitionController.
        /// </summary>
        /// <returns></returns>
        public override void FocusOnPlayerWithZoom()
        {
            StartCoroutine(ZoomIn());
        }

        /// <summary>
        /// Helper function for FocusOnPlayerWithZoom() that moves and zoom in to match the player
        /// </summary>
        /// <returns></returns>
        private IEnumerator ZoomIn()
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = player.transform.position + idealOffset;
            for (float t = 0; t <= zoomTime; t += Time.deltaTime)
            {
                float fov = Mathf.Lerp(normalFOV, zoomFOV, t / zoomTime);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t / zoomTime);
                cam.fieldOfView = fov;
                yield return null;
            }
            cam.fieldOfView = zoomFOV;
        }

        private IEnumerator ZoomOut()
        {
            for (float t = 0; t <= zoomTime; t += Time.deltaTime)
            {
                float fov = Mathf.Lerp(zoomFOV, normalFOV, t / zoomTime);
                cam.fieldOfView = fov;
                yield return null;
            }
            cam.fieldOfView = normalFOV;
        }

        /// <summary>
        /// Returns a rect player position on canvas based on top left
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetPlayerPositionOnScreen()
        {
            Vector3 playerPositionOnScreen = cam.WorldToScreenPoint(player.transform.position);
            if (playerPositionOnScreen.z < 0) return Vector2Int.zero;
            
            float playerXRatio = playerPositionOnScreen.x / Screen.width;
            float playerYRatio = (playerPositionOnScreen.y / Screen.height) - 1f;
            
            return new Vector2(playerXRatio * CanvasSize.Width, playerYRatio * CanvasSize.Height);
        }

        private void HandleCameraTracking()
        {
            if (player.IsOnPavement && !isTracking)
            {
                isTracking = true;
                offset = transform.position - player.transform.position;
            }
            else if (!player.IsOnPavement)
            {
                isTracking = false;
            }

            if (isTracking)
            {
                transform.position = player.transform.position + offset;
            }
        }
    }
}