# BuildInfoHandler.cs

## File-level notes
Modal info panel for a building being placed. Shows combined description, attributes, operation requirements, operation effects, room type, facade selector, and material selectors. Enter on a material item opens MaterialPickerHandler; Enter on the facade item opens FacadePickerHandler.

---

```
class BuildInfoHandler : BaseMenuHandler (line 14)

  // Fields
  private readonly BuildingDef _def (line 15)
  private List<InfoItem> _items (line 16)

  // Room constraint tags that are filtered out of the room-type display
  private static readonly HashSet<Tag> _hiddenRoomTags (line 18)

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 31)

  // Properties
  public override IReadOnlyList<HelpEntry> HelpEntries (line 38)
  public override string DisplayName (line 39)

  public BuildInfoHandler(BuildingDef def) (line 41)

  public override int ItemCount (line 45)

  public override string GetItemLabel(int index) (line 47)

  // Speaks the item label directly; ignores parentContext
  public override void SpeakCurrentItem(string parentContext = null) (line 52)

  // Lifecycle: plays open sound, rebuilds items list, positions at index 0, speaks first item
  public override void OnActivate() (line 57)

  public override void OnDeactivate() (line 69)

  // Enter on a material item (SelectorIndex >= 0) pushes MaterialPickerHandler;
  // Enter on facade item (SelectorIndex == -2) pushes FacadePickerHandler.
  // Non-activatable items (SelectorIndex == -1) do nothing.
  protected override void ActivateCurrentItem() (line 74)

  // Intercepts Escape to pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 86)

  // Rebuilds _items by calling all Add* helpers in order
  private void RebuildItems() (line 96)

  // Appends combined Effect + Desc as a single "Description: ..." item
  private void AddDescriptionItem() (line 108)

  // Appends summed building attribute modifiers as "Attributes: ..." item; returns the attribute map
  private Dictionary<Klei.AI.Attribute, float> AddBaseAttributeItem() (line 127)

  // Appends scaled material attribute effects as "Material effects: ..." item
  // Reads currently selected element from the live materialSelectionPanel
  private void AddMaterialEffectsItem(Dictionary<Klei.AI.Attribute, float> baseAttrs) (line 157)

  // Appends "Facade: <name>" item if the def has available facades; SelectorIndex == -2
  private void AddFacadeItem() (line 204)

  // Appends requirement and effect descriptor groups via AddMergedDescriptorItem
  private void AddDescriptorItems() (line 224)

  // Appends a single "Prefix: item1, item2, ..." item from a descriptor list
  private void AddMergedDescriptorItem(string prefix, List<Descriptor> descriptors) (line 238)

  // Appends "Category: roomtype1, ..." item from the building's KPrefabID tags (filtered by _hiddenRoomTags)
  private void AddRoomTypeItem() (line 253)

  // Appends one material slot item per recipe ingredient; SelectorIndex == ingredient index
  private void AddMaterialItems() (line 274)

  // Builds the label for one material slot: "Category (Selected, quantity)" or with "insufficient" warning
  private static string BuildMaterialLabel(
      Recipe.Ingredient ingredient, MaterialSelectionPanel panel, int index) (line 287)

  // Splits a tag like "Metal&RefinedMetal" on '&' and joins parts with localized "or" separator
  private static string GetIngredientCategoryName(Tag tag) (line 317)

  private static void PlayOpenSound() (line 325)
  private static void PlayCloseSound() (line 329)

  // Private struct holding label text and selector index (-1 = not activatable, -2 = facade, >= 0 = material slot)
  private struct InfoItem (line 333)
    public string Label (line 334)
    public int SelectorIndex (line 335)
    public InfoItem(string label, int selectorIndex) (line 337)
```
