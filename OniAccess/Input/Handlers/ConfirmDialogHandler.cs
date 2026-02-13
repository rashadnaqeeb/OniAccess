using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for confirmation dialogs (ConfirmDialogScreen).
	/// Per locked decision: confirmation dialogs are treated as a vertical list.
	/// Focus starts on the dialog message text (Label widget), then buttons below.
	///
	/// ConfirmDialogScreen inherits KModalScreen (not KButtonMenu), so we manually
	/// find the message text and confirm/cancel buttons via Traverse and child walks.
	/// </summary>
	public class ConfirmDialogHandler: BaseMenuHandler {
		private string _dialogTitle;

		public override string DisplayName => _dialogTitle
			?? (string)STRINGS.ONIACCESS.HANDLERS.CONFIRM_DIALOG;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ConfirmDialogHandler(KScreen screen) : base(screen) {
			var entries = new List<HelpEntry>();
			entries.AddRange(CommonHelpEntries);
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			HelpEntries = entries;
		}

		public override void OnActivate() {
			// Try to extract the dialog title from the screen's title LocText
			TryExtractTitle(_screen);
			base.OnActivate();
		}

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// Find the dialog message text via popupMessage field or titleText field
			string messageText = null;

			// Try popupMessage first (ConfirmDialogScreen's main message)
			var popupMessage = Traverse.Create(screen).Field("popupMessage")
				.GetValue<LocText>();
			if (popupMessage != null && !string.IsNullOrEmpty(popupMessage.text)) {
				messageText = popupMessage.text;
			}

			// If no popupMessage, search for a child LocText with content
			if (string.IsNullOrEmpty(messageText)) {
				var locTexts = screen.GetComponentsInChildren<LocText>(false);
				foreach (var lt in locTexts) {
					if (lt != null && !string.IsNullOrEmpty(lt.text)
						&& lt.text.Length > 10) // Skip short labels like button text
					{
						messageText = lt.text;
						break;
					}
				}
			}

			// Add message as a Label widget (readable, not clickable)
			if (!string.IsNullOrEmpty(messageText)) {
				_widgets.Add(new WidgetInfo {
					Label = messageText,
					Component = null,
					Type = WidgetType.Label,
					GameObject = screen.gameObject
				});
			}

			// Find confirm button
			var confirmButton = Traverse.Create(screen).Field("confirmButton")
				.GetValue<KButton>();
			if (confirmButton != null && confirmButton.gameObject.activeInHierarchy) {
				string confirmLabel = GetButtonLabel(confirmButton, "OK");
				_widgets.Add(new WidgetInfo {
					Label = confirmLabel,
					Component = confirmButton,
					Type = WidgetType.Button,
					GameObject = confirmButton.gameObject
				});
			}

			// Find cancel button
			var cancelButton = Traverse.Create(screen).Field("cancelButton")
				.GetValue<KButton>();
			if (cancelButton != null && cancelButton.gameObject.activeInHierarchy) {
				string cancelLabel = GetButtonLabel(cancelButton, "Cancel");
				_widgets.Add(new WidgetInfo {
					Label = cancelLabel,
					Component = cancelButton,
					Type = WidgetType.Button,
					GameObject = cancelButton.gameObject
				});
			}

			// If no named buttons found, walk children for any KButton instances
			if (confirmButton == null && cancelButton == null) {
				var kbuttons = screen.GetComponentsInChildren<KButton>(false);
				foreach (var kb in kbuttons) {
					if (kb == null || !kb.gameObject.activeInHierarchy
						|| !kb.isInteractable) continue;

					string label = GetButtonLabel(kb, null);
					if (string.IsNullOrEmpty(label)) continue;

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = kb,
						Type = WidgetType.Button,
						GameObject = kb.gameObject
					});
				}
			}

			Util.Log.Debug($"ConfirmDialogHandler.DiscoverWidgets: {_widgets.Count} widgets");
		}

		/// <summary>
		/// Extract button label from its child LocText, or use a fallback.
		/// </summary>
		private string GetButtonLabel(KButton button, string fallback) {
			var locText = button.GetComponentInChildren<LocText>();
			if (locText != null && !string.IsNullOrEmpty(locText.text))
				return locText.text;
			return fallback;
		}

		/// <summary>
		/// Try to extract a title from the dialog's titleText field.
		/// If found, use it as the DisplayName instead of the generic "Confirm".
		/// </summary>
		private void TryExtractTitle(KScreen screen) {
			try {
				var titleText = Traverse.Create(screen).Field("titleText")
					.GetValue<LocText>();
				if (titleText != null && !string.IsNullOrEmpty(titleText.text)) {
					_dialogTitle = titleText.text;
				}
			} catch (System.Exception) {
				// titleText field may not exist on all dialog types
			}
		}
	}
}
