namespace OniAccess.Handlers.Tiles.AreaScan {
	/// <summary>
	/// Produces a speech summary for a rectangular area of tiles.
	/// Each overlay can provide a custom scanner; overlays without one
	/// fall back to DefaultAreaScanner.
	/// </summary>
	public interface IAreaScanner {
		/// <param name="cells">Visible cells in the cursor area.</param>
		/// <param name="totalCells">Total cells in the area (including unexplored).</param>
		/// <param name="unexploredCount">Number of unexplored cells in the area.</param>
		string Scan(int[] cells, int totalCells, int unexploredCount);
	}
}
