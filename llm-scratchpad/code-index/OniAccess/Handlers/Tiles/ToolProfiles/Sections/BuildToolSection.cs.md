# BuildToolSection.cs

namespace: OniAccess.Handlers.Tiles.ToolProfiles.Sections

## Summary
Three related classes for the build tool cursor. All live in the same file.

---

```
class BuildToolSection : ICellSection (line 10)
  // Utility line feedback. No-op until the player has set a utility start point.
  // Reports cell count and "invalid" when the proposed line is not straight or
  // contains bad cells.
  IEnumerable<string> Read(int cell, CellContext ctx) (line 11)
  private static IEnumerable<string> ReadUtilityLineStatus(int cell, BuildToolHandler handler) (line 19)
    // Computes line length/validity from start->cursor. Diagonal lines are always
    // flagged invalid; straight lines check BuildToolHandler.IsUtilityLineValid.

class UtilityLayerSection : ICellSection (line 60)
  // Delegates to the overlay section matching the utility type being placed
  // (wire->Power, gas pipe->Ventilation, etc.). No-op for non-utility buildings.
  IEnumerable<string> Read(int cell, CellContext ctx) (line 61)
  private static ICellSection MapDefToSection(ObjectLayer layer) (line 73)
    // Maps ObjectLayer enum value to the appropriate GlanceComposer shared section

class BuildPrioritySection : ICellSection (line 90)
  // Reads construction priority of a pending build order at the cursor cell.
  // Lets the player check what priority their queued buildings have while the
  // build tool is active.
  private static readonly int[] _layers (line 91)
    // All layers that can hold a Constructable component
  IEnumerable<string> Read(int cell, CellContext ctx) (line 101)
```
