// Speaks lux value at the cell. Always emits (0 lux is useful info).

class LightSection : ICellSection (line 7)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 8)
    // Returns GameUtil.GetFormattedLux(Grid.LightIntensity[cell]).
