namespace OniAccess.Handlers.Screens.Schedule

internal interface IScheduleTab (line 2)
  string TabName { get; } (line 3)
  void OnTabActivated(bool announce) (line 4)
  void OnTabDeactivated() (line 5)
  bool HandleInput() (line 6)
  bool HandleKeyDown(KButtonEvent e) (line 7)
