# OrderRouter.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Routing`

Order detection configuration and clustering strategy definitions. Mirrors `OrderSection.cs` in the game but returns booleans/type keys for clustering instead of formatted text. Used by both `GridScanner` (forward pass detection) and `OrderBackend` (validation).

```
static class OrderRouter (line 9)

  enum ClusterStrategy (line 10)
    BoxSelection (line 11)   -- contiguous region clustered by adjacency (dig, mop, sweep, disinfect)
    SameType (line 12)       -- contiguous region of same prefab type (build, deconstruct, harvest, uproot)
    Individual (line 13)     -- one entry per entity, not clustered (attack, capture, empty pipe)

  struct OrderType (line 16)
    string Label (line 17)   -- spoken label, from STRINGS.ONIACCESS.GLANCE.*
    ClusterStrategy Strategy (line 18)
```

### Static OrderType instances

```
  static OrderType Dig (line 21)        -- BoxSelection
  static OrderType Mop (line 26)        -- BoxSelection
  static OrderType Sweep (line 31)      -- BoxSelection
  static OrderType Disinfect (line 36)  -- BoxSelection
  static OrderType Build (line 41)      -- SameType
  static OrderType Deconstruct (line 46) -- SameType
  static OrderType Harvest (line 51)    -- SameType
  static OrderType Uproot (line 56)     -- SameType
  static OrderType Attack (line 61)     -- Individual
  static OrderType Capture (line 66)    -- Individual
  static OrderType EmptyPipe (line 71)  -- Individual
```

### Detection methods

```
  static bool HasDigOrder(int cell) (line 80)
    -- Checks DigPlacer layer for a Diggable component.

  static bool HasMopOrder(int cell) (line 85)
    -- Checks MopPlacer layer for a Moppable component.

  static string GetDigTarget(int cell) (line 94)
    -- Returns Grid.Element[cell].name — the material being dug. Used for cluster naming.

  static string GetMopTarget(int cell) (line 101)
    -- Returns Grid.Element[cell].name — the liquid being mopped.

  static bool HasSweepOrder(int cell) (line 108)
    -- Checks Pickupables linked list for any entity with GameTags.Garbage.

  static bool HasDisinfectOrder(int cell) (line 123)
    -- Checks Building, FoundationTile, and Pickupables layers for a Disinfectable
    -- with MarkedForDisinfection status item.

  private static bool HasDisinfectOnLayer(int cell, int layer) (line 129)

  static string GetBuildOrderType(int cell) (line 144)
    -- Returns building prefab ID if a Constructable is on the Building layer; else null.
    -- Type key used for same-type clustering.

  static string GetBuildOrderName(int cell) (line 155)
    -- Returns KSelectable.GetName() from the Building layer.

  static string GetDeconstructOrderType(int cell) (line 161)
    -- Checks Building layer first, then FoundationTile layer for a marked Deconstructable.

  private static string GetDeconstructOnLayer(int cell, int layer) (line 167)

  static string GetDeconstructOrderName(int cell) (line 176)
    -- Checks Building then FoundationTile layers.

  private static string GetDeconstructNameOnLayer(int cell, int layer) (line 182)

  static string GetHarvestOrderType(int cell) (line 190)
    -- Returns prefab tag name if a HarvestDesignatable on Building layer is MarkedForHarvest.

  static string GetHarvestOrderName(int cell) (line 199)

  static string GetUprootOrderType(int cell) (line 205)
    -- Returns prefab tag name if an Uprootable on Building layer is IsMarkedForUproot.

  static string GetUprootOrderName(int cell) (line 214)

  static bool HasEmptyPipeOrder(int cell, int conduitLayer) (line 223)
    -- Checks the given conduit layer for an IEmptyConduitWorkable with an active
    -- EmptyLiquidConduit, EmptyGasConduit, or EmptySolidConduit status item.
```
