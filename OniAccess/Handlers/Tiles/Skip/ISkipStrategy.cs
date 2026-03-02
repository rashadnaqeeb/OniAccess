namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Returns an opaque signature for a cell. The skip engine walks
	/// cells until the signature changes. Compared via Equals.
	/// </summary>
	public interface ISkipStrategy {
		object GetSignature(int cell);
	}
}
