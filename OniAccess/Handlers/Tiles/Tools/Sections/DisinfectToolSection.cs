using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class DisinfectToolSection : ICellSection {
		private static readonly int[] Layers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
			(int)ObjectLayer.Pickupables,
		};

		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			foreach (int layer in Layers)
				ReadLayer(cell, layer, tokens);
			return tokens;
		}

		private static void ReadLayer(int cell, int layer, List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;

			if (layer == (int)ObjectLayer.Pickupables) {
				var pickupable = go.GetComponent<Pickupable>();
				if (pickupable == null) return;
				var item = pickupable.objectLayerListItem;
				while (item != null) {
					ReadDisinfectable(item.gameObject, tokens);
					item = item.nextItem;
				}
			} else {
				ReadDisinfectable(go, tokens);
			}
		}

		private static void ReadDisinfectable(UnityEngine.GameObject go, List<string> tokens) {
			var disinfectable = go.GetComponent<Disinfectable>();
			if (disinfectable == null) return;

			if (IsMarkedForDisinfect(disinfectable)) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DISINFECT);
				return;
			}

			var pe = go.GetComponent<PrimaryElement>();
			if (pe != null && pe.DiseaseCount > 0) {
				var sel = go.GetComponent<KSelectable>();
				if (sel == null) return;
				string diseaseName = GameUtil.GetFormattedDiseaseName(pe.DiseaseIdx);
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.DISINFECT_OBJECT,
					sel.GetName(), diseaseName, pe.DiseaseCount));
			}
		}

		private static bool IsMarkedForDisinfect(Disinfectable disinfectable) {
			var selectable = disinfectable.GetComponent<KSelectable>();
			return selectable != null
				&& selectable.HasStatusItem(Db.Get().MiscStatusItems.MarkedForDisinfection);
		}
	}
}
