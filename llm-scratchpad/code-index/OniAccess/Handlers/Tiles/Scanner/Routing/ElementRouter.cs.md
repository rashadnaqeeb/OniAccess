# ElementRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Routes natural elements to scanner subcategories based on phase (solid/liquid/gas) and element identity or tag.

```
static class ElementRouter (line 10)

  private static Dictionary<SimHashes, float> _gasExposureRates (line 11)
    -- Lazily loaded from GasLiquidExposureMonitor.customExposureRates via reflection.

  private static Dictionary<Tag, string> _solidSubcategories (line 13)
    -- Tag → Solids subcategory mapping:
    --   Metal/RefinedMetal/Alloy → Ores
    --   BuildableRaw → Stone
    --   ConsumableOre/Filter/CookingIngredient/Sublimating/Other → Consumables
    --   Organics/Farmable/Agriculture → Organics
    --   Liquifiable → Ice
    --   BuildableProcessed/ManufacturedMaterial/RareMaterials → Refined

  private static HashSet<SimHashes> _waters (line 33)
    -- Water, DirtyWater, SaltWater, Brine

  private static HashSet<SimHashes> _fuels (line 37)
    -- CrudeOil, Petroleum, Naphtha, Ethanol

  private static HashSet<SimHashes> _molten (line 41)
    -- Magma, MoltenIron, MoltenCopper, MoltenGold, MoltenAluminum, MoltenTungsten,
    -- MoltenNiobium, MoltenCobalt, MoltenGlass, MoltenLead

  static string GetSolidSubcategory(Element element) (line 48)
    -- Looks up element.materialCategory in _solidSubcategories; defaults to Consumables.

  static string GetLiquidSubcategory(Element element) (line 54)
    -- Waters → Waters, fuels → Fuels, molten → Molten, else → Misc.

  static string GetGasSubcategory(Element element) (line 69)
    -- Breathable tag → Safe.
    -- exposure rate >= 1.0 → Unsafe; else → Safe.
    -- Uses GasLiquidExposureMonitor.customExposureRates; unlisted gases default to 1.0.

  private static float GetGasExposureRate(SimHashes id) (line 81)
    -- Lazily initializes _gasExposureRates by reflection-invoking
    -- GasLiquidExposureMonitor.InitializeCustomRates() then reading the
    -- customExposureRates static field. Logs a warning if the field cannot be read.
    -- Returns 1.0f for unlisted gases (game default).
```
