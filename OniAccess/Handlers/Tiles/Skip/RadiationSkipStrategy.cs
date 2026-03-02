namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips between irradiated and clean zones. In the base game
	/// without Spaced Out, radiation is always 0 so the skip never
	/// finds a change, which is harmless.
	/// </summary>
	public class RadiationSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			return Grid.Radiation[cell] > 0f;
		}
	}
}
