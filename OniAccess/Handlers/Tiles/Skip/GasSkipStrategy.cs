namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the element type changes. Useful on the Oxygen
	/// overlay to find boundaries between gas types.
	/// </summary>
	public class GasSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			return Grid.Element[cell].id;
		}
	}
}
