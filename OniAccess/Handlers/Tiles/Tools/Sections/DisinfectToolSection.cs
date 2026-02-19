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
			try {
				return HarmonyLib.Traverse.Create(disinfectable)
					.Field<bool>("isMarkedForDisinfect").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"DisinfectToolSection: {ex.Message}");
				return false;
			}
		}
	}
}
