using UnityEngine;
using Fujin.Constants;

namespace Fujin.Player
{
    public class PlayerEvadeHandler : MonoBehaviour
    {
        [SerializeField]private PlayerPhysics mPhysics;
        private void OnTriggerStay(Collider col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.MovableSurface))
            {
                Debug.Log("Evade enabled!");
                mPhysics.SetCanEvade(true);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.MovableSurface))
            {
                mPhysics.SetCanEvade(false);
            }
        }
    }
}