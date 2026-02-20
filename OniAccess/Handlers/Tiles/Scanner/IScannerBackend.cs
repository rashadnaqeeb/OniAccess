using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner {
	/// <summary>
	/// Backend interface for scanner data sources. Backends are stateless
	/// between refreshes — they query game state fresh in Scan() and
	/// return ScanEntry objects.
	/// </summary>
	public interface IScannerBackend {
		/// <summary>
		/// Query game state and return entries for the given world.
		/// Cursor position is not passed here — distance sorting happens
		/// in ScannerSnapshot after all backends return.
		/// </summary>
		IEnumerable<ScanEntry> Scan(int worldId);

		/// <summary>
		/// Validate that an entry is still current. Called when the user
		/// navigates to an instance. Returns false if the entry is stale
		/// and should be removed from the snapshot.
		/// </summary>
		bool ValidateEntry(ScanEntry entry, int cursorCell);

		/// <summary>
		/// Return the spoken label for this instance.
		/// </summary>
		string FormatName(ScanEntry entry);
	}
}
