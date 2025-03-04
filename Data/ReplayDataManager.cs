using UnityEngine;

namespace Fujin.Data
{
    /// <summary>
    /// Allows access to ReplayData instance for other classes
    /// </summary>
    public class ReplayDataManager
    {
        private static ReplayData _instance;

        public static ReplayData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ReplayData();
                    Debug.Log("ReplayData instance initialized.");
                }
                return _instance;
            }
        }

        public static void ResetInstance()
        {
            if (_instance != null)
            {
                _instance = null;
            }
            
            _instance = new ReplayData();
        }

        private ReplayDataManager() { }
    }
}