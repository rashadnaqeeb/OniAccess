using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProcGen;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Resolves SubWorld.ZoneType enum values to localized display names.
	/// Built once from SettingsCache.subworlds at first use. Strips the
	/// surrounding text from BIOME_NAME (e.g. " Biome" suffix in English)
	/// which is redundant in the Biomes subcategory.
	/// </summary>
	public class BiomeNameResolver {
		private Dictionary<SubWorld.ZoneType, string> _names;

		public string GetName(SubWorld.ZoneType zoneType) {
			if (_names == null)
				Build();
			if (_names.TryGetValue(zoneType, out string name))
				return name;
			return InsertSpaces(zoneType.ToString());
		}

		private void Build() {
			_names = new Dictionary<SubWorld.ZoneType, string>();
			if (SettingsCache.subworlds == null) return;
			foreach (var kvp in SettingsCache.subworlds) {
				var subWorld = kvp.Value;
				if (subWorld == null) continue;
				if (_names.ContainsKey(subWorld.zoneType))
					continue;
				if (subWorld.nameKey == null) continue;
				string localized = Strings.Get(subWorld.nameKey);
				string[] parts = ((string)STRINGS.ONIACCESS.SCANNER.BIOME_NAME).Split(
					new[] { "{0}" }, System.StringSplitOptions.None);
				string prefix = parts.Length > 0 ? parts[0] : "";
				string suffix = parts.Length > 1 ? parts[1] : "";
				if (localized != null) {
					if (prefix.Length > 0 && localized.StartsWith(prefix))
						localized = localized.Substring(prefix.Length);
					if (suffix.Length > 0 && localized.EndsWith(suffix))
						localized = localized.Substring(0, localized.Length - suffix.Length);
				}
				_names[subWorld.zoneType] = localized ?? InsertSpaces(subWorld.zoneType.ToString());
			}
		}

		private static string InsertSpaces(string camelCase) {
			return Regex.Replace(camelCase, "(\\B[A-Z])", " $1");
		}
	}
}
