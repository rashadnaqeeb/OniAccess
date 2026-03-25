using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProcGen;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Resolves SubWorld.ZoneType enum values to localized display names.
	/// Looks up STRINGS.SUBWORLDS.{KEY}.NAME directly so names are always
	/// localized regardless of which subworlds SettingsCache has loaded.
	/// Strips the surrounding text from BIOME_NAME (e.g. " Biome" suffix
	/// in English) which is redundant in the Biomes subcategory.
	/// </summary>
	public class BiomeNameResolver {
		private Dictionary<SubWorld.ZoneType, string> _names;

		private static readonly Dictionary<SubWorld.ZoneType, string> StringKeys
			= new Dictionary<SubWorld.ZoneType, string> {
			{ SubWorld.ZoneType.FrozenWastes,        "FROZEN" },
			{ SubWorld.ZoneType.BoggyMarsh,          "MARSH" },
			{ SubWorld.ZoneType.Sandstone,           "SANDSTONE" },
			{ SubWorld.ZoneType.ToxicJungle,         "JUNGLE" },
			{ SubWorld.ZoneType.MagmaCore,           "MAGMA" },
			{ SubWorld.ZoneType.OilField,            "OIL" },
			{ SubWorld.ZoneType.Space,               "SPACE" },
			{ SubWorld.ZoneType.Ocean,               "OCEAN" },
			{ SubWorld.ZoneType.Rust,                "RUST" },
			{ SubWorld.ZoneType.Forest,              "FOREST" },
			{ SubWorld.ZoneType.Radioactive,         "RADIOACTIVE" },
			{ SubWorld.ZoneType.Swamp,               "SWAMP" },
			{ SubWorld.ZoneType.Wasteland,           "WASTELAND" },
			{ SubWorld.ZoneType.Metallic,            "METALLIC" },
			{ SubWorld.ZoneType.Barren,              "BARREN" },
			{ SubWorld.ZoneType.Moo,                 "MOO" },
			{ SubWorld.ZoneType.IceCaves,            "ICECAVES" },
			{ SubWorld.ZoneType.CarrotQuarry,        "CARROTQUARRY" },
			{ SubWorld.ZoneType.SugarWoods,          "SUGARWOODS" },
			{ SubWorld.ZoneType.PrehistoricGarden,   "GARDEN" },
			{ SubWorld.ZoneType.PrehistoricRaptor,   "RAPTOR" },
			{ SubWorld.ZoneType.PrehistoricWetlands, "WETLANDS" },
		};

		public string GetName(SubWorld.ZoneType zoneType) {
			if (_names == null)
				Build();
			if (_names.TryGetValue(zoneType, out string name))
				return name;
			return InsertSpaces(zoneType.ToString());
		}

		private void Build() {
			_names = new Dictionary<SubWorld.ZoneType, string>();
			string[] parts = ((string)STRINGS.ONIACCESS.SCANNER.BIOME_NAME).Split(
				new[] { "{0}" }, System.StringSplitOptions.None);
			string prefix = parts.Length > 0 ? parts[0] : "";
			string suffix = parts.Length > 1 ? parts[1] : "";
			foreach (var kvp in StringKeys) {
				string localized = Strings.Get(
					"STRINGS.SUBWORLDS." + kvp.Value + ".NAME");
				if (localized == null) continue;
				if (prefix.Length > 0 && localized.StartsWith(prefix))
					localized = localized.Substring(prefix.Length);
				if (suffix.Length > 0 && localized.EndsWith(suffix))
					localized = localized.Substring(0, localized.Length - suffix.Length);
				_names[kvp.Key] = localized;
			}
		}

		private static string InsertSpaces(string camelCase) {
			return Regex.Replace(camelCase, "(\\B[A-Z])", " $1");
		}
	}
}
