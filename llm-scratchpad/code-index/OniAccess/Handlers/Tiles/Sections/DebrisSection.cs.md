// Lists loose items (pickupables) on the cell by proper name.
// Uses GetProperName (not GetUnitFormattedName) so no mass is spoken.
// Traverses the ObjectLayerListItem linked list for stacked items.
// Skips duplicants and critters (handled by EntitySection).

class DebrisSection : ICellSection (line 10)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 11)
    // Reads the Pickupables layer, walks the objectLayerListItem linked list,
    // skips MinionIdentity and CreatureBrain objects, appends GetProperName() for the rest.
