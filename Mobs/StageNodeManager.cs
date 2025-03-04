using UnityEngine;
using Fujin.System;
using UnityEngine.UI;

namespace Fujin.Mobs
{
    /// <summary>
    /// 1. Holds a value for stage name
    /// 2. Locks / unlocks a stage
    /// 3. Displays acquired collections as UI
    /// </summary>
    public class StageNodeManager : MonoBehaviour
    {
        [Header("For each node")] 
        [SerializeField] public string areaName = "Area 1";
        [SerializeField] public string stageName = "practice room";
        
        [Header("Assigned at Asset folder")]
        [SerializeField] private RectTransform getterDisplayer;
        [SerializeField] private GameObject getterIconTemplate;
        private bool unlocked;

        private Collider col;

        private const float PieceLogoDiameter = 1.0f;
        private const float SpacingBetweenPieces = 0.1f;
        private const float SideMarginDisplayer = 0.2f;
        private const float VerticalMarginDisplayer = 0.05f;

        private void UpdateGetterDisplayer()
        {
            int n = GameCollectionManager.GetNumberOfPiecesIn(stageName);
            getterDisplayer.sizeDelta = new Vector2(SideMarginDisplayer*2 + (n-1)*SpacingBetweenPieces*2 + PieceLogoDiameter*n, PieceLogoDiameter + VerticalMarginDisplayer*2 + VerticalMarginDisplayer*2);
            float lastX = SideMarginDisplayer + PieceLogoDiameter / 2f;
            
            // Get a reference for collection status here
            uint c = GameCollectionManager.GetPlayerCollections(areaName).TryGetValue(stageName, out var value) ? value : 0;

            for (int i = 0; i < n; ++i)
            {
                // Set the appropriate RectTransform value
                GameObject newPiece = Instantiate(getterIconTemplate, getterDisplayer.transform);
                
                // Let the icon initialize the UI itself
                newPiece.GetComponent<GetterIconManager>().Initialize(n, PieceLogoDiameter, lastX, c);
                
                // Update offsets for subsequent calls
                lastX += SpacingBetweenPieces + PieceLogoDiameter;
            }
        }

        private void UpdateLockStatus()
        {
            col.isTrigger = unlocked;
        }

        public bool Unlocked
        {
            get => unlocked;
            set
            {
                unlocked = value;
                col.isTrigger = value;
            }
        }

        private void Start()
        {
            col ??= GetComponent<Collider>();
            UpdateLockStatus();
            UpdateGetterDisplayer();
        }
    }
}