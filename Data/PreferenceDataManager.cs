using UnityEngine;

namespace Fujin.Data
{
    public class PreferenceDataManager
    {
        private static PreferenceData _instance;

        public static PreferenceData Instance
        {
            get
            {
                if (_instance == null)
                {
                    RenewInstance();
                }
                return _instance;
            }
        }

        private static void RenewInstance()
        {
            _instance = new PreferenceData();
            Debug.Log(_instance.TryLoadFromJson() ? 
                "PreferenceData instance successfully loaded from saved settings" : 
                "PreferenceData instance initialized with default settings");
        }
        
        private PreferenceDataManager() { }
    }
}