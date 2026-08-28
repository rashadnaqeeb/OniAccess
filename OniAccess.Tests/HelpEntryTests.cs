using System.Collections.Generic;

using OniAccess.Handlers;

namespace OniAccess.Tests {
	/// <summary>
	/// Offline tests for the macOS help key-name rewrite. Key names are authored
	/// for Windows; on Mac the spoken name must say the key the player presses.
	/// </summary>
	static class HelpEntryTests {
		private static (string, bool, string) Check(string name, bool mac, string input, string expected) {
			HelpEntry.IsMacSource = () => mac;
			try {
				string actual = new HelpEntry(input, "").KeyName;
				bool ok = actual == expected;
				return (name, ok, ok ? "OK" : $"expected \"{expected}\", got \"{actual}\"");
			} finally {
				HelpEntry.IsMacSource = () => false;
			}
		}

		public static IEnumerable<(string, bool, string)> All() {
			// Every occurrence is rewritten, not just the first
			yield return Check("MacKeyNameRewritesEveryCtrl", true, "Ctrl+Tab/Ctrl+Shift+Tab", "Option+Tab/Option+Shift+Tab");
			yield return Check("MacKeyNameRewritesAlt", true, "Alt+H", "Command+H");
			// Word boundary holds before a non-letter key
			yield return Check("MacKeyNameRewritesBeforeSymbol", true, "Ctrl+\\", "Option+\\");
			yield return Check("MacKeyNameLeavesShiftAlone", true, "Shift+Up/Down", "Shift+Up/Down");
			yield return Check("WindowsKeyNameUnchanged", false, "Ctrl+Tab", "Ctrl+Tab");
		}
	}
}
