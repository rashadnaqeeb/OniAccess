# ColonySetupHandler.cs

## File-level / class-level comment

Handler for three screens in the new-game setup flow:
- **ModeSelectScreen** (Survival vs No Sweat) - two MultiToggle buttons
- **ClusterCategorySelectionScreen** (game mode select) - list of MultiToggle buttons
- **ColonyDestinationSelectScreen** (asteroid selection + settings) - flat widget list: cluster selector at index 0 (Left/Right cycles, Enter opens info), then Shuffle/Coordinate/Customize/Launch buttons

Customize button opens a sub-view with three sub-tabs cycled with Tab/Shift+Tab:
Settings -> Mixing -> Story Traits (Escape exits back to main list).

Locked decisions:
- Game mode entries speak name + description
- Cluster selector speaks name, difficulty, N traits, N planetoids
- Enter on cluster selector opens info submenu
- Story traits: name + guaranteed/forbidden state; Enter toggles
- Mixing DLC toggles: name + enabled/disabled; Enter toggles
- Mixing cyclers: name + value; Left/Right cycles
- Settings: "label, value" with Left/Right cycling
- Tab/Shift+Tab is no-op on the main destination screen

---

```
public class ColonySetupHandler : BaseWidgetHandler (line 34)

  // Constants
  private const int SubTabSettings = 0 (line 36)
  private const int SubTabMixing = 1 (line 37)
  private const int SubTabStoryTraits = 2 (line 38)
  private const int SubTabCount = 3 (line 39)

  // Type references (resolved once via AccessTools)
  private static readonly System.Type ClusterCategoryScreenType (line 41)
  private static readonly System.Type ModeSelectScreenType (line 43)

  // Fields
  private List<string> _clusterKeys (line 49)                        -- ordered cluster keys from destination panel
  private int _clusterIndex (line 54)                                -- current position in _clusterKeys for Left/Right cycling
  private bool _inInfoSubmenu (line 59)
  private string _infoClusterKey (line 64)                          -- saved when entering info submenu; used to restore position on exit
  private bool _inCustomize (line 69)
  private int _currentSubTab (line 74)
  private bool _pendingClusterRefresh (line 83)                      -- set after Left/Right cycling or shuffle; causes Tick() to re-discover and speak next frame after traits populate
  private bool _speakClusterNameOnly (line 89)                       -- when true, next cluster speech omits the "Choose a Destination:" prefix (used during repeated cycling)

  // Properties
  public override string DisplayName (line 94)                       -- returns ModeSelectScreen header for mode/category screens; destination title otherwise
  public override IReadOnlyList<HelpEntry> HelpEntries (line 102)
  private bool IsClusterCategoryScreen (line 107)
  private bool IsModeSelectScreen (line 113)

  // Constructor
  public ColonySetupHandler(KScreen screen) (line 116)

  // Tab navigation (Customize sub-tabs only)
  protected override void NavigateTabForward() (line 124)            -- no-op on main screen and mode screens; cycles sub-tab forward in Customize
  protected override void NavigateTabBackward() (line 137)           -- no-op on main screen and mode screens; cycles sub-tab backward in Customize
  private void SyncGameTab() (line 156)                             -- maps our sub-tab index to the game's selectedMenuTabIdx and calls RefreshMenuTabs
  private string GetPanelName() (line 175)                          -- returns localized name of current Customize sub-tab; empty string outside Customize
  private void RediscoverAndReset() (line 191)                      -- clears search, re-discovers widgets, resets _currentIndex to 0
  private void RefreshSubTab() (line 201)                           -- SyncGameTab + RediscoverAndReset + speak panel name + first widget

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 214)   -- dispatches to the right private method based on screen type and current mode
  private void DiscoverGameModeWidgets(KScreen screen) (line 246)   -- ClusterCategorySelectionScreen: walks vanillaStyle/classicStyle/spacedOutStyle/eventStyle fields; combines game STRINGS title + hoverDescriptionText
  private void DiscoverModeSelectWidgets(KScreen screen) (line 288) -- ModeSelectScreen: reads survivalButton + nosweatButton MultiToggles; combines title + description strings
  private bool DiscoverDestinationWidgets(KScreen screen) (line 325) -- main destination list: cluster selector at [0] + Shuffle + Coordinate text field + Customize + Launch; returns false if cluster keys not ready yet
  private void PopulateClusterKeys(KScreen screen) (line 377)       -- extracts clusterKeys list from destinationMapPanel via Traverse; syncs _clusterIndex to game's selectedIndex
  private string BuildClusterSelectorLabel(string clusterKey, bool includePrefix = true) (line 398) -- reads belt data via Traverse; builds "Title: Name, difficulty, N traits, N planetoids"
  private void DiscoverClusterInfoWidgets(KScreen screen) (line 454) -- populates info submenu for _infoClusterKey: description, difficulty, nearby/distant asteroid lists, per-world sections with traits
  private void DiscoverSettingsWidgets(KScreen screen) (line 624)   -- reads CustomGameSettingWidget list from newGameSettingsPanel; handles ListWidget (DropdownWidget), ToggleWidget, and Seed (ButtonWidget that randomizes)
  private void DiscoverStoryTraitWidgets(KScreen screen) (line 717)  -- walks storyRowContainer children; reads names from Db.Get().Stories; reads state from CustomGameSettings API; adds ToggleWidgets
  private void DiscoverMixingWidgets(KScreen screen) (line 804)     -- walks contentPanel sections; emits section LabelWidgets as headers; adds ToggleWidgets for DLC checkboxes and DropdownWidgets for cyclers

  // Widget speech
  private string BuildStoryTraitSpeech(string storyId, string fallbackLabel) (line 971) -- reads live state from CustomGameSettings.GetCurrentStoryTraitSetting; re-fetches name/desc from Db
  private static string BuildSettingDropdownSpeech(CustomGameSettingWidget settingWidget, string fallbackName) (line 1002) -- reads Label + ValueLabel LocTexts; falls back to Cycler/Box/"Value Label" hierarchy if ValueLabel is empty

  // Widget interaction
  protected override void ActivateCurrentItem() (line 1039)         -- dispatches: MultiToggle game modes -> onClick; cluster selector -> enter info submenu; story trait toggle -> checkbox onClick; mixing toggle -> compute+announce new state, fire onClick; Customize button -> CustomizeClicked(); shuffle -> base + _pendingClusterRefresh; settings toggle -> ToggleSetting()+Refresh()+speak; settings seed -> randomize+speak
  private void SelectClusterSilent(string clusterKey) (line 1152)   -- fires OnAsteroidClicked without speaking; speech follows on next frame via _pendingClusterRefresh
  protected override void CycleDropdown(Widget widget, int direction) (line 1173) -- clicks CycleLeft/CycleRight on CustomGameSettingListWidget; falls back to Cycler/Arrow_Left and Arrow_Right child buttons for mixing widgets; calls Refresh() synchronously; only speaks if value actually changed

  // Tick
  public override bool Tick() (line 1245)                           -- handles _pendingClusterRefresh (re-discover + speak current widget); handles Left/Right on cluster selector (cycle + SelectClusterSilent + set _pendingClusterRefresh); delegates to base

  // HandleKeyDown
  public override bool HandleKeyDown(KButtonEvent e) (line 1294)    -- intercepts Escape to exit Customize (calls CustomizeClose) or info submenu; delegates to base first
```
