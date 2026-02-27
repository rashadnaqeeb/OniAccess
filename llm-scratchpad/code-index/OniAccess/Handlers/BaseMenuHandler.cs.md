# BaseMenuHandler.cs

## File-level comment
Reusable 1D list navigation base extending `BaseScreenHandler`. Provides
arrow-key navigation with wrap-around, Home/End, Enter activation, Left/Right
adjustment, Tab stubs, and A-Z type-ahead search.

Accepts a null KScreen because it serves both screen-bound widget handlers and
lightweight overlay browsers like `TooltipBrowserHandler` and `HelpHandler`
that have no KScreen.

Subclasses implement `ItemCount`, `GetItemLabel`, and `SpeakCurrentItem`.
Override `ActivateCurrentItem`, `AdjustCurrentItem`, and `NavigateTab*` for
interaction behavior.

---

```
abstract class BaseMenuHandler : BaseScreenHandler, ISearchable (line 19)

  // Fields
  protected int _currentIndex                             (line 20)
  protected readonly TypeAheadSearch _search              (line 21)
  private int _searchSuppressFrame                        (line 22)

  // Constructor
  protected BaseMenuHandler(KScreen screen = null)        (line 24)

  // Properties
  override bool CapturesAllInput { get; }                 (line 29)
  // Always true — menus are modal.

  // Abstract: list description
  abstract int ItemCount { get; }                         (line 38)
  abstract string GetItemLabel(int index)                 (line 43)
  abstract void SpeakCurrentItem(string parentContext = null) (line 48)

  // Virtual hooks
  protected virtual bool IsItemValid(int index)           (line 58)
  // Returns true by default. Widget handlers override to check component state.

  protected virtual void ActivateCurrentItem()            (line 63)
  // Enter key handler. No-op default.

  protected virtual void AdjustCurrentItem(int direction, int stepLevel) (line 68)
  // Left/Right key handler. No-op default.

  protected virtual void NavigateTabForward()             (line 73)
  protected virtual void NavigateTabBackward()            (line 78)
  protected virtual void JumpNextGroup()                  (line 83)
  protected virtual void JumpPrevGroup()                  (line 88)

  // Composable help entry lists
  protected static readonly List<HelpEntry> MenuHelpEntries      (line 97)
  // Contains: "A-Z" -> type-search help entry.

  protected static readonly List<HelpEntry> ListNavHelpEntries   (line 104)
  // Contains: Up/Down, Home/End, Enter, Left/Right, Shift+Left/Right,
  // Ctrl+Left/Right, Ctrl+Shift+Left/Right help entries.

  protected IReadOnlyList<HelpEntry> BuildHelpEntries(params HelpEntry[] extra) (line 117)
  // Combines MenuHelpEntries + ListNavHelpEntries + any extra entries.

  // Navigation methods
  protected virtual void NavigateNext()                   (line 132)
  // Move to next valid item with wrap-around; plays wrap or hover sound.

  protected virtual void NavigatePrev()                   (line 152)
  // Move to previous valid item with wrap-around; plays wrap or hover sound.

  protected virtual void NavigateFirst()                  (line 172)
  // Jump to first valid item.

  protected virtual void NavigateLast()                   (line 187)
  // Jump to last valid item.

  // Sounds
  protected void PlayWrapSound()                          (line 203)
  protected void PlayHoverSound()                         (line 211)

  // Search
  private static readonly UnityEngine.KeyCode[] _searchNavKeys   (line 223)
  // UpArrow, DownArrow, Home, End, Backspace — captured by search when active.

  protected bool TryRouteToSearch(bool ctrlHeld, bool altHeld)   (line 233)
  // Routes A-Z and navigation keys through _search.HandleKey.
  // Returns true if search consumed the key.
  // Skips on the frame search was suppressed (prevents opening hotkey from
  // triggering search, e.g., pressing R to open Research).

  // Lifecycle
  protected void SuppressSearchThisFrame()                (line 264)
  // Records current frameCount to suppress type-ahead for this frame.

  override void OnActivate()                              (line 271)
  // Calls base (speaks DisplayName), resets _currentIndex to 0, clears search,
  // and suppresses search for this frame.

  override void OnDeactivate()                            (line 281)
  // Calls base, resets _currentIndex to 0, clears search.

  // Tick: key detection
  override bool Tick()                                    (line 295)
  // Handles: type-ahead routing, Down/Up (with Ctrl group-jump), Home, End,
  // Enter (activate), Tab (tab nav), Left/Right (adjust/drill-down).

  protected virtual void HandleLeftRight(int direction, int stepLevel) (line 348)
  // Default delegates to AdjustCurrentItem.
  // Overridden by NestedMenuHandler for drill-down/go-back behavior.

  override bool HandleKeyDown(KButtonEvent e)             (line 356)
  // If search is active and Escape is pressed: clears search and speaks
  // "search cleared" instead of letting the game close the screen.

  // ISearchable implementation
  int SearchItemCount { get; }                            (line 370)
  // Bridges to ItemCount.

  string GetSearchLabel(int index)                        (line 372)
  // Returns filtered label (TextFilter.FilterForSpeech) for the item, or null
  // if out of range.

  void SearchMoveTo(int index)                            (line 377)
  // Sets _currentIndex and calls SpeakCurrentItem. Called by TypeAheadSearch
  // when a result is selected.
```
