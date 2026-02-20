using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Scanner.Routing;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for entity-scanned categories: Buildings (non-tile, non-utility),
	/// Debris, and Life. Iterates multiple Components registries.
	/// Each entity is one ScanEntry with a GameObject reference as BackendData.
	/// </summary>
	public class EntityBackend: IScannerBackend {
		private readonly BuildingRouter _buildingRouter;

		public EntityBackend(BuildingRouter buildingRouter) {
			_buildingRouter = buildingRouter;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			foreach (var entry in ScanBuildings(worldId))
				yield return entry;
			foreach (var entry in ScanDebris(worldId))
				yield return entry;
			foreach (var entry in ScanDuplicants(worldId))
				yield return entry;
			foreach (var entry in ScanRobots(worldId))
				yield return entry;
			foreach (var entry in ScanCritters(worldId))
				yield return entry;
			foreach (var entry in ScanPlants(worldId))
				yield return entry;
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var go = (GameObject)entry.BackendData;
			if (go == null || go.IsNullOrDestroyed()) return false;
			int cell = Grid.PosToCell(go.transform.GetPosition());
			if (!Grid.IsVisible(cell)) return false;
			entry.Cell = cell;
			return true;
		}

		public string FormatName(ScanEntry entry) {
			var go = (GameObject)entry.BackendData;
			return go.GetComponent<KSelectable>()?.GetName() ?? entry.ItemName;
		}

		private IEnumerable<ScanEntry> ScanBuildings(int worldId) {
			foreach (var building in Components.BuildingCompletes.GetWorldItems(worldId)) {
				var def = building.Def;
				if (def.IsTilePiece) continue;
				if (def.isUtility) continue;

				string prefabId = def.PrefabID;
				var (category, subcategory) = _buildingRouter.Route(prefabId);
				if (category == null) continue;

				var go = building.gameObject;
				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = category,
					Subcategory = subcategory,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? prefabId,
				};
			}
		}

		private IEnumerable<ScanEntry> ScanDebris(int worldId) {
			foreach (var pickupable in Components.Pickupables.GetWorldItems(worldId)) {
				var prefabId = pickupable.GetComponent<KPrefabID>();
				if (prefabId == null) continue;
				if (DebrisRouter.ShouldExclude(prefabId)) continue;

				string subcategory = DebrisRouter.GetSubcategory(prefabId);
				var go = pickupable.gameObject;
				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = ScannerTaxonomy.Categories.Debris,
					Subcategory = subcategory,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? go.name,
				};
			}
		}

		private IEnumerable<ScanEntry> ScanDuplicants(int worldId) {
			foreach (var identity in Components.LiveMinionIdentities.GetWorldItems(worldId)) {
				var go = identity.gameObject;
				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = ScannerTaxonomy.Categories.Life,
					Subcategory = ScannerTaxonomy.Subcategories.Duplicants,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? go.name,
				};
			}
		}

		private IEnumerable<ScanEntry> ScanRobots(int worldId) {
			foreach (var brain in Components.Brains.GetWorldItems(worldId)) {
				var go = brain.gameObject;
				if (!go.GetComponent<KPrefabID>().HasTag(GameTags.Robot)) continue;

				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = ScannerTaxonomy.Categories.Life,
					Subcategory = ScannerTaxonomy.Subcategories.Robots,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? go.name,
				};
			}
		}

		private IEnumerable<ScanEntry> ScanCritters(int worldId) {
			foreach (var brain in Components.Brains.GetWorldItems(worldId)) {
				var go = brain.gameObject;
				if (go.GetComponent<CreatureBrain>() == null) continue;
				if (go.GetComponent<KPrefabID>().HasTag(GameTags.Robot)) continue;

				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				string subcategory = LifeRouter.IsWild(go)
					? ScannerTaxonomy.Subcategories.WildCritters
					: ScannerTaxonomy.Subcategories.TameCritters;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = ScannerTaxonomy.Categories.Life,
					Subcategory = subcategory,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? go.name,
				};
			}
		}

		private IEnumerable<ScanEntry> ScanPlants(int worldId) {
			foreach (var uprootable in Components.Uprootables.GetWorldItems(worldId)) {
				var go = uprootable.gameObject;
				int cell = Grid.PosToCell(go.transform.GetPosition());
				if (!Grid.IsVisible(cell)) continue;

				string subcategory = LifeRouter.IsFarmPlant(uprootable)
					? ScannerTaxonomy.Subcategories.FarmPlants
					: ScannerTaxonomy.Subcategories.WildPlants;

				yield return new ScanEntry {
					Cell = cell,
					Backend = this,
					BackendData = go,
					Category = ScannerTaxonomy.Categories.Life,
					Subcategory = subcategory,
					ItemName = go.GetComponent<KSelectable>()?.GetName() ?? go.name,
				};
			}
		}
	}
}
