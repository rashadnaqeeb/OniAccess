using HarmonyLib;
using OniAccess.Speech;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
	internal static class Localization_Initialize_Patch {
		private static void Postfix() {
			TranslationLoader.LoadModTranslations();
			SpeechPipeline.SpeakInterrupt(
				string.Format(STRINGS.ONIACCESS.SPEECH.MOD_LOADED, Mod.Version));
		}
	}
}
