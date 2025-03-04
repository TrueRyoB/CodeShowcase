using UnityEngine;

namespace Fujin.Mobs
{
    /// <summary>
    /// Used in the world map
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class GetterManager : MonoBehaviour
    {
        [SerializeField][Tooltip("This value must be more than 0 and does not duplicate to other index of GetterManager.")] private int pieceIndex = -1;
        
        public int Index => pieceIndex;

        private void Start()
        {
            if (pieceIndex == -1)
            {
                Debug.LogError("Error: one of GetterManager.pieceIndex is not initialized appropriately.");
            }
        }
    }
}