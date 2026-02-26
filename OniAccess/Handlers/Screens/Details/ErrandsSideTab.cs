using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	class ErrandsSideTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME;
		public int StartLevel => 1;
		public string GameTabId => null;

		public bool IsAvailable(GameObject target) {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Errands);
			return tab != null && tab.IsVisible;
		}

		public void OnTabSelected() {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Errands);
			if (tab?.tabInstance != null)
				WidgetOps.ClickMultiToggle(tab.tabInstance);
		}

		public void Populate(GameObject target, List<DetailSection> sections) {
			var ds = DetailsScreen.Instance;
			if (ds == null) return;

			MinionTodoSideScreen screen = null;
			foreach (var s in ConfigSideTab.GetActiveScreens(
					ds, DetailsScreen.SidescreenTabTypes.Errands)) {
				screen = s as MinionTodoSideScreen;
				if (screen != null) break;
			}
			if (screen == null) return;

			AddScheduleSection(screen, sections);
			AddCurrentTaskSection(screen, sections);
			AddPriorityGroupSections(screen, sections);
		}

		private static void AddScheduleSection(
				MinionTodoSideScreen screen, List<DetailSection> sections) {
			string shiftText = screen.currentShiftLabel.text;
			if (string.IsNullOrEmpty(shiftText)) return;

			var section = new DetailSection();
			section.Header = (string)STRINGS.ONIACCESS.DETAILS.SCHEDULE;
			section.Items.Add(new LabelWidget {
				Label = shiftText,
				SpeechFunc = () => screen.currentShiftLabel.text
			});
			sections.Add(section);
		}

		private static void AddCurrentTaskSection(
				MinionTodoSideScreen screen, List<DetailSection> sections) {
			var entry = screen.currentTask;
			if (entry == null || !entry.gameObject.activeSelf) return;

			var kbutton = entry.GetComponentInChildren<KButton>();
			string speech = BuildEntrySpeech(entry);
			var section = new DetailSection();
			section.Header = (string)STRINGS.ONIACCESS.DETAILS.CURRENT_TASK;
			section.Items.Add(new ButtonWidget {
				Component = kbutton,
				GameObject = entry.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => speech
			});
			sections.Add(section);
		}

		private static void AddPriorityGroupSections(
				MinionTodoSideScreen screen, List<DetailSection> sections) {
			List<Tuple<PriorityScreen.PriorityClass, int, HierarchyReferences>> groups;
			try {
				groups = Traverse.Create(screen)
					.Field<List<Tuple<PriorityScreen.PriorityClass, int, HierarchyReferences>>>(
						"priorityGroups").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"ErrandsSideTab: priorityGroups read failed: {ex.Message}");
				return;
			}
			if (groups == null) return;

			foreach (var group in groups) {
				var refs = group.third;
				if (!refs.gameObject.activeSelf) continue;

				var container = refs.GetReference<RectTransform>("EntriesContainer");
				if (container == null || container.childCount == 0) continue;

				var title = refs.GetReference<LocText>("Title");
				string header = title != null ? title.text : "";

				var section = new DetailSection();
				section.Header = header;

				// Snapshot speech text at populate time. The game continuously
				// re-sorts MinionTodoChoreEntry objects via Apply(), recycling
				// them with new chore data even while paused. Live reads cause
				// navigation to land on different items between frames.
				// Snapshots refresh on tab switch, target change, or activation.
				for (int i = 0; i < container.childCount; i++) {
					var child = container.GetChild(i);
					if (!child.gameObject.activeSelf) continue;

					var entry = child.GetComponent<MinionTodoChoreEntry>();
					if (entry == null) continue;

					var kbutton = entry.GetComponentInChildren<KButton>();
					string speech = BuildEntrySpeech(entry);
					section.Items.Add(new ButtonWidget {
						Component = kbutton,
						GameObject = entry.gameObject,
						SuppressTooltip = true,
						SpeechFunc = () => speech
					});
				}

				if (section.Items.Count > 0)
					sections.Add(section);
			}
		}

		private static string BuildEntrySpeech(MinionTodoChoreEntry entry) {
			string label = StripNulls(entry.label.GetParsedText());
			string sub = StripNulls(entry.subLabel.GetParsedText());
			string priority = StripNulls(entry.priorityLabel.GetParsedText());
			string more = entry.moreLabel.text;

			var sb = new System.Text.StringBuilder();
			AppendNonEmpty(sb, label);
			if (!string.IsNullOrEmpty(sub) && !label.EndsWith(sub))
				AppendNonEmpty(sb, sub);
			AppendNonEmpty(sb, priority);
			AppendNonEmpty(sb, more);
			if (sb.Length > 0) sb.Append('.');

			string tooltip = GetPriorityTooltip(entry);
			if (tooltip != null) {
				sb.Append(' ');
				sb.Append(tooltip);
			}
			return sb.ToString();
		}

		private static string GetPriorityTooltip(MinionTodoChoreEntry entry) {
			var tt = entry.GetComponent<ToolTip>();
			if (tt == null) return null;
			string text = WidgetOps.ReadAllTooltipText(tt);
			if (string.IsNullOrEmpty(text)) return null;

			text = Speech.TextFilter.FilterForSpeech(text);
			int idx = text.IndexOf("Total Priority:");
			if (idx < 0) return null;
			return text.Substring(idx);
		}

		private static string StripNulls(string text) {
			if (string.IsNullOrEmpty(text)) return "";
			text = text.Replace("\0", "").Trim();
			return text;
		}

		private static void AppendNonEmpty(System.Text.StringBuilder sb, string text) {
			if (string.IsNullOrEmpty(text)) return;
			if (sb.Length > 0) sb.Append(", ");
			sb.Append(text);
		}
	}
}
