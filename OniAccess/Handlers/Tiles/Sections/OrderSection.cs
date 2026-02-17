using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads pending player orders at a cell:
	///   - Dig (Diggable on ObjectLayer.DigPlacer)
	///   - Mop (Moppable on ObjectLayer.MopPlacer)
	///   - Sweep (Clearable.isMarkedForClear on ObjectLayer.Pickupables)
	/// Build orders are handled by BuildingSection (Constructable check).
	/// Each order includes its work priority from Prioritizable.
	/// </summary>
	public class OrderSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			ReadDigOrder(cell, tokens);
			ReadMopOrder(cell, tokens);
			ReadSweepOrder(cell, tokens);
			return tokens;
		}

		private static void ReadDigOrder(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.DigPlacer];
			if (go == null) return;
			if (go.GetComponent<Diggable>() == null) return;
			string priority = GetPriority(go);
			tokens.Add(priority != null
				? string.Format((string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY,
					(string)STRINGS.ONIACCESS.GLANCE.ORDER_DIG, priority)
				: (string)STRINGS.ONIACCESS.GLANCE.ORDER_DIG);
		}

		private static void ReadMopOrder(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.MopPlacer];
			if (go == null) return;
			if (go.GetComponent<Moppable>() == null) return;
			string priority = GetPriority(go);
			tokens.Add(priority != null
				? string.Format((string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY,
					(string)STRINGS.ONIACCESS.GLANCE.ORDER_MOP, priority)
				: (string)STRINGS.ONIACCESS.GLANCE.ORDER_MOP);
		}

		private static void ReadSweepOrder(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return;
			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return;

			var item = pickupable.objectLayerListItem;
			while (item != null) {
				var clearable = item.gameObject.GetComponent<Clearable>();
				if (clearable != null && IsMarkedForClear(clearable)) {
					string priority = GetPriority(item.gameObject);
					tokens.Add(priority != null
						? string.Format((string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY,
							(string)STRINGS.ONIACCESS.GLANCE.ORDER_SWEEP, priority)
						: (string)STRINGS.ONIACCESS.GLANCE.ORDER_SWEEP);
					return;
				}
				item = item.nextItem;
			}
		}

		private static string GetPriority(GameObject go) {
			var prioritizable = go.GetComponent<Prioritizable>();
			if (prioritizable == null) return null;
			var setting = prioritizable.GetMasterPriority();
			return setting.priority_value.ToString();
		}

		private static bool IsMarkedForClear(Clearable clearable) {
			try {
				return HarmonyLib.Traverse.Create(clearable)
					.Field<bool>("isMarkedForClear").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"OrderSection.IsMarkedForClear: {ex}");
				return false;
			}
		}
	}
}
