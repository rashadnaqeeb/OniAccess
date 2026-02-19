using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class SweepToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return tokens;
			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return tokens;

			var item = pickupable.objectLayerListItem;
			while (item != null) {
				var obj = item.gameObject;
				if (obj.GetComponent<MinionIdentity>() != null ||
					obj.GetComponent<CreatureBrain>() != null) {
					item = item.nextItem;
					continue;
				}

				var clearable = obj.GetComponent<Clearable>();
				if (clearable != null) {
					if (IsMarkedForClear(clearable)) {
						var pri = obj.GetComponent<Prioritizable>();
						if (pri != null)
							tokens.Add(string.Format(
								(string)STRINGS.ONIACCESS.TOOLS.MARKED_SWEEP_PRIORITY,
								pri.GetMasterPriority().priority_value));
						else
							tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_SWEEP);
					} else {
						var sel = obj.GetComponent<KSelectable>();
						if (sel != null)
							tokens.Add(sel.GetName());
					}
				}
				item = item.nextItem;
			}
			return tokens;
		}

		private static bool IsMarkedForClear(Clearable clearable) {
			try {
				return HarmonyLib.Traverse.Create(clearable)
					.Field<bool>("isMarkedForClear").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"SweepToolSection: {ex.Message}");
				return false;
			}
		}
	}
}
