using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Handlers.Screens.Details;
using OniAccess.Speech;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the DetailsScreen (entity inspection panel).
	/// Two-level nested navigation: section headers (level 0) and items within
	/// each section (level 1). Manages tab cycling across informational and
	/// side screen tabs, delegating section population to IDetailTab readers.
	///
	/// Lifecycle: Show-patch on DetailsScreen.OnShow(bool).
	/// The DetailsScreen is a persistent singleton that shows/hides rather than
	/// activating/deactivating, so KScreen.Activate patches skip it.
	/// </summary>
	public class DetailsScreenHandler: NestedMenuHandler {
		private readonly IDetailTab[] _tabs;
		private readonly List<IDetailTab> _activeTabs = new List<IDetailTab>();
		private readonly List<DetailSection> _sections = new List<DetailSection>();
		private int _tabIndex;
		private GameObject _lastTarget;
		private bool _tabSwitching;

		public override string DisplayName {
			get {
				if (_tabSwitching) return null;
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
			var list = new List<HelpEntry>();
			list.AddRange(NestedNavHelpEntries);
			list.Add(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
			HelpEntries = list.AsReadOnly();
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => 1;
		protected override int SearchLevel => Level;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0) return _sections.Count;
			if (indices[0] < 0 || indices[0] >= _sections.Count) return 0;
			return _sections[indices[0]].Items.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
				return _sections[indices[0]].Header;
			}
			if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
			var items = _sections[indices[0]].Items;
			if (indices[1] < 0 || indices[1] >= items.Count) return null;
			return WidgetOps.GetSpeechText(items[indices[1]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level < 1) return null;
			if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
			return _sections[indices[0]].Header;
		}

		protected override void ActivateLeafItem(int[] indices) { }

		protected override int GetSearchItemCount(int[] indices) {
			if (Level == 0) return _sections.Count;
			int total = 0;
			for (int s = 0; s < _sections.Count; s++)
				total += _sections[s].Items.Count;
			return total;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			if (Level == 0) {
				if (flatIndex < 0 || flatIndex >= _sections.Count) return null;
				return _sections[flatIndex].Header;
			}
			int remaining = flatIndex;
			for (int s = 0; s < _sections.Count; s++) {
				int count = _sections[s].Items.Count;
				if (remaining < count)
					return WidgetOps.GetSpeechText(_sections[s].Items[remaining]);
				remaining -= count;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			if (Level == 0) {
				outIndices[0] = flatIndex;
				return;
			}
			int remaining = flatIndex;
			for (int s = 0; s < _sections.Count; s++) {
				int count = _sections[s].Items.Count;
				if (remaining < count) {
					outIndices[0] = s;
					outIndices[1] = remaining;
					return;
				}
				remaining -= count;
			}
		}

		// ========================================
		// SPEECH
		// ========================================

		public override void SpeakCurrentItem() {
			if (Level == 0) {
				base.SpeakCurrentItem();
				return;
			}

			int sIdx = GetIndex(0);
			int iIdx = GetIndex(1);
			if (sIdx < 0 || sIdx >= _sections.Count) return;
			var items = _sections[sIdx].Items;
			if (iIdx < 0 || iIdx >= items.Count) return;

			var w = items[iIdx];
			string text = WidgetOps.GetSpeechText(w);
			string tip = WidgetOps.GetTooltipText(w);
			if (tip != null) text = $"{text}, {tip}";
			if (!string.IsNullOrEmpty(text))
				SpeechPipeline.SpeakInterrupt(text);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			_lastTarget = DetailsScreen.Instance != null
				? DetailsScreen.Instance.target : null;
			RebuildActiveTabs(_lastTarget);
			_tabIndex = 0;
			SwitchGameTab();
			RebuildSections();
			base.OnActivate();

			SpeakFirstSection();
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
					_tabIndex = 0;
					SwitchGameTab();
					RebuildSections();
					ResetNavigation();

					SpeechPipeline.SpeakInterrupt(DisplayName);
					SpeakFirstSection();
				}
			}

			base.Tick();
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
			_tabIndex = ((_tabIndex + direction) % _activeTabs.Count + _activeTabs.Count)
				% _activeTabs.Count;

			bool wrapped = direction > 0
				? _tabIndex <= oldIndex
				: _tabIndex >= oldIndex;

			SwitchGameTab();
			RebuildSections();
			ResetNavigation();

			if (wrapped) PlayWrapSound();
			else PlayHoverSound();

			SpeechPipeline.SpeakInterrupt(_activeTabs[_tabIndex].DisplayName);
			SpeakFirstSection();
		}

		// ========================================
		// SECTION MANAGEMENT
		// ========================================

		private void RebuildSections() {
			_sections.Clear();
			var ds = DetailsScreen.Instance;
			if (ds == null || ds.target == null) return;
			if (_tabIndex < 0 || _tabIndex >= _activeTabs.Count) return;

			try {
				_activeTabs[_tabIndex].Populate(ds.target, _sections);
			} catch (System.Exception ex) {
				Util.Log.Error(
					$"DetailsScreenHandler: tab '{_activeTabs[_tabIndex].DisplayName}' " +
					$"Populate failed: {ex}");
			}
		}

		// ========================================
		// TAB MANAGEMENT
		// ========================================

		private void RebuildActiveTabs(GameObject target) {
			_activeTabs.Clear();
			if (target == null) return;

			Dictionary<string, MultiToggle> gameTabs = null;
			var ds = DetailsScreen.Instance;
			if (ds != null) {
				var tabHeader = Traverse.Create(ds)
					.Field<DetailTabHeader>("tabHeader").Value;
				if (tabHeader != null)
					gameTabs = Traverse.Create(tabHeader)
						.Field<Dictionary<string, MultiToggle>>("tabs").Value;
			}

			foreach (var tab in _tabs) {
				if (tab.GameTabId != null && gameTabs != null) {
					if (gameTabs.TryGetValue(tab.GameTabId, out var toggle)
							&& !toggle.gameObject.activeSelf)
						continue;
				} else if (!tab.IsAvailable(target)) {
					continue;
				}
				_activeTabs.Add(tab);
			}
		}

		/// <summary>
		/// Switch the game's visual tab to match our logical tab.
		/// </summary>
		private void SwitchGameTab() {
			if (_tabIndex < 0 || _tabIndex >= _activeTabs.Count) return;

			var gameTabId = _activeTabs[_tabIndex].GameTabId;
			if (gameTabId == null) return;

			var ds = DetailsScreen.Instance;
			if (ds == null) return;

			var tabHeader = Traverse.Create(ds)
				.Field<DetailTabHeader>("tabHeader").Value;
			if (tabHeader == null) return;

			Traverse.Create(tabHeader).Method("ChangeTab", gameTabId).GetValue();
		}

		// ========================================
		// PRIVATE HELPERS
		// ========================================

		/// <summary>
		/// Reset NestedMenuHandler's private navigation state by cycling
		/// through OnDeactivate/OnActivate. Suppresses speech during reset.
		/// </summary>
		private void ResetNavigation() {
			_tabSwitching = true;
			base.OnDeactivate();
			base.OnActivate();
			_tabSwitching = false;
		}

		private void SpeakFirstSection() {
			if (_sections.Count > 0 && !string.IsNullOrEmpty(_sections[0].Header))
				SpeechPipeline.SpeakQueued(_sections[0].Header);
		}

		// ========================================
		// TAB CONFIGURATION
		// ========================================

		private static IDetailTab[] BuildTabs() {
			return new IDetailTab[] {
				// Main info tabs (match game's DetailTabHeader order).
				// Availability is determined by the game's tab toggle visibility,
				// not hardcoded predicates — see RebuildActiveTabs.
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.SIMPLEINFO.NAME, "SIMPLEINFO"),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.PERSONALITY.NAME, "PERSONALITY"),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME, "BUILDINGCHORES"),
				new PropertiesTab(),

				// Side screen tabs (null gameTabId — use sidescreenTabHeader when implemented).
				// TODO: query SidescreenTab.IsVisible instead of hardcoded predicates.
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.CONFIGURATION.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.MATERIAL.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.COSMETICS.NAME),
			};
		}
	}
}
