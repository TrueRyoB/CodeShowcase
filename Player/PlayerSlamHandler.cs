using UnityEngine;
using Fujin.Constants;

namespace Fujin.Player
{
    public class PlayerSlamHandler : MonoBehaviour
    {
        [SerializeField]private PlayerPhysics mPhysics;
        private void OnTriggerEnter(Collider col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                if (mPhysics.IsDiving)
                {
                    mPhysics.Slam();
                }
            }
        }
    }
}