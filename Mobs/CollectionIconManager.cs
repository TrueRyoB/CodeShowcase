using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fujin.Constants;
using Fujin.System;

namespace Fujin.Mobs
{
    /// <summary>
    /// Used in the Result scene; helper class for UIGetterStockSection;
    /// 1. Obscure
    /// 2. Initialize
    /// 3. Shine
    /// </summary>
    public class CollectionIconManager : MonoBehaviour
    {
        [Header("Prefab and Custom Material")] 
        [SerializeField] private GameObject risingBonusScoreDisplayer;
        [SerializeField] private GameObject shineParticleSystem;
        [SerializeField] private Material silhouetteMaterial;

        [Header("Child Object")] 
        [SerializeField] private Transform shiningSummonPos;
        [SerializeField] private Image img;
        [SerializeField] private AudioClip shineSe;
        [SerializeField] private AudioSource audioSource;

        private const int BonusScore = 100;
        private const float XInterval = 0.2f;
        private Material originalMaterial;
        
        public void Obscure()
        {
            if (originalMaterial == null)
            {
                originalMaterial = img.material;
            }

            img.material = silhouetteMaterial;
        }

        /// <summary>
        /// 1. Change sprite
        /// 2. Transform itself to the right position
        /// </summary>
        /// <param name="basePos"></param>
        /// <param name="step"></param>
        /// <param name="sprite"></param>
        public void Initialize(Vector3 basePos, int step, Sprite sprite)
        {
            basePos.x -= XInterval * step;
            transform.position = basePos;
            img.sprite = sprite;
        }

        /// <summary>
        /// 1. Reverse-obscure itself
        /// 2. Play particle system and SE
        /// 3. Show a bonus score sign
        /// 4. Wait for a while
        /// </summary>
        /// <param name="callback"></param>
        public void Shine(Action callback = null)
        {
            StartCoroutine(PerformShine(callback));
        }

        private readonly WaitForSeconds waitHalfSeconds = new WaitForSeconds(0.5f);
        
        private IEnumerator PerformShine(Action callback = null)
        {
            Instantiate(shineParticleSystem).GetComponent<ParticleSystem>().Play();

            yield return waitHalfSeconds;
            
            AudioManager.Instance.Play(audioSource, SoundName.ResultScene.SFX_BonusSilhouetteOut);
            
            img.material = originalMaterial;
            RisingBonusScoreManager displayer =
                Instantiate(risingBonusScoreDisplayer).GetComponent<RisingBonusScoreManager>();
            displayer.ActOnInfo(BonusScore, shiningSummonPos.position);
            
            yield return waitHalfSeconds;
            
            callback?.Invoke();
        }
    }
}