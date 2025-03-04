using UnityEngine;
using System;
using Fujin.Data;

namespace Fujin.System
{
    /// <summary>
    /// Child class of ResultSceneUIManager to be called its functions by other class (like UI.Interactable.UIColorController)
    /// </summary>
    public sealed class ResultSceneUIController : ResultSceneUIManager
    {
        private ResultUIScene currentScene = ResultUIScene.Invalid;
        public ResultUIScene CurrentScene => currentScene;

        /// <summary>
        /// Is called by SceneLoadManager only once
        /// </summary>
        public void MechanicsOnSceneLoaded()
        {
            currentScene = ResultUIScene.Intro;
            PlayIntroUI();
        }
        
        /// <summary>
        /// Is called by either UIColorController (individuals) or ResultSceneAction
        /// </summary>
        /// <param name="toForward"></param>
        public void MoveRoom(bool toForward)
        {
            ResultUIScene nextScene = GetNextUIScene(currentScene, toForward);
            
            switch (nextScene)
            {
                case ResultUIScene.Calculation:
                    if (currentScene == ResultUIScene.Intro)
                        PlayTransitionFromIntroToCalculationUI();
                    else if (currentScene == ResultUIScene.PickDig) 
                        PlayTransitionFromPickDigToCalculationUI();
                    break;
                case ResultUIScene.PickDig:
                    PlayTransitionFromCalculationToPickDigUI();
                    break;
                case ResultUIScene.PreSceneLoad:
                    if (PreferenceDataManager.Instance.OmitCommenting) ExitResult(true);
                    else DisplayCommentBox();
                    break;
                default:
                    Debug.LogError($"Error: Invalid next scene selected: {nextScene}");
                    return;
            }
            
            currentScene = nextScene;
        }


        /// <summary>
        /// Helper function for function MoveRoom(bool toForward)
        /// </summary>
        /// <param name="targetScene"></param>
        /// <param name="toForward"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private ResultUIScene GetNextUIScene(ResultUIScene targetScene, bool toForward)
        {
            switch (targetScene)
            {
                case ResultUIScene.Intro:
                    if (toForward) return ResultUIScene.Calculation;
                    else throw new InvalidOperationException("The operation cannot be performed because it is simply impossible");
                case ResultUIScene.Calculation:
                    if (toForward) return ResultUIScene.PreSceneLoad;
                    else return ResultUIScene.PickDig;
                case ResultUIScene.PickDig:
                    if (toForward) return ResultUIScene.Calculation;
                    else throw new InvalidOperationException("The operation cannot be performed because it is simply impossible");
                case ResultUIScene.PreSceneLoad:
                case ResultUIScene.Invalid:
                default:
                    throw new InvalidOperationException("The operation cannot be performed because it is simply impossible");
            }
        }
    }
}