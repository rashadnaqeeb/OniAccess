using System.Collections.Generic;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads a CollapsibleDetailContentPanel into a DetailSection.
	/// Shared by PropertiesTab and PersonalityTab — both panels use the same
	/// SetLabel/Commit pattern with DetailLabel children under Content.
	/// </summary>
	static class CollapsiblePanelReader {
		/// <summary>
		/// Walk active DetailLabel children in the panel's Content transform.
		/// Entries whose text starts with a space are folded into the preceding
		/// non-indented entry's SpeechFunc (spoken as one item).
		/// </summary>
		public static DetailSection BuildSection(CollapsibleDetailContentPanel gameSection) {
			var section = new DetailSection();

			var headerLabel = gameSection.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text))
				section.Header = headerLabel.text;

			var content = gameSection.Content;
			if (content == null) return section;

			// Collect active DetailLabel children. Indented entries (leading whitespace)
			// are children of the preceding non-indented header entry.
			var activeLabels = new List<DetailLabel>();
			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel != null)
					activeLabels.Add(detailLabel);
			}

			int idx = 0;
			while (idx < activeLabels.Count) {
				var header = activeLabels[idx];
				var children = new List<DetailLabel>();
				int next = idx + 1;
				while (next < activeLabels.Count) {
					string nextText = activeLabels[next].label.text;
					if (string.IsNullOrEmpty(nextText) || nextText[0] != ' ')
						break;
					children.Add(activeLabels[next]);
					next++;
				}

				if (children.Count == 0) {
					var captured = header;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Label,
						GameObject = captured.gameObject,
						SpeechFunc = () => captured.label.text
					});
				} else {
					var capturedHeader = header;
					var capturedChildren = children.ToArray();
					section.Items.Add(new WidgetInfo {
						Label = capturedHeader.label.text,
						Type = WidgetType.Label,
						GameObject = capturedHeader.gameObject,
						SpeechFunc = () => {
							string text = capturedHeader.label.text;
							foreach (var child in capturedChildren) {
								string childText = child.label.text?.Trim();
								if (!string.IsNullOrEmpty(childText))
									text = $"{text} {childText}";
							}
							return text;
						}
					});
				}
				idx = next;
			}

			return section;
		}
	}
}
