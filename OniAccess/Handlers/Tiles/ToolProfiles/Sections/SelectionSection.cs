using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	public class SelectionSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var toolHandler = OniAccess.Handlers.Tools.ToolHandler.Instance;
			var buildHandler = OniAccess.Handlers.Build.BuildToolHandler.Instance;
			if ((toolHandler != null && toolHandler.IsCellSelected(cell))
				|| (buildHandler != null && buildHandler.IsCellSelected(cell)))
				return new[] { (string)STRINGS.ONIACCESS.TOOLS.SELECTED };
			return System.Array.Empty<string>();
		}
	}
}
