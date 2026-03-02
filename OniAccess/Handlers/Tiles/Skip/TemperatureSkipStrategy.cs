namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the temperature band changes. Uses 8 absolute
	/// bands matching the game's SimDebugView color intervals.
	/// </summary>
	public class TemperatureSkipStrategy: ISkipStrategy {
		private const float Band1 = 273.15f;
		private const float Band2 = 283.15f;
		private const float Band3 = 293.15f;
		private const float Band4 = 303.15f;
		private const float Band5 = 310.15f;
		private const float Band6 = 373.15f;
		private const float Band7 = 2073.15f;

		public object GetSignature(int cell) {
			float kelvin = Grid.Temperature[cell];
			if (kelvin < Band1) return 0;
			if (kelvin < Band2) return 1;
			if (kelvin < Band3) return 2;
			if (kelvin < Band4) return 3;
			if (kelvin < Band5) return 4;
			if (kelvin < Band6) return 5;
			if (kelvin < Band7) return 6;
			return 7;
		}
	}
}
