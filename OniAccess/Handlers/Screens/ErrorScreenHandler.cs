using System.Collections.Generic;

using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for ReportErrorDialog — the crash/error screen.
	///
	/// ReportErrorDialog is a plain MonoBehaviour (not a KScreen), so the normal
	/// screen lifecycle patches don't apply. Start/OnDestroy patches push/pop
	/// this handler directly.
	///
	/// KScreenManager input is disabled while this dialog is active, so all key
	/// detection uses UnityEngine.Input in Tick(). Enter invokes the continue
	/// button if available; any other key re-speaks the error message.
	/// </summary>
	public class ErrorScreenHandler: IAccessHandler {
		private readonly ReportErrorDialog _dialog;
		private readonly LocText _crashLabel;
		private readonly KButton _continueButton;

		public ReportErrorDialog Dialog => _dialog;
		public string DisplayName => STRINGS.ONIACCESS.HANDLERS.ERROR_SCREEN;
		public bool CapturesAllInput => true;
		public IReadOnlyList<HelpEntry> HelpEntries { get; } = new List<HelpEntry>().AsReadOnly();
		public IReadOnlyList<ConsumedKey> ConsumedKeys { get; } = System.Array.Empty<ConsumedKey>();

		public ErrorScreenHandler(ReportErrorDialog dialog) {
			_dialog = dialog;
			_crashLabel = HarmonyLib.Traverse.Create(dialog).Field<LocText>("CrashLabel").Value;
			_continueButton = HarmonyLib.Traverse.Create(dialog).Field<KButton>("continueGameButton").Value;
		}

		public void OnActivate() {
			SpeechPipeline.SpeakInterrupt(DisplayName + ", " + TextFilter.FilterForSpeech(_crashLabel.text));
		}

		public void OnDeactivate() { }

		public bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return) ||
				UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.KeypadEnter)) {
				if (_continueButton.gameObject.activeSelf) {
					_dialog.OnSelect_CONTINUE();
					SpeechPipeline.SpeakQueued((string)STRINGS.ONIACCESS.TOOLTIP.CONTINUING);
				} else {
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CANNOT_CONTINUE);
				}
				return true;
			}

			if (UnityEngine.Input.anyKeyDown) {
				SpeechPipeline.SpeakInterrupt(TextFilter.FilterForSpeech(_crashLabel.text));
				return true;
			}

			return false;
		}

		public bool HandleKeyDown(KButtonEvent e) => false;
	}
}
