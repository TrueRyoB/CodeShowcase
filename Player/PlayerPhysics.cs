using System.Collections;
using UnityEngine;
using Fujin.Constants;
using Fujin.System;
using Fujin.Prefab;
using Fujin.Mobs;
using System;
using Fujin.UI;
using Fujin.Camera;


namespace Fujin.Player
{
    public enum PlayerEnvState
    {
        OnGround,
        OnAir,
        OnWater,
    }
    
    /// <summary>
    /// designed to make a playable character responsive to a timeframe-command framework
    /// for implementing a replay system
    ///
    /// still has a function Update to control the flow efficiently
    /// </summary>
    public class PlayerPhysics : MonoBehaviour
    {
        [SerializeField] protected GameObject footPrefab;
        private readonly float speed = 10f;
        private readonly float boundsPower = 100f; 
        private readonly float jumpForce = 1.8f;//500f
        private  PlayerActionParticleEmitter mAPEmitter;
        private  PlayerGroundHandler mGroundHandler;
        [SerializeField]private PlayerStatusDisplayer mStatusDisplayer;

        private Rigidbody rb;
        private SpriteRenderer sp;
        private Animator anm;
        private float horizontalKey;

        protected bool Movable => movable;
        private bool movable = true;
        protected bool Jumpable = true;
        private bool canDive = true;
        protected bool CanDive => canDive;
        protected bool HasJustBeenPressed;
        protected bool IsFollowedByTap;
        private bool isFacingRight;
        private bool isRunning;
        private bool isDiving;
        public bool IsDiving => isDiving;
        private bool isCrouching;
        protected bool IsCrouching => isCrouching;

        //for dive
        [SerializeField][Tooltip("diveSpeed = diveScaler * speed")]
        private float diveScalerBase = 2f;
        [SerializeField][Tooltip("time took in 45 to -45 transition in seconds")]
        private float transTimeBase = 0.2f;
        private float diveScaler;
        private float transTime;
        [SerializeField][Range(20f, 70f)]private float initAngle = 70f;
        private Coroutine mDiveCoroutine;
        private Coroutine mSlamCoroutine;
        private Vector2 diveVec = Vector2.zero;
        private float maxFallSpeed;

        //for reading signs
        private string signID = "";
        private DialoguePlayer dialoguePlayer;

        //for animation shift optimization
        private int isWalkingHash;
        private int isNinjaHash;
        private int chargeHash;
        private int crouchHash;
        private int danceHash;
        private int diveHash;
        private int idleHash;
        private int runHash;
        private int slamHash;
        private int spinHash;
        private int rocketHash;

        private int slamDurationHash;
        private int rocketDurationHash;
        
        //for a better control of a unit
        public PlayerEnvState envState = PlayerEnvState.OnGround;
        private GameFlowManager gameFlowManager;
        private GameCollectionManager collectionManager;
        
        //for kagu energy storage
        private int kaguEnergyLevel;
        
        //for kagu energy consumption
        [SerializeField] private KeProgressBarManager kePbManager;
        private bool isAiming;
        protected bool IsAiming => isAiming;
        private bool isRocketing;
        protected bool IsRocketing => isRocketing;
        private bool isGating;
        protected bool IsGating => isGating;
        
        //for evade
        //private bool canEvade;
        
        //for spinning
        private bool canSpin;
        private readonly float spinForce = 1.6f;
        protected bool CanSpin => canSpin;
        private bool isSpinning;
        protected bool IsSpinning => isSpinning;
        private readonly float spinDuration = 0.8f;
        private readonly float spinSpeedMultiplier = 0.8f;
        private Coroutine mSpinCoroutine;
        private bool isInsideTerrain;
        private readonly float maxEscapeDistance = 100f;
        private readonly float incrementEscape = 5f;
        private readonly float playerWidth;
        private readonly float teleportTime = 0.1f;
        private readonly float slamDuration = 0.6f;
        
        //for running
        private Coroutine tapCoroutine;
        private readonly WaitForSeconds waitWhileTapActive = new WaitForSeconds(DoubleTapActiveDuration);
        private bool isDoubleTapActive;
        private float lastPressTime;
        private readonly float tapThreshold = 0.2f;
        private const float DoubleTapActiveDuration = 0.2f;
        private float horizontalInput;
        private readonly float runSpeedMultiplier = 1.3f;
        private Coroutine runCoroutine;
        
        //for lift
        private Vector3 liftModifier = Vector3.zero;
        
        //for mini-furniture gimmick
        private float furnitureLevel;
        private readonly float furnitureLevelThreshold = 100f;

        protected CameraControllerPlatformer PCamera => pCamera;
        private CameraControllerPlatformer pCamera;

        private bool canRead;
        protected bool CanRead => canRead;

        protected void SetCanRead(bool value)
        {
            canRead = value;
            ChangeTimeScaleTo(1, true);
        }

        protected virtual void Start()
        {
            pCamera ??= FindObjectOfType<CameraControllerPlatformer>();
            rb = GetComponent<Rigidbody>();
            maxFallSpeed = speed * -3f;
            anm = GetComponent<Animator>();
            sp = GetComponent<SpriteRenderer>();
            mAPEmitter = GetComponent<PlayerActionParticleEmitter>();
            InitializeAnimationHash();
            gameFlowManager = FindObjectOfType<GameFlowManager>();
            collectionManager = gameFlowManager.GetComponent<GameCollectionManager>();
            rb.useGravity = false;
            UpdateGravityUse();
            if (footPrefab != null) 
                mGroundHandler = footPrefab.GetComponent<PlayerGroundHandler>();
        }
        private void InitializeAnimationHash()
        {
            isWalkingHash = Animator.StringToHash("isWalking");
            isNinjaHash = Animator.StringToHash("isNinja");
            chargeHash = Animator.StringToHash("charge");
            crouchHash = Animator.StringToHash("crouch");
            danceHash = Animator.StringToHash("dance");
            diveHash = Animator.StringToHash("dive");
            idleHash = Animator.StringToHash("idle");
            runHash = Animator.StringToHash("run");
            slamHash = Animator.StringToHash("slam");
            spinHash = Animator.StringToHash("spin");
            rocketHash = Animator.StringToHash("rocket");
            
            slamDurationHash = Animator.StringToHash("slamDuration");
            rocketDurationHash = Animator.StringToHash("rocketDuration");
        }

        private bool onWall;
        private bool isSliding;
        protected bool IsSliding => isSliding;
        public bool OnWall => onWall;
        [SerializeField] private PhysicMaterial playerMaterial;
        private Coroutine slideCoroutine;
        
        /// <summary>
        /// Is called on wall to minimize the vertical friction
        /// </summary>
        protected void SetSliding(bool value)
        {
            if (isSliding != value)
            {
                AddAcceleration(Vector3.down * 50f, !value);
                isSliding = value;
            
                if (value)
                {
                    slideCoroutine ??= StartCoroutine(PerformSliding());
                }
            }
        }

        private IEnumerator PerformSliding()
        {
            float elapsedTime = 0f;
            while (isSliding)
            {
                mAPEmitter.Splash();
                if (elapsedTime >= KaguEnergyMap.Keep.SlideSfc)
                {
                    ChangeKaguEnergyBy(KaguEnergyMap.Keep.Slide);
                    elapsedTime = 0f;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            slideCoroutine = null;
        }
        
        private DialoguePlayer DialoguePlayer
        {
            get {
                dialoguePlayer ??= FindObjectOfType<DialoguePlayer>();
                return dialoguePlayer;
            }
        }
        
        public void BoostDiving(float effectiveness, float angle, float duration)
        {
            // diveScaler = effectiveness;
            // initAngle = angle;
            // transTime = duration;
        }
        

        protected void ReadSign()
        {
            Stun();
            DialoguePlayer.ShowUI();
            DialoguePlayer.Narrate(signID);
        }

        /// <summary>
        /// Is called by class GoalRocketManager of gameObject GoalRocket(demo) to show a sign of reaching a goal.
        /// </summary>
        public void ReachGoal()
        {
            gameFlowManager.GoalAchieved();
            Stun();
        }

        /// <summary>
        /// Is called by OnTriggerEnter() to
        /// 1. play a cool VFX
        /// 2. make an update in the UI
        /// </summary>
        /// <param name="pieceIndex"></param>
        private void CollectPiece(int pieceIndex)
        {
            collectionManager.CollectPieceOf(pieceIndex);
        }

        /// <summary>
        /// Helper method
        /// </summary>
        private void ChangeKaguEnergyBy(int value, bool isMetabolic = false) 
        {
            kaguEnergyLevel = Mathf.Max(Mathf.Min(kaguEnergyLevel + value, KaguEnergyMap.Keep.MaxStorageSize), 0);
            if (!isMetabolic)
            {
                kePbManager.ApplyProgress(kaguEnergyLevel);
            }
        }
        
        private readonly WaitForSeconds waitForSteamStatus = new WaitForSeconds(0.2f);

        private IEnumerator UpdateSteamStatus()
        {
            mStatusDisplayer.Register(PlayerStatus.Steaming);
            yield return waitForSteamStatus;
            mStatusDisplayer.Register(PlayerStatus.Steaming, true);
            steamStatusCoroutine = null;
        }

        private Coroutine steamStatusCoroutine;
        
        protected void SteamBoost()
        {
            if (steamStatusCoroutine != null)
            {
                StopCoroutine(steamStatusCoroutine);
            }

            StartCoroutine(UpdateSteamStatus());
            anm.SetTrigger(chargeHash);
            ChangeKaguEnergyBy(KaguEnergyMap.Keep.SteamBoost);
        }

        /// <summary>
        /// Helper function for class PlayerController
        /// </summary>
        /// <returns></returns>
        protected bool CanGate()
        {
            return false;
        }

        protected void Gate()
        {
            
        }

        private int numCartridge;

        protected bool CanRocket()
        {
            return kaguEnergyLevel > KaguEnergyMap.Use.Rocket || numCartridge > 0;
        }

        protected void Appeal()
        {
            Dance();
        }
        

        private readonly float aimTimeScale = 0.2f;
        private readonly float defaultTimeScale = 1.0f;
        private const float AimDuration = 3f;
        private Coroutine aimCoroutine;

        /// <summary>
        /// Is called when the confirmation key is pressed with a sufficient amount of energy
        /// ...to slow time (but not OST and stop watch), black out a bit, and display a cursor;
        /// This stat is automatically cancelled if held more than permitted (let's say explosionHoldCap)
        /// </summary>
        protected void AimForRocket()
        {
            isAiming = true;
            mStatusDisplayer.Register(PlayerStatus.Aiming);
            
            pCamera.SetAiming(true);

            Vector3 worldToScreen = UnityEngine.Camera.main!.WorldToScreenPoint(rb.position);
            worldToScreen.x = worldToScreen.x / Screen.width * 1980f;
            worldToScreen.y = worldToScreen.y / Screen.height * 1080f;
            
            // Display a cursor
            CursorManager.Instance.SetCursorTracking(true, worldToScreen);
            CursorManager.Instance.ChangeCursorSensitivity(5f, 0.2f, AimDuration); 
            
            // Change the timeScale
            ChangeTimeScaleTo(aimTimeScale);
            
            // Change the theme
            pCamera.SetTimeSlowPpv(true);
            
            // Start count down
            if (aimCoroutine != null)
            {
                StopCoroutine(aimCoroutine);
            }
            aimCoroutine = StartCoroutine(CancelAimOnExpiration());
        }

        private void RevertFromAim()
        {
            mStatusDisplayer.Register(PlayerStatus.Aiming, true);
            
            // Prevent double call
            if (aimCoroutine != null)
            {
                StopCoroutine(aimCoroutine);
                aimCoroutine = null;
            }
            
            // Consume kagu energy
            ChangeKaguEnergyBy(-KaguEnergyMap.Use.Rocket);

            pCamera.SetAiming(false);
            if (!isAiming) return;
            
            // Revert settings only if necessary
            isAiming = false;
            CursorManager.Instance.SetCursorTracking(false);
            ChangeTimeScaleTo(defaultTimeScale);
            pCamera.SetTimeSlowPpv(false);
        }

        private IEnumerator CancelAimOnExpiration()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < AimDuration)
            {
                // Pause counting when opening a menu ( == when PlayerController is set inactive)
                if (canRead)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                }

                yield return null;
            }
            
            RevertFromAim();
            aimCoroutine = null;
        }

        private float lastTimeScale = 1f;

        private void ChangeTimeScaleTo(float value, bool ignoreValue = false)
        {
            if (value > 1f || value < 0f) return;
            
            // Stop time when player controller is inactive
            Time.timeScale = canRead ? (ignoreValue ? lastTimeScale : value) : 0f; 
            lastTimeScale = canRead ? Time.timeScale : lastTimeScale;
        }



        private const float RocketEarlyMS = 30f; //40f
        private const float RocketMidMS = 5f; //15f
        private const float RocketLateMS = 2f; //10f
        
        private const float RocketDuration = 0.6f;
        private const float RocketInitRatio = 0.2f;
        private const float RocketMidRatio = 0.4f;
        private const float RocketLateRatio = 0.4f;

        private readonly WaitForSeconds waitForRocketInit = new WaitForSeconds(RocketInitRatio * RocketDuration);
        private readonly WaitForSeconds waitForRocketMid = new WaitForSeconds(RocketMidRatio * RocketDuration);
        private readonly WaitForSeconds waitForRocketLate = new WaitForSeconds(RocketLateRatio * RocketDuration);
        
        
        private Vector3 rocketVec;

        private Coroutine rocketPsCoroutine;
        private readonly float rocketPsPS = 0.05f; //particle system par second (OMG)

        private IEnumerator EmitRocketPs(Vector3 lookVec)
        {
            float elapsedTime = 0f;

            while (isRocketing)
            {
                if (elapsedTime >= rocketPsPS)
                {
                    mAPEmitter.RocketLaunch(lookVec);
                    elapsedTime = 0f;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            rocketPsCoroutine = null;
        }

        /// <summary>
        /// Only time constraint is controlled here; collisions and interruptions must be regulated externally by calling CancelRocket()
        /// </summary>
        /// <param name="targetUnitVec"></param>
        /// <param name="earlyMS"></param>
        /// <param name="midMS"></param>
        /// <param name="lateMS"></param>
        /// <returns></returns>
        private IEnumerator PerformRocket(Vector2 targetUnitVec, 
            float earlyMS = RocketEarlyMS, float midMS = RocketMidMS, float lateMS = RocketLateMS)
        {
            // 0) Be mad
            if (!Number.IsEqualFloat(targetUnitVec.magnitude, 1f))
            {
                Debug.Log("NONONNONONOONONONONONONOONONONONONONONONONONONONONONONONOONONONONONONNONONO");
                targetUnitVec.Normalize();
            }

            if (targetUnitVec.magnitude <= Mathf.Epsilon)
            {
                Debug.Log("Small vector was automatically replaced with a forward one");
                targetUnitVec = Vector2.right;
            }
            
            // 1) Set the variables and animation
            mStatusDisplayer.Register(PlayerStatus.Rocketing);
            isRocketing = true;
            
            isFacingRight = targetUnitVec.x >= 0f;
            AdjustSpriteFlip();
            
            UpdateGravityUse();
            anm.SetFloat(rocketDurationHash, 1f/RocketDuration);
            anm.SetTrigger(rocketHash);
            
            // 2) Start emitting the PS
            if (rocketPsCoroutine != null)
            {
                StopCoroutine(rocketPsCoroutine);
            }
            rocketPsCoroutine = StartCoroutine(EmitRocketPs(-targetUnitVec));
            
            // 3) Change speed over time 
            rocketVec = targetUnitVec * earlyMS;
            yield return waitForRocketInit;
            
            rocketVec = targetUnitVec * midMS;
            yield return waitForRocketMid;
            
            rocketVec = targetUnitVec * lateMS;
            yield return waitForRocketLate;
            
            // 6) Cancel rocketing by itself 
            rocketCoroutine = null;
            CancelRocket();
        }
        
        /// <summary>
        /// Dash to where the cursor is while stunning itself by burning a fixed amount of energy.
        /// </summary>
        protected void Rocket(Vector2 targetUnitVec)
        {
            RevertFromAim();
            if (rocketCoroutine != null)
            {
                CancelRocket();
            }
            rocketCoroutine = StartCoroutine(PerformRocket(targetUnitVec));
        }

        private Coroutine rocketCoroutine;

        /// <summary>
        /// Call this if you want to stop rocketing
        /// </summary>
        protected void CancelRocket()
        {
            mStatusDisplayer.Register(PlayerStatus.Rocketing, true);
            anm.SetTrigger(idleHash);
            isRocketing = false;
            AdjustSpriteFlip();
            if (rocketCoroutine != null)
            {
                StopCoroutine(rocketCoroutine);
                rocketCoroutine = null;
            }

            if (rocketPsCoroutine != null)
            {
                StopCoroutine(rocketPsCoroutine);
                rocketPsCoroutine = null;
            }
        }
        

        /// <summary>
        /// Make a player jump at a given strength
        /// </summary>
        protected void Jump()
        {
            AddForce(transform.up * jumpForce);
            Jumpable = false;
        }

        /// <summary>
        /// make player float underwater (has cooldown to prevent spamming this)
        /// called in edge manner
        /// </summary>
        protected void Buoy() //uncompleted
        {
            //implementation is intentionally procrastinated as it's uncertain even
            //how underwater interaction should be like in this game
        }

        /// <summary>
        /// shuts down every input while not stopping game
        /// used when a dialogue is playing or when the menu is open
        /// </summary>
        public void Stun()
        { 
            movable = false;
            mStatusDisplayer.Register(PlayerStatus.Stunned);
            anm.speed = 0;
        }

        private bool isGravityActive;

        /// <summary>
        /// Makes sure that many variables do not compete over the registration of gravity to the player unit
        /// </summary>
        private void UpdateGravityUse()
        {
            bool needsGravity = !onWall || isRocketing; //add more variables here later on

            if (isGravityActive != needsGravity)
            {
                isGravityActive = needsGravity;
                AddAcceleration(Vector3.up * Physics.gravity.y, !needsGravity);
            }
        }

        /// <summary>
        /// restores normal input and movement functionality after being stunned.
        /// </summary>
        public void Cleanse()
        {
            mStatusDisplayer.Register(PlayerStatus.Stunned, true);
            movable = true;
            anm.speed = 1;
        }
        

        /// <summary>
        /// Makes a player sits down, resets its speed, and grants an immunity to subsequent movements
        /// ...while not interrupting energy charging, the byproduct of movement commands
        /// </summary>
        /// <param name="value"></param>
        protected void SetCrouching(bool value)
        {
            mStatusDisplayer.Register(PlayerStatus.Crouching, !value);
            anm.SetTrigger(value ? crouchHash : idleHash);
            isCrouching = value;
        }

        /// <summary>
        /// makes a player DIVE
        /// is automatically canceled upon a collision to regarding types of terrain (through CancelDive)
        ///
        /// should be functioning in hold manner
        /// </summary>
        protected void Dive() //unsupervised
        {
            mDiveCoroutine ??= StartCoroutine(PerformDive());
        }

        /// <summary>
        /// 1. Can dodge certain objects while active
        /// 2. Cannot cancel with another key input
        /// 3. Small jump
        /// 4. No side speed boost
        /// 5. Low efficiency in charging kagu energy
        /// </summary>
        protected void Spin()
        {
            mSpinCoroutine ??= StartCoroutine(PerformSpin());
        }

        /// <summary>
        /// Makes a playable character enter the helicopter mode for a solid one second,
        /// allowing a bypass of certain objects and a horizontal speed boost.
        /// A successful attempt resets the divable count (externally operated by OnTriggerExit)
        /// Remaining inside a terrain after a completion of this IEnumerator triggers an expulsion (by OnTriggerEnter)
        /// </summary>
        private IEnumerator PerformSpin()
        {
            Debug.Log("Spin is called!");
            canSpin = false;
            isSpinning = true;
            mStatusDisplayer.Register(PlayerStatus.Spinning);
            AddForce(transform.up * spinForce);
            
            anm.SetTrigger(spinHash);
            yield return new WaitForSeconds(spinDuration);
            
            // Expel the player if it remains inside the terrain
            if (isInsideTerrain)
            {
                isInsideTerrain = false;
                StartCoroutine(TeleportPlayerToOpenAreaAt(FindNonTerrainLayerForEscape(),
                    () =>
                    {
                        isSpinning = false;
                        mSpinCoroutine = null;
                        mStatusDisplayer.Register(PlayerStatus.Spinning, true);
                    }));
            }
            // Spin animation is canceled automatically by ExitTime at Animator
            isSpinning = false;
            mStatusDisplayer.Register(PlayerStatus.Spinning, true);
            canSpin = true;
            mSpinCoroutine = null;
        }

        /// <summary>
        /// Find the shortest distance horizontally to the non-terrain area
        /// Is called by TeleportPlayerToOpenAreaAt()
        /// </summary>
        private Vector3 FindNonTerrainLayerForEscape()
        {
            float currentDistance = 0f;
            LayerMask terrainLayer = LayerMask.GetMask(LayerTag.Terrain);
            while (currentDistance <= maxEscapeDistance)
            {
                Vector3 point;
                currentDistance += incrementEscape;
                if (!Physics.Raycast(rb.position, Vector3.right, out RaycastHit hit, currentDistance, terrainLayer))
                {
                    point = hit.point;
                    return new Vector3(point.x + playerWidth, point.y, point.z);
                }
                else if (!Physics.Raycast(rb.position, Vector3.left, out hit, currentDistance, terrainLayer))
                {
                    point = hit.point;
                    return new Vector3(point.x - playerWidth, point.y, point.z);
                }
            }
            return rb.position;
        }

        /// <summary>
        /// Is called by Spin() in case when a playable character is stuck inside a terrain
        /// Teleport-like movement not to mess with the OnTriggerExit functionality
        /// Still, manipulation of rigidbody position might cause inconsistency
        /// </summary>
        /// <param name="targetPosition"></param><param name="onComplete"></param>
        ///
        private IEnumerator TeleportPlayerToOpenAreaAt(Vector3 targetPosition, Action onComplete = null)
        {
            Vector3 startPosition = rb.position;
            float timeElapsed = 0f;

            while (timeElapsed < teleportTime)
            {
                rb.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / teleportTime);
                timeElapsed += Time.deltaTime;
                yield return null; 
            }
            rb.position = targetPosition;
            //leave a trail perhaps
            //discard the change after a brief delay
            onComplete?.Invoke();
        }

        /// <summary>
        /// Changes a moving direction over time within an angular boundary
        /// keeps falling as long as both flags (key not released and feet not on ground) are valid
        /// they are controlled externally by CancelDive(); 
        /// </summary>
        private IEnumerator PerformDive()
        {
            // Set the bool variable and animation
            canDive = false;
            isDiving = true;
            mStatusDisplayer.Register(PlayerStatus.Diving);
            anm.SetTrigger(diveHash);
            transTime = transTimeBase;
            diveScaler = diveScalerBase;
            
            // Initialize essential variables.
            float orientation = isFacingRight ? 1 : -1;
            float timer = Time.time;
            diveVec = VecOnTime(0f, orientation);
            
            // Changes the direction over time until either an angle is met or the key is released.
            while (Time.time < timer + transTime)
            {
                diveVec = VecOnTime(Time.time - timer, orientation);
                yield return null;
            }
            diveVec = VecOnTime(transTime, orientation);

            // Keeps diving until interrupted by CancelDive();
            while (IsDiving)
            {
                yield return null;
            }

            // Animation detail is covered by CancelDiveOnAir() or Slam() by the way.
            ResetDiveVariables();
        }
        
        /// <summary>
        /// Stop coroutine, reset vectors, and change animation
        /// </summary>
        protected void CancelDive()
        {
            // Cancel dive coroutine
            if (mDiveCoroutine != null)
            {
                StopCoroutine(mDiveCoroutine);
                ResetDiveVariables();
            }
            
            // back to normalcy
            anm.SetTrigger(idleHash);
        }
        
        /// <summary>
        /// Helper function for PerformDive() called by ResetCountOnLand() to
        /// ... set a special animation and make Kaguo unplayable for a brief moment
        /// ... then restores idle and dive count.
        ///
        /// Is called by PlayerSlamHandler() attached to a child gameObject SlamHandler
        /// </summary>
        public void Slam()
        {
            mSlamCoroutine ??= StartCoroutine(PerformSlam());
        }
        
        private IEnumerator PerformSlam()
        {
            // Cancel dive coroutine
            if (mDiveCoroutine != null)
            {
                StopCoroutine(mDiveCoroutine);
                ResetDiveVariables();
            }
            
            // Enter slam state
            anm.SetFloat(slamDurationHash, slamDuration);
            anm.SetTrigger(slamHash);
            Stun();
            
            // Bounce a playable unit
            Debug.LogWarning("Warning: Bounce mechanism is not implemented yet at PerformSlam()!");
            for (float i = 0; i <= slamDuration; i += Time.deltaTime)
            {
                // Insert a code here
                yield return null;
            }

            // Restores a normal state
            Cleanse();
            canDive = true;
            mSlamCoroutine = null;
        }

        /// <summary>
        /// Helper function called by PerformDive(), PerformSlam(), or CancelDiveOnAir()
        /// </summary>
        private void ResetDiveVariables()
        {
            isDiving = false;
            diveScaler = diveScalerBase;
            transTime = transTimeBase;
            diveVec = Vector3.zero;
            mDiveCoroutine = null;
            mStatusDisplayer.Register(PlayerStatus.Diving, true);
        }
        
        
        /// <summary>
        /// Performs or de-perform an adorable ninja pose by switching the animation variable and flipX
        /// Is called by PlayerSideHandler.cs
        /// </summary>
        /// <param name="value"></param>
        public void SetOnWall(bool value)
        {
            if (onWall == value) return;
            
            anm.SetBool(isNinjaHash, value);
            onWall = value;
            UpdateGravityUse();
            AdjustSpriteFlip();
        }

        public void SetCanEvade(bool value)
        {
            Debug.LogError("Error: evade is not implemented");
            //canEvade = value;
        }

        /// <summary>
        /// Helper function that must be called everytime as a change onWall or isFacingRight occurs.
        /// </summary>
        private void AdjustSpriteFlip()
        {
            sp.flipX = onWall ^ isFacingRight ^ isRocketing;
        }
        

        /// <summary>
        /// Sets an appropriate animation and perform a cool dance
        /// Called by GameFlowManager at the result scene
        /// Functionality is similar to SetNinja
        /// </summary>
        /// <param name="value"></param>
        private void Dance(bool value = true)
        {
            if (!value)
            {
                Debug.LogError("Error: a dev was not free enough to implement a dance lasting as long as it's set true!!");
            }
            anm.SetTrigger(value?danceHash:idleHash);
        }

        //private Coroutine tapCoroutine;
        // private bool isDoubleTapActive;
        // private float lastPressTime;
        // private readonly float tapThreshold = 0.2f;
        // private float horizontalInput;
        // private readonly float runSpeedMultiplier = 1.3f;
        // private Coroutine runCoroutine;
        

        /// <summary>
        /// Only called in the actual gameplay to effectively
        /// 1. read the horizontal key and convert it into the actionType
        /// 2. detect double tap for run()
        /// </summary>
        /// <param name="pressed"></param>
        protected void OnSideKey(bool pressed)
        {
            bool isRunningLocal = isRunning;
            if (pressed)
            {
                // Record the last input time stamp
                lastPressTime = Time.time;
                
                // Turn the run flag on if applicable
                if (isDoubleTapActive)
                {
                    isRunningLocal = true;
                }
            }
            else
            {
                // Activate double tap condition being met
                if (tapCoroutine != null)
                {
                    StopCoroutine(tapCoroutine);
                }
                tapCoroutine = (Time.time-lastPressTime < tapThreshold) ? StartCoroutine(CountDoubleTapCoolDown()) : null;
            }
            
            float horizontal = GetUpdatedHorizontalAxis();

            if (horizontal == 0f)
            {
                InputRecorder.Instance.Record(ActionTypePlatformer.StopMoving);
                Move(ActionTypePlatformer.StopMoving);
            }
            else
            {
                ActionTypePlatformer action;
                if (horizontal > 0f)
                {
                    action = (isRunningLocal)?ActionTypePlatformer.RunRight:ActionTypePlatformer.WalkRight;
                }
                else
                {
                    action = (isRunningLocal)?ActionTypePlatformer.RunLeft:ActionTypePlatformer.WalkLeft;
                }
                
                // Register
                InputRecorder.Instance.Record(action);
                Move(action);
            }
        }

        protected void Move(ActionTypePlatformer action)
        {
            switch (action)
            {
                case ActionTypePlatformer.RunLeft:
                    horizontalInput = -1f;
                    SetRunning(true);
                    isFacingRight = false;
                    break;
                case ActionTypePlatformer.RunRight:
                    horizontalInput = 1f;
                    SetRunning(true);
                    isFacingRight = true;
                    break;
                case ActionTypePlatformer.WalkLeft:
                    horizontalInput = -1f;
                    SetWalking(true);
                    isFacingRight = false;
                    break;
                case ActionTypePlatformer.WalkRight: 
                    horizontalInput = 1f;
                    SetWalking(true);
                    isFacingRight = true;
                    break;
                case ActionTypePlatformer.StopMoving:
                    horizontalInput = 0f;
                    SetWalking(false);
                    SetRunning(false);
                        break;
            }
            AdjustSpriteFlip();
        }
        
        private IEnumerator CountDoubleTapCoolDown()
        {
            isDoubleTapActive = true;
            yield return waitWhileTapActive;
            isDoubleTapActive = false;
            tapCoroutine = null;
        }

        private float GetUpdatedHorizontalAxis()
        {
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }
            
            return horizontal;
        }
        
        /// <summary>
        /// Is called to control animation, VFX, and the flag isRunning that
        /// interferes with Update() in determining the xSpeed
        /// </summary>
        /// <param name="value"></param>
        private void SetRunning(bool value)
        {
            // Ignore if non-changing
            if (isRunning == value) return;
            
            isRunning = value;
            if (value)
            {
                anm.SetTrigger(runHash);
                runCoroutine ??= StartCoroutine(PlayRunVFX());
            }
            else
            {
                anm.SetBool(isWalkingHash, false);
                if (runCoroutine != null)
                {
                    StopCoroutine(runCoroutine);
                    runCoroutine = null; 
                }
            }
        }

        /// <summary>
        /// Is called to play either VFX or SFX while running
        /// Has to be called by runCoroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayRunVFX()
        {
            while (isRunning)
            {
                if (envState == PlayerEnvState.OnGround)
                {
                    mAPEmitter.Splash();
                }
                yield return null;
            }
            runCoroutine = null;
        }

        private bool isWalking;

        /// <summary>
        /// Is called to easily control animation (and SFX later on) regarding walk
        /// Speed is operated externally on hVelocity by Stride() so no need for modification here
        /// </summary>
        /// <param name="value"></param>
        private void SetWalking(bool value)
        {
            if (isWalking == value) return;

            isWalking = value;
            anm ??= GetComponent<Animator>();
            anm.SetBool(isWalkingHash, value);
        }
        
        /// <summary>
        /// Prevents a playable character from being stuck inside terrain forever
        /// </summary>
        public void UnstuckFromTerrainUpward()
        {
            if(rb.velocity.y is >= 0f and <= 10f) { 
                rb.position += Vector3.up * 1f;
            }
        }
        
        /// <summary>
        /// For an outside gameObject to switch it for stress-free
        /// </summary>
        protected void ToggleStun()
        {
            if (movable)
                Stun();
            else
                Cleanse();
        }

        private Vector3 mixedAcceleration;
        private Vector3 mixedVelocity;
        private Vector3 mixedImpulse;

        private void AddImpulse(Vector3 imp)
        {
            mixedImpulse += imp;
        }
        private void AddAcceleration(Vector3 acc, bool subtractive = false)
        {
            if (subtractive)
            {
                mixedAcceleration -= acc;
            }
            else
            {
                mixedAcceleration += acc;
            }
        }
        private void AddForce(Vector3 force)
        {
            mixedVelocity += force / rb.mass;
        }

        protected virtual void Update()
        {
            // For a character to move when the time is set 0
            if (isGating && Time.timeScale == 0f)
            {
                rb.velocity = GetPlayerVelocity();
            }
        }
        
        protected virtual void FixedUpdate()
        {
            rb.velocity = GetPlayerVelocity();
            if (mixedVelocity.y > 0f)
            {
                mixedVelocity.y = Mathf.Max(0f, mixedVelocity.y + Physics.gravity.y * Time.fixedDeltaTime);
            }
            mixedImpulse *= 1.0f - rb.drag * Time.fixedDeltaTime;
        }

        private Vector3 finalVel = Vector3.zero;
        
        /// <summary>
        /// Gets a 3d velocity of a player for substitution at Update()
        /// </summary>
        /// <returns></returns>
        private Vector3 GetPlayerVelocity()
        {
            // Limit horizontal displacement when crouching
            if (!movable)
            {
                return rb.velocity;
            }
            if (isCrouching)
            {
                return new Vector3(0f, Mathf.Max(rb.velocity.y, maxFallSpeed), 0f);
            }
            if (isRocketing)
            {
                return rocketVec;
            }
            
            // Incorporate Move(), lifts, and dive otherwise
            float xSpeed = speed * horizontalInput * (isRunning?runSpeedMultiplier:1f) * (isSpinning?spinSpeedMultiplier:1f);
            liftModifier.x = mGroundHandler.OnLift ? mGroundHandler.Lift.Velocity.x : 0f;

            // Finalize the calculation
            finalVel.x = IsDiving ? xSpeed + diveVec.x : xSpeed + liftModifier.x;
            finalVel.y = IsDiving ? diveVec.y
                : (onWall && !isSliding ? 0f : Mathf.Max(rb.velocity.y + liftModifier.y, maxFallSpeed));
            
            // Add external forces by v = at
            finalVel += mixedVelocity + mixedAcceleration*Time.deltaTime;
            
            return finalVel;
        }
        
        private Vector2 VecOnTime(float t, float ori)
        {
            float angle = (-(initAngle*2) * t / transTime + initAngle) * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            float multiplier = diveScaler * speed;

            return new Vector2(cosAngle * ori, sinAngle) * multiplier;
        }
        
        private void OnTriggerEnter(Collider obj)
        {
            if(obj.CompareTag(GameObjectTag.Sign)) {
                signID = obj.gameObject.GetComponent<SignManager>().ID;
            } else if(obj.CompareTag(GameObjectTag.Shadow))
            {
                //SignID = obj.transform.parent.gameObject.GetComponent<SignManager>().ID;
            } else if(obj.gameObject.CompareTag(GameObjectTag.StrongSign))
            {
                signID = obj.gameObject.GetComponent<SignManager>().ID;
                DialoguePlayer.HideUI();
                DialoguePlayer.Narrate(signID);
            } else if (obj.gameObject.CompareTag(GameObjectTag.GetterCheckpoint))
            {
                if (furnitureLevel > furnitureLevelThreshold) 
                {
                    CollectPiece(obj.GetComponent<GetterManager>().Index); 
                }
            }
        }
        private void OnTriggerExit(Collider obj)
        {
            if(obj.CompareTag(GameObjectTag.Sign) || obj.CompareTag(GameObjectTag.Shadow)) {
                signID = "";
            } 
        }

        /// <summary>
        /// Knocks back the playable character itself by a set strength boundsPower
        /// </summary>
        /// <param name="other"></param>
        private void KnockBackItself(Collision other)
        {
            Vector3 hitPos = other.contacts[0].point;
            Vector3 boundVec = rb.position - hitPos;

            Vector3 forceDir = boundsPower * boundVec.normalized;
            //rb.AddForce(forceDir, ForceMode.Impulse);
            AddImpulse(forceDir);
        }
        private void ResetCountOnLand(GameObject obj)
        {
            Jumpable = true;
            canDive = true; // Ignores every bouncy-related bullshit
            
            mGroundHandler ??= footPrefab.GetComponent<PlayerGroundHandler>();

            if (mGroundHandler.IsGround)
            {
                envState = PlayerEnvState.OnGround;
                SetSliding(false);
            }

            if (isRocketing)
            {
                CancelRocket();
            }
            
            // Reset a dive count 
            if (!canDive && (!isDiving || isDiving && !(obj.CompareTag(GameObjectTag.Bouncy) || envState == PlayerEnvState.OnGround)))
            {
                CancelDive();
            }
        }
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(GameObjectTag.Enemy))
            {
                KnockBackItself(other);
            }

            if (GameObjectTag.IsObjectInGroup(other.gameObject, TagGroup.JumpResetSurface))
            {
                ResetCountOnLand(other.gameObject);
            }

            if (other.gameObject.CompareTag(GameObjectTag.Bypassable) && isSpinning)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), other.gameObject.GetComponent<Collider>(), true);
                isInsideTerrain = true;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (GameObjectTag.IsObjectInGroup(other.gameObject, TagGroup.InteractableSurface))
            {
                mGroundHandler ??= footPrefab.GetComponent<PlayerGroundHandler>();
                envState = mGroundHandler.IsGround ? PlayerEnvState.OnGround : PlayerEnvState.OnAir;

                SetSliding(false);

                // Reset canDive and re-initialize states
                if (other.gameObject.CompareTag(GameObjectTag.Bypassable))
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), other.gameObject.GetComponent<Collider>(), false);
                    isInsideTerrain = false;
                    canDive = true;
                }
            }
        }

    }
}