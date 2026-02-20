using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProcGen;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Resolves SubWorld.ZoneType enum values to localized display names.
	/// Built once from SettingsCache.subworlds at first use. Strips the
	/// trailing " Biome" suffix which is redundant in the Biomes subcategory.
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
			foreach (var kvp in SettingsCache.subworlds) {
				var subWorld = kvp.Value;
				if (_names.ContainsKey(subWorld.zoneType))
					continue;
				string localized = Strings.Get(subWorld.nameKey);
				if (localized.EndsWith(" Biome"))
					localized = localized.Substring(0, localized.Length - 6);
				_names[subWorld.zoneType] = localized;
			}
		}

		private static string InsertSpaces(string camelCase) {
			return Regex.Replace(camelCase, "(\\B[A-Z])", " $1");
		}
	}
}
