using System.Text.RegularExpressions;

namespace OniAccess.Handlers {
	/// <summary>
	/// Simple data class for help list entries.
	/// Each handler provides its own list of these via IAccessHandler.HelpEntries.
	/// Displayed in the ? navigable help list.
	/// Key names are written for Windows (Ctrl, Alt). On macOS the mod reads
	/// Option where it says Ctrl and Command where it says Alt (see InputUtil),
	/// so the spoken key name is rewritten to what the player actually presses.
	/// </summary>
	public sealed class HelpEntry {
		public string KeyName { get; }
		public string Description { get; }

		/// <summary>
		/// Platform check. Defaults to InputUtil.IsMac; tests replace it because
		/// InputUtil reads UnityEngine.SystemInfo, which needs the Unity runtime.
		/// </summary>
		internal static System.Func<bool> IsMacSource = () => Input.InputUtil.IsMac;

		public HelpEntry(string keyName, string description) {
			KeyName = IsMacSource() ? ToMacKeyName(keyName) : keyName;
			Description = description;
		}

		private static string ToMacKeyName(string keyName) {
			keyName = Regex.Replace(keyName, @"\bCtrl\b", (string)STRINGS.ONIACCESS.HELP.MAC_OPTION);
			return Regex.Replace(keyName, @"\bAlt\b", (string)STRINGS.ONIACCESS.HELP.MAC_COMMAND);
		}

		public override string ToString() => $"{KeyName}: {Description}";
	}
}
