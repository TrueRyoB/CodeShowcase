using UnityEngine;
using Fujin.Constants;
using Fujin.Prefab;
using System;

namespace Fujin.Player
{
    /// <summary>
    /// Informs if a player is on a ground (using collider)
    /// Informs if a player is on a lift (using ray)
    /// Sets the bool value "on ground" for the animator autonomously
    /// </summary>
    public class PlayerGroundHandler : MonoBehaviour
    {
        [SerializeField] private Animator mAnimator;
        [SerializeField] private LayerMask liftLayer;
        private readonly float mRaycastDistance = 2f;
        private LiftManager mLiftManager;
        private bool isGround;
        public bool IsGround
        {
            get => isGround;
            set => throw new InvalidOperationException("isGround cannot be set.");
        }
        public bool OnLift
        {
            get => mLiftManager != null;
            set => throw new InvalidOperationException("OnLift cannot be set.");
        }

        public LiftManager Lift
        {
            get => mLiftManager;
            set => throw new InvalidOperationException("mLiftManager cannot be set.");
        }
        

        private void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, mRaycastDistance, liftLayer))
            {
                mLiftManager = hit.collider ? hit.collider.gameObject.GetComponent<LiftManager>() : null;
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                isGround = true;
            }
        }

        private void OnTriggerStay(Collider col)
        {
            if(GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                isGround = true;
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if(GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.InteractableSurface))
            {
                isGround = false;
            }
        }
    }
}
