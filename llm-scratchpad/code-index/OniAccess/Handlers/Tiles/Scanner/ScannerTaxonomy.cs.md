# ScannerTaxonomy.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Pure data class. Defines string constants for all category and subcategory names, their canonical display order, and helper methods that return sort indices. No game API calls. Backends reference these constants so typos are compile errors.

```
static class ScannerTaxonomy (line 9)
```

### Nested class Categories (line 10)

```
  static class Categories (line 10)
    const string Solids = "Solids" (line 11)
    const string Liquids = "Liquids" (line 12)
    const string Gases = "Gases" (line 13)
    const string Buildings = "Buildings" (line 14)
    const string Networks = "Networks" (line 15)
    const string Automation = "Automation" (line 16)
    const string Debris = "Debris" (line 17)
    const string Zones = "Zones" (line 18)
    const string Life = "Life" (line 19)
```

### Category ordering

```
  static string[] CategoryOrder (line 22)   -- Solids, Liquids, Gases, Buildings, Networks, Automation, Debris, Zones, Life
```

### Nested class Subcategories (line 34)

```
  static class Subcategories (line 34)
    -- Solids subcategories:
    const string All = "all" (line 35)
    const string Ores = "Ores" (line 38)
    const string Stone = "Stone" (line 39)
    const string Consumables = "Consumables" (line 40)
    const string Organics = "Organics" (line 41)
    const string Ice = "Ice" (line 42)
    const string Refined = "Refined" (line 43)
    const string Tiles = "Tiles" (line 44)
    -- Liquids:
    const string Waters = "Waters" (line 47)
    const string Fuels = "Fuels" (line 48)
    const string Molten = "Molten" (line 49)
    const string Misc = "Misc" (line 50)
    -- Gases:
    const string Safe = "Safe" (line 53)
    const string Unsafe = "Unsafe" (line 54)
    -- Buildings:
    const string Oxygen = "Oxygen" (line 57)
    const string Generators = "Generators" (line 58)
    const string Farming = "Farming" (line 59)
    const string Production = "Production" (line 60)
    const string Storage = "Storage" (line 61)
    const string Refining = "Refining" (line 62)
    const string Temperature = "Temperature" (line 63)
    const string Wellness = "Wellness" (line 64)
    const string Morale = "Morale" (line 65)
    const string Infrastructure = "Infrastructure" (line 66)
    const string Rocketry = "Rocketry" (line 67)
    const string Geysers = "Geysers" (line 68)
    -- Networks:
    const string Power = "Power" (line 71)
    const string Liquid = "Liquid" (line 72)
    const string Gas = "Gas" (line 73)
    const string Conveyor = "Conveyor" (line 74)
    const string Transport = "Transport" (line 75)
    -- Automation:
    const string Sensors = "Sensors" (line 78)
    const string Gates = "Gates" (line 79)
    const string Controls = "Controls" (line 80)
    const string Wires = "Wires" (line 81)
    -- Debris:
    const string Materials = "Materials" (line 84)
    const string Food = "Food" (line 85)
    const string Items = "Items" (line 86)
    const string Bottles = "Bottles" (line 87)
    -- Zones:
    const string Orders = "Orders" (line 90)
    const string Rooms = "Rooms" (line 91)
    const string Biomes = "Biomes" (line 92)
    -- Life:
    const string Duplicants = "Duplicants" (line 95)
    const string Robots = "Robots" (line 96)
    const string TameCritters = "Tame Critters" (line 97)
    const string WildCritters = "Wild Critters" (line 98)
    const string WildPlants = "Wild Plants" (line 99)
    const string FarmPlants = "Farm Plants" (line 100)
```

### Subcategory ordering and index caches

```
  static Dictionary<string, string[]> SubcategoryOrder (line 103)   -- per-category ordered subcategory arrays

  private static Dictionary<string, int> _categoryIndices (line 149)
  private static Dictionary<string, Dictionary<string, int>> _subcategoryIndices (line 150)

  static ScannerTaxonomy() (line 152)   -- static constructor that builds the index lookup dictionaries

  static int CategorySortIndex(string category) (line 166)
    -- Returns position in CategoryOrder, or int.MaxValue for unknown categories.

  static int SubcategorySortIndex(string category, string subcategory) (line 170)
    -- Returns position within the given category's SubcategoryOrder, or int.MaxValue if unknown.
```
