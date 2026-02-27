# DigToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads dig-order status and the element at the cursor cell. Reports dig order priority
if a Diggable is present in the DigPlacer layer; then reports element name, hardness
string, and formatted mass for the cell's tile element.

---

```
class DigToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
```
