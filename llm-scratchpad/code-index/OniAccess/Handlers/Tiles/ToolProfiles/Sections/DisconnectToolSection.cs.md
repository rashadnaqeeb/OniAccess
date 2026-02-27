# DisconnectToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads conduits (liquid, gas, solid) and power wire at the cursor cell for the disconnect
tool. Respects the active tool's filter layers. The `ConduitName` helper is internal so
`EmptyPipeToolSection` can reuse it without duplicating the switch.

---

```
class DisconnectToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
  private static void ReadConduit(int cell, ConduitType type, ObjectLayer layer,
      FilteredDragTool tool, List<string> tokens) (line 15)
    // Skips if the tool's filter excludes this layer; checks flow manager for conduit presence
  private static void ReadPowerConnection(int cell, FilteredDragTool tool, List<string> tokens) (line 23)
  internal static string ConduitName(ConduitType type) (line 30)
    // Returns localized conduit type name; shared with EmptyPipeToolSection
```
