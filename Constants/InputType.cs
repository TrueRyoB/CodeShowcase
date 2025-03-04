namespace Fujin.Constants
{
    public enum InputActionType
    {
        MoveRight,      //side arrow
        MoveLeft,       //side arrow
        MoveUp,         //space, up arrow
        MoveDown,       //s key, down arrow
        ToggleMenu,     //esc key
        Confirm,        //enter key
        ToggleCamera,   //toggle camera
        Bug,            //exception (summons a bug on a platformer)
    }

    public enum InputPhaseType
    {
        Press,
        Release,
    }

    public class InputType
    {
        public InputActionType ActionType;
        public InputPhaseType PhaseType;

        public InputType(InputActionType actionType, InputPhaseType phaseType)
        {
            ActionType = actionType;
            PhaseType = phaseType;
        }
    }
}