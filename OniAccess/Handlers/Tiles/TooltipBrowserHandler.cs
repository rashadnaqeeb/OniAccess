using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Navigable browser for tooltip lines. Pushed onto the HandlerStack when I
	/// is pressed in TileCursorHandler. Extends BaseMenuHandler for 1D navigation
	/// with type-ahead search, Home/End, and wrap sounds. No KScreen.
	/// Escape or I closes the browser and returns to the tile cursor.
	/// </summary>
	public class TooltipBrowserHandler: BaseMenuHandler {
		private readonly IReadOnlyList<string> _lines;

		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.TOOLTIP_BROWSER;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }
			= BuildBrowserHelpEntries();

		public TooltipBrowserHandler(IReadOnlyList<string> lines) {
			_lines = lines;
		}

		public override int ItemCount => _lines.Count;

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _lines.Count) return null;
			return _lines[index];
		}

		public override void SpeakCurrentItem() {
			if (_currentIndex >= 0 && _currentIndex < _lines.Count)
				SpeechPipeline.SpeakInterrupt(
					TextFilter.FilterForSpeech(_lines[_currentIndex]));
		}

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_currentIndex = 0;
			_search.Clear();
			if (_lines.Count > 0)
				SpeechPipeline.SpeakInterrupt(
					TextFilter.FilterForSpeech(_lines[_currentIndex]));
		}

		public override void OnDeactivate() {
			PlaySound("HUD_Click_Close");
			base.OnDeactivate();
		}

		public override void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)) {
				Close();
				return;
			}
			base.Tick();
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				Close();
				return true;
			}
			return false;
		}

		private void Close() {
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
			HandlerStack.Pop();
		}

		private static void PlaySound(string name) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(name));
			} catch (System.Exception ex) {
				OniAccess.Util.Log.Error($"TooltipBrowserHandler.PlaySound failed: {ex.Message}");
			}
		}

		private static IReadOnlyList<HelpEntry> BuildBrowserHelpEntries() {
			return new List<HelpEntry> {
				new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
				new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
				new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
				new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE),
				new HelpEntry("I", STRINGS.ONIACCESS.HELP.CLOSE),
			}.AsReadOnly();
		}
	}
}
