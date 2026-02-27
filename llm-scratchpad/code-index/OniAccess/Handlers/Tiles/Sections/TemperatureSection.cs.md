// Speaks cell temperature and warns when near a phase transition.

class TemperatureSection : ICellSection (line 7)
  IEnumerable<string> Read(int cell, CellContext ctx) (line 8)
    // Yields nothing if Grid.Temperature[cell] <= 0 (invalid/vacuum cell).
    // Otherwise yields GameUtil.GetFormattedTemperature(kelvin).
    // Then checks element.lowTemp: if within 5% of lowTemp and a lowTempTransition
    // exists, yields STRINGS.ONIACCESS.TEMPERATURE.NEAR_FREEZING.
    // Then checks element.highTemp: if within 5% of highTemp and a highTempTransition
    // exists, yields STRINGS.ONIACCESS.TEMPERATURE.NEAR_BOILING.
