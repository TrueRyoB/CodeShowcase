using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Data
{
    /// <summary>
    /// Holds a list of datatype KeyCode to be used by DialogueData
    /// </summary>
    public class KeyBinding
    {
        public List<KeyCode> KeyCodes;

        public KeyBinding(List<KeyCode> keyCodes)
        {
            KeyCodes = keyCodes;
        }

        public KeyBinding()
        {
            KeyCodes = new List<KeyCode>();
        }
    }
}