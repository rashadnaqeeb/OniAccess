# PrioritizeToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads all prioritizable objects at a cell for the prioritize tool. Covers buildings,
utility layers, dig/mop placers, and pickupables (excluding duplicants). Respects filter
layers. For each object, identifies its type (dig order, construction, deconstruct,
sweep, mop, or generic) and formats the appropriate priority token. Adds building-layer
objects to `ctx.Claimed`.

---

```
class PrioritizeToolSection : ICellSection (line 4)
  private static readonly int[] Layers (line 5)
    // Full set of layers that can hold Prioritizable components, including Pickupables
  private static readonly int[] BuildingLayers (line 25)
    // Subset that qualifies for ctx.Claimed (Building, FoundationTile, Backwall)

  IEnumerable<string> Read(int cell, CellContext ctx) (line 31)
    // Iterates layers with dedup; handles Pickupables linked list separately
  private static bool ReadPrioritizable(UnityEngine.GameObject go,
      FilteredDragTool tool, List<string> tokens) (line 60)
    // Checks filter layer, then Prioritizable.showIcon/IsPrioritizable; dispatches by
    // component type (Diggable > Constructable > Deconstructable > Clearable > Moppable > KSelectable).
    // Returns true if a token was added.
```
