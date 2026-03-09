using HarmonyLib;
using OniAccess.Handlers.Sandbox;
using OniAccess.Input;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patches for sandbox mode accessibility:
	/// - Announces sandbox toggle via Game.SandboxModeActive setter
	/// - Triggers parameter menu rediscovery on RefreshDisplay
	/// </summary>

	/// <summary>
	/// Announce sandbox tools on/off when the player toggles with Shift+S.
	/// Game.SandboxModeActive is a property whose setter fires trigger -1948169901.
	/// We patch the setter to speak the new state.
	/// </summary>
	[HarmonyPatch(typeof(Game), nameof(Game.SandboxModeActive), MethodType.Setter)]
	internal static class Game_SandboxModeActive_Patch {
		private static void Postfix(Game __instance) {
			if (!ModToggle.IsEnabled) return;
			string announcement = __instance.SandboxModeActive
				? (string)STRINGS.ONIACCESS.SANDBOX.TOOLS_ON
				: (string)STRINGS.ONIACCESS.SANDBOX.TOOLS_OFF;
			Speech.SpeechPipeline.SpeakInterrupt(announcement);
		}
	}

	/// <summary>
	/// Trigger widget rediscovery in SandboxParamMenuHandler when the
	/// parameter menu refreshes its visible rows (on tool change).
	/// </summary>
	[HarmonyPatch(typeof(SandboxToolParameterMenu), nameof(SandboxToolParameterMenu.RefreshDisplay))]
	internal static class SandboxToolParameterMenu_RefreshDisplay_Patch {
		private static void Postfix() {
			if (!ModToggle.IsEnabled) return;
			SandboxParamMenuHandler.OnRefreshDisplay();
		}
	}
}
