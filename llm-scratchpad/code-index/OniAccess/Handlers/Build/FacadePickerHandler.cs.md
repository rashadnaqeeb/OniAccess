# FacadePickerHandler.cs

## File-level notes
Modal facade picker for a building with cosmetic skins. Lists unlocked facades plus the default appearance. Enter selects the facade and pops back to BuildInfoHandler.

---

```
class FacadePickerHandler : BaseMenuHandler (line 11)

  // Fields
  private readonly BuildingDef _def (line 12)
  private List<FacadeEntry> _facades (line 13)

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 15)

  // Properties
  public override IReadOnlyList<HelpEntry> HelpEntries (line 23)
  public override string DisplayName (line 24)    // => "" (no display name spoken on entry)

  public FacadePickerHandler(BuildingDef def) (line 26)

  public override int ItemCount (line 30)

  public override string GetItemLabel(int index) (line 32)

  // Speaks the current facade label directly; ignores parentContext
  public override void SpeakCurrentItem(string parentContext = null) (line 37)

  // Lifecycle: plays open sound, rebuilds list, resets to index 0, positions on currently selected facade, speaks it
  public override void OnActivate() (line 42)

  public override void OnDeactivate() (line 54)

  // Sets facadePanel.SelectedFacade to the chosen entry's Id and pops
  protected override void ActivateCurrentItem() (line 59)

  // Intercepts Escape to pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 69)

  // Builds _facades list: DEFAULT_FACADE entry first, then unlocked facades from _def.AvailableFacades
  // (only adds entries with a non-null permit that IsUnlocked and a valid resource)
  private void RebuildList() (line 79)

  // Sets _currentIndex to match the facade currently selected in FacadeSelectionPanel
  private void PositionOnSelected() (line 107)

  private static void PlayOpenSound() (line 124)
  private static void PlayCloseSound() (line 128)

  // Private struct holding facade Id and display Label
  private struct FacadeEntry (line 132)
    public string Id (line 133)
    public string Label (line 134)
```
