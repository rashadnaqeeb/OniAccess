using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Starmap {
	/// <summary>
	/// Tab 3: Destination Details. Two-level NestedMenuHandler.
	/// Level 0 = sections (identity, analysis, research, mass,
	///           composition, resources, artifacts) plus analyze action.
	/// Level 1 = items within section.
	/// The analyze action is a leaf at level 0 (empty Items).
	/// </summary>
	internal class DestinationDetailsTab : NestedMenuHandler, IStarmapTab {
		private readonly StarmapScreenHandler _parent;

		internal DestinationDetailsTab(StarmapScreenHandler parent)
				: base(screen: null) {
			_parent = parent;
		}

		public string TabName =>
			(string)STRINGS.ONIACCESS.STARMAP.DETAILS_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries => NestedNavHelpEntries;

		// ========================================
		// IStarmapTab
		// ========================================

		public void OnTabActivated(bool announce) {
			ResetState();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0) {
				string label = GetItemLabel(CurrentIndex);
				if (!string.IsNullOrEmpty(label))
					SpeechPipeline.SpeakQueued(label);
			} else {
				SpeechPipeline.SpeakQueued(
					STRINGS.ONIACCESS.STARMAP.NO_DESTINATION_SELECTED);
			}
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

		internal void OnDestinationChanged() {
			ResetState();
		}

		// ========================================
		// NestedMenuHandler abstracts
		// ========================================

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 0;
		protected override int StartLevel => 0;

		protected override int GetItemCount(int level, int[] indices) {
			var dest = _parent.SelectedDestination;
			if (dest == null) return 0;
			var sections = StarmapHelper.BuildDestinationSections(
				dest, _parent.ActiveRocket);
			if (level == 0)
				return sections.Count;
			if (indices[0] < 0 || indices[0] >= sections.Count) return 0;
			return sections[indices[0]].Items.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			var dest = _parent.SelectedDestination;
			if (dest == null) return null;
			var sections = StarmapHelper.BuildDestinationSections(
				dest, _parent.ActiveRocket);
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= sections.Count) return null;
				// Re-query the action label live (last item)
				if (indices[0] == sections.Count - 1)
					return StarmapHelper.GetAnalyzeActionLabel(dest);
				return sections[indices[0]].Name;
			}
			if (indices[0] < 0 || indices[0] >= sections.Count) return null;
			var section = sections[indices[0]];
			if (indices[1] < 0 || indices[1] >= section.Items.Count) return null;
			return section.Items[indices[1]];
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) {
				var dest = _parent.SelectedDestination;
				if (dest == null) return null;
				var sections = StarmapHelper.BuildDestinationSections(
					dest, _parent.ActiveRocket);
				if (indices[0] >= 0 && indices[0] < sections.Count)
					return sections[indices[0]].Name;
			}
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			var dest = _parent.SelectedDestination;
			if (dest == null) return;

			if (StarmapHelper.IsAnalyzed(dest)) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.UI.STARMAP.ANALYSIS_COMPLETE);
				return;
			}

			int currentTarget = SpacecraftManager.instance
				.GetStarmapAnalysisDestinationID();
			if (currentTarget == dest.id) {
				SpacecraftManager.instance.SetStarmapAnalysisDestinationID(-1);
				StarmapHelper.PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(
					STRINGS.ONIACCESS.STARMAP.ANALYSIS_SUSPENDED);
			} else {
				SpacecraftManager.instance
					.SetStarmapAnalysisDestinationID(dest.id);
				StarmapHelper.PlaySound("HUD_Click");
				SpeechPipeline.SpeakInterrupt(
					STRINGS.ONIACCESS.STARMAP.ANALYSIS_STARTED);
			}
		}

		// ========================================
		// Search: section names at level 0
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			var dest = _parent.SelectedDestination;
			if (dest == null) return 0;
			return StarmapHelper.BuildDestinationSections(
				dest, _parent.ActiveRocket).Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var dest = _parent.SelectedDestination;
			if (dest == null) return null;
			var sections = StarmapHelper.BuildDestinationSections(
				dest, _parent.ActiveRocket);
			if (flatIndex < 0 || flatIndex >= sections.Count) return null;
			return sections[flatIndex].Name;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			outIndices[0] = flatIndex;
		}
	}
}
