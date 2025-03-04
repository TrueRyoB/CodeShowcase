namespace Fujin.Constants
{
    /// <summary>
    /// Structure contianing three types of information (means, pattern, and shape)...
    /// for passing it to a class SceneLoadManager and its function LoadScene
    /// </summary>
    public struct LoadingType
    {
        public LoadingTransitionType BeginningTransition;
        public LoadingScreenType LoadingScreen;
        public LoadingTransitionType EndingTransition;

        public LoadingType(LoadingTransitionType v1, LoadingScreenType v2, LoadingTransitionType v3)
        {
            BeginningTransition = v1;
            LoadingScreen = v2;
            EndingTransition = v3;
        }

        public static LoadingType WorldMapToPlatformer()
        {
            return new LoadingType(LoadingTransitionType.IntoStageNode(), LoadingScreenType.BeforePlatformer(), LoadingTransitionType.IntoPlatformerStage());
        }

        public static LoadingType HomeToWorldMap()
        {
            return new LoadingType(LoadingTransitionType.BlightBeginning(), LoadingScreenType.BeforeWorldMap(), LoadingTransitionType.DefaultEnding());
        }

        public static LoadingType PlatformerToResult()
        {
            return new LoadingType(LoadingTransitionType.FocusBeginning(), LoadingScreenType.JustWait(), LoadingTransitionType.Nothing());
        }

        public static LoadingType DefaultLoad()
        {
            return new LoadingType(LoadingTransitionType.DefaultBeginning(), LoadingScreenType.DefaultLoadingScreen(), LoadingTransitionType.DefaultEnding());
        }
    }

    /// <summary>
    /// Child of structure LoadingType to
    /// ...pass the info about the loading screen type (such as the presence of tips and such)
    /// </summary>
    public struct LoadingScreenType
    {
        public LoadingBackgroundPattern BackgroundPattern;
        public LoadingScreenDecoration Decoration;

        public LoadingScreenType(LoadingBackgroundPattern backgroundPattern, LoadingScreenDecoration decoration)
        {
            BackgroundPattern = backgroundPattern;
            Decoration = decoration;
        }
        
        /// <summary>
        /// Consists of "plain dark" and "tips and spin"
        /// </summary>
        /// <returns></returns>
        public static LoadingScreenType BeforePlatformer()
        {
            return new LoadingScreenType(LoadingBackgroundPattern.PlainDark, LoadingScreenDecoration.TipsAndSpin);
        }

        public static LoadingScreenType JustWait()
        {
            return new LoadingScreenType(LoadingBackgroundPattern.Transparent, LoadingScreenDecoration.JustWait);
        }

        /// <summary>
        /// Consists of "plain white" and "yukkuri spin"
        /// </summary>
        /// <returns></returns>
        public static LoadingScreenType BeforeWorldMap()
        {
            return new LoadingScreenType(LoadingBackgroundPattern.PlainWhite, LoadingScreenDecoration.YukkuriSpin);
        }

        /// <summary>
        /// Consists of "plain dark" and "nothing"
        /// </summary>
        /// <returns></returns>
        public static LoadingScreenType DefaultLoadingScreen()
        {
            return new LoadingScreenType(LoadingBackgroundPattern.PlainDark, LoadingScreenDecoration.Nothing);
        }
        
        public static LoadingScreenType Nothing()
        {
            return new LoadingScreenType(LoadingBackgroundPattern.Transparent,
                LoadingScreenDecoration.Nothing);
        }
    }

    /// <summary>
    /// Child of structure LoadingType to
    /// ...pass the info about beginning/ending of the loading screen animation
    /// </summary>
    public struct LoadingTransitionType
    {
        public LoadingTransitionMeans Means;
        public LoadingBackgroundPattern BackgroundPattern;
        public LoadingPunchShape PunchShape;
        
        public LoadingTransitionType(LoadingTransitionMeans means, LoadingBackgroundPattern pattern, LoadingPunchShape shape)
        {
            Means = means;
            BackgroundPattern = pattern;
            PunchShape = shape;
        }
        
        /// <summary>
        /// Stands for "FadeOut", "plain dark", "circle"
        /// </summary>
        /// <returns></returns>
        public static LoadingTransitionType DefaultBeginning()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.FadeOut, LoadingBackgroundPattern.PlainDark, LoadingPunchShape.Circle);
        }

        /// <summary>
        /// To hide a scene transition from players
        /// </summary>
        /// <returns></returns>
        public static LoadingTransitionType Nothing()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.Nothing, LoadingBackgroundPattern.Transparent,
                LoadingPunchShape.Circle);
        }

        /// <summary>
        /// Consists of "Focus", "transparent", "circle"
        /// </summary>
        /// <returns></returns>
        public static LoadingTransitionType FocusBeginning()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.Focus, LoadingBackgroundPattern.Transparent,
                LoadingPunchShape.Circle);
        }

        /// <summary>
        /// Stands for "FadeOut", "plain white", "circle"
        /// </summary>
        /// <returns></returns>
        public static LoadingTransitionType BlightBeginning()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.FadeOut, LoadingBackgroundPattern.PlainWhite, LoadingPunchShape.Circle);
        }

        /// <summary>
        /// Stands for "FadeOut", "plain dark", "circle"
        /// </summary>
        /// <returns></returns>
        public static LoadingTransitionType DefaultEnding()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.FadeIn, LoadingBackgroundPattern.PlainDark, LoadingPunchShape.Circle);
        }

        public static LoadingTransitionType IntoStageNode()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.SpotIn, LoadingBackgroundPattern.PlainDark, LoadingPunchShape.Circle);
        }
        
        public static LoadingTransitionType IntoPlatformerStage()
        {
            return new LoadingTransitionType(LoadingTransitionMeans.SpotOut, LoadingBackgroundPattern.PlainDark, LoadingPunchShape.Circle);
        }
    }
    
    public enum LoadingTransitionMeans
    {
        FadeIn = 0,
        FadeOut = 1,
        SpotIn = 2,
        SpotOut = 3,
        SlideToRight = 4,
        Nothing = 5,
        Focus = 6,
    }

    public enum LoadingBackgroundPattern
    {
        PlainDark = 0,
        PlainWhite = 1,
        Transparent = 2,
        //GradientBlue = 3,
    }

    public enum LoadingPunchShape
    {
        Circle = 0,
        Square = 1,
        Star = 2,
        Kaguo = 3
    }

    public enum LoadingScreenDecoration
    {
        Nothing = 0,
        TipsAndSpin = 1,
        YukkuriSpin = 2,
        GirlPraying = 3,
        JustWait = 4,
    }

}