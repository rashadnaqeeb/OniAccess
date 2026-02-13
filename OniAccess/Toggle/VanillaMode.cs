using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Toggle {
	/// <summary>
	/// Mod on/off toggle with full handler-stack integration.
	///
	/// Per locked decisions:
	/// - Toggle OFF: speak "Oni-Access off" THEN full disable (deactivate all handlers,
	///   stop speech, set flag). Speech must happen BEFORE disable.
	/// - Toggle ON: set flag first, enable speech, speak "Oni-Access on", detect current
	///   state and activate handler. NO state dump.
	/// - No background work when off -- full stop.
	/// - Only Ctrl+Shift+F12 remains active when mod is off (handled by KeyPoller directly).
	/// </summary>
	public static class VanillaMode {
		/// <summary>
		/// Whether the mod is currently enabled. Starts ON.
		/// When false, ModInputRouter passes all keys through and KeyPoller
		/// only checks Ctrl+Shift+F12.
		/// </summary>
		public static bool IsEnabled { get; private set; } = true;

		/// <summary>
		/// Toggle the mod on or off. Called by KeyPoller on Ctrl+Shift+F12.
		/// </summary>
		public static void Toggle() {
			if (IsEnabled) {
				// Turning OFF
				// 1. Speak confirmation WHILE pipeline is still active
				SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_OFF);
				// 2. Deactivate all handlers (calls OnDeactivate on active, clears stack)
				HandlerStack.DeactivateAll();
				// 3. Disable speech pipeline (all subsequent calls are no-ops)
				SpeechPipeline.SetEnabled(false);
				// 4. Set flag last -- ModInputRouter checks this to pass all keys through
				IsEnabled = false;
			} else {
				// Turning ON
				// 1. Set flag first -- enables ModInputRouter and KeyPoller processing
				IsEnabled = true;
				// 2. Enable speech pipeline
				SpeechPipeline.SetEnabled(true);
				// 3. Speak confirmation only -- no state dump per locked decision
				SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_ON);
				// 4. Detect current game state and activate appropriate handler
				ContextDetector.DetectAndActivate();
			}
		}
	}
}
