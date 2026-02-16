using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Input;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for FileNameDialog (filename entry for new saves).
	///
	/// Presents a text field (Enter to edit, Enter to confirm, Escape to cancel)
	/// followed by OK and Cancel buttons. Uses TextEditHelper for text editing,
	/// same pattern as colony name / dupe rename in MinionSelectHandler.
	/// </summary>
	public class FileNameDialogHandler : BaseMenuHandler {
		private readonly TextEditHelper _textEdit = new TextEditHelper();

		public override string DisplayName => (string)STRINGS.UI.FRONTEND.SAVESCREEN.SAVENAMETITLE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public FileNameDialogHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			// The game auto-activates the input field on open. Deactivate it
			// so the user controls when editing starts via Enter.
			try {
				var inputField = Traverse.Create(_screen).Field("inputField")
					.GetValue<KInputTextField>();
				if (inputField != null)
					inputField.DeactivateInputField();
			} catch (System.Exception ex) {
				Util.Log.Error($"FileNameDialogHandler.OnActivate: {ex.Message}");
			}

			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			var st = Traverse.Create(screen);

			// Text input field
			try {
				var inputField = st.Field("inputField").GetValue<KInputTextField>();
				if (inputField != null) {
					_widgets.Add(new WidgetInfo {
						Label = inputField.text,
						Component = inputField,
						Type = WidgetType.TextInput,
						GameObject = inputField.gameObject,
						Tag = "filename"
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"FileNameDialogHandler.DiscoverWidgets(inputField): {ex.Message}");
			}

			// Confirm button
			try {
				var confirmBtn = st.Field("confirmButton").GetValue<KButton>();
				if (confirmBtn != null && confirmBtn.gameObject.activeInHierarchy) {
					_widgets.Add(new WidgetInfo {
						Label = GetButtonLabel(confirmBtn, (string)STRINGS.UI.CONFIRMDIALOG.OK),
						Component = confirmBtn,
						Type = WidgetType.Button,
						GameObject = confirmBtn.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"FileNameDialogHandler.DiscoverWidgets(confirmButton): {ex.Message}");
			}

			// Cancel button
			try {
				var cancelBtn = st.Field("cancelButton").GetValue<KButton>();
				if (cancelBtn != null && cancelBtn.gameObject.activeInHierarchy) {
					_widgets.Add(new WidgetInfo {
						Label = GetButtonLabel(cancelBtn, (string)STRINGS.UI.FRONTEND.NEWGAMESETTINGS.BUTTONS.CANCEL),
						Component = cancelBtn,
						Type = WidgetType.Button,
						GameObject = cancelBtn.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"FileNameDialogHandler.DiscoverWidgets(cancelButton): {ex.Message}");
			}

			Util.Log.Debug($"FileNameDialogHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return _widgets.Count > 0;
		}

		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.Type == WidgetType.TextInput && widget.Component is KInputTextField textField)
				return $"{(string)STRINGS.UI.FRONTEND.SAVESCREEN.SAVENAMETITLE}, {textField.text}";

			return base.GetWidgetSpeechText(widget);
		}

		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (widget.Type == WidgetType.TextInput && widget.Component is KInputTextField textField) {
				if (!_textEdit.IsEditing) {
					_textEdit.Begin(textField);
				} else {
					_textEdit.Confirm();
				}
				return;
			}

			base.ActivateCurrentWidget();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (_textEdit.IsEditing) {
				if (e.TryConsume(Action.Escape)) {
					_textEdit.Cancel();
					return true;
				}
				return false;
			}

			return base.HandleKeyDown(e);
		}

		public override void Tick() {
			if (_textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return))
					_textEdit.Confirm();
				return;
			}

			base.Tick();
		}
	}
}
