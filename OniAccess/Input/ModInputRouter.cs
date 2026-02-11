using OniAccess.Toggle;

namespace OniAccess.Input
{
    /// <summary>
    /// Single IInputHandler registered in ONI's KInputHandler tree at priority 50.
    /// Receives every KButtonEvent before PlayerController (20), KScreenManager (10),
    /// CameraController (1), and DebugHandler (-1).
    ///
    /// Delegates events to the active handler on HandlerStack. Supports two modes:
    /// - Selective claim: handler decides what to consume via e.TryConsume
    /// - Full capture: handler processes first, then all non-mouse keyboard events are consumed
    ///
    /// When VanillaMode is off, all events pass through untouched.
    /// </summary>
    public class ModInputRouter : IInputHandler
    {
        public static ModInputRouter Instance { get; private set; }

        public string handlerName => "OniAccess";
        public KInputHandler inputHandler { get; set; }

        public ModInputRouter()
        {
            Instance = this;
        }

        public void OnKeyDown(KButtonEvent e)
        {
            if (e.Consumed) return;

            // When mod is off, pass everything through
            if (!VanillaMode.IsEnabled) return;

            var handler = HandlerStack.ActiveHandler;
            if (handler == null) return;

            if (handler.CapturesAllInput)
            {
                // Full capture: handler processes first, then consume all non-mouse events
                handler.HandleKeyDown(e);
                if (!e.Consumed)
                {
                    // Let mouse and zoom actions through -- per pitfall #6
                    if (!IsMouseOrZoomAction(e))
                    {
                        e.Consumed = true;
                    }
                }
            }
            else
            {
                // Selective: handler decides what to consume via e.TryConsume
                handler.HandleKeyDown(e);
            }
        }

        public void OnKeyUp(KButtonEvent e)
        {
            if (e.Consumed || !VanillaMode.IsEnabled) return;

            var handler = HandlerStack.ActiveHandler;
            if (handler == null) return;

            if (handler.CapturesAllInput)
            {
                handler.HandleKeyUp(e);
                if (!e.Consumed && !IsMouseOrZoomAction(e))
                {
                    e.Consumed = true;
                }
            }
            else
            {
                handler.HandleKeyUp(e);
            }
        }

        /// <summary>
        /// Check if the event is a mouse or zoom action that should pass through
        /// even in full-capture mode. Per pitfall #6: full capture must not block mouse.
        /// </summary>
        private static bool IsMouseOrZoomAction(KButtonEvent e)
        {
            return e.IsAction(Action.MouseLeft) || e.IsAction(Action.MouseRight)
                || e.IsAction(Action.MouseMiddle) || e.IsAction(Action.ShiftMouseLeft)
                || e.IsAction(Action.ZoomIn) || e.IsAction(Action.ZoomOut);
        }
    }
}
