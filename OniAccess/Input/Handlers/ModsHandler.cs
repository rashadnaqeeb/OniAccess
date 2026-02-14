using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for ModsScreen: mod management from the main menu.
	///
	/// Discovers mod entries from entryParent children (each with HierarchyReferences
	/// containing "Title" LocText and "EnabledToggle" MultiToggle), plus action buttons
	/// (Toggle All, Workshop, Close).
	///
	/// Toggling a mod or clicking Toggle All triggers BuildDisplay which destroys and
	/// recreates all entries. RediscoverAndRestore re-discovers widgets and restores
	/// the cursor position by label match.
	/// </summary>
	public class ModsHandler: BaseMenuHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.MODS;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ModsHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// Walk entryParent's direct children — each is a mod entry with HierarchyReferences
			var entryParent = Traverse.Create(screen).Field<UnityEngine.Transform>("entryParent").Value;
			if (entryParent != null) {
				for (int i = 0; i < entryParent.childCount; i++) {
					var child = entryParent.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;

					var hierRef = child.GetComponent<HierarchyReferences>();
					if (hierRef == null) continue;

					// Title LocText for the mod name
					LocText titleText = null;
					if (hierRef.HasReference("Title"))
						titleText = hierRef.GetReference<LocText>("Title");

					string label = titleText != null ? titleText.text : child.gameObject.name;
					if (string.IsNullOrEmpty(label)) continue;

					// EnabledToggle MultiToggle for the enabled/disabled state
					MultiToggle toggle = null;
					if (hierRef.HasReference("EnabledToggle"))
						toggle = hierRef.GetReference<MultiToggle>("EnabledToggle");

					if (toggle == null) continue;

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = toggle,
						Type = WidgetType.Toggle,
						GameObject = child.gameObject
					});

					// ManageButton — "Browse" for local mods, "Subscription" for Workshop
					if (hierRef.HasReference("ManageButton")) {
						var manageBtn = hierRef.GetReference<KButton>("ManageButton");
						if (manageBtn != null && manageBtn.isInteractable) {
							var manageText = manageBtn.GetComponentInChildren<LocText>();
							string manageLabel = manageText != null ? manageText.text : "Manage";
							_widgets.Add(new WidgetInfo {
								Label = manageLabel,
								Component = manageBtn,
								Type = WidgetType.Button,
								GameObject = manageBtn.gameObject
							});
						}
					}
				}
			}

			// Append action buttons
			WidgetDiscoveryUtil.TryAddButtonField(screen, "toggleAllButton", null, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "workshopButton", null, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", null, _widgets);

			Log.Debug($"ModsHandler.DiscoverWidgets: {_widgets.Count} widgets");
		}

		/// <summary>
		/// Accept MultiToggle as valid Toggle (base only accepts KToggle).
		/// Same pattern as OptionsMenuHandler.
		/// </summary>
		protected override bool IsWidgetValid(WidgetInfo widget) {
			if (widget == null || widget.GameObject == null) return false;
			if (!widget.GameObject.activeInHierarchy) return false;

			if (widget.Type == WidgetType.Toggle) {
				var mt = widget.Component as MultiToggle;
				if (mt != null) return true;
				return false;
			}

			return base.IsWidgetValid(widget);
		}

		/// <summary>
		/// Toggles: "ModName, enabled" / "ModName, disabled" via MultiToggle.CurrentState.
		/// Buttons: read live LocText (Toggle All label changes between "ENABLE ALL"/"DISABLE ALL").
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.Type == WidgetType.Toggle) {
				var mt = widget.Component as MultiToggle;
				if (mt != null) {
					string state = mt.CurrentState == 1 ? "enabled" : "disabled";
					return $"{widget.Label}, {state}";
				}
			}

			if (widget.Type == WidgetType.Button) {
				// Re-read live LocText for buttons whose label changes (Toggle All)
				var kb = widget.Component as KButton;
				if (kb != null) {
					var locText = kb.GetComponentInChildren<LocText>();
					if (locText != null && !string.IsNullOrEmpty(locText.text))
						return locText.text;
				}
			}

			return base.GetWidgetSpeechText(widget);
		}

		/// <summary>
		/// Toggle widgets: invoke multiToggle.onClick(), then RediscoverAndRestore by label.
		/// Toggle All button: SignalClick, then RediscoverAndRestore(null) to stay on button.
		/// Other buttons (Workshop, Close): default SignalClick.
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (widget.Type == WidgetType.Toggle) {
				var multiToggle = widget.Component as MultiToggle;
				if (multiToggle != null) {
					string label = widget.Label;
					multiToggle.onClick?.Invoke();
					RediscoverAndRestore(label);
					return;
				}
			}

			if (widget.Type == WidgetType.Button) {
				var kb = widget.Component as KButton;
				if (kb != null) {
					// Toggle All button: triggers BuildDisplay, need to rediscover
					string fieldName = GetButtonFieldName(kb);
					if (fieldName == "toggleAllButton") {
						kb.SignalClick(KKeyCode.Mouse0);
						RediscoverAndRestore(null);
						return;
					}
				}
			}

			// Workshop, Close, or anything else
			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// Identify which button field this KButton corresponds to.
		/// </summary>
		private string GetButtonFieldName(KButton button) {
			var toggleAll = Traverse.Create(_screen).Field<KButton>("toggleAllButton").Value;
			if (button == toggleAll) return "toggleAllButton";
			var workshop = Traverse.Create(_screen).Field<KButton>("workshopButton").Value;
			if (button == workshop) return "workshopButton";
			return "closeButton";
		}

		/// <summary>
		/// Re-discover widgets after BuildDisplay destroys and recreates entries.
		/// If targetLabel is given, find the widget by label match and restore cursor.
		/// Otherwise, clamp to previous index (for Toggle All case).
		/// </summary>
		private void RediscoverAndRestore(string targetLabel) {
			int previousIndex = _currentIndex;
			DiscoverWidgets(_screen);

			if (targetLabel != null) {
				// Find widget by label match
				for (int i = 0; i < _widgets.Count; i++) {
					if (_widgets[i].Label == targetLabel) {
						_currentIndex = i;
						SpeakCurrentWidget();
						return;
					}
				}
			}

			// Clamp to previous index
			if (_widgets.Count > 0) {
				_currentIndex = System.Math.Min(previousIndex, _widgets.Count - 1);
				SpeakCurrentWidget();
			}
		}
	}
}
