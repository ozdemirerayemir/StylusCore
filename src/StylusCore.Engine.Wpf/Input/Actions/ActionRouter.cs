using StylusCore.Core.Actions;

namespace StylusCore.Engine.Wpf.Input.Actions
{
    /// <summary>
    /// Runtime action dispatcher that maps ActionIds to handlers.
    /// This is the Engine-side implementation that owns the handler registry.
    /// 
    /// IMPORTANT: Handler registration happens HERE, not in Core.
    /// Core only defines the action IDs and payloads.
    /// </summary>
    public class ActionRouter : IActionDispatcher
    {
        private readonly Dictionary<string, ActionHandler> _handlers = new();
        private readonly Dictionary<string, Func<bool>> _canExecuteChecks = new();

        /// <summary>
        /// Event raised when an action is dispatched (for logging/telemetry).
        /// </summary>
        public event Action<string, bool>? ActionDispatched;

        /// <summary>
        /// Register a handler for an action.
        /// </summary>
        /// <param name="actionId">Action ID from ActionIds constants</param>
        /// <param name="handler">Handler to execute</param>
        /// <param name="canExecute">Optional predicate for can-execute state</param>
        public void Register(string actionId, ActionHandler handler, Func<bool>? canExecute = null)
        {
            _handlers[actionId] = handler;
            if (canExecute != null)
            {
                _canExecuteChecks[actionId] = canExecute;
            }
        }

        /// <summary>
        /// Unregister a handler for an action.
        /// </summary>
        public void Unregister(string actionId)
        {
            _handlers.Remove(actionId);
            _canExecuteChecks.Remove(actionId);
        }

        /// <inheritdoc />
        public bool Dispatch(string actionId, IActionPayload? payload = null)
        {
            if (_handlers.TryGetValue(actionId, out var handler))
            {
                if (!CanExecute(actionId))
                {
                    ActionDispatched?.Invoke(actionId, false);
                    return false;
                }

                handler(payload);
                ActionDispatched?.Invoke(actionId, true);
                return true;
            }

            ActionDispatched?.Invoke(actionId, false);
            return false;
        }

        /// <inheritdoc />
        public bool CanExecute(string actionId)
        {
            if (_canExecuteChecks.TryGetValue(actionId, out var check))
            {
                return check();
            }
            // Default: can execute if handler exists
            return _handlers.ContainsKey(actionId);
        }

        /// <summary>
        /// Check if a handler is registered for an action.
        /// </summary>
        public bool IsRegistered(string actionId) => _handlers.ContainsKey(actionId);

        /// <summary>
        /// Get all registered action IDs.
        /// </summary>
        public IEnumerable<string> GetRegisteredActions() => _handlers.Keys;
    }
}
