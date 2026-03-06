using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Starmap {
	/// <summary>
	/// Tab 1: Rockets. Three-level NestedMenuHandler.
	/// Level 0 = rocket list (search level).
	/// Level 1 = detail categories (Status, Checklist, Range, etc.).
	/// Level 2 = items within category.
	/// Space launches the active rocket from any level.
	/// </summary>
	internal class RocketsTab : NestedMenuHandler, IStarmapTab {
		private readonly StarmapScreenHandler _parent;

		internal RocketsTab(StarmapScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.STARMAP.ROCKETS_TAB;

		public override string DisplayName => TabName;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry>(NestedNavHelpEntries) {
			new HelpEntry("Space", STRINGS.ONIACCESS.STARMAP.LAUNCH_HELP),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

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
				SpeechPipeline.SpeakQueued(STRINGS.ONIACCESS.STARMAP.NO_ROCKETS);
			}
		}

		public void OnTabDeactivated() {
			_search.Clear();
		}

		public bool HandleInput() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)) {
				var rocket = _parent.ActiveRocket;
				string result = StarmapHelper.TryLaunch(rocket);
				SpeechPipeline.SpeakInterrupt(result);
				return true;
			}
			return base.Tick();
		}

		public new bool HandleKeyDown(KButtonEvent e) {
			return base.HandleKeyDown(e);
		}

		// ========================================
		// NestedMenuHandler abstracts
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 0;
		protected override int StartLevel => 0;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return StarmapHelper.GetSpacecraft().Count;
			var rockets = StarmapHelper.GetSpacecraft();
			if (indices[0] < 0 || indices[0] >= rockets.Count) return 0;
			var categories = StarmapHelper.BuildRocketCategories(rockets[indices[0]]);
			if (level == 1)
				return categories.Count;
			if (indices[1] < 0 || indices[1] >= categories.Count) return 0;
			return categories[indices[1]].Items.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				var rockets = StarmapHelper.GetSpacecraft();
				if (indices[0] < 0 || indices[0] >= rockets.Count) return null;
				return StarmapHelper.BuildRocketListLabel(rockets[indices[0]]);
			}
			var rocketList = StarmapHelper.GetSpacecraft();
			if (indices[0] < 0 || indices[0] >= rocketList.Count) return null;
			var categories = StarmapHelper.BuildRocketCategories(rocketList[indices[0]]);
			if (level == 1) {
				if (indices[1] < 0 || indices[1] >= categories.Count) return null;
				return categories[indices[1]].Name;
			}
			if (indices[1] < 0 || indices[1] >= categories.Count) return null;
			var cat = categories[indices[1]];
			if (indices[2] < 0 || indices[2] >= cat.Items.Count) return null;
			return cat.Items[indices[2]];
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level == 1) {
				var rockets = StarmapHelper.GetSpacecraft();
				if (indices[0] >= 0 && indices[0] < rockets.Count)
					return rockets[indices[0]].GetRocketName();
			}
			if (level == 2) {
				var rockets = StarmapHelper.GetSpacecraft();
				if (indices[0] >= 0 && indices[0] < rockets.Count) {
					var categories = StarmapHelper.BuildRocketCategories(
						rockets[indices[0]]);
					if (indices[1] >= 0 && indices[1] < categories.Count)
						return categories[indices[1]].Name;
				}
			}
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			// Detail items have no action
		}

		protected override void ActivateCurrentItem() {
			if (Level == 0) {
				var rockets = StarmapHelper.GetSpacecraft();
				if (CurrentIndex >= 0 && CurrentIndex < rockets.Count)
					_parent.SetActiveRocket(rockets[CurrentIndex]);
			}
			base.ActivateCurrentItem();
		}

		// ========================================
		// Search: rockets at level 0
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return StarmapHelper.GetSpacecraft().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var rockets = StarmapHelper.GetSpacecraft();
			if (flatIndex < 0 || flatIndex >= rockets.Count) return null;
			return StarmapHelper.BuildRocketListLabel(rockets[flatIndex]);
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			outIndices[0] = flatIndex;
		}
	}
}
