namespace OniAccess.Handlers.Screens.Details

// Reads active Config side screens into sections for the details screen.
// Each active SideScreenContent becomes one section whose header is
// the screen's GetTitle() and whose items are discovered by SideScreenWalker.

class ConfigSideTab : IDetailTab (line 13)
  public string DisplayName { get; }  (line 14)  // => STRINGS.UI.DETAILTABS.CONFIGURATION.NAME
  public int StartLevel { get; }      (line 15)  // => 1
  public string GameTabId { get; }    (line 16)  // => null (side screen tab)

  public bool IsAvailable(GameObject target) (line 18)
  // Checks DetailsScreen.SidescreenTabTypes.Config tab visibility.

  public void OnTabSelected() (line 24)
  // Clicks the Config tab MultiToggle to activate the game's side tab.

  public void Populate(GameObject target, List<DetailSection> sections) (line 31)
  // Iterates GetActiveScreens(Config) and uses SideScreenWalker.Walk per screen.

  // Yields active SideScreenContent instances for the given tab type.
  // Reads DetailsScreen.sideScreens via Traverse; logs and yields-breaks on failure.
  internal static IEnumerable<SideScreenContent> GetActiveScreens(DetailsScreen ds, DetailsScreen.SidescreenTabTypes tabType) (line 45)
  // Also used by ErrandsSideTab to iterate the Errands tab's screens.
