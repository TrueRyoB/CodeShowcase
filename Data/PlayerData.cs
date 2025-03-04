using System;
using System.IO;
using Fujin.Constants;
using UnityEngine;

namespace Fujin.Data
{
    [Flags]
    public enum VideoFlags
    {
        None = 0,
        Animation1 = 1 << 0,
        Animation2 = 1 << 1,
        Animation3 = 1 << 2,
        Animation4 = 1 << 3  
    }
    
    [Serializable]
    public class PlayerData
    {
        public string UserName { get; private set; }
        public int Fund { get; private set; }

        private int playedVideos;
        
        public bool PlayedAnimation1 { get; private set; }
        public bool PlayedAnimation2 { get; private set; }
        public bool PlayedAnimation3 { get; private set; }

        private const string DefaultUserName = "Kaguo";
        private const string FileName = "PlayerData.json";
        private const int MaxCharacter = 8;
        private const int MaxFund = int.MaxValue;
        
        /// <summary>
        /// Check if the selected animation is played
        /// </summary>
        /// <param name="videoFlag"></param>
        /// <returns></returns>
        public bool HasPlayedAnimation(VideoFlags videoFlag)
        {
            return (playedVideos & (int)videoFlag) != 0;
        }

        /// <summary>
        /// Set animation bool played
        /// </summary>
        /// <param name="videoFlag"></param>
        public void SetAnimationPlayed(VideoFlags videoFlag)
        {
            playedVideos |= (int)videoFlag;
        }


        public void SaveAsJson()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, DirectoryName.Player);
            
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            
                string json = JsonUtility.ToJson(this);
                File.WriteAllText(Path.Combine(directoryPath, FileName), json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: failed to save instance PlayerData to filePath {Path.Combine(directoryPath, FileName)}. Exception: {e}");
                throw;
            }
        }
        
        public bool TryLoadFromJson()
        {
            string filePath = Path.Combine(Application.persistentDataPath, DirectoryName.Player, FileName);

            if (File.Exists(filePath))
            {
                try
                {
                    PlayerData savedPlayerData = JsonUtility.FromJson<PlayerData>(File.ReadAllText(filePath));
                    CopyPlayerData(savedPlayerData);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error: failed to parse JSON content. Exception: {e}");
                    throw;
                }
            }
            else
            {
                return false;
            }
        }
        
        private void CopyPlayerData(PlayerData playerData)
        {
            UserName = playerData.UserName;
            Fund = playerData.Fund;
        }

        /// <summary>
        /// Return true ONLY if the change doesn't interfere with a digit limit;
        /// is used for subtraction as well (assuming a user cannot spend more than their budget).
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        public bool AddFundBy(int change)
        {
            if (change > 0 && Fund > MaxFund - change)
            {
                Fund = MaxFund;
                return false;
            }
            else
            {
                Fund += change;
                return true;
            }
        }

        /// <summary>
        /// Return true and assign the parameter value ONLY if the criteria (character limit) is met.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool SetUserName(string input)
        {
            if (input.Length > MaxCharacter)
            {
                return false;
            }
            else
            {
                UserName = input;
                return true;
            }
        }

        public PlayerData() : this(DefaultUserName, 0) { }
        public PlayerData(string userName, int fund)
        {
            Debug.LogError("Error: constructor method is not supported for class PlayerData; use AddFundBy() and SetUserName() instead.");
        }
    }
}