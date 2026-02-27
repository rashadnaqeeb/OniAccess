# KleiItemDropHandler.cs

## File-level / class-level comment

Handler for `KleiItemDropScreen`: cosmetic item reveal triggered from the Supply Closet.

This is a coroutine-driven sequential presentation. An animated pod reveals items one at a time,
with buttons fading in/out via `CanvasGroup` alpha at each stage. At any moment there are at most
2-3 buttons visible.

`Tick()` polls for state changes (item reveal, error, button availability) since coroutines
animate elements asynchronously.

Lifecycle note: Like `LockerMenuScreen`, `OnActivate()` calls `Show(false)` during prefab init,
so a Harmony patch on `KleiItemDropScreen.Show` pushes/pops this handler.

---

```
public class KleiItemDropHandler : BaseWidgetHandler (line 20)

  // Properties
  public override string DisplayName (line 21)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 23)

  // Fields (state tracking)
  private bool _announcedItemInfo (line 26)                         -- prevents re-announcing the same item reveal
  private bool _announcedError (line 27)                            -- prevents re-announcing the same error
  private string _lastAcceptButtonText (line 28)                    -- tracks accept button text changes to detect new button availability

  // Fields (cached component references resolved in OnActivate)
  private UnityEngine.RectTransform _acceptButtonRect (line 33)    -- parent rect with CanvasGroup; alpha > 0.5 means button is visible
  private KButton _acceptButton (line 34)
  private KButton _acknowledgeButton (line 35)
  private UnityEngine.RectTransform _itemTextContainer (line 36)   -- parent of acknowledgeButton; alpha used for visibility check
  private LocText _itemNameLabel (line 37)
  private LocText _itemRarityLabel (line 38)
  private LocText _itemCategoryLabel (line 39)
  private LocText _itemDescriptionLabel (line 40)
  private LocText _errorMessage (line 41)
  private KButton _closeButton (line 42)

  // Constructor
  public KleiItemDropHandler(KScreen screen) (line 44)

  // Lifecycle
  public override void OnActivate() (line 48)                      -- resets state flags; resolves all component references via Traverse; calls base

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 68)   -- adds acceptButton if acceptButtonRect is active and CanvasGroup alpha > 0.5; adds acknowledgeButton if active and itemTextContainer alpha > 0.5; adds closeButton if SetActive (no alpha check); always returns true

  // Tick
  public override bool Tick() (line 128)                           -- re-discovers widgets every tick (buttons appear/disappear via coroutines); clamps cursor index; detects item reveal (itemNameLabel goes non-empty -> announce rarity/category/name/description); detects error (errorMessage activated); detects accept button text change (announces new text if button is currently first widget); delegates to base

  // Widget validity override
  protected override bool IsWidgetValid(Widget widget) (line 209)  -- extends base check with CanvasGroup alpha > 0.5 on the widget itself and on its parent; faded-out buttons are not valid navigation targets
```
