using System.IO;
using OniAccess.Util;

namespace OniAccess {
	internal static class TranslationLoader {
		public static void LoadModTranslations() {
			try {
				var code = Localization.GetCurrentLanguageCode();
				if (string.IsNullOrEmpty(code) || code == "en") return;

				var translationsDir = Path.Combine(Mod.ModDir, "translations");
				var poPath = Path.Combine(translationsDir, code + ".po");

				if (!File.Exists(poPath)) {
					// Strip _klei suffix (e.g. ru_klei -> ru)
					int underscore = code.IndexOf('_');
					if (underscore > 0) {
						poPath = Path.Combine(translationsDir, code.Substring(0, underscore) + ".po");
					}
				}

				if (!File.Exists(poPath)) {
					Log.Info($"No translation file found for language '{code}'");
					return;
				}

				var lines = File.ReadAllLines(poPath);
				Localization.LoadTranslation(lines);
				Log.Info($"Loaded translation from {Path.GetFileName(poPath)}");
			} catch (System.Exception ex) {
				Log.Warn($"Failed to load mod translations: {ex}");
			}
		}
	}
}
