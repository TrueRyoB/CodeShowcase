using UnityEngine;

namespace Fujin.Camera
{
    public sealed class CameraControllerGeneral : CameraController
    {
        public override Vector2 GetPlayerPositionOnScreen()
        {
            Debug.LogError("Error: Function GetPlayerPositionOnScreen() is not supported for class CameraControllerGeneral.");
            return Vector2.zero;
        }

        public override void FocusOnPlayerWithZoom()
        {
            Debug.LogError("Error: Function FocusOnPlayerWithZoom() is not supported for class CameraControllerGeneral.");
        }
    }
}