# SweepToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads sweepable items at the cursor cell from the Pickupables layer. Skips duplicants
and creatures. For each `Clearable` item: reports sweep priority if marked for clearing
(detected via `GameTags.Garbage`), or reports the item name if not yet marked.

---

```
class SweepToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
  private static bool IsMarkedForClear(Clearable clearable) (line 42)
    // Returns true if the clearable has the GameTags.Garbage tag
```
