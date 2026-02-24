using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Contract for tab readers in the details screen handler.
	/// Each implementation knows how to read one tab's UI into structured sections.
	/// </summary>
	interface IDetailTab {
		/// <summary>
		/// Localized tab name spoken on tab switch (from game's UI.DETAILTABS strings).
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Navigation level to start at when this tab is selected.
		/// 0 = root level, 1 = drilled into first child level.
		/// </summary>
		int StartLevel { get; }

		/// <summary>
		/// Game's DetailTabHeader tab ID (e.g., "SIMPLEINFO", "DETAILS").
		/// Null for side screen tabs which use a separate tab header.
		/// When non-null, the handler switches the game's visual tab to match.
		/// </summary>
		string GameTabId { get; }

		/// <summary>
		/// Whether this tab applies to the given entity.
		/// Unavailable tabs are silently skipped during Tab/Shift+Tab cycling.
		/// </summary>
		bool IsAvailable(GameObject target);

		/// <summary>
		/// Called when this tab is selected (tab switch or initial activation).
		/// Side screen tabs click their game-side MultiToggle so the game shows
		/// the correct tab body. Game tabs (GameTabId != null) leave this as a
		/// no-op since SwitchGameTab handles them via DetailTabHeader.ChangeTab.
		/// </summary>
		void OnTabSelected();

		/// <summary>
		/// Read the tab's live UI into structured sections.
		/// Called on target change, tab switch, and before keypress processing.
		/// The handler owns the list and clears it before calling.
		/// </summary>
		void Populate(GameObject target, List<DetailSection> sections);
	}
}
