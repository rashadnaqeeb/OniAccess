using HarmonyLib;

namespace OniAccess.Patches {
	/// <summary>
	/// PlayerControlledToggleSideScreen.RenderEveryTick polls
	/// Input.GetKeyDown(Return) every frame to toggle the switch.
	/// This conflicts with our handler's Enter key handling,
	/// causing a double-toggle that cancels itself out.
	/// Skip the method entirely when the mod is active.
	/// </summary>
	[HarmonyPatch(typeof(PlayerControlledToggleSideScreen), nameof(PlayerControlledToggleSideScreen.RenderEveryTick))]
	internal static class PlayerControlledToggleSideScreen_RenderEveryTick_Patch {
		private static bool Prefix() {
			return !ModToggle.IsEnabled;
		}
	}
}
