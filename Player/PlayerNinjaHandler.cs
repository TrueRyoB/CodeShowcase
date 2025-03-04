using UnityEngine;

using Fujin.Constants;

namespace Fujin.Player
{
    public class PlayerOnWallHandler : MonoBehaviour
    {
        [SerializeField]private PlayerPhysics mPhysics;
        private void OnTriggerEnter(Collider col) //Stay -> Enterに変更
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                mPhysics.SetOnWall(true);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                mPhysics.SetOnWall(false);
            }
        }
    }
}