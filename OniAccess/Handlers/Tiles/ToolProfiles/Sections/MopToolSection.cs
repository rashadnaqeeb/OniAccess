using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	public class MopToolSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var mopGo = Grid.Objects[cell, (int)ObjectLayer.MopPlacer];
			if (mopGo == null) return System.Array.Empty<string>();
			if (mopGo.GetComponent<Moppable>() == null) return System.Array.Empty<string>();
			var pri = mopGo.GetComponent<Prioritizable>();
			if (pri != null)
				return new[] { string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER_PRIORITY,
					Widgets.PriorityWidget.FormatPriority(pri.GetMasterPriority())) };
			return new[] { (string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER };
		}
	}
}
