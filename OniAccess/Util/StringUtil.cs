using System.Globalization;
using System.Text;

namespace OniAccess.Util {
	public static class StringUtil {
		/// <summary>
		/// Strips diacritics and expands ligatures so accented and composed
		/// characters match their plain Latin equivalents
		/// (é→e, ç→c, œ→oe, æ→ae, etc.).
		/// </summary>
		public static string RemoveDiacritics(string text) {
			var decomposed = text.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder(decomposed.Length);
			for (int i = 0; i < decomposed.Length; i++) {
				char c = decomposed[i];
				switch (c) {
					case 'œ': sb.Append("oe"); break;
					case 'Œ': sb.Append("oe"); break;
					case 'æ': sb.Append("ae"); break;
					case 'Æ': sb.Append("ae"); break;
					default:
						if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
							sb.Append(c);
						break;
				}
			}
			return sb.ToString();
		}
	}
}
