using System;
using System.Collections.Generic;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Speech {
	/// <summary>Which Prism backend the player wants speech routed to.</summary>
	public enum SpeechOutputMode {
		/// <summary>Prism's best-ranked backend: the running screen reader, or the system voice when none runs.</summary>
		ScreenReader,
		/// <summary>The platform's own TTS (AVSpeech on Mac, SAPI on Windows), which queues announcements.</summary>
		SystemVoice,
	}

	/// <summary>
	/// Starts speech on the backend ModConfig asks for and pushes the configured
	/// voice, rate, and volume into it. The 0-100 settings are the player-facing
	/// scale; Prism takes [0.0, 1.0] and rejects anything outside it. A voice or
	/// rate the player has never touched leaves the operating system's own choice
	/// in place: SAPI starts on the voice and rate from the Windows Speech control
	/// panel, and on macOS the mod applies the Spoken Content voice and rate itself
	/// because Prism's AVSpeech backend does not. Volume is different: the system
	/// level is set for reading alone and can sit too low to hear over the game's
	/// music, so it always starts at full until the player turns it down.
	/// </summary>
	public static class SpeechOutputSelector {
		/// <summary>A rate the player has not set; the backend's current rate shows in its place.</summary>
		public const int Unset = -1;
		public const int VolumeDefault = 100;

		/// <summary>The Prism registry id of this platform's system TTS, or BACKEND_BEST where there is none.</summary>
		public static ulong PlatformSystemVoice() {
			switch (Application.platform) {
				case RuntimePlatform.OSXPlayer: return PrismBackend.BACKEND_AV_SPEECH;
				case RuntimePlatform.WindowsPlayer: return PrismBackend.BACKEND_SAPI;
				case RuntimePlatform.LinuxPlayer: return PrismBackend.BACKEND_SPEECH_DISPATCHER;
				default: return PrismBackend.BACKEND_BEST;
			}
		}

		/// <summary>
		/// Start (or restart) speech on the backend the config selects, then apply the
		/// voice settings when that backend accepts them.
		/// </summary>
		public static void Start() {
			ulong id = ConfigManager.Config.SpeechOutput == SpeechOutputMode.SystemVoice
				? PlatformSystemVoice()
				: PrismBackend.BACKEND_BEST;
			var backend = new PrismBackend(id);
			SpeechEngine.Initialize(backend);
			if (backend.SupportsVoiceControl)
				ApplySettings(backend);
		}

		/// <summary>The running Prism backend when it takes voice settings; otherwise null.</summary>
		public static PrismBackend VoiceControlledBackend {
			get {
				var backend = SpeechEngine.Backend as PrismBackend;
				return backend != null && backend.SupportsVoiceControl ? backend : null;
			}
		}

		/// <summary>
		/// Push the configured voice (by name), rate, and volume into <paramref name="backend"/>.
		/// Unset values fall back to the operating system's Spoken Content settings on
		/// macOS and are left alone elsewhere.
		/// </summary>
		public static void ApplySettings(PrismBackend backend) {
			var config = ConfigManager.Config;
			// The Spoken Content voice serves as the voice when none is saved and as
			// the owner of the rate when none is saved, even under a picked voice, so
			// the speed heard in the picker is the speed heard on the next launch.
			string systemVoiceId = null, systemName = null, systemLanguage = null;
			bool haveSystemVoice = Application.platform == RuntimePlatform.OSXPlayer
				&& (string.IsNullOrEmpty(config.SystemVoice) || config.SystemVoiceRate == Unset)
				&& MacSpokenContent.TryGetDefaultVoice(out systemVoiceId, out systemName, out systemLanguage);

			if (!string.IsNullOrEmpty(config.SystemVoice)) {
				int index = FindVoice(backend, config.SystemVoice, config.SystemVoiceLanguage);
				if (index >= 0)
					backend.SetVoice(index);
				else
					Log.Warn($"Configured voice \"{config.SystemVoice}\" ({config.SystemVoiceLanguage}) is not installed, using the backend default");
			} else if (haveSystemVoice) {
				int index = FindVoice(backend, systemName, systemLanguage);
				if (index >= 0) {
					backend.SetVoice(index);
					Log.Info($"Using the macOS Spoken Content voice {systemName} ({systemLanguage})");
				} else {
					Log.Warn($"macOS Spoken Content voice {systemName} ({systemLanguage}) is not in Prism's list, using the backend default");
				}
			}

			if (config.SystemVoiceRate != Unset) {
				backend.SetRate(ToUnit(config.SystemVoiceRate));
			} else if (haveSystemVoice) {
				float rate = MacSpokenContent.DefaultVoiceRate(systemVoiceId);
				if (rate >= 0f) backend.SetRate(rate);
			}
			backend.SetVolume(ToUnit(config.SystemVoiceVolume));
		}

		/// <summary>The rate slider's value: the saved setting, else what the backend is speaking at.</summary>
		public static int RatePercent(PrismBackend backend) {
			int saved = ConfigManager.Config.SystemVoiceRate;
			return saved != Unset ? saved : ToPercent(backend.GetRate());
		}

		/// <summary>
		/// Index of the voice named <paramref name="name"/> in <paramref name="language"/>,
		/// or -1. An empty language takes the first voice of that name. Voices are stored
		/// by name and language because indices shift whenever the OS voice list changes,
		/// and macOS offers one name in many languages.
		/// </summary>
		public static int FindVoice(PrismBackend backend, string name, string language) {
			int count = backend.VoiceCount;
			for (int i = 0; i < count; i++) {
				if (backend.GetVoiceName(i) != name) continue;
				if (string.IsNullOrEmpty(language) || backend.GetVoiceLanguage(i) == language) return i;
			}
			return -1;
		}

		/// <summary>
		/// Primary language subtag of a game language code ("en", "ru_klei") or a voice
		/// language tag ("en-US", "zh_CN"), lowercased. Null or empty gives "".
		/// </summary>
		public static string PrimaryLanguage(string tag) {
			if (string.IsNullOrEmpty(tag)) return "";
			int cut = tag.IndexOfAny(new[] { '-', '_' });
			return (cut > 0 ? tag.Substring(0, cut) : tag).ToLowerInvariant();
		}

		/// <summary>
		/// Backend indices of the voices that speak <paramref name="languageCode"/>,
		/// compared by primary subtag so "en" takes en-US, en-GB, and en-AU alike.
		/// Every voice when the code is empty or no voice matches: a picker with no
		/// rows would be a dead end.
		/// </summary>
		public static List<int> VoicesForLanguage(PrismBackend backend, string languageCode) {
			int count = backend.VoiceCount;
			string wanted = PrimaryLanguage(languageCode);
			var voices = new List<int>(count);
			if (wanted.Length > 0)
				for (int i = 0; i < count; i++)
					if (PrimaryLanguage(backend.GetVoiceLanguage(i)) == wanted) voices.Add(i);
			if (voices.Count > 0) return voices;
			for (int i = 0; i < count; i++) voices.Add(i);
			return voices;
		}

		/// <summary>Spoken form of voice <paramref name="index"/>: its name, then its language when known.</summary>
		public static string VoiceLabel(PrismBackend backend, int index) {
			string name = backend.GetVoiceName(index);
			string language = backend.GetVoiceLanguage(index);
			return string.IsNullOrEmpty(language) ? name : name + ", " + language;
		}

		/// <summary>Map a 0-100 setting onto Prism's [0.0, 1.0], clamping out-of-range config values.</summary>
		public static float ToUnit(int percent) {
			return Math.Max(0, Math.Min(100, percent)) / 100f;
		}

		/// <summary>
		/// Map a Prism [0.0, 1.0] value onto the 0-100 slider, rounding to the nearest
		/// step (0.9f times 100 is 89.999 in float). A failed read (negative) shows as 0.
		/// </summary>
		public static int ToPercent(float unit) {
			return (int)Math.Round(Math.Max(0f, Math.Min(1f, unit)) * 100f);
		}
	}
}
