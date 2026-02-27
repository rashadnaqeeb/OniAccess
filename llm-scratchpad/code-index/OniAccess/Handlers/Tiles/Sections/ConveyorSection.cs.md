// Reads solid conveyor infrastructure (rails, bridges) at a cell.
// The game also registers buildings on SolidConduitConnection at
// their port cells; skip those so BuildingSection handles them.

class ConveyorSection : ICellSection (line 9)
  private static readonly int[] _layers (line 10)
    // Initialized to { ObjectLayer.SolidConduit, ObjectLayer.SolidConduitConnection }
  IEnumerable<string> Read(int cell, CellContext ctx) (line 14)
    // Iterates _layers; skips already-claimed objects and port registrations
    // (via PowerSection.IsPortRegistration), claims the rest, appends KSelectable name.
