using System.Collections.Generic;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads active Errands side screens (dupe task list) into sections.
	/// Mirrors ConfigSideTab for the SidescreenTabTypes.Errands tab.
	/// </summary>
	class ErrandsSideTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME;
		public int StartLevel => 0;
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

			foreach (var screen in ConfigSideTab.GetActiveScreens(
					ds, DetailsScreen.SidescreenTabTypes.Errands)) {
				var section = new DetailSection();
				section.Header = screen.GetTitle();
				SideScreenWalker.Walk(screen, section.Items);
				if (section.Items.Count > 0)
					sections.Add(section);
			}
		}
	}
}
