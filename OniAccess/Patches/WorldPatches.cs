using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patch on both public ActiveWorldStarWipe overloads to announce
	/// the destination world name on any world switch (hotkeys, world list,
	/// starmap clicks).
	/// </summary>
	[HarmonyPatch]
	internal static class CameraController_ActiveWorldStarWipe_Patch {
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(CameraController),
				"ActiveWorldStarWipe",
				new[] { typeof(int), typeof(System.Action) });
			yield return AccessTools.Method(typeof(CameraController),
				"ActiveWorldStarWipe",
				new[] { typeof(int), typeof(UnityEngine.Vector3), typeof(float), typeof(System.Action) });
		}

		private static void Postfix(int id) {
			if (!ModToggle.IsEnabled) return;
			if (ClusterManager.Instance.activeWorldId == id) return;
			try {
				var world = ClusterManager.Instance.GetWorld(id);
				if (world == null) return;
				string name = world.GetComponent<ClusterGridEntity>().Name;
				SpeechPipeline.SpeakInterrupt(name);
			} catch (System.Exception ex) {
				Log.Warn($"CameraController_ActiveWorldStarWipe_Patch: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Harmony patch on WorldSelector.TriggerVisualNotification to announce
	/// diagnostic degradation on non-active worlds. WorldSelector only exists
	/// in Spaced Out DLC; Harmony skips this patch if the type is absent.
	/// </summary>
	[HarmonyPatch(typeof(WorldSelector), "TriggerVisualNotification")]
	internal static class WorldSelector_TriggerVisualNotification_Patch {
		private static void Postfix(int worldID, ColonyDiagnostic.DiagnosticResult.Opinion result) {
			if (!ModToggle.IsEnabled) return;
			if (!LoadGate.IsReady) return;
			try {
				var world = ClusterManager.Instance.GetWorld(worldID);
				if (world == null) return;
				string name = world.GetComponent<ClusterGridEntity>().Name;
				string severity = Handlers.Tiles.TileCursorHandler.OpinionWord(result);
				string status = world.GetStatus();
				string speech = string.IsNullOrEmpty(status)
					? $"{name}: {severity}"
					: $"{name}: {status}, {severity}";
				SpeechPipeline.SpeakInterrupt(speech);
			} catch (System.Exception ex) {
				Log.Warn($"WorldSelector_TriggerVisualNotification_Patch: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Harmony patch on WorldContainer.SetDiscovered to announce newly
	/// discovered worlds. Prefix captures the pre-call state; postfix
	/// announces only on false-to-true transitions. Spaced Out DLC only.
	/// </summary>
	[HarmonyPatch(typeof(WorldContainer), "SetDiscovered")]
	internal static class WorldContainer_SetDiscovered_Patch {
		private static bool Prefix(WorldContainer __instance, out bool __state) {
			__state = __instance.IsDiscovered;
			return true;
		}

		private static void Postfix(WorldContainer __instance, bool __state) {
			if (!ModToggle.IsEnabled) return;
			if (!LoadGate.IsReady) return;
			if (__state) return;
			if (!DlcManager.FeatureClusterSpaceEnabled()) return;
			try {
				string name = __instance.GetComponent<ClusterGridEntity>().Name;
				SpeechPipeline.SpeakInterrupt(
					string.Format((string)STRINGS.ONIACCESS.WORLD_SELECTOR.DISCOVERED, name));
			} catch (System.Exception ex) {
				Log.Warn($"WorldContainer_SetDiscovered_Patch: {ex.Message}");
			}
		}
	}
}
