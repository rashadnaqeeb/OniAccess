# ColonySummaryHandler.cs

## File-level / class-level comment

Handler for `RetiredColonyInfoScreen` (colony summary, accessible from main menu and pause menu).

Two views:
1. **Explorer view** (`explorerRoot`): grid of colony buttons for past/retired colonies
2. **Colony detail view** (`colonyDataRoot`): stats, achievements, duplicants for a selected colony

Navigation:
- Explorer: Up/Down across colonies, Enter opens colony detail
- Detail: Up/Down within current section, Tab switches sections
- Escape from detail returns to explorer; Escape from explorer closes screen

Detail view sections (Tab navigation): 0=Duplicants, 1=Buildings, 2=Statistics, 3=Achievements
Explorer sections: 0=Colonies, 1=Achievements

Duplicant/building/stat widgets use live reading (SpeechFunc) at speech time because the game
populates LocTexts via `SetText()` which updates TMP's internal buffer but not `m_text`.
`GetParsedText()` needs a frame to catch up.

---

```
public class ColonySummaryHandler : BaseWidgetHandler (line 27)

  // Constants
  private const int ExplorerSectionMain = 0 (line 28)
  private const int ExplorerSectionAchievements = 1 (line 29)
  private const int ExplorerSectionCount = 2 (line 30)
  private const int DetailSectionDuplicants = 0 (line 32)
  private const int DetailSectionBuildings = 1 (line 33)
  private const int DetailSectionStats = 2 (line 34)
  private const int DetailSectionAchievements = 3 (line 35)
  private const int DetailSectionCount = 4 (line 36)

  // Fields
  private bool _inColonyDetail (line 38)
  private bool _isInGameContext (line 39)                           -- true when opened from in-game pause menu (explorerRoot is inactive); skips "View other colonies" back navigation
  private int _currentSection (line 40)
  private KButton _viewOtherColoniesButton (line 41)

  // Properties
  protected override int MaxDiscoveryRetries => 5 (line 43)        -- detail view is coroutine-populated, needs extra retry frames
  private int SectionCount (line 45)                               -- returns DetailSectionCount or ExplorerSectionCount depending on _inColonyDetail
  public override string DisplayName (line 47)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 49)

  // Constructor
  public ColonySummaryHandler(KScreen screen) (line 51)

  // Lifecycle
  public override void OnActivate() (line 55)                      -- detects in-game context (explorerRoot inactive -> skip to detail view); calls CacheButtons; calls base.OnActivate
  private void CacheButtons() (line 77)                            -- resolves viewOtherColoniesButton via Traverse

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 86)   -- dispatches to the right Discover* method based on _inColonyDetail and _currentSection; returns true if any widgets found
  private void DiscoverExplorerViewWidgets(KScreen screen) (line 117)  -- walks explorerGrid children; reads colony entry labels via ReadColonyEntryLabel; adds detail/close buttons
  private static string ReadColonyEntryLabel(UnityEngine.Transform entry) (line 149) -- reads ColonyNameLabel/CycleCountLabel/DateLabel from HierarchyReferences via GetParsedText() (avoids SetText/TMP buffer quirk); format: "Name, Cycle Count: X, date"
  private void DiscoverDuplicantWidgets(KScreen screen) (line 182) -- reads activeColonyWidgets["duplicants"] content children; adds LabelWidgets with SpeechFunc=ReadDuplicantEntry
  private static string ReadDuplicantEntry(UnityEngine.Transform entry) (line 215) -- reads NameLabel/AgeLabel/SkillLabel from HierarchyReferences via GetParsedText()
  private void DiscoverBuildingWidgets(KScreen screen) (line 248)  -- reads activeColonyWidgets["buildings"] content children; adds LabelWidgets with SpeechFunc=ReadBuildingEntry
  private static string ReadBuildingEntry(UnityEngine.Transform entry) (line 280) -- reads NameLabel/CountLabel from HierarchyReferences via GetParsedText()
  private void DiscoverStatWidgets(KScreen screen) (line 306)      -- iterates activeColonyWidgets (excluding timelapse/duplicants/buildings); adds LabelWidgets with SpeechFunc=ReadStatEntry
  private static string ReadStatEntry(UnityEngine.GameObject go, string fallbackName) (line 334) -- reads GraphBase.graphName + last data point from LineLayer.lines[0].points; appends unit from axis_y.name; shows "None" when no data points
  private void DiscoverAchievementWidgets(KScreen screen) (line 372) -- walks achievementsContainer children; inserts "Victory conditions" header before victory condition entries; adds LabelWidgets with SpeechFunc=ReadAchievementText
  private void AddDetailButtons(KScreen screen) (line 424)         -- appends viewOtherColoniesButton and closeScreenButton to _widgets; used by all detail sections and explorer view

  // View transitions
  protected override void ActivateCurrentItem() (line 438)         -- colony buttons in explorer -> click + enter detail + _pendingRediscovery; "View other colonies" -> ReturnToExplorerView(); in-game achievements with AchievementWidget -> SpeakAchievementProgress; otherwise base
  private void SpeakDetailHeader() (line 480)                      -- speaks colony name + cycle count + section name when entering detail; reads from colonyName.text/cycleCount.text (not GetParsedText, since set by direct assignment not SetText)
  public override bool HandleKeyDown(KButtonEvent e) (line 502)    -- intercepts Escape in colony detail to call ReturnToExplorerView instead of closing screen (only in non-game context)
  private void ReturnToExplorerView() (line 518)                   -- clicks viewOtherColoniesButton; resets _inColonyDetail/_currentSection; re-discovers; speaks DisplayName + first widget

  // Tab navigation
  protected override void NavigateTabForward() (line 544)          -- increments _currentSection mod SectionCount; plays wrap sound at 0; calls RediscoverForCurrentSection
  protected override void NavigateTabBackward() (line 551)         -- decrements _currentSection; plays wrap sound when wrapping back to last; calls RediscoverForCurrentSection
  private void RediscoverForCurrentSection() (line 559)            -- re-discovers widgets for new section; speaks section name + first widget
  private string GetSectionName(int section) (line 569)            -- returns localized section name; falls back to DisplayName for explorer main section

  // Widget speech
  private string ReadAchievementText(UnityEngine.Transform entry) (line 595) -- reads nameLabel/descriptionLabel from HierarchyReferences via GetParsedText(); combines if different
  private void SpeakAchievementProgress(UnityEngine.GameObject achGO) (line 629) -- temporarily activates progressParent + ForceUpdateCanvases so TMP renders SetText()-populated text; reads Desc LocTexts from HierarchyReferences children; speaks joined entries; restores active state
```
