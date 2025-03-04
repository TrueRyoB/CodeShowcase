using System.IO;
using Fujin.Constants;
using System;
using UnityEngine;

namespace Fujin.Data
{
    [Serializable]
    public class PreferenceData
    {
        public bool EnableSfx { get; private set; }
        public bool EnableBGM { get; private set; }
        public float Volume { get; private set; }
        public float BGMVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public Language Language { get; private set; }
        public bool OmitCommenting { get; private set; }
        
        private const string FileName = "Preferences.json";

        public void SaveAsJson()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, DirectoryName.Settings);

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
                Debug.LogError($"Error: failed to save instance PreferenceData to filePath {Path.Combine(directoryPath, FileName)}. Exception: {e}");
                throw;
            }
        }

        public bool TryLoadFromJson()
        {
            string filePath = Path.Combine(Application.persistentDataPath, DirectoryName.Settings, FileName);

            if (File.Exists(filePath))
            {
                try
                {
                    PreferenceData savedPreference = JsonUtility.FromJson<PreferenceData>(File.ReadAllText(filePath));
                    CopyPreferenceData(savedPreference);
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

        /// <summary>
        /// Helper function that serves as a copy constructor method
        /// </summary>
        /// <param name="preferenceData"></param>
        private void CopyPreferenceData(PreferenceData preferenceData)
        {
            EnableSfx = preferenceData.EnableSfx;
            EnableBGM = preferenceData.EnableBGM;
            Volume = preferenceData.Volume;
            BGMVolume = preferenceData.BGMVolume;
            SfxVolume = preferenceData.SfxVolume;
            Language = preferenceData.Language;
            OmitCommenting = preferenceData.OmitCommenting;
        }

        public PreferenceData()
        {
            EnableSfx = true;
            EnableBGM = true;
            Volume = 1.0f;
            BGMVolume = 1.0f;
            SfxVolume = 1.0f;
            Language = Language.English;
            OmitCommenting = false;
        }

        public PreferenceData(bool enableSfx, bool enableBGM, float volume, float bgmVolume, float sfxVolume, Language language, bool omitCommenting)
        {
            EnableSfx = enableSfx;
            EnableBGM = enableBGM;
            Volume = volume;
            BGMVolume = bgmVolume;
            SfxVolume = sfxVolume;
            Language = language;
            OmitCommenting = omitCommenting;
        }
    }
}