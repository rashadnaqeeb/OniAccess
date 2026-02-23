using System;
using System.Collections.Generic;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Placeholder tab that produces no widgets.
	/// Used for tabs not yet implemented in the current phase.
	/// </summary>
	class StubTab : IDetailTab {
		private readonly string _displayName;
		private readonly Func<GameObject, bool> _isAvailable;

		/// <param name="displayName">Localized tab name from game strings.</param>
		/// <param name="isAvailable">
		/// Optional predicate. When null, the tab is available for all entities.
		/// </param>
		public StubTab(string displayName, Func<GameObject, bool> isAvailable = null) {
			_displayName = displayName;
			_isAvailable = isAvailable;
		}

		public string DisplayName => _displayName;

		public bool IsAvailable(GameObject target) =>
			_isAvailable == null || _isAvailable(target);

		public void Populate(GameObject target, List<WidgetInfo> widgets) { }
	}
}
