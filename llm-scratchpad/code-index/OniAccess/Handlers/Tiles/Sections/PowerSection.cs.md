// Reads power infrastructure (wires, bridges) at a cell.
// The game registers buildings on WireConnectors at their power
// port cells; skip those so BuildingSection handles them.

class PowerSection : ICellSection (line 9)
  private static readonly int[] _layers (line 10)
    // Initialized to { ObjectLayer.Wire, ObjectLayer.WireConnectors }
  IEnumerable<string> Read(int cell, CellContext ctx) (line 14)
    // Iterates _layers; skips already-claimed objects and port registrations
    // (via IsPortRegistration), claims the rest, appends KSelectable name.

  // True when a building was registered on this layer for port
  // tracking rather than being infrastructure that lives here.
  internal static bool IsPortRegistration(UnityEngine.GameObject go, int layer) (line 32)
    // Returns true when the building's Def.ObjectLayer differs from the given layer,
    // meaning the building was registered here only because it has a port at this cell.
    // Used by PowerSection, PlumbingSection, VentilationSection, and ConveyorSection.
