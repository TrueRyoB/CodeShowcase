using System;
using Fujin.Constants;
using UnityEngine;
using Fujin.UI.Interactable;
using System.Collections;
using sysRandom = System.Random;

namespace Fujin.Player
{
    public class PlayerPhysicsResultScene : MonoBehaviour
    {
        private Rigidbody rb;
        private Animator anm;
        
        [Header("References")]
        [SerializeField] private UIInteractableController jumpButton;

        [Header("For adjustment")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float dashSpeed = 3f;
        [SerializeField] private float bigJumpHeight = 6f;
        [SerializeField] private float unit = 3f;
        [SerializeField] private float randomFallRangeHorizontal = 10f;
        [SerializeField] [Range(0.3f, 7f)] private float skateSpeed = 5f;
        
        private readonly float jumpStrength = 3f;

        private Vector3 rotation;
        private float previousMousePosX;
        
        private bool isJustClicked;
        private bool canRotate;

        private int freeFallHash;
        private int stepHash1;
        private int stepHash2;
        private int standStillHash;
        private int bigJumpHash1;
        private int bigJumpHash2;
        private int isDashingHash;
        private int isSkatingHash;

        private float maxFallSpeed;
        
        private void Start()
        {
            InitializeRbProperty();
            InitializeAnimatorHash();
        }

        private void InitializeRbProperty()
        {
            // Cheeky things deserve to be stirred out...lol
            rb = GetComponent<Rigidbody>();
            rb.drag = 0f;
            rb.angularDrag = 0f;
            maxFallSpeed = dashSpeed * 3f;
        }
        
        private void InitializeAnimatorHash()
        {
            anm = GetComponent<Animator>();
            
            freeFallHash = Animator.StringToHash("freeFall");
            stepHash1 = Animator.StringToHash("step_1");
            stepHash2 = Animator.StringToHash("step_2");
            bigJumpHash1 = Animator.StringToHash("bigJump_1");
            bigJumpHash2 = Animator.StringToHash("bigJump_2");
            standStillHash = Animator.StringToHash("standStill");
            isDashingHash = Animator.StringToHash("isDashing");
            isSkatingHash = Animator.StringToHash("isSkating");
        }

        private void Update()
        {
            if (canRotate)
            {
                if (isJustClicked)
                {
                    isJustClicked = false;
                    previousMousePosX = Input.mousePosition.x;
                }
                
                float horizontalInput = Input.GetAxis("Horizontal");

                if (!Number.IsEqualFloat(horizontalInput, 0f))
                {
                    // Reads player input (most prioritized)
                    rotation = Vector3.up * (horizontalInput * rotationSpeed * Time.deltaTime);
                }
                else if (!Number.IsEqualFloat(previousMousePosX, Input.mousePosition.x))
                {
                    // Reads mouse position relocation
                    rotation = Vector3.up * (Mathf.Sign(Input.mousePosition.x - previousMousePosX) * rotationSpeed * Time.deltaTime);
                    previousMousePosX = Input.mousePosition.x;
                }
                rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
            }
        }

        private event Action OnLand;

        /// <summary>
        /// Rocket doesn't exist in the Result scene so here we just pretend instead
        /// </summary>
        /// <param name="callback"></param>
        public void LandOffFromRocket(Action callback = null)
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main ?? FindObjectOfType<UnityEngine.Camera>();
            
            float z = rb.position.z - cam.transform.position.z;
            Vector3 screenPos = new Vector3(Screen.width / 2f, Screen.height + 10f, z);
            Vector3 summonPos = cam.ScreenToWorldPoint(screenPos);
            sysRandom random = new sysRandom(); //TODO:randomManager的なのを実装する seed値に則る
            summonPos.x += randomFallRangeHorizontal * ((float)random.NextDouble() -0.5f);
            summonPos.z += randomFallRangeHorizontal * ((float)random.NextDouble() -0.5f);
            
            Debug.Log($"summonPos: {summonPos}");

            anm.SetTrigger(freeFallHash);
            rb.position = summonPos;
            OnLand += callback;
        }

        private Coroutine danceCoroutine;

        /// <summary>
        /// Dance!!
        /// </summary>
        /// <param name="callback"></param>
        public void VictoryDance(Action callback = null)
        {
            danceCoroutine ??= StartCoroutine(PerformVictoryDance(callback));
        }

        [SerializeField] private Transform danceCenterTransform;
        

        /// <summary>
        /// Helper coroutine for VictoryDance(Action)
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator PerformVictoryDance(Action callback = null)
        {
            yield return StartCoroutine(DashToPos(danceCenterTransform.position));
            yield return StartCoroutine(TakeSteps(Vector3.right, 4));
            yield return StartCoroutine(BigJump(Vector3.left, 6));
            yield return StartCoroutine(RotateSkate(false, danceCenterTransform.position));
            yield return StartCoroutine(FinalPose());

            danceCoroutine = null;
            callback?.Invoke();
        }

        //TODO:
        private IEnumerator FinalPose()
        {
            yield return null;
        }
        
        /// <summary>
        /// Helper method for IEnumerator RotateSkate()
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        private IEnumerator RbMvInCircularTrajectoryTo(Vector3 targetPosition)
        {
            // Calculate a duration
            float duration = Vector3.Distance(transform.position, targetPosition) / skateSpeed;
            if (Mathf.Approximately(duration, 0f))
            {
                Debug.LogError("Error: duration cannot be close to 0.");
                yield break;
            }
            
            // Initialize / cache required components for both x and z
            float targetX = targetPosition.x - rb.position.x;
            float targetZ = targetPosition.z - rb.position.z;

            const float attenuationRateX = 0.21f;
            const float attenuationRateZ = 0.1f; // TODO: verify this
            const float periodX = Mathf.PI * 8f / 3f;
            const float periodZ = Mathf.PI * 4f / 3f;
            const float callEveryInSecond = 0.05f;
            
            const float ratioZ = 1.3f;

            float pOverDofX = periodX / duration;
            float pOverDofZ = periodZ / duration;
            
            float cachedEKpHalfCosPHalfX = Mathf.Exp(-attenuationRateX * periodX / 2f) * Mathf.Cos(periodX / 2f);
            float cachedEKpCosPNormalX = Mathf.Exp(-attenuationRateX * periodX);
            float yInterceptX = targetX / (1f - cachedEKpCosPNormalX);
            float slopeX = yInterceptX * (cachedEKpCosPNormalX - cachedEKpHalfCosPHalfX) / periodX;
            
            float cachedAsinOverRofZ = Mathf.Asin(-1f / ratioZ);
            float mWithoutLastPowZ = Mathf.Pow(periodZ, -attenuationRateZ) * Mathf.Log(Mathf.Asin(targetZ / (2f * ratioZ) / (-periodZ + cachedAsinOverRofZ)));
            float returnWithoutLastPowZ = 0.5f * targetZ * (1 + ratioZ * Mathf.Sin(cachedAsinOverRofZ - periodZ));
            
            float t = 0f;
            float updateTimer = 0f;
            Vector3 movePosVec = rb.position;
            
            Debug.Log("Started moving");
            
            while (t < duration)
            {
                if (updateTimer >= callEveryInSecond)
                {
                    updateTimer -= callEveryInSecond;
                    
                    float scaledTofX = t * pOverDofX;
                    float scaledTofZ = t * pOverDofZ;
                    
                    movePosVec.x = slopeX * scaledTofX + yInterceptX * (1f - Mathf.Exp(-attenuationRateX * scaledTofX) * Mathf.Cos(scaledTofX));
                    //TODO: replace this with a simplified formula
                    movePosVec.z = returnWithoutLastPowZ *
                              Mathf.Exp(mWithoutLastPowZ * Mathf.Pow(scaledTofZ, attenuationRateZ));
                        
                    rb.MovePosition(movePosVec);
                }

                t += Time.deltaTime;
                updateTimer += Time.deltaTime;
                yield return null;
            }

            Debug.Log("Completed relocation");
        }

        //TODO:
        private IEnumerator RotateSkate(bool clockwise, Vector3 targetPosition)
        {
            anm.SetBool(isSkatingHash, true);

            //TODO: clockwiseによる挙動の変化をまとめる (u,v) = (sinΘ*x + cosΘ*y, cosΘ*x + sinΘ*y)
            yield return RbMvInCircularTrajectoryTo(targetPosition);
            
            anm.SetBool(isSkatingHash, false);
            yield return null;
        }
        
        /// <summary>
        /// Height and gravity are fixed.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="distanceInUnit"></param>
        /// <returns></returns>
        private IEnumerator BigJump(Vector3 direction, int distanceInUnit)
        {
            float h = bigJumpHeight;
            float distance = distanceInUnit * unit; // horizontal distance
            Vector3 launchDirection = (direction.normalized * distance + Vector3.up * h).normalized;

            float g = Mathf.Abs(Physics.gravity.y);
            float sinTheta = h / (Mathf.Pow(h, 2) + Mathf.Pow(distance, 2));
            float initialVelocity = Mathf.Sqrt((sinTheta / g) + (Mathf.Pow(sinTheta, 2) / (2f * Mathf.Pow(g, 3))) / h);
            WaitForSeconds waitForHalfAJump = new WaitForSeconds(initialVelocity*sinTheta/g);
            
            anm.SetTrigger(bigJumpHash1);
            rb.AddForce(launchDirection * initialVelocity, ForceMode.Impulse);
            yield return waitForHalfAJump;
            
            anm.SetTrigger(bigJumpHash2);
            yield return waitForHalfAJump;
        }
        
        private Vector3 horizontalVec;

        /// <summary>
        /// Inherit only x and z components of the parameter vector
        /// </summary>
        private void FixHorizontalVec(in Vector3 refVec)
        {
            horizontalVec.x = refVec.x;
            horizontalVec.y = 0f;
            horizontalVec.z = refVec.z;
            horizontalVec.Normalize();
        }

        /// <summary>
        /// Take steps with alternating animations for a fixed unit;
        /// assumes that the gravity is set negative and there is no aerodynamic drag.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="n"></param>
        /// <param name="endStandStill"></param>
        /// <returns></returns>
        private IEnumerator TakeSteps(Vector3 direction, int n, bool endStandStill = false)
        {
            Vector3 launchDirection = (Vector3.up + direction).normalized;
            FixHorizontalVec(launchDirection);
            
            float rad = Vector3.Angle(launchDirection, horizontalVec);
            float initialVelocity = Mathf.Sqrt(unit * Mathf.Abs(Physics.gravity.y) / Mathf.Sin(2f * rad));
            WaitForSeconds waitForLanding = new WaitForSeconds(initialVelocity * Mathf.Cos(rad) / unit);
            
            for (int i = 0; i < n; ++i)
            {
                rb.AddForce(launchDirection * initialVelocity, ForceMode.Impulse);
                anm.SetTrigger(i%2==0 ? stepHash1 : stepHash2);
                yield return waitForLanding;
            }
            
            if(endStandStill) anm.SetTrigger(standStillHash);
        }

        /// <summary>
        /// Is called when you want a character to move at a constant speed to the specific location
        /// </summary>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        private IEnumerator DashToPos(Vector3 targetPos)
        {
            Vector3 startPos = rb.position;
            float duration = Vector3.Magnitude(targetPos - startPos) / dashSpeed;
            float elapsedTime = 0f;
            SetDash(true);
            
            while (elapsedTime < duration)
            {
                float t = Mathf.Clamp01(elapsedTime / duration);
                rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            SetDash(false);
            rb.MovePosition(targetPos);
            rb.velocity = Vector3.zero;
            yield return null;
        }

        //TODO: add a PS effect here
        private void SetDash(bool isActive)
        {
            Debug.Log($"Set dash {isActive}");
            anm.SetBool(isDashingHash, isActive);
        }
        
        public void Jump()
        {
            rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (GameObjectTag.IsObjectInGroup(col.gameObject, TagGroup.JumpResetSurface))
            {
                jumpButton.ReEnable();
                OnLand?.Invoke();
                OnLand = null;
            }
        }

        public void BanRotation()
        {
            canRotate = false;
        }
        public void AllowRotation()
        {
            canRotate = true;
            isJustClicked = true;
        }
    }
}