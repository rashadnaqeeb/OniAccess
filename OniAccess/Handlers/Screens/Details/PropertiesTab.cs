using System;
using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;
using OniAccess.Widgets;
using UnityEngine;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the AdditionalDetailsPanel (Properties tab) into structured sections.
	/// Eight CollapsibleDetailContentPanel sections, each with a header and DetailLabel children.
	/// All widgets use SpeechFunc for live text since the game updates labels every frame.
	/// </summary>
	class PropertiesTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.DETAILS.NAME;
		public int StartLevel => 0;
		public string GameTabId => "DETAILS";

		public bool IsAvailable(GameObject target) => true;

		public void OnTabSelected() { }

		private static readonly string[] SectionFields = {
			"detailsPanel",
			"immuneSystemPanel",
			"diseaseSourcePanel",
			"currentGermsPanel",
			"overviewPanel",
			"generatorsPanel",
			"consumersPanel",
			"batteriesPanel",
		};

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) {
				Util.Log.Warn("PropertiesTab.Populate: AdditionalDetailsPanel not found");
				return;
			}

			// The handler switches the game's visual tab before calling Populate,
			// so the panel is already active with SetTarget called by the game.
			// Guard against edge cases where the panel hasn't been refreshed yet.
			if (panel.gameObject.activeSelf)
				panel.SetTarget(target);

			foreach (var fieldName in SectionFields) {
				CollapsibleDetailContentPanel gameSection;
				try {
					gameSection = Traverse.Create(panel)
						.Field<CollapsibleDetailContentPanel>(fieldName).Value;
				} catch (System.Exception ex) {
					Util.Log.Warn($"PropertiesTab: field '{fieldName}' read failed: {ex.Message}");
					continue;
				}

				if (gameSection == null || !gameSection.gameObject.activeSelf) continue;

				var section = CollapsiblePanelReader.BuildSection(gameSection);
				section.Key = fieldName;
				if (section.Items.Count > 0)
					sections.Add(section);
			}

			AppendRangeWidget(target, sections);
		}

		private static void AppendRangeWidget(GameObject target, List<DetailSection> sections) {
			var rangeVis = target.GetComponent<RangeVisualizer>();
			if (rangeVis == null) return;

			var detailsSection = sections.Find(s => s.Key == "detailsPanel");
			if (detailsSection == null) {
				detailsSection = new DetailSection { Key = "range", Header = "Range" };
				sections.Add(detailsSection);
			}

			detailsSection.Items.Add(new LabelWidget {
				Key = "range",
				SpeechFunc = () => FormatRange(target, rangeVis)
			});
		}

		private static string FormatRange(GameObject target, RangeVisualizer rangeVis) {
			Grid.PosToXY(target.transform.GetPosition(), out int bx, out int by);

			var origin = rangeVis.OriginOffset;
			var rMin = rangeVis.RangeMin;
			var rMax = rangeVis.RangeMax;

			if (target.TryGetComponent<Rotatable>(out var rot)) {
				origin = rot.GetRotatedOffset(origin);
				var a = rot.GetRotatedOffset(rMin);
				var b = rot.GetRotatedOffset(rMax);
				rMin = new Vector2I(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
				rMax = new Vector2I(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
			}

			int blX = bx + origin.x + rMin.x;
			int blY = by + origin.y + rMin.y;
			int trX = bx + origin.x + rMax.x;
			int trY = by + origin.y + rMax.y;

			return string.Format((string)STRINGS.ONIACCESS.DETAILS.RANGE,
				GridCoordinates.Format(blX, blY),
				GridCoordinates.Format(trX, trY));
		}

		private static AdditionalDetailsPanel FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;

			var tabHeader = Traverse.Create(ds)
				.Field<DetailTabHeader>("tabHeader").Value;
			if (tabHeader == null) return null;

			var tabPanels = Traverse.Create(tabHeader)
				.Field<Dictionary<string, TargetPanel>>("tabPanels").Value;
			if (tabPanels == null || !tabPanels.TryGetValue("DETAILS", out var panel))
				return null;

			return panel as AdditionalDetailsPanel;
		}

	}
}
