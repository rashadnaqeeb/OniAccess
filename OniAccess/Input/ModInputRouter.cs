using OniAccess.Toggle;

namespace OniAccess.Input
{
    /// <summary>
    /// Single IInputHandler registered in ONI's KInputHandler tree at priority 50.
    /// Receives every KButtonEvent before PlayerController (20), KScreenManager (10),
    /// CameraController (1), and DebugHandler (-1).
    ///
    /// Walks the handler stack top-to-bottom for each event. Each handler gets a
    /// chance to consume the key. If it doesn't and CapturesAllInput is false, the
    /// event falls through to the next handler. A CapturesAllInput handler stops
    /// the walk -- unconsumed non-mouse keys are blocked from reaching handlers
    /// below or the game.
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
            if (e.Consumed || !VanillaMode.IsEnabled) return;

            var handlers = HandlerStack.Handlers;

            // Walk top-to-bottom: top handler gets first refusal
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                var handler = handlers[i];
                handler.HandleKeyDown(e);

                if (e.Consumed) return;

                // CapturesAllInput stops the walk -- block non-mouse keys
                if (handler.CapturesAllInput)
                {
                    if (!IsMouseOrZoomAction(e))
                        e.Consumed = true;
                    return;
                }
            }
        }

        public void OnKeyUp(KButtonEvent e)
        {
            if (e.Consumed || !VanillaMode.IsEnabled) return;

            var handlers = HandlerStack.Handlers;

            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                var handler = handlers[i];
                handler.HandleKeyUp(e);

                if (e.Consumed) return;

                if (handler.CapturesAllInput)
                {
                    if (!IsMouseOrZoomAction(e))
                        e.Consumed = true;
                    return;
                }
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
