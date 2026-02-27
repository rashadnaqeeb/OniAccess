# CancelToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads all cancellable orders at a cell: dig orders, mop orders, deconstruct marks, and
pending construction. Respects the active tool's filter layers via `FilteredDragTool`.
Adds claimed buildings to `ctx.Claimed` so the default building section won't re-report
them. Uses a `HashSet<GameObject>` to deduplicate objects that appear in multiple layers.

---

```
class CancelToolSection : ICellSection (line 4)
  private static readonly int[] Layers (line 5)
    // All layers that can hold cancellable orders
  private static readonly int[] BuildingLayers (line 24)
    // Subset of Layers that qualify for ctx.Claimed (Building, FoundationTile, Backwall)

  IEnumerable<string> Read(int cell, CellContext ctx) (line 30)
  private static bool ReadCancellable(UnityEngine.GameObject go, List<string> tokens) (line 49)
    // Checks for Diggable, Moppable, Deconstructable (marked), or Constructable in priority order.
    // Returns true if a cancellable order was found and a token added.
```
