namespace OniAccess.Handlers.Tiles.Scanner {
	/// <summary>
	/// One instance in the 4-level scanner hierarchy.
	/// Multiple ScanEntries with the same Category/Subcategory/ItemName
	/// form the instance list for that item.
	/// </summary>
	public class ScanEntry {
		public int Cell;
		public IScannerBackend Backend;
		public object BackendData;
		public string Category;
		public string Subcategory;
		public string ItemName;
	}
}
