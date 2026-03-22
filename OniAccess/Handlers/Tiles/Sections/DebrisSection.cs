using System.Collections.Generic;
using OniAccess.Util;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Lists loose items (pickupables) on the cell by proper name.
	/// Uses GetProperName (not GetUnitFormattedName) so no mass is spoken.
	/// Traverses the ObjectLayerListItem linked list for stacked items.
	/// Skips duplicants and critters (handled by EntitySection).
	/// </summary>
	public class DebrisSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return System.Array.Empty<string>();

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return System.Array.Empty<string>();

			bool tempOverlay = OverlayScreen.Instance != null
				&& OverlayScreen.Instance.GetMode() == OverlayModes.Temperature.ID;
			float cellTemp = tempOverlay ? Grid.Temperature[cell] : 0f;

			var tokens = new List<string>();
			var item = pickupable.objectLayerListItem;
			while (item != null) {
				if (item.gameObject.GetComponent<MinionIdentity>() == null
					&& item.gameObject.GetComponent<CreatureBrain>() == null) {
					string name = DebrisNameHelper.GetDisplayName(item.gameObject);
					if (tempOverlay) {
						var pe = item.gameObject.GetComponent<PrimaryElement>();
						if (pe != null) {
							float itemTemp = pe.Temperature;
							if (cellTemp <= 0f
								|| System.Math.Abs(itemTemp - cellTemp) >= 1f)
								name += " " + GameUtil.GetFormattedTemperature(itemTemp);
							var warnings = new List<string>();
							TemperatureWarnings.AppendPhaseWarnings(pe, warnings);
							foreach (var w in warnings)
								name += " " + w;
						}
					}
					tokens.Add(name);
				}
				item = item.nextItem;
			}
			return tokens;
		}
	}
}
