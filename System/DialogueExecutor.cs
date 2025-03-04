using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Fujin.System
{
    public class DialogueExecutor : MonoBehaviour
    {
        private Dictionary<string, Action> commandMap;
        //[SerializeField]private ShadowMotion shadow;
        [SerializeField]private GameFlagManager gameFlagManager;
        //[SerializeField]private GameDataManager gameDataManager;
        [SerializeField]private GameFlowManager gameFlowManager;
        public static bool IsAfterTutorial;

        private void InitializeMap()
        {
            commandMap = new Dictionary<string, Action>
            {
                //{"countdown_3", () => gameDataManager.StartRecording()},
                //{"goal_0", () => gameFlowManager.PlayerGoaled()},
                //{"t_3_11", () => SummonShadow(new Vector2(580f, 97.8801f))},
                //{"t_5_0", () => shadow.Recognized()},
                //{"t_5_5", () => shadow.Performance_StageT()},
                //{"t_6_0", () => SummonShadow(new Vector2(640f, 60f))},
                {"t_7_4", Tut2Stage1},
                //{"",},
                //{"",},
            };
        }

        public void ExecuteInNeed(string logID)
        {
            if(commandMap == null)
                InitializeMap();
            if(commandMap != null && commandMap.TryGetValue(logID, out Action action)) {
                action.Invoke();
            }
        }

        // private void SummonShadow(Vector2 loc)
        // {
        //     if(shadow != null) {
        //         shadow.gameObject.transform.position = loc;
        //         shadow.HappyDance();
        //     } else {
        //         Debug.LogError("A game object with a tag Shadow was not found!");
        //     }
        // }
        private void Tut2Stage1()
        {
            // gameFlagManager.isAfterTutorial = true;
            MarkPresent();
            LoadSceneOf("Stage1");
        }

        private void LoadSceneOf(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void MarkPresent()
        {
            IsAfterTutorial = true;
        }
    }
}