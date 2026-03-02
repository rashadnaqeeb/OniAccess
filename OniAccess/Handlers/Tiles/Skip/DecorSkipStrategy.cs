namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the decor band changes. Uses 7 bands derived from
	/// DecorMonitor morale thresholds (fractions of MAXIMUM_DECOR_VALUE 120).
	/// </summary>
	public class DecorSkipStrategy: ISkipStrategy {
		private const float Band1 = -30f;
		private const float Band2 = 0f;
		private const float Band3 = 30f;
		private const float Band4 = 60f;
		private const float Band5 = 90f;
		private const float Band6 = 120f;

		public object GetSignature(int cell) {
			float decor = GameUtil.GetDecorAtCell(cell);
			if (decor < Band1) return 0;
			if (decor < Band2) return 1;
			if (decor < Band3) return 2;
			if (decor < Band4) return 3;
			if (decor < Band5) return 4;
			if (decor < Band6) return 5;
			return 6;
		}
	}
}
