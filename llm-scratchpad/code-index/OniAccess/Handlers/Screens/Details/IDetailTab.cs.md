namespace OniAccess.Handlers.Screens.Details

// Contract for tab readers in the details screen handler.
// Each implementation knows how to read one tab's UI into structured sections.

interface IDetailTab (line 9)
  // Localized tab name spoken on tab switch (from game's UI.DETAILTABS strings).
  string DisplayName { get; } (line 13)

  // Navigation level to start at when this tab is selected.
  // 0 = root level, 1 = drilled into first child level.
  int StartLevel { get; } (line 19)

  // Game's DetailTabHeader tab ID (e.g., "SIMPLEINFO", "DETAILS").
  // Null for side screen tabs which use a separate tab header.
  // When non-null, the handler switches the game's visual tab to match.
  string GameTabId { get; } (line 26)

  // Whether this tab applies to the given entity.
  // Unavailable tabs are silently skipped during Tab/Shift+Tab cycling.
  bool IsAvailable(GameObject target) (line 32)

  // Called when this tab is selected (tab switch or initial activation).
  // Side screen tabs click their game-side MultiToggle so the game shows
  // the correct tab body. Game tabs (GameTabId != null) leave this as a
  // no-op since SwitchGameTab handles them via DetailTabHeader.ChangeTab.
  void OnTabSelected() (line 40)

  // Read the tab's live UI into structured sections.
  // Called on target change, tab switch, and before keypress processing.
  // The handler owns the list and clears it before calling.
  void Populate(GameObject target, List<DetailSection> sections) (line 47)
