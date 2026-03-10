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

			var activeLabels = new List<DetailLabel>();
			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel != null)
					activeLabels.Add(detailLabel);
			}

			FoldIndentedItems(activeLabels, section,
				dl => dl.label.text, dl => dl.gameObject);

			return section;
		}

		/// <summary>
		/// Groups items by leading-whitespace indentation: entries whose text
		/// starts with a space are folded into the preceding non-indented
		/// entry's SpeechFunc (spoken as one combined item).
		/// </summary>
		public static void FoldIndentedItems<T>(
				List<T> items, DetailSection section,
				System.Func<T, string> getText,
				System.Func<T, GameObject> getGameObject) {
			int idx = 0;
			while (idx < items.Count) {
				var header = items[idx];
				int next = idx + 1;
				while (next < items.Count) {
					string nextText = getText(items[next]);
					if (string.IsNullOrEmpty(nextText) || nextText[0] != ' ')
						break;
					next++;
				}

				if (next == idx + 1) {
					var captured = header;
					section.Items.Add(new LabelWidget {
						Label = getText(captured),
						GameObject = getGameObject(captured),
						SpeechFunc = () => getText(captured)
					});
				} else {
					var capturedHeader = header;
					int childStart = idx + 1;
					int childEnd = next;
					var capturedItems = items;
					section.Items.Add(new LabelWidget {
						Label = getText(capturedHeader),
						GameObject = getGameObject(capturedHeader),
						SpeechFunc = () => {
							string text = getText(capturedHeader);
							for (int i = childStart; i < childEnd; i++) {
								string childText = getText(capturedItems[i])?.Trim();
								if (!string.IsNullOrEmpty(childText))
									text = $"{text} {childText}";
							}
							return text;
						}
					});
				}
				idx = next;
			}
		}
	}
}
