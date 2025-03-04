using UnityEngine;

namespace Fujin.Data
{
    [CreateAssetMenu(fileName = "(chara) (emo)", menuName = "KaguoSpeedRun/IconType Template", order = 3)]
    public class IconType : ScriptableObject 
    {
        public enum Emo {
            Calm,
            Angry,
            Sad,
            Mocking,
        }
        public enum Icon {
            Tonsuke,
            Shadie,
            Kaguo,
            Balloonguy,
        }

        public Icon IconLog;
        public Emo EmoLog;
    }
}