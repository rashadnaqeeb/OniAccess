using System.Collections.Generic;

namespace OniAccess.Input {
	/// <summary>
	/// Abstract base for ALL screen handlers. Provides only the infrastructure
	/// that every screen type shares:
	/// - Screen reference for ContextDetector matching
	/// - Display name spoken on activation
	/// - F12 help system (detected in Tick)
	/// - CapturesAllInput (all screens block input fallthrough)
	///
	/// BaseMenuHandler extends this with widget lists, 1D navigation, type-ahead
	/// search, tooltip reading, and widget interaction.
	/// Future 2D grid handlers extend ScreenHandler directly with their own
	/// state (cursor position, tile data) without inheriting menu-specific baggage.
	///
	/// Per locked decisions:
	/// - CapturesAllInput = true for all screen handlers
	/// - Name first, vary early: DisplayName is spoken on activation
	/// </summary>
	public abstract class ScreenHandler: IAccessHandler {
		protected KScreen _screen;

		/// <summary>
		/// The KScreen this handler manages. Used by ContextDetector to match
		/// a deactivating screen to its handler for correct Pop behavior.
		/// </summary>
		public KScreen Screen => _screen;

		/// <summary>
		/// Display name spoken on activation (e.g., "Options", "Pause").
		/// Per locked decision: name first, vary early.
		/// </summary>
		public abstract string DisplayName { get; }

		/// <summary>
		/// Help entries for F12 navigable help list. Subclasses compose from
		/// screen-type-specific entry lists (MenuHelpEntries, ListNavHelpEntries, etc.).
		/// </summary>
		public abstract IReadOnlyList<HelpEntry> HelpEntries { get; }

		/// <summary>
		/// Whether this handler blocks input from reaching handlers below it on the stack.
		/// Menus return true (modal); grid/world handlers return false (non-modal).
		/// </summary>
		public abstract bool CapturesAllInput { get; }

		// ========================================
		// CONSTRUCTOR
		// ========================================

		protected ScreenHandler(KScreen screen) {
			_screen = screen;
		}

		// ========================================
		// IAccessHandler IMPLEMENTATION
		// ========================================

		/// <summary>
		/// Called when this handler becomes active on the stack.
		/// Speaks the screen name. Subclasses extend for additional setup.
		/// </summary>
		public virtual void OnActivate() {
			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		/// <summary>
		/// Called when this handler is popped off the stack.
		/// </summary>
		public virtual void OnDeactivate() {
		}

		/// <summary>
		/// Per-frame key detection. F12 (without modifiers) pushes help.
		/// Subclasses override and call base.Tick() to keep F12 behavior.
		/// </summary>
		public virtual void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F12)
				&& !InputUtil.AnyModifierHeld()) {
				HandlerStack.Push(new HelpHandler(HelpEntries));
			}
		}

		/// <summary>
		/// Handle Escape interception from ONI's KButtonEvent system.
		/// Default: pass through (let the game close the screen, which pops
		/// the handler via Harmony patch).
		/// </summary>
		public virtual bool HandleKeyDown(KButtonEvent e) {
			return false;
		}
	}
}
