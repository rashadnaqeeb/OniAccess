using System.Collections.Generic;

using OniAccess.Speech;
using OniAccess.Widgets;

namespace OniAccess.Handlers {
	/// <summary>
	/// Modal list of the system voices in the game's language (every voice when none
	/// speaks it), pushed from the Speech section of settings. Rows carry the
	/// backend's own voice indices, so the filter never shifts what Enter or Escape
	/// apply. Landing on a row switches the backend to that voice before the row is
	/// spoken, so every announcement is its own preview. Enter keeps the highlighted
	/// voice and saves it; Escape puts back the voice that was in use on open.
	/// </summary>
	public class VoicePickerHandler: NavTreeHandler {
		private readonly IVoiceControl _backend;
		private int _voiceOnOpen = -1;

		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.VOICE_PICKER;
		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public VoicePickerHandler(IVoiceControl backend) : base(null) {
			_backend = backend;
			HelpEntries = new List<HelpEntry> {
				new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Enter", STRINGS.ONIACCESS.CONFIG.HELP_KEEP_VOICE),
				new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
			}.AsReadOnly();
		}

		private List<int> Voices() =>
			SpeechOutputSelector.VoicesForLanguage(_backend, Localization.GetCurrentLanguageCode());

		protected override IReadOnlyList<NavItem> BuildRoots() {
			var voices = Voices();
			var roots = new List<NavItem>(voices.Count);
			foreach (int voice in voices) {
				roots.Add(new MenuNode(
					() => Preview(voice),
					activate: () => { Keep(voice); return true; },
					searchText: () => Label(voice)));
			}
			return roots;
		}

		// The announce callback runs once per landing, right before the text reaches
		// the backend, which is the only point where the voice can be swapped in time
		// for the row to be spoken in it.
		private string Preview(int index) {
			_backend.SetVoice(index);
			return Label(index);
		}

		private string Label(int index) => SpeechOutputSelector.VoiceLabel(_backend, index);

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_voiceOnOpen = _backend.CurrentVoice;
			Nav.Reset();
			int row = Voices().IndexOf(_voiceOnOpen);
			if (row >= 0)
				Nav.SetPath(new[] { row });
			_search.Clear();
			SuppressSearchThisFrame();
			SpeechPipeline.SpeakInterrupt(DisplayName);
			AnnounceCurrent(interrupt: false);
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;
			if (e.TryConsume(Action.Escape)) {
				if (_backend.CurrentVoice != _voiceOnOpen)
					_backend.SetVoice(_voiceOnOpen);
				Close();
				return true;
			}
			return false;
		}

		private void Keep(int index) {
			ConfigManager.Config.SystemVoice = _backend.GetVoiceName(index);
			ConfigManager.Config.SystemVoiceLanguage = _backend.GetVoiceLanguage(index) ?? "";
			ConfigManager.Config.SystemVoiceIdentifier = _backend.GetVoiceIdentifier(index) ?? "";
			ConfigManager.Save();
			_backend.SetVoice(index);
			PlaySound("HUD_Click");
			HandlerStack.Pop();
		}

		private void Close() {
			SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.TOOLTIP.CLOSED);
			PlaySound("HUD_Click_Close");
			HandlerStack.Pop();
		}
	}
}
