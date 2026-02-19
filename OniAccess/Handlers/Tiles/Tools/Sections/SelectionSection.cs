using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class SelectionSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var handler = OniAccess.Handlers.Tools.ToolHandler.Instance;
			if (handler != null && handler.IsCellSelected(cell))
				return new[] { (string)STRINGS.ONIACCESS.TOOLS.SELECTED };
			return System.Array.Empty<string>();
		}
	}
}
