# BaseWidgetHandler.cs

## File-level / class-level comment

Screen-bound widget handler extending BaseMenuHandler with widget discovery, interaction, speech,
and lifecycle management. Bridges BaseMenuHandler's abstract list navigation and concrete
Widget-based screen handlers. Implements ItemCount, GetItemLabel, SpeakCurrentItem, IsItemValid
by delegating to the `_widgets` list.

Concrete screen handlers extend this and implement only:
- `DiscoverWidgets` (populate `_widgets`)
- `DisplayName` (screen title for speech)
- `HelpEntries` (composing from MenuHelpEntries + ListNavHelpEntries + screen-specific)

Locked decisions:
- Enter activates (ClickButton for KButton, KToggle.Click)
- Left/Right adjust sliders and cycle dropdowns
- Shift/Ctrl/Ctrl+Shift+Left/Right for progressively larger step adjustment
- Tab/Shift+Tab for tabbed screens (virtual stubs)
- Widget readout: label and value only, no type announcement
- TextInput: Enter to begin editing, Enter to confirm, Escape to cancel

---

```
public abstract class BaseWidgetHandler : BaseMenuHandler (line 30)

  // Fields
  protected readonly List<Widget> _widgets (line 31)
  private TextEditHelper _textEdit (line 32)
  protected TextEditHelper TextEdit (line 33)                         -- lazy-initialized property
  protected bool IsTextEditing (line 34)
  protected bool _pendingRediscovery (line 43)                        -- when true, Tick() retries DiscoverWidgets; set when OnActivate finds zero widgets (screen UI not ready yet)
  private bool _pendingSilentRefresh (line 44)
  private int _retryCount (line 45)

  // Properties
  protected virtual int MaxDiscoveryRetries => 1 (line 51)           -- override in subclasses that need more retry frames (e.g. coroutine-driven screens)

  // Constructor
  protected BaseWidgetHandler(KScreen screen) (line 53)

  // BaseMenuHandler abstract implementations
  public override int ItemCount => _widgets.Count (line 59)
  public override string GetItemLabel(int index) (line 61)
  public override void SpeakCurrentItem(string parentContext = null) (line 66)  -- delegates to SpeakCurrentWidget()
  protected override bool IsItemValid(int index) (line 70)

  // Abstract: widget discovery
  public abstract bool DiscoverWidgets(KScreen screen) (line 87)     -- return true = ready; false = not ready yet (base retries next frame)

  // Lifecycle
  public override void OnActivate() (line 93)                        -- calls DiscoverWidgets; sets _pendingRediscovery or _pendingSilentRefresh depending on result
  public override void OnDeactivate() (line 106)                     -- clears _widgets

  // Tick
  public override bool Tick() (line 115)                             -- handles text-edit Return, deferred silent refresh (re-discover + announce first widget), deferred rediscovery with retry limit, invalid-widget detection, then delegates to base

  // HandleKeyDown
  public override bool HandleKeyDown(KButtonEvent e) (line 178)      -- intercepts Escape during text editing to cancel; otherwise delegates to base

  // Widget interaction
  protected override void ActivateCurrentItem() (line 201)           -- dispatches by Widget subclass: Button -> bw.Activate(); Toggle -> tw.Activate() + speak; TextInput -> TextEdit.Begin/Confirm
  protected override void AdjustCurrentItem(int direction, int stepLevel) (line 234)  -- dispatches: Slider -> sw.Adjust + play sound + speak; Dropdown -> CycleDropdown
  protected virtual void CycleDropdown(Widget widget, int direction) (line 258)  -- no-op default; override in subclasses

  // Widget validity
  protected virtual bool IsWidgetValid(Widget widget) (line 264)
  protected static string GetButtonLabel(KButton button, string fallback = null) (line 266)

  // Widget speech
  protected virtual string GetWidgetSpeechText(Widget widget) (line 273)
  protected void SpeakCurrentWidget() (line 279)                     -- SpeakInterrupt with tooltip appended
  protected void QueueCurrentWidget() (line 293)                     -- SpeakQueued; used after text-edit confirm/cancel so it follows a preceding SpeakInterrupt

  // Tooltip text
  protected virtual string GetTooltipText(Widget widget) (line 307)
  protected static string ReadAllTooltipText(ToolTip tooltip) (line 309)

  // Utility methods
  protected static void ClickButton(KButton button) (line 315)
  protected static void ClickMultiToggle(MultiToggle toggle) (line 316)
  private void PlaySliderSound(string soundName) (line 318)
  protected virtual string FormatSliderValue(KSlider slider) (line 326)
```
