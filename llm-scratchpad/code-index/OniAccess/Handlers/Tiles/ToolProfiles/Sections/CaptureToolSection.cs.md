# CaptureToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads capturable critters in the Pickupables layer. Reports entity name; appends
`MARKED_CAPTURE` if already marked, or the game's NOT_CAPTURABLE string if capture
is not allowed. Iterates the full objectLayerListItem linked list for all pickupables
in the cell.

---

```
class CaptureToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
```
