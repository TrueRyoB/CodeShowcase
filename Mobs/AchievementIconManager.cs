using System;
using Fujin.Data;
using Fujin.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

namespace Fujin.Mobs
{
    /// <summary>
    /// Result scene something something something attached to a PREFAB created by UIMileBonusSection
    /// </summary>
    public class AchievementIconManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Child Objects")]
        [SerializeField] private Image img;
        [SerializeField] private GameObject newLogo;
        [SerializeField] private GameObject eruptedLogo;
        [SerializeField] private CanvasGroup cgDescriptiveBox;
        [SerializeField] private TextMeshProUGUI titleHolder;
        [SerializeField] private TextMeshProUGUI detailHolder;

        public void OnPointerExit(PointerEventData eventData)
        {
            SetDescriptiveBox(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetDescriptiveBox(true);
        }

        private Coroutine fadeCoroutine;
        
        /// <summary>
        /// Fade in or out flexibly
        /// </summary>
        /// <param name="fadeIn"></param>
        private void SetDescriptiveBox(bool fadeIn)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (fadeIn)
            {
                fadeCoroutine = StartCoroutine(FadeBoxIn());
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeBoxOut());
            }
        }

        private IEnumerator FadeBoxIn()
        {
            float duration = (1f - cgDescriptiveBox.alpha) * 0.5f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                cgDescriptiveBox.alpha = elapsedTime / duration;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            fadeCoroutine = null;
        }

        private IEnumerator FadeBoxOut()
        {
            float duration = cgDescriptiveBox.alpha * 0.5f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                cgDescriptiveBox.alpha = 1f - elapsedTime / duration;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            fadeCoroutine = null;
        }

        private void HideLogos()
        {
            newLogo.SetActive(false);
            eruptedLogo.SetActive(false);
        }
        
        public void ActUponInfo(int id, int score, Transform movingContainer, Action callback = null)
        {
            StartCoroutine(PerformFlip(id, score, movingContainer, callback));
        }

        private const float FlipDuration = 0.3f;

        private IEnumerator PerformFlip(int id, int score, Transform movingContainer, Action callback = null)
        {
            HideLogos();
            AchievementData sample = AchievementDataManager.Instance.GetAchievementDataByID(id);
            img.sprite = sample.icon;
            titleHolder.text = sample.title;
            detailHolder.text = sample.description;
            
            // Flip mechanics
            float elapsedTime = 0f;
            Vector3 scaleVec = transform.localScale;
            while (elapsedTime < FlipDuration)
            {
                scaleVec.x = Mathf.Sign(Mathf.Cos((Mathf.PingPong(elapsedTime, FlipDuration) / FlipDuration) * Mathf.PI));
                transform.localScale = scaleVec;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Make a score callback
            callback?.Invoke();
            
            // Register itself as a child to the moving gameObject
            transform.SetParent(movingContainer, false);
            
            // Show "NEW" or "ERUPTED" if applicable
            int currentProgress = AchievementDataManager.Instance.GetAchievementProgressByID(id);
            if (currentProgress == 0)
            {
                newLogo.SetActive(true);
            }
            else if (sample.MeetsErupted(currentProgress, score))
            {
                eruptedLogo.SetActive(true);
            }
        }
    }
}