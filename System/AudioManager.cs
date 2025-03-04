using Fujin.Data;
using UnityEngine;
using System.Collections.Generic;
using Fujin.Constants;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fujin.System
{
    
    /// <summary>
    /// Dear me in the future,
    ///
    /// I hope this message finds you well. I am Ryoji Araki, a crazy game dev.
    /// Please make sure to take a look at files "Constants.SoundName" and {insert json file name} for any modifications/updates
    /// and "Constants.ClipType" for suffix reference.
    ///
    /// Thanks!
    ///
    /// Sincerely,
    /// Ryoji Araki
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        private readonly Dictionary<string, SoundInfo> soundHash = new Dictionary<string, SoundInfo>();
        private readonly Dictionary<AudioSource, SoundInfo> asHash = new Dictionary<AudioSource, SoundInfo>();
        
        /// <summary>
        /// Return true if the same sound is currently played
        /// </summary>
        /// <param name="clipKey"></param>
        /// <param name="targetSource"></param>
        /// <returns></returns>
        public bool IsSameSoundPlayed(string clipKey, AudioSource targetSource)
        {
            return !IsNoLongerUsed(targetSource) && asHash.TryGetValue(targetSource, out SoundInfo asInfo) && asInfo.fileName == clipKey;
        }

        /// <summary>
        /// Return true if the same sound is currently played
        /// </summary>
        /// <param name="soundInfo"></param>
        /// <param name="targetSource"></param>
        /// <returns></returns>
        public bool IsSameSoundPlayed(SoundInfo soundInfo, AudioSource targetSource)
        {
            return !IsNoLongerUsed(targetSource) && asHash.TryGetValue(targetSource, out SoundInfo asInfo) && asInfo.fileName == soundInfo.fileName;
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="isBGM"></param>
        /// <param name="volumeScale"></param>
        /// <returns></returns>
        private float GetAdjustedVolume(bool isBGM, float volumeScale)
        {
            var prefs = PreferenceDataManager.Instance;
            return prefs.Volume * (isBGM ? prefs.BGMVolume : prefs.SfxVolume) * volumeScale;
        }

        private void Cease(AudioSource audioSource)
        {
            if(!asHash.TryGetValue(audioSource, out SoundInfo si)) return;

            switch (si.clipType)
            {
                case ClipType.Music:
                case ClipType.BGM:
                    StartCoroutine(FadeOut(audioSource));
                    break;
                case ClipType.Oneshot:
                case ClipType.DynamicSFX:
                case ClipType.StaticSFX:
                    audioSource.Stop();
                    audioSource.time = 0;
                    audioSource.clip = null;
                    asHash.Remove(audioSource);
                    break;
            }
        }

        /// <summary>
        /// Helper coroutine for Stop(AudioSource)
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="fadeTime"></param>
        /// <param name="isBGM"></param>
        /// <returns></returns>
        private IEnumerator FadeOut(AudioSource audioSource, float fadeTime = 0.3f, bool isBGM = true)
        {
            float elapsedTime = 0;

            while (elapsedTime < fadeTime)
            {
                audioSource.volume = Mathf.Lerp(1, 0, elapsedTime / fadeTime) * GetAdjustedVolume(isBGM, audioSource.volume);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        public void PauseEveryBGMOver(float seconds)
        {
            foreach (KeyValuePair<AudioSource, SoundInfo> kvp in asHash)
            {
                if (kvp.Value.clipType == ClipType.BGM)
                {
                    StartCoroutine(FadeOut(kvp.Key, seconds));
                }
            }
        }

        public void Play(AudioSource audioSource, string key, bool interruptSameGroup = false)
        {
            SoundInfo sampleSoundInfo = soundHash.GetValueOrDefault(key);

            if (sampleSoundInfo == null)
            {
                Debug.LogError($"Error: soundInfo extraction failed at the key {key}");
                return;
            }

            if (interruptSameGroup)
            {
                ClipType targetType = sampleSoundInfo.clipType;
                foreach (KeyValuePair<AudioSource, SoundInfo> kvp in asHash)
                {
                    if (kvp.Value.clipType == targetType)
                    {
                        Cease(kvp.Key);
                    }
                }
            }
            
            // Immediately interrupt the previous sound if it's a duplicate or has the lower priority
            if (asHash.TryGetValue(audioSource, out SoundInfo preSoundInfo))
            {
                if (preSoundInfo.fileName == sampleSoundInfo.fileName ||
                    preSoundInfo.priority < sampleSoundInfo.priority)
                {
                    Cease(audioSource);
                }
                else if (preSoundInfo.priority > sampleSoundInfo.priority)
                {
                    // Do not play the SE otherwise
                    return;
                }
            }
            
            // Register the data to the dictionary
            asHash[audioSource] = sampleSoundInfo;
            
            // Make the audio source invulnerable to the scene load as needed
            switch (sampleSoundInfo.clipType)
            {
                case ClipType.BGM:
                case ClipType.StaticSFX:
                    DontDestroyOnLoad(audioSource); //TODO: 次に曲が流れないときに限りStopの時に破壊するようにする
                    break;
            }

            // Pick the most appropriate playing mechanism
            switch (sampleSoundInfo.clipType)
            {
                case ClipType.Oneshot:
                    PlayOneShot(audioSource, sampleSoundInfo.audioClip, sampleSoundInfo.volumeScale);
                    break;
                case ClipType.StaticSFX: 
                case ClipType.DynamicSFX:
                    PlayFromSource(audioSource, sampleSoundInfo.audioClip, sampleSoundInfo.volumeScale,
                        sampleSoundInfo.pitchScale, false, false);
                    break;
                case ClipType.BGM:
                    PlayFromSource(audioSource, sampleSoundInfo.audioClip, sampleSoundInfo.volumeScale,
                        sampleSoundInfo.pitchScale, true, true);
                    break;
                case ClipType.Music:
                    PlayFromSource(audioSource, sampleSoundInfo.audioClip, sampleSoundInfo.volumeScale,
                        sampleSoundInfo.pitchScale, true, false);
                    break;
            }
        }

        /// <summary>
        /// Reflect a change in the volume settings
        /// </summary>
        public void ReflectVolumeChange()
        {
            foreach (KeyValuePair<AudioSource, SoundInfo> kvp in asHash)
            {
                switch (kvp.Value.clipType)
                {
                    case ClipType.Oneshot:
                    case ClipType.StaticSFX:
                    case ClipType.DynamicSFX:
                        kvp.Key.volume = GetAdjustedVolume(false, kvp.Value.volumeScale);
                        kvp.Key.mute = PreferenceDataManager.Instance.EnableSfx;
                        break;
                    case ClipType.BGM:
                        kvp.Key.mute = PreferenceDataManager.Instance.EnableBGM;
                        goto case ClipType.Music; // Music should always be played
                    case ClipType.Music:
                        kvp.Key.volume = GetAdjustedVolume(true, kvp.Value.volumeScale);
                        break;
                }
            }
        }
        

        /// <summary>
        /// Helper method for LoadSoundInfoJson
        /// </summary>
        /// <param name="soundInfo"></param>
        /// <returns></returns>
        private IEnumerator LoadAudioClip(SoundInfo soundInfo)
        {
            AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(soundInfo.filePath);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                soundInfo.audioClip = handle.Result;
                Debug.Log("AudioClip loaded successfully");
            }
            else
            {
                Debug.LogError($"Failed to load AudioClip from Addressables path {soundInfo.filePath} of {soundInfo.fileName}");
            }
        }

        public void ReleaseSoundInfo()
        {
            asHash.Clear();
            soundHash.Clear();
        }
        
        /// <summary>
        /// Should be called every time the scene is loaded and pick the SE necessary for scene loads
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadSoundInfoJson(string targetSoundGroup) //TODO: make it load only a list with the parameter key
        {
            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(AddressablesPath.SoundInfoJson);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string jsonText = handle.Result.text;
                Debug.Log($"Loaded JSON: {jsonText}");
                
                List<SoundInfo> soundInfos = JsonUtility.FromJson<SoundInfoWrapper>("{\"soundInfos\": " + jsonText + "}").soundInfos;
                
                foreach (var soundInfo in soundInfos)
                {
                    StartCoroutine(LoadAudioClip(soundInfo));
                    soundHash[soundInfo.fileName] = soundInfo;
                }
            }
            else
            {
                Debug.LogError($"Failed to load json file {AddressablesPath.SoundInfoJson}.{targetSoundGroup}");
            }

            Addressables.Release(handle);
        }

        private bool isPaused;

        private void Update()
        {
            if (Time.timeScale == 0)
            {
                if (!isPaused)
                {
                    isPaused = true;
                    TriggerPauseSound();
                }
            }
            else
            {
                if (isPaused)
                {
                    isPaused = false;
                    TriggerUnPauseSound();
                }
            }
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsNoLongerUsed(AudioSource target)
        {
            return target == null || (target.timeSamples == target.clip.samples - 1 && !target.loop);
        }

        private void TriggerPauseSound()
        {
            foreach (KeyValuePair<AudioSource, SoundInfo> kvp in asHash)
            {
                // Remove audio source that is no longer used (either destroyed or played at full and is not expected to loop)
                if (IsNoLongerUsed(kvp.Key))
                {
                    asHash.Remove(kvp.Key);
                    continue;
                }

                // Pause every target audio source
                switch (kvp.Value.clipType)
                {
                    case ClipType.Music:
                    case ClipType.DynamicSFX:
                        kvp.Key.Pause();
                        break;
                }
            }
        }

        private void TriggerUnPauseSound()
        {
            foreach (KeyValuePair<AudioSource, SoundInfo> kvp in asHash)
            {
                switch (kvp.Value.clipType)
                {
                    case ClipType.Music:
                    case ClipType.DynamicSFX:
                        kvp.Key.UnPause();
                        break;
                }
            }
        }
        
        private void PlayOneShot(AudioSource audioSource, AudioClip clip, float volumeScale)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
        
        private void PlayFromSource(AudioSource audioSource, AudioClip clip, float volumeScale, float pitchScale, bool isBGM, bool loop)
        {
            if (clip == null || audioSource == null)
            {
                Debug.LogError("Error: either clip or audio source is null");
                return;
            }
            
            if (!isBGM && !PreferenceDataManager.Instance.EnableSfx) return;
            
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.volume = GetAdjustedVolume(isBGM, volumeScale);
            audioSource.pitch = pitchScale;
            
            audioSource.Play();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}