using System;
using System.Collections.Generic;
using Fujin.Constants;
using System.IO;

using UnityEngine;

namespace Fujin.Data
{
    [Serializable]
    public class ReplayData
    {
        public string StageName { get; set;  }
        public DateTime? Date { get; set; }
        public float? GoalTime { get; set; }
        public int? Score { get; set; }
        public int? SeedValue { get; set; }
        public string Comment { get; set; }
        public List<RecordedInput> InputDetails { get; set; }
        public uint? PieceCollected { get; set; }
        private bool isFlagged;

        private bool IsComplete =>
            Date.HasValue &&
            GoalTime.HasValue &&
            Score.HasValue &&
            SeedValue.HasValue &&
            !string.IsNullOrEmpty(Comment) &&
            !string.IsNullOrEmpty(StageName) &&
            InputDetails != null &&
            PieceCollected.HasValue;
        
        /// <summary>
        /// Helper function that is called everytime a new data is added
        /// ... to convert the entire structure to the json file and save it.
        /// </summary>
        private void TrySaveIfComplete()
        {
            if (IsComplete)
            {
                SaveAsJson();
            }
        }

        public void SaveAsJson() //TODO: データ保存しますか？代わりに消します　的なプレイヤーの同意を得てから呼ばれるべき
        {
            if (isFlagged)
            {
                Debug.Log("This replay data is disqualified therefore will not be saved.");
                InputDetails.Clear();
                ReplayDataManager.ResetInstance();
                return;
            }
            
            string json = JsonUtility.ToJson(this);
            
            // Get a score of a fixed length (10 digits)
            string hashedScore = Number.GetIntAtFixedLength(Score ?? 0, 10);
            
            // Get a hashed comment (20 characters max)
            string hashedComment = (Comment.Length > 20 ? Comment.Substring(0, 20) : Comment);
            
            string fileName = $"Replay_{hashedScore}_{(hashedComment)}_{DateTime.Now.Ticks}.json"; //TODO: parse用のコードを作る
            string directoryPath = Path.Combine(Application.persistentDataPath, DirectoryName.Replays, StageName);
            string filePath = Path.Combine(directoryPath, fileName);
            
            // Create a directory if it doesn't exist beforehand
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            File.WriteAllText(filePath, json);
            Debug.Log($"ReplayData saved at: {filePath}");
            InputDetails.Clear();
            
            //TODO: スコアの低いデータを消去する
            
            // string[] files = Directory.GetFiles(directory, $"Replay_{stageId}_*.json");
            // if (files.Length > maxReplaysPerStage)
            // {
            //     Array.Sort(files, (a, b) => File.GetLastWriteTime(a).CompareTo(File.GetLastWriteTime(b)));
            //     File.Delete(files[0]);
            // }
            
            ReplayDataManager.ResetInstance();
        }

        /// <summary>
        /// If flagged, data will not be saved to the directory.
        /// </summary>
        public void FlagReplayData()
        {
            isFlagged = true;
        }

        public void SetPieceCollected(uint pieceCollected)
        {
            PieceCollected = pieceCollected;
            TrySaveIfComplete();
        }

        public void SetStageName(string stageName)
        {
            StageName = stageName;
            TrySaveIfComplete();
        }

        public void SetDate(DateTime date)
        {
            Date = date;
            TrySaveIfComplete();
        }

        public void SetGoalTime(float time)
        {
            GoalTime = time;
            TrySaveIfComplete();
        }

        public void SetScore(int score)
        {
            Score = score;
            TrySaveIfComplete();
        }

        public void SetSeedValue(int seedValue)
        {
            SeedValue = seedValue;
            TrySaveIfComplete();
        }
        
        private const int CharacterLimit = 310;

        public void SetComment(string comment)
        {
            Comment = comment;
            TrySaveIfComplete();
        }

        public void SetInputDetails(List<RecordedInput> inputDetails)
        {
            InputDetails = inputDetails;
            TrySaveIfComplete();
        }

    }
}
