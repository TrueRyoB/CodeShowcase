using UnityEditor;
using UnityEngine;
using Fujin.Data;
using Fujin.Framework;
using Fujin.Constants;
using UnityEngine.SceneManagement;
using System;
using Fujin.ScriptableObjects;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections;

namespace Fujin.System
{
    /// <summary>
    /// responsible for making permanent classes adapt to scene changes
    /// </summary>
    [RequireComponent(typeof(MenuNavigationAction))]
    [RequireComponent(typeof(ResultSceneAction))]
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] private GameDataManager dataManager;
        [SerializeField] private GameInputHandler inputHandler;
        [SerializeField] private GameScoreManager scoreManager;
        [SerializeField] private GameCollectionManager collectionManager;
        [SerializeField] private SceneLoadManager sceneLoadManager;
        public int platformerSeed;

        /// <summary>
        /// Generate a seed of a positive # based on tokyo time reproducible and return it afterward.
        /// </summary>
        /// <returns></returns>
        public int GetRegeneratedPlatformerSeed()
        {
            DateTime tokyoTime = DateTime.UtcNow.AddHours(9);
            int combinedTime = tokyoTime.Hour*10000 + tokyoTime.Minute*100 + tokyoTime.Second;
            
            platformerSeed = combinedTime.GetHashCode();
            return platformerSeed;
        }
        
        private void Start()
        {
            // Subscribe to the event
            InputRecorder.Instance.OnRecordingComplete += dataManager.HandleRecordingComplete;//TODO:
            
            DontDestroyOnLoad(gameObject); // Persist across scenes
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Load stage info
            if (_stageInfoInstance == null)
            {
                StageInfoLoadCompleted = false;
                StartCoroutine(InitializeStageCoroutine());
            }
            else
            {
                StageInfoLoadCompleted = true;
            }
        }

        /// <summary>
        /// Is called by PlayerController(platformer) to...
        /// 1. make GameInputHandler call TogglePause()
        /// 2. show a cool UI
        /// 3. and more
        /// </summary>
        public void TogglePause()
        {
             //inputHandler.TogglePause();
            //TODO: add more logics here
        }

        /// <summary>
        /// Is called by PlayerPhysics(platformer) to...
        /// 1. stop timer by calling StopRecording() of class GameInputHandler
        /// 2. make a scene transition to the result screen
        /// 3. and more
        /// </summary>
        public void GoalAchieved()
        {
            CallForCompletingReplayDataInstance();
            sceneLoadManager.LoadScene(SceneName.ResultScene); 
        }

        /// <summary>
        /// Helper function for GoalAchieved() that collects every info other than player comment before the result scene is loaded
        /// </summary>
        private void CallForCompletingReplayDataInstance()
        {
            scoreManager.PassScoreToReplayDataManager(); // Score
            collectionManager.PassHoldingPieceToReplayDataManager(); // Piece collected
            ReplayDataManager.Instance.SetStageName(GetCurrentSceneName()); // StageName
            ReplayDataManager.Instance.SetSeedValue(platformerSeed); // Seed Value
            ReplayDataManager.Instance.SetDate(DateTime.Now); // Date
        }

        private static BasicStageDataPlatformer _stageInfoInstance;
        
        /// <summary>
        /// For other class to register events involving a reference to the stage data
        /// </summary>
        public static event Action OnStageInfoLoaded;
        
        /// <summary>
        /// Show if the load is completed or not
        /// </summary>
        public static bool StageInfoLoadCompleted { get; private set; }

        public static BasicStageDataPlatformer StageInfoInstance
        {
            get
            {
                if (_stageInfoInstance == null)
                {
                    Debug.LogError("Error: stage info instance is not loaded from a group of scriptable objects!!");
                    return ScriptableObject.CreateInstance<BasicStageDataPlatformer>();
                }
                return _stageInfoInstance;
            }
        }

        private IEnumerator InitializeStageCoroutine()
        {
            var handle = LoadStageInfoAsync();
            yield return new WaitUntil(() => handle.IsCompleted);

            if (handle.Exception != null)
            {
                Debug.LogError($"Error: An error occurred: {handle.Exception}");
            }
            else if (handle.Result != null)
            {
                _stageInfoInstance = handle.Result;
            }
            else
            {
                Debug.LogError("Error: Stage data loading failed.");
            }
            
            StageInfoLoadCompleted = true;
            OnStageInfoLoaded?.Invoke();
            OnStageInfoLoaded = null;
        }
        
        /*
         * 実装すること
         *
         * Scriptable Objectの動的読み込みに時間がかかる為、Start()などで初期化前に呼び出されたら困る
         *
         * どうするか？
         * ・こちらからロード後に呼ぶようにする
         * ・向こうにロード後まで待つようプログラムを作る
         */

        private async Task<BasicStageDataPlatformer> LoadStageInfoAsync()
        {
            //string address = $"Stage Info Platformer/{ParseStageName(GetCurrentSceneName())}"; TODO:
            string address = "Stage Info Platformer/1-1";//debug 
            var handle = Addressables.LoadAssetAsync<BasicStageDataPlatformer>(address);
            await handle.Task;

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }
            else
            {
                Debug.LogError($"Error: failed to load stage data: {address}");
                return null;
            }
        }

        /// <summary>
        /// Remove the prefix for searching scriptable objects
        /// </summary>
        /// <param name="stageName"></param>
        /// <returns></returns>
        public string ParseStageName(string stageName)
        {
            const string prefix = "Stage ";
            if (stageName.StartsWith(prefix))
            {
                Debug.Log($"Successfully extracted the substring of stage name {stageName}");
                return stageName.Substring(prefix.Length);
            }
            else
            {
                Debug.LogError($"Error: failed to extract the substring of stage name {stageName}");
                return stageName;
            }
        }

        /// <summary>
        /// Get current Scene Name
        /// TODO: replace this with something more decent
        /// </summary>
        private string GetCurrentSceneName()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!SceneName.IsValidSceneName(currentSceneName))
            {
                Debug.LogError($"Error: the scene name {currentSceneName} is not registered properly as a constant value of static class Constants.SceneName");
            }

            return currentSceneName;
        }
        
        /// <summary>
        /// Initialize/load non-critical elements after the load is completed for a smooth transition.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Handle logic post-scene loading
        }

        
        public void ExitGame()
        {
            #if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
            #else
                Application.Quit();
            #endif  
        }
    }
}
