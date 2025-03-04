using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Mobs
{
    public class AchievementsContainerController : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private Rigidbody rb;

        private void Start()
        {
            if (rb == null)
            {
                Debug.LogError("Error: rb is not assigned to the gameObject AchievementsContainer.");
                rb = GetComponent<Rigidbody>();
            }
            
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        public void StartMoving()
        {
            rb.velocity = Vector3.back * speed;
        }

        public void PauseMoving()
        {
            rb.velocity = Vector3.zero;
        }
    }
}