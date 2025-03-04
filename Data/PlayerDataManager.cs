using UnityEngine;

namespace Fujin.Data
{
    public class PlayerDataManager
    {
        private static PlayerData _instance;

        public static PlayerData Instance
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
            _instance = new PlayerData();
            Debug.Log(_instance.TryLoadFromJson() ? 
                "PlayerData instance successfully loaded from saved settings" : 
                "PlayerData instance initialized with default settings");
        }
        
        private PlayerDataManager() { }
    }
}