using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fujin.ScriptableObjects;
using Fujin.System;
using Fujin.Constants;

namespace Fujin.Mobs
{
    /// <summary>
    /// Used in the Result scene to update itself and the surrounding bead round on function calling
    /// </summary>
    public class MedalManager : MonoBehaviour
    {
        [Header("Reference")] 
        [SerializeField] private Gradient beadsGradient;
        [SerializeField] private Sprite woodMedal;
        [SerializeField] private Sprite bronzeMedal;
        [SerializeField] private Sprite silverMedal;
        [SerializeField] private Sprite goldMedal;
        [SerializeField] private Sprite dokiMedal;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject beadPrefab;
        [SerializeField] private GameObject thresholdPrefab;
        [SerializeField] private AudioClip beadSe;
        [SerializeField] private AudioClip promotionSe;
        [SerializeField] private AudioClip settleSe;
        
        [Header("Child Objects")]
        [SerializeField] private Image img;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform center;
        [SerializeField] private float beadsRadius;

        private const int BeadCount = 60;
        private Queue<GameObject> beadsQueue;

        /// <summary>
        /// 1. Creates an object pool
        /// 2. Set two thresholds (bronze, silver) on the radius
        /// </summary>
        private void Start()
        {
            if (beadPrefab.GetComponent<Image>() == null)
            {
                Debug.LogError("Error: no image component attached to beadPrefab.");
            }
            PlaceBeadsInCircleAndHide();
            SetThresholds();
        }
        
        private Vector3 position = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private float angle;
        
        private void PlaceBeadsInCircleAndHide()
        {
            beadsQueue = new Queue<GameObject>();
            
            for (int i = 0; i < BeadCount; i++)
            {
                angle = Mathf.PI * (2f * i / BeadCount + 0.5f);
                
                UpdatePosVecOnCircle(beadsRadius, angle, ref position, ref rotation);
                
                GameObject newBead = Instantiate(beadPrefab, transform);
                
                RectTransform rectTransform = newBead.GetComponent<RectTransform>();
                
                rectTransform.anchoredPosition = position;
                rectTransform.rotation = rotation;
                
                newBead.GetComponent<Image>().color = beadsGradient.Evaluate(i / (float)BeadCount);
                
                newBead.SetActive(false);
                beadsQueue.Enqueue(newBead);
            }
        }

        /// <summary>
        /// Helper method (ct = center, r = radius, rad = angle in radians, qua = quaternion)
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="r"></param>
        /// <param name="rad"></param>
        /// <param name="result"></param>
        /// <param name="qua"></param>
        private void UpdatePosVecOnCircle(float r, float rad, ref Vector3 result, ref Quaternion qua)
        {
            result.x = r* Mathf.Cos(rad);
            result.y = r* Mathf.Sin(rad);
            result.z = 0f;
            
            //qua = Quaternion.AngleAxis(rad * Mathf.Rad2Deg, Vector3.right);
            qua = Quaternion.Euler(0f, 0f, rad * Mathf.Rad2Deg - 90f);
        }

        private int bronzeThreshold;
        private int silverThreshold;
        private int goldThreshold;
        private int dokiThreshold;

        private bool eventRegistered;
        
        private void SetThresholds()
        {
            // Get access to the stage score thresholds
            BasicStageDataPlatformer stageInfo;
            if (GameFlowManager.StageInfoLoadCompleted)
            {
                stageInfo = GameFlowManager.StageInfoInstance;
            }
            else
            {
                if (!eventRegistered)
                {
                    GameFlowManager.OnStageInfoLoaded += SetThresholds;
                    eventRegistered = true;
                }
                return;
            }

            bronzeThreshold = stageInfo.bronzeThreshold;
            silverThreshold = stageInfo.silverThreshold;
            goldThreshold = stageInfo.goldThreshold;
            dokiThreshold = stageInfo.dokiThreshold;
            
            // Create three thresholds (for bronze, silver, gold)
            angle = Mathf.PI * (2f * bronzeThreshold / goldThreshold + 0.5f);
            UpdatePosVecOnCircle(beadsRadius, angle, ref position, ref rotation);
            RectTransform targetThreshold = Instantiate(thresholdPrefab, transform).GetComponent<RectTransform>();
            targetThreshold.anchoredPosition = position;
            targetThreshold.rotation = rotation;

            angle = Mathf.PI * (2f * silverThreshold / goldThreshold + 0.5f);
            UpdatePosVecOnCircle(beadsRadius, angle, ref position, ref rotation);
            targetThreshold = Instantiate(thresholdPrefab, transform).GetComponent<RectTransform>();
            targetThreshold.anchoredPosition = position;
            targetThreshold.rotation = rotation;
            
            angle = Mathf.PI * 0.5f;
            UpdatePosVecOnCircle(beadsRadius, angle, ref position, ref rotation);
            targetThreshold = Instantiate(thresholdPrefab, transform).GetComponent<RectTransform>();
            targetThreshold.anchoredPosition = position;
            targetThreshold.rotation = rotation;
        }

        private int currentScore;

        private void UpdateMedalImage(int index)
        {
            AudioManager.Instance.Play(audioSource, SoundName.ResultScene.SFX_MedalPromoted); //promotionSe

            switch (index)
            {
                case 0:
                    img.sprite = bronzeMedal;
                    break;
                case 1:
                    img.sprite = silverMedal;
                    break;
                case 2: 
                    img.sprite = goldMedal;
                    break;
                case 3:
                    img.sprite = dokiMedal;
                    break;
            }
        }

        public void PlaySettleAnimation()
        {
            // Pause bead SE
            StopBeadSfx();
            
            // Play settle SE
            AudioManager.Instance.Play(audioSource, SoundName.ResultScene.SFX_LastBeadDrop); //settleSe
            
            //TODO: add happy animation
        }


        /// <summary>
        /// Helper method
        /// </summary>
        private void StopBeadSfx()
        {
            audioSource.Stop();
            isPlayingBead = false;
        }

        private bool isPlayingBead;
        
        /// <summary>
        /// 1. Add one bead
        /// 2. Play SE
        /// </summary>
        public void ReflectScore(int newScore)
        {
            if (!isPlayingBead)
            {
                AudioManager.Instance.Play(audioSource, SoundName.ResultScene.SFX_BeadDrop);
                isPlayingBead = true;
            }
            
            if (beadsQueue.Count == 0)
            {
                Debug.LogError("Error: Mobs.MedalManager.AddBeadsOnScene() is called more than the fixed limit of 60.");
                return;
            }
            
            beadsQueue.Dequeue().SetActive(true);
            
            int[] thresholds = { bronzeThreshold, silverThreshold, goldThreshold, dokiThreshold };

            int index = 0;

            foreach (int threshold in thresholds)
            {
                if (currentScore < threshold && newScore >= threshold)
                {
                    UpdateMedalImage(index);
                    return;
                }
                ++index;
            }
        }
    }
}