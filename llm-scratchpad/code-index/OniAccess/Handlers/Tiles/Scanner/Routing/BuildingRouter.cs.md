# BuildingRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Routes non-tile, non-utility buildings to scanner `(category, subcategory)` pairs. Built once at construction time by iterating `TUNING.BUILDINGS.PLANORDER`.

Three-level lookup priority (applied in order):
1. `_prefabOverrides` — explicit per-prefab exceptions applied first.
2. `_wholeCategoryMap` — all buildings in a game category map to the same scanner destination.
3. `_subcategoryMap` — (game category, game subcategory) pair maps to scanner destination.

Unknown buildings emit a `Log.Warn` and are left unmapped. `Route()` returns `(null, null)` for unmapped prefabs.

```
class BuildingRouter (line 15)

  private Dictionary<string, (string category, string subcategory)> _prefabToScanner (line 16)
    -- Built once in the constructor; keyed by building prefab ID.

  private static Dictionary<string, (string, string)> _prefabOverrides (line 18)
    -- Hard-coded per-prefab routing exceptions:
    --   HighEnergyParticleRedirector, HEPBridgeTile → Buildings > Rocketry
    --   Headquarters → Buildings > Infrastructure
    --   ResetSkillsStation, RoleStation → Buildings > Production

  private static Dictionary<string, (string, string)> _wholeCategoryMap (line 29)
    -- Lowercase game category → scanner destination:
    --   "oxygen" → Buildings > Oxygen
    --   "refining" → Buildings > Refining
    --   "medical" → Buildings > Wellness
    --   "rocketry" / "hep" → Buildings > Rocketry

  private static Dictionary<(string, string), (string, string)> _subcategoryMap (line 40)
    -- (lowercase game category, game subcategory) → scanner destination.
    -- ~40 entries covering Generators, Farming, Production, Storage, Temperature,
    --   Refining, Wellness, Morale, Infrastructure, Rocketry, Transport, Power,
    --   Liquid/Gas/Conveyor networks, and Automation subcategories.

  BuildingRouter() (line 143)
    -- Calls BuildMap() to populate _prefabToScanner.

  (string category, string subcategory) Route(string prefabId) (line 152)
    -- Returns (null, null) if the prefab is not mapped.

  private void BuildMap() (line 158)
    -- Iterates TUNING.BUILDINGS.PLANORDER. For each building, applies prefix override,
    -- whole-category map, subcategory map in priority order, or logs a warning.
    -- After the loop, also ensures all _prefabOverrides entries are inserted
    -- even if not found in PLANORDER.
```
