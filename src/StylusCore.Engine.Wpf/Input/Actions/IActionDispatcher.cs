using StylusCore.Core.Actions;

namespace StylusCore.Engine.Wpf.Input.Actions
{
    /// <summary>
    /// Delegate type for action handlers.
    /// </summary>
    /// <param name="payload">Optional payload for parameterized actions</param>
    public delegate void ActionHandler(IActionPayload? payload = null);

    /// <summary>
    /// Interface for action dispatching.
    /// Engine and App layers use this to execute actions by ID.
    /// </summary>
    public interface IActionDispatcher
    {
        /// <summary>
        /// Execute an action by its ID.
        /// </summary>
        /// <param name="actionId">Action identifier from ActionIds</param>
        /// <param name="payload">Optional payload for parameterized actions</param>
        /// <returns>True if action was handled, false if no handler registered</returns>
        bool Dispatch(string actionId, IActionPayload? payload = null);

        /// <summary>
        /// Check if an action can be executed (is enabled).
        /// </summary>
        bool CanExecute(string actionId);
    }
}
