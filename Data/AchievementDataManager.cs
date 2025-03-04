using System;
using System.Collections.Generic;
using System.IO;
using Fujin.Constants;
using UnityEngine;
using Fujin.ScriptableObjects;

namespace Fujin.Data
{
    /// <summary>
    /// 1. Saves and loads data
    /// 2. Static function open for changes in value
    /// 3. Get info from Achievement Data by a quick search
    /// 4. Should be attached to the gameObject GameManager prefab
    /// </summary>
    public class AchievementDataManager : MonoBehaviour
    {
        [SerializeField] private List<AchievementData> achievementDatas = new List<AchievementData>();
        private List<AchievementProgressData> achievementDataProgress = new List<AchievementProgressData>();
        
        /// <summary>
        /// Is initialized for every stage and is ultimately referred to by class Fujin.UI.UIMileBonusSection
        /// </summary>
        private List<AchievementStageData> progressAcquiredOnStage = new List<AchievementStageData>();
        
        private static AchievementDataManager _instance;
        public static AchievementDataManager Instance => _instance;

        private void Start()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public List<AchievementStageData> GetProgressAcquiredOnStage()
        {
            return progressAcquiredOnStage;
        }

        /// <summary>
        /// Updates or adds a value stored in the achievement progress list.
        /// </summary>
        /// <param name="id">The ID of the achievement.</param>
        /// <param name="additionalValue">The value to add to the existing achievement.</param>
        public void UpdateOrAddAchievementValue(int id, int additionalValue)
        {
            int index = achievementDataProgress.FindIndex(item => item.id == id);

            if (index != -1)
            {
                // Update the existing value
                achievementDataProgress[index] = new AchievementProgressData(
                    id, 
                    achievementDataProgress[index].value + additionalValue
                );
            }
            else
            {
                // Add a new entry if not found
                Debug.LogWarning("Warning: No value was previously added to achievement data list.");
                achievementDataProgress.Add(new AchievementProgressData(id, additionalValue));
            }
        }

        private const string JsonFileName = "AchievementDataProgress.json";
        
        /// <summary>
        /// Uploads List of info to the directory;
        /// does NOT delete the list memory occupancy automatically.
        /// </summary>
        public void SaveAchievement()
        {
            try
            {
                string json = JsonUtility.ToJson(new AchievementProgressDataWrapper(achievementDataProgress));
            
                string directoryPath = Path.Combine(Application.persistentDataPath, DirectoryName.Achievements);
                string filePath = Path.Combine(directoryPath, JsonFileName);
            
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Get json info and convert it to List
        /// </summary>
        private void LoadAchievement()
        {
            string filePath = Path.Combine(Application.persistentDataPath, DirectoryName.Achievements, JsonFileName);
            
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"Error: JSON file not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                    
                AchievementProgressDataWrapper wrapper = JsonUtility.FromJson<AchievementProgressDataWrapper>(json);
                if (wrapper == null || wrapper.data == null)
                {
                    Debug.LogError($"Error: No valid data found in JSON file: {filePath}");
                    return;
                }

                achievementDataProgress = wrapper.data;
            }
            catch
            {
                Debug.LogError($"Error: Failed to load or parse JSON file: {filePath}");
            }
        }

        /// <summary>
        /// Get the current player progress through an ID search
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetAchievementProgressByID(int id)
        {
            AchievementProgressData searchResult = achievementDataProgress.Find(item => item.id == id);
            if (searchResult.id == 0 && searchResult.value == 0)
            {
                Debug.LogError("Error: Achievement not found.");
            }
            return searchResult.value;
        }

        public void UpdateAchievementProgressByID(int id, int additionalValue)
        {
            int index = achievementDataProgress.FindIndex(item => item.id == id);
            if (index == -1)
            {
                Debug.LogError("Error: Achievement not found.");
                return;
            }
            achievementDataProgress[index].value += additionalValue;
        }


        private Dictionary<int, AchievementData> achievementIDtoDataDictionary;

        private void InitializeDictionaryAchievementData()
        {
            achievementIDtoDataDictionary = new Dictionary<int, AchievementData>();

            foreach (var data in achievementDatas)
            {
                if (!achievementIDtoDataDictionary.TryAdd(data.id, data))
                {
                    Debug.LogError($"Duplicate ID found: {data.title}");
                }
            }
        }
        
        /// <summary>
        /// Get info of a specific AchievementData (such as for a result scene) from dictionary
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AchievementData GetAchievementDataByID(int id)
        {
            return achievementIDtoDataDictionary[id];
        }
    }
}