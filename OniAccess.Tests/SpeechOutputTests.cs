using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using OniAccess.Speech;

namespace OniAccess.Tests {
	/// <summary>
	/// Offline tests for the two pure boundaries between the voice settings and Prism.
	/// Prism rejects a rate or volume outside [0.0, 1.0] with RANGE_OUT_OF_BOUNDS, so a
	/// config value that maps outside it silently leaves the old setting in place; and
	/// voice names come back as UTF-8, so an ANSI decode would garble any non-ASCII
	/// name in the picker and never match the saved name on the next launch. The
	/// picker filters voices by the game language, so a wrong primary-subtag split
	/// would silently hide every voice or show all 180 of them. The sliders show
	/// the backend's live value when nothing is saved, so a truncating float to
	/// percent conversion would announce 89 for a rate of 0.9; and the macOS Spoken
	/// Content rate is stored with a period, so a locale-sensitive parse would
	/// throw it away on any comma-decimal system.
	/// </summary>
	static class SpeechOutputTests {
		public static IEnumerable<(string, bool, string)> All() {
			yield return UnitEndpoints();
			yield return UnitClampsOutOfRangeConfig();
			yield return PercentRoundsFloat();
			yield return PercentOfFailedReadIsZero();
			yield return RateParsesWithPeriodUnderCommaLocale();
			yield return RateRejectsOutOfRange();
			yield return Utf8NameDecodes();
			yield return NullNameIsNull();
			yield return PrimaryLanguageSplitsRegionAndSuffix();
			yield return PrimaryLanguageOfEmptyIsEmpty();
		}

		private static (string, bool, string) Assert(string name, bool ok, string detail)
			=> (name, ok, ok ? "OK" : detail);

		private static (string, bool, string) UnitEndpoints() {
			float lo = SpeechOutputSelector.ToUnit(0);
			float mid = SpeechOutputSelector.ToUnit(50);
			float hi = SpeechOutputSelector.ToUnit(100);
			bool ok = lo == 0f && mid == 0.5f && hi == 1f;
			return Assert("UnitEndpoints", ok, $"{lo} {mid} {hi}");
		}

		private static (string, bool, string) UnitClampsOutOfRangeConfig() {
			// A hand-edited config of 150 or -5 must still land inside Prism's range.
			float over = SpeechOutputSelector.ToUnit(150);
			float under = SpeechOutputSelector.ToUnit(-5);
			bool ok = over == 1f && under == 0f;
			return Assert("UnitClampsOutOfRangeConfig", ok, $"{over} {under}");
		}

		private static (string, bool, string) PercentRoundsFloat() {
			// 0.9f * 100 is 89.99999 in single precision; the slider must say 90.
			int a = SpeechOutputSelector.ToPercent(0.9f);
			int b = SpeechOutputSelector.ToPercent(0.29f);
			int c = SpeechOutputSelector.ToPercent(1f);
			bool ok = a == 90 && b == 29 && c == 100;
			return Assert("PercentRoundsFloat", ok, $"{a} {b} {c}");
		}

		private static (string, bool, string) PercentOfFailedReadIsZero() {
			// PrismBackend.GetRate returns -1 on failure; the slider must stay in range.
			int p = SpeechOutputSelector.ToPercent(-1f);
			return Assert("PercentOfFailedReadIsZero", p == 0, $"{p}");
		}

		private static (string, bool, string) RateParsesWithPeriodUnderCommaLocale() {
			var previous = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			try {
				float rate = MacSpokenContent.ParseRate("0.9");
				return Assert("RateParsesWithPeriodUnderCommaLocale", rate == 0.9f, $"{rate}");
			} finally {
				Thread.CurrentThread.CurrentCulture = previous;
			}
		}

		private static (string, bool, string) RateRejectsOutOfRange() {
			// Prism refuses anything outside [0, 1]; a bad preference must not reach it.
			float a = MacSpokenContent.ParseRate("1.5");
			float b = MacSpokenContent.ParseRate("abc");
			float c = MacSpokenContent.ParseRate(null);
			bool ok = a == -1f && b == -1f && c == -1f;
			return Assert("RateRejectsOutOfRange", ok, $"{a} {b} {c}");
		}

		private static string RoundTrip(string text) {
			byte[] bytes = Encoding.UTF8.GetBytes(text + "\0");
			IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
			try {
				Marshal.Copy(bytes, 0, ptr, bytes.Length);
				return PrismBackend.PtrToUtf8(ptr);
			} finally {
				Marshal.FreeHGlobal(ptr);
			}
		}

		private static (string, bool, string) Utf8NameDecodes() {
			// Multi-byte names (a Chinese voice, an accented one) survive intact.
			string a = RoundTrip("婷婷");
			string b = RoundTrip("Amélie");
			bool ok = a == "婷婷" && b == "Amélie";
			return Assert("Utf8NameDecodes", ok, $"\"{a}\" \"{b}\"");
		}

		private static (string, bool, string) NullNameIsNull() {
			string s = PrismBackend.PtrToUtf8(IntPtr.Zero);
			return Assert("NullNameIsNull", s == null, $"\"{s}\"");
		}

		private static (string, bool, string) PrimaryLanguageSplitsRegionAndSuffix() {
			// Voice tags use a hyphen (en-GB), the game's Klei codes an underscore
			// (ru_klei), and a bare code (zh) has nothing to strip.
			var cases = new[] {
				("en-GB", "en"), ("ru_klei", "ru"), ("zh", "zh"), ("EN-us", "en"), ("yue-HK", "yue"),
			};
			foreach (var (tag, expected) in cases) {
				string got = SpeechOutputSelector.PrimaryLanguage(tag);
				if (got != expected)
					return Assert("PrimaryLanguageSplitsRegionAndSuffix", false, $"{tag} -> \"{got}\", expected {expected}");
			}
			return Assert("PrimaryLanguageSplitsRegionAndSuffix", true, "");
		}

		private static (string, bool, string) PrimaryLanguageOfEmptyIsEmpty() {
			// Empty never equals a real subtag, so an unknown game code or a voice
			// without a language falls through to the unfiltered list.
			string a = SpeechOutputSelector.PrimaryLanguage(null);
			string b = SpeechOutputSelector.PrimaryLanguage("");
			return Assert("PrimaryLanguageOfEmptyIsEmpty", a == "" && b == "", $"\"{a}\" \"{b}\"");
		}
	}
}
