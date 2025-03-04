
using System;

namespace Fujin.Constants
{
    /// <summary>
    /// Is used by PlayerController and InputRecorder to parse a function calling in a recording mode
    /// </summary>
    [Serializable]
    public enum ActionTypePlatformer
    {
        Invalid,
        Jump,
        Spin,
        Slide,
        CancelSlide,
        SteamBoost,
        AimForRocket,
        Rocket,
        CancelRocket,
        WalkLeft,
        WalkRight,
        RunLeft,
        RunRight,
        StopMoving,
        Appeal,
        Crouch,
        CancelCrouch,
        Dive,
        CancelDive,
        Gate,
        ToggleMenu,
        ToggleCamera,
        SummonBug,
    }
}