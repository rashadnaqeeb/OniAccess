using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class CancelToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();

			var digGo = Grid.Objects[cell, (int)ObjectLayer.DigPlacer];
			if (digGo != null && digGo.GetComponent<Diggable>() != null)
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER);

			var mopGo = Grid.Objects[cell, (int)ObjectLayer.MopPlacer];
			if (mopGo != null && mopGo.GetComponent<Moppable>() != null)
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER);

			CheckDeconstructOrder(cell, (int)ObjectLayer.Building, tokens);
			CheckDeconstructOrder(cell, (int)ObjectLayer.FoundationTile, tokens);
			return tokens;
		}

		private static void CheckDeconstructOrder(int cell, int layer, List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			var d = go.GetComponent<Deconstructable>();
			if (d != null && d.IsMarkedForDeconstruction())
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT);
		}
	}
}
