using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
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
			var entries = new List<HelpEntry>();
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			HelpEntries = entries;
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (!_inColonySaveView) {
				DiscoverColonyList(screen);
			} else {
				DiscoverColonySaves(screen);
			}

			Util.Log.Debug($"SaveLoadHandler.DiscoverWidgets: {_widgets.Count} widgets");
		}

		/// <summary>
		/// Discover colony entries in the colony list view.
		/// Each colony shows: name, cycle, duplicant count, date.
		/// </summary>
		private void DiscoverColonyList(KScreen screen) {
			var traverse = Traverse.Create(screen);

			// Access colonyListRoot Transform
			UnityEngine.GameObject colonyListRoot = null;
			try {
				colonyListRoot = traverse.Field("colonyListRoot")
					.GetValue<UnityEngine.GameObject>();
			} catch (System.Exception) { }

			if (colonyListRoot == null || !colonyListRoot.activeInHierarchy) {
				// Fallback: try finding colony entries from screen children directly
				DiscoverColonyListFallback(screen);
				return;
			}

			// Find colony entries: each is a HierarchyReferences entry
			var entries = colonyListRoot.GetComponentsInChildren<HierarchyReferences>(true);
			if (entries == null) return;

			foreach (var entry in entries) {
				if (entry == null || !entry.gameObject.activeInHierarchy) continue;

				// Build composite label from colony info
				string label = BuildColonyEntryLabel(entry);
				if (string.IsNullOrEmpty(label)) continue;

				// Find the clickable button on this colony entry
				var kbutton = entry.GetComponent<KButton>();
				if (kbutton == null) {
					kbutton = entry.GetComponentInChildren<KButton>();
				}

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = kbutton,
					Type = kbutton != null ? WidgetType.Button : WidgetType.Label,
					GameObject = entry.gameObject
				});
			}
		}

		/// <summary>
		/// Build a composite label for a colony list entry.
		/// Format: "colony name, cycle N, X duplicants, date"
		/// </summary>
		private string BuildColonyEntryLabel(HierarchyReferences entry) {
			var parts = new List<string>();

			// Colony name from HeaderTitle
			string headerTitle = GetReferenceText(entry, "HeaderTitle");
			if (!string.IsNullOrEmpty(headerTitle)) {
				parts.Add(headerTitle);
			}

			// Save info from SaveTitle (contains cycle count and other info)
			string saveTitle = GetReferenceText(entry, "SaveTitle");
			if (!string.IsNullOrEmpty(saveTitle)) {
				parts.Add(saveTitle);
			}

			// Date from HeaderDate
			string headerDate = GetReferenceText(entry, "HeaderDate");
			if (!string.IsNullOrEmpty(headerDate)) {
				parts.Add(headerDate);
			}

			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		/// <summary>
		/// Get text from a named reference within a HierarchyReferences component.
		/// </summary>
		private string GetReferenceText(HierarchyReferences refs, string refName) {
			try {
				var obj = refs.GetReference<LocText>(refName);
				if (obj != null && !string.IsNullOrEmpty(obj.text)) {
					return obj.text.Trim();
				}
			} catch (System.Exception) {
				// Reference may not exist in this entry
			}

			return null;
		}

		/// <summary>
		/// Fallback colony list discovery when colonyListRoot field is not accessible.
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
		/// </summary>
		private void DiscoverColonySaves(KScreen screen) {
			var traverse = Traverse.Create(screen);

			UnityEngine.GameObject colonyViewRoot = null;
			try {
				colonyViewRoot = traverse.Field("colonyViewRoot")
					.GetValue<UnityEngine.GameObject>();
			} catch (System.Exception) { }

			if (colonyViewRoot == null || !colonyViewRoot.activeInHierarchy) {
				// If view root not active, fall back to colony list
				_inColonySaveView = false;
				DiscoverColonyList(screen);
				return;
			}

			// Find save entries within the colony view
			var entries = colonyViewRoot.GetComponentsInChildren<HierarchyReferences>(true);
			if (entries == null || entries.Length == 0) {
				// Try finding save entries via KButton children
				DiscoverColonySavesFallback(colonyViewRoot);
				return;
			}

			foreach (var entry in entries) {
				if (entry == null || !entry.gameObject.activeInHierarchy) continue;

				string label = BuildSaveEntryLabel(entry);
				if (string.IsNullOrEmpty(label)) continue;

				// Find the LoadButton
				KButton loadButton = null;
				try {
					loadButton = entry.GetReference<KButton>("LoadButton");
				} catch (System.Exception) { }

				if (loadButton == null) {
					loadButton = entry.GetComponentInChildren<KButton>();
				}

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = loadButton,
					Type = loadButton != null ? WidgetType.Button : WidgetType.Label,
					GameObject = entry.gameObject
				});
			}
		}

		/// <summary>
		/// Build a composite label for an individual save entry.
		/// Format: "[auto-save] [newest] save_name, date"
		/// Per decision: colony name, cycle, duplicant count, date. File size omitted.
		/// </summary>
		private string BuildSaveEntryLabel(HierarchyReferences entry) {
			var parts = new List<string>();

			// Check for auto-save and newest labels
			bool isAutoSave = IsLabelActive(entry, "AutoLabel");
			bool isNewest = IsLabelActive(entry, "NewestLabel");

			if (isNewest) parts.Add("newest");
			if (isAutoSave) parts.Add("auto-save");

			// Save name from SaveText
			string saveText = GetReferenceText(entry, "SaveText");
			if (!string.IsNullOrEmpty(saveText)) {
				parts.Add(saveText);
			}

			// Date from DateText
			string dateText = GetReferenceText(entry, "DateText");
			if (!string.IsNullOrEmpty(dateText)) {
				parts.Add(dateText);
			}

			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		/// <summary>
		/// Check if a named label reference is active (visible) in the entry.
		/// </summary>
		private bool IsLabelActive(HierarchyReferences refs, string refName) {
			try {
				var obj = refs.GetReference<UnityEngine.Component>(refName);
				return obj != null && obj.gameObject.activeInHierarchy;
			} catch (System.Exception) {
				return false;
			}
		}

		/// <summary>
		/// Fallback save entry discovery from a view root when HierarchyReferences
		/// are not available.
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
		/// - In colony list: Enter drills into colony saves
		/// - In save view: Enter loads the selected save
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (!_inColonySaveView) {
				// Colony list view: Enter clicks colony entry to drill into saves
				if (widget.Type == WidgetType.Button) {
					var kbutton = widget.Component as KButton;
					kbutton?.SignalClick(KKeyCode.Mouse0);

					// Check if the view transitioned
					if (IsColonyViewRootActive()) {
						TransitionToSaveView();
					} else {
						// Async transition: set pending flag
						_pendingViewTransition = true;
					}
				}
			} else {
				// Save view: Enter loads the selected save
				base.ActivateCurrentWidget();
			}
		}

		/// <summary>
		/// Check for pending view transition each frame.
		/// </summary>
		public override void Tick() {
			if (_pendingViewTransition && IsColonyViewRootActive()) {
				TransitionToSaveView();
				_pendingViewTransition = false;
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

			// Speak colony name then first save entry
			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
			}
		}

		/// <summary>
		/// Transition from save view back to colony list.
		/// Clicks the back button if available, then rediscovers colony list widgets.
		/// </summary>
		private void TransitionToColonyList() {
			// Try clicking the back button
			try {
				var traverse = Traverse.Create(_screen);
				var backButton = traverse.Field("backButton")
					.GetValue<KButton>();
				if (backButton != null) {
					backButton.SignalClick(KKeyCode.Mouse0);
				}
			} catch (System.Exception) { }

			_inColonySaveView = false;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			// Speak screen name then first colony
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
			} catch (System.Exception) {
				return false;
			}
		}
	}
}
