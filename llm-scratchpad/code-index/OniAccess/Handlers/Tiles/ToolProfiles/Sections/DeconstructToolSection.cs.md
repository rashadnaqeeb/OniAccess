# DeconstructToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads all deconstructable objects at a cell across building and utility layers. Respects
the active tool's filter layers. For each deconstructable: reports priority if already
marked, then always reports the object name. Adds objects in BuildingLayers to
`ctx.Claimed`. Uses a `HashSet<GameObject>` to deduplicate objects spanning multiple layers.

---

```
class DeconstructToolSection : ICellSection (line 4)
  private static readonly int[] Layers (line 5)
    // All layers that can hold a Deconstructable component
  private static readonly int[] BuildingLayers (line 22)
    // Subset that qualifies for ctx.Claimed (Building, FoundationTile, Backwall)

  IEnumerable<string> Read(int cell, CellContext ctx) (line 28)
```
