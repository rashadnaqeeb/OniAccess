using HarmonyLib;
using OniAccess.Speech;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
	internal static class Localization_Initialize_Patch {
		private static void Postfix() {
			TranslationLoader.LoadModTranslations();
			// OverloadStrings updates LocString fields but not the global Strings
			// table. Re-running CreateLocStringKeys re-adds each entry using the
			// now-translated _text, so ToString() (used by string interpolation)
			// returns the translated value instead of English.
			LocString.CreateLocStringKeys(typeof(STRINGS.ONIACCESS), "STRINGS.");
			SpeechPipeline.SpeakInterrupt(
				string.Format(STRINGS.ONIACCESS.SPEECH.MOD_LOADED, Mod.Version));
		}
	}
}
