# HarvestToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads harvest designation status from the building at the cursor cell. Reports the
game's "Harvest When Ready" or "Do Not Harvest" filter layer name string depending on
`HarvestDesignatable.MarkedForHarvest`. Uses `ToolFilterHandler` constants to look up
the game's own localized filter layer names via `Strings.Get`.

---

```
class HarvestToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
```
