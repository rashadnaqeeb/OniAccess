using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class HarvestToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Plants];
			if (go == null) return System.Array.Empty<string>();
			var harvestable = go.GetComponent<HarvestDesignatable>();
			if (harvestable == null) return System.Array.Empty<string>();

			if (harvestable.MarkedForHarvest)
				return new[] { (string)Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS.HARVEST_WHEN_READY.NAME") };
			return new[] { (string)Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS.DO_NOT_HARVEST.NAME") };
		}
	}
}
