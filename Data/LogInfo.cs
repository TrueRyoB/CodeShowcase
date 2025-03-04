using UnityEngine;

namespace Fujin.Data
{
    [CreateAssetMenu(fileName = "LI 0_0_0", menuName = "KaguoSpeedRun/LogInfo Template", order = 1)]
    public class LogInfo : ScriptableObject 
    {
        public enum DisplayCondition {
            Always,
            OnlyFirstTime,
            FromSecondTime,
        }

        public string Text;
        public IconType IconTypeLog;
        public DisplayCondition DisplayConditionLog;
    }
}