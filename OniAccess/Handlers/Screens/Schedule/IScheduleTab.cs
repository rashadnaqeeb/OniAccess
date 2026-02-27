namespace OniAccess.Handlers.Screens.Schedule {
	internal interface IScheduleTab {
		string TabName { get; }
		void OnTabActivated(bool announce);
		void OnTabDeactivated();
		bool HandleInput();
		bool HandleKeyDown(KButtonEvent e);
	}
}
