using HarmonyLib;
using OniAccess.Handlers;
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
	///
	/// Show/OnShow patches: Some screens call Show(false) during prefab init, which means
	/// KScreen.Activate/Deactivate hooks don't fire for user-visible show/hide transitions.
	/// These patches dispatch to ContextDetector via DispatchShowEvent instead.
	/// Whether to patch Show or OnShow depends on the screen — KModalScreen subclasses
	/// that override Show use Show; screens that only override OnShow use OnShow.
	/// </summary>

	/// <summary>
	/// Shared dispatch for Show/OnShow postfixes. Pushes or pops the handler
	/// via ContextDetector based on the show flag.
	/// </summary>
	static class ShowDispatch {
		internal static void Handle(KScreen instance, bool show) {
			if (!ModToggle.IsEnabled) return;
			if (show)
				ContextDetector.OnScreenActivated(instance);
			else
				ContextDetector.OnScreenDeactivating(instance);
		}
	}

	/// <summary>
	/// Detect screen activations for context-aware handler switching.
	/// Postfix: fires after KScreen.Activate completes (screen is now on the stack).
	/// </summary>
	[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
	internal static class KScreen_Activate_Patch {
		private static void Postfix(KScreen __instance) {
			if (!ModToggle.IsEnabled) return;
			// Skip screens managed via Show patches -- their OnActivate calls Show(false)
			// during prefab init, so this postfix would push a zombie handler.
			if (ContextDetector.IsShowPatched(__instance.GetType())) return;
			ContextDetector.OnScreenActivated(__instance);
		}
	}

	/// <summary>
	/// Detect screen deactivations for context-aware handler switching.
	/// Prefix: fires BEFORE KScreen.Deactivate because Deactivate calls PopScreen then Destroy.
	/// </summary>
	[HarmonyPatch(typeof(KScreen), nameof(KScreen.Deactivate))]
	internal static class KScreen_Deactivate_Patch {
		private static void Prefix(KScreen __instance) {
			if (!ModToggle.IsEnabled) return;
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
			try {
				var panelRoot = Traverse.Create(__instance).Field<GameObject>("panelRoot").Value;
				if (panelRoot == null) {
					Log.Debug("KModalButtonMenu.Unhide skipped: panelRoot is null (screen already destroyed)");
					return false;
				}
			} catch (System.Exception ex) {
				// Traverse failed (field renamed?). Fall through to let original Unhide run.
				Log.Warn($"KModalButtonMenu_Unhide_Patch: Traverse failed, skipping guard: {ex.Message}");
			}
			return true;
		}
	}

	/// Patch Show — OnActivate calls Show(false) during prefab init.
	[HarmonyPatch(typeof(LockerMenuScreen), nameof(LockerMenuScreen.Show))]
	internal static class LockerMenuScreen_Show_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// Same pattern as LockerMenuScreen.
	[HarmonyPatch(typeof(KleiItemDropScreen), nameof(KleiItemDropScreen.Show))]
	internal static class KleiItemDropScreen_Show_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// Patch OnShow — PauseScreen overrides OnShow, not Show.
	[HarmonyPatch(typeof(PauseScreen), "OnShow")]
	internal static class PauseScreen_Show_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// Same pattern as PauseScreen.
	[HarmonyPatch(typeof(VideoScreen), "OnShow")]
	internal static class VideoScreen_OnShow_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// <summary>
	/// TableScreen subclasses (JobsTableScreen, ConsumablesTableScreen) extend
	/// ShowOptimizedKScreen, which hides via canvas alpha in Show(false) without
	/// calling Deactivate. ManagementMenu toggles them via Show(). Patch
	/// TableScreen.OnShow and let ContextDetector filter by registration.
	/// </summary>
	[HarmonyPatch(typeof(TableScreen), "OnShow")]
	internal static class TableScreen_OnShow_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// <summary>
	/// DetailsScreen.OnPrefabInit() calls Show(false) during init, so
	/// KScreen.Activate/Deactivate patches do not fire for user-visible show/hide.
	/// Push-only: handler removal is handled by OnCmpDisable below.
	/// </summary>
	[HarmonyPatch(typeof(DetailsScreen), "OnShow")]
	internal static class DetailsScreen_OnShow_Patch {
		private static void Postfix(DetailsScreen __instance, bool show) {
			if (!ModToggle.IsEnabled) return;
			if (show) {
				ContextDetector.OnScreenActivated(__instance);
			}
		}
	}

	/// <summary>
	/// RootMenu.CloseSubMenus() hides DetailsScreen via gameObject.SetActive(false),
	/// bypassing Show(false) entirely. OnCmpDisable fires for both SetActive(false)
	/// and Show(false), so it catches all hiding paths.
	/// </summary>
	[HarmonyPatch(typeof(DetailsScreen), "OnCmpDisable")]
	internal static class DetailsScreen_OnCmpDisable_Patch {
		private static void Postfix(DetailsScreen __instance) {
			if (!ModToggle.IsEnabled) return;
			ContextDetector.OnScreenDeactivating(__instance);
		}
	}

	/// <summary>
	/// MinionSelectScreen.OnSpawn() does not call base.OnSpawn(), so
	/// KScreen.Activate() is never invoked. Patch OnSpawn directly.
	/// </summary>
	[HarmonyPatch(typeof(MinionSelectScreen), "OnSpawn")]
	internal static class MinionSelectScreen_OnSpawn_Patch {
		private static void Postfix(MinionSelectScreen __instance) {
			if (!ModToggle.IsEnabled) return;
			ContextDetector.OnScreenActivated(__instance);
		}
	}

	/// <summary>
	/// ResearchScreen extends KModalScreen whose OnActivate calls OnShow(true)
	/// during prefab init. Patch Show (not OnShow) to avoid that init path.
	/// </summary>
	[HarmonyPatch(typeof(ResearchScreen), nameof(ResearchScreen.Show))]
	internal static class ResearchScreen_Show_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// Patch OnShow — SkillsScreen overrides OnShow, not Show.
	[HarmonyPatch(typeof(SkillsScreen), "OnShow")]
	internal static class SkillsScreen_OnShow_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// ScheduleScreen extends KScreen (not KModalScreen). ManagementMenu toggles
	/// it via Show(), which calls OnShow(), without going through Activate/Deactivate.
	[HarmonyPatch(typeof(ScheduleScreen), "OnShow")]
	internal static class ScheduleScreen_OnShow_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}

	/// <summary>
	/// RetiredColonyInfoScreen reuses its instance via Show(true) on subsequent opens,
	/// so KScreen.Activate never fires again. The duplicate guard in OnScreenActivated
	/// prevents double-pushing when both Activate and Show(true) fire on first open.
	/// </summary>
	[HarmonyPatch(typeof(RetiredColonyInfoScreen), nameof(RetiredColonyInfoScreen.Show))]
	internal static class RetiredColonyInfoScreen_Show_Patch {
		private static void Postfix(KScreen __instance, bool show) =>
			ShowDispatch.Handle(__instance, show);
	}
}
