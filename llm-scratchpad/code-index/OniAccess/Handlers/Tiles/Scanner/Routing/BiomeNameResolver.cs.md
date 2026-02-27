# BiomeNameResolver.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Resolves `SubWorld.ZoneType` enum values to localized display names. Built lazily on first `GetName()` call from `SettingsCache.subworlds`. Strips the trailing " Biome" suffix (configurable via `STRINGS.ONIACCESS.SCANNER.BIOME_SUFFIX`) since it's redundant in the Biomes subcategory.

```
class BiomeNameResolver (line 11)

  private Dictionary<SubWorld.ZoneType, string> _names (line 12)   -- null until Build() is called

  string GetName(SubWorld.ZoneType zoneType) (line 14)
    -- Lazily calls Build() on first invocation. Returns the localized name from the dictionary,
    -- or InsertSpaces(zoneType.ToString()) as a fallback for unknown zone types.

  private void Build() (line 22)
    -- Iterates SettingsCache.subworlds; maps each SubWorld's zoneType to its localized name.
    -- Skips duplicate zone types (first entry wins). Strips " Biome" suffix if present.
    -- Falls back to InsertSpaces(zoneType.ToString()) if localized string is null.

  private static string InsertSpaces(string camelCase) (line 39)
    -- Inserts a space before each uppercase letter that follows a non-uppercase letter
    -- (regex: \B[A-Z] → " $1"). Used for unknown zone type names.
```
