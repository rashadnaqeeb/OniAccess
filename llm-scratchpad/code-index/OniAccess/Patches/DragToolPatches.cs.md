// Harmony patches for DragTool sound suppression.
// ToolHandler sets SuppressConfirmSound before submitting multiple rectangles so intermediate
// confirm sounds are silenced; only the final rectangle plays the game's native sound.
internal static class DragToolPatches (line 9)
  internal static bool SuppressConfirmSound (line 10)  // flag set by ToolHandler before batch submits

// Prefix patch on DragTool.GetConfirmSound().
// Returns null (silences sound) when SuppressConfirmSound is true; otherwise lets original run.
// DragTool.GetConfirmSound() is virtual and returns "Tile_Confirm" by default.
// KMonoBehaviour.PlaySound no-ops on null, so returning null is safe.
[HarmonyPatch(typeof(DragTool), "GetConfirmSound")]
internal static class DragTool_GetConfirmSound_Patch (line 19)
  private static bool Prefix(ref string __result) (line 20)
