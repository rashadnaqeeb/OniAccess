using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Starmap {
	/// <summary>
	/// Tab 2: Destinations. Two-level NestedMenuHandler.
	/// Level 0 = pre-filtered non-empty distance tiers.
	/// Level 1 = destinations within the tier.
	/// Enter selects destination, assigns to active rocket if grounded,
	/// and auto-switches to Tab 3 (destination details).
	/// </summary>
	internal class DestinationsTab : NestedMenuHandler, IStarmapTab {
		private readonly StarmapScreenHandler _parent;

		internal DestinationsTab(StarmapScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName {
			get {
				string name = (string)STRINGS.ONIACCESS.STARMAP.DESTINATIONS_TAB;
				var rocket = _parent.ActiveRocket;
				if (rocket != null
						&& rocket.state == Spacecraft.MissionState.Grounded)
					return string.Format(
						STRINGS.ONIACCESS.STARMAP.DESTINATIONS_TAB_WITH_ROCKET,
						rocket.GetRocketName());
				return name;
			}
		}

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

		// ========================================
		// NestedMenuHandler abstracts
		// ========================================

		protected override int MaxLevel => 1;
		protected override int SearchLevel => 1;
		protected override int StartLevel => 1;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return StarmapHelper.GetPopulatedDistanceTiers().Count;
			var tiers = StarmapHelper.GetPopulatedDistanceTiers();
			if (indices[0] < 0 || indices[0] >= tiers.Count) return 0;
			return StarmapHelper.GetDestinationsAtTier(tiers[indices[0]]).Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			var tiers = StarmapHelper.GetPopulatedDistanceTiers();
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= tiers.Count) return null;
				return StarmapHelper.GetTierLabel(tiers[indices[0]]);
			}
			if (indices[0] < 0 || indices[0] >= tiers.Count) return null;
			var dests = StarmapHelper.GetDestinationsAtTier(tiers[indices[0]]);
			if (indices[1] < 0 || indices[1] >= dests.Count) return null;
			return StarmapHelper.GetDestinationLabel(dests[indices[1]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level >= 1) {
				var tiers = StarmapHelper.GetPopulatedDistanceTiers();
				if (indices[0] >= 0 && indices[0] < tiers.Count)
					return StarmapHelper.GetTierLabel(tiers[indices[0]]);
			}
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			var tiers = StarmapHelper.GetPopulatedDistanceTiers();
			if (indices[0] < 0 || indices[0] >= tiers.Count) return;
			var dests = StarmapHelper.GetDestinationsAtTier(tiers[indices[0]]);
			if (indices[1] < 0 || indices[1] >= dests.Count) return;

			var dest = dests[indices[1]];
			_parent.SelectDestination(dest);

			// Assign to active rocket if grounded
			var rocket = _parent.ActiveRocket;
			if (rocket != null
					&& rocket.state == Spacecraft.MissionState.Grounded) {
				SpacecraftManager.instance.SetSpacecraftDestination(
					rocket.launchConditions, dest);
				string destName = StarmapHelper.IsAnalyzed(dest)
					? dest.GetDestinationType().Name
					: (string)STRINGS.UI.STARMAP.UNKNOWN_DESTINATION;
				SpeechPipeline.SpeakInterrupt(string.Format(
					STRINGS.ONIACCESS.STARMAP.DESTINATION_ASSIGNED,
					destName, rocket.GetRocketName()));
			} else {
				string destName = StarmapHelper.IsAnalyzed(dest)
					? dest.GetDestinationType().Name
					: (string)STRINGS.UI.STARMAP.UNKNOWN_DESTINATION;
				SpeechPipeline.SpeakInterrupt(destName);
			}

			// Auto-switch to destination details tab
			_parent.JumpToDetailsTab();
		}

		// ========================================
		// Search across all destinations
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return StarmapHelper.GetAllDestinations().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var all = StarmapHelper.GetAllDestinations();
			if (flatIndex < 0 || flatIndex >= all.Count) return null;
			return StarmapHelper.GetDestinationLabel(all[flatIndex]);
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			var all = StarmapHelper.GetAllDestinations();
			if (flatIndex < 0 || flatIndex >= all.Count) return;
			var dest = all[flatIndex];

			var tiers = StarmapHelper.GetPopulatedDistanceTiers();
			for (int t = 0; t < tiers.Count; t++) {
				if (tiers[t] == dest.OneBasedDistance) {
					var dests = StarmapHelper.GetDestinationsAtTier(tiers[t]);
					for (int d = 0; d < dests.Count; d++) {
						if (dests[d].id == dest.id) {
							outIndices[0] = t;
							outIndices[1] = d;
							return;
						}
					}
				}
			}
			Util.Log.Warn($"DestinationsTab.MapSearchIndex: destination '{dest.id}' not found in tiers");
		}

	}
}
