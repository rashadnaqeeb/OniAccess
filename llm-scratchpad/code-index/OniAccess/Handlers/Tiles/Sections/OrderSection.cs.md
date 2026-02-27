// Reads pending player orders at a cell. Collects individual order
// labels (with per-order priority), then emits a single token
// prefixed with "pending": e.g. "pending dig priority 5, mop".
// Build orders are handled by BuildingSection (Constructable check).

class OrderSection : ICellSection (line 11)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 12)
    // Calls all Collect* helpers into a parts list; returns empty if none,
    // otherwise joins all parts into one comma-separated token.

  private static void CollectDigOrder(int cell, List<string> parts) (line 31)
    // Checks ObjectLayer.DigPlacer for a Diggable component.

  private static void CollectMopOrder(int cell, List<string> parts) (line 39)
    // Checks ObjectLayer.MopPlacer for a Moppable component.

  private static void CollectSweepOrder(int cell, List<string> parts) (line 47)
    // Walks Pickupables linked list; appends sweep order for the first item
    // marked for clear (HasTag(GameTags.Garbage)).

  private static void CollectDeconstructOrder(int cell, List<string> parts) (line 66)
    // Checks Building and FoundationTile layers for Deconstructable marked for deconstruction.

  private static void CollectDeconstructOnLayer(GameObject go, List<string> parts) (line 74)
    // Helper for CollectDeconstructOrder; checks a single GameObject.

  private static void CollectHarvestOrder(int cell, List<string> parts) (line 84)
    // Checks ObjectLayer.Building for HarvestDesignatable.MarkedForHarvest.

  private static void CollectUprootOrder(int cell, List<string> parts) (line 94)
    // Checks ObjectLayer.Building for Uprootable.IsMarkedForUproot.

  private static void CollectDisinfectOrder(int cell, List<string> parts) (line 104)
    // Calls CollectDisinfectOnLayer for Building, FoundationTile, and Pickupables layers.

  private static void CollectDisinfectOnLayer(int cell, int layer, List<string> parts) (line 110)
    // Checks one layer for Disinfectable marked for disinfection via status item lookup.

  private static void CollectAttackOrder(int cell, List<string> parts) (line 121)
    // Walks Pickupables linked list; appends attack order for items with
    // FactionAlignment.IsPlayerTargeted().

  private static void CollectCaptureOrder(int cell, List<string> parts) (line 139)
    // Walks Pickupables linked list; appends capture order for items with
    // Capturable.IsMarkedForCapture.

  private static readonly int[] _conduitLayers (line 157)
    // { ObjectLayer.GasConduit, ObjectLayer.LiquidConduit, ObjectLayer.SolidConduit }

  private static void CollectEmptyPipeOrder(int cell, List<string> parts) (line 163)
    // Checks each conduit layer for an IEmptyConduitWorkable with the empty-pipe status item.

  private static bool IsMarkedForEmptying(IEmptyConduitWorkable workable) (line 175)
    // Checks for "EmptyLiquidConduit" or "EmptyGasConduit" status item IDs on the workable.

  private static string FormatOrder(string label, GameObject go) (line 182)
    // Returns label with priority appended (ORDER_PRIORITY format) if Prioritizable is present.

  private static string GetPriority(GameObject go) (line 190)
    // Returns the master priority value as a string, or null if no Prioritizable.

  private static bool IsMarkedForClear(Clearable clearable) (line 197)
    // Returns clearable.HasTag(GameTags.Garbage).

  private static bool IsMarkedForDisinfect(Disinfectable disinfectable) (line 201)
    // Checks for Db.Get().MiscStatusItems.MarkedForDisinfection on the KSelectable.
