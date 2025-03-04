using Fujin.Constants;

namespace Fujin.Player
{
    public sealed class PlayerControllerWorldMap : PlayerPhysicsWorldMap
    {
        public void Pressed(InputActionType actionType)
        {
            switch (actionType)
            {
                case InputActionType.MoveUp:
                case InputActionType.MoveDown:
                case InputActionType.MoveLeft:
                case InputActionType.MoveRight:
                    Register(GetDirectionOf(actionType));
                    break;
                case InputActionType.ToggleMenu:
                    SetStunnedStatus(!isStunned);
                    //TODO: open menu
                    break;
                case InputActionType.Confirm:
                    //Debug
                    break;
            }
        }
        
        public void Released(InputActionType actionType)
        {
            switch (actionType)
            {
                case InputActionType.MoveUp:
                case InputActionType.MoveDown:
                case InputActionType.MoveLeft:
                case InputActionType.MoveRight:
                    Unregister(GetDirectionOf(actionType));
                    break;
            }
        }
    }

}