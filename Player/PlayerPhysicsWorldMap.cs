using Fujin.Camera;
using UnityEngine;
using Fujin.Constants;
using Fujin.Mobs;
using Fujin.System;

namespace Fujin.Player
{
    public enum Direction
    {
        East,
        West,
        South,
        North,
    }
    public class PlayerPhysicsWorldMap : MonoBehaviour
    {
        private Vector3 totalInputDir;
        private bool isWalking;
        private bool isDashing;
        private bool isOnPavement;
        protected bool isStunned;

        // Kaguo scale in the world map
        private readonly float kaguoScale = 0.5f;

        private Direction isFacing = Direction.South;

        private readonly float speedModifier = 4.0f;
        private readonly float dashSpeedModifier = 1.6f;
        
        private Rigidbody rb;
        private SpriteRenderer sp;
        private Animator anm;

        private int walkSideHash;
        private int walkFrontHash;
        private int stopSideHash;
        private int stopFrontHash;
        
        protected GameInputHandler InputHandler;
        private SceneLoadManager sceneLoadManager;
        protected CameraController CameraController;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameObjectTag.StageNode))
            {
                sceneLoadManager.LoadScene(other.GetComponent<StageNodeManager>().stageName);
                SetStunnedStatus(true);
            }

            if (other.CompareTag(GameObjectTag.Pavement))
            {
                isOnPavement = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(GameObjectTag.Pavement))
            {
                isOnPavement = false;
            }
        }

        private void Start()
        {
            sp = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody>();
            anm = GetComponent<Animator>();
            rb.position = new Vector3(20, 0.4f, 0);
            InputHandler = FindObjectOfType<GameInputHandler>();
            sceneLoadManager = InputHandler.gameObject.GetComponent<SceneLoadManager>();
            CameraController = FindObjectOfType<CameraController>();
            GetAnmHash();
            transform.localScale = new Vector3(kaguoScale, kaguoScale, kaguoScale);
        }

        public bool IsOnPavement => isOnPavement;

        private void GetAnmHash()
        {
            walkSideHash = Animator.StringToHash("walk side");
            walkFrontHash = Animator.StringToHash("walk front");
            stopSideHash = Animator.StringToHash("stop side");
            stopFrontHash = Animator.StringToHash("stop front");
        }

        // Move around to eight direction while switching animation
        private void Update()
        {
            rb.velocity = GetPlayerVelocity();
        }
        
        // Enter the stage once verified
        
        // Jumps over steps
        
        // Stops before the cliff

        /// <summary>
        /// Helper function for Update() that returns player velocity based on...
        /// 1. Dash status
        /// 2. TotalInputDir
        /// 3. Stunned status
        /// </summary>
        /// <returns></returns>
        private Vector3 GetPlayerVelocity()
        {
            Vector3 velocity = new Vector3(0f, rb.velocity.y, 0f);

            if (!isStunned)
            {
                Vector3 playerInputSpeed = totalInputDir * speedModifier;
                if (isDashing) playerInputSpeed *= dashSpeedModifier;
                velocity += playerInputSpeed;
            }
            
            // Leaving room for extension (such as by outside force)

            return velocity;
        }

        /// <summary>
        /// Is called by PlayerControllerWorldMap
        /// </summary>
        /// <param name="input"></param>
        protected void SetDashStatus(bool input)
        {
            isDashing = input;
        }
        protected void SetStunnedStatus(bool input)
        {
            isStunned = input;
        }
        
        /// <summary>
        /// Register/Unregister direction; is called by controller.
        /// </summary>
        /// <param name="dir"></param>
        protected void Register(Direction dir)
        {
            FixNormalizedVector(ref totalInputDir);
            
            totalInputDir += GetVectorOf(dir);
            if (Number.IsEqualFloat(totalInputDir.magnitude, 0))
            {
                totalInputDir = Vector3.zero;
            }
            else
            {
                totalInputDir.Normalize();
            }
            TriggerAnimation(dir);
        }
        protected void Unregister(Direction dir)
        {
            FixNormalizedVector(ref totalInputDir);
            
            totalInputDir -= GetVectorOf(dir);
            if (Number.IsEqualFloat(totalInputDir.magnitude, 0))
            {
                totalInputDir = Vector3.zero;
            }
            else
            {
                totalInputDir.Normalize();
            }
            TriggerAnimation(dir);
        }
        
        /// <summary>
        /// Helper function for register/unregister
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private void FixNormalizedVector(ref Vector3 vector)
        {
            vector = new Vector3(
                vector.x == 0 ? 0 : Mathf.Sign(vector.x),
                vector.y == 0 ? 0 : Mathf.Sign(vector.y),
                vector.z == 0 ? 0 : Mathf.Sign(vector.z)
            );
        }

        /// <summary>
        /// Is called by Register/Unregister to change the sprite based on the input direction
        /// ...by using a dictionary indicating an apposite sprite for each direction (8 in total)
        ///
        /// ...although it's yet to be implemented due to insufficient Sprite resources.
        /// </summary>
        private void TriggerAnimation(Direction dir)
        {
            // flip character
            isFacing = GetNewDirectionPlayer();
            UpdateXFlip(isFacing);
            
            // return if walking state remains the same
            bool wasWalking = isWalking;
            isWalking = Number.IsEqualFloat(totalInputDir.magnitude, 1f);
            if (isWalking == wasWalking) return;
            
            // Determine animation trigger based on direction and walking state
            bool isVertical = (dir == Direction.North || dir == Direction.South);
            int walkHash = isVertical ? walkFrontHash : walkSideHash;
            int stopHash = isVertical ? stopFrontHash : stopSideHash;

            anm.SetTrigger(isWalking ? walkHash : stopHash);

        }

        /// <summary>
        /// Helper function for TriggerAnimation()
        /// </summary>
        /// <param name="dir"></param>
        private void UpdateXFlip(Direction dir)
        {
            sp.flipX = (dir == Direction.East);
        }

        /// <summary>
        /// Helper function for SwitchSprite() in acquiring Sprite.
        /// </summary>
        /// <returns></returns>
        private Direction GetNewDirectionPlayer()
        {
            // Handling edge case by returning itself
            if (totalInputDir == Vector3.zero)
            {
                return isFacing;
            }
            // Assuming camera is facing the direction as intended here
            float rad = Mathf.Atan2(totalInputDir.z, totalInputDir.x);

            if (Number.IsEqualFloat(rad, Mathf.PI/2))
            {
                return Direction.North;
            }
            else if (Number.IsEqualFloat(Mathf.Abs(rad), Mathf.PI/2))
            {
                return Direction.South;
            }
            else if (Mathf.Abs(rad) < Mathf.PI / 2)
            {
                return Direction.East;
            }
            else
            {
                return Direction.West;
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private Vector3 GetVectorOf(Direction dir)
        {
            switch (dir)
            {
                case Direction.East:
                    return new Vector3(1, 0, 0);
                case Direction.West:
                    return new Vector3(-1, 0, 0);
                case Direction.South:
                    return new Vector3(0, 0, -1);
                case Direction.North:
                    return new Vector3(0, 0, 1);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Helper function for class PlayerControllerWorldMap
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        protected Direction GetDirectionOf(InputActionType actionType)
        {
            switch (actionType)
            {
                case InputActionType.MoveRight:
                    return Direction.East;
                case InputActionType.MoveLeft:
                    return Direction.West;
                case InputActionType.MoveDown:
                    return Direction.South;
                case InputActionType.MoveUp:
                    return Direction.North;
                default:
                    Debug.LogError("Unknown property is passed at func GetDirectionOf in class PlayerPhysicsWorldMap.");
                    return Direction.East;
            }
        }
    }
}