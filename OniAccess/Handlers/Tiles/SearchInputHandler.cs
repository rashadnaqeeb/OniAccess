using System.Collections.Generic;
using System.Text;
using OniAccess.Handlers.Tiles.Scanner;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	public class SearchInputHandler : IAccessHandler {
		private readonly ScannerNavigator _scanner;
		private readonly StringBuilder _query = new StringBuilder();

		public SearchInputHandler(ScannerNavigator scanner) {
			_scanner = scanner;
		}

		public string DisplayName => (string)STRINGS.ONIACCESS.SCANNER.SEARCH.PROMPT;
		public bool CapturesAllInput => true;
		public IReadOnlyList<HelpEntry> HelpEntries { get; } = new List<HelpEntry>().AsReadOnly();
		public IReadOnlyList<ConsumedKey> ConsumedKeys { get; } = System.Array.Empty<ConsumedKey>();

		public void OnActivate() {
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.SCANNER.SEARCH.PROMPT);
		}

		public void OnDeactivate() { }

		public bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				HandlerStack.Pop();
				if (_query.Length > 0)
					_scanner.SearchRefresh(_query.ToString());
				return true;
			}

			string typed = UnityEngine.Input.inputString;
			for (int i = 0; i < typed.Length; i++) {
				char c = typed[i];
				if (c == '\b') {
					if (_query.Length > 0)
						_query.Length -= 1;
				} else if (c >= ' ') {
					_query.Append(c);
				}
			}

			return true;
		}

		public bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				HandlerStack.Pop();
				return true;
			}
			return false;
		}
	}
}
