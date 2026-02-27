# LockerMenuHandler.cs

## File-level / class-level comment

Handler for `LockerMenuScreen`: the Supply Closet hub accessed from the main menu.

Discovers 4 MultiToggle buttons (Inventory, Duplicants, Wardrobe, Claim Blueprints) plus a
close KButton.

Lifecycle note: `LockerMenuScreen.OnActivate()` immediately calls `Show(false)` during prefab
init, so the normal KScreen.Activate/Deactivate hooks don't work. A Harmony patch on
`LockerMenuScreen.Show` pushes/pops this handler instead.

When a sub-screen is open (`LockerNavigator` is active), all input is suppressed so keys are not
intercepted from the sub-screen handler.

---

```
public class LockerMenuHandler : BaseWidgetHandler (line 17)

  // Properties
  public override string DisplayName (line 18)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 20)

  // Fields
  private static readonly string[] MultiToggleFields (line 22)     -- ["buttonInventory", "buttonDuplicants", "buttonOutfitBroswer" (sic: game typo), "buttonClaimItems"]
  private static readonly string[] DescriptionStrings (line 27)    -- parallel array to MultiToggleFields; index 3 (Claim Items) is null (set dynamically based on CurrentState)

  // Constructor
  public LockerMenuHandler(KScreen screen) (line 34)

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 38)   -- iterates MultiToggleFields; reads LocText label (falls back to GetFallbackLabel); Claim Items: description and SpeechFunc are dynamic (CurrentState 0 = items available, else "no items"); adds close button via WidgetDiscoveryUtil; always returns true
  private static string GetFallbackLabel(int index) (line 88)      -- returns game STRINGS label for each button index

  // Tooltip
  protected override string GetTooltipText(Widget widget) (line 102) -- reads description string from widget.Tag (set in DiscoverWidgets)

  // Lifecycle
  public override void OnActivate() (line 108)                     -- calls base; then checks noConnectionIcon: if active, queues OFFLINE announcement

  // Input suppression when sub-screen is open
  public override bool Tick() (line 128)                           -- returns false (suppresses all input) if IsSubScreenOpen()
  public override bool HandleKeyDown(KButtonEvent e) (line 133)    -- returns false if IsSubScreenOpen()
  private static bool IsSubScreenOpen() (line 138)                 -- returns true if LockerNavigator.Instance is not null and isActiveAndEnabled
```
