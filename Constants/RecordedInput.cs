using System;
using UnityEngine;

namespace Fujin.Constants
{
    [Serializable]
    public class RecordedInput
    {
        public ActionTypePlatformer actionType;
        public float timeStamp;
        public Vector2? Vec;

        public RecordedInput(ActionTypePlatformer actionType, float executedTime, Vector2? vec = null)
        {
            this.actionType = actionType;
            timeStamp = executedTime;
            Vec = vec;
        }
    }
}