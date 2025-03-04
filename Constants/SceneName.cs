using System;
using System.Collections.Generic;


namespace Fujin.Constants
{
    public enum SceneTag
    {
        InitialScreen,
        HomeMenu,
        WorldMap,
        Stage1,
    }
    
    public static class SceneName
    {
        public const string InitialScene = "Title Scene";
        public const string HomeMenu = "Home Menu";
        public const string WorldMap = "World Map";
        public const string Stage1_1 = "Stage 1-1";
        public const string Stage1_2 = "Stage 1-2";
        public const string ResultScene = "Result Scene(platformer)";
        
        // Don't forget to add medal score thresholds for each stage at Constants.MedalPlatformer! by past me
        
        private static readonly HashSet<string> ValidSceneNames = new HashSet<string>
        {
            InitialScene,
            HomeMenu,
            WorldMap,
            Stage1_1,
            Stage1_2,
            ResultScene,
        };

        public static LoadingType GetLoadingType(string sceneName)
        {
            switch (sceneName)
            {
                case InitialScene:
                case HomeMenu:
                    return LoadingType.DefaultLoad();
                case WorldMap:
                    return LoadingType.HomeToWorldMap();
                case ResultScene:
                    return LoadingType.PlatformerToResult();
                case Stage1_1:
                case Stage1_2:
                    return LoadingType.WorldMapToPlatformer();
                default:
                    return LoadingType.DefaultLoad();
            }
        }

        public static bool IsValidSceneName(string input)
        {
            return ValidSceneNames.Contains(input);
        }

        public static InputProcessor GetInputProcessor(string sceneName)
        {
            switch (sceneName)
            {
                case InitialScene: 
                    throw new NotImplementedException("InitialScene is not implemented.");

                case HomeMenu: 
                    return InputProcessor.MenuNavigationAction;

                case WorldMap: 
                    return InputProcessor.MapNavigationAction;

                case Stage1_1: 
                case Stage1_2:
                    return InputProcessor.PlatformerNavigationAction;
                
                case ResultScene:
                    return InputProcessor.ResultSceneAction;

                default:
                    throw new ArgumentException($"The scene '{sceneName}' is not supported.");
            }

        }
        
    }
}