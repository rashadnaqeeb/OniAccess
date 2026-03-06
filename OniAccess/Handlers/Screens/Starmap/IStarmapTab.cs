namespace OniAccess.Handlers.Screens.Starmap {
	internal interface IStarmapTab {
		string TabName { get; }
		void OnTabActivated(bool announce);
		void OnTabDeactivated();
		bool HandleInput();
		bool HandleKeyDown(KButtonEvent e);
	}
}
