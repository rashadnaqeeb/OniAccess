# DisinfectToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads disinfectable objects at a cell. Checks Building, FoundationTile, and Pickupables
layers. For buildings: reports `MARKED_DISINFECT` if already marked, or reports the
object name + disease name + disease count if it has active germs. For pickupables:
iterates the linked list and applies the same logic to each item. Adds building-layer
objects to `ctx.Claimed`. Uses a `HashSet<GameObject>` to deduplicate.

---

```
class DisinfectToolSection : ICellSection (line 4)
  private static readonly int[] Layers (line 5)
    // Building, FoundationTile, Pickupables
  private static readonly int[] BuildingLayers (line 11)
    // Building, FoundationTile — subset that qualifies for ctx.Claimed

  IEnumerable<string> Read(int cell, CellContext ctx) (line 16)
  private static void ReadLayer(int cell, int layer,
      HashSet<UnityEngine.GameObject> seen, CellContext ctx,
      List<string> tokens) (line 24)
    // Dispatches to linked-list iteration for Pickupables, or single-object path for building layers
  private static void ReadDisinfectable(UnityEngine.GameObject go, int layer,
      CellContext ctx, List<string> tokens) (line 44)
    // Checks Disinfectable component; reports marked status or germ details via PrimaryElement
  private static bool IsMarkedForDisinfect(Disinfectable disinfectable) (line 69)
    // Checks KSelectable status items for MarkedForDisinfection
```
