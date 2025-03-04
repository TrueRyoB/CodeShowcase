using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fujin.UI;
using Fujin.Constants;
using Fujin.Camera;
using System;
using UnityEngine.UI;

namespace Fujin.System
{
    public class SceneLoadManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameFlowManager gameFlowManager;

        [SerializeField] private GameObject panelPrefab;

        [SerializeField] private GameObject nothingTransitionPrefab;
        [SerializeField] private GameObject focusTransitionPrefab;
        [SerializeField] private GameObject fadeTransitionPrefab;
        [SerializeField] private GameObject spotlightTransitionPrefab;

        [SerializeField] private GameObject justWaitScreenPrefab;
        [SerializeField] private GameObject nothingScreenPrefab;
        [SerializeField] private GameObject tipsAndSpinScreenPrefab;
        
        private float mLoadingProgress;
        private CameraController cameraController;

        private Coroutine sceneLoadCoroutine;
        
        
        /// <summary>
        /// Loads a new scene while playing a relevant loading screen/animation
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadScene(string sceneName)
        {
            sceneLoadCoroutine ??= StartCoroutine(LoadSceneAsync(SceneName.GetLoadingType(sceneName), sceneName));
        }

        private bool canLoadNextScreen;

        /// <summary>
        /// Helper function called by LoadingPrefabController to demonstrate callbacks
        /// ...while sticking to a single IEnumerator.
        /// </summary>
        public void AllowLoading()
        {
            canLoadNextScreen = true;
        }

        private bool isWaitingForPlayerInput;

        /// <summary>
        /// Helper function called by LoadingScreenAction to demonstrate callbacks while
        /// ...preventing a spam/unexpected behavior from excessive player inputs.
        /// </summary>
        public void AllowLoadingByPlayer()
        {
            if (isWaitingForPlayerInput)
            {
                isWaitingForPlayerInput = false;
                canLoadNextScreen = true;
            }
        }

        private IEnumerator LoadSceneAsync(LoadingType loadingType, string sceneName)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError("CameraController not found.");
                yield break;
            }
            
            // あとで参照しやすいように解体を済ませておく
            LoadingTransitionType beginning = loadingType.BeginningTransition;
            LoadingScreenType loading = loadingType.LoadingScreen;
            LoadingTransitionType ending = loadingType.EndingTransition;
            
            // Tips内容変動やレア演出のためのシードを新規生成する
            gameFlowManager.GetRegeneratedPlatformerSeed();
            
            // beginning内の内容を読み取り、適したprefabを呼ぶ あとでSetActive(true)して呼ぶ
            GameObject beginningObject = GetDisabledTransitionPrefab(beginning);
            GameObject loadingObject = GetDisabledLoadingPrefab(loading);
            GameObject endingObject = GetDisabledTransitionPrefab(ending);
            GameObject backgroundObject = GetDisabledPanelFeaturing(beginning.BackgroundPattern);
            LoadingPrefabController backgroundController = backgroundObject.GetComponent<LoadingPrefabController>();
            Image backgroundImage = backgroundController.GetImageComponent();
            
            // A margin of one frame
            yield return null;
            
            // panelとbeginningを同時に表示することでmaskを間に合わせる
            canLoadNextScreen = false;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            backgroundController.PlayAnimation();
            LoadingPrefabController beginningController = beginningObject.GetComponent<LoadingPrefabController>();
            beginningController.InitializeBasedOn(backgroundImage);
            beginningController.InitializeBasedOn(cameraController);
            beginningController.PlayAnimation();
            
            // Play next animation as receiving a callback from beginningObject
            while (!canLoadNextScreen)
            {
                yield return null;
            }
            beginningController.EndAnimation();
            backgroundController.UpdateBackgroundTo(loading.BackgroundPattern);
            
            LoadingPrefabController loadingController = loadingObject.GetComponent<LoadingPrefabController>();
            loadingController.PlayAnimation();
            
            // Play next screen once the scene is fully loaded
            int frameInterval = Mathf.CeilToInt(1 / asyncLoad.progress);
            int frameCount = 0;

            while (asyncLoad.progress < 0.9f)
            {
                if (frameCount % frameInterval == 0)
                {
                    mLoadingProgress = asyncLoad.progress; 
                    loadingController.UpdateProgressBarBy(mLoadingProgress);
                }
                ++frameCount;
                yield return null;
            }
            
            // ここでloadingControllerが特別なものだったら足止めをさせる
            if (loadingController.RequiresConfirmation())
            {
                // 内装を変更
                loadingController.AwaitForConfirmation();
                
                // ここではConfirmボタン待ち LoadingScreenActionがこのクラスの関数を呼ぶ形で実装する
                canLoadNextScreen = false;
                isWaitingForPlayerInput = true; // Allows a player interception through AllowLoadingByPlayer()
                while (!canLoadNextScreen)
                {
                    yield return null;
                }
                loadingController.ActUponConfirmation();
                // ここではLoadingScreen.EndAnimationのcallback待ち
                canLoadNextScreen = false;
                
                loadingController.UpdateProgressBarBy(1f);
                asyncLoad.allowSceneActivation = true;
                
                // Wait for the next frame to ensure the scene is fully loaded
                yield return null;
                
                // Wait for callback of ActUponConfirmation
                while (!canLoadNextScreen)
                {
                    yield return null;
                }
            }
            else
            {
                // don't forget to show off honorable loading animations
                if (loading.Decoration != LoadingScreenDecoration.Nothing)
                {
                    yield return new WaitForSeconds(1f);
                }
                // load carefully and certainly
                loadingController.UpdateProgressBarBy(1f);
                asyncLoad.allowSceneActivation = true;
                
                // Wait for the next frame to ensure the scene is fully loaded
                yield return null;
                loadingController.EndAnimation();
            }
            
            canLoadNextScreen = false;
            backgroundController.UpdateBackgroundTo(ending.BackgroundPattern);
            LoadingPrefabController endController = endingObject.GetComponent<LoadingPrefabController>();
            endController.InitializeBasedOn(backgroundImage);
            cameraController = FindObjectOfType<CameraController>();
            if (ending.Means == LoadingTransitionMeans.SpotOut || ending.Means == LoadingTransitionMeans.SpotIn)
            {
                endController.InitializeBasedOn(cameraController.GetPlayerPositionOnScreen()); 
            }
            endController.InitializeBasedOn(cameraController);
            endController.PlayAnimation();
            
            // Switch the processor and call mechanics for the next scene
            CallMechanicsOnSceneLoaded(sceneName);
            
            // Unload unused assets to free up memory
            Resources.UnloadUnusedAssets();
            
            // Manually stop loading animation of the backgroundController
            while (!canLoadNextScreen)
            {
                yield return null;
            }
            endController.EndAnimation();

            yield return null;
            
            // Destroy every unwanted object because why not
            Destroy(backgroundObject);
            Destroy(beginningObject);
            Destroy(loadingObject);
            Destroy(endingObject);
            sceneLoadCoroutine = null;
        }


        /// <summary>
        /// Is called every time the scene is loaded to call a specific action
        /// ... that only works when the scene transition had taken place.
        /// </summary>
        /// <param name="sceneName"></param>
        private void CallMechanicsOnSceneLoaded(string sceneName)
        {
            switch (sceneName)
            {
                case SceneName.ResultScene:
                    FindObjectOfType<ResultSceneUIController>().MechanicsOnSceneLoaded();
                    break;
                case SceneName.Stage1_1:
                case SceneName.Stage1_2:
                    // Start a countdown timer and actually let it begin
                    break;
                //Add more mechanics here
            }
        }

        private GameObject GetDisabledPanelFeaturing(LoadingBackgroundPattern backgroundPattern)
        {
            GameObject panel = Instantiate(panelPrefab);
            panel.GetComponent<LoadingPrefabController>().UpdateBackgroundTo(backgroundPattern);
            
            DontDestroyOnLoad(panel);
            panel.SetActive(false);
            return panel;
        }

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="prefabInfo"></param>
        /// <returns></returns>
        private GameObject GetDisabledLoadingPrefab(LoadingScreenType prefabInfo)
        {
            GameObject res;
            LoadingPrefabController controller;
            switch (prefabInfo.Decoration)
            {
                case LoadingScreenDecoration.Nothing:
                    res = Instantiate(nothingScreenPrefab);
                    controller = res.GetComponent<LoadingPrefabController>();
                    break;
                case LoadingScreenDecoration.TipsAndSpin:
                    res = Instantiate(tipsAndSpinScreenPrefab);
                    controller = res.GetComponent<LoadingPrefabController>();
                    controller.InitializeBasedOn(gameFlowManager.platformerSeed);
                    break;
                case LoadingScreenDecoration.GirlPraying:
                case LoadingScreenDecoration.YukkuriSpin:
                    Debug.LogError("Error: No prefab had been designed for this loading decoration yet. (SceneLoadManager.cs)");
                    res = Instantiate(nothingScreenPrefab);
                    controller = res.GetComponent<LoadingPrefabController>();
                    break;
                case LoadingScreenDecoration.JustWait:
                    res = Instantiate(justWaitScreenPrefab);
                    controller = res.GetComponent<LoadingPrefabController>();
                    break;
                default:
                    res = Instantiate(nothingScreenPrefab);
                    controller = res.GetComponent<LoadingPrefabController>();
                    break;
            }
            controller.InitializeBasedOn(this);
            DontDestroyOnLoad(res);
            res.SetActive(false);
            return res;
        }
        
        private readonly Vector2 goalFocusOffset = Vector2.zero; //TODO: adjust a focus offset deployed at the switch case below

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="prefabInfo"></param>
        /// <returns></returns>
        private GameObject GetDisabledTransitionPrefab(LoadingTransitionType prefabInfo)
        {
            GameObject res = prefabInfo.Means switch
            {
                LoadingTransitionMeans.Nothing => Instantiate(nothingTransitionPrefab),
                LoadingTransitionMeans.Focus => Instantiate(focusTransitionPrefab),
                LoadingTransitionMeans.FadeIn or LoadingTransitionMeans.FadeOut => Instantiate(fadeTransitionPrefab),
                LoadingTransitionMeans.SpotIn or LoadingTransitionMeans.SpotOut => Instantiate(spotlightTransitionPrefab),
                LoadingTransitionMeans.SlideToRight => throw new NotImplementedException("No prefab designed for this transition means."),
                _ => throw new NotSupportedException("No prefab designed for this transition means."),
            };
            
            LoadingPrefabController controller = res.GetComponent<LoadingPrefabController>();

            switch (prefabInfo.Means)
            {
                case LoadingTransitionMeans.Focus: 
                    break;
                case LoadingTransitionMeans.FadeIn:
                    controller.InitializeBasedOn(InOrOut.In);
                    break;
                case LoadingTransitionMeans.FadeOut:
                    controller.InitializeBasedOn(InOrOut.Out);
                    break;
                case LoadingTransitionMeans.SpotIn:
                    controller.InitializeBasedOn(InOrOut.In);
                    controller.InitializeBasedOn(prefabInfo.PunchShape);
                    controller.InitializeBasedOn(cameraController.GetPlayerPositionOnScreen());
                    break;
                case LoadingTransitionMeans.SpotOut:
                    controller.InitializeBasedOn(InOrOut.Out);
                    controller.InitializeBasedOn(prefabInfo.PunchShape);
                    controller.InitializeBasedOn(cameraController.GetPlayerPositionOnScreen());
                    break;
            }
            controller.InitializeBasedOn(this);
            // DontDestroyOnLoadのおまじないのあとにおやすみのfalse
            DontDestroyOnLoad(res);
            res.SetActive(false);
            return res;
        }
        
    }
}