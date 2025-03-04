using Fujin.Constants;
using TMPro;
using UnityEngine;

namespace Fujin.Mobs
{
    /// <summary>
    /// Is attached to Scoreboard(UI) to update the UI given a value sent by class GameScoreManager.
    /// !! should be updated as soon as the implementing mechanics change from TMP to something else
    /// </summary>
    public class ScoreDisplayManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textHolder;
        [SerializeField] private string prefix = "SCORE: ";
        [SerializeField] private int digitLength = 10;
        private int score;
        public void UpdateUI(int newScore)
        {
            score = newScore;
            textHolder.text = prefix + Number.InsertCommaEveryThreeDigits(score, digitLength);
            // Play SE here
        }
    }
}