using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Data
{

    [CreateAssetMenu(fileName = "LogSet 0_0", menuName = "KaguoSpeedRun/LogSet Template", order = 2)]
    public class LogSet : ScriptableObject  //for each sign
    {
        [Tooltip("stage # followed by sign #")]public string ID;
        public List<LogInfo> logInfos;
        public bool IsReadFirstTime { get; private set; }

        public void OnEnable() => IsReadFirstTime = true;

        public void MarkAsRead() => IsReadFirstTime = false;
    }
}