// Speaks radiation level at the cell. Always emits.

class RadiationSection : ICellSection (line 7)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 8)
    // Returns GameUtil.GetFormattedRads(Grid.Radiation[cell]).
