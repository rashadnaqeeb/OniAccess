using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for LoadScreen (save/load screen).
	///
	/// Three-level navigation:
	/// 1. Colony list (colonyListRoot): browse colonies by name, cycle, date
	/// 2. Colony save view (colonyViewRoot): individual saves for a selected colony
	/// 3. Save detail: info fields, Load button, Delete button for a selected save
	///
	/// Enter on a colony drills into its saves. Enter on a save drills into its
	/// detail view. Enter on Load loads the game. Escape goes back one level.
	/// </summary>
	public class SaveLoadHandler: BaseWidgetHandler {
		private enum ViewLevel { ColonyList, SaveList, SaveDetail }
		private ViewLevel _viewLevel;
		private bool _pendingViewTransition;
		private HierarchyReferences _selectedSaveEntry;
		private int _saveListCursorIndex;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.SAVE_LOAD;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public SaveLoadHandler(KScreen screen) : base(screen) {
			_viewLevel = ViewLevel.ColonyList;
			HelpEntries = BuildHelpEntries();
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			switch (_viewLevel) {
				case ViewLevel.ColonyList:
					DiscoverColonyList(screen);
					break;
				case ViewLevel.SaveList:
					DiscoverColonySaves(screen);
					break;
				case ViewLevel.SaveDetail:
					DiscoverSaveDetail(screen);
					break;
			}

			Util.Log.Debug($"SaveLoadHandler.DiscoverWidgets: {_widgets.Count} widgets ({_viewLevel})");
			return true;
		}

		/// <summary>
		/// Discover colony entries in the colony list view.
		/// Each colony shows: name, cycle, duplicant count, date.
		/// Management buttons (Save Info, Convert All, Load More) bookend the list.
		/// </summary>
		private void DiscoverColonyList(KScreen screen) {
			var traverse = Traverse.Create(screen);

			// Access saveButtonRoot: the actual container for colony entry buttons.
			// colonyListRoot contains unrelated HierarchyReferences we don't want.
			UnityEngine.GameObject saveButtonRoot = null;
			try {
				saveButtonRoot = traverse.Field("saveButtonRoot")
					.GetValue<UnityEngine.GameObject>();
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.DiscoverColonyList(saveButtonRoot): {ex.Message}");
			}

			if (saveButtonRoot == null || !saveButtonRoot.activeInHierarchy) {
				DiscoverColonyListFallback(screen);
				return;
			}

			// Management buttons at top of list
			AddTraverseButton(traverse, "colonyInfoButton",
				STRINGS.ONIACCESS.SAVE_LOAD.SAVE_INFO);
			AddTraverseButton(traverse, "colonyCloudButton",
				STRINGS.ONIACCESS.SAVE_LOAD.CONVERT_ALL_TO_CLOUD);
			AddTraverseButton(traverse, "colonyLocalButton",
				STRINGS.ONIACCESS.SAVE_LOAD.CONVERT_ALL_TO_LOCAL);

			// Walk direct children of saveButtonRoot for colony entries
			var root = saveButtonRoot.transform;
			for (int i = 0; i < root.childCount; i++) {
				var child = root.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;

				var entry = child.GetComponent<HierarchyReferences>();
				if (entry == null) continue;

				string label = BuildColonyEntryLabel(entry);
				if (string.IsNullOrEmpty(label)) continue;

				// Find clickable button via named "Button" reference first
				KButton kbutton = null;
				if (entry.HasReference("Button")) {
					var btnRef = entry.GetReference("Button");
					if (btnRef != null)
						kbutton = btnRef.gameObject.GetComponent<KButton>();
				}
				if (kbutton == null)
					kbutton = entry.GetComponent<KButton>();

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = kbutton,
					Type = kbutton != null ? WidgetType.Button : WidgetType.Label,
					GameObject = entry.gameObject,
					Tag = "colony_entry"
				});
			}

			// Load More button at bottom of list
			AddTraverseButton(traverse, "loadMoreButton", null);
		}

		/// <summary>
		/// Add a management button widget from a Traverse field.
		/// Only adds if the field resolves to an active, interactable KButton.
		/// Reads child LocText for the label, falling back to the provided string.
		/// </summary>
		private void AddTraverseButton(Traverse traverse, string fieldName, string fallbackLabel) {
			try {
				var button = traverse.Field(fieldName).GetValue<KButton>();
				if (button == null || !button.gameObject.activeInHierarchy
					|| !button.isInteractable) return;

				string label = null;
				var locText = button.GetComponentInChildren<LocText>();
				if (locText != null && !string.IsNullOrEmpty(locText.text))
					label = locText.text.Trim();
				if (string.IsNullOrEmpty(label))
					label = fallbackLabel;
				if (string.IsNullOrEmpty(label)) return;

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = button,
					Type = WidgetType.Button,
					GameObject = button.gameObject
				});
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.AddTraverseButton: {ex.Message}");
			}
		}

		/// <summary>
		/// Build a composite label for a colony list entry.
		/// Format: "colony name, cycle N, X duplicants, date, cloud/local status"
		/// </summary>
		private string BuildColonyEntryLabel(HierarchyReferences entry) {
			var parts = new List<string>();

			string headerTitle = GetReferenceText(entry, "HeaderTitle");
			if (!string.IsNullOrEmpty(headerTitle)) {
				parts.Add(headerTitle);
			}

			string saveTitle = GetReferenceText(entry, "SaveTitle");
			if (!string.IsNullOrEmpty(saveTitle)) {
				parts.Add(saveTitle);
			}

			string headerDate = GetReferenceText(entry, "HeaderDate");
			if (!string.IsNullOrEmpty(headerDate)) {
				parts.Add(headerDate);
			}

			// Cloud/local status (shown when cloud saves are visible)
			string locationText = GetReferenceText(entry, "LocationText");
			if (!string.IsNullOrEmpty(locationText)) {
				parts.Add(locationText);
			}

			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		/// <summary>
		/// Get text from a named reference within a HierarchyReferences component.
		/// Uses non-generic GetReference to avoid type-check failures (LoadScreen
		/// stores references as RectTransform, not LocText).
		/// </summary>
		private string GetReferenceText(HierarchyReferences refs, string refName) {
			if (!refs.HasReference(refName)) return null;
			try {
				var component = refs.GetReference(refName);
				if (component == null) return null;
				var locText = component as LocText
					?? component.gameObject.GetComponent<LocText>();
				if (locText != null && !string.IsNullOrEmpty(locText.text))
					return locText.text.Trim();
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.GetReferenceText: {ex.Message}");
			}
			return null;
		}

		/// <summary>
		/// Add a detail panel LocText field as a Label widget.
		/// Uses the field's own GameObject so tooltip lookup stays scoped
		/// to the individual field rather than picking up the panel's tooltip.
		/// </summary>
		private void AddDetailField(HierarchyReferences refs, string refName) {
			if (!refs.HasReference(refName)) return;
			try {
				var component = refs.GetReference(refName);
				if (component == null) return;
				var locText = component as LocText
					?? component.gameObject.GetComponent<LocText>();
				if (locText == null || string.IsNullOrEmpty(locText.text)) return;

				_widgets.Add(new WidgetInfo {
					Label = locText.text.Trim(),
					Type = WidgetType.Label,
					GameObject = component.gameObject
				});
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.AddDetailField: {ex.Message}");
			}
		}

		/// <summary>
		/// Fallback colony list discovery when saveButtonRoot field is not accessible.
		/// Walks the screen's children for KButton instances with LocText labels.
		/// </summary>
		private void DiscoverColonyListFallback(KScreen screen) {
			var kbuttons = screen.GetComponentsInChildren<KButton>(false);
			if (kbuttons == null) return;

			foreach (var kb in kbuttons) {
				if (kb == null || !kb.gameObject.activeInHierarchy
					|| !kb.isInteractable) continue;

				var locText = kb.GetComponentInChildren<LocText>();
				if (locText == null || string.IsNullOrEmpty(locText.text)) continue;

				_widgets.Add(new WidgetInfo {
					Label = locText.text,
					Component = kb,
					Type = WidgetType.Button,
					GameObject = kb.gameObject
				});
			}
		}

		// ========================================
		// COLONY SAVE VIEW
		// ========================================

		/// <summary>
		/// Discover individual save entries for the selected colony.
		/// Each save shows: save name, date, with auto-save/newest prefix if applicable.
		/// Scoped to the Content container within colonyViewRoot, filtering for cloned entries.
		/// </summary>
		private void DiscoverColonySaves(KScreen screen) {
			var traverse = Traverse.Create(screen);

			UnityEngine.GameObject colonyViewRoot = null;
			try {
				colonyViewRoot = traverse.Field("colonyViewRoot")
					.GetValue<UnityEngine.GameObject>();
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.DiscoverColonySaves(colonyViewRoot): {ex.Message}");
			}

			if (colonyViewRoot == null || !colonyViewRoot.activeInHierarchy) {
				_viewLevel = ViewLevel.ColonyList;
				DiscoverColonyList(screen);
				return;
			}

			// Access the Content container from colonyViewRoot's HierarchyReferences
			UnityEngine.Transform contentContainer = null;
			var viewRefs = colonyViewRoot.GetComponent<HierarchyReferences>();
			if (viewRefs != null && viewRefs.HasReference("Content")) {
				var contentRef = viewRefs.GetReference("Content");
				if (contentRef != null)
					contentContainer = contentRef.transform;
			}

			if (contentContainer == null) {
				DiscoverColonySavesFallback(colonyViewRoot);
				return;
			}

			// Walk active children that are clones (instantiated save entries)
			for (int i = 0; i < contentContainer.childCount; i++) {
				var child = contentContainer.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;
				if (!child.gameObject.name.Contains("Clone")) continue;

				var entry = child.GetComponent<HierarchyReferences>();
				if (entry == null) continue;

				string label = BuildSaveEntryLabel(entry);
				if (string.IsNullOrEmpty(label)) continue;

				// Row button used to drill into detail view
				KButton rowButton = child.GetComponent<KButton>();

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = rowButton,
					Type = rowButton != null ? WidgetType.Button : WidgetType.Label,
					GameObject = entry.gameObject,
					Tag = rowButton
				});
			}
		}

		// ========================================
		// SAVE DETAIL VIEW
		// ========================================

		/// <summary>
		/// Discover detail fields and action buttons for the selected save.
		/// Reads LocText fields from colonyViewRoot that were populated by
		/// ShowColonySave() when the row was clicked during transition.
		/// </summary>
		private void DiscoverSaveDetail(KScreen screen) {
			// Guard: if the selected entry was destroyed, fall back to save list
			if (_selectedSaveEntry == null
				|| _selectedSaveEntry.gameObject == null) {
				TransitionToSaveList();
				return;
			}

			var traverse = Traverse.Create(screen);

			UnityEngine.GameObject colonyViewRoot = null;
			try {
				colonyViewRoot = traverse.Field("colonyViewRoot")
					.GetValue<UnityEngine.GameObject>();
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.DiscoverSaveDetail(colonyViewRoot): {ex.Message}");
			}

			if (colonyViewRoot == null || !colonyViewRoot.activeInHierarchy) {
				TransitionToSaveList();
				return;
			}

			var viewRefs = colonyViewRoot.GetComponent<HierarchyReferences>();
			if (viewRefs == null) {
				TransitionToSaveList();
				return;
			}

			// Detail info fields from the panel LocText components.
			// Each widget gets the field's own GameObject so tooltips are
			// scoped to the individual field, not the whole panel.
			string[] detailFields = {
				"Title", "Date", "InfoWorld", "InfoCycles",
				"InfoDupes", "FileSize", "Filename"
			};
			foreach (string field in detailFields) {
				AddDetailField(viewRefs, field);
			}

			// AutoInfo: version warning, only if active/visible
			if (viewRefs.HasReference("AutoInfo")) {
				try {
					var autoRef = viewRefs.GetReference("AutoInfo");
					if (autoRef != null && autoRef.gameObject.activeInHierarchy) {
						string autoText = null;
						var locText = autoRef as LocText
							?? autoRef.gameObject.GetComponent<LocText>();
						if (locText != null && !string.IsNullOrEmpty(locText.text))
							autoText = locText.text.Trim();
						if (!string.IsNullOrEmpty(autoText)) {
							_widgets.Add(new WidgetInfo {
								Label = autoText,
								Type = WidgetType.Label,
								GameObject = autoRef.gameObject
							});
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Error($"SaveLoadHandler.DiscoverSaveDetail(AutoInfo): {ex.Message}");
				}
			}

			// Load button from the save entry clone
			if (_selectedSaveEntry.HasReference("LoadButton")) {
				try {
					var lbRef = _selectedSaveEntry.GetReference("LoadButton");
					if (lbRef != null) {
						var loadButton = lbRef.gameObject.GetComponent<KButton>();
						if (loadButton != null && lbRef.gameObject.activeInHierarchy) {
							string label = null;
							var locText = lbRef.gameObject.GetComponentInChildren<LocText>();
							if (locText != null && !string.IsNullOrEmpty(locText.text))
								label = locText.text.Trim();
							if (string.IsNullOrEmpty(label))
								label = (string)STRINGS.UI.FRONTEND.LOADSCREEN.TITLE;

							_widgets.Add(new WidgetInfo {
								Label = label,
								Component = loadButton,
								Type = WidgetType.Button,
								GameObject = lbRef.gameObject
							});
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Error($"SaveLoadHandler.DiscoverSaveDetail(LoadButton): {ex.Message}");
				}
			}

			// Delete button from the detail panel
			if (viewRefs.HasReference("DeleteButton")) {
				try {
					var delRef = viewRefs.GetReference("DeleteButton");
					if (delRef != null) {
						var delButton = delRef.gameObject.GetComponent<KButton>();
						if (delButton != null && delRef.gameObject.activeInHierarchy
							&& delButton.isInteractable) {
							_widgets.Add(new WidgetInfo {
								Label = STRINGS.ONIACCESS.SAVE_LOAD.DELETE,
								Component = delButton,
								Type = WidgetType.Button,
								GameObject = delRef.gameObject
							});
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Error($"SaveLoadHandler.DiscoverSaveDetail(DeleteButton): {ex.Message}");
				}
			}
		}

		/// <summary>
		/// Build a composite label for an individual save entry.
		/// Format: "[auto-save] [newest] save_name, date"
		/// Per decision: colony name, cycle, duplicant count, date. File size omitted.
		/// </summary>
		private string BuildSaveEntryLabel(HierarchyReferences entry) {
			var parts = new List<string>();

			bool isAutoSave = IsLabelActive(entry, "AutoLabel");
			bool isNewest = IsLabelActive(entry, "NewestLabel");

			if (isNewest) parts.Add((string)STRINGS.ONIACCESS.SAVE_LOAD.NEWEST);
			if (isAutoSave) parts.Add((string)STRINGS.ONIACCESS.SAVE_LOAD.AUTO_SAVE);

			string saveText = GetReferenceText(entry, "SaveText");
			if (!string.IsNullOrEmpty(saveText)) {
				parts.Add(saveText);
			}

			string dateText = GetReferenceText(entry, "DateText");
			if (!string.IsNullOrEmpty(dateText)) {
				parts.Add(dateText);
			}

			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		/// <summary>
		/// Check if a named label reference is active (visible) in the entry.
		/// Uses non-generic GetReference to handle RectTransform storage.
		/// </summary>
		private bool IsLabelActive(HierarchyReferences refs, string refName) {
			if (!refs.HasReference(refName)) return false;
			try {
				var obj = refs.GetReference(refName);
				return obj != null && obj.gameObject.activeInHierarchy;
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.IsLabelActive: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Fallback save entry discovery from a view root when Content container
		/// is not available.
		/// </summary>
		private void DiscoverColonySavesFallback(UnityEngine.GameObject viewRoot) {
			var kbuttons = viewRoot.GetComponentsInChildren<KButton>(false);
			if (kbuttons == null) return;

			foreach (var kb in kbuttons) {
				if (kb == null || !kb.gameObject.activeInHierarchy
					|| !kb.isInteractable) continue;

				var locText = kb.GetComponentInChildren<LocText>();
				if (locText == null || string.IsNullOrEmpty(locText.text)) continue;

				_widgets.Add(new WidgetInfo {
					Label = locText.text,
					Component = kb,
					Type = WidgetType.Button,
					GameObject = kb.gameObject
				});
			}
		}

		// ========================================
		// VIEW TRANSITIONS
		// ========================================

		/// <summary>
		/// Override to handle three-level navigation:
		/// - Colony list: Enter on colony_entry drills into saves; other buttons click normally
		/// - Save list: Enter on save entry drills into detail view
		/// - Save detail: Enter on buttons (Load/Delete) activates them normally
		/// </summary>
		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			switch (_viewLevel) {
				case ViewLevel.ColonyList:
					if (widget.Type == WidgetType.Button
						&& widget.Tag is string tag && tag == "colony_entry") {
						var kbutton = widget.Component as KButton;
						if (kbutton != null)
							ClickButton(kbutton);

						if (IsColonyViewRootActive()) {
							TransitionToSaveView();
						} else {
							_pendingViewTransition = true;
						}
					} else {
						base.ActivateCurrentItem();
					}
					break;

				case ViewLevel.SaveList:
					if (widget.Tag is KButton) {
						TransitionToSaveDetail();
					} else {
						base.ActivateCurrentItem();
					}
					break;

				case ViewLevel.SaveDetail:
					base.ActivateCurrentItem();
					break;
			}
		}

		/// <summary>
		/// Check for pending view transition and stale widgets each frame.
		/// </summary>
		public override bool Tick() {
			if (_pendingViewTransition && IsColonyViewRootActive()) {
				TransitionToSaveView();
				_pendingViewTransition = false;
			}

			// Stale widget detection: after delete or dialog rebuild, the current
			// widget's GameObject may be destroyed. Rediscover and clamp cursor.
			if (_widgets.Count > 0 && _currentIndex >= 0
				&& _currentIndex < _widgets.Count) {
				var go = _widgets[_currentIndex].GameObject;
				if (go == null) {
					DiscoverWidgets(_screen);
					if (_currentIndex >= _widgets.Count)
						_currentIndex = _widgets.Count > 0 ? _widgets.Count - 1 : 0;
					if (_widgets.Count > 0 && _currentIndex < _widgets.Count) {
						Speech.SpeechPipeline.SpeakInterrupt(
							GetWidgetSpeechText(_widgets[_currentIndex]));
					}
				}
			}

			return base.Tick();
		}

		/// <summary>
		/// Escape goes back one level: detail → save list → colony list → close screen.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;

			if (_viewLevel == ViewLevel.SaveDetail && e.TryConsume(Action.Escape)) {
				TransitionToSaveList();
				return true;
			}

			if (_viewLevel == ViewLevel.SaveList && e.TryConsume(Action.Escape)) {
				TransitionToColonyList();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Transition from colony list to individual save view.
		/// </summary>
		private void TransitionToSaveView() {
			_viewLevel = ViewLevel.SaveList;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
			}
		}

		/// <summary>
		/// Transition from save list to save detail view.
		/// Clicks the row button to sync game selection and populate the detail panel.
		/// </summary>
		private void TransitionToSaveDetail() {
			_saveListCursorIndex = _currentIndex;

			var widget = _widgets[_currentIndex];
			_selectedSaveEntry = widget.GameObject.GetComponent<HierarchyReferences>();

			// Click the row to trigger ShowColonySave() and populate detail fields
			var rowButton = widget.Tag as KButton;
			if (rowButton != null)
				ClickButton(rowButton);

			_viewLevel = ViewLevel.SaveDetail;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
			}
		}

		/// <summary>
		/// Transition from save detail back to save list.
		/// </summary>
		private void TransitionToSaveList() {
			_selectedSaveEntry = null;
			_viewLevel = ViewLevel.SaveList;
			DiscoverWidgets(_screen);

			// Restore cursor position, clamped to valid range
			if (_saveListCursorIndex >= _widgets.Count)
				_saveListCursorIndex = _widgets.Count > 0 ? _widgets.Count - 1 : 0;
			_currentIndex = _saveListCursorIndex;

			if (_widgets.Count > 0 && _currentIndex < _widgets.Count) {
				Speech.SpeechPipeline.SpeakInterrupt(
					GetWidgetSpeechText(_widgets[_currentIndex]));
			}
		}

		/// <summary>
		/// Transition from save view back to colony list.
		/// Clicks the back button via colonyViewRoot's HierarchyReferences "Back" ref.
		/// </summary>
		private void TransitionToColonyList() {
			try {
				var traverse = Traverse.Create(_screen);
				var colonyViewRoot = traverse.Field("colonyViewRoot")
					.GetValue<UnityEngine.GameObject>();
				if (colonyViewRoot != null) {
					var viewRefs = colonyViewRoot.GetComponent<HierarchyReferences>();
					if (viewRefs != null && viewRefs.HasReference("Back")) {
						var backRef = viewRefs.GetReference("Back");
						var backButton = backRef?.gameObject.GetComponent<KButton>();
						if (backButton != null)
							ClickButton(backButton);
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.TransitionToColonyList: {ex.Message}");
			}

			_selectedSaveEntry = null;
			_viewLevel = ViewLevel.ColonyList;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
			}
		}

		/// <summary>
		/// Check if colonyViewRoot is active (transition to save view completed).
		/// </summary>
		private bool IsColonyViewRootActive() {
			try {
				var viewRoot = Traverse.Create(_screen).Field("colonyViewRoot")
					.GetValue<UnityEngine.GameObject>();
				return viewRoot != null && viewRoot.activeInHierarchy;
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveLoadHandler.IsColonyViewRootActive: {ex.Message}");
				return false;
			}
		}
	}
}
