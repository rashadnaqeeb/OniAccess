namespace OniAccess.Handlers {
	/// <summary>
	/// Default handler for world view. Selective claim (CapturesAllInput = false).
	/// Active when no menu/overlay is open. Per locked decision: when mod is on,
	/// there is always an active handler -- WorldHandler is that baseline.
	///
	/// F12 help is handled centrally by KeyPoller.
	/// Phase 4 adds arrow keys for cursor movement.
	/// </summary>
	public class WorldHandler: IAccessHandler {
		public string DisplayName => STRINGS.ONIACCESS.HANDLERS.WORLD_VIEW;
		public bool CapturesAllInput => false;

		public System.Collections.Generic.IReadOnlyList<HelpEntry> HelpEntries { get; }
			= new System.Collections.Generic.List<HelpEntry>().AsReadOnly();

		public void Tick() {
		}

		public bool HandleKeyDown(KButtonEvent e) => false;

		public void OnActivate() {
			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		public void OnDeactivate() { }
	}
}
