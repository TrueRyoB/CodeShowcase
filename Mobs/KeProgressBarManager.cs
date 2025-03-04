using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fujin.Constants;

namespace Fujin.Mobs
{
    public class KeProgressBarManager : MonoBehaviour
    {
        [SerializeField] private Image centerUI;
        [SerializeField] private Image frameUI;
        [SerializeField] private Image progressUI;

        [SerializeField] private Color whenNotSufficient;
        [SerializeField] private Color whenRocketAffordable;
        [SerializeField] private Color whenGateAffordable;
        
        private float maxWidth;
        private Vector2 tempSize;
        private int holdingValue;

        private readonly WaitForSeconds waitTillHide = new WaitForSeconds(1f);
        private Coroutine hideCoroutine;
        
        private void Start()
        {
            InitializeData();
            ApplyProgressFloat(0f);
            SetAllActive(false);
        }

        private IEnumerator HideTemp()
        {
            yield return waitTillHide;
            SetAllActive(false);
            hideCoroutine = null;
        }

        private void SetAllActive(bool value)
        {
            centerUI.gameObject.SetActive(value);
            frameUI.gameObject.SetActive(value);
            progressUI.gameObject.SetActive(value);
        }
        
        private void InitializeData()
        {
            maxWidth = frameUI.rectTransform.rect.width - 5f;
            tempSize.y = progressUI.rectTransform.rect.height;
        }

        /// <summary>
        /// Context: kagu energy is entirely regulated at file PlayerPhysics
        /// Here it only accepts an updated value and animate essentials in need
        /// </summary>
        /// <param name="updatedValue"></param>
        public void ApplyProgress(int updatedValue)
        {
            SetAllActive(true);
            if (updatedValue > KaguEnergyMap.Keep.MaxStorageSize || updatedValue < 0)
            {
                Debug.LogError("Error: progress must be between the set range and not " + updatedValue);
                return;
            }
            holdingValue = updatedValue;
            ApplyProgressFloat((float)updatedValue / KaguEnergyMap.Keep.MaxStorageSize);

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            hideCoroutine = StartCoroutine(HideTemp());
        }

        private void ApplyProgressFloat(float progress)
        {
            //TODO: ideally insert a cool animation here
            //
            
            // Apply a change in size of filler
            tempSize.x = maxWidth * progress;
            progressUI.rectTransform.sizeDelta = tempSize;
            
            // Change color
            if (holdingValue >= KaguEnergyMap.Use.Akeboshi || holdingValue >= KaguEnergyMap.Use.Gate)
            {
                progressUI.color = whenGateAffordable;
            }
            else if (holdingValue >= KaguEnergyMap.Use.Rocket)
            {
                progressUI.color = whenRocketAffordable;
            }
            else
            {
                progressUI.color = whenNotSufficient;
            }
        }
    }
}