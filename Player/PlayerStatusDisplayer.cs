using UnityEngine;
using TMPro;
using Fujin.Constants;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Fujin.Player
{

    public class PlayerStatusDisplayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusHolder;
        private int numStatus;

        private readonly Dictionary<PlayerStatus, string> statusMap = new Dictionary<PlayerStatus, string>
        {
            { PlayerStatus.Default, "KAGUO"},
            { PlayerStatus.Crouching, "CROUCHING" },
            { PlayerStatus.Diving , "DIVING" }, 
            { PlayerStatus.Aiming , "AIMING"},
            { PlayerStatus.Rocketing, "ROCKETING" }, 
            { PlayerStatus.Spinning , "SPINNING" }, 
            { PlayerStatus.Gating , "GATE" }, 
            { PlayerStatus.Akeboshing , "AKEBOSHI" }, 
            { PlayerStatus.Stunned , "STUNNED" }, 
            { PlayerStatus.Slippery , "SLIPPERY" }, 
            { PlayerStatus.Frozen , "FROZEN" }, 
            { PlayerStatus.Steaming , "STEAMING"},
            { PlayerStatus.Slammed, "SLAMMED"},
        };

        private List<bool> playerStats;

        public void Register(PlayerStatus status, bool subtractive = false, bool updateText = true)
        {
            // If the value is non-changing and is set true, there is no change in both the value and the text displayed
            if (!subtractive && playerStats[(int)status])
            {
                return;
            }
            
            playerStats[(int)status] = !subtractive;

            if (updateText)
            {
                for (int i = 0; i < numStatus; ++i)
                {
                    if (playerStats[i])
                    {
                        UpdateText((PlayerStatus)i);
                        return;
                    }
                }
            }
        }

        private void UpdateText(PlayerStatus status)
        {
            if (statusMap.TryGetValue(status, out string statusText))
            {
                statusHolder.text = statusText;
            }
        }

        private void Start()
        {
            UpdateText(PlayerStatus.Default);
            numStatus = Enum.GetValues(typeof(PlayerStatus)).Length;
            playerStats = Enumerable.Repeat(false, numStatus).ToList();
            playerStats[numStatus-1] = true;
        }
    }
}