using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for LockerMenuScreen: the Supply Closet hub accessed from the main menu.
	///
	/// Discovers 4 MultiToggle buttons (Inventory, Duplicants, Wardrobe, Claim Blueprints)
	/// plus a close KButton. Sub-screen handlers (inventory, outfits, etc.) will come later.
	///
	/// Lifecycle note: LockerMenuScreen.OnActivate() immediately calls Show(false) during
	/// prefab init, so the normal KScreen.Activate/Deactivate hooks don't work. A Harmony
	/// patch on LockerMenuScreen.Show pushes/pops this handler instead.
	/// </summary>
	public class LockerMenuHandler : BaseMenuHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.SUPPLY_CLOSET;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		private static readonly string[] MultiToggleFields = {
			"buttonInventory", "buttonDuplicants", "buttonOutfitBroswer", "buttonClaimItems"
		};

		private static readonly string[] DescriptionStrings = {
			STRINGS.UI.LOCKER_MENU.BUTTON_INVENTORY_DESCRIPTION,
			STRINGS.UI.LOCKER_MENU.BUTTON_DUPLICANTS_DESCRIPTION,
			STRINGS.UI.LOCKER_MENU.BUTTON_OUTFITS_DESCRIPTION,
			null // Claim Items description is dynamic -- set in DiscoverWidgets
		};

		public LockerMenuHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			var screenTraverse = Traverse.Create(screen);

			for (int i = 0; i < MultiToggleFields.Length; i++) {
				var multiToggle = screenTraverse.Field<MultiToggle>(MultiToggleFields[i]).Value;
				if (multiToggle == null || !multiToggle.gameObject.activeInHierarchy) continue;

				// Try LocText for the button label
				var locText = multiToggle.GetComponentInChildren<LocText>();
				string label = locText != null && !string.IsNullOrEmpty(locText.text)
					? locText.text
					: GetFallbackLabel(i);

				// Description for tooltip -- Claim Items is dynamic based on state
				string description = DescriptionStrings[i];
				if (i == 3) {
					// Claim Items: check state to pick the right description
					description = multiToggle.CurrentState == 0
						? STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM_DESCRIPTION
						: STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM_NONE_DESCRIPTION;
				}

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = multiToggle,
					Type = WidgetType.Button,
					GameObject = multiToggle.gameObject,
					Tag = description
				});
			}

			// Close button
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", null, _widgets);

			Log.Debug($"LockerMenuHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		private static string GetFallbackLabel(int index) {
			switch (index) {
				case 0: return STRINGS.UI.LOCKER_MENU.BUTTON_INVENTORY;
				case 1: return STRINGS.UI.LOCKER_MENU.BUTTON_DUPLICANTS;
				case 2: return STRINGS.UI.LOCKER_MENU.BUTTON_OUTFITS;
				case 3: return STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM;
				default: return (string)STRINGS.ONIACCESS.BUTTONS.UNKNOWN;
			}
		}

		/// <summary>
		/// Accept MultiToggle as valid Button (base only accepts KButton).
		/// </summary>
		protected override bool IsWidgetValid(WidgetInfo widget) {
			if (widget == null || widget.GameObject == null) return false;
			if (!widget.GameObject.activeInHierarchy) return false;

			if (widget.Type == WidgetType.Button) {
				var mt = widget.Component as MultiToggle;
				if (mt != null) return true;
			}

			return base.IsWidgetValid(widget);
		}

		/// <summary>
		/// For Claim Items, append availability status.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			var mt = widget.Component as MultiToggle;
			if (mt != null && widget == GetClaimItemsWidget()) {
				// State 0 = has claimable items, state 1 = none
				if (mt.CurrentState != 0) {
					return $"{widget.Label}, {(string)STRINGS.ONIACCESS.SUPPLY_CLOSET.NO_ITEMS}";
				}
			}

			return base.GetWidgetSpeechText(widget);
		}

		/// <summary>
		/// Return description from Tag for tooltip.
		/// </summary>
		protected override string GetTooltipText(WidgetInfo widget) {
			if (widget.Tag is string description && !string.IsNullOrEmpty(description))
				return description;
			return null;
		}

		/// <summary>
		/// OnPointerClick for MultiToggle buttons, base behavior for KButton (close).
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			var multiToggle = widget.Component as MultiToggle;
			if (multiToggle != null) {
				var eventData = new UnityEngine.EventSystems.PointerEventData(
					UnityEngine.EventSystems.EventSystem.current) {
					button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
					clickCount = 1
				};
				multiToggle.OnPointerClick(eventData);
				return;
			}

			base.ActivateCurrentWidget();
		}

		public override void OnActivate() {
			base.OnActivate();

			// Announce offline status if not connected to server
			try {
				var noConnectionIcon = Traverse.Create(_screen)
					.Field<UnityEngine.GameObject>("noConnectionIcon").Value;
				if (noConnectionIcon != null && noConnectionIcon.activeSelf) {
					Speech.SpeechPipeline.SpeakQueued(
						(string)STRINGS.ONIACCESS.SUPPLY_CLOSET.OFFLINE);
				}
			} catch (System.Exception ex) {
				Log.Debug($"LockerMenuHandler: failed to check connection icon: {ex.Message}");
			}
		}

		/// <summary>
		/// When a sub-screen is open (LockerNavigator visible), suppress input
		/// so we don't intercept keys meant for the sub-screen.
		/// </summary>
		public override void Tick() {
			if (IsSubScreenOpen()) return;
			base.Tick();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (IsSubScreenOpen()) return false;
			return base.HandleKeyDown(e);
		}

		private static bool IsSubScreenOpen() {
			return LockerNavigator.Instance != null && LockerNavigator.Instance.isActiveAndEnabled;
		}

		private WidgetInfo GetClaimItemsWidget() {
			for (int i = 0; i < _widgets.Count; i++) {
				var mt = _widgets[i].Component as MultiToggle;
				if (mt == null) continue;
				var field = Traverse.Create(_screen).Field<MultiToggle>("buttonClaimItems").Value;
				if (mt == field) return _widgets[i];
			}
			return null;
		}
	}
}
