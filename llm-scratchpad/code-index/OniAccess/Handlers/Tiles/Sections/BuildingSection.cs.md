// Reads all buildings at a cell across ObjectLayer.Building,
// ObjectLayer.FoundationTile, and ObjectLayer.Backwall.
// Plants also occupy ObjectLayer.Building.
//
// For each building: utility ports (when overlay active), name,
// status items, construction state. Ports come first so the
// overlay-specific info is the first thing the player hears.
// Door access state comes through status items automatically.

class BuildingSection : ICellSection (line 15)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 16)
    // Top-level entry point. Reads building/foundation/backwall layers,
    // calls ReadBuilding for unclaimed objects, then calls ReadPortCell.
    // Catches and logs all exceptions.

  private static void ReadBuilding(GameObject go, int cell, List<string> tokens) (line 44)
    // Reads ports, name (prefixed "under construction" if Constructable),
    // status items, and cell-of-interest label for multi-cell buildings.

  // When the cursor is on a port cell that's outside the building's
  // footprint, the building won't be found on ObjectLayer.Building.
  //
  // For power and conduit overlays the game registers the building
  // on port-specific object layers, so a direct lookup works.
  //
  // For automation and radbolt overlays there is no port layer.
  // We scan nearby cells on the Building and FoundationTile layers
  // to find buildings whose ports resolve to the cursor cell.
  private static void ReadPortCell(int cell, GameObject buildingGo, GameObject foundationGo, List<string> tokens) (line 82)
    // Dispatches to ScanNearbyForPorts for Logic/Radiation overlays,
    // or does a direct layer lookup for Power/Liquid/Gas/Solid overlays.

  private const int PortScanRadius = 5 (line 127)

  private static void ScanNearbyForPorts(int cell, GameObject buildingGo, GameObject foundationGo, HashedString activeMode, List<string> tokens) (line 129)
    // Searches a 5-cell radius grid for buildings with ports at the target cell.

  private static void CheckScanCell(int nearbyCell, int layer, HashSet<GameObject> seen, int targetCell, HashedString activeMode, List<string> tokens) (line 151)
    // Checks one cell/layer for a building with ports at targetCell;
    // appends building name if any port tokens were added.

  private static void ReadStatusItems(KSelectable selectable, bool isPlant, List<string> tokens) (line 176)
    // Iterates the status item group, filters via StatusFilter.ShouldSpeak,
    // appends non-empty names.

  private static void ReadPorts(GameObject go, Building building, int cell, List<string> tokens) (line 200)
    // Calls ReadOverlayDetails, ReadAutomationPorts, ReadRadboltPorts.

  private static void ReadCellOfInterest(GameObject go, Building building, int origin, int cell, List<string> tokens) (line 208)
    // Determines if the current cell is the access point or output point of
    // a multi-cell building (fabricator, geyser, storage, dispenser).
    // Appends TILE_OF_INTEREST, ACCESS_POINT, or OUTPUT_POINT label.

  private static void ReadOverlayDetails(GameObject go, Building building, int origin, int cell, List<string> tokens) (line 253)
    // Emits conduit input/output port labels (numbered when multiple of same kind),
    // power input/output labels, and element name for TileMode overlay.

  private static int CountSecondaryPorts<T>(GameObject go, ConduitType conduitType) (line 336)
    // Counts ISecondaryInput or ISecondaryOutput components matching the given conduit type.

  private static void AddNumberedLabels(List<string> labels, int totalOfKind, List<string> tokens) (line 351)
    // Appends labels to tokens; uses NUMBERED_PORT format string when totalOfKind > 1.

  private static void ReadAutomationPorts(Building building, int origin, int cell, List<string> tokens) (line 364)
    // Only runs in Logic overlay. Reads LogicPorts.inputPortInfo and outputPortInfo.

  private static void ReadAutomationPortArray(LogicPorts.Port[] ports, Orientation orientation, int origin, int cell, List<string> tokens) (line 380)
    // Iterates a port array; tracks per-description ordinals to produce
    // numbered labels when multiple ports share the same description.

  private static void ReadRadboltPorts(Building building, int origin, int cell, List<string> tokens) (line 417)
    // Only runs in Radiation overlay. Checks UseHighEnergyParticleInputPort /
    // UseHighEnergyParticleOutputPort on BuildingDef.

  private static ConduitType OverlayModeToConduitType(HashedString mode) (line 439)
    // Maps overlay mode HashedString to ConduitType (Gas/Liquid/Solid/None).

  private static string ConduitInputLabel(ConduitType type) (line 446)
    // Returns localized input label for a conduit type (GAS_INPUT, LIQUID_INPUT, SOLID_INPUT, INPUT_PORT).

  private static string ConduitOutputLabel(ConduitType type) (line 455)
    // Returns localized output label for a conduit type.

  private static bool IsOverlayFocused() (line 464)
    // Returns true when the active overlay is one of the utility overlays tracked by StatusFilter.

  private static bool IsDecorOverlay() (line 469)
    // Returns true when the active overlay is Decor.

  private static string GetBuildingName(GameObject go, KSelectable selectable) (line 474)
    // Returns the building's def name when a non-original facade is active (except in Decor overlay);
    // otherwise returns KSelectable.GetName().
