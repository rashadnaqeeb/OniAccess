namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the light intensity band changes. Uses 7 bands
	/// based on DUPLICANTSTATS.STANDARD.Light thresholds.
	/// </summary>
	public class LightSkipStrategy: ISkipStrategy {
		private const int Band1 = 500;
		private const int Band2 = 1000;
		private const int Band3 = 10000;
		private const int Band4 = 40000;
		private const int Band5 = 50000;
		private const int Band6 = 72000;

		public object GetSignature(int cell) {
			int lux = Grid.LightIntensity[cell];
			if (lux <= 0) return 0;
			if (lux < Band1) return 1;
			if (lux < Band2) return 2;
			if (lux < Band3) return 3;
			if (lux < Band4) return 4;
			if (lux < Band5) return 5;
			if (lux < Band6) return 6;
			return 7;
		}
	}
}
