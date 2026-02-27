namespace OniAccess.Handlers.Screens.Skills

internal interface ISkillsTab (line 2)
  string TabName { get; } (line 3)
  void OnTabActivated(bool announce) (line 4)
  void OnTabDeactivated() (line 5)
  bool HandleInput() (line 6)
  bool HandleKeyDown(KButtonEvent e) (line 7)
