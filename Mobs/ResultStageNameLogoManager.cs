using Fujin.System;
using TMPro;
using UnityEngine;

namespace Fujin.Mobs
{
    public class ResultStageNameLogoManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textHolder;
        private void Start()
        {
            textHolder.text = GameFlowManager.StageInfoInstance.stageName;
        }
    }
}