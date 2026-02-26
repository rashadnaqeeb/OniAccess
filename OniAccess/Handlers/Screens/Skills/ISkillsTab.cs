namespace OniAccess.Handlers.Screens.Skills {
	internal interface ISkillsTab {
		string TabName { get; }
		void OnTabActivated(bool announce);
		void OnTabDeactivated();
		bool HandleInput();
		bool HandleKeyDown(KButtonEvent e);
	}
}
