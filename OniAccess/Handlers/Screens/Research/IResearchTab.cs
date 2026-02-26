namespace OniAccess.Handlers.Screens.Research {
	/// <summary>
	/// Contract for research screen tabs. Tab objects are composed inside
	/// ResearchScreenHandler — they are never pushed onto the HandlerStack.
	/// The parent handler delegates input to the active tab after consuming
	/// Tab for tab cycling.
	/// </summary>
	internal interface IResearchTab {
		string TabName { get; }
		void OnTabActivated(bool announce);
		void OnTabDeactivated();

		/// <summary>
		/// Handle one frame of input. Called by ResearchScreenHandler.Tick()
		/// after Tab has already been consumed by the parent.
		/// Returns true if a key was consumed.
		/// </summary>
		bool HandleInput();

		/// <summary>
		/// Handle Escape interception from KButtonEvent.
		/// Returns true if Escape was consumed (e.g., clearing search).
		/// </summary>
		bool HandleKeyDown(KButtonEvent e);
	}
}
