namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the temperature band changes. Uses 8 absolute
	/// bands matching the game's SimDebugView color intervals.
	/// </summary>
	public class TemperatureSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			int band = TemperatureBand.Classify(cell);
			return band == TemperatureBand.Vacuum ? (object)0 : band;
		}
	}
}
