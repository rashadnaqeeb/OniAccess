using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class CaptureToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Critter];
			if (go == null) return System.Array.Empty<string>();
			var sel = go.GetComponent<KSelectable>();
			if (sel == null) return System.Array.Empty<string>();

			var capturable = go.GetComponent<Capturable>();
			if (capturable == null)
				return new[] { sel.GetName() + ", " +
					(string)STRINGS.UI.TOOLS.CAPTURE.NOT_CAPTURABLE };
			if (capturable.IsMarkedForCapture)
				return new[] { sel.GetName() + ", " +
					(string)STRINGS.ONIACCESS.TOOLS.MARKED_CAPTURE };
			return new[] { sel.GetName() };
		}
	}
}
