# BuildExtentSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Appends building extent directions (e.g. "extends 1 left, 1 right, 1 up") for buildings
larger than 1x1. Always appended last in the composer so experienced players can interrupt
before it plays. Delegates to `BuildToolHandler.BuildExtentText` using the current
orientation from `BuildMenuData`.

---

```
class BuildExtentSection : ICellSection (line 11)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 12)
```
