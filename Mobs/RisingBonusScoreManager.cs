using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using TMPro;

namespace Fujin.Mobs
{
    /// <summary>
    /// Used in the result scene
    /// the similar one can be used for platformer as well
    /// </summary>
    public class RisingBonusScoreManager : MonoBehaviour
    {
        private const float Rise = 5f;
        private const float Drop = 3f;
        private const float NoteTwo = 0.2f;

        private Vector3 pos;
        
        [Header("Child objects")]
        [SerializeField] private TextMeshProUGUI textHolder;

        [Header("References")]
        [SerializeField] private Color insignificant;
        [SerializeField] private Color normal;
        [SerializeField] private Color significant;
        
        /// <summary>
        /// Change text and color based on the degree, rise quickly, remain for a while, and disappear
        /// </summary>
        /// <param name="score"></param>
        /// <param name="initialPosition"></param>
        /// <param name="callback"></param>
        public void ActOnInfo(int score, Vector3 initialPosition, Action callback = null)
        {
            gameObject.SetActive(true);
            SetTextColorOnScore(score);
            pos = initialPosition;

            StartCoroutine(PerformAscention(() =>
            {
                if (callback != null)
                {
                    gameObject.SetActive(false);
                    callback.Invoke();
                }
                else
                {
                    Destroy(gameObject);
                }
            }));
        }

        /// <summary>
        /// めっちゃ適当...
        /// </summary>
        /// <param name="score"></param>
        private void SetTextColorOnScore(int score)
        {
            textHolder.text = $"+ {score}";
            
            if (score < 100) textHolder.color = insignificant;
            else if (score > 200) textHolder.color = significant;
            else textHolder.color = normal;
        }

        /// <summary>
        /// 0.6 seconds of vibration (0.2f up, 0.2 bit down, 0.2 stay)
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator PerformAscention(Action callback = null)
        {
            float elapsedTime = 0f;
            float baseY = pos.y;

            while (elapsedTime < NoteTwo)
            {
                pos.y = Mathf.Lerp(baseY, baseY + Rise, elapsedTime / NoteTwo);
                transform.position = pos;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0f;

            while (elapsedTime < NoteTwo)
            {
                pos.y = Mathf.Lerp(baseY + Rise, baseY + Rise - Drop, elapsedTime / NoteTwo);
                transform.position = pos;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(NoteTwo);
            
            callback?.Invoke();
        }
    }
}