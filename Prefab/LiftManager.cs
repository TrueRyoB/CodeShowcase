using System.Collections;
using UnityEngine;
using System;

namespace Fujin.Prefab
{
    public enum MoveP //move pattern
    {
        Constant,
        Accelerate,
    }
    public enum TriggerP //trigger pattern
    {
        AlwaysActive,
        UponTouch,
        BeforeTouch,
    }

    public class LiftManager : MonoBehaviour
    {
        [SerializeField][Header("MovePattern")] private MoveP mP = MoveP.Constant;
        [SerializeField][Header("TriggerPattern")] private TriggerP tP = TriggerP.UponTouch;
        [SerializeField][Header("Reposition Itself? (always true for Constant)")] private bool willReposition = true;

        [SerializeField][Header("AccelRate")]   private float acceleration = 50f;
        [SerializeField][Header("MoveSpeed")]   private float mMoveSpeed = 20f;
        [SerializeField][Header("Final Speed for Accel")] private float mMaxSpeed = 2000f;
        [SerializeField][Header("Edges")]       private Transform[] mEnds;
        
        private Rigidbody2D rb;

        private int index;
        private int n;  //size of _ends

        private Vector2 oldPos; 
        private Vector2 myVelocity;//exists as a kinetic obj cannot implicitly generate vel from rb

        private Coroutine accelCoroutine;
        private bool isPaused;
        private bool tempVoluntarily;

        public Vector2 Velocity
        {
            get => myVelocity;
            set => throw new InvalidOperationException("Velocity cannot be set.");
        }
        
        private void Start() 
        {
            rb = GetComponent<Rigidbody2D>();
            if (mEnds?.Length > 0 && rb != null) {
                n = mEnds.Length;
                rb.position = mEnds[0].position;
            }
            else
                Debug.LogError("One of the lifts is not properly assigned its ends.");
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if(tP == TriggerP.UponTouch && collision.gameObject.CompareTag(Constants.GameObjectTag.Player))
            {
                if (mP == MoveP.Accelerate && accelCoroutine == null) { //assuming the lift is already at _ends[0].position
                    accelCoroutine = StartCoroutine(SuddenAccel(mEnds[0].position, mEnds[1].position, true));
                } else if (mP == MoveP.Constant) {
                    tempVoluntarily = true;
                }
            }
            if(tP == TriggerP.BeforeTouch && collision.gameObject.CompareTag(Constants.GameObjectTag.Player))
            {
                isPaused = true;
            }
        }
        void OnCollisionExit2D(Collision2D collision)
        {
            if(tP == TriggerP.BeforeTouch && collision.gameObject.CompareTag(Constants.GameObjectTag.Player))
                isPaused = false;
        }

        private IEnumerator SuddenAccel(Vector3 start, Vector3 target, bool isOutbound)
        {
            float initialSpeed = 0f;
            float finalSpeed = mMaxSpeed;
            Vector3 direction = (target - start).normalized;

            float currentSpeed = initialSpeed;

            while (true)
            {
                if (currentSpeed < finalSpeed){
                    currentSpeed += acceleration * Time.deltaTime;
                    currentSpeed = Mathf.Min(currentSpeed, finalSpeed);
                }

                Vector3 nextPosition = transform.position + direction * currentSpeed * Time.deltaTime;
                Vector3 toTarget = target - transform.position;
                if (Vector3.Dot(toTarget, direction) <= 0) {
                    rb.MovePosition(target); 
                    break; 
                }

                rb.MovePosition(nextPosition);

                myVelocity = ((Vector2)transform.position - oldPos) * Time.deltaTime;
                oldPos = transform.position;

                yield return null; 
            }

            rb.MovePosition(target);

            if(willReposition && isOutbound) {
                StartCoroutine(SuddenAccel(target, start, false));
            } else {
                accelCoroutine = null;
            }
        }

        private int count;

        private void FixedUpdate()
        {
            if((mP == MoveP.Constant) && (tP == TriggerP.AlwaysActive || tempVoluntarily) && !isPaused)
            {
                //keep moving until the distance between this object and the target is smaller than epsilon
                if(Vector2.Distance(rb.position, mEnds[index].position) > 0.1f){
                    Vector2 toVector = Vector2.MoveTowards(rb.position, mEnds[index].position, mMoveSpeed * Time.deltaTime);
                    rb.MovePosition(toVector);
                }
                else {
                    rb.MovePosition(mEnds[index].position);
                    index = Increment(index);
                    if (index == 1) 
                    {
                        count++;
                        if (count >= 2) {
                            tempVoluntarily = false;
                            count = 1; 
                        }
                    }
                }
            }

            myVelocity = (rb.position - oldPos) / Time.deltaTime;
            oldPos = rb.position;
        }

        private int Increment(int x)
        {
            return (x < n-1)? x+1 : 0;
        }
    }
}
