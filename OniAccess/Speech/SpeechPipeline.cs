using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// Central speech dispatch point for ALL speech output in the mod.
	/// No code should call SpeechEngine.Say() directly -- all speech flows through here.
	///
	/// Pipeline: Caller -> SpeechPipeline -> TextFilter -> SpeechEngine -> Tolk
	/// </summary>
	public static class SpeechPipeline {
		/// <summary>
		/// When false, all speech methods return immediately without speaking.
		/// Controlled by VanillaMode.
		/// </summary>
		private static bool _enabled = true;

		private static string _lastInterruptText;
		private static float _lastInterruptTime;
		private const float DeduplicateWindowSeconds = 0.2f;

		/// <summary>
		/// Whether the pipeline is active. When false (mod toggled off),
		/// all methods return immediately.
		/// </summary>
		public static bool IsActive => _enabled;

		/// <summary>
		/// Enable or disable the speech pipeline.
		/// Called by VanillaMode when the mod is toggled on/off.
		/// </summary>
		internal static void SetEnabled(bool enabled) {
			_enabled = enabled;
		}

		/// <summary>
		/// Interrupt mode: stop current speech and speak immediately.
		/// Used for navigation and announcements where responsiveness matters.
		/// </summary>
		/// <param name="text">Raw text (will be filtered through TextFilter).</param>
		public static void SpeakInterrupt(string text) {
			if (!_enabled) return;

			string filtered = TextFilter.FilterForSpeech(text);
			if (string.IsNullOrEmpty(filtered)) return;
			if (filtered == _lastInterruptText && UnityEngine.Time.unscaledTime - _lastInterruptTime < DeduplicateWindowSeconds)
				return;
			_lastInterruptText = filtered;
			_lastInterruptTime = UnityEngine.Time.unscaledTime;
			SpeechEngine.Say(filtered);
		}

		/// <summary>
		/// Queue mode: speak after any current speech finishes, without interrupting.
		/// Used when multiple pieces of info should be spoken in sequence
		/// (e.g., menu name followed by first item, tile element + temperature + building).
		/// </summary>
		/// <param name="text">Raw text (will be filtered through TextFilter).</param>
		public static void SpeakQueued(string text) {
			if (!_enabled) return;

			string filtered = TextFilter.FilterForSpeech(text);
			if (string.IsNullOrEmpty(filtered)) return;
			SpeechEngine.Say(filtered, interrupt: false);
		}

		/// <summary>
		/// Stop all speech.
		/// </summary>
		public static void Silence() {
			SpeechEngine.Stop();
		}
	}
}
