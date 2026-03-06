using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Starmap {
	/// <summary>
	/// Tab 1: Rockets. Dual-view (list/detail) BaseMenuHandler.
	/// List view shows all spacecraft. Detail view shows full rocket info.
	/// Enter in list selects rocket as active and shows detail.
	/// Escape in detail returns to list. Space launches.
	/// </summary>
	internal class RocketsTab : BaseMenuHandler, IStarmapTab {
		private readonly StarmapScreenHandler _parent;
		private bool _inDetail;
		private List<StarmapHelper.DetailItem> _detailItems;

		internal RocketsTab(StarmapScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.STARMAP.ROCKETS_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry>(MenuHelpEntries) {
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
				new HelpEntry("Space", STRINGS.ONIACCESS.STARMAP.LAUNCH_HELP),
			}.AsReadOnly();

		// ========================================
		// IStarmapTab
		// ========================================

		public void OnTabActivated(bool announce) {
			_inDetail = false;
			_detailItems = null;
			CurrentIndex = 0;
			_search.Clear();
			SuppressSearchThisFrame();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(CurrentIndex));
			else
				SpeechPipeline.SpeakQueued(STRINGS.ONIACCESS.STARMAP.NO_ROCKETS);
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
			if (base.HandleKeyDown(e)) return true;

			if (_inDetail && e.TryConsume(global::Action.Escape)) {
				ReturnToList();
				return true;
			}

			return false;
		}

		// ========================================
		// BaseMenuHandler
		// ========================================

		public override int ItemCount {
			get {
				if (_inDetail)
					return _detailItems != null ? _detailItems.Count : 0;
				return StarmapHelper.GetSpacecraft().Count;
			}
		}

		public override string GetItemLabel(int index) {
			if (_inDetail) {
				if (_detailItems == null || index < 0 || index >= _detailItems.Count)
					return null;
				return _detailItems[index].Label;
			}
			var rockets = StarmapHelper.GetSpacecraft();
			if (index < 0 || index >= rockets.Count) return null;
			return StarmapHelper.BuildRocketListLabel(rockets[index]);
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			string label = GetItemLabel(CurrentIndex);
			if (string.IsNullOrEmpty(label)) return;
			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;
			SpeechPipeline.SpeakInterrupt(label);
		}

		protected override void ActivateCurrentItem() {
			if (_inDetail) return;

			var rockets = StarmapHelper.GetSpacecraft();
			if (CurrentIndex < 0 || CurrentIndex >= rockets.Count) return;

			var rocket = rockets[CurrentIndex];
			_parent.SetActiveRocket(rocket);

			_inDetail = true;
			_detailItems = StarmapHelper.BuildRocketDetails(rocket);
			CurrentIndex = 0;

			SpeechPipeline.SpeakInterrupt(
				string.Format(STRINGS.ONIACCESS.STARMAP.ROCKET_DETAIL_HEADER,
					rocket.GetRocketName()));
			if (_detailItems.Count > 0)
				SpeechPipeline.SpeakQueued(_detailItems[0].Label);
		}

		// ========================================
		// PRIVATE
		// ========================================

		private void ReturnToList() {
			_inDetail = false;
			_detailItems = null;

			// Try to position on the previously selected rocket
			var activeRocket = _parent.ActiveRocket;
			var rockets = StarmapHelper.GetSpacecraft();
			CurrentIndex = 0;
			if (activeRocket != null) {
				for (int i = 0; i < rockets.Count; i++) {
					if (rockets[i].id == activeRocket.id) {
						CurrentIndex = i;
						break;
					}
				}
			}

			SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(CurrentIndex));
		}
	}
}
