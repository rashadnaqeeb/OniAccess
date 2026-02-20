using UnityEngine;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Entity classification for the Life category.
	/// Determines whether critters are wild/tame and plants are wild/farmed.
	/// </summary>
	public static class LifeRouter {
		public static bool IsWild(GameObject go) {
			return go.GetComponent<KPrefabID>().HasTag(GameTags.Creatures.Wild);
		}

		/// <summary>
		/// A plant is farmed if it sits on a FarmTile, PlanterBox, or HydroponicFarm.
		/// </summary>
		public static bool IsFarmPlant(Uprootable plant) {
			int cell = Grid.PosToCell(plant.transform.GetPosition());
			var tileGo = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
			if (tileGo == null) return false;
			string prefabId = tileGo.GetComponent<Building>().Def.PrefabID;
			return prefabId == "FarmTile"
				|| prefabId == "PlanterBox"
				|| prefabId == "HydroponicFarm";
		}
	}
}
