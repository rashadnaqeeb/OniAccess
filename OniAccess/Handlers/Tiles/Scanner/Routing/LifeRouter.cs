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

		public static bool IsFarmPlant(Uprootable plant) {
			return plant.GetPlanterStorage != null;
		}
	}
}
