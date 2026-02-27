// Speaks decor value at the cell with sign prefix. Always emits.
// Clamps to the game's maximum decor value (DecorMonitor.MAXIMUM_DECOR_VALUE)
// to match what actually affects duplicants.

class DecorSection : ICellSection (line 10)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 11)
    // Gets decor via GameUtil.GetDecorAtCell, clamps to MAXIMUM_DECOR_VALUE,
    // formats with sign prefix using STRINGS.ONIACCESS.GLANCE.OVERLAY_DECOR.
