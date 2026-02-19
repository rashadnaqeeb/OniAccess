using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class HarvestToolSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Building];
			if (go == null) return System.Array.Empty<string>();
			var harvestable = go.GetComponent<HarvestDesignatable>();
			if (harvestable == null) return System.Array.Empty<string>();

			if (harvestable.MarkedForHarvest)
				return new[] { (string)Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + OniAccess.Handlers.Tools.ToolFilterHandler.HarvestWhenReadyKey + ".NAME") };
			return new[] { (string)Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + OniAccess.Handlers.Tools.ToolFilterHandler.DoNotHarvestKey + ".NAME") };
		}
	}
}
