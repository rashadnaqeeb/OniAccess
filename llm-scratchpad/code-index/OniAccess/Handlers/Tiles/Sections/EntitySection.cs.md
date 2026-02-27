// Reads duplicants (ObjectLayer.Minion) and critters (CreatureBrain on
// ObjectLayer.Pickupables) at a cell.
// Traverses ObjectLayerListItem linked lists for critters.

class EntitySection : ICellSection (line 9)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 10)
    // Calls ReadMinions and ReadCritters, returns combined tokens.

  private static void ReadMinions(int cell, List<string> tokens) (line 17)
    // Reads ObjectLayer.Minion, appends KSelectable name if present.

  private static void ReadCritters(int cell, List<string> tokens) (line 25)
    // Reads ObjectLayer.Pickupables, walks objectLayerListItem linked list,
    // appends KSelectable name for each item with a CreatureBrain component.
