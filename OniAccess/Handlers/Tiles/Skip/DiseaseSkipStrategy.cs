namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips until the disease type changes. byte.MaxValue (255) means
	/// no disease. Finds boundaries between clean and contaminated
	/// zones, and between different disease types.
	/// </summary>
	public class DiseaseSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			return Grid.DiseaseIdx[cell];
		}
	}
}
