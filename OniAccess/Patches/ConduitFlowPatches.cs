using HarmonyLib;
using OniAccess.ConduitTracking;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(Game), "OnPrefabInit")]
	internal static class Game_OnPrefabInit_FlowTracker_Patch {
		private static void Postfix() {
			FlowTracker.Initialize();
			Game.Instance.gasConduitFlow.onConduitsRebuilt +=
				FlowTracker.Gas.Clear;
			Game.Instance.liquidConduitFlow.onConduitsRebuilt +=
				FlowTracker.Liquid.Clear;
			Game.Instance.solidConduitFlow.onConduitsRebuilt +=
				FlowTracker.Solid.Clear;
		}
	}

	[HarmonyPatch(typeof(ConduitFlow), nameof(ConduitFlow.Sim200ms))]
	internal static class ConduitFlow_Sim200ms_Patch {
		private static void Prefix(ConduitFlow __instance, float dt,
				ref bool __state) {
			float elapsed = Traverse.Create(__instance)
				.Field<float>("elapsedTime").Value;
			__state = dt > 0f && elapsed + dt >= 1f;
		}

		private static void Postfix(ConduitFlow __instance, bool __state) {
			if (!__state) return;
			var tracker = __instance.conduitType == ConduitType.Gas
				? FlowTracker.Gas : FlowTracker.Liquid;
			tracker.RecordFluid(__instance);
		}
	}

	[HarmonyPatch(typeof(SolidConduitFlow),
		nameof(SolidConduitFlow.Sim200ms))]
	internal static class SolidConduitFlow_Sim200ms_Patch {
		private static void Prefix(SolidConduitFlow __instance, float dt,
				ref bool __state) {
			float elapsed = Traverse.Create(__instance)
				.Field<float>("elapsedTime").Value;
			__state = dt > 0f && elapsed + dt >= 1f;
		}

		private static void Postfix(SolidConduitFlow __instance,
				bool __state) {
			if (!__state) return;
			FlowTracker.Solid.RecordSolid(__instance);
		}
	}
}
