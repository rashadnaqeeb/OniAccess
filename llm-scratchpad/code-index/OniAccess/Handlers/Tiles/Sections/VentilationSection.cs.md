// Reads gas ventilation infrastructure (pipes, bridges) at a cell.
// The game also registers buildings on GasConduitConnection at
// their port cells; skip those so BuildingSection handles them.

class VentilationSection : ICellSection (line 9)
  private static readonly int[] _layers (line 10)
    // Initialized to { ObjectLayer.GasConduit, ObjectLayer.GasConduitConnection }
  IEnumerable<string> Read(int cell, CellContext ctx) (line 14)
    // Iterates _layers; skips already-claimed objects and port registrations
    // (via PowerSection.IsPortRegistration), claims the rest, appends KSelectable name.
