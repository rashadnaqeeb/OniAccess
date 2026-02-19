using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// A stateless, single-concern tile glance contributor.
	/// Each section queries live game data for the given cell and
	/// returns zero or more text tokens. GlanceComposer concatenates
	/// all tokens with ", " and speaks the result.
	///
	/// Sections must never cache game state. Every token must come
	/// from a LocString or game-provided string.
	/// </summary>
	public interface ICellSection {
		IEnumerable<string> Read(int cell, CellContext ctx);
	}
}
