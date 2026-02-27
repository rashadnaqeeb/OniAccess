# MopToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads mop order status from the MopPlacer layer at the cursor cell. Reports priority
if a `Prioritizable` is present, otherwise the plain `MOP_ORDER` string. Returns empty
if no mop order exists at the cell.

---

```
class MopToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
```
