# AttackToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Reads critters/entities in the Pickupables layer that are hostile (non-Assist disposition
toward Duplicants). Reports the entity name; appends `MARKED_ATTACK` suffix if already
player-targeted. Iterates the full objectLayerListItem linked list to cover multiple
pickupables in the same cell.

---

```
class AttackToolSection : ICellSection (line 4)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 5)
```
