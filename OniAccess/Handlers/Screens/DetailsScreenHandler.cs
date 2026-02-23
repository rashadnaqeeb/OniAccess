using System.Collections.Generic;
using UnityEngine;

using OniAccess.Handlers.Screens.Details;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the DetailsScreen (entity inspection panel).
	/// Manages tab cycling across informational and side screen tabs,
	/// delegating widget population to IDetailTab readers.
	///
	/// Lifecycle: Show-patch on DetailsScreen.OnShow(bool).
	/// The DetailsScreen is a persistent singleton that shows/hides rather than
	/// activating/deactivating, so KScreen.Activate patches skip it.
	/// </summary>
	public class DetailsScreenHandler : BaseWidgetHandler {
		private readonly IDetailTab[] _tabs;
		private readonly List<IDetailTab> _activeTabs = new List<IDetailTab>();
		private int _tabIndex;
		private GameObject _lastTarget;

		public override string DisplayName {
			get {
				var ds = DetailsScreen.Instance;
				if (ds == null || ds.target == null)
					return STRINGS.ONIACCESS.HANDLERS.DETAILS_SCREEN;
				string entityName = ds.target.GetProperName();
				if (_tabIndex >= 0 && _tabIndex < _activeTabs.Count)
					return $"{entityName}, {_activeTabs[_tabIndex].DisplayName}";
				return entityName;
			}
		}

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public DetailsScreenHandler(KScreen screen) : base(screen) {
			_tabs = BuildTabs();
			HelpEntries = BuildHelpEntries(
				new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		public override void OnActivate() {
			// Set _lastTarget before base.OnActivate to prevent double-speak:
			// base.OnActivate -> SpeakInterrupt(DisplayName) -> DiscoverWidgets
			// Then Tick()'s target-change check sees no change on the first frame.
			_lastTarget = DetailsScreen.Instance != null
				? DetailsScreen.Instance.target : null;
			RebuildActiveTabs(_lastTarget);
			_tabIndex = 0;
			base.OnActivate();
		}

		// ========================================
		// TICK: TARGET CHANGE DETECTION
		// ========================================

		public override void Tick() {
			var currentTarget = DetailsScreen.Instance != null
				? DetailsScreen.Instance.target : null;

			if (currentTarget != _lastTarget) {
				_lastTarget = currentTarget;
				if (currentTarget != null) {
					RebuildActiveTabs(currentTarget);

					// Reset tab to first available (typically Status/Properties)
					_tabIndex = 0;

					DiscoverWidgets(_screen);
					_currentIndex = 0;

					Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
					if (_widgets.Count > 0)
						Speech.SpeechPipeline.SpeakQueued(
							GetWidgetSpeechText(_widgets[0]));
				}
			}

			base.Tick();
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();
			var ds = DetailsScreen.Instance;
			if (ds == null || ds.target == null) return true;
			if (_tabIndex < 0 || _tabIndex >= _activeTabs.Count) return true;

			try {
				_activeTabs[_tabIndex].Populate(ds.target, _widgets);
			} catch (System.Exception ex) {
				Util.Log.Error(
					$"DetailsScreenHandler: tab '{_activeTabs[_tabIndex].DisplayName}' " +
					$"Populate failed: {ex}");
			}

			return true;
		}

		// ========================================
		// TAB CYCLING
		// ========================================

		protected override void NavigateTabForward() {
			AdvanceTab(1);
		}

		protected override void NavigateTabBackward() {
			AdvanceTab(-1);
		}

		private void AdvanceTab(int direction) {
			if (_activeTabs.Count <= 1) return;

			int oldIndex = _tabIndex;

			// Save cursor GO for stability
			GameObject prevGO = (_currentIndex >= 0 && _currentIndex < _widgets.Count)
				? _widgets[_currentIndex].GameObject : null;

			_tabIndex = ((_tabIndex + direction) % _activeTabs.Count + _activeTabs.Count)
				% _activeTabs.Count;

			bool wrapped = direction > 0
				? _tabIndex <= oldIndex
				: _tabIndex >= oldIndex;

			DiscoverWidgets(_screen);

			// Restore cursor: find prevGO in new list
			_currentIndex = 0;
			if (prevGO != null) {
				for (int i = 0; i < _widgets.Count; i++) {
					if (_widgets[i].GameObject == prevGO) {
						_currentIndex = i;
						break;
					}
				}
			}

			if (wrapped) PlayWrapSound();
			else PlayHoverSound();

			Speech.SpeechPipeline.SpeakInterrupt(_activeTabs[_tabIndex].DisplayName);
			if (_widgets.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(
					GetWidgetSpeechText(_widgets[_currentIndex]));
		}

		// ========================================
		// TAB MANAGEMENT
		// ========================================

		private void RebuildActiveTabs(GameObject target) {
			_activeTabs.Clear();
			if (target == null) return;

			foreach (var tab in _tabs) {
				if (tab.IsAvailable(target))
					_activeTabs.Add(tab);
			}
		}

		private static IDetailTab[] BuildTabs() {
			return new IDetailTab[] {
				// Main info tabs (match game's DetailTabHeader order)
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.SIMPLEINFO.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.PERSONALITY.NAME,
					t => t.GetComponent<MinionIdentity>() != null),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME,
					t => t.GetComponent<Chore>() == null
						&& t.GetComponent<MinionIdentity>() == null
						&& t.GetComponent<BuildingComplete>() != null),
				new PropertiesTab(),

				// Side screen tabs
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.CONFIGURATION.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME,
					t => t.GetComponent<MinionIdentity>() != null),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.MATERIAL.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.COSMETICS.NAME),
			};
		}
	}
}
