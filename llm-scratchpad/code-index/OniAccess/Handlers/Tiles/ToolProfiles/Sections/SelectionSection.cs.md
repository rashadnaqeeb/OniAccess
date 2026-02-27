# SelectionSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reports "selected" for cells that the active tool handler has marked as selected. Used
as the first section in most tool profiles so the player always hears selection status
before cell content.

---

```
class SelectionSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
    // Queries ToolHandler.Instance.IsCellSelected; returns STRINGS.ONIACCESS.TOOLS.SELECTED or empty
```
