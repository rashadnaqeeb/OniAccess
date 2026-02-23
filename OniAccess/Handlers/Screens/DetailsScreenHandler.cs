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
		private readonly Input.TextEditHelper _textEdit = new Input.TextEditHelper();
		private int _tabIndex;
		private GameObject _lastTarget;
		private bool _tabSwitching;
		private bool _pendingFirstSection;
		private bool _pendingActivationSpeech;

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

		protected override int MaxLevel => 2;
		protected override int SearchLevel => Level;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0) return _sections.Count;
			if (indices[0] < 0 || indices[0] >= _sections.Count) return 0;
			var items = _sections[indices[0]].Items;
			if (level == 1) return items.Count;
			if (indices[1] < 0 || indices[1] >= items.Count) return 0;
			return items[indices[1]].Children?.Count ?? 0;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
				return _sections[indices[0]].Header;
			}
			if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
			var items = _sections[indices[0]].Items;
			if (level == 1) {
				if (indices[1] < 0 || indices[1] >= items.Count) return null;
				return WidgetOps.GetSpeechText(items[indices[1]]);
			}
			if (indices[1] < 0 || indices[1] >= items.Count) return null;
			var children = items[indices[1]].Children;
			if (children == null || indices[2] < 0 || indices[2] >= children.Count) return null;
			return WidgetOps.GetSpeechText(children[indices[2]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level < 1) return null;
			if (indices[0] < 0 || indices[0] >= _sections.Count) return null;
			if (level == 1) return _sections[indices[0]].Header;
			var items = _sections[indices[0]].Items;
			if (indices[1] < 0 || indices[1] >= items.Count) return null;
			return WidgetOps.GetSpeechText(items[indices[1]]);
		}

		protected override void ActivateLeafItem(int[] indices) {
			var w = GetWidgetAt(indices);
			if (w == null) return;

			switch (w.Type) {
				case WidgetType.Button: {
						var btn = w.Component as KButton;
						if (btn != null) {
							WidgetOps.ClickButton(btn);
							RebuildSections();
							return;
						}
						var mt = w.Component as MultiToggle;
						if (mt != null) {
							WidgetOps.ClickMultiToggle(mt);
							RebuildSections();
						}
						break;
					}
				case WidgetType.Toggle: {
						var toggle = w.Component as KToggle;
						if (toggle != null) {
							toggle.Click();
							_pendingActivationSpeech = true;
							return;
						}
						var mt = w.Component as MultiToggle;
						if (mt != null) {
							WidgetOps.ClickMultiToggle(mt);
							_pendingActivationSpeech = true;
						}
						break;
					}
				case WidgetType.Slider:
					SpeakCurrentItem();
					break;
				case WidgetType.TextInput: {
						KInputTextField field = null;
						var knum = w.Component as KNumberInputField;
						if (knum != null)
							field = knum.field;
						else {
							var kinput = w.Component as KInputField;
							if (kinput != null) field = kinput.field;
						}
						if (field != null) {
							if (!_textEdit.IsEditing)
								_textEdit.Begin(field);
							else
								_textEdit.Confirm();
						}
						break;
					}
			}
		}

		protected override int GetSearchItemCount(int[] indices) {
			if (Level == 0) return _sections.Count;
			if (Level == 1) {
				int total = 0;
				for (int s = 0; s < _sections.Count; s++)
					total += _sections[s].Items.Count;
				return total;
			}
			// Level 2: flat count of all children in current section
			int sIdx = indices[0];
			if (sIdx < 0 || sIdx >= _sections.Count) return 0;
			var sectionItems = _sections[sIdx].Items;
			int childTotal = 0;
			for (int i = 0; i < sectionItems.Count; i++)
				childTotal += sectionItems[i].Children?.Count ?? 0;
			return childTotal;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			if (Level == 0) {
				if (flatIndex < 0 || flatIndex >= _sections.Count) return null;
				return _sections[flatIndex].Header;
			}
			if (Level == 1) {
				int remaining = flatIndex;
				for (int s = 0; s < _sections.Count; s++) {
					int count = _sections[s].Items.Count;
					if (remaining < count)
						return WidgetOps.GetSpeechText(_sections[s].Items[remaining]);
					remaining -= count;
				}
				return null;
			}
			// Level 2: search within current section's children
			int sIdx = GetIndex(0);
			if (sIdx < 0 || sIdx >= _sections.Count) return null;
			var items = _sections[sIdx].Items;
			int rem = flatIndex;
			for (int i = 0; i < items.Count; i++) {
				var children = items[i].Children;
				int cc = children?.Count ?? 0;
				if (rem < cc)
					return WidgetOps.GetSpeechText(children[rem]);
				rem -= cc;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			if (Level == 0) {
				outIndices[0] = flatIndex;
				return;
			}
			if (Level == 1) {
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
				return;
			}
			// Level 2
			int sIdx = outIndices[0];
			if (sIdx < 0 || sIdx >= _sections.Count) return;
			var items = _sections[sIdx].Items;
			int rem = flatIndex;
			for (int i = 0; i < items.Count; i++) {
				var children = items[i].Children;
				int cc = children?.Count ?? 0;
				if (rem < cc) {
					outIndices[1] = i;
					outIndices[2] = rem;
					return;
				}
				rem -= cc;
			}
		}

		// ========================================
		// LEFT/RIGHT: SLIDER ADJUSTMENT AT LEAF LEVEL
		// ========================================

		protected override void HandleLeftRight(int direction, int stepLevel) {
			if (Level > 0) {
				var w = GetWidgetAt(null);
				if (w != null && w.Type == WidgetType.Slider) {
					AdjustSlider(w, direction, stepLevel);
					return;
				}
			}
			base.HandleLeftRight(direction, stepLevel);
		}

		private void AdjustSlider(WidgetInfo w, int direction, int stepLevel) {
			var slider = w.Component as KSlider;
			if (slider == null) return;

			float step;
			if (slider.wholeNumbers) {
				step = Input.InputUtil.StepForLevel(stepLevel);
			} else {
				float range = slider.maxValue - slider.minValue;
				step = range * Input.InputUtil.FractionForLevel(stepLevel);
			}

			float oldValue = slider.value;
			slider.value = Mathf.Clamp(
				slider.value + step * direction,
				slider.minValue, slider.maxValue);

			// KSlider events only fire on mouse/keyboard input, not programmatic
			// value changes. Invoke onMove so side screens (e.g., CapacityControl)
			// sync related widgets like text fields.
			try {
				Traverse.Create(slider).Field<System.Action>("onMove").Value?.Invoke();
			} catch (System.Exception ex) {
				Util.Log.Warn($"AdjustSlider: onMove invoke failed: {ex.Message}");
			}

			if (slider.value <= slider.minValue && direction < 0)
				PlaySliderSound("Slider_Boundary_Low");
			else if (slider.value >= slider.maxValue && direction > 0)
				PlaySliderSound("Slider_Boundary_High");
			else if (slider.value != oldValue)
				PlaySliderSound("Slider_Move");

			RebuildSections();
			var fresh = GetWidgetAt(null);
			if (fresh != null)
				SpeechPipeline.SpeakInterrupt(WidgetOps.GetSpeechText(fresh));
		}

		private static void PlaySliderSound(string soundName) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(soundName));
			} catch (System.Exception ex) {
				Util.Log.Warn($"DetailsScreenHandler.PlaySliderSound({soundName}): {ex.Message}");
			}
		}

		// ========================================
		// KEY INTERCEPTION: TEXT EDITING
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (_textEdit.IsEditing) {
				if (e.TryConsume(Action.Escape)) {
					_textEdit.Cancel();
					RebuildSections();
					return true;
				}
				return false;
			}
			return base.HandleKeyDown(e);
		}

		// ========================================
		// SPEECH
		// ========================================

		public override void SpeakCurrentItem(string parentContext = null) {
			if (Level == 0) {
				base.SpeakCurrentItem(parentContext);
				return;
			}

			var w = GetWidgetAt(null);
			if (w == null) return;

			string text = WidgetOps.GetSpeechText(w);
			text = WidgetOps.AppendTooltip(text, WidgetOps.GetTooltipText(w));
			if (!string.IsNullOrEmpty(parentContext))
				text = parentContext + ", " + text;
			if (!string.IsNullOrEmpty(text))
				SpeechPipeline.SpeakInterrupt(text);
		}

		/// <summary>
		/// Get the WidgetInfo at the given indices, or at the current indices if null.
		/// </summary>
		private WidgetInfo GetWidgetAt(int[] indices) {
			int sIdx = indices != null ? indices[0] : GetIndex(0);
			int iIdx = indices != null ? indices[1] : GetIndex(1);
			if (sIdx < 0 || sIdx >= _sections.Count) return null;
			var items = _sections[sIdx].Items;
			if (iIdx < 0 || iIdx >= items.Count) return null;

			if (Level <= 1) return items[iIdx];

			int cIdx = indices != null ? indices[2] : GetIndex(2);
			var children = items[iIdx].Children;
			if (children == null || cIdx < 0 || cIdx >= children.Count) return null;
			return children[cIdx];
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
			_pendingFirstSection = true;
			_tabSwitching = true;
			base.OnActivate();
			_tabSwitching = false;
		}

		// ========================================
		// TICK: TARGET CHANGE DETECTION
		// ========================================

		public override void Tick() {
			if (_textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					_textEdit.Confirm();
					RebuildSections();
				}
				return;
			}

		if (_pendingActivationSpeech) {
				_pendingActivationSpeech = false;
				RebuildSections();
				SpeakCurrentItem();
			}

			var currentTarget = DetailsScreen.Instance != null
				? DetailsScreen.Instance.target : null;

			if (currentTarget != _lastTarget) {
				_lastTarget = currentTarget;
				_pendingFirstSection = false;
				if (currentTarget != null) {
					RebuildActiveTabs(currentTarget);
					_tabIndex = 0;
					SwitchGameTab();
					RebuildSections();
					ResetNavigation();

					SpeechPipeline.SpeakInterrupt(DisplayName);
					SpeakFirstSection();
				}
			} else if (_pendingFirstSection) {
				RebuildSections();
				if (_sections.Count > 0) {
					_pendingFirstSection = false;
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
		/// For main tabs (non-null GameTabId), calls the game's ChangeTab.
		/// For all tabs, calls OnTabSelected() for tab-specific activation
		/// (side screen tabs click their game MultiToggle here).
		/// </summary>
		private void SwitchGameTab() {
			if (_tabIndex < 0 || _tabIndex >= _activeTabs.Count) return;

			_activeTabs[_tabIndex].OnTabSelected();

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
			if (_sections.Count == 0 || string.IsNullOrEmpty(_sections[0].Header))
				return;
			string header = _sections[0].Header;
			if (_tabIndex >= 0 && _tabIndex < _activeTabs.Count
				&& header == _activeTabs[_tabIndex].DisplayName)
				return;
			SpeechPipeline.SpeakQueued(header);
		}

		// ========================================
		// TAB CONFIGURATION
		// ========================================

		private static IDetailTab[] BuildTabs() {
			return new IDetailTab[] {
				// Main info tabs (match game's DetailTabHeader order).
				// Availability is determined by the game's tab toggle visibility,
				// not hardcoded predicates — see RebuildActiveTabs.
				new StatusTab(),
				new PersonalityTab(),
				new ChoresTab(),
				new PropertiesTab(),

				// Side screen tabs (null gameTabId — availability via SidescreenTab.IsVisible).
				new ConfigSideTab(),
				new ErrandsSideTab(),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.MATERIAL.NAME),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.COSMETICS.NAME),
			};
		}
	}
}
