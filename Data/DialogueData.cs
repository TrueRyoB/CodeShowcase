using System;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Fujin.Data
{
    [Serializable]
    public class DialogueData
    {
        public string id;
        public string text;
        public string imageAddress;
        public string bgmKey;
        public string nextId;
        public string nextActionKey;
        public string flagKey;
        public string gradationKey;
        public List<string> altIds;

        [NonSerialized]private AudioClip bgm;
        [NonSerialized]private Sprite image;
        [NonSerialized]private byte flag;
        [NonSerialized]private Action nextAction;
        [NonSerialized]private bool loadingVis;

        [NonSerialized] public bool HasBGM;

        public AudioClip BGM => HasBGM ? bgm : null;
        public Sprite Image => image;
        public byte Flag => flag;
        public Action NextAction => nextAction;
        
        //TODO: backgroundの色替えをするためのコード忘れてた...
        
        //TODO: create two registries for parsing flagKey and nextAction at class GameManager
        //

        /// <summary>
        /// Register!!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <param name="imageAddress"></param>
        /// <param name="bgmKey"></param>
        /// <param name="nextId"></param>
        /// <param name="nextActionKey"></param>
        /// <param name="flagKey"></param>
        /// <param name="altIds"></param>
        public void Register(string id, string imageAddress, string nextId, string text, string flagKey, string nextActionKey, string bgmKey, string gradationKey, List<string> altIds)
        {
            this.id = id;
            this.text = text;
            this.imageAddress = imageAddress;
            this.bgmKey = bgmKey;
            this.nextId = nextId;
            this.altIds = altIds;
            this.nextActionKey = nextActionKey;
            this.flagKey = flagKey;
            this.gradationKey = gradationKey;
        }

        public void Reset()
        {
            id = null;
            text = null;
            imageAddress = null;
            bgmKey = null;
            bgm = null;
            flagKey = null;
            image = null;
            flag = 0;
            nextActionKey = null;
            nextAction = null;
            altIds.Clear();
            nextId = null;
            loadingVis = false;
            gradationKey = null;
        }
        
        public async Task InitializeWith(string[] row)
        {
            if (row == null)
            {
                Debug.LogError("Error: row is null!");
                return;
            }

            List<string> altIds = new List<string>();
            
            int i = 8;
            while (i < row.Length)
            {
                if (string.IsNullOrEmpty(row[i])) break;
                
                altIds.Add(row[i]);
            }
            
            Register(row[0], row[1],row[2], row[3],row[4],
                row[5], row[6], row[7], altIds);

            await LoadVisualInfoAsync();
        }

        public async ValueTask LoadVisualInfoAsync()
        {
            if (bgm != null && image != null) return;

            if (!loadingVis)
            {
                loadingVis = true;

                // Load BGM only when necessary
                //TODO:本当であれば、soundHashに登録されていなかったらロードさせる方法を取るべきだけど
                // 膨大なJSONファイルのList<List<SoundInfo>>と複雑化している中でSoundInfo.keyからSoundInfoを探すのが大変すぎるから
                // とりあえず放置。別の解決策が見出せるまで (keyからGroupNameのDictionaryをロード時に全て書き出すっていう手段もあるけどやりすぎな気がする)
                if (!string.IsNullOrEmpty(bgmKey)) //TODO: ここですでに大間違い... ;-;
                {
                    HasBGM = true;
                    var handle = Addressables.LoadAssetAsync<AudioClip>(bgmKey);
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        bgm = handle.Result;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Loading BGM failed: {0}", bgmKey);
                    }
                }
                
                // Load Image every time
                if (string.IsNullOrEmpty(imageAddress))
                {
                    Debug.LogError($"Error: image address is not set to the csv file for a dialogue ID \"{id}\"!");
                }
                else
                {
                    var handle = Addressables.LoadAssetAsync<Sprite>(imageAddress);
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        image = handle.Result;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Loading image failed: {0}", imageAddress);
                    }
                }

                loadingVis = false;
            }
        }
    }
}