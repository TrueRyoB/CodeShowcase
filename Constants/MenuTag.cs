
namespace Fujin.Constants
{
    /// <summary>
    /// Each represents a menu potentially summoned regardless of the current scene
    /// Each should have its own child class of UIBaseMenu
    /// ...so that MenuNavigationAction can handle these
    /// </summary>
    public enum MenuTag
    {
        General,
        Settings,
    }
}