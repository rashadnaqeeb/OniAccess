# MaterialPickerHandler.cs

## File-level notes
Modal material picker for a single recipe ingredient slot. Lists discovered materials with available quantities, sufficient materials first then insufficient. Enter selects the material and pops back to BuildInfoHandler.

---

```
class MaterialPickerHandler : BaseMenuHandler (line 10)

  // Fields
  private readonly BuildingDef _def (line 11)
  private readonly int _selectorIndex (line 12)    // which recipe ingredient slot this picker is for
  private List<MaterialEntry> _materials (line 13)

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 15)

  // Properties
  public override IReadOnlyList<HelpEntry> HelpEntries (line 23)
  public override string DisplayName (line 24)    // => "" (no display name spoken on entry)

  public MaterialPickerHandler(BuildingDef def, int selectorIndex) (line 26)

  public override int ItemCount (line 31)

  public override string GetItemLabel(int index) (line 33)

  // Speaks the current material label directly; ignores parentContext
  public override void SpeakCurrentItem(string parentContext = null) (line 38)

  // Lifecycle: plays open sound, rebuilds list, resets to index 0, positions on currently selected material, speaks it
  public override void OnActivate() (line 43)

  public override void OnDeactivate() (line 56)

  // Calls SelectMaterial with the chosen tag and pops
  protected override void ActivateCurrentItem() (line 61)

  // Intercepts Escape to pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 70)

  // Builds _materials list from valid + discovered materials for the ingredient slot.
  // Splits into sufficient (available >= required amount) and insufficient buckets;
  // sufficient entries appear first in the final list.
  private void RebuildList() (line 80)

  // Sets _currentIndex to match the tag currently selected in the materialSelectionPanel at _selectorIndex
  private void PositionOnSelected() (line 125)

  // Selects material in the game's materialSelectionPanel.
  // Slot 0: ForceSelectPrimaryTag. Slot 1+: reflects into materialSelectors list and calls OnSelectMaterial.
  private void SelectMaterial(Tag tag) (line 143)

  private static void PlayOpenSound() (line 162)
  private static void PlayCloseSound() (line 166)

  // Private struct holding Tag and display Label
  private struct MaterialEntry (line 170)
    public Tag Tag (line 171)
    public string Label (line 172)
```
