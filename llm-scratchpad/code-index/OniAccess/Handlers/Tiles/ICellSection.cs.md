// A stateless, single-concern tile glance contributor.
// Each section queries live game data for the given cell and
// returns zero or more text tokens. GlanceComposer concatenates
// all tokens with ", " and speaks the result.
//
// Sections must never cache game state. Every token must come
// from a LocString or game-provided string.

namespace OniAccess.Handlers.Tiles

interface ICellSection (line 13)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 14)
    // Queries live game state for cell. Uses ctx.Claimed to avoid
    // outputting entities already spoken by a prior section in the same pass.
