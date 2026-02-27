# KeyBindingsHandler.cs

## File-level / class-level comment

Handler for `InputBindingsScreen` (keyboard rebinding UI). Paginated categories (Global, Tool,
Management, etc.) with Prev/Next navigation via Tab/Shift+Tab.

Each page shows binding rows: action name + current key + rebind button.

Special rebind mode: when the user activates a binding row, the game enters `waitingForKeyPress`
mode and scans for the next physical keypress. All mod input is suppressed during this mode to
avoid consuming keys meant for the rebind scanner.

Conflict dialogs are handled by `ConfirmDialogHandler` (auto-activates on stack).

Key design: the rebind click is deferred by one frame via a coroutine so that the `GetKeyDown(Return)`
from ActivateCurrentItem has cleared before the game's rebind scanner runs in `Update()`. Without
this, Enter would immediately be bound to the action.

---

```
public class KeyBindingsHandler : BaseWidgetHandler (line 22)

  // Properties
  public override string DisplayName (line 23)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 25)

  // Fields (Traverse accessors for private fields on InputBindingsScreen)
  private Traverse _parentField (line 30)                          -- "parent" GameObject containing the binding rows
  private Traverse _waitingField (line 31)                         -- "waitingForKeyPress" bool; polled every Tick
  private Traverse _activeScreenField (line 32)                    -- "activeScreen" int index; used to detect category wrap
  private Traverse _screensField (line 33)                         -- "screens" List<string>; fallback for category name
  private Traverse _screenTitleField (line 34)                     -- "screenTitle" LocText; primary source for category name

  // Fields (public button references)
  private KButton _resetButton (line 39)
  private KButton _prevScreenButton (line 40)
  private KButton _nextScreenButton (line 41)

  // Fields (state)
  private bool _wasWaiting (line 46)                               -- tracks previous waitingForKeyPress to detect enter/exit transitions
  private string _rebindActionName (line 52)                       -- stored when entering rebind mode; used in the "press a key for X" announcement
  private UnityEngine.Coroutine _rebindCoroutine (line 54)

  // Constructor
  public KeyBindingsHandler(KScreen screen) (line 56)

  // Lifecycle
  public override void OnActivate() (line 60)                      -- caches all Traverse accessors and button references; resets _wasWaiting/_rebindActionName
  public override void OnDeactivate() (line 81)                    -- stops any in-flight rebind coroutine

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 93)   -- walks active children of "parent" GameObject; each row contributes a ButtonWidget with SpeechFunc reading live keyText LocText; appends reset button at end; always returns true

  // Tick: rebind mode + key detection
  public override bool Tick() (line 160)                           -- polls _waitingField each frame; on enter: announces "press a key for X"; on exit: re-discovers + speaks current widget; while waiting: returns false to suppress all mod input; otherwise delegates to base

  // HandleKeyDown: suppress during rebind
  public override bool HandleKeyDown(KButtonEvent e) (line 195)    -- returns false (not consuming) while waitingForKeyPress; otherwise delegates to base

  // Widget activation
  protected override void ActivateCurrentItem() (line 206)         -- reset button: click + re-discover + announce; binding row: defer click one frame via DeferredRebindClick coroutine to let GetKeyDown(Return) clear
  private IEnumerator DeferredRebindClick(KButton button) (line 233) -- yields one frame then calls ClickButton(button)

  // Tab navigation: category switching
  protected override void NavigateTabForward() (line 243)          -- clicks nextScreenButton; reads activeScreen index before/after to detect wrap; calls OnCategoryChanged
  protected override void NavigateTabBackward() (line 253)         -- clicks prevScreenButton; reads activeScreen index to detect wrap; calls OnCategoryChanged
  private void OnCategoryChanged(bool wrapped) (line 266)          -- plays wrap sound if wrapped; re-discovers widgets; speaks category name + first widget (or just category if no widgets)
  private string GetCurrentCategoryName() (line 284)               -- reads screenTitle LocText; falls back to screens list at activeScreen index; falls back to handler display name
```
