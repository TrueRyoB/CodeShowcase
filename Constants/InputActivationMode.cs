
namespace Fujin.Constants
{
    /// <summary>
    /// represents different modes of input activation
    /// designed to alleviate data loads for replay
    ///
    /// specifically, it allows storing release case only for InputActivationMode.Hold as
    /// it's not necessary for Edge as there has to be an intervening KeyGetUp
    /// </summary>
    public enum InputActivationMode
    {
        /// <summary>
        /// An input is triggered only on KeyGetDown and requires release and re-press for subsequent activation.
        /// </summary>
        Edge, 
        
        /// <summary>
        /// An input is triggered and will keep activating until a release
        /// </summary>
        Hold,
    }
}