using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class CaptureToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return System.Array.Empty<string>();

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return System.Array.Empty<string>();

			var tokens = new List<string>();
			var item = pickupable.objectLayerListItem;
			while (item != null) {
				var capturable = item.gameObject.GetComponent<Capturable>();
				if (capturable != null) {
					var sel = item.gameObject.GetComponent<KSelectable>();
					if (sel != null) {
						if (capturable.IsMarkedForCapture)
							tokens.Add(sel.GetName() + ", " +
								(string)STRINGS.ONIACCESS.TOOLS.MARKED_CAPTURE);
						else if (!capturable.allowCapture)
							tokens.Add(sel.GetName() + ", " +
								(string)STRINGS.UI.TOOLS.CAPTURE.NOT_CAPTURABLE);
						else
							tokens.Add(sel.GetName());
					}
				}
				item = item.nextItem;
			}
			return tokens;
		}
	}
}
