// Reads automation infrastructure (wires, gates) at a cell.

class AutomationSection : ICellSection (line 7)
  private static readonly int[] _layers (line 8)
    // Initialized to { ObjectLayer.LogicWire, ObjectLayer.LogicGate }
  IEnumerable<string> Read(int cell, CellContext ctx) (line 12)
    // Iterates _layers, looks up Grid.Objects[cell, layer], claims unclaimed
    // GameObjects via ctx.Claimed, returns KSelectable names.
