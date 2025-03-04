using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Fujin.Constants
{
    public enum TagGroup
    {
        InteractableSurface,
        JumpResetSurface,
        MovableSurface,
    }
    public static class GameObjectTag
    {
        public const string Player = "Player";
        public const string Sign = "Sign";
        public const string Shadow = "Shadow";
        public const string StrongSign = "StrongSign";
        public const string Enemy = "Enemy";
        public const string Terrain = "Terrain";
        public const string Lift = "Lift";
        public const string Bouncy = "Bouncy";
        public const string Slippery = "Slippery";
        public const string Spring = "Spring";
        public const string DialoguePlayerGameObject = "DialoguePlayerGameObject";
        public const string Bypassable = "Bypassable";
        public const string StageNode = "Stage Node";    
        public const string Pavement = "Pavement";
        public const string Avalanche = "Avalanche";
        public const string GetterCheckpoint = "GetterCheckpoint";
        public const string ActionableUI = "ActionableUI";

        /// <summary>
        /// Merry Kurusimimasu... (average game dev)
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public static bool IsObjectInGroup(GameObject gameObject, TagGroup tagGroup)
        {
            var tagMapping = new Dictionary<TagGroup, string[]>
            {
                { TagGroup.InteractableSurface, new[] { Terrain, Lift, Bouncy, Slippery, Bypassable } },
                { TagGroup.JumpResetSurface, new[] { Terrain, Lift, Bouncy, Bypassable } },
                { TagGroup.MovableSurface, new[] {Lift, Avalanche} },
            };

            // Return false if the enum is not registered
            if (!tagMapping.ContainsKey(tagGroup))
            {
                Debug.LogWarning("Warning: Enumerator passed to function IsTagInGroup has not been registered to the dictionary yet!");
                return false;
            }

            return tagMapping[tagGroup].Any(tag => gameObject.CompareTag(tag));
        }

    }
}