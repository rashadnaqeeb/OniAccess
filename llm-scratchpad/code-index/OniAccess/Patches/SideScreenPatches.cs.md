// PlayerControlledToggleSideScreen.RenderEveryTick polls Input.GetKeyDown(Return) every frame
// to toggle the switch. This conflicts with the mod handler's Enter key handling, causing
// a double-toggle that cancels itself out. The Prefix skips the method entirely when the mod is active.

[HarmonyPatch(typeof(PlayerControlledToggleSideScreen), nameof(PlayerControlledToggleSideScreen.RenderEveryTick))]
internal static class PlayerControlledToggleSideScreen_RenderEveryTick_Patch (line 12)
  // Returns false (skip original) when ModToggle.IsEnabled; returns true (run original) when mod is off.
  private static bool Prefix() (line 13)
