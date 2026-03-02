namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips between lit and dark areas. A cell is lit when it has
	/// at least one artificial light source.
	/// </summary>
	public class LightSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			return Grid.LightCount[cell] > 0;
		}
	}
}
