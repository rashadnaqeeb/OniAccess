using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Starmap {
	/// <summary>
	/// Tab 3: Destination Details. Flat BaseMenuHandler showing all details
	/// for the selected destination. Last item is the analyze/suspend action.
	/// Rebuilt when the selected destination changes.
	/// </summary>
	internal class DestinationDetailsTab : BaseMenuHandler, IStarmapTab {
		private readonly StarmapScreenHandler _parent;
		private List<string> _items;

		internal DestinationDetailsTab(StarmapScreenHandler parent)
				: base(screen: null) {
			_parent = parent;
		}

		public string TabName =>
			(string)STRINGS.ONIACCESS.STARMAP.DETAILS_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(MenuHelpEntries) {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.STARMAP.ANALYZE_HELP),
			}.AsReadOnly();

		// ========================================
		// IStarmapTab
		// ========================================

		public void OnTabActivated(bool announce) {
			Rebuild();
			CurrentIndex = 0;
			_search.Clear();
			SuppressSearchThisFrame();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(CurrentIndex));
			else
				SpeechPipeline.SpeakQueued(
					STRINGS.ONIACCESS.STARMAP.NO_DESTINATION_SELECTED);
		}

		public void OnTabDeactivated() {
			_search.Clear();
		}

		public bool HandleInput() {
			return base.Tick();
		}

		public new bool HandleKeyDown(KButtonEvent e) {
			return base.HandleKeyDown(e);
		}

		// ========================================
		// BaseMenuHandler
		// ========================================

		public override int ItemCount =>
			_items != null ? _items.Count : 0;

		public override string GetItemLabel(int index) {
			if (_items == null || index < 0 || index >= _items.Count)
				return null;
			// Re-query the analyze action (last item) live to avoid stale state
			if (index == _items.Count - 1) {
				var dest = _parent.SelectedDestination;
				if (dest != null)
					return StarmapHelper.GetAnalyzeActionLabel(dest);
			}
			return _items[index];
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			string label = GetItemLabel(CurrentIndex);
			if (string.IsNullOrEmpty(label)) return;
			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;
			SpeechPipeline.SpeakInterrupt(label);
		}

		protected override void ActivateCurrentItem() {
			var dest = _parent.SelectedDestination;
			if (dest == null) return;

			// Only the last item is the analyze action
			if (_items == null || CurrentIndex != _items.Count - 1) return;

			if (StarmapHelper.IsAnalyzed(dest)) {
				// Analysis complete, no action
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.UI.STARMAP.ANALYSIS_COMPLETE);
				return;
			}

			int currentTarget = SpacecraftManager.instance
				.GetStarmapAnalysisDestinationID();
			if (currentTarget == dest.id) {
				// Currently analyzing — suspend
				SpacecraftManager.instance.SetStarmapAnalysisDestinationID(-1);
				StarmapHelper.PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(
					STRINGS.ONIACCESS.STARMAP.ANALYSIS_SUSPENDED);
			} else {
				// Start analyzing
				SpacecraftManager.instance
					.SetStarmapAnalysisDestinationID(dest.id);
				StarmapHelper.PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(
					STRINGS.ONIACCESS.STARMAP.ANALYSIS_STARTED);
			}

			// Rebuild to reflect new state
			int savedIndex = CurrentIndex;
			Rebuild();
			CurrentIndex = System.Math.Min(savedIndex, ItemCount - 1);
		}

		// ========================================
		// INTERNAL
		// ========================================

		internal void OnDestinationChanged() {
			Rebuild();
			CurrentIndex = 0;
		}

		private void Rebuild() {
			var dest = _parent.SelectedDestination;
			if (dest == null) {
				_items = null;
				return;
			}
			_items = StarmapHelper.BuildDestinationDetails(
				dest, _parent.ActiveRocket);
		}
	}
}
