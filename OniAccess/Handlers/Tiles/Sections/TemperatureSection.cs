using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks cell temperature and warns when near a phase transition.
	/// </summary>
	public class TemperatureSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			float kelvin = Grid.Temperature[cell];
			if (kelvin <= 0f) yield break;

			yield return GameUtil.GetFormattedTemperature(kelvin);

			Element element = Grid.Element[cell];
			float threshold;

			threshold = element.lowTemp * 0.05f;
			if (kelvin < element.lowTemp + threshold && element.lowTempTransition != null)
				yield return STRINGS.ONIACCESS.TEMPERATURE.NEAR_FREEZING;

			threshold = element.highTemp * 0.05f;
			if (kelvin > element.highTemp - threshold && element.highTempTransition != null)
				yield return STRINGS.ONIACCESS.TEMPERATURE.NEAR_BOILING;
		}
	}
}
