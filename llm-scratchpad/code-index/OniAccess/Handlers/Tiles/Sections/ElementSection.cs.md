// Speaks Grid.Element[cell].name with a glance-friendly mass.
// Suppressed when a foreground building (ObjectLayer.Building) or foundation
// tile (ObjectLayer.FoundationTile) is present, unless the Oxygen overlay
// is active. Still speaks when only a Backwall (drywall, tempshift plate)
// is present, since sighted players see the element through background buildings.

class ElementSection : ICellSection (line 11)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 12)
    // Returns empty when a building or foundation tile is present (except in Oxygen overlay).
    // Otherwise returns "{element.name}, {FormatGlanceMass(kg)}".

  // Compact mass for glance speech. Grid.Mass is in kg.
  // Under 0.1 kg: grams with one decimal ("52.3 g").
  // 0.1 to 10 kg: kg with two decimals ("1.54 kg").
  // Over 10 kg: whole kg ("688 kg").
  internal static string FormatGlanceMass(float kg) (line 31)
