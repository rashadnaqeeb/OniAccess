using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
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
	public class LockerMenuHandler: BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.SUPPLY_CLOSET;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		private static readonly string[] MultiToggleFields = {
			"buttonInventory", "buttonDuplicants", "buttonOutfitBroswer", // sic: game typo
			"buttonClaimItems"
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
				if (MultiToggleFields[i] == "buttonClaimItems") {
					// Claim Items: check state to pick the right description
					description = multiToggle.CurrentState == 0
						? STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM_DESCRIPTION
						: STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM_NONE_DESCRIPTION;
				}

				// Claim Items button: SpeechFunc reads live state to append "no items" when empty
				System.Func<string> speechFunc = null;
				if (MultiToggleFields[i] == "buttonClaimItems") {
					var claimToggle = multiToggle;
					string claimLabel = label;
					speechFunc = () => claimToggle.CurrentState != 0
						? $"{claimLabel}, {(string)STRINGS.ONIACCESS.SUPPLY_CLOSET.NO_ITEMS}"
						: claimLabel;
				}

				_widgets.Add(new ButtonWidget {
					Label = label,
					Component = multiToggle,
					GameObject = multiToggle.gameObject,
					Tag = description,
					SpeechFunc = speechFunc
				});
			}

			// Close button
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", STRINGS.UI.TOOLTIPS.CLOSETOOLTIP, _widgets);

			Log.Debug($"LockerMenuHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		private static string GetFallbackLabel(int index) {
			switch (index) {
				case 0: return STRINGS.UI.LOCKER_MENU.BUTTON_INVENTORY;
				case 1: return STRINGS.UI.LOCKER_MENU.BUTTON_DUPLICANTS;
				case 2: return STRINGS.UI.LOCKER_MENU.BUTTON_OUTFITS;
				case 3: return STRINGS.UI.LOCKER_MENU.BUTTON_CLAIM;
				default: return (string)STRINGS.UI.FRONTEND.TRANSLATIONS_SCREEN.UNKNOWN;
			}
		}


		/// <summary>
		/// Return description from Tag for tooltip.
		/// </summary>
		protected override string GetTooltipText(Widget widget) {
			if (widget.Tag is string description && !string.IsNullOrEmpty(description))
				return description;
			return null;
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
				Log.Error($"LockerMenuHandler: failed to check connection icon: {ex.Message}");
			}
		}

		/// <summary>
		/// When a sub-screen is open (LockerNavigator visible), suppress input
		/// so we don't intercept keys meant for the sub-screen.
		/// </summary>
		public override bool Tick() {
			if (IsSubScreenOpen()) return false;
			return base.Tick();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (IsSubScreenOpen()) return false;
			return base.HandleKeyDown(e);
		}

		private static bool IsSubScreenOpen() {
			return LockerNavigator.Instance != null && LockerNavigator.Instance.isActiveAndEnabled;
		}

	}
}
