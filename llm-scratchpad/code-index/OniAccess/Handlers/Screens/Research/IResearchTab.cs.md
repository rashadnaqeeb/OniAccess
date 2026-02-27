namespace OniAccess.Handlers.Screens.Research

// Contract for research screen tabs. Tab objects are composed inside
// ResearchScreenHandler — they are never pushed onto the HandlerStack.
// The parent handler delegates input to the active tab after consuming
// Tab for tab cycling.

internal interface IResearchTab (line 8)
  string TabName { get; } (line 9)
  void OnTabActivated(bool announce) (line 10)
  void OnTabDeactivated() (line 11)

  // Handle one frame of input. Called by ResearchScreenHandler.Tick()
  // after Tab has already been consumed by the parent.
  // Returns true if a key was consumed.
  bool HandleInput() (line 18)

  // Handle Escape interception from KButtonEvent.
  // Returns true if Escape was consumed (e.g., clearing search).
  bool HandleKeyDown(KButtonEvent e) (line 24)
