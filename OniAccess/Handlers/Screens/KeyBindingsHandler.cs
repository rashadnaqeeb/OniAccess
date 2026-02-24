using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for InputBindingsScreen -- keyboard rebinding UI.
	/// Paginated categories (Global, Tool, Management, etc.) with Prev/Next navigation.
	/// Each page shows binding rows: action name + current key + rebind button.
	///
	/// Special rebind mode: when the user activates a binding row, the game enters
	/// waitingForKeyPress mode and scans for the next physical keypress. Our handler
	/// must suppress ALL input during this mode to avoid consuming keys meant for
	/// the rebind scanner.
	///
	/// Category switching via Tab/Shift+Tab clicks prevScreenButton/nextScreenButton.
	/// Reset button resets all bindings to defaults.
	/// Conflict dialogs are handled by ConfirmDialogHandler (auto-activates on stack).
	/// </summary>
	public class KeyBindingsHandler: BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.KEY_BINDINGS;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		/// <summary>
		/// Traverse accessors for private fields on InputBindingsScreen.
		/// </summary>
		private Traverse _parentField;
		private Traverse _waitingField;
		private Traverse _activeScreenField;
		private Traverse _screensField;
		private Traverse _screenTitleField;

		/// <summary>
		/// Public button references accessed directly from the screen.
		/// </summary>
		private KButton _resetButton;
		private KButton _prevScreenButton;
		private KButton _nextScreenButton;

		/// <summary>
		/// Track previous waitingForKeyPress state to detect transitions.
		/// </summary>
		private bool _wasWaiting;

		/// <summary>
		/// The action name that was focused when rebind mode was entered.
		/// Used to announce "Press a key for {action}".
		/// </summary>
		private string _rebindActionName;

		private UnityEngine.Coroutine _rebindCoroutine;

		public KeyBindingsHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		public override void OnActivate() {
			var traverse = Traverse.Create(_screen);

			// Cache Traverse accessors for private fields
			_parentField = traverse.Field("parent");
			_waitingField = traverse.Field("waitingForKeyPress");
			_activeScreenField = traverse.Field("activeScreen");
			_screensField = traverse.Field("screens");
			_screenTitleField = traverse.Field("screenTitle");

			// Cache public button references
			_resetButton = traverse.Field<KButton>("resetButton").Value;
			_prevScreenButton = traverse.Field<KButton>("prevScreenButton").Value;
			_nextScreenButton = traverse.Field<KButton>("nextScreenButton").Value;

			_wasWaiting = false;
			_rebindActionName = null;

			base.OnActivate();
		}

		public override void OnDeactivate() {
			if (_rebindCoroutine != null) {
				_screen.StopCoroutine(_rebindCoroutine);
				_rebindCoroutine = null;
			}
			base.OnDeactivate();
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			var parentObj = _parentField?.GetValue<UnityEngine.GameObject>();
			if (parentObj == null) {
				Log.Debug("KeyBindingsHandler: parent GameObject is null");
				return true;
			}

			var parentTransform = parentObj.transform;

			// Walk active children of parent -- each is a HorizontalLayoutGroup row from entryPool
			for (int i = 0; i < parentTransform.childCount; i++) {
				var row = parentTransform.GetChild(i);
				if (!row.gameObject.activeInHierarchy) continue;

				// Child 0: action name LocText
				// Child 1: key text LocText + KButton for rebinding
				if (row.childCount < 2) continue;

				var actionLocText = row.GetChild(0).GetComponentInChildren<LocText>();
				var keyLocText = row.GetChild(1).GetComponentInChildren<LocText>();
				var rebindButton = row.GetComponentInChildren<KButton>();

				if (actionLocText == null || rebindButton == null) continue;

				string actionName = actionLocText.text;
				if (string.IsNullOrEmpty(actionName)) continue;

				var keyRef = keyLocText;
				string bindingLabel = actionName;
				_widgets.Add(new ButtonWidget {
					Label = actionName,
					Component = rebindButton,
					GameObject = row.gameObject,
					Tag = keyLocText,
					SpeechFunc = () => {
						string keyText = keyRef != null ? keyRef.text : null;
						if (string.IsNullOrEmpty(keyText) || keyText == "None")
							return $"{bindingLabel}, {(string)STRINGS.ONIACCESS.KEY_BINDINGS.UNBOUND}";
						return $"{bindingLabel}, {keyText}";
					}
				});
			}

			// Add reset button as final widget
			if (_resetButton != null && _resetButton.gameObject.activeInHierarchy) {
				_widgets.Add(new ButtonWidget {
					Label = (string)STRINGS.ONIACCESS.KEY_BINDINGS.RESET_ALL,
					Component = _resetButton,
					GameObject = _resetButton.gameObject
				});
			}

			Log.Debug($"KeyBindingsHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		// ========================================
		// SPEECH
		// ========================================


		// ========================================
		// TICK: REBIND MODE + KEY DETECTION
		// ========================================

		public override bool Tick() {
			bool isWaiting = _waitingField?.GetValue<bool>() ?? false;

			// Detect transitions in/out of rebind mode
			if (isWaiting && !_wasWaiting) {
				// Entered rebind mode -- announce what action we're rebinding
				string actionName = _rebindActionName ?? "unknown";
				string msg = string.Format(
					(string)STRINGS.ONIACCESS.KEY_BINDINGS.PRESS_KEY_FOR, actionName);
				Speech.SpeechPipeline.SpeakInterrupt(msg);
				_wasWaiting = true;
				return false;
			}

			if (!isWaiting && _wasWaiting) {
				// Exited rebind mode -- display was rebuilt, rediscover and announce
				_wasWaiting = false;
				DiscoverWidgets(_screen);
				if (_currentIndex >= _widgets.Count)
					_currentIndex = _widgets.Count > 0 ? _widgets.Count - 1 : 0;
				if (_widgets.Count > 0)
					SpeakCurrentWidget();
				return false;
			}

			// While waiting for keypress, suppress ALL mod input
			if (isWaiting) return false;

			return base.Tick();
		}

		// ========================================
		// HANDLE KEY DOWN: SUPPRESS DURING REBIND
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			bool isWaiting = _waitingField?.GetValue<bool>() ?? false;
			if (isWaiting) return false;

			return base.HandleKeyDown(e);
		}

		// ========================================
		// WIDGET ACTIVATION
		// ========================================

		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			// Reset button: click it, then rediscover and announce
			if (widget.Component == _resetButton) {
				ClickButton(_resetButton);
				// OnReset runs synchronously: resets bindings + calls BuildDisplay()
				DiscoverWidgets(_screen);
				_currentIndex = 0;
				Speech.SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.KEY_BINDINGS.BINDINGS_RESET);
				return;
			}

			// Binding row: defer the click by one frame so GetKeyDown(Return)
			// has cleared before the game's rebind scanner runs in Update().
			// Without this, Enter itself gets immediately bound to the action.
			_rebindActionName = widget.Label;
			var button = widget.Component as KButton;
			if (button != null) {
				if (_rebindCoroutine != null)
					_screen.StopCoroutine(_rebindCoroutine);
				_rebindCoroutine = _screen.StartCoroutine(DeferredRebindClick(button));
			}
		}

		private IEnumerator DeferredRebindClick(KButton button) {
			yield return null;
			_rebindCoroutine = null;
			ClickButton(button);
		}

		// ========================================
		// TAB NAVIGATION: CATEGORY SWITCHING
		// ========================================

		protected override void NavigateTabForward() {
			if (_nextScreenButton == null) return;

			int before = _activeScreenField?.GetValue<int>() ?? -1;
			ClickButton(_nextScreenButton);
			int after = _activeScreenField?.GetValue<int>() ?? -1;

			OnCategoryChanged(after < before);
		}

		protected override void NavigateTabBackward() {
			if (_prevScreenButton == null) return;

			int before = _activeScreenField?.GetValue<int>() ?? -1;
			ClickButton(_prevScreenButton);
			int after = _activeScreenField?.GetValue<int>() ?? -1;

			OnCategoryChanged(after > before);
		}

		/// <summary>
		/// After a category switch: rediscover widgets, announce category + first binding.
		/// </summary>
		private void OnCategoryChanged(bool wrapped) {
			if (wrapped) PlayWrapSound();

			DiscoverWidgets(_screen);
			_currentIndex = 0;

			string category = GetCurrentCategoryName();
			if (_widgets.Count > 0) {
				string widgetText = GetWidgetSpeechText(_widgets[0]);
				Speech.SpeechPipeline.SpeakInterrupt($"{category}, {widgetText}");
			} else {
				Speech.SpeechPipeline.SpeakInterrupt(category);
			}
		}

		/// <summary>
		/// Read the current category name from the screenTitle LocText.
		/// </summary>
		private string GetCurrentCategoryName() {
			var titleLocText = _screenTitleField?.GetValue<LocText>();
			if (titleLocText != null && !string.IsNullOrEmpty(titleLocText.text))
				return titleLocText.text;

			// Fallback: build from screens list + activeScreen index
			var screens = _screensField?.GetValue<List<string>>();
			int idx = _activeScreenField?.GetValue<int>() ?? -1;
			if (screens != null && idx >= 0 && idx < screens.Count)
				return screens[idx];

			return (string)STRINGS.ONIACCESS.HANDLERS.KEY_BINDINGS;
		}
	}
}
