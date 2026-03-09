using System.Globalization;
using System.Text;

namespace OniAccess.Util {
	public static class StringUtil {
		/// <summary>
		/// Strips combining diacritical marks so accented characters match their
		/// base Latin letter (é→e, ç→c, ñ→n, etc.).
		/// </summary>
		public static string RemoveDiacritics(string text) {
			var decomposed = text.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder(decomposed.Length);
			for (int i = 0; i < decomposed.Length; i++) {
				if (CharUnicodeInfo.GetUnicodeCategory(decomposed[i]) != UnicodeCategory.NonSpacingMark)
					sb.Append(decomposed[i]);
			}
			return sb.ToString();
		}
	}
}
