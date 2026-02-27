# EntityBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for entity-scanned categories: `Buildings` (non-tile, non-utility), `Debris`, and `Life`. Iterates game `Components` registries directly (no GridScanner pre-processing). Each entity is one `ScanEntry`; `BackendData` is the `GameObject`.

```
class EntityBackend : IScannerBackend (line 12)

  private BuildingRouter _buildingRouter (line 13)

  EntityBackend(BuildingRouter buildingRouter) (line 15)

  IEnumerable<ScanEntry> Scan(int worldId) (line 19)
    -- Delegates to all six private scan methods in sequence:
    -- ScanBuildings, ScanDebris, ScanDuplicants, ScanRobots, ScanCritters, ScanPlants.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 34)
    -- Checks go is not null/destroyed and its current cell is still visible.
    -- Updates entry.Cell to the entity's current cell position.

  string FormatName(ScanEntry entry) (line 43)
    -- Returns KSelectable.GetName() if available; falls back to entry.ItemName.

  private IEnumerable<ScanEntry> ScanBuildings(int worldId) (line 48)
    -- Iterates Components.BuildingCompletes. Skips KAnimTile and utility buildings.
    -- Routes via BuildingRouter; skips unmapped buildings (category == null).
    -- Uses BuildingFacade check: if building has a non-original facade, uses def.Name
    -- (facade name would be the cosmetic skin name, not the building type name).

  private IEnumerable<ScanEntry> ScanDebris(int worldId) (line 78)
    -- Iterates Components.Pickupables. Skips stored items, excluded tags (creatures, geysers, planted uprootables).
    -- Routes via DebrisRouter.GetSubcategory.

  private IEnumerable<ScanEntry> ScanDuplicants(int worldId) (line 101)
    -- Iterates Components.LiveMinionIdentities. Category = Life, Subcategory = Duplicants.

  private IEnumerable<ScanEntry> ScanRobots(int worldId) (line 118)
    -- Iterates Components.Brains; filters to GameTags.Robot. Category = Life, Subcategory = Robots.

  private IEnumerable<ScanEntry> ScanCritters(int worldId) (line 137)
    -- Iterates Components.Brains; filters to CreatureBrain component, excludes robots.
    -- Routes to TameCritters or WildCritters via LifeRouter.IsWild.

  private IEnumerable<ScanEntry> ScanPlants(int worldId) (line 161)
    -- Iterates Components.Uprootables.
    -- Routes to FarmPlants or WildPlants via LifeRouter.IsFarmPlant.
```
