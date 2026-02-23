using System.Collections.Generic;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Contract for tab readers in the details screen handler.
	/// Each implementation knows how to read one tab's UI into a widget list.
	/// </summary>
	interface IDetailTab {
		/// <summary>
		/// Localized tab name spoken on tab switch (from game's UI.DETAILTABS strings).
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Whether this tab applies to the given entity.
		/// Unavailable tabs are silently skipped during Tab/Shift+Tab cycling.
		/// </summary>
		bool IsAvailable(GameObject target);

		/// <summary>
		/// Read the tab's live UI into the widget list.
		/// Called on target change, tab switch, and before keypress processing.
		/// The handler owns the list and clears it before calling.
		/// </summary>
		void Populate(GameObject target, List<WidgetInfo> widgets);
	}
}
