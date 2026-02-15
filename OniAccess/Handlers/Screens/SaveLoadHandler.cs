using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for LoadScreen (save/load screen).
	///
	/// Two-level navigation:
	/// 1. Colony list (colonyListRoot): browse colonies by name, cycle, date
	/// 2. Colony save view (colonyViewRoot): individual saves for a selected colony
	///
	/// Enter on a colony drills into its saves. Escape in save view goes back to
	/// colony list. Enter on a save loads it.
	///
	/// Per locked decisions:
	/// - Read colony name, cycle number, duplicant count, and date per entry
	/// - File size omitted
	/// - Standard list navigation (arrows, Home/End, Enter, type-ahead)
	/// </summary>
	public class SaveLoadHandler: BaseMenuHandler {
		private bool _inColonySaveView;
		private bool _pendingViewTransition;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.SAVE_LOAD;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public SaveLoadHandler(KScreen screen) : base(screen) {
			_inColonySaveView = false;
			HelpEntries = BuildHelpEntries();
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (!_inColonySaveView) {
				DiscoverColonyList(screen);
			} else {
				DiscoverColonySaves(screen);
			}

			Util.Log.Debug($"SaveLoadHandler.DiscoverWidgets: {_widgets.Count} widgets");
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
				Util.Log.Debug($"SaveLoadHandler.DiscoverColonyList(saveButtonRoot): {ex.Message}");
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
				Util.Log.Debug($"SaveLoadHandler.AddTraverseButton: {ex.Message}");
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
				Util.Log.Debug($"SaveLoadHandler.GetReferenceText: {ex.Message}");
			}
			return null;
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
				Util.Log.Debug($"SaveLoadHandler.DiscoverColonySaves(colonyViewRoot): {ex.Message}");
			}

			if (colonyViewRoot == null || !colonyViewRoot.activeInHierarchy) {
				_inColonySaveView = false;
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

				// Find the LoadButton via non-generic reference
				KButton loadButton = null;
				if (entry.HasReference("LoadButton")) {
					var lbRef = entry.GetReference("LoadButton");
					if (lbRef != null)
						loadButton = lbRef.gameObject.GetComponent<KButton>();
				}
				if (loadButton == null)
					loadButton = entry.GetComponentInChildren<KButton>();

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = loadButton,
					Type = loadButton != null ? WidgetType.Button : WidgetType.Label,
					GameObject = entry.gameObject
				});
			}

			// Add Delete button from the detail panel
			if (viewRefs != null && viewRefs.HasReference("DeleteButton")) {
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
					Util.Log.Debug($"SaveLoadHandler.DiscoverColonySaves(deleteButton): {ex.Message}");
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
				Util.Log.Debug($"SaveLoadHandler.IsLabelActive: {ex.Message}");
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
		/// Override to handle two-level navigation:
		/// - In colony list: Enter on colony_entry drills into saves; other buttons click normally
		/// - In save view: Enter loads the selected save
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (!_inColonySaveView) {
				// Only colony_entry tagged widgets trigger the drill-into-saves transition
				if (widget.Type == WidgetType.Button
					&& widget.Tag is string tag && tag == "colony_entry") {
					var kbutton = widget.Component as KButton;
					kbutton?.SignalClick(KKeyCode.Mouse0);

					if (IsColonyViewRootActive()) {
						TransitionToSaveView();
					} else {
						_pendingViewTransition = true;
					}
				} else {
					// Management buttons: normal click without view transition
					base.ActivateCurrentWidget();
				}
			} else {
				// Save view: Enter loads the selected save
				base.ActivateCurrentWidget();
			}
		}

		/// <summary>
		/// Check for pending view transition and stale widgets each frame.
		/// </summary>
		public override void Tick() {
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

			base.Tick();
		}

		/// <summary>
		/// Escape in save view goes back to colony list instead of closing the screen.
		/// Escape in colony list passes through to close the screen normally.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;

			if (_inColonySaveView && e.TryConsume(Action.Escape)) {
				TransitionToColonyList();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Transition from colony list to individual save view.
		/// </summary>
		private void TransitionToSaveView() {
			_inColonySaveView = true;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
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
						backButton?.SignalClick(KKeyCode.Mouse0);
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"SaveLoadHandler.TransitionToColonyList: {ex.Message}");
			}

			_inColonySaveView = false;
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
				Util.Log.Debug($"SaveLoadHandler.IsColonyViewRootActive: {ex.Message}");
				return false;
			}
		}
	}
}
