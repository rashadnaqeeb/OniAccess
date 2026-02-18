namespace OniAccess.Handlers {
	/// <summary>
	/// Interface for handlers that support type-ahead search via TypeAheadSearch.HandleKey.
	/// Handlers implement this to describe their searchable list at the current navigation level.
	/// </summary>
	public interface ISearchable {
		/// <summary>
		/// Number of searchable items at the current navigation level.
		/// Return 0 to disable search (A-Z keys pass through to handler).
		/// </summary>
		int SearchItemCount { get; }

		/// <summary>
		/// Current cursor position (reserved for future use).
		/// </summary>
		int SearchCurrentIndex { get; }

		/// <summary>
		/// Searchable label for the item at the given index.
		/// Return null to skip an item in search results.
		/// </summary>
		string GetSearchLabel(int index);

		/// <summary>
		/// Move cursor to index and announce. Called during search navigation
		/// and when search results are found. The move is permanent.
		/// </summary>
		void SearchMoveTo(int index);
	}
}
