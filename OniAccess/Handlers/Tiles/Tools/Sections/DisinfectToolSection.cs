using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class DisinfectToolSection: ICellSection {
		private static readonly int[] Layers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
			(int)ObjectLayer.Pickupables,
		};

		private static readonly int[] BuildingLayers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
		};

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tokens = new List<string>();
			var seen = new HashSet<UnityEngine.GameObject>();
			foreach (int layer in Layers)
				ReadLayer(cell, layer, seen, ctx, tokens);
			return tokens;
		}

		private static void ReadLayer(int cell, int layer,
			HashSet<UnityEngine.GameObject> seen, CellContext ctx,
			List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			if (!seen.Add(go)) return;

			if (layer == (int)ObjectLayer.Pickupables) {
				var pickupable = go.GetComponent<Pickupable>();
				if (pickupable == null) return;
				var item = pickupable.objectLayerListItem;
				while (item != null) {
					ReadDisinfectable(item.gameObject, -1, ctx, tokens);
					item = item.nextItem;
				}
			} else {
				ReadDisinfectable(go, layer, ctx, tokens);
			}
		}

		private static void ReadDisinfectable(UnityEngine.GameObject go, int layer,
			CellContext ctx, List<string> tokens) {
			var disinfectable = go.GetComponent<Disinfectable>();
			if (disinfectable == null) return;

			bool isBuildingLayer = System.Array.IndexOf(BuildingLayers, layer) >= 0;

			if (IsMarkedForDisinfect(disinfectable)) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DISINFECT);
				if (isBuildingLayer) ctx.Claimed.Add(go);
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
				if (isBuildingLayer) ctx.Claimed.Add(go);
			}
		}

		private static bool IsMarkedForDisinfect(Disinfectable disinfectable) {
			var selectable = disinfectable.GetComponent<KSelectable>();
			return selectable != null
				&& selectable.HasStatusItem(Db.Get().MiscStatusItems.MarkedForDisinfection);
		}
	}
}
