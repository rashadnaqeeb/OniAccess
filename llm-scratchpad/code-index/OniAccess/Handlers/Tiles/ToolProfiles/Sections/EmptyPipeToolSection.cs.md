# EmptyPipeToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads conduit contents at the cursor cell for the empty pipe tool (liquid, gas, solid).
Respects filter layers. For each conduit present: reports "pipe empty" if mass is zero,
otherwise reports conduit type + element name + formatted mass. Reuses
`DisconnectToolSection.ConduitName` for type name and `ElementSection.FormatGlanceMass`
for mass formatting.

---

```
class EmptyPipeToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
  private static void ReadConduit(int cell, ConduitType type, ObjectLayer layer,
      FilteredDragTool tool, List<string> tokens) (line 14)
    // Gets flow manager contents; formats "empty" or "typeName elementName mass" token
```
