using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for WorldGenScreen -- world generation progress display.
	///
	/// NOT a BaseMenuHandler subclass. Per Pitfall 6: WorldGenScreen has no
	/// interactive widgets. It's a progress display where the game blocks Escape
	/// and has no buttons during generation.
	///
	/// Implements IAccessHandler directly for the handler stack.
	/// Tick() polls progress each frame.
	///
	/// Per locked decisions:
	/// - Periodic progress updates: "25 percent... 50 percent... 75 percent... Done"
	/// - No user interaction during world generation
	/// - CapturesAllInput = true to block all keys
	/// </summary>
	public class WorldGenHandler: IAccessHandler {
		private readonly KScreen _screen;
		private float _lastSpokenPercent = -1f;
		private float _lastPollTime;
		private const float PollInterval = 2f; // seconds between progress checks
		private const float SpeechInterval = 25f; // speak every 25% increment

		/// <summary>
		/// Expose the screen reference so ContextDetector.OnScreenDeactivating can
		/// match this non-ScreenHandler handler to its deactivating screen.
		/// </summary>
		public KScreen Screen => _screen;

		public string DisplayName => STRINGS.ONIACCESS.HANDLERS.WORLD_GEN;

		/// <summary>
		/// Block all input during generation. The game also blocks Escape.
		/// </summary>
		public bool CapturesAllInput => true;

		/// <summary>
		/// Minimal help -- nothing to do during world gen.
		/// </summary>
		public IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>().AsReadOnly();

		public WorldGenHandler(KScreen screen) {
			_screen = screen;
		}

		/// <summary>
		/// Speak "Generating world" on activation.
		/// </summary>
		public void OnActivate() {
			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			_lastSpokenPercent = 0f;
			_lastPollTime = UnityEngine.Time.time;
		}

		public void OnDeactivate() {
			// No cleanup needed
		}

		/// <summary>
		/// Called each frame by KeyPoller.
		/// Polls world generation progress and speaks at 25% intervals.
		/// </summary>
		public void Tick() {
			float now = UnityEngine.Time.time;
			if (now - _lastPollTime < PollInterval) return;
			_lastPollTime = now;

			float percent = GetCurrentPercent();
			if (percent < 0f) return;

			int rounded = UnityEngine.Mathf.RoundToInt(percent * 100f);

			// Speak at 25% intervals
			if (rounded >= _lastSpokenPercent + SpeechInterval) {
				if (percent >= 1f) {
					Speech.SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.WORLD_GEN.COMPLETE);
				} else {
					Speech.SpeechPipeline.SpeakInterrupt(string.Format(STRINGS.ONIACCESS.WORLD_GEN.PERCENT, rounded));
				}
				_lastSpokenPercent = rounded;
			}
		}

		/// <summary>
		/// Access the current percent from OfflineWorldGen.
		/// OfflineWorldGen.currentPercent is a private float (0.0 to 1.0).
		/// Falls back to parsing percentText if Traverse fails.
		/// </summary>
		private float GetCurrentPercent() {
			try {
				// Access via screen -> offlineWorldGen -> currentPercent
				var offlineGen = Traverse.Create(_screen).Field("offlineWorldGen").GetValue<object>();
				if (offlineGen != null) {
					float percent = Traverse.Create(offlineGen).Field("currentPercent").GetValue<float>();
					return percent;
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"WorldGenHandler: failed to read currentPercent: {ex.Message}");
			}

			// Fallback: try to read the percentText LocText
			try {
				var offlineGen = Traverse.Create(_screen).Field("offlineWorldGen").GetValue<object>();
				if (offlineGen != null) {
					var percentText = Traverse.Create(offlineGen).Field("percentText").GetValue<LocText>();
					if (percentText != null && !string.IsNullOrEmpty(percentText.text)) {
						// Text is formatted like "45%" via GameUtil.GetFormattedPercent
						string text = percentText.text.Replace("%", "").Trim();
						if (float.TryParse(text, out float val)) {
							return val / 100f;
						}
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"WorldGenHandler: failed to read percentText: {ex.Message}");
			}

			return -1f;
		}

		/// <summary>
		/// No Escape interception during world generation.
		/// </summary>
		public bool HandleKeyDown(KButtonEvent e) => false;
	}
}
