using HarmonyLib;

namespace OniAccess.Patches {
	[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
	internal static class Localization_Initialize_Patch {
		private static void Postfix() {
			TranslationLoader.LoadModTranslations();
		}
	}
}
