using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the entity action surface into one flat "Actions" section:
	///   1. User menu buttons (from UserMenuScreen.buttonInfos)
	///   2. Priority widget (when entity has Prioritizable)
	///   3. Title bar buttons: Codex Entry, Pin Resource, Rename, Random Name
	///      (each only when its GameObject is active)
	/// </summary>
	class ActionsTab: IDetailTab {
		public string DisplayName =>
			(string)STRINGS.ONIACCESS.DETAILS.ACTIONS_TAB;
		public int StartLevel => 1;
		public string GameTabId => null;

		public bool IsAvailable(GameObject target) => target != null;

		public void OnTabSelected() { }

		public void Populate(GameObject target, List<DetailSection> sections) {
			var ds = DetailsScreen.Instance;
			if (ds == null) return;

			var section = new DetailSection();
			section.Header = (string)STRINGS.ONIACCESS.DETAILS.ACTIONS_TAB;

			AddUserMenuButtons(ds, section.Items);
			AddPriorityWidget(target, section.Items);
			AddTitleBarButtons(ds, section.Items);

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddUserMenuButtons(
				DetailsScreen ds, List<Widget> items) {
			var userMenu = GetUserMenuScreen(ds);
			if (userMenu == null) return;

			// Guard against stale buttons: UserMenuScreen.Refresh only runs
			// after the game event fires for the current entity. If the menu's
			// selected target doesn't match, buttons belong to the old entity.
			try {
				var menuTarget = Traverse.Create(userMenu)
					.Field<GameObject>("selected").Value;
				if (menuTarget != ds.target) return;
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"ActionsTab: selected read failed: {ex.Message}");
			}

			List<KIconButtonMenu.ButtonInfo> buttonInfos;
			try {
				buttonInfos = Traverse.Create(userMenu)
					.Field<List<KIconButtonMenu.ButtonInfo>>("buttonInfos").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"ActionsTab: buttonInfos read failed: {ex.Message}");
				return;
			}
			if (buttonInfos == null) return;

			foreach (var info in buttonInfos) {
				if (info == null) continue;
				var captured = info;
				items.Add(new UserMenuButtonWidget {
					Label = captured.text,
					SpeechFunc = () => captured.text,
					OnClick = captured.onClick,
					IsInteractableFunc = () => captured.isInteractable
				});
			}
		}

		private static UserMenuScreen GetUserMenuScreen(DetailsScreen ds) {
			var panel = ds.UserMenuPanel;
			if (panel == null) return null;
			return panel.GetComponentInChildren<UserMenuScreen>(true);
		}

		private static void AddPriorityWidget(
				GameObject target, List<Widget> items) {
			var prioritizable = target.GetComponent<Prioritizable>();
			if (prioritizable == null || !prioritizable.IsPrioritizable()) return;

			items.Add(new PriorityWidget {
				Label = (string)STRINGS.ONIACCESS.DETAILS.PRIORITY,
				Prioritizable = prioritizable
			});
		}

		private static void AddTitleBarButtons(
				DetailsScreen ds, List<Widget> items) {
			WidgetDiscoveryUtil.TryAddButtonField(
				ds, "CodexEntryButton",
				(string)STRINGS.UI.TOOLTIPS.OPEN_CODEX_ENTRY, items);

			WidgetDiscoveryUtil.TryAddButtonField(
				ds, "PinResourceButton",
				(string)STRINGS.ONIACCESS.DETAILS.PIN_RESOURCE, items);

			try {
				var tabTitle = Traverse.Create(ds)
					.Field<EditableTitleBar>("TabTitle").Value;
				if (tabTitle == null) return;

				TryAddKButton(tabTitle.editNameButton,
					(string)STRINGS.ONIACCESS.PANELS.RENAME, items);
				TryAddKButton(tabTitle.randomNameButton,
					(string)STRINGS.ONIACCESS.PANELS.SHUFFLE_NAME, items);
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"ActionsTab: TabTitle read failed: {ex.Message}");
			}
		}

		private static void TryAddKButton(
				KButton button, string fallbackLabel, List<Widget> items) {
			if (button == null || !button.gameObject.activeInHierarchy) return;

			var captured = button;
			items.Add(new ButtonWidget {
				Label = fallbackLabel,
				Component = captured,
				GameObject = captured.gameObject,
				SpeechFunc = () =>
					WidgetOps.GetButtonLabel(captured, fallbackLabel)
			});
		}
	}
}
