using System;
using System.Collections.Generic;

namespace Fujin.Data
{
    /// <summary>
    /// To save player progress on their achievements;
    /// store info by id and value.
    /// </summary>
    [Serializable]
    public class AchievementProgressData
    {
        public int id;
        public int value;

        public AchievementProgressData(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    public class AchievementProgressDataWrapper
    {
        public List<AchievementProgressData> data;

        public AchievementProgressDataWrapper(List<AchievementProgressData> data)
        {
            this.data = data;
        }
    }
}