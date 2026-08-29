using System;
using System.Globalization;
using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// The voice and rate the player set under System Settings, Accessibility,
	/// Spoken Content on macOS, which the system voice follows until the player
	/// picks a voice or moves the rate slider. The voice comes from
	/// NSSpeechSynthesizer.defaultVoice, which is public API. The rate has no
	/// public accessor: it lives in the com.apple.Accessibility preference
	/// domain, which is readable through NSUserDefaults without any file or
	/// privacy permission, under a key that Apple does not document. Talks to
	/// the Objective-C runtime through <see cref="ObjC"/>. Only called on macOS.
	/// </summary>
	public static class MacSpokenContent {
		/// <summary>
		/// The Spoken Content voice: its identifier (shared by NSSpeechSynthesizer
		/// and AVSpeechSynthesisVoice), display name, and BCP 47 language.
		/// False, with a logged reason, when macOS gives none or AVSpeech does not know it.
		/// </summary>
		public static bool TryGetDefaultVoice(out string identifier, out string name, out string language) {
			identifier = name = language = null;
			IntPtr pool = IntPtr.Zero;
			try {
				ObjC.LoadSpeechFrameworks();
				pool = ObjC.AutoreleasePoolPush();
				identifier = ObjC.ToManagedString(ObjC.Send(ObjC.Class("NSSpeechSynthesizer"), ObjC.Sel("defaultVoice")));
				if (string.IsNullOrEmpty(identifier)) {
					Log.Warn("macOS reports no default speech voice");
					return false;
				}
				IntPtr voice = ObjC.Send(ObjC.Class("AVSpeechSynthesisVoice"), ObjC.Sel("voiceWithIdentifier:"), ObjC.NSString(identifier));
				if (voice == IntPtr.Zero) {
					Log.Warn($"AVSpeech has no voice for the default identifier {identifier}");
					return false;
				}
				name = ObjC.ToManagedString(ObjC.Send(voice, ObjC.Sel("name")));
				language = ObjC.ToManagedString(ObjC.Send(voice, ObjC.Sel("language")));
				return !string.IsNullOrEmpty(name);
			} catch (Exception ex) {
				Log.Warn($"macOS default voice lookup failed: {ex}");
				return false;
			} finally {
				if (pool != IntPtr.Zero) ObjC.AutoreleasePoolPop(pool);
			}
		}

		/// <summary>
		/// The Spoken Content rate for the voice with <paramref name="identifier"/>,
		/// on AVSpeech's own [0, 1] scale, or -1 when the preference is absent. Read
		/// from the com.apple.Accessibility domain, key
		/// SpokenContentDefaultVoiceSelectionsByLanguage: an array alternating a
		/// language code with a dictionary holding voiceId and rate.
		/// </summary>
		public static float DefaultVoiceRate(string identifier) {
			IntPtr pool = IntPtr.Zero;
			try {
				ObjC.LoadSpeechFrameworks();
				pool = ObjC.AutoreleasePoolPush();
				// alloc/init hands back a +1 reference; released below. objectForKey:
				// returns an object the defaults keep alive for this pool cycle.
				IntPtr defaults = ObjC.Send(ObjC.Send(ObjC.Class("NSUserDefaults"), ObjC.Sel("alloc")),
					ObjC.Sel("initWithSuiteName:"), ObjC.NSString("com.apple.Accessibility"));
				IntPtr selections = ObjC.Send(defaults, ObjC.Sel("objectForKey:"),
					ObjC.NSString("SpokenContentDefaultVoiceSelectionsByLanguage"));
				ObjC.Release(defaults);
				if (selections == IntPtr.Zero || !ObjC.IsKindOfClass(selections, ObjC.Class("NSArray"))) {
					Log.Info("macOS has no Spoken Content voice selections; keeping the default rate");
					return -1f;
				}
				long count = ObjC.Count(selections);
				for (long i = 0; i < count; i++) {
					IntPtr entry = ObjC.Send(selections, ObjC.Sel("objectAtIndex:"), (IntPtr)i);
					if (!ObjC.IsKindOfClass(entry, ObjC.Class("NSDictionary"))) continue;
					if (ObjC.ToManagedString(ObjC.Send(entry, ObjC.Sel("objectForKey:"), ObjC.NSString("voiceId"))) != identifier) continue;
					string rateText = ObjC.ToManagedString(ObjC.Send(entry, ObjC.Sel("objectForKey:"), ObjC.NSString("rate")));
					float rate = ParseRate(rateText);
					if (rate < 0f)
						Log.Warn($"Spoken Content rate for {identifier} is unreadable: \"{rateText}\"");
					return rate;
				}
				Log.Info($"No Spoken Content rate stored for {identifier}; keeping the default rate");
				return -1f;
			} catch (Exception ex) {
				Log.Warn($"macOS Spoken Content rate lookup failed: {ex}");
				return -1f;
			} finally {
				if (pool != IntPtr.Zero) ObjC.AutoreleasePoolPop(pool);
			}
		}

		/// <summary>
		/// Parse a rate as macOS stores it ("0.9"), always with a period regardless
		/// of the game's locale. -1 when the text is not a number in [0, 1].
		/// </summary>
		public static float ParseRate(string text) {
			if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float rate)) return -1f;
			return rate >= 0f && rate <= 1f ? rate : -1f;
		}
	}
}
