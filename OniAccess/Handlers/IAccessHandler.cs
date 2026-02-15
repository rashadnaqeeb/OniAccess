namespace OniAccess.Handlers {
	/// <summary>
	/// Interface for mod input handlers in the handler stack.
	///
	/// Each handler detects its own keys via UnityEngine.Input.GetKeyDown() in Tick(),
	/// called once per frame by KeyPoller. HandleKeyDown processes KButtonEvents from
	/// ModInputRouter (primarily Escape interception via e.TryConsume).
	///
	/// KeyPoller and ModInputRouter walk the stack top-to-bottom. Each handler gets
	/// Tick() / HandleKeyDown() until one consumes the event or a CapturesAllInput
	/// barrier is reached. Barriers stop the walk (inclusive): handlers below a barrier
	/// receive nothing. Handlers with CapturesAllInput=false let unhandled keys fall
	/// through to handlers below, and ultimately to the game.
	///
	/// Per locked decisions:
	/// - Mode announcements interrupt current speech (OnActivate speaks DisplayName)
	/// - Name first, vary early: "Build menu" not "Menu, build"
	/// </summary>
	public interface IAccessHandler {
		/// <summary>
		/// Display name spoken on activation (e.g., "World view", "Build menu").
		/// Per locked decision: name first, vary early ("Build menu" not "Menu, build").
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Whether this handler blocks non-passthrough keys from reaching the game.
		/// When true, ModInputRouter consumes all keyboard events except mouse and Escape.
		/// When false, unconsumed events pass through to the game.
		/// </summary>
		bool CapturesAllInput { get; }

		/// <summary>
		/// Help entries for F12 navigable help list. Each handler owns its help text.
		/// </summary>
		System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }

		/// <summary>
		/// Called once per frame by KeyPoller during top-to-bottom stack walk.
		/// All key detection and mod logic happens here via UnityEngine.Input.GetKeyDown().
		/// The walk stops after any CapturesAllInput barrier (inclusive).
		/// </summary>
		void Tick();

		/// <summary>
		/// Process a key down event from ONI's KButtonEvent system during
		/// top-to-bottom stack walk. Return true to consume the event (stops walk).
		/// Primarily used for Escape interception via e.TryConsume(Action.Escape).
		/// </summary>
		bool HandleKeyDown(KButtonEvent e);

		/// <summary>
		/// Called when this handler becomes the active handler on the stack.
		/// Per locked decision: speak DisplayName here (interrupts current speech).
		/// </summary>
		void OnActivate();

		/// <summary>
		/// Called when this handler is removed from the active position.
		/// </summary>
		void OnDeactivate();
	}
}
