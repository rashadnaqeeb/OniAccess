// Reads liquid plumbing infrastructure (pipes, bridges) at a cell.
// The game also registers buildings on LiquidConduitConnection at
// their port cells; skip those so BuildingSection handles them.

class PlumbingSection : ICellSection (line 9)
  private static readonly int[] _layers (line 10)
    // Initialized to { ObjectLayer.LiquidConduit, ObjectLayer.LiquidConduitConnection }
  IEnumerable<string> Read(int cell, CellContext ctx) (line 14)
    // Iterates _layers; skips already-claimed objects and port registrations
    // (via PowerSection.IsPortRegistration), claims the rest, appends KSelectable name.
