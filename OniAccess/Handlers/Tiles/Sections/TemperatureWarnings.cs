using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Phase-change and overheat proximity warnings shared by
	/// BuildingSection and DebrisSection on the temperature overlay.
	/// </summary>
	static class TemperatureWarnings {
		public static void AppendPhaseWarnings(PrimaryElement pe, List<string> tokens) {
			float temp = pe.Temperature;
			Element element = pe.Element;

			float threshold = element.lowTemp * 0.05f;
			if (temp < element.lowTemp + threshold && element.lowTempTransition != null)
				tokens.Add((string)STRINGS.ONIACCESS.TEMPERATURE.NEAR_FREEZING);

			threshold = element.highTemp * 0.05f;
			if (temp > element.highTemp - threshold && element.highTempTransition != null)
				tokens.Add((string)STRINGS.ONIACCESS.TEMPERATURE.NEAR_BOILING);
		}

		public static void AppendOverheatWarning(
				UnityEngine.GameObject go, PrimaryElement pe, List<string> tokens) {
			var overheatable = go.GetComponent<Overheatable>();
			if (overheatable == null) return;

			float overheatTemp = overheatable.OverheatTemperature;
			if (overheatTemp >= 10000f) return;

			float threshold = overheatTemp * 0.05f;
			if (pe.Temperature > overheatTemp - threshold)
				tokens.Add((string)STRINGS.ONIACCESS.TEMPERATURE.NEAR_OVERHEAT);
		}
	}
}
