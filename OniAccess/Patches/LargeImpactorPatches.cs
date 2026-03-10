using System.Reflection;
using HarmonyLib;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(LargeImpactorNotificationMonitor), "CompleteSequence")]
	internal static class LargeImpactorNotificationMonitor_CompleteSequence_Patch {
		static void Postfix(LargeImpactorNotificationMonitor.Instance smi) {
			if (!ModToggle.IsEnabled) return;
			try {
				var status = smi.GetSMI<LargeImpactorStatus.Instance>();
				string cycles = GameUtil.GetFormattedCycles(status.TimeRemainingBeforeCollision);
				SpeechPipeline.SpeakInterrupt(
					string.Format((string)STRINGS.ONIACCESS.DEMOLIOR.DISCOVERED, cycles));
				LargeImpactorStatus_Instance_DealDamage_Patch._lastAnnouncedPercent = 100;
			} catch (System.Exception ex) {
				Log.Error($"LargeImpactorNotificationMonitor_CompleteSequence_Patch: {ex}");
			}
		}
	}

	[HarmonyPatch]
	internal static class LargeImpactorEvent_InitializeLandingSequence_Patch {
		static MethodBase TargetMethod() {
			var method = AccessTools.Method(typeof(LargeImpactorEvent), "InitializeLandingSequence");
			if (method == null)
				Log.Error("LargeImpactorEvent_InitializeLandingSequence_Patch: target method not found");
			return method;
		}

		static void Postfix() {
			if (!ModToggle.IsEnabled) return;
			try {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.DEMOLIOR.IMPACTED);
			} catch (System.Exception ex) {
				Log.Error($"LargeImpactorEvent_InitializeLandingSequence_Patch: {ex}");
			}
		}
	}

	[HarmonyPatch]
	internal static class LargeImpactorEvent_UnlockWinAchievement_Patch {
		static MethodBase TargetMethod() {
			var method = AccessTools.Method(typeof(LargeImpactorEvent), "UnlockWinAchievement");
			if (method == null)
				Log.Error("LargeImpactorEvent_UnlockWinAchievement_Patch: target method not found");
			return method;
		}

		static void Postfix() {
			if (!ModToggle.IsEnabled) return;
			try {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.DEMOLIOR.DESTROYED);
			} catch (System.Exception ex) {
				Log.Error($"LargeImpactorEvent_UnlockWinAchievement_Patch: {ex}");
			}
		}
	}

	[HarmonyPatch(typeof(LargeImpactorStatus.Instance), "DealDamage")]
	internal static class LargeImpactorStatus_Instance_DealDamage_Patch {
		internal static int _lastAnnouncedPercent = 100;

		static void Postfix(LargeImpactorStatus.Instance __instance) {
			if (!ModToggle.IsEnabled) return;
			try {
				int health = __instance.Health;
				if (health <= 0) {
					_lastAnnouncedPercent = 100;
					return;
				}
				int percent = health * 100 / __instance.def.MAX_HEALTH;
				int boundary = (percent / 10) * 10;
				if (boundary < _lastAnnouncedPercent) {
					_lastAnnouncedPercent = boundary;
					SpeechPipeline.SpeakInterrupt(
						string.Format((string)STRINGS.ONIACCESS.DEMOLIOR.HEALTH, boundary));
				}
			} catch (System.Exception ex) {
				Log.Error($"LargeImpactorStatus_Instance_DealDamage_Patch: {ex}");
			}
		}
	}
}
