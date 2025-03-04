using UnityEngine;
using UnityEngine.UI;

namespace Fujin.Mobs
{
    /// <summary>
    /// Helper class for StageNodeManager in initializing the UI
    /// ...by reduced access to GetComponent through a direct reference on Inspector
    /// </summary>
    public class GetterIconManager : MonoBehaviour
    {
        [SerializeField]private RectTransform rectTransform;
        [SerializeField] private Image img;
        [SerializeField] private Sprite emptyIcon;
        [SerializeField] private Sprite acquiredIcon;
        public void Initialize(int n, float diameter, float lastX, uint c)
        {
            // Set the appropriate positional value
            rectTransform.anchoredPosition = new Vector2(lastX, 0);
            rectTransform.sizeDelta = new Vector2(diameter, diameter);
                
            // Get bit info (assuming 1 indicates true and 0 false)
            bool isCollected = (c & 1u << n) != 0;
            
            // Assign its image based on completion level TODO: change this later on
            img.sprite = isCollected ? acquiredIcon : emptyIcon;
        }
    }
}