# DetailsScreenHandler.cs

## File-level / class-level comment

Handler for the `DetailsScreen` (entity inspection panel). Two-level nested navigation: section
headers (level 0) and items within each section (level 1). Manages tab cycling across
informational and side-screen tabs, delegating section population to `IDetailTab` readers.

Lifecycle: Show-patch on `DetailsScreen.OnShow(bool)`. The DetailsScreen is a persistent
singleton that shows/hides rather than activating/deactivating, so KScreen.Activate patches
skip it.

Extends `NestedMenuHandler`.

---

```
public class DetailsScreenHandler : NestedMenuHandler (line 20)

  // Properties
  protected override int StartLevel (line 21)                       -- returns the active tab's StartLevel; used by NestedMenuHandler to decide whether to start at level 0 or 1

  // Fields
  private readonly IDetailTab[] _tabs (line 25)                     -- full set of known tabs built once in constructor via BuildTabs()
  private readonly List<IDetailTab> _activeTabs (line 26)           -- subset of _tabs visible for the current target entity
  private readonly List<int> _sectionStarts (line 27)               -- indices into _activeTabs where a new section (info vs side screen) starts; used by AdvanceSection (Ctrl+Tab)
  private readonly List<DetailSection> _sections (line 28)          -- current tab's section list, rebuilt by RebuildSections()
  private readonly Input.TextEditHelper _textEdit (line 29)
  private int _tabIndex (line 30)
  private int _sectionIndex (line 31)
  private GameObject _lastTarget (line 32)
  private bool _suppressDisplayName (line 33)                       -- suppressed during OnActivate base call to prevent speaking before entity name is known
  private bool _pendingFirstSection (line 34)
  private bool _pendingTabSpeech (line 35)                          -- set when switching to a side-screen tab (GameTabId == null); side screen content populates async, so speech is deferred one tick
  private bool _pendingActivationSpeech (line 36)

  // Properties
  public override string DisplayName (line 38)                      -- builds "EntityName[, HatName, SkillSubtitle][, TabName]"; returns null when _suppressDisplayName is true
  public override IReadOnlyList<HelpEntry> HelpEntries (line 61)

  // Constructor
  public DetailsScreenHandler(KScreen screen) (line 63)             -- calls BuildTabs(); builds help entries including Ctrl+Tab for section switching

  // NestedMenuHandler abstracts
  protected override int MaxLevel => 2 (line 76)
  protected override int SearchLevel => Level (line 77)
  protected override int GetItemCount(int level, int[] indices) (line 79)
  protected override string GetItemLabel(int level, int[] indices) (line 88)   -- returns section header at level 0; WidgetOps.GetSpeechText at levels 1 and 2
  protected override string GetParentLabel(int level, int[] indices) (line 105) -- returns section header for level 1; parent item text for level 2
  protected override void ActivateLeafItem(int[] indices) (line 114) -- dispatches by widget type: Button/Toggle -> activate + _pendingActivationSpeech; Slider -> SpeakCurrentItem; TextInput -> TextEdit.Begin/Confirm; Dropdown -> CycleRadioGroup forward
  protected override int GetSearchItemCount(int[] indices) (line 150) -- flat count for search: sections at level 0; all items across sections at level 1; all children in current section at level 2
  protected override string GetSearchItemLabel(int flatIndex) (line 168)
  protected override void MapSearchIndex(int flatIndex, int[] outIndices) (line 198) -- converts flat search index back to [sectionIdx, itemIdx, childIdx]

  // Left/Right: slider adjustment at leaf level
  protected override void HandleLeftRight(int direction, int stepLevel) (line 237) -- at Level > 0: SliderWidget -> AdjustSlider; DropdownWidget -> CycleRadioGroup; otherwise base
  private void AdjustSlider(Widget w, int direction, int stepLevel) (line 252)    -- calls sw.Adjust; invokes slider's onMove action so side screens (e.g. CapacityControl) sync; plays slider sound; on change: RebuildSections + speak
  private void CycleRadioGroup(Widget w, int direction) (line 278)  -- reads RadioMember list from widget.Tag; finds active index via FindActiveRadioIndex; clicks next member's Toggle or MultiToggle
  private static void PlaySliderSound(string soundName) (line 295)
  private static int FindActiveRadioIndex(List<SideScreenWalker.RadioMember> members) (line 311) -- checks KToggle.IsToggleActive; for AlarmSideScreen matches by NotificationType tag; falls back to MultiToggle.CurrentState == 1

  // Key interception: text editing
  public override bool HandleKeyDown(KButtonEvent e) (line 341)     -- intercepts Escape during text editing to cancel + RebuildSections; otherwise delegates to base

  // Speech
  public override void SpeakCurrentItem(string parentContext = null) (line 357) -- at level 0 uses base; at level > 0 gets current widget, appends tooltip, prepends parentContext if given
  private Widget GetCurrentWidget() (line 374)
  private Widget GetWidgetAt(int sIdx, int iIdx, int cIdx) (line 378)

  // Lifecycle
  public override void OnActivate() (line 392)                      -- captures last target; RebuildActiveTabs; sets _tabIndex=0; SwitchGameTab; sets _pendingFirstSection; suppresses DisplayName during base.OnActivate call

  // Tick: target change detection
  public override bool Tick() (line 408)                            -- handles: text-edit Return; _pendingActivationSpeech (RebuildSections + speak); _pendingTabSpeech (RebuildSections + ResetNavigation + SpeakFirstSection); target change detection (full rebuild + speak); _pendingFirstSection (speak when sections available); Ctrl+Tab/Ctrl+Shift+Tab for AdvanceSection; delegates to base

  // Tab cycling
  protected override void NavigateTabForward() (line 471)           -- calls AdvanceTab(1)
  protected override void NavigateTabBackward() (line 475)          -- calls AdvanceTab(-1)
  private void AdvanceTab(int direction) (line 479)                 -- cycles within current section group; plays wrap/hover sound; speaks tab name; if side-screen tab sets _pendingTabSpeech; otherwise RebuildSections + SpeakFirstSection
  private void AdvanceSection(int direction) (line 513)             -- cycles across section groups (info tabs vs side-screen tabs); updates _tabIndex to new section's start; plays wrap/hover sound; same deferred/immediate speech pattern as AdvanceTab

  // Section management
  private void RebuildSections() (line 545)                        -- clears _sections; calls active tab's Populate(target, _sections); logs on exception

  // Tab management
  private void RebuildActiveTabs(GameObject target) (line 564)     -- builds _activeTabs by checking game tab toggle visibility (for GameTabId != null) or tab.IsAvailable; builds _sectionStarts by grouping consecutive info/side-screen tabs
  private void SwitchGameTab() (line 607)                          -- calls tab.OnTabSelected(); for non-null GameTabId calls DetailTabHeader.ChangeTab via Traverse

  // Private helpers
  private void ResetNavigation() (line 629)                        -- calls ResetState() (inherited)
  private void SpeakFirstSection() (line 633)                      -- if no sections: speaks NoConfig or No Errands message; if header == tab name, speaks first item directly; otherwise speaks "header, item"

  // Tab configuration
  private static IDetailTab[] BuildTabs() (line 669)               -- returns: StatusTab, PersonalityTab, ChoresTab, PropertiesTab, ConfigSideTab, ErrandsSideTab, MaterialTab, BlueprintTab
```
