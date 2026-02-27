namespace OniAccess.Handlers.Screens.Details

// Placeholder tab that produces no widgets.
// Used for tabs not yet implemented in the current phase.

class StubTab : IDetailTab (line 10)
  private readonly string _displayName (line 11)
  private readonly string _gameTabId (line 12)
  private readonly Func<GameObject, bool> _isAvailable (line 13)

  // displayName: Localized tab name from game strings.
  // gameTabId:   Game's DetailTabHeader tab ID (e.g., "SIMPLEINFO"). Null for side screen tabs.
  // isAvailable: Optional predicate. When null, the tab is available for all entities.
  public StubTab(string displayName, string gameTabId = null, Func<GameObject, bool> isAvailable = null) (line 22)

  public string DisplayName { get; }  (line 29)  // => _displayName
  public int StartLevel { get; }      (line 30)  // => 0
  public string GameTabId { get; }    (line 31)  // => _gameTabId

  public bool IsAvailable(GameObject target) (line 33)  // returns true if _isAvailable is null, else calls it

  public void OnTabSelected() (line 36)  // no-op

  public void Populate(GameObject target, List<DetailSection> sections) (line 38)  // no-op
