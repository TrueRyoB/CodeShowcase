using Fujin.Constants;
using UnityEngine;
using Fujin.System;
using System.Collections;
using Fujin.UI;

namespace Fujin.Player
{
    public sealed class PlayerController : PlayerPhysics
    {
        private readonly InputProcessor processorType = InputProcessor.PlatformerNavigationAction;

        // Extremely trash code needs to be deleted asap
        public void HideWarning()
        {
            Move(ActionTypePlatformer.RunLeft);
        }
        
        /// <summary>
        /// Toggle the reading permission externally
        /// Cannot be called by itself
        /// </summary>
        /// <param name="value"></param>
        private void ToggleReadingPermission(bool value)
        {
            SetCanRead(value);
        }

        protected override void Start()
        {
            base.Start();
            GameInputHandler.Instance.RegisterInputReader(processorType, ToggleReadingPermission);
        }
        
        private void SetMenuPlatformer()
        {
            // Make the GameInputHandler toggleReadingPermission
            GameInputHandler.Instance.SwitchProcessorTo(InputProcessor.PlatformerMenuAction);
            
            // Ideally, interrupt the input recorder (and deprive the recording of a star mark)
        }

        private void Record(ActionTypePlatformer value, Vector2? vec = null)
        {
            InputRecorder.Instance.Record(value, vec);
        }

        //TODO: playerControllerとplayerPhysicsでInput Keyが跨っているのをどうにかする(共通Dictionary<enum, List<KeyCode>>の参照とか？)
        private bool UpKeyPressed() => 
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        private bool DownKeyPressed() => 
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        
        private bool DownKeyReleased() => 
            Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S);
        
        private bool LeftKeyPressed() => 
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        private bool LeftKeyReleased() =>
            Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A);
        
        private bool RightKeyPressed() =>
            Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        
        private bool RightKeyReleased() =>
            Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D);

        private bool ConfirmKeyPressed() => 
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z);
        
        private bool ConfirmKeyReleased() => 
            Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Z);

        private bool DebugKeyPressed() =>
            Input.GetKeyDown(KeyCode.B);
        
        private bool DebugKeyReleased() =>
            Input.GetKeyUp(KeyCode.B);

        private bool EscKeyPressed() => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab);


        private const float CursorUpdateInterval = 0.05f;
        private float lastCursorActiveTime;
        private Vector2 currentCursorPos = Vector2.zero;
        private Vector2 lastCursorPos = Vector2.zero;

        private bool IsCursorActive() => Time.time - lastCursorActiveTime <= CursorUpdateInterval;

        private float GetCursorMvAngle()
        {
            Vector2 delta = currentCursorPos - lastCursorPos;
            if (delta.magnitude > 0.1f)
            {
                return Mathf.Atan2(currentCursorPos.y - lastCursorPos.y, currentCursorPos.x - lastCursorPos.x) *
                       Mathf.Rad2Deg;
            }
            return 0f;
        }

        private void ReadCursorMovement()
        {
            currentCursorPos = Input.mousePosition;
            Vector2 delta = currentCursorPos - lastCursorPos;

            // Update the active time only if the movement occurs intentionally (not by accident)
            if (delta.magnitude > 0.1f) 
            {
                lastCursorActiveTime = Time.time;
            }
            
            lastCursorPos = currentCursorPos;
        }

        private Coroutine toggleCameraCoroutine;
        private readonly WaitForSeconds waitForToggleTap = new WaitForSeconds(InputTimeConfig.MaxTapThreshold);
        private bool withinCtCancelDuration;
        
        private IEnumerator ToggleTapTimer()
        {
            withinCtCancelDuration = true;
            yield return waitForToggleTap;
            withinCtCancelDuration = false;
            toggleCameraCoroutine = null;
        }

        private void ParseKeyInput()
        {
            if (UpKeyPressed() && DownKeyPressed() && CanSpin)
            {
                CancelDive();
                SetCrouching(false);
                Record(ActionTypePlatformer.CancelDive);
                Spin();
                Record(ActionTypePlatformer.Spin);
            }
            else if (UpKeyPressed() && Jumpable && !IsSpinning && !IsCrouching)
            {
                Jump();
                Record(ActionTypePlatformer.Jump);
            }
            else if (DownKeyPressed() && !IsSpinning)
            {
                // Priority: Spin >> Slide >> Cancel Dive >> Dive >> Crouch
                
                if (OnWall)
                {
                    SetSliding(true);
                    Record(ActionTypePlatformer.Slide);
                }
                else if (IsDiving)
                {
                    CancelDive();
                    Record(ActionTypePlatformer.CancelDive);
                }
                else if (CanDive && PlayerEnvState.OnGround != envState)
                {
                    Dive();
                    Record(ActionTypePlatformer.Dive);
                }
                else if(PlayerEnvState.OnGround == envState)
                {
                    SetCrouching(true);
                    Record(ActionTypePlatformer.Crouch);
                }
            }

            if (DownKeyReleased())
            {
                if (IsSliding)
                {
                    SetSliding(false);
                    Record(ActionTypePlatformer.CancelSlide);
                }
                else if (IsCrouching)
                {
                    SetCrouching(false);
                    Record(ActionTypePlatformer.CancelCrouch);
                }
            }

            if (LeftKeyPressed())
            {
                OnSideKey(true);
            }
            else if (LeftKeyReleased())
            {
                OnSideKey(false);
            }

            if (RightKeyPressed())
            {
                OnSideKey(true);
            }
            else if(RightKeyReleased())
            {
                OnSideKey(false);
            }
            
            if (ConfirmKeyPressed())
            {
                if (IsSpinning) return;
                
                // Priority: Cancel Rocket >> SteamBoost >> Consume (Gate >> HeatTackle >> Appeal)
                if (IsRocketing)
                {
                    if (CanRocket() && IsCursorActive())
                    {
                        float angle = GetCursorMvAngle();
                        Vector2 angleVec = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Rocket(angleVec);
                        Record(ActionTypePlatformer.Rocket, angleVec);
                    }
                    else
                    {
                        CancelRocket();
                        Record(ActionTypePlatformer.CancelRocket);
                    }
                }
                else if (IsCrouching)
                {
                    SteamBoost();
                    Record(ActionTypePlatformer.SteamBoost);
                }
                else if (CanGate())
                {
                    Gate();
                    Record(ActionTypePlatformer.Gate);
                }
                else if (CanRocket())
                {
                    if (IsCursorActive())
                    {
                        // Get a unit vec and return it
                        float angle = GetCursorMvAngle();
                        Vector2 angleVec = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Rocket(angleVec);
                        Record(ActionTypePlatformer.Rocket, angleVec);
                    }
                    else
                    {
                        AimForRocket();
                        Record(ActionTypePlatformer.AimForRocket);
                    }
                }
                else
                {
                    Appeal();
                }
            }
            else if (ConfirmKeyReleased())
            {
                if (IsAiming)
                {
                    Vector2 cursorPos = CursorManager.Instance.GetPosOnScreen();
                    Vector2 playerPos = UnityEngine.Camera.main!.WorldToScreenPoint(transform.position);
                    Vector2 angleVec = new Vector2(cursorPos.x - playerPos.x, cursorPos.y - playerPos.y).normalized;
                    
                    Rocket(angleVec);
                    Record(ActionTypePlatformer.Rocket, angleVec);
                }
            }

            if (DebugKeyPressed())
            {
                Debug.Log("Morning, world");
            }
            else if (DebugKeyReleased())
            {
                Debug.Log("Night, world");
            }

            if (EscKeyPressed())
            {
                SetMenuPlatformer();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                PCamera.SetHardTracking(true);
                if (toggleCameraCoroutine != null)
                {
                    StopCoroutine(toggleCameraCoroutine);
                }
                toggleCameraCoroutine = StartCoroutine(ToggleTapTimer());
            }
            else if (Input.GetKeyUp(KeyCode.T))
            {
                if (withinCtCancelDuration)
                {
                    PCamera.SetBoundedBox(true);
                }
                PCamera.DisableHardTrack();
            }
        }


        private float elapsedTime;

        /// <summary>
        /// 0. Read cursor movements
        /// 1. Call the function physically on key inputs
        /// 2. Record the action history
        /// </summary>
        protected override void Update()
        {
            base.Update();
            
            // Not called during the replay mode
            if (CanRead)
            {
                if (elapsedTime >= CursorUpdateInterval)
                {
                    ReadCursorMovement();
                    elapsedTime = 0f;
                }
                ParseKeyInput();
                elapsedTime += Time.deltaTime;
            }
        }
    }
}