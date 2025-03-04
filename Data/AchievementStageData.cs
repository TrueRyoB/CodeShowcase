using System;

namespace Fujin.Data
{
    /// <summary>
    /// Temporarily stores info regarding achievements, specifically about its ID, bonus score, and count for a quick sort;
    /// has an attribute [Serializable] for nomenclature rule alleviation so is never saved.
    /// </summary>
    [Serializable]
    public class AchievementStageData
    {
        public int id;
        public int bonusScore;
        public int count;
        
        public AchievementStageData(int id, int bonusScore, int count)
        {
            this.id = id;
            this.bonusScore = bonusScore;
            this.count = count;
        }
    }
}