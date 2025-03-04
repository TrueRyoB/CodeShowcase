using UnityEngine;
using Fujin.Player;
using Fujin.Mobs;
using Fujin.Constants;
using System.Collections;
using Cinemachine;
using UnityEngine.Rendering;

namespace Fujin.Camera
{
    /// <summary>
    /// This class has to be set to a main camera of every platformer so that
    /// ...the loading animation works as intended (especially referring to spot-out)
    /// </summary>
    public sealed class CameraControllerPlatformer : CameraController
    {
        private PlayerPhysics player;
        private Rigidbody playerRb;
        private GoalRocketManager rocket;
        
        private readonly float delayRatio = 0.7f;
        private readonly float yFixThreshold = 5f;
        
        private bool shouldTrack;
        private Coroutine trackingCoroutine;

        private bool shouldHardTrack;
        
        [SerializeField] private UnityEngine.Camera cam;
        
        [SerializeField] private Vector3 idealOffset;
        [SerializeField] private float zoomTime = 0.2f;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float zoomFOV = 50f;
        [SerializeField] private float goalFOV = 30f;

        [SerializeField] private CinemachineVirtualCamera hardTrack;
        [SerializeField] private CinemachineVirtualCamera boundedBox;

        [SerializeField] private GameObject toggleIndicatorUI;

        private readonly float mouseMShardTrack = 10f;

        private void Start()
        {
            ppvManager ??= FindObjectOfType<Volume>();
            player ??= FindObjectOfType<PlayerPhysics>();
            playerRb = player.GetComponent<Rigidbody>();
            rocket ??= FindObjectOfType<GoalRocketManager>();

            if (hardTrack == null || boundedBox == null || toggleIndicatorUI == null)
            {
                Debug.LogError("Error: at least one of the important camera objects reference is missing");
            }
            
            SetBoundedBox(true);
        }

        private void Update()
        {
            if (shouldHardTrack)
            {
                UpdateHtOffset();
            }
        }
        
        [SerializeField]private VolumeProfile normal;
        [SerializeField]private VolumeProfile timeSlow;

        private Volume ppvManager;
        
        public void SetTimeSlowPpv(bool value)
        {
            ppvManager.profile = value ? timeSlow : normal;
        }

        private Vector3 tempOffset;
        private readonly Vector2 xLimitRatio = new Vector2(0.1f, 0.9f);
        private readonly Vector2 yLimitRatio = new Vector2(0.2f, 0.9f);
        private Vector2 xLimit = Vector2.zero;
        private Vector2 yLimit = Vector2.zero;

        public void DisableHardTrack()
        {
            shouldHardTrack = false;
        }

        private void UpdateHtOffset()
        {
            var transposer = hardTrack.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            float moveX = Input.GetAxis("Mouse X") * mouseMShardTrack * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * mouseMShardTrack * Time.deltaTime;
            
            tempOffset = transposer.m_TrackedObjectOffset;
            tempOffset.x = Mathf.Clamp(tempOffset.x + moveX, xLimit.x, xLimit.y);
            tempOffset.y = Mathf.Clamp(tempOffset.y + moveY, yLimit.x, yLimit.y);
            
            transposer.m_TrackedObjectOffset = tempOffset;
        }
        
        /// <summary>
        /// Should be called when either the z value or the screen size changes
        /// </summary>
        private void InitializeHtPos()
        {
            // Update the offset and the rotation to match the previous camera for a smooth transition
            hardTrack.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = boundedBox.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
            hardTrack.transform.rotation = boundedBox.transform.rotation;
            
            Vector3 worldPosMin = cam.ViewportToWorldPoint(new Vector3(xLimitRatio.x, yLimitRatio.x, playerRb.position.z - hardTrack.transform.position.z));
            Vector3 worldPosMax = cam.ViewportToWorldPoint(new Vector3(xLimitRatio.y, yLimitRatio.y, playerRb.position.z - hardTrack.transform.position.z));
            Vector3 worldPosMid = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, playerRb.position.z - hardTrack.transform.position.z));
            
            xLimit.x = worldPosMin.x - worldPosMid.x;
            xLimit.y = worldPosMax.x - worldPosMid.x;
            yLimit.x = worldPosMin.y - worldPosMid.y;
            yLimit.y = worldPosMax.y - worldPosMid.y;
        }

        private bool isToggled;
        private bool isAiming;

        public void SetAiming(bool value)
        {
            isAiming = value;
            SetHardTracking(isToggled);
        }
        
        // Issue
        
        // The current status is sent
        // Ignore the call if the state seems static

        public void SetHardTracking(bool value) // equivalent to the new value of isToggled
        {
            isToggled = value;
            
            // Initialize the HtPos if the priority was not previously set at 10 and if isAiming || setHardTrack
            if (hardTrack.Priority == 0 && (isToggled || isAiming))
            {
                InitializeHtPos();
                SetBoundedBox(false);
                hardTrack.Priority = 10;
            }

            // Deactivate self if not used at all
            if (!isToggled && !isAiming)
            {
                hardTrack.Priority = 0;
            }
            
            // Set shouldHardTrack true only if isToggled
            shouldHardTrack = isToggled;
            toggleIndicatorUI.SetActive(isToggled);
        }

        public void SetBoundedBox(bool value)
        {
            if (value) SetHardTracking(false);

            boundedBox.Priority = !isAiming && value ? 10 : 0;
        }

        public override void FocusOnPlayerWithZoom()
        {
            transform.position = player.transform.position + idealOffset;
            StartCoroutine(ZoomIn());
        }

        private void DisableCinemachine()
        {
            CinemachineBrain cineBrain = UnityEngine.Camera.main!.GetComponent<CinemachineBrain>();
            if (cineBrain != null)
            {
                cineBrain.enabled = false;
            }
        }

        /// <summary>
        /// Film both player and rocket by setting the center to the between of both
        /// ...followed by a brief zoom.
        /// 1. Set the camera to film both a player and a goal rocket
        /// 2. Zoom briefly (change orthograph size)
        /// </summary>
        public override void FocusOnPlayerAndRocketWithZoom()
        {
            // Disable Cinemachine for a while
            DisableCinemachine();
            
            // Get an ideal camera coordinates
            Vector3 midpoint = (player.transform.position + rocket.transform.position) / 2;
            midpoint.z = transform.position.z; // fix z value
            
            // Move camera and make it zoom in TODO: (ideally, over time)
            cam.transform.position = midpoint;
            cam.fieldOfView = goalFOV;
        }

        /// <summary>
        /// 1. Lock x and z value
        /// 2. Track player but with delay
        /// 3. Fix the relative position once the filming target is at the certain point
        /// </summary>
        public override void LoseTargetWithDelayTrackingYAxis()
        {
            DisableCinemachine();
            trackingCoroutine ??= StartCoroutine(PerformLosingTargetWithDelayTrackingYAxis());
        }

        /// <summary>
        /// Helper function for PerformLosingTargetWithDelayTrackingYAxis()
        /// </summary>
        private void StopTracking()
        {
            shouldTrack = false;
            trackingCoroutine = null;
        }

        private IEnumerator PerformLosingTargetWithDelayTrackingYAxis()
        {
            // Initialize a base value
            shouldTrack = true;
            float cameraBaseY = transform.position.y;
            float playerY = player.transform.position.y;

            // Keep increasing the y value by ratio until reaching a certain point
            while (player.transform.position.y - transform.position.y<= yFixThreshold)
            {
                float targetY = (player.transform.position.y - playerY) * delayRatio;
                transform.position = new Vector3(transform.position.x, cameraBaseY + targetY , transform.position.z);
                yield return null;
                Debug.Log("chasing!");
            }

            // Remain the same relative place
            while (shouldTrack)
            {
                transform.position = new Vector3(transform.position.x, player.transform.position.y - yFixThreshold, transform.position.z);
                yield return null;
            }
        }
        
        private IEnumerator ZoomIn()
        {
            for (float t = 0; t <= zoomTime; t += Time.deltaTime)
            {
                float fov = Mathf.Lerp(normalFOV, zoomFOV, t / zoomTime);
                cam.fieldOfView = fov;
                yield return null;
            }
            cam.fieldOfView = zoomFOV;
        }
        
        public override Vector2 GetPlayerPositionOnScreen()
        {
            player ??= FindObjectOfType<PlayerPhysics>();
            
            Vector3 playerPositionOnScreen = cam.WorldToScreenPoint(player.transform.position);
            if (playerPositionOnScreen.z < 0) return Vector2Int.zero;
            
            float playerXRatio = playerPositionOnScreen.x / Screen.width;
            float playerYRatio = (playerPositionOnScreen.y / Screen.height) - 1f;
            
            return new Vector2(playerXRatio * CanvasSize.Width, playerYRatio * CanvasSize.Height);
        }
    }
}