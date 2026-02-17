namespace OniAccess.Handlers {
	/// <summary>
	/// Silent baseline handler that sits at the bottom of the handler stack.
	/// Always present when the mod is active, ensuring the stack is never empty.
	/// Announces "Loading" since this handler is typically active during phase
	/// transitions before a real context handler takes over.
	/// </summary>
	public class BaselineHandler: IAccessHandler {
		public string DisplayName => STRINGS.ONIACCESS.HANDLERS.LOADING;
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
