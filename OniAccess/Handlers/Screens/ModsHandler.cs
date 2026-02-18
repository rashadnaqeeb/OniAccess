using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
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
	public class ModsHandler: BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.MODS;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ModsHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override bool DiscoverWidgets(KScreen screen) {
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

					string label = titleText != null ? titleText.text : null;
					if (string.IsNullOrEmpty(label)) continue;

					// EnabledToggle MultiToggle for the enabled/disabled state
					MultiToggle toggle = null;
					if (hierRef.HasReference("EnabledToggle"))
						toggle = hierRef.GetReference<MultiToggle>("EnabledToggle");

					if (toggle == null) continue;

					var modToggle = toggle;
					string modLabel = label;
					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = toggle,
						Type = WidgetType.Toggle,
						GameObject = child.gameObject,
						SpeechFunc = () => {
							string state = modToggle.CurrentState == 1 ? (string)STRINGS.ONIACCESS.STATES.ENABLED : (string)STRINGS.ONIACCESS.STATES.DISABLED;
							return $"{modLabel}, {state}";
						}
					});

					// ManageButton — "Browse" for local mods, "Subscription" for Workshop
					if (hierRef.HasReference("ManageButton")) {
						var manageBtn = hierRef.GetReference<KButton>("ManageButton");
						if (manageBtn != null && manageBtn.isInteractable) {
							var manageText = manageBtn.GetComponentInChildren<LocText>();
							string manageLabel = manageText != null ? manageText.text : (string)STRINGS.ONIACCESS.BUTTONS.MANAGE;
							var btn = manageBtn;
							_widgets.Add(new WidgetInfo {
								Label = manageLabel,
								Component = manageBtn,
								Type = WidgetType.Button,
								GameObject = manageBtn.gameObject,
								SpeechFunc = () => {
									var lt = btn.GetComponentInChildren<LocText>();
									return lt != null && !string.IsNullOrEmpty(lt.text) ? lt.text : manageLabel;
								}
							});
						}
					}
				}
			}

			// Append action buttons
			int beforeToggleAll = _widgets.Count;
			WidgetDiscoveryUtil.TryAddButtonField(screen, "toggleAllButton", STRINGS.ONIACCESS.BUTTONS.TOGGLE_ALL, _widgets);
			// Toggle All label changes between "ENABLE ALL"/"DISABLE ALL"; read LocText live
			if (_widgets.Count > beforeToggleAll) {
				var toggleAllWidget = _widgets[beforeToggleAll];
				var toggleAllBtn = toggleAllWidget.Component as KButton;
				if (toggleAllBtn != null) {
					var fallback = toggleAllWidget.Label;
					toggleAllWidget.SpeechFunc = () => {
						var lt = toggleAllBtn.GetComponentInChildren<LocText>();
						return lt != null && !string.IsNullOrEmpty(lt.text) ? lt.text : fallback;
					};
				}
			}
			WidgetDiscoveryUtil.TryAddButtonField(screen, "workshopButton", STRINGS.UI.FRONTEND.SCENARIOS_MENU.BUTTON_WORKSHOP, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", STRINGS.UI.TOOLTIPS.CLOSETOOLTIP, _widgets);

			Log.Debug($"ModsHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}


		/// <summary>
		/// Mod toggles and Toggle All rebuild the widget list, so we rediscover after clicking.
		/// Other buttons (Workshop, Close): handled by base.
		/// </summary>
		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (widget.Type == WidgetType.Toggle) {
				var multiToggle = widget.Component as MultiToggle;
				if (multiToggle != null) {
					string label = widget.Label;
					ClickMultiToggle(multiToggle);
					RediscoverAndRestore(label);
					return;
				}
			}

			if (widget.Type == WidgetType.Button) {
				var kb = widget.Component as KButton;
				if (kb != null) {
					string fieldName = GetButtonFieldName(kb);
					if (fieldName == "toggleAllButton") {
						ClickButton(kb);
						RediscoverAndRestore(null);
						return;
					}
				}
			}

			base.ActivateCurrentItem();
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
