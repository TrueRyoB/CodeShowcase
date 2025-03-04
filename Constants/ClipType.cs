using System;

namespace Fujin.Constants
{
    /// <summary>
    /// Is used by class SoundInfo to store the clip time
    /// </summary>
    [Serializable]
    public enum ClipType
    {
        Invalid,
        Music,  // BGM that is affected by the game state (msc)
        BGM,    // loops, is persistent across scene, and does not stop on pause (bgm)
        DynamicSFX,    // is affected by the game state (even by the menu pause) (dse)
        StaticSFX,      // is played regardless of the game state (sse)
        Oneshot,     // its duration is too short to be registered (st)
    }
}