namespace OniAccess.Handlers.Screens.Codex {
	internal interface ICodexTab {
		string TabName { get; }
		void OnTabActivated(bool announce);
		void OnTabDeactivated();

		/// <summary>
		/// Handle one frame of input. Called by CodexScreenHandler.Tick()
		/// after Tab has already been consumed by the parent.
		/// </summary>
		bool HandleInput();

		/// <summary>
		/// Handle Escape interception from KButtonEvent.
		/// </summary>
		bool HandleKeyDown(KButtonEvent e);
	}
}
