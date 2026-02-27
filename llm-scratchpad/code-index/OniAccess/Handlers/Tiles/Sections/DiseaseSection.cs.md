// Aggregates germs by disease type across all sources at a cell:
// tile surface, buildings, pickupables, and pipe contents.
// Returns one token per disease type, or "clean" if total is zero.

class DiseaseSection : ICellSection (line 9)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 10)
    // Calls all four Add* helpers into a totals dictionary (byte diseaseIdx -> int count),
    // returns DISEASE_CLEAR if empty, otherwise one DISEASE_ENTRY token per disease type.

  private static void Accumulate(Dictionary<byte, int> totals, byte idx, int count) (line 31)
    // Adds count to totals[idx]; skips byte.MaxValue (no disease) and count <= 0.

  private static void AddTileSurface(int cell, Dictionary<byte, int> totals) (line 40)
    // Reads Grid.DiseaseIdx[cell] and Grid.DiseaseCount[cell].

  private static void AddBuildings(int cell, Dictionary<byte, int> totals) (line 44)
    // Calls AddBuildingLayer for ObjectLayer.Building and ObjectLayer.FoundationTile.

  private static void AddBuildingLayer(int cell, ObjectLayer layer, Dictionary<byte, int> totals) (line 49)
    // Gets PrimaryElement from the building at the given layer; accumulates its disease data.

  private static void AddPickupables(int cell, Dictionary<byte, int> totals) (line 58)
    // Walks the objectLayerListItem linked list on the Pickupables layer;
    // accumulates PrimaryElement disease data for each item.

  private static void AddConduits(int cell, Dictionary<byte, int> totals) (line 72)
    // Calls AddConduitFlow for liquid and gas conduit flows.

  private static void AddConduitFlow(ConduitFlow flow, int cell, Dictionary<byte, int> totals) (line 77)
    // Gets conduit contents at cell and accumulates disease data.
