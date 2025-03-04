using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fujin.Mobs
{
    /// <summary>
    /// Used in the result scene;
    /// is attached to the dynamic UI gameObject;
    /// is activated on instantiation.
    /// </summary>
    public class FrameWormManager : MonoBehaviour
    {
        [Header("Reference")] 
        [SerializeField] private float hueSpeed = 1.0f;

        [Header("Child objects")] 
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private Image img;
        
        private float saturation = 1.0f;
        private float value = 1.0f;

        private void Update()
        {
            float hue = Mathf.Repeat(Time.time * hueSpeed, 1.0f);
            Color color = Color.HSVToRGB(hue, saturation, value);
            
            img.color = color;
        }
        
        private void Awake()
        {
            Color.RGBToHSV(img.color, out float hue, out saturation, out value);
            ShowUpAndWander();
        }

        private Transform trackingTransform;

        /// <summary>
        /// Instead of a dynamic move, now it only shows up over time for a technical reason
        /// </summary>
        private void ShowUpAndWander()
        {
            StartCoroutine(PerformFadeInOverASecond());
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <returns></returns>
        private IEnumerator PerformFadeInOverASecond()
        {
            cg.alpha = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                cg.alpha = elapsedTime;
                yield return null;
            }
            cg.alpha = 1f;
        }
    }
}