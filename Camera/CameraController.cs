using UnityEngine;

namespace Fujin.Camera
{
    public abstract class CameraController : MonoBehaviour
    {
        public abstract Vector2 GetPlayerPositionOnScreen();

        public abstract void FocusOnPlayerWithZoom();

        public virtual void FocusOnPlayerAndRocketWithZoom()
        {
            Debug.LogError("Error: function FocusOnPlayerAndRocketWithZoom() is not supported.");
        }

        public virtual void LoseTargetWithDelayTrackingYAxis()
        {
            Debug.LogError("Error: function LoseTargetWithDelayTrackingYAxis() is not supported.");
        }
    }
}