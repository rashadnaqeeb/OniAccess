using System.Collections.Generic;

namespace OniAccess.Input
{
    /// <summary>
    /// Manages a stack of IAccessHandler instances.
    /// The last element is the active handler that receives input events.
    ///
    /// Push/Pop/Replace manage the handler lifecycle with proper OnActivate/OnDeactivate
    /// callbacks. The handler stack represents the current input context hierarchy:
    /// e.g., [WorldHandler] or [WorldHandler, MenuHandler] or [WorldHandler, MenuHandler, HelpHandler].
    ///
    /// All methods are safe for null/empty stack (no exceptions thrown).
    /// </summary>
    public static class HandlerStack
    {
        private static readonly List<IAccessHandler> _stack = new List<IAccessHandler>();

        /// <summary>
        /// The currently active handler (top of stack), or null if stack is empty.
        /// </summary>
        public static IAccessHandler ActiveHandler =>
            _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        /// <summary>
        /// Number of handlers on the stack. Useful for debugging.
        /// </summary>
        public static int Count => _stack.Count;

        /// <summary>
        /// Push a handler onto the stack, making it the active handler.
        /// Calls handler.OnActivate(). Does NOT call OnDeactivate on the previous
        /// active handler -- it is just obscured, not removed. It will become active
        /// again when the top handler is popped.
        /// </summary>
        public static void Push(IAccessHandler handler)
        {
            if (handler == null)
            {
                Util.Log.Warn("HandlerStack.Push called with null handler");
                return;
            }

            _stack.Add(handler);
            handler.OnActivate();
            Util.Log.Debug($"HandlerStack.Push: {handler.DisplayName} (depth={_stack.Count})");
        }

        /// <summary>
        /// Remove the top handler from the stack, calling its OnDeactivate.
        /// If a handler is now exposed underneath, calls its OnActivate.
        /// </summary>
        public static void Pop()
        {
            if (_stack.Count == 0)
            {
                Util.Log.Warn("HandlerStack.Pop called on empty stack");
                return;
            }

            var removed = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            removed.OnDeactivate();
            Util.Log.Debug($"HandlerStack.Pop: {removed.DisplayName} (depth={_stack.Count})");

            // If a handler is now exposed underneath, reactivate it
            var newActive = ActiveHandler;
            if (newActive != null)
            {
                newActive.OnActivate();
                Util.Log.Debug($"HandlerStack.Pop: reactivated {newActive.DisplayName}");
            }
        }

        /// <summary>
        /// Replace the current active handler with a new one.
        /// Pops the current handler (calling OnDeactivate), then pushes the new handler
        /// (calling OnActivate). Used by ContextDetector when switching between
        /// same-level handlers (e.g., WorldHandler to MenuHandler).
        /// If stack is empty, equivalent to Push.
        /// </summary>
        public static void Replace(IAccessHandler handler)
        {
            if (handler == null)
            {
                Util.Log.Warn("HandlerStack.Replace called with null handler");
                return;
            }

            if (_stack.Count > 0)
            {
                var removed = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
                removed.OnDeactivate();
                Util.Log.Debug($"HandlerStack.Replace: removed {removed.DisplayName}");
            }

            _stack.Add(handler);
            handler.OnActivate();
            Util.Log.Debug($"HandlerStack.Replace: activated {handler.DisplayName} (depth={_stack.Count})");
        }

        /// <summary>
        /// Deactivate the active handler and clear the entire stack.
        /// Only calls OnDeactivate on the active (top) handler -- obscured handlers
        /// are silently removed. Used by VanillaMode toggle OFF.
        /// </summary>
        public static void DeactivateAll()
        {
            var active = ActiveHandler;
            if (active != null)
            {
                active.OnDeactivate();
                Util.Log.Debug($"HandlerStack.DeactivateAll: deactivated {active.DisplayName}");
            }
            _stack.Clear();
        }

        /// <summary>
        /// Clear the stack without calling any callbacks.
        /// Used for emergency cleanup only.
        /// </summary>
        public static void Clear()
        {
            _stack.Clear();
            Util.Log.Debug("HandlerStack.Clear: stack cleared without callbacks");
        }
    }
}
