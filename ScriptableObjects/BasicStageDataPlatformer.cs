using UnityEngine;
using System.Collections.Generic;
using Fujin.Data;

namespace Fujin.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewStageData", menuName = "Fujin/StageData")]
    public class BasicStageDataPlatformer : ScriptableObject
    {
        public string stageID;
        public string stageName;

        public int goalTimeLimit;
        
        public int count;
        public List<Sprite> collectionSprites;

        public int bronzeThreshold;
        public int silverThreshold;
        public int goldThreshold;
        public int dokiThreshold;

        public string StageID
        {
            get => $"Stage {stageID}";
        }

        [Tooltip("Insert regarding scene name here")]public string goalVideoName;
        public VideoFlags videoFlag = VideoFlags.None;

        public BasicStageDataPlatformer()
        {
        }
    }
}