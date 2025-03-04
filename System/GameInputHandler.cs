using System.Collections.Generic;
using UnityEngine;
using Fujin.Framework;
using System;
using Fujin.Constants;
using System.Collections;

namespace Fujin.System
{
    /// <summary>
    /// Activates/deactivates the input reading files by registering its action and its processor enum
    /// </summary>
    public class GameInputHandler : MonoBehaviour
    {
        private InputRecorder mInputRecorder;
        private bool shouldRecord;
        private readonly Dictionary<InputProcessor, Action<bool>> toggleInputReadingMap = new Dictionary<InputProcessor, Action<bool>>();
        private Action<bool> activeInputReader;

        public static GameInputHandler Instance => _instance;
        private static GameInputHandler _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register the wild input readers;
        /// automatically activates the first input reader registered.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="reader"></param>
        public void RegisterInputReader(InputProcessor processor, Action<bool> reader)
        {
            toggleInputReadingMap.Add(processor, reader);

            if (processor == InputProcessor.PlatformerNavigationAction)
            {
                SwitchProcessorTo(processor);
            }
        }

        private IEnumerator ActivateProcessorAtNextFrame(Action<bool> targetReader)
        {
            yield return null;
            targetReader.Invoke(true);
            activeInputReader = targetReader;
        }

        /// <summary>
        /// Switch the processor to the new one primarily called by GameFlowManager.
        /// </summary>
        /// <param name="processor"></param>
        public void SwitchProcessorTo(InputProcessor processor)
        {
            if (activeInputReader != null)
            {
                activeInputReader.Invoke(false);
            }
            
            if (toggleInputReadingMap.TryGetValue(processor, out Action<bool> targetReader))
            {
                StartCoroutine(ActivateProcessorAtNextFrame(targetReader));
            }
            else
            {
                Debug.LogError("Error: processor is not properly registered!");
            }
        }
    }
}
