namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the radiation band changes. Uses 6 bands based on
	/// DUPLICANTSTATS.RADIATION_EXPOSURE_LEVELS thresholds. In the base
	/// game without Spaced Out, radiation is always 0 so every cell
	/// lands in band 0 and the skip never fires, which is harmless.
	/// </summary>
	public class RadiationSkipStrategy: ISkipStrategy {
		private const float Band1 = 100f;
		private const float Band2 = 300f;
		private const float Band3 = 600f;
		private const float Band4 = 900f;

		public object GetSignature(int cell) {
			float rads = Grid.Radiation[cell];
			if (rads <= 0f) return 0;
			if (rads < Band1) return 1;
			if (rads < Band2) return 2;
			if (rads < Band3) return 3;
			if (rads < Band4) return 4;
			return 5;
		}
	}
}
