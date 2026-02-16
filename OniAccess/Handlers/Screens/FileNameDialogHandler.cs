using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for FileNameDialog (filename entry for new saves).
	///
	/// Minimal handler: the dialog has one always-active text field and
	/// the game handles Enter (confirm) and Escape (cancel) natively.
	/// Just speaks the screen name + current input text on activation
	/// and sits on the HandlerStack to block input fallthrough.
	/// </summary>
	public class FileNameDialogHandler: BaseScreenHandler {
		public override bool CapturesAllInput => true;

		public override string DisplayName => (string)STRINGS.UI.FRONTEND.SAVESCREEN.SAVENAMETITLE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; } = new List<HelpEntry>();

		public FileNameDialogHandler(KScreen screen) : base(screen) { }

		public override void OnActivate() {
			string inputText = null;
			try {
				var inputField = Traverse.Create(_screen).Field("inputField")
					.GetValue<KInputTextField>();
				if (inputField != null && !string.IsNullOrEmpty(inputField.text))
					inputText = inputField.text;
			} catch (System.Exception ex) {
				Util.Log.Error($"FileNameDialogHandler.OnActivate: {ex.Message}");
			}

			if (!string.IsNullOrEmpty(inputText))
				Speech.SpeechPipeline.SpeakInterrupt($"{DisplayName}, {inputText}");
			else
				Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
		}
	}
}
