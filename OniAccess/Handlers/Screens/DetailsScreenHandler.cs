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
		protected override int StartLevel =>
			_tabIndex >= 0 && _tabIndex < _activeTabs.Count
				? _activeTabs[_tabIndex].StartLevel : 0;

		private readonly IDetailTab[] _tabs;
		private readonly List<IDetailTab> _activeTabs = new List<IDetailTab>();
		private readonly List<int> _sectionStarts = new List<int>();
		private readonly List<DetailSection> _sections = new List<DetailSection>();
		private readonly Input.TextEditHelper _textEdit = new Input.TextEditHelper();
		private int _tabIndex;
		private int _sectionIndex;
		private GameObject _lastTarget;
		private bool _suppressDisplayName;
		private bool _pendingFirstSection;
		private bool _pendingTabSpeech;
		private bool _pendingActivationSpeech;

		public override string DisplayName {
			get {
				if (_suppressDisplayName) return null;
				var ds = DetailsScreen.Instance;
				if (ds == null || ds.target == null)
					return STRINGS.ONIACCESS.HANDLERS.DETAILS_SCREEN;
				string entityName = ds.target.GetProperName();
				var resume = ds.target.GetComponent<MinionResume>();
				if (resume != null) {
					string hatName = null;
					if (resume.CurrentHat != null)
						foreach (var skill in Db.Get().Skills.resources)
							if (skill.hat == resume.CurrentHat) { hatName = skill.Name; break; }
					entityName = hatName != null
						? $"{entityName}, {hatName}, {resume.GetSkillsSubtitle()}"
						: $"{entityName}, {resume.GetSkillsSubtitle()}";
				}
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
			list.Add(new HelpEntry("Ctrl+Tab/Ctrl+Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_SECTION));
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
			var w = GetWidgetAt(indices[0], indices[1], indices[2]);
			if (w == null) return;

			if (w is ButtonWidget bw) {
				bw.Activate();
				_pendingActivationSpeech = true;
				return;
			}

			if (w is ToggleWidget) {
				w.Activate();
				_pendingActivationSpeech = true;
				return;
			}

			if (w is SliderWidget) {
				SpeakCurrentItem();
				return;
			}

			if (w is TextInputWidget tiw) {
				var textField = tiw.GetTextField();
				if (textField != null) {
					if (!_textEdit.IsEditing)
						_textEdit.Begin(textField);
					else
						_textEdit.Confirm();
				}
				return;
			}

			if (w is DropdownWidget)
				CycleRadioGroup(w, 1);
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
				var w = GetCurrentWidget();
				if (w is SliderWidget) {
					AdjustSlider(w, direction, stepLevel);
					return;
				}
				if (w is DropdownWidget) {
					CycleRadioGroup(w, direction);
					return;
				}
			}
			base.HandleLeftRight(direction, stepLevel);
		}

		private void AdjustSlider(Widget w, int direction, int stepLevel) {
			var sw = (SliderWidget)w;
			bool changed = sw.Adjust(direction, stepLevel);

			// KSlider events only fire on mouse/keyboard input, not programmatic
			// value changes. Invoke onMove so side screens (e.g., CapacityControl)
			// sync related widgets like text fields.
			var slider = w.Component as KSlider;
			if (changed && slider != null) {
				try {
					Traverse.Create(slider).Field<System.Action>("onMove").Value?.Invoke();
				} catch (System.Exception ex) {
					Util.Log.Warn($"AdjustSlider: onMove invoke failed: {ex.Message}");
				}
			}

			PlaySliderSound(sw.GetBoundarySound(direction));

			if (changed) {
				RebuildSections();
				var fresh = GetCurrentWidget();
				if (fresh != null)
					SpeechPipeline.SpeakInterrupt(WidgetOps.GetSpeechText(fresh));
			}
		}

		private void CycleRadioGroup(Widget w, int direction) {
			var members = w.Tag as List<SideScreenWalker.RadioMember>;
			if (members == null || members.Count == 0) return;

			int activeIndex = FindActiveRadioIndex(members);
			if (activeIndex < 0) return;

			int newIndex = ((activeIndex + direction) % members.Count + members.Count)
				% members.Count;
			var target = members[newIndex];
			if (target.Toggle != null)
				target.Toggle.Click();
			else
				WidgetOps.ClickMultiToggle(target.MultiToggleRef);
			_pendingActivationSpeech = true;
		}

		private static void PlaySliderSound(string soundName) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(soundName));
			} catch (System.Exception ex) {
				Util.Log.Warn($"DetailsScreenHandler.PlaySliderSound({soundName}): {ex.Message}");
			}
		}

		/// <summary>
		/// Find which RadioMember is currently active. KToggle members use
		/// IsToggleActive. MultiToggle members use CurrentState (state 1 =
		/// selected) unless they carry a NotificationType tag, where
		/// AlarmSideScreen's targetAlarm is matched instead (the alarm's
		/// default notificationType enum value is 0 which doesn't map to
		/// any valid type, leaving all toggles at state 1).
		/// </summary>
		private static int FindActiveRadioIndex(List<SideScreenWalker.RadioMember> members) {
			for (int i = 0; i < members.Count; i++) {
				if (members[i].Toggle != null && SideScreenWalker.IsToggleActive(members[i].Toggle))
					return i;
			}
			// AlarmSideScreen path: match by NotificationType tag.
			if (members[0].Tag is NotificationType) {
				var alarm = members[0].MultiToggleRef.GetComponentInParent<AlarmSideScreen>();
				if (alarm != null) {
					var activeType = alarm.targetAlarm.notificationType;
					for (int i = 0; i < members.Count; i++) {
						if (members[i].Tag is NotificationType nt && nt == activeType)
							return i;
					}
					return 0;
				}
			}
			// Generic MultiToggle path (FewOptionSideScreen, etc.):
			// state 1 = selected, state 0 = not selected.
			for (int i = 0; i < members.Count; i++) {
				if (members[i].MultiToggleRef != null && members[i].MultiToggleRef.CurrentState == 1)
					return i;
			}
			return -1;
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

			var w = GetCurrentWidget();
			if (w == null) return;

			string text = WidgetOps.GetSpeechText(w);
			text = WidgetOps.AppendTooltip(text, WidgetOps.GetTooltipText(w));
			if (!string.IsNullOrEmpty(parentContext))
				text = parentContext + ", " + text;
			if (!string.IsNullOrEmpty(text))
				SpeechPipeline.SpeakInterrupt(text);
		}

		private Widget GetCurrentWidget() {
			return GetWidgetAt(GetIndex(0), GetIndex(1), GetIndex(2));
		}

		private Widget GetWidgetAt(int sIdx, int iIdx, int cIdx) {
			if (sIdx < 0 || sIdx >= _sections.Count) return null;
			var items = _sections[sIdx].Items;
			if (iIdx < 0 || iIdx >= items.Count) return null;
			if (Level <= 1) return items[iIdx];
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
			_suppressDisplayName = true;
			base.OnActivate();
			_suppressDisplayName = false;
		}

		// ========================================
		// TICK: TARGET CHANGE DETECTION
		// ========================================

		public override bool Tick() {
			if (_textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					_textEdit.Confirm();
					RebuildSections();
					return true;
				}
				return false;
			}

			if (_pendingActivationSpeech) {
				_pendingActivationSpeech = false;
				RebuildSections();
				SpeakCurrentItem();
			}

			if (_pendingTabSpeech) {
				_pendingTabSpeech = false;
				RebuildSections();
				ResetNavigation();
				SpeakFirstSection();
			}

			var currentTarget = DetailsScreen.Instance != null
				? DetailsScreen.Instance.target : null;

			if (currentTarget != _lastTarget) {
				_lastTarget = currentTarget;
				_pendingFirstSection = false;
				_pendingTabSpeech = false;
				if (currentTarget != null) {
					RebuildActiveTabs(currentTarget);
					_tabIndex = 0;
					SwitchGameTab();
					RebuildSections();
					ResetNavigation();

					SpeechPipeline.SpeakInterrupt(DisplayName);
					SpeakFirstSection();
				}
				return false;
			} else if (_pendingFirstSection) {
				RebuildSections();
				if (_sections.Count > 0) {
					_pendingFirstSection = false;
					SpeechPipeline.SpeakInterrupt(DisplayName);
					SpeakFirstSection();
					return false;
				}
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab) && Input.InputUtil.CtrlHeld()) {
				AdvanceSection(Input.InputUtil.ShiftHeld() ? -1 : 1);
				return true;
			}

			return base.Tick();
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
			if (_sectionStarts.Count == 0) return;

			int start = _sectionStarts[_sectionIndex];
			int end = _sectionIndex + 1 < _sectionStarts.Count
				? _sectionStarts[_sectionIndex + 1] : _activeTabs.Count;
			int sectionSize = end - start;
			if (sectionSize <= 1) return;

			int oldIndex = _tabIndex;
			int offset = ((_tabIndex - start + direction) % sectionSize + sectionSize)
				% sectionSize;
			_tabIndex = start + offset;

			bool wrapped = direction > 0
				? _tabIndex <= oldIndex
				: _tabIndex >= oldIndex;

			SwitchGameTab();

			if (wrapped) PlayWrapSound();
			else PlayHoverSound();

			SpeechPipeline.SpeakInterrupt(_activeTabs[_tabIndex].DisplayName);

			if (_activeTabs[_tabIndex].GameTabId == null) {
				_pendingTabSpeech = true;
			} else {
				RebuildSections();
				ResetNavigation();
				SpeakFirstSection();
			}
		}

		private void AdvanceSection(int direction) {
			if (_sectionStarts.Count <= 1) return;

			int oldSection = _sectionIndex;
			_sectionIndex = ((_sectionIndex + direction) % _sectionStarts.Count
				+ _sectionStarts.Count) % _sectionStarts.Count;
			_tabIndex = _sectionStarts[_sectionIndex];

			bool wrapped = direction > 0
				? _sectionIndex <= oldSection
				: _sectionIndex >= oldSection;

			SwitchGameTab();

			if (wrapped) PlayWrapSound();
			else PlayHoverSound();

			SpeechPipeline.SpeakInterrupt(_activeTabs[_tabIndex].DisplayName);

			if (_activeTabs[_tabIndex].GameTabId == null) {
				_pendingTabSpeech = true;
			} else {
				RebuildSections();
				ResetNavigation();
				SpeakFirstSection();
			}
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

			_sectionStarts.Clear();
			_sectionIndex = 0;
			bool? lastWasSideScreen = null;
			for (int i = 0; i < _activeTabs.Count; i++) {
				bool isSideScreen = _activeTabs[i].GameTabId == null;
				if (lastWasSideScreen != isSideScreen) {
					_sectionStarts.Add(i);
					lastWasSideScreen = isSideScreen;
				}
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

		private void ResetNavigation() {
			ResetState();
		}

		private void SpeakFirstSection() {
			if (_sections.Count == 0) {
				if (_tabIndex >= 0 && _tabIndex < _activeTabs.Count) {
					if (_activeTabs[_tabIndex] is ConfigSideTab)
						SpeechPipeline.SpeakQueued(
							(string)STRINGS.UI.UISIDESCREENS.NOCONFIG.LABEL);
					else if (_activeTabs[_tabIndex] is ErrandsSideTab)
						SpeechPipeline.SpeakQueued(
							(string)STRINGS.ONIACCESS.DETAILS.NO_ERRANDS);
				}
				return;
			}
			if (string.IsNullOrEmpty(_sections[0].Header))
				return;
			string header = _sections[0].Header;
			bool headerIsTabName = _tabIndex >= 0 && _tabIndex < _activeTabs.Count
				&& string.Equals(header, _activeTabs[_tabIndex].DisplayName,
					System.StringComparison.OrdinalIgnoreCase);

			if (Level > 0) {
				var items = _sections[0].Items;
				if (items.Count == 0) return;
				string item = WidgetOps.GetSpeechText(items[0]);
				if (string.IsNullOrWhiteSpace(item)) return;
				string label = headerIsTabName ? item : header + ", " + item;
				SpeechPipeline.SpeakQueued(label);
			} else {
				if (!headerIsTabName)
					SpeechPipeline.SpeakQueued(header);
			}
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
				new MaterialTab(),
				new StubTab(
					(string)STRINGS.UI.DETAILTABS.COSMETICS.NAME),
			};
		}
	}
}
