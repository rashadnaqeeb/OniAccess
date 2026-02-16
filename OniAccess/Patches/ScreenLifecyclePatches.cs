using HarmonyLib;
using OniAccess.Handlers;
using OniAccess.Toggle;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patches for KScreen lifecycle events (activate, deactivate, show/hide).
	/// These feed screen transitions into ContextDetector for handler switching.
	///
	/// KScreen_Activate_Patch: Fires context detection when screens open.
	/// KScreen_Deactivate_Patch: Fires context detection when screens close (Prefix because
	/// Deactivate calls PopScreen then Destroy).
	/// </summary>

	/// <summary>
	/// Detect screen activations for context-aware handler switching.
	/// Postfix: fires after KScreen.Activate completes (screen is now on the stack).
	/// </summary>
	[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
	internal static class KScreen_Activate_Patch {
		private static void Postfix(KScreen __instance) {
			if (!VanillaMode.IsEnabled) return;
			// Skip screens managed via Show patches -- their OnActivate calls Show(false)
			// during prefab init, so this postfix would push a zombie handler.
			if (ContextDetector.IsShowPatched(__instance.GetType())) return;
			ContextDetector.OnScreenActivated(__instance);
		}
	}

	/// <summary>
	/// Detect screen deactivations for context-aware handler switching.
	/// Prefix: fires BEFORE KScreen.Deactivate because Deactivate calls PopScreen then Destroy.
	/// We need the screen instance before it's destroyed.
	/// </summary>
	[HarmonyPatch(typeof(KScreen), nameof(KScreen.Deactivate))]
	internal static class KScreen_Deactivate_Patch {
		private static void Prefix(KScreen __instance) {
			if (!VanillaMode.IsEnabled) return;
			// Skip screens managed via Show patches -- lifecycle handled there.
			if (ContextDetector.IsShowPatched(__instance.GetType())) return;
			ContextDetector.OnScreenDeactivating(__instance);
		}
	}

	/// <summary>
	/// Guard against NullReferenceException in KModalButtonMenu.Unhide.
	/// When a child screen's Close event fires after the parent is already destroyed,
	/// panelRoot is null and the original Unhide crashes. Skip the call if panelRoot is null.
	/// </summary>
	[HarmonyPatch(typeof(KModalButtonMenu), "Unhide")]
	internal static class KModalButtonMenu_Unhide_Patch {
		private static bool Prefix(KModalButtonMenu __instance) {
			var panelRoot = Traverse.Create(__instance).Field<GameObject>("panelRoot").Value;
			if (panelRoot == null) {
				Log.Debug("KModalButtonMenu.Unhide skipped: panelRoot is null (screen already destroyed)");
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// LockerMenuScreen.OnActivate() immediately calls Show(false) during prefab init,
	/// so KScreen.Activate/Deactivate hooks don't fire for user-visible show/hide.
	/// Patch Show(bool) to push/pop the handler via ContextDetector instead.
	/// </summary>
	[HarmonyPatch(typeof(LockerMenuScreen), nameof(LockerMenuScreen.Show))]
	internal static class LockerMenuScreen_Show_Patch {
		private static void Postfix(LockerMenuScreen __instance, bool show) {
			if (!VanillaMode.IsEnabled) return;
			if (show) {
				ContextDetector.OnScreenActivated(__instance);
			} else {
				ContextDetector.OnScreenDeactivating(__instance);
			}
		}
	}

	/// <summary>
	/// KleiItemDropScreen.OnActivate() calls Show(false) during prefab init (same as LockerMenuScreen).
	/// Patch Show(bool) to push/pop the handler via ContextDetector instead.
	/// </summary>
	[HarmonyPatch(typeof(KleiItemDropScreen), nameof(KleiItemDropScreen.Show))]
	internal static class KleiItemDropScreen_Show_Patch {
		private static void Postfix(KleiItemDropScreen __instance, bool show) {
			if (!VanillaMode.IsEnabled) return;
			if (show) {
				ContextDetector.OnScreenActivated(__instance);
			} else {
				ContextDetector.OnScreenDeactivating(__instance);
			}
		}
	}

	/// <summary>
	/// PauseScreen.OnActivate() calls Show(false) during prefab init (same as LockerMenuScreen).
	/// Patch Show(bool) to push/pop the handler via ContextDetector instead.
	/// </summary>
	[HarmonyPatch(typeof(PauseScreen), nameof(PauseScreen.Show))]
	internal static class PauseScreen_Show_Patch {
		private static void Postfix(PauseScreen __instance, bool show) {
			if (!VanillaMode.IsEnabled) return;
			if (show) {
				ContextDetector.OnScreenActivated(__instance);
			} else {
				ContextDetector.OnScreenDeactivating(__instance);
			}
		}
	}

	/// <summary>
	/// MinionSelectScreen.OnSpawn() does not call base.OnSpawn(), so
	/// KScreen.Activate() is never invoked and our generic KScreen_Activate_Patch
	/// never fires. Patch OnSpawn directly to trigger handler activation.
	/// </summary>
	[HarmonyPatch(typeof(MinionSelectScreen), "OnSpawn")]
	internal static class MinionSelectScreen_OnSpawn_Patch {
		private static void Postfix(MinionSelectScreen __instance) {
			if (!VanillaMode.IsEnabled) return;
			ContextDetector.OnScreenActivated(__instance);
		}
	}
}
