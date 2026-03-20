using HarmonyLib;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(SpeedControlScreen), nameof(SpeedControlScreen.Pause))]
	internal static class SpeedControlScreen_Pause_Patch {
		static void Postfix() {
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().SpeedPausedMigrated,
				FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}
}
