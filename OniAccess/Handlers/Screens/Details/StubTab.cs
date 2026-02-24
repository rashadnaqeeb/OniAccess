using System;
using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Placeholder tab that produces no widgets.
	/// Used for tabs not yet implemented in the current phase.
	/// </summary>
	class StubTab: IDetailTab {
		private readonly string _displayName;
		private readonly string _gameTabId;
		private readonly Func<GameObject, bool> _isAvailable;

		/// <param name="displayName">Localized tab name from game strings.</param>
		/// <param name="gameTabId">
		/// Game's DetailTabHeader tab ID (e.g., "SIMPLEINFO"). Null for side screen tabs.
		/// </param>
		/// <param name="isAvailable">
		/// Optional predicate. When null, the tab is available for all entities.
		/// </param>
		public StubTab(string displayName, string gameTabId = null,
				Func<GameObject, bool> isAvailable = null) {
			_displayName = displayName;
			_gameTabId = gameTabId;
			_isAvailable = isAvailable;
		}

		public string DisplayName => _displayName;
		public int StartLevel => 0;
		public string GameTabId => _gameTabId;

		public bool IsAvailable(GameObject target) =>
			_isAvailable == null || _isAvailable(target);

		public void OnTabSelected() { }

		public void Populate(GameObject target, List<DetailSection> sections) { }
	}
}
