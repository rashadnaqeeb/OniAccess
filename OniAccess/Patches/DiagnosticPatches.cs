using System.Reflection;
using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Patches {
	/// <summary>
	/// Harmony patch on ColonyDiagnosticScreen.DiagnosticRow.TriggerVisualNotification
	/// to announce worsening diagnostics via speech. DiagnosticRow is a private inner
	/// class, so we use TargetMethod with reflection.
	/// </summary>
	[HarmonyPatch]
	internal static class DiagnosticRow_TriggerVisualNotification_Patch {
		private static MethodBase TargetMethod() {
			var rowType = typeof(ColonyDiagnosticScreen).GetNestedType(
				"DiagnosticRow", BindingFlags.NonPublic);
			if (rowType == null) {
				Log.Error("DiagnosticRow_TriggerVisualNotification_Patch: DiagnosticRow type not found");
				return null;
			}
			var method = AccessTools.Method(rowType, "TriggerVisualNotification");
			if (method == null)
				Log.Error("DiagnosticRow_TriggerVisualNotification_Patch: TriggerVisualNotification method not found");
			return method;
		}

		private static void Postfix(object __instance) {
			if (!ModToggle.IsEnabled) return;
			if (!LoadGate.IsReady) return;
			try {
				var diagnostic = Traverse.Create(__instance)
					.Field<ColonyDiagnostic>("diagnostic").Value;
				string name = diagnostic.name;
				string message = diagnostic.LatestResult.Message;
				if (string.IsNullOrWhiteSpace(message))
					message = Handlers.Tiles.TileCursorHandler.OpinionWord(
						diagnostic.LatestResult.opinion);
				SpeechPipeline.SpeakInterrupt(name + ": " + message);
			} catch (System.Exception ex) {
				Log.Warn($"DiagnosticRow_TriggerVisualNotification_Patch: {ex.Message}");
			}
		}
	}
}
