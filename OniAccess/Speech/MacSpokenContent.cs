using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// The voice and rate the player set under System Settings, Accessibility,
	/// Spoken Content on macOS. Prism's AVSpeech backend starts on the first voice
	/// of the current language instead, so the mod asks AppKit for the real one.
	/// The voice comes from NSSpeechSynthesizer.defaultVoice, which is public API.
	/// The rate has no public accessor: it lives in the com.apple.Accessibility
	/// preference domain, which is readable through NSUserDefaults without any
	/// file or privacy permission, under a key that Apple does not document.
	/// Everything here talks to the Objective-C runtime through objc_msgSend with
	/// pointer or BOOL results only, since the game runs as x86_64 under Rosetta
	/// where floating-point returns would need a different entry point.
	/// </summary>
	public static class MacSpokenContent {
		const string ObjC = "/usr/lib/libobjc.A.dylib";
		const int RTLD_NOW = 2;

		[DllImport("/usr/lib/libSystem.B.dylib")]
		private static extern IntPtr dlopen(string path, int mode);

		[DllImport(ObjC)]
		private static extern IntPtr objc_getClass(string name);

		[DllImport(ObjC)]
		private static extern IntPtr sel_registerName(string name);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr Send(IntPtr receiver, IntPtr selector);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr Send(IntPtr receiver, IntPtr selector, byte[] utf8);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static extern bool SendBool(IntPtr receiver, IntPtr selector, IntPtr arg);

		private static IntPtr Class(string name) => objc_getClass(name);
		private static IntPtr Sel(string name) => sel_registerName(name);

		/// <summary>An autoreleased NSString holding <paramref name="text"/>.</summary>
		private static IntPtr NSString(string text) =>
			Send(Class("NSString"), Sel("stringWithUTF8String:"), PrismBackend.ToUtf8(text));

		/// <summary>The UTF-8 contents of an NSString, or of any object's description when it is not one.</summary>
		private static string ToString(IntPtr obj) {
			if (obj == IntPtr.Zero) return null;
			if (!SendBool(obj, Sel("isKindOfClass:"), Class("NSString")))
				obj = Send(obj, Sel("description"));
			return PrismBackend.PtrToUtf8(Send(obj, Sel("UTF8String")));
		}

		private static bool _loaded;

		private static void LoadFrameworks() {
			if (_loaded) return;
			// The game links AppKit already; AVFoundation may not be resident yet.
			dlopen("/System/Library/Frameworks/AppKit.framework/AppKit", RTLD_NOW);
			dlopen("/System/Library/Frameworks/AVFoundation.framework/AVFoundation", RTLD_NOW);
			_loaded = true;
		}

		/// <summary>
		/// The Spoken Content voice, as Prism reports it: its identifier (for
		/// <see cref="DefaultVoiceRate"/>), display name, and BCP 47 language.
		/// False, with a logged reason, when macOS gives none or AVSpeech does not know it.
		/// </summary>
		public static bool TryGetDefaultVoice(out string identifier, out string name, out string language) {
			identifier = name = language = null;
			try {
				LoadFrameworks();
				identifier = ToString(Send(Class("NSSpeechSynthesizer"), Sel("defaultVoice")));
				if (string.IsNullOrEmpty(identifier)) {
					Log.Warn("macOS reports no default speech voice");
					return false;
				}
				IntPtr voice = Send(Class("AVSpeechSynthesisVoice"), Sel("voiceWithIdentifier:"), NSString(identifier));
				if (voice == IntPtr.Zero) {
					Log.Warn($"AVSpeech has no voice for the default identifier {identifier}");
					return false;
				}
				name = ToString(Send(voice, Sel("name")));
				language = ToString(Send(voice, Sel("language")));
				return !string.IsNullOrEmpty(name);
			} catch (Exception ex) {
				Log.Warn($"macOS default voice lookup failed: {ex}");
				return false;
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
			try {
				LoadFrameworks();
				IntPtr defaults = Send(Send(Class("NSUserDefaults"), Sel("alloc")),
					Sel("initWithSuiteName:"), NSString("com.apple.Accessibility"));
				IntPtr selections = Send(defaults, Sel("objectForKey:"),
					NSString("SpokenContentDefaultVoiceSelectionsByLanguage"));
				if (selections == IntPtr.Zero || !SendBool(selections, Sel("isKindOfClass:"), Class("NSArray"))) {
					Log.Info("macOS has no Spoken Content voice selections; keeping the default rate");
					return -1f;
				}
				long count = (long)Send(selections, Sel("count"));
				for (long i = 0; i < count; i++) {
					IntPtr entry = Send(selections, Sel("objectAtIndex:"), (IntPtr)i);
					if (!SendBool(entry, Sel("isKindOfClass:"), Class("NSDictionary"))) continue;
					if (ToString(Send(entry, Sel("objectForKey:"), NSString("voiceId"))) != identifier) continue;
					string rateText = ToString(Send(entry, Sel("objectForKey:"), NSString("rate")));
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
