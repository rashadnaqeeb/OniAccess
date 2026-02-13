using OniAccess.Toggle;

namespace OniAccess.Input {
	/// <summary>
	/// Single IInputHandler registered in ONI's KInputHandler tree at priority 50.
	/// Receives every KButtonEvent before PlayerController (20), KScreenManager (10),
	/// CameraController (1), and DebugHandler (-1).
	///
	/// Acts as a gate: asks the active handler to handle Escape (the only key that
	/// needs atomic TryConsume), then blocks all non-passthrough keys for handlers
	/// with CapturesAllInput=true. No stack walking, no dispatch.
	///
	/// When VanillaMode is off, all events pass through untouched.
	/// </summary>
	public class ModInputRouter: IInputHandler {
		public static ModInputRouter Instance { get; private set; }

		public string handlerName => "OniAccess";
		public KInputHandler inputHandler { get; set; }

		public ModInputRouter() {
			Instance = this;
		}

		public void OnKeyDown(KButtonEvent e) {
			if (e.Consumed || !VanillaMode.IsEnabled) return;

			var handler = HandlerStack.ActiveHandler;
			if (handler == null) return;

			// Escape interception: handler consumes atomically
			if (handler.HandleKeyDown(e)) return;

			// Gate: block non-passthrough keys for capturing handlers
			if (handler.CapturesAllInput && !IsPassThroughAction(e))
				e.Consumed = true;
		}

		public void OnKeyUp(KButtonEvent e) {
			if (e.Consumed || !VanillaMode.IsEnabled) return;

			var handler = HandlerStack.ActiveHandler;
			if (handler == null) return;

			// Gate: block non-passthrough key-ups for capturing handlers
			if (handler.CapturesAllInput && !IsPassThroughAction(e))
				e.Consumed = true;
		}

		/// <summary>
		/// Check if the event is an action that should pass through even in
		/// full-capture mode. Includes mouse/zoom (pitfall #6) and Escape.
		/// Escape must reach game screens so KScreen.OnKeyDown can call Deactivate(),
		/// which fires our Harmony patch, which pops the handler. Handlers that need
		/// to intercept Escape consume it in HandleKeyDown before this check runs.
		/// </summary>
		private static bool IsPassThroughAction(KButtonEvent e) {
			return e.IsAction(Action.MouseLeft) || e.IsAction(Action.MouseRight)
				|| e.IsAction(Action.MouseMiddle) || e.IsAction(Action.ShiftMouseLeft)
				|| e.IsAction(Action.ZoomIn) || e.IsAction(Action.ZoomOut)
				|| e.IsAction(Action.Escape);
		}
	}
}
