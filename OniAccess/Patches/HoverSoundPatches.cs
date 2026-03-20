using HarmonyLib;
using UnityEngine.EventSystems;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(KButton), nameof(KButton.OnPointerEnter),
		typeof(PointerEventData))]
	internal static class KButton_OnPointerEnter_Patch {
		static void Prefix(ButtonSoundPlayer ___soundPlayer, out bool __state) {
			__state = ___soundPlayer.Enabled;
			if (ConfigManager.Config.FootstepEarcons
				&& KInputManager.isMousePosLocked)
				___soundPlayer.Enabled = false;
		}

		static void Postfix(ButtonSoundPlayer ___soundPlayer, bool __state) {
			___soundPlayer.Enabled = __state;
		}
	}

	[HarmonyPatch(typeof(KToggle), "OnPointerEnter", new System.Type[0])]
	internal static class KToggle_OnPointerEnter_Patch {
		static void Prefix(ToggleSoundPlayer ___soundPlayer, out bool __state) {
			__state = ___soundPlayer.Enabled;
			if (ConfigManager.Config.FootstepEarcons
				&& KInputManager.isMousePosLocked)
				___soundPlayer.Enabled = false;
		}

		static void Postfix(ToggleSoundPlayer ___soundPlayer, bool __state) {
			___soundPlayer.Enabled = __state;
		}
	}

	[HarmonyPatch(typeof(KSelectable), nameof(KSelectable.Hover))]
	internal static class KSelectable_Hover_Patch {
		static void Prefix(ref bool playAudio) {
			if (ConfigManager.Config.FootstepEarcons
				&& KInputManager.isMousePosLocked)
				playAudio = false;
		}
	}

	[HarmonyPatch(typeof(SpeedControlScreen), nameof(SpeedControlScreen.Pause))]
	internal static class SpeedControlScreen_Pause_Patch {
		static void Postfix() {
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().SpeedPausedMigrated,
				FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}
}
