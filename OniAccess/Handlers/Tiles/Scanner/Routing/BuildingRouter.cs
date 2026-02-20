using System.Collections.Generic;
using OniAccess.Util;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Routes non-tile, non-utility buildings to scanner category/subcategory.
	/// Built once from TUNING.BUILDINGS.PLANORDER at construction time.
	///
	/// Two-layer mapping:
	///   Layer 1: prefab ID → (game category, game subcategory)
	///   Layer 2: (game category, game subcategory) → (scanner category, scanner subcategory)
	///
	/// Prefab override table applied first for known routing exceptions.
	/// </summary>
	public class BuildingRouter {
		private readonly Dictionary<string, (string category, string subcategory)> _prefabToScanner;

		private static readonly Dictionary<string, (string, string)> _prefabOverrides =
			new Dictionary<string, (string, string)> {
				{ "HighEnergyParticleRedirector", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ "HEPBridgeTile", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ "Headquarters", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ "ResetSkillsStation", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
				{ "RoleStation", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
			};

		// Whole-category mappings: any building in these game categories routes here
		private static readonly Dictionary<string, (string, string)> _wholeCategoryMap =
			new Dictionary<string, (string, string)> {
				{ "Oxygen", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Oxygen) },
				{ "Refining", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Refining) },
				{ "Medical", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Wellness) },
				{ "Rocketry", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ "HEP", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
			};

		// Subcategory-level mappings: (game category, game subcategory) → scanner destination
		private static readonly Dictionary<(string, string), (string, string)> _subcategoryMap =
			new Dictionary<(string, string), (string, string)> {
				// Buildings > Generators
				{ ("Power", "generators"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Generators) },
				{ ("Power", "electrobankbuildings"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Generators) },
				{ ("Equipment", "industrialstation"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Generators) },

				// Buildings > Farming
				{ ("Food", "farming"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Farming) },
				{ ("Food", "ranching"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Farming) },
				{ ("Equipment", "workstations"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Farming) },

				// Buildings > Production
				{ ("Food", "cooking"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
				{ ("Equipment", "research"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
				{ ("Equipment", "manufacturing"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
				{ ("Equipment", "archaeology"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },
				{ ("Equipment", "meteordefense"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Production) },

				// Buildings > Storage
				{ ("Base", "storage"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Storage) },
				{ ("Food", "storage"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Storage) },

				// Buildings > Temperature
				{ ("Utilities", "temperature"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Temperature) },

				// Buildings > Refining
				{ ("Utilities", "oil"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Refining) },

				// Buildings > Wellness
				{ ("Plumbing", "washroom"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Wellness) },
				{ ("Furniture", "beds"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Wellness) },

				// Buildings > Morale
				{ ("Furniture", "lights"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Morale) },
				{ ("Furniture", "dining"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Morale) },
				{ ("Furniture", "recreation"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Morale) },
				{ ("Furniture", "decor"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Morale) },

				// Buildings > Infrastructure
				{ ("Base", "doors"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ ("Base", "printingpods"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ ("Base", "operations"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ ("Utilities", "sanitation"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ ("Equipment", "equipment"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },

				// Buildings > Rocketry (subcategory-level additions)
				{ ("Equipment", "exploration"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ ("Equipment", "telescopes"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ ("Plumbing", "buildmenuports"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ ("HVAC", "buildmenuports"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },
				{ ("Conveyance", "buildmenuports"), (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Rocketry) },

				// Networks > Transport
				{ ("Base", "transport"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Transport) },

				// Networks > Power
				{ ("Power", "batteries"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Power) },
				{ ("Power", "powercontrol"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Power) },
				{ ("Power", "switches"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Power) },

				// Networks > Liquid
				{ ("Plumbing", "pumps"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Liquid) },
				{ ("Plumbing", "pipes"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Liquid) },
				{ ("Plumbing", "valves"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Liquid) },
				{ ("Plumbing", "sensors"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Liquid) },

				// Networks > Gas
				{ ("HVAC", "pumps"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Gas) },
				{ ("HVAC", "pipes"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Gas) },
				{ ("HVAC", "valves"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Gas) },
				{ ("HVAC", "sensors"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Gas) },

				// Networks > Conveyor
				{ ("Conveyance", "conveyancestructures"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Conveyor) },
				{ ("Conveyance", "automated"), (ScannerTaxonomy.Categories.Networks, ScannerTaxonomy.Subcategories.Conveyor) },

				// Automation > Sensors
				{ ("Automation", "sensors"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Sensors) },

				// Automation > Gates
				{ ("Automation", "logicgates"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Gates) },

				// Automation > Controls
				{ ("Automation", "switches"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Controls) },
				{ ("Automation", "logicmanager"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Controls) },
				{ ("Automation", "logicaudio"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Controls) },
				{ ("Automation", "transmissions"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Controls) },

				// Automation > Wires
				{ ("Automation", "wires"), (ScannerTaxonomy.Categories.Automation, ScannerTaxonomy.Subcategories.Wires) },
			};

		public BuildingRouter() {
			_prefabToScanner = new Dictionary<string, (string, string)>();
			BuildMap();
		}

		/// <summary>
		/// Returns (category, subcategory) for the given building prefab ID.
		/// Returns null values if the building is unmapped.
		/// </summary>
		public (string category, string subcategory) Route(string prefabId) {
			if (_prefabToScanner.TryGetValue(prefabId, out var dest))
				return dest;
			return (null, null);
		}

		private void BuildMap() {
			foreach (var planInfo in TUNING.BUILDINGS.PLANORDER) {
				string gameCategoryStr = HashCache.Get().Get(planInfo.category);

				foreach (var kvp in planInfo.buildingAndSubcategoryData) {
					string buildingPrefabId = kvp.Key;
					string gameSubcategory = kvp.Value;

					if (_prefabOverrides.TryGetValue(buildingPrefabId, out var overrideDest)) {
						_prefabToScanner[buildingPrefabId] = overrideDest;
						continue;
					}

					if (_prefabToScanner.ContainsKey(buildingPrefabId))
						continue;

					if (_wholeCategoryMap.TryGetValue(gameCategoryStr, out var wholeDest)) {
						_prefabToScanner[buildingPrefabId] = wholeDest;
						continue;
					}

					if (_subcategoryMap.TryGetValue((gameCategoryStr, gameSubcategory), out var subDest)) {
						_prefabToScanner[buildingPrefabId] = subDest;
						continue;
					}

					Log.Warn($"BuildingRouter: unmapped building '{buildingPrefabId}' " +
						$"in game category '{gameCategoryStr}' > '{gameSubcategory}'");
				}
			}

			foreach (var kvp in _prefabOverrides) {
				if (!_prefabToScanner.ContainsKey(kvp.Key))
					_prefabToScanner[kvp.Key] = kvp.Value;
			}
		}
	}
}
