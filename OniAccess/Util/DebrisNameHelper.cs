using UnityEngine;

namespace OniAccess.Util {
	/// <summary>
	/// Shared helper for prepending "Loose" or "Bottled" to pickupable names.
	/// Used by EntityPickerHandler, DetailsScreenHandler, DebrisSection,
	/// and EntityBackend to keep prefix logic in one place.
	/// </summary>
	public static class DebrisNameHelper {
		public static string GetDisplayName(GameObject go) {
			string name = go.GetComponent<KSelectable>()?.GetName()
				?? go.GetProperName();
			var prefabId = go.GetComponent<KPrefabID>();
			if (prefabId != null && IsBottle(prefabId))
				return (string)STRINGS.ONIACCESS.SCANNER.BOTTLE_PREFIX + name;
			if (go.GetComponent<ElementChunk>() != null)
				return (string)STRINGS.ONIACCESS.SCANNER.LOOSE_PREFIX + name;
			return name;
		}

		public static bool IsBottle(KPrefabID prefabId) {
			return prefabId.HasTag(GameTags.Liquid)
				|| prefabId.HasTag(GameTags.Breathable)
				|| prefabId.HasTag(GameTags.Unbreathable);
		}
	}
}
