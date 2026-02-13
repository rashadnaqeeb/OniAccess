namespace OniAccess.Input {
	/// <summary>
	/// Handler for F12 help mode. Captures all input.
	/// Speaks help entries one at a time with Up/Down arrow navigation.
	/// Escape or F12 returns to the previous handler.
	///
	/// Per locked decision: F12 opens a navigable list (arrow keys step through entries),
	/// not a speech dump. Show only the active handler's keys.
	/// </summary>
	public class HelpHandler: IAccessHandler {
		private readonly System.Collections.Generic.IReadOnlyList<HelpEntry> _entries;
		private int _currentIndex;

		public string DisplayName => STRINGS.ONIACCESS.HANDLERS.HELP;
		public bool CapturesAllInput => true;

		/// <summary>
		/// HelpHandler's own help entries describe how to navigate the help list itself.
		/// </summary>
		public System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new System.Collections.Generic.List<HelpEntry>
			{
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE),
				new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
				new HelpEntry("F12", STRINGS.ONIACCESS.HELP.CLOSE),
			}.AsReadOnly();

		public HelpHandler(System.Collections.Generic.IReadOnlyList<HelpEntry> entries) {
			_entries = entries ?? new System.Collections.Generic.List<HelpEntry>().AsReadOnly();
			_currentIndex = 0;
		}

		public void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F12)) {
				Close();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				NavigateNext();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				NavigatePrev();
				return;
			}
		}

		public bool HandleKeyDown(KButtonEvent e) {
			// Escape closes help -- Escape IS a game Action, so use TryConsume
			if (e.TryConsume(Action.Escape)) {
				Close();
				return true;
			}
			// All other KButtonEvents are consumed by CapturesAllInput in ModInputRouter
			return false;
		}

		public void OnActivate() {
			// Speak "Help" then queue the first entry so both are heard
			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			if (_entries.Count > 0) {
				_currentIndex = 0;
				Speech.SpeechPipeline.SpeakQueued(_entries[_currentIndex].ToString());
			} else {
				Speech.SpeechPipeline.SpeakQueued(STRINGS.ONIACCESS.SPEECH.NO_COMMANDS);
			}
		}

		public void OnDeactivate() {
			_currentIndex = 0;
		}

		private void NavigateNext() {
			if (_entries.Count == 0) return;
			_currentIndex = (_currentIndex + 1) % _entries.Count;
			SpeakCurrentEntry();
		}

		private void NavigatePrev() {
			if (_entries.Count == 0) return;
			_currentIndex = (_currentIndex - 1 + _entries.Count) % _entries.Count;
			SpeakCurrentEntry();
		}

		private void SpeakCurrentEntry() {
			if (_currentIndex >= 0 && _currentIndex < _entries.Count) {
				Speech.SpeechPipeline.SpeakInterrupt(_entries[_currentIndex].ToString());
			}
		}

		private void Close() {
			HandlerStack.Pop();
		}
	}
}
