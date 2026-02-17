using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for WattsonMessage, the welcome screen shown at the start of every
	/// new colony. Presents the welcome narrative as a Label and the dismiss button.
	///
	/// LocText.text returns a MISSING.STRINGS key because TMP's SetText (used by
	/// OnPrefabInit) doesn't update the m_text field that the text getter reads.
	/// We read the welcome message directly from game data instead.
	/// </summary>
	public class WattsonMessageHandler : BaseMenuHandler {
		public override string DisplayName =>
			(string)STRINGS.ONIACCESS.HANDLERS.WELCOME_MESSAGE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public WattsonMessageHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			string text = ReadWelcomeText();
			if (!string.IsNullOrEmpty(text)) {
				_widgets.Add(new WidgetInfo {
					Label = text,
					Component = null,
					Type = WidgetType.Label,
					GameObject = screen.gameObject
				});
			}

			var button = Traverse.Create(screen).Field("button").GetValue<KButton>();
			if (button != null) {
				_widgets.Add(new WidgetInfo {
					Label = GetButtonLabel(button, (string)STRINGS.UI.CONFIRMDIALOG.OK),
					Component = button,
					Type = WidgetType.Button,
					GameObject = button.gameObject
				});
			}

			Util.Log.Debug($"WattsonMessageHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		/// <summary>
		/// Read the welcome message from game data, replicating the logic in
		/// WattsonMessage.OnPrefabInit. LocText.text is unreliable here.
		/// </summary>
		private static string ReadWelcomeText() {
			try {
				var layout = CustomGameSettings.Instance.GetCurrentClusterLayout();
				string welcomeMessage = layout.welcomeMessage;
				if (welcomeMessage != null) {
					return Strings.TryGet(welcomeMessage, out var result)
						? result.String : welcomeMessage;
				}
				if (DlcManager.IsExpansion1Active())
					return (string)STRINGS.UI.WELCOMEMESSAGEBODY_SPACEDOUT;
				return (string)STRINGS.UI.WELCOMEMESSAGEBODY;
			} catch (System.Exception ex) {
				Util.Log.Error($"WattsonMessageHandler.ReadWelcomeText: {ex.Message}");
				return null;
			}
		}
	}
}
