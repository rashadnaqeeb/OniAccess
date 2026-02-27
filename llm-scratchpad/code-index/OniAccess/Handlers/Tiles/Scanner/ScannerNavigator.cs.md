# ScannerNavigator.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Owns scanner navigation state and orchestrates refresh, navigation, and teleport. Called from `TileCursorHandler.Tick()` for keybind dispatch. Holds all backend instances and the current `ScannerSnapshot`.

```
class ScannerNavigator (line 12)
```

### Private fields

```
  ScannerSnapshot _snapshot (line 13)
  int _categoryIndex (line 14)
  int _subcategoryIndex (line 15)
  int _itemIndex (line 16)
  int _instanceIndex (line 17)
  int _lastWorldId (line 18)            -- -1 = uninitialized; used to detect world switches
  GridScanner _gridScanner (line 21)
  ElementClusterBackend _elementBackend (line 22)
  TileClusterBackend _tileBackend (line 23)
  NetworkSegmentBackend _networkBackend (line 24)
  OrderBackend _orderBackend (line 25)
  BiomeBackend _biomeBackend (line 26)
  EntityBackend _entityBackend (line 27)
  GeyserBackend _geyserBackend (line 28)
  RoomBackend _roomBackend (line 29)
  static Dictionary<string, LocString> _categoryNames (line 32)
  static Dictionary<string, LocString> _subcategoryNames (line 33)
```

### Constructor

```
  ScannerNavigator() (line 35)
```

### Public methods

```
  void CheckWorldSwitch() (line 45)
    -- Call every tick. Clears snapshot if the active world changed so the next navigation key triggers a refresh.

  void Refresh() (line 56)
    -- Runs all backends, builds a new snapshot, preserves category position if the same category still exists.

  void CycleCategory(int direction) (line 115)
  void CycleSubcategory(int direction) (line 141)
  void CycleItem(int direction) (line 167)
  void CycleInstance(int direction) (line 191)

  void Teleport() (line 213)
    -- Validates current entry; if stale, removes it and speaks INVALID. Otherwise calls TileCursor.JumpTo.
```

### Private helpers

```
  bool EnsureSnapshot() (line 240)
    -- Auto-refreshes if no snapshot. Returns true if a refresh was triggered (caller should return immediately).

  ScannerSubcategory CurrentSubcategory() (line 246)
  ScannerItem CurrentItem() (line 254)
  ScanEntry CurrentEntry() (line 260)

  string ValidateAndAnnounce(bool speakOnEmpty = true) (line 266)
    -- Loops: validates current entry, removes stale ones, returns announcement string or null.
    -- speakOnEmpty: if false, returns null silently when no items remain (used after cycling category/subcategory).

  string FormatAnnouncement(ScanEntry entry, ScannerItem item) (line 289)

  void RemoveCurrentAndAdvance() (line 297)
    -- Removes current entry from snapshot, then re-resolves all four indices by name to survive structural changes.

  static int FindIndexByName<T>(List<T> list, string name, Func<T,string> getName) (line 331)

  void ClampInstanceIndex(ScannerItem item) (line 339)

  static int Wrap(int current, int direction, int count) (line 344)

  static int WrapSkipEmpty<T>(int current, int direction, List<T> list, Func<T,bool> isNonEmpty) (line 349)
    -- Wraps while skipping entries that don't satisfy the predicate (used to skip empty subcategories).

  static void PlayWrapCheck(int prev, int next, int direction, int count) (line 362)
    -- Plays the wrap sound when navigation wraps around the list boundary.

  static void PlayWrapSound() (line 370)

  static void SpeakEmpty() (line 378)
```

### Private — LocString name lookup

```
  static string GetCategoryName(string taxonomyName) (line 387)
  static string GetSubcategoryName(string taxonomyName) (line 392)
  static Dictionary<string, LocString> BuildCategoryNames() (line 397)
  static Dictionary<string, LocString> BuildSubcategoryNames() (line 411)
```
