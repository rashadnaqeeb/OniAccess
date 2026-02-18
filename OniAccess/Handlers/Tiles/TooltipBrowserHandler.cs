using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Navigable browser for tooltip lines. Pushed onto the HandlerStack when Q
	/// is pressed in TileCursorHandler. Up/Down arrows step through entries.
	/// Escape or Q closes the browser and returns to the tile cursor.
	/// </summary>
	public class TooltipBrowserHandler : IAccessHandler {
		private readonly IReadOnlyList<string> _lines;
		private int _currentIndex;

		public string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.TOOLTIP_BROWSER;
		public bool CapturesAllInput => true;

		public IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new List<HelpEntry> {
				new HelpEntry("Up/Down", (string)STRINGS.ONIACCESS.HELP.NAVIGATE),
				new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.CLOSE),
				new HelpEntry("Q", (string)STRINGS.ONIACCESS.HELP.CLOSE),
			}.AsReadOnly();

		public TooltipBrowserHandler(IReadOnlyList<string> lines) {
			_lines = lines;
			_currentIndex = 0;
		}

		public void OnActivate() {
			_currentIndex = 0;
			if (_lines.Count > 0)
				SpeechPipeline.SpeakInterrupt(
					TextFilter.FilterForSpeech(_lines[_currentIndex]));
		}

		public void OnDeactivate() {
			_currentIndex = 0;
		}

		public void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Q)) {
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
			if (e.TryConsume(Action.Escape)) {
				Close();
				return true;
			}
			return false;
		}

		private void NavigateNext() {
			if (_lines.Count == 0) return;
			_currentIndex = (_currentIndex + 1) % _lines.Count;
			SpeakCurrentLine();
		}

		private void NavigatePrev() {
			if (_lines.Count == 0) return;
			_currentIndex = (_currentIndex - 1 + _lines.Count) % _lines.Count;
			SpeakCurrentLine();
		}

		private void SpeakCurrentLine() {
			if (_currentIndex >= 0 && _currentIndex < _lines.Count)
				SpeechPipeline.SpeakInterrupt(
					TextFilter.FilterForSpeech(_lines[_currentIndex]));
		}

		private void Close() {
			HandlerStack.Pop();
		}
	}
}
