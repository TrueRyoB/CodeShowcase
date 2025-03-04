using UnityEngine;

namespace Fujin.ScriptableObjects
{
    /// <summary>
    /// Was thinking of automatic assignments on valueForLevelN and bonusScore based on the difficulty/prevalence
    /// for reduced capacity loads but maybe later on... i guess?
    /// </summary>
    public enum AchievementType
    {
        EasyPrevalent,
        MediumNormal,
        HardRare,
    }
    
    [CreateAssetMenu(fileName = "NewAchievementData", menuName = "Fujin/AchievementData")]
    public class AchievementData : ScriptableObject
    {
        public int id;
        public string title;
        public string description;
        public int bonusScore;
        public Sprite icon;
        public float valueForLevel2;
        public float valueForLevel3;

        public bool MeetsErupted(int currentScore, int additionalScore)
        {
            return ((currentScore < valueForLevel2 && currentScore > valueForLevel2 - additionalScore) ||
                    (currentScore < valueForLevel3 && currentScore > valueForLevel3 - additionalScore));
        }
    }
}