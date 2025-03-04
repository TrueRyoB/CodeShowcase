using Fujin.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Data
{
    [Serializable]
    public class SoundInfo
    {
        public string fileName;
        public string filePath;
        public ClipType clipType;
        public bool affectedByGlobalModifier;
        public float volumeScale;
        public float pitchScale;
        public int priority;    // Sound is replaced automatically if the priority is lower (incur nothing if the same)
        public AudioClip audioClip;

        public SoundInfo(string fileName, string filePath, string clipType, bool affectedByGlobalModifier, 
            float volumeScale, float pitchScale, int priority)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.clipType = (ClipType)Enum.Parse(typeof(ClipType), clipType);
            this.affectedByGlobalModifier = affectedByGlobalModifier;
            this.volumeScale = volumeScale;
            this.pitchScale = pitchScale;
            this.priority = priority;
        }
    }

    [Serializable]
    public class SoundInfoWrapper
    {
        public List<SoundInfo> soundInfos;
    }
}