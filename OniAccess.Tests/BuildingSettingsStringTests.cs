using System;
using System.Collections.Generic;

namespace OniAccess.Tests {
	/// <summary>
	/// Offline guard on the building-settings readout strings. Every one of these is
	/// fed to string.Format at speech time with a fixed argument count from
	/// BuildingSettings. A placeholder the call site does not supply throws
	/// FormatException inside the tile readout, and one placeholder too few silently
	/// drops a value the player is relying on. Neither is visible in game, so the
	/// arity is pinned here instead.
	/// </summary>
	static class BuildingSettingsStringTests {
		private static (string, bool, string) Check(string name, bool ok, string detail)
			=> (name, ok, ok ? "OK" : detail);

		// Format strings paired with the argument count BuildingSettings passes.
		private static readonly (string Name, string Format, int Args)[] Formats = {
			("SETTING_THRESHOLD", (string)STRINGS.ONIACCESS.GLANCE.SETTING_THRESHOLD, 3),
			("SETTING_FILTER", (string)STRINGS.ONIACCESS.GLANCE.SETTING_FILTER, 1),
			("SETTING_ACTIVATION_RANGE", (string)STRINGS.ONIACCESS.GLANCE.SETTING_ACTIVATION_RANGE, 4),
			("SETTING_SLIDER", (string)STRINGS.ONIACCESS.GLANCE.SETTING_SLIDER, 2),
			("SETTING_FLOW", (string)STRINGS.ONIACCESS.GLANCE.SETTING_FLOW, 1),
			("SETTING_LIMIT", (string)STRINGS.ONIACCESS.GLANCE.SETTING_LIMIT, 1),
			("SETTING_CAPACITY", (string)STRINGS.ONIACCESS.GLANCE.SETTING_CAPACITY, 1),
			("SETTING_COUNT", (string)STRINGS.ONIACCESS.GLANCE.SETTING_COUNT, 1),
			("SETTING_TIMER", (string)STRINGS.ONIACCESS.GLANCE.SETTING_TIMER, 2),
			("SETTING_TIME_RANGE", (string)STRINGS.ONIACCESS.GLANCE.SETTING_TIME_RANGE, 2),
			("SETTING_BITS", (string)STRINGS.ONIACCESS.GLANCE.SETTING_BITS, 1),
			("SETTING_ALARM", (string)STRINGS.ONIACCESS.GLANCE.SETTING_ALARM, 1),
			("SETTING_CHANNEL", (string)STRINGS.ONIACCESS.GLANCE.SETTING_CHANNEL, 1),
			("SETTING_AIMED", (string)STRINGS.ONIACCESS.GLANCE.SETTING_AIMED, 1),
			("SETTING_SELECTED", (string)STRINGS.ONIACCESS.GLANCE.SETTING_SELECTED, 1),
			("SETTING_BAIT", (string)STRINGS.ONIACCESS.GLANCE.SETTING_BAIT, 1),
		};

		// Strings spoken verbatim. A stray placeholder here reaches the speech engine
		// as a literal "{0}".
		private static readonly (string Name, string Text)[] Literals = {
			("SETTING_FILTER_NONE", (string)STRINGS.ONIACCESS.GLANCE.SETTING_FILTER_NONE),
			("SETTING_COUNT_ADVANCED", (string)STRINGS.ONIACCESS.GLANCE.SETTING_COUNT_ADVANCED),
			("SETTING_CRITTERS", (string)STRINGS.ONIACCESS.GLANCE.SETTING_CRITTERS),
			("SETTING_EGGS", (string)STRINGS.ONIACCESS.GLANCE.SETTING_EGGS),
			("SETTING_CRITTERS_AND_EGGS", (string)STRINGS.ONIACCESS.GLANCE.SETTING_CRITTERS_AND_EGGS),
			("SETTING_BITS_NONE", (string)STRINGS.ONIACCESS.GLANCE.SETTING_BITS_NONE),
			("SETTING_AUTOMATION_ONLY", (string)STRINGS.ONIACCESS.GLANCE.SETTING_AUTOMATION_ONLY),
			("SETTING_SELECTED_NONE", (string)STRINGS.ONIACCESS.GLANCE.SETTING_SELECTED_NONE),
		};

		/// <summary>
		/// Formats each string with as many distinct markers as its call site supplies
		/// and requires every marker to survive into the result. Too many placeholders
		/// throws, too few drops a marker.
		/// </summary>
		private static (string, bool, string) PlaceholderArityMatchesCallSites() {
			var failures = new List<string>();

			foreach (var entry in Formats) {
				var args = new object[entry.Args];
				for (int i = 0; i < entry.Args; i++)
					args[i] = "<arg" + i + ">";

				string result;
				try {
					result = string.Format(entry.Format, args);
				} catch (FormatException ex) {
					failures.Add(entry.Name + " threw " + ex.GetType().Name);
					continue;
				}

				for (int i = 0; i < entry.Args; i++)
					if (result.IndexOf("<arg" + i + ">", StringComparison.Ordinal) < 0)
						failures.Add(entry.Name + " dropped argument " + i);

				if (result.IndexOf('{') >= 0)
					failures.Add(entry.Name + " left an unfilled placeholder: \"" + result + "\"");
			}

			return Check("SettingPlaceholderArityMatchesCallSites", failures.Count == 0,
				string.Join("; ", failures.ToArray()));
		}

		private static (string, bool, string) LiteralsHaveNoPlaceholders() {
			var failures = new List<string>();

			foreach (var entry in Literals) {
				if (string.IsNullOrEmpty(entry.Text)) {
					failures.Add(entry.Name + " is empty");
					continue;
				}
				if (entry.Text.IndexOf('{') >= 0)
					failures.Add(entry.Name + " contains a placeholder: \"" + entry.Text + "\"");
			}

			return Check("SettingLiteralsHaveNoPlaceholders", failures.Count == 0,
				string.Join("; ", failures.ToArray()));
		}

		public static IEnumerable<(string, bool, string)> All() {
			yield return PlaceholderArityMatchesCallSites();
			yield return LiteralsHaveNoPlaceholders();
		}
	}
}
