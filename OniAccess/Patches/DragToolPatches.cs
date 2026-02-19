using HarmonyLib;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patches for DragTool sound suppression.
	/// ToolHandler sets SuppressConfirmSound before submitting rectangles so intermediate
	/// confirm sounds are silenced; only the final rectangle plays the game's native sound.
	/// </summary>
	internal static class DragToolPatches {
		internal static bool SuppressConfirmSound;
	}

	/// <summary>
	/// Suppress the confirm sound when ToolHandler is submitting multiple rectangles.
	/// DragTool.GetConfirmSound() is virtual and returns "Tile_Confirm" by default.
	/// KMonoBehaviour.PlaySound safely no-ops on null, so returning null silences it.
	/// </summary>
	[HarmonyPatch(typeof(DragTool), "GetConfirmSound")]
	internal static class DragTool_GetConfirmSound_Patch {
		private static bool Prefix(ref string __result) {
			if (!DragToolPatches.SuppressConfirmSound) return true;
			__result = null;
			return false;
		}
	}
}
