# Phase 3: Menu Navigation - Research

**Researched:** 2026-02-11
**Domain:** ONI KScreen/KScreenManager lifecycle, Unity UI widget traversal, keyboard navigation layer, screen reader speech patterns
**Confidence:** HIGH

## Summary

ONI's menu system is built on a `KScreen` class hierarchy with lifecycle managed by `KScreenManager`. Screens call `Activate()` to push onto a screen stack and `Deactivate()` to pop off. The existing Phase 2 Harmony patches on `KScreen.Activate` (postfix) and `KScreen.Deactivate` (prefix) already fire `ContextDetector.OnScreenActivated` and `ContextDetector.OnScreenDeactivating` -- these are the exact hooks Phase 3 needs to detect when screens open and close, push/pop the appropriate accessibility handler onto the `HandlerStack`.

The screens in scope form a clear inheritance hierarchy: `KScreen -> KModalScreen -> NewGameFlowScreen` (for game flow screens like `ColonyDestinationSelectScreen`, `WorldGenScreen`, `MinionSelectScreen`), and `KScreen -> KButtonMenu -> KModalButtonMenu` (for menu screens like `OptionsMenuScreen`, `PauseScreen`). Each screen type has predictable widget patterns: `KButtonMenu` derivatives have a `buttons` array of `ButtonInfo` objects with `.text` labels; `KModalScreen` derivatives consume all input via `e.Consumed = true` at the end of `OnKeyDown`; settings panels use `CustomGameSettingListWidget` (cycle settings), `CustomGameSettingToggleWidget`, and `KSlider`-based volume controls.

The mod's existing input architecture (ModInputRouter at priority 50 > KScreenManager at 10) means our handler sees every key event before screens do. Combined with the `HandleUnboundKey` path for arrow keys (already polled by `KeyPoller`), the navigation system has all the input hooks it needs. The primary challenge is building a widget discovery system that can enumerate interactive children of any `KScreen` and track focus across them, plus porting the `TypeAheadSearch` class from the repo root into the `OniAccess` namespace.

**Primary recommendation:** Build an abstract `BaseMenuHandler : IAccessHandler` that provides arrow navigation with wrap, Home/End, Enter activation, focus tracking, `ISearchable` support, and speech queuing. Each concrete screen handler inherits from this and implements `DiscoverWidgets()` to enumerate that screen's interactive elements. The `ContextDetector` maps screen types to handler constructors, pushing handlers on activate and popping on deactivate.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Navigation Model
- Arrow keys are the primary navigation (Up/Down between items, Left/Right for value adjustment)
- Wrap-around at list edges (after last item, Down goes to first) with a wrap sound (investigate game's built-in UI sounds for an appropriate click)
- Home/End for jumping to top/bottom of lists
- Nested screens are NOT manually managed -- hook into ONI's existing KScreen lifecycle (Harmony patches on OnActivate/OnDeactivate). Enter triggers the game's own button clicks, Escape triggers the game's own close behavior. Our handlers react to screen state changes, not drive them
- Tabs within a single screen use Tab/Shift+Tab. Arrow keys navigate within the active tab's content
- Focus lands on first interactive widget when a screen activates. Widget announcement is SpeakQueued behind the screen title SpeakInterrupt, so both are heard in order (same pattern as HelpHandler)
- No section jumping (Ctrl+Up/Down). Screens aren't long enough to need it
- Type-ahead search using the existing TypeAheadSearch class (ISearchable interface). A-Z keys start/extend search, Escape clears, same-letter cycling. Each screen handler implements ISearchable
- A base menu handler class is needed to avoid boilerplate across the many screens. Common patterns: arrow nav with wrap, Home/End, Enter to activate, Escape handled by game, focus tracking, ISearchable support, speech queuing on entry

#### Speech Feedback Style
- Widget readout: label and value only, no type announcement. "Master Volume, 80 percent" not "Master Volume, slider, 80 percent"
- Screen entry: screen name only via SpeakInterrupt, first widget queued behind it
- No position info ever ("3 of 7"). Not even on demand
- Strip all Unity Rich Text markup before speaking. Consistent with Phase 1's text filtering pipeline
- Shift+I reads tooltip/hover info -- specifically for content that sighted players only see on mouse hover. All visually-present info is spoken during normal navigation without extra key presses. This applies to the ENTIRE phase
- Use ONI's labels as-is. No reordering or rephrasing of game strings
- No movement earcons for Up/Down navigation. Button clicks come naturally from simulating game button clicks. Investigate game's built-in sounds for a wrap-around earcon
- Value changes on sliders/dropdowns use SpeakInterrupt (cuts off previous speech for rapid adjustment)
- Filter out mouse-only UI controls (merge, hide, resize, drag handles) that are irrelevant for keyboard navigation -- researcher needs to identify these across ONI's screens
- Action button confirmation: deferred until concrete examples are encountered during research

#### Widget Interaction
- Sliders: Left/Right for minor adjustment (1 step), Shift+Left/Right for increments of 10. Value spoken on each change via SpeakInterrupt
- Dropdowns: Left/Right cycles options in-place. No separate open/browse step
- Toggles/Checkboxes: Enter toggles state. New state spoken: "Fullscreen, on"
- Confirmation dialogs: treated as vertical list. Focus starts on text element (dialog message). Down navigates to OK, Cancel, etc. buttons. Wrap-around back to text. Enter activates focused button
- Text input fields: Enter to activate (clears field), type new content, Enter to confirm, Escape to cancel (restores original value). Must cache pre-edit value for Escape rollback

#### Colony Setup Flow
- Mode Select (Survival/No Sweat): name and short description read together since description is visually present
- Asteroid selection screen (ColonyDestinationSelectScreen): tabbed sections. Tab/Shift+Tab between panels
- Cluster list entries: name, difficulty, AND world traits all read on navigation
- Game Settings panel: flat list. Each setting on one line: "Disease, Default". Left/Right cycles difficulty levels
- Duplicant selection: Tab/Shift+Tab switches between the 3 dupe slots. Up/Down reads items within each slot one at a time
- Attribute readout: one per arrow press: "Athletics 3", "Cooking 0". Shift+I for description
- Trait readout: full info upfront: name, effect, and description all spoken together
- Interest filter dropdown and Reroll button at the bottom of each dupe's list. Left/Right on filter to cycle. Enter on Reroll
- After rerolling: speak new dupe name, interests, and traits. User navigates to read full attributes
- Save/Load screen: read colony name, cycle number, duplicant count, and date per entry. File size omitted
- World generation: periodic progress updates. "Generating world, 25 percent... 50 percent... Done"
- No extra Embark confirmation
- Printing Pod selection (recurring every 3 cycles): same approach as initial selection, but no rerolling

### Claude's Discretion
- Exact step size for slider adjustments (should be based on slider.wholeNumbers and range)
- Which game sounds to reuse for wrap-around earcon
- How to handle the WorldGenScreen progress polling interval
- Exact base class design for menu handlers (interface vs abstract class, which methods to make virtual)
- How to structure the KScreen handler registry (screen type -> handler mapping)

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

## Standard Stack

### Core
| Library/System | Version | Purpose | Why Standard |
|---|---|---|---|
| ONI's KScreen lifecycle | Game built-in | Screen activate/deactivate events via existing Harmony patches | Already hooked in Phase 2 -- `KScreen_Activate_Patch` and `KScreen_Deactivate_Patch` fire `ContextDetector` |
| ONI's KScreenManager | Game built-in | Screen stack tracking, widget parent hierarchy | `KScreenManager.Instance.screenStack` provides the current screen stack for `DetectAndActivate` |
| Unity UI Component hierarchy | Unity 2020.3 | `GetComponentsInChildren<T>()` to discover interactive widgets | Standard Unity approach for traversing UI trees |
| OniAccess HandlerStack | Phase 2 | Push/Pop/Replace handlers on screen open/close | Already built and tested. ContextDetector already has placeholder Push/Pop code |
| OniAccess KeyPoller | Phase 2 | Polls arrow keys, F12, Home/End, Tab, Shift+Tab, A-Z keys | Already polls Up/Down/Left/Right arrows. Must be extended for Home, End, Tab, Shift+Tab, A-Z, Shift+I |
| OniAccess SpeechPipeline | Phase 1 | SpeakInterrupt/SpeakQueued for all speech output | Already built with TextFilter integration |
| TypeAheadSearch | Repo root | Type-ahead search with word-start matching, same-letter cycling | Proven implementation from another project, decision to port it into OniAccess namespace |

### Supporting
| Library/System | Version | Purpose | When to Use |
|---|---|---|---|
| HarmonyLib | Game-bundled | Any additional patches needed (unlikely -- Phase 2 patches cover it) | Only if new hooks beyond KScreen.Activate/Deactivate are needed |
| ONI's ToolTip | Game built-in | Read tooltip text for Shift+I | `ToolTip.GetMultiString()` or `toolTip` property on any widget |
| ONI's KFMOD | Game built-in | Play wrap-around earcon sound | `KFMOD.PlayUISound(GlobalAssets.GetSound("soundname"))` |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|---|---|---|
| Abstract base class `BaseMenuHandler` | Interface with extension methods | Abstract class is better here: provides default implementations for navigation, speech, focus tracking. Interface + extension methods can't hold state (current index, widget list) |
| Screen type dictionary for handler mapping | Switch statement in ContextDetector | Dictionary is more maintainable as screens grow across phases. New screens just register a mapping |
| Widget discovery via `GetComponentsInChildren` | Manual per-screen widget lists | Auto-discovery reduces per-screen boilerplate but needs filtering for mouse-only controls. Hybrid approach: auto-discover then filter |

## Architecture Patterns

### Recommended Project Structure
```
OniAccess/
  Input/
    IAccessHandler.cs          # (existing) Handler interface
    HandlerStack.cs            # (existing) Handler stack
    ModInputRouter.cs          # (existing) Routes KButtonEvents
    KeyPoller.cs               # (existing, extended) Polls unbound keys
    ContextDetector.cs         # (existing, fleshed out) Screen -> handler mapping
    HelpHandler.cs             # (existing) F12 help
    WorldHandler.cs            # (existing) World view
    BaseMenuHandler.cs         # NEW: Abstract base for all menu handlers
    ISearchable.cs             # NEW: Ported from TypeAheadSearch.cs
    TypeAheadSearch.cs         # NEW: Ported from repo root
  Input/Handlers/
    MainMenuHandler.cs         # NEW: Main menu navigation
    PauseMenuHandler.cs        # NEW: Pause menu navigation
    OptionsMenuHandler.cs      # NEW: Options menu and sub-screens
    ConfirmDialogHandler.cs    # NEW: Confirmation dialogs
    ColonySetupHandler.cs      # NEW: Colony destination / game mode screens
    DuplicantSelectHandler.cs  # NEW: MinionSelectScreen / Printing Pod
    SaveLoadHandler.cs         # NEW: Load screen navigation
    WorldGenHandler.cs         # NEW: World generation progress
  Speech/
    SpeechPipeline.cs          # (existing) Speech dispatch
    TextFilter.cs              # (existing) Rich text stripping
  Patches/
    InputArchPatches.cs        # (existing) KScreen activate/deactivate hooks
```

### Pattern 1: BaseMenuHandler Abstract Class
**What:** Abstract class implementing `IAccessHandler` with default arrow navigation, wrap-around, Home/End, Enter activation, focus tracking, and `ISearchable` integration.
**When to use:** Every menu screen handler inherits from this.
**Key design decisions:**
- Holds a `List<WidgetInfo>` where `WidgetInfo` wraps a Unity `Component` reference with its speakable label and widget type (button, slider, toggle, label, text input)
- `_currentIndex` tracks focused widget. Navigation methods update this and call `SpeakCurrentWidget()`
- `abstract void DiscoverWidgets(KScreen screen)` -- each subclass implements to enumerate that screen's interactive elements
- `virtual string GetWidgetSpeechText(WidgetInfo widget)` -- builds "label, value" string for the current widget
- `virtual void ActivateWidget(WidgetInfo widget)` -- default calls `KButton.SignalClick(KKeyCode.Mouse0)` for buttons, toggles state for toggles, enters edit mode for text fields
- `virtual void AdjustWidget(WidgetInfo widget, int direction, bool large)` -- moves slider by step or cycles dropdown
- Navigation wrap calls `PlayWrapSound()` which plays the chosen earcon

**Example:**
```csharp
public abstract class BaseMenuHandler : IAccessHandler
{
    protected readonly List<WidgetInfo> _widgets = new List<WidgetInfo>();
    protected int _currentIndex;
    protected KScreen _screen;
    protected TypeAheadSearch _search = new TypeAheadSearch();

    public abstract string DisplayName { get; }
    public bool CapturesAllInput => true; // Menus block all input
    public abstract IReadOnlyList<HelpEntry> HelpEntries { get; }

    public void OnActivate()
    {
        Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
        _currentIndex = 0;
        if (_widgets.Count > 0)
            Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
    }

    public bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.DownArrow: NavigateNext(); return true;
            case KeyCode.UpArrow: NavigatePrev(); return true;
            case KeyCode.Home: NavigateFirst(); return true;
            case KeyCode.End: NavigateLast(); return true;
            // ... etc
        }
    }

    protected void NavigateNext()
    {
        if (_widgets.Count == 0) return;
        int prev = _currentIndex;
        _currentIndex = (_currentIndex + 1) % _widgets.Count;
        if (_currentIndex == 0) PlayWrapSound(); // wrapped
        SpeakCurrentWidget();
    }
    // ...
}
```

### Pattern 2: ContextDetector Screen-to-Handler Registry
**What:** Dictionary mapping `System.Type` (KScreen subclass) to handler factory `Func<KScreen, IAccessHandler>`. When a screen activates, look up its type. If found, create and push a handler. When it deactivates, pop the handler.
**When to use:** `ContextDetector.OnScreenActivated` and `ContextDetector.OnScreenDeactivating`.
**Key insight:** Some screens must be ignored (e.g., `FrontEndBackground`, `BuildWatermark`, `VideoScreen`) -- these are structural UI, not interactive menus. The registry only contains screens that need accessibility handlers.

**Example:**
```csharp
private static readonly Dictionary<Type, Func<KScreen, IAccessHandler>> _registry
    = new Dictionary<Type, Func<KScreen, IAccessHandler>>
{
    { typeof(MainMenu), screen => new MainMenuHandler(screen) },
    { typeof(PauseScreen), screen => new PauseMenuHandler(screen) },
    { typeof(OptionsMenuScreen), screen => new OptionsMenuHandler(screen) },
    { typeof(LoadScreen), screen => new SaveLoadHandler(screen) },
    { typeof(ConfirmDialogScreen), screen => new ConfirmDialogHandler(screen) },
    { typeof(ColonyDestinationSelectScreen), screen => new ColonySetupHandler(screen) },
    { typeof(MinionSelectScreen), screen => new DuplicantSelectHandler(screen) },
    { typeof(WorldGenScreen), screen => new WorldGenHandler(screen) },
    // Audio, Graphics, Game options screens all use OptionsMenuHandler
};

// For screens that don't have an exact type match but share a base:
// AudioOptionsScreen, GraphicsOptionsScreen, GameOptionsScreen -> OptionsMenuHandler
```

### Pattern 3: Widget Discovery and Filtering
**What:** Enumerate interactive widgets from a KScreen's Unity hierarchy, classify them, and build the navigation list.
**When to use:** In each handler's `DiscoverWidgets()` call.
**Key insight:** Must filter OUT mouse-only controls. Identified mouse-only patterns:
- DragMe components (drag handles)
- Resize handles (RectTransform with "resize" in name)
- Scroll rect handles (KScrollRect internal handles)
- Close/X buttons with only pointer events (no text label)
- DLC logo toggles on MainMenu (these are store links, not settings)
- Image-only decorative elements
- The `MakeButton` pattern in `MainMenu` uses `LocText` for button labels -- these ARE interactive
- The `KButtonMenu.buttons` array provides programmatic access to button labels without needing to walk the hierarchy

### Pattern 4: Programmatic Button Activation
**What:** Instead of simulating mouse clicks at coordinates, call the button's click handler directly.
**When to use:** When Enter is pressed on a focused button.
**Key insight:** `KButton.SignalClick(KKeyCode.Mouse0)` invokes the `onClick` delegate. `KToggle.Click()` toggles state and fires callbacks. For `KButtonMenu`-derived screens, the `buttons[i].onClick` delegate can be invoked directly, or `buttonObjects[i].GetComponent<KButton>().SignalClick(KKeyCode.Mouse0)` for consistency.
**Why this matters:** Calling `SignalClick` also triggers the button's sound player, providing natural audio feedback without extra earcon management.

### Anti-Patterns to Avoid
- **Building separate screen management:** Do NOT track which screens are open. Hook into ONI's existing `KScreen.Activate`/`Deactivate` lifecycle. The game already manages the screen stack.
- **Polling screen state in Update():** Do NOT check `KScreenManager.Instance.screenStack` every frame. Use event-driven detection from the existing Harmony patches.
- **Hardcoding widget indices:** Do NOT assume "the 3rd button is Load Game." Discover widgets dynamically by walking the hierarchy and reading labels.
- **Consuming Escape in handlers:** Do NOT handle Escape directly. Per decision: Escape triggers the game's own close behavior. `KModalScreen.OnKeyDown` already handles `Action.Escape` -> `Deactivate()`. Our handler will be popped by the deactivate patch firing `ContextDetector.OnScreenDeactivating`.
- **Speaking widget types:** Do NOT say "button", "slider", "toggle". Just say label and value per user decision.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---|---|---|---|
| Type-ahead search | Custom search buffer with timeout | `TypeAheadSearch` class (port from repo root) | Handles buffer timeout, same-letter cycling, word-start matching, result navigation |
| Rich text stripping | Per-handler text cleaning | `TextFilter.FilterForSpeech()` (Phase 1) | Already handles sprites, links, rich text tags, TMP brackets, whitespace normalization |
| Screen lifecycle detection | Custom screen tracking state machine | Existing Harmony patches on `KScreen.Activate/Deactivate` (Phase 2) | Already fires `ContextDetector.OnScreenActivated/OnScreenDeactivating` |
| Button click simulation | Mouse position warping / ray casting | `KButton.SignalClick(KKeyCode.Mouse0)` | ONI's own programmatic click API, triggers sound too |
| Toggle state changes | Direct `isOn` property mutation | `KToggle.Click()` | Handles state change, visual update, sound, and callbacks |
| Slider value changes | Direct `slider.value` assignment | `slider.value += step` with `onValueChanged` | KSlider inherits from Unity Slider, setting `.value` triggers `onValueChanged` listeners |

**Key insight:** ONI's UI widgets (`KButton`, `KToggle`, `KSlider`) all have programmatic APIs for simulating user interaction. Using these ensures sound effects play naturally and all game-side callbacks fire correctly.

## Common Pitfalls

### Pitfall 1: Multiple KScreen Activations for One Logical Screen
**What goes wrong:** Some screens activate helper screens that are NOT separate UI contexts. For example, `KModalButtonMenu.ActivateChildScreen` hides the parent but keeps it on the stack, activating a child. Both fire `KScreen.Activate`.
**Why it happens:** ONI's screen hierarchy is not 1:1 with user-visible screens. `OptionsMenuScreen` activates child screens like `AudioOptionsScreen` while hiding itself.
**How to avoid:** In `ContextDetector.OnScreenActivated`, check if the new screen should replace or stack on the current handler. For child screens of `KModalButtonMenu`, replace the handler rather than pushing a new one on top of the parent's handler.
**Warning signs:** Two handlers for the same logical flow on the stack.

### Pitfall 2: Screen Deactivation Ordering
**What goes wrong:** `KScreen.Deactivate()` calls `OnDeactivate()` then `PopScreen()` then `Destroy`. Our prefix patch fires before all of this. If the handler tries to read widget state during deactivation, widgets may already be in teardown.
**Why it happens:** Our patch is a Prefix on `Deactivate`, but screen state is still valid at that point.
**How to avoid:** The prefix timing is correct (Phase 2 already chose this). Just don't do heavy work in the deactivate handler -- simply pop the handler off the stack.
**Warning signs:** Null reference exceptions reading widget text during deactivation.

### Pitfall 3: KButtonMenu Button Array Rebuilt on RefreshButtons
**What goes wrong:** `KButtonMenu.RefreshButtons()` destroys all `buttonObjects` and recreates them. If the handler cached references to the old GameObjects, they become null.
**Why it happens:** `PauseScreen` calls `RefreshButtons()` in `OnShow(true)`. `ConfigureButtonInfos()` changes the button list.
**How to avoid:** Re-discover widgets whenever the handler is activated (OnActivate), not just on construction. The `buttons` IList in `KButtonMenu` is the source of truth for labels, while `buttonObjects` are the GameObjects.
**Warning signs:** NullReferenceException on button GameObjects after screen refresh.

### Pitfall 4: KScreen Classes Without Corresponding KScreenManager Presence
**What goes wrong:** Some `KScreen` subclasses (like `CharacterContainer`) are not pushed via `KScreenManager.PushScreen`. They exist as child components within a parent screen.
**Why it happens:** `CharacterContainer` inherits `KScreen` but is used as a UI component inside `MinionSelectScreen`, not as a standalone screen.
**How to avoid:** The `KScreen_Activate_Patch` fires for ALL `KScreen.Activate()` calls. Filter in `ContextDetector` to only handle registered screen types. `CharacterContainer` should NOT get its own handler -- it's navigated WITHIN `DuplicantSelectHandler`.
**Warning signs:** Unexpected handler pushes for non-screen components.

### Pitfall 5: Key Conflicts Between TypeAheadSearch and Game Input
**What goes wrong:** A-Z keys typed for search also generate KButtonEvents that the game might interpret as hotkeys.
**Why it happens:** ModInputRouter sees KButtonEvents at priority 50, before KScreenManager. But KeyPoller catches raw keypresses in Update(). If A-Z is handled in KeyPoller, the KButtonEvent for the same key still arrives in ModInputRouter.
**How to avoid:** Handle A-Z search in `HandleKeyDown(KButtonEvent e)` by checking `e.Controller.GetKeyDown(KKeyCode.A)` through `KKeyCode.Z`. Consume the event with `e.Consumed = true` when search handles it. This prevents the key from reaching the game's screen handlers.
**Warning signs:** Typing "s" for search also triggers the game's "Save" hotkey.

### Pitfall 6: WorldGenScreen Has No Interactive Widgets
**What goes wrong:** `WorldGenScreen` blocks Escape and has no buttons during generation. It's a progress display, not a navigable screen.
**Why it happens:** `WorldGenScreen.OnKeyDown` consumes Escape and blocks all navigation.
**How to avoid:** `WorldGenHandler` should NOT be a `BaseMenuHandler` subclass. It should be a specialized handler that polls `OfflineWorldGen.currentPercent` and speaks progress at intervals. It does NOT navigate widgets.
**Warning signs:** Empty widget list, stuck handler.

### Pitfall 7: Adding Too Many Keys to KeyPoller
**What goes wrong:** KeyPoller polls raw keys in `Update()`. Adding many keys (Home, End, Tab, Shift+Tab, A-Z, Shift+I) to the poll list would mean checking dozens of keys every frame.
**Why it happens:** KeyPoller was designed for a small set of truly unbound keys.
**How to avoid:** Home and End have no ONI Action binding, so they must be polled. Tab has no Action binding either. But Shift+I might map to a game action -- check. A-Z keys DO generate KButtonEvents (they're mapped to game actions), so handle them via `HandleKeyDown(KButtonEvent e)`, NOT via KeyPoller. Only poll keys that have NO KButtonEvent equivalent.
**Warning signs:** Performance issues from polling, or duplicate handling of the same key.

## Code Examples

### Widget Discovery in a KButtonMenu Screen
```csharp
// For screens inheriting KButtonMenu (MainMenu, PauseScreen, OptionsMenuScreen):
// The buttons IList provides labels, buttonObjects provides GameObjects
protected override void DiscoverWidgets(KScreen screen)
{
    _widgets.Clear();
    var buttonMenu = screen as KButtonMenu;
    if (buttonMenu == null) return;

    // Use reflection or Traverse to access protected 'buttons' and 'buttonObjects'
    var buttons = Traverse.Create(buttonMenu).Field("buttons").GetValue<IList<KButtonMenu.ButtonInfo>>();
    var buttonObjects = Traverse.Create(buttonMenu).Field("buttonObjects").GetValue<GameObject[]>();

    if (buttons == null || buttonObjects == null) return;

    for (int i = 0; i < buttons.Count && i < buttonObjects.Length; i++)
    {
        if (buttonObjects[i] == null || !buttonObjects[i].activeInHierarchy) continue;

        var kbutton = buttonObjects[i].GetComponent<KButton>();
        if (kbutton == null || !kbutton.isInteractable) continue;

        _widgets.Add(new WidgetInfo
        {
            Label = buttons[i].text,
            Component = kbutton,
            Type = WidgetType.Button,
            GameObject = buttonObjects[i]
        });
    }
}
```

### Programmatic Button Click (Enter key)
```csharp
protected virtual void ActivateWidget(WidgetInfo widget)
{
    switch (widget.Type)
    {
        case WidgetType.Button:
            var kbutton = widget.Component as KButton;
            kbutton?.SignalClick(KKeyCode.Mouse0);
            break;
        case WidgetType.Toggle:
            var ktoggle = widget.Component as KToggle;
            ktoggle?.Click();
            // Speak new state
            string state = ktoggle.isOn ? "on" : "off";
            SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
            break;
    }
}
```

### Slider Adjustment (Left/Right keys)
```csharp
protected virtual void AdjustWidget(WidgetInfo widget, int direction, bool large)
{
    if (widget.Type == WidgetType.Slider)
    {
        var slider = widget.Component as KSlider;
        if (slider == null) return;

        float step;
        if (slider.wholeNumbers)
        {
            step = large ? 10f : 1f;
        }
        else
        {
            float range = slider.maxValue - slider.minValue;
            step = large ? range * 0.1f : range * 0.01f;
        }

        slider.value = Mathf.Clamp(slider.value + step * direction, slider.minValue, slider.maxValue);
        // KSlider.onValueChanged fires automatically from setting .value
        SpeechPipeline.SpeakInterrupt($"{widget.Label}, {FormatSliderValue(slider)}");
    }
    else if (widget.Type == WidgetType.Dropdown)
    {
        // Cycle in-place: invoke the game's cycle function
        // For CustomGameSettingListWidget, call CycleQualitySettingLevel
        CycleDropdown(widget, direction);
        SpeechPipeline.SpeakInterrupt($"{widget.Label}, {GetDropdownValue(widget)}");
    }
}
```

### ToolTip Reading (Shift+I)
```csharp
// Read tooltip text for the currently focused widget
protected void SpeakTooltip()
{
    if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
    var widget = _widgets[_currentIndex];

    var tooltip = widget.GameObject.GetComponent<ToolTip>();
    if (tooltip == null) return;

    // ToolTip stores text in multiStringToolTips list
    string text = tooltip.GetMultiString(0); // or iterate for multi-string
    if (string.IsNullOrEmpty(text)) return;

    SpeechPipeline.SpeakInterrupt(text);
}
```

### WorldGen Progress Polling
```csharp
public class WorldGenHandler : IAccessHandler
{
    private float _lastSpokenPercent = -1f;
    private float _pollInterval = 2f; // seconds between progress reports
    private float _lastPollTime;

    public void OnActivate()
    {
        SpeechPipeline.SpeakInterrupt("Generating world");
        _lastSpokenPercent = 0f;
        _lastPollTime = Time.time;
    }

    // Called from a coroutine or Update check
    public void CheckProgress()
    {
        if (Time.time - _lastPollTime < _pollInterval) return;
        _lastPollTime = Time.time;

        // Access OfflineWorldGen.currentPercent via Traverse
        float percent = Traverse.Create(WorldGenScreen.Instance)
            .Field("offlineWorldGen").Field("currentPercent").GetValue<float>();

        int rounded = Mathf.RoundToInt(percent * 100f);
        if (rounded > _lastSpokenPercent + 10) // speak every 10% jump
        {
            SpeechPipeline.SpeakInterrupt($"{rounded} percent");
            _lastSpokenPercent = rounded;
        }

        if (percent >= 1f)
        {
            SpeechPipeline.SpeakInterrupt("World generation complete");
        }
    }
}
```

### ContextDetector Handler Mapping
```csharp
public static void OnScreenActivated(KScreen screen)
{
    if (screen == null) return;

    var screenType = screen.GetType();
    if (!_registry.TryGetValue(screenType, out var factory))
    {
        // Check base types for generic handling
        // e.g., AudioOptionsScreen -> use generic OptionsSubScreenHandler
        if (screen is KModalScreen && !(screen is NewGameFlowScreen))
        {
            // Could be a dialog or options sub-screen
            // Try parent type lookup
        }
        Log.Debug($"No handler for screen: {screenType.Name}");
        return;
    }

    var handler = factory(screen);
    HandlerStack.Push(handler);
}

public static void OnScreenDeactivating(KScreen screen)
{
    if (screen == null) return;

    // Only pop if the active handler corresponds to this screen
    var active = HandlerStack.ActiveHandler;
    if (active is BaseMenuHandler menuHandler && menuHandler.Screen == screen)
    {
        HandlerStack.Pop();
    }
}
```

## Discretion Recommendations

### Slider Step Size
**Recommendation:** Use `slider.wholeNumbers` to determine behavior. When `wholeNumbers` is true, step = 1 (minor) / 10 (major). When false, use 1% of range (minor) / 10% of range (major). ONI's AudioOptionsScreen sliders are 0-100 floats for volume, so 1% = 1 unit, 10% = 10 units. This maps naturally to the user expectation of "1 step" and "10 steps."
**Confidence:** HIGH -- directly verified by reading `KSlider` inheriting Unity `Slider` which has `wholeNumbers`, `minValue`, `maxValue` properties.

### Wrap-Around Earcon
**Recommendation:** Use `"HUD_Click_Close"` for the wrap-around sound. It's a short, distinctive UI sound that ONI already uses for closing/boundary actions. Other candidates: `"Negative"` (too harsh -- implies error), `"HUD_Click"` (too similar to button activation). `"HUD_Click_Deselect"` is another option -- slightly softer than Close.
**Confidence:** MEDIUM -- based on sound name analysis from decompiled code. Actual sound character requires in-game testing. The sound is played via `KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click_Close"))`.

### WorldGen Progress Polling Interval
**Recommendation:** Poll every 2 seconds, speak at 25% intervals (25, 50, 75, 100). `OfflineWorldGen.Update()` updates `currentPercent` every frame from the background thread. Polling less frequently avoids speech spam but keeps the user informed. The `currentPercent` field is private -- use `Traverse.Create(offlineWorldGen).Field("currentPercent").GetValue<float>()`.
**Confidence:** HIGH -- verified from decompiled `OfflineWorldGen.Update()` which reads `currentPercent` and displays it as `percentText.text`.

### Base Class Design
**Recommendation:** Use an abstract class, not an interface. Reasons:
1. State is required: `_widgets`, `_currentIndex`, `_search`, `_screen` must be held per-handler
2. Most behavior is shared: arrow nav, wrap, Home/End, focus tracking, speech queuing
3. Per-screen customization via `abstract DiscoverWidgets()`, `virtual GetWidgetSpeechText()`, `virtual ActivateWidget()`, `virtual AdjustWidget()`
4. `ISearchable` implementation can live in the base class with `SearchItemCount => _widgets.Count`, `GetSearchLabel(i) => _widgets[i].Label`, `SearchMoveTo(i)` updating `_currentIndex` and speaking

Make these methods virtual (overridable): `GetWidgetSpeechText`, `ActivateWidget`, `AdjustWidget`, `OnActivate`, `OnDeactivate`, `HandleKeyDown`, `HandleKeyUp`, `HandleUnboundKey`. Make these abstract (required): `DiscoverWidgets`, `DisplayName`, `HelpEntries`.
**Confidence:** HIGH -- follows the same pattern as HelpHandler but with extensible base behavior.

### Handler Registry Structure
**Recommendation:** Dictionary<Type, Func<KScreen, IAccessHandler>> in `ContextDetector`. Registered during mod initialization. Supports exact type matching plus fallback to base type matching for screens that share handling logic (e.g., all options sub-screens use a generic settings handler).

For screens that are part of `NewGameFlow`, the flow order is:
1. `ClusterCategorySelectionScreen` (game mode select -- Survival/No Sweat)
2. `ColonyDestinationSelectScreen` (asteroid selection, settings)
3. `MinionSelectScreen` (duplicant selection)
4. `WorldGenScreen` (world generation progress -- triggers on NavigateForward from MinionSelectScreen)

Each gets its own handler via the registry. The `NewGameFlow` component on `MainMenu` manages screen transitions internally -- our patches just react to each screen's activate/deactivate.
**Confidence:** HIGH -- verified `NewGameFlow.newGameFlowScreens` list and screen activation flow from decompiled code.

## Key Technical Findings

### Finding 1: KeyPoller Extension Requirements
**Source:** Decompiled ONI code analysis
**Confidence:** HIGH

Keys that MUST be added to `KeyPoller.PollKeys`:
- `KeyCode.Home` -- no ONI Action binding
- `KeyCode.End` -- no ONI Action binding
- `KeyCode.Tab` -- no ONI Action binding (ONI uses it for cycling, but via a different mechanism)

Keys that should NOT be in KeyPoller (handle via KButtonEvent in HandleKeyDown):
- A-Z keys -- these generate KButtonEvents (mapped to game debug/cheat actions)
- Enter/Return -- mapped to `Action.Escape` or consumed by KScreen
- Shift+I -- Shift is a modifier; I has a KButtonEvent. Check modifiers in HandleKeyDown

Keys that are already in KeyPoller:
- F12, UpArrow, DownArrow, LeftArrow, RightArrow

### Finding 2: Screen Class Hierarchy Map
**Source:** ONI decompiled sources
**Confidence:** HIGH

```
KScreen
  KButtonMenu
    KModalButtonMenu        -- OptionsMenuScreen, PauseScreen
  KModalScreen              -- AudioOptionsScreen, GraphicsOptionsScreen, GameOptionsScreen,
                               LoadScreen, ConfirmDialogScreen
    NewGameFlowScreen       -- ColonyDestinationSelectScreen, WorldGenScreen
      CharacterSelectionController -- (abstract)
        MinionSelectScreen
  CharacterContainer         -- NOT a standalone screen; child component
  MainMenu                  -- Direct KScreen subclass (not modal)
```

### Finding 3: MainMenu Button Structure
**Source:** `MainMenu.OnPrefabInit()` decompiled
**Confidence:** HIGH

MainMenu creates buttons dynamically via `MakeButton(ButtonInfo)`. The button order is:
1. Resume Game (only if save exists, positioned via `Button_ResumeGame`)
2. New Game
3. Load Game
4. Retired Colonies
5. Locker Menu
6. Translations (Steam only)
7. Mods (Steam only)
8. Options
9. Quit to Desktop

These are KButton instances with LocText children containing the label. The `Button_ResumeGame` is a separate serialized field, not in the MakeButton list. The MainMenu inherits directly from `KScreen` (NOT KButtonMenu), so we cannot use the `buttons` array pattern. Widget discovery must walk the `buttonParent` transform children.

### Finding 4: KModalScreen Consumes ALL Input
**Source:** `KModalScreen.OnKeyDown` decompiled
**Confidence:** HIGH

`KModalScreen.OnKeyDown` ends with `e.Consumed = true` -- it blocks ALL events from reaching screens below it on the KScreenManager stack. This means:
- Our ModInputRouter (priority 50) sees events BEFORE KModalScreen
- When a modal screen is open, we can intercept keys before the game's modal screen handling
- When our handler has `CapturesAllInput = true` and the screen also consumes all input, there's no conflict -- we process first, then if we don't consume, the screen does

### Finding 5: Enter Key Routing
**Source:** ONI Action/KKeyCode analysis
**Confidence:** HIGH

The Enter key is mapped to `Action.Escape` in ONI's default bindings (this is NOT a typo -- Return/Enter and Escape may share bindings depending on context). More precisely, KKeyCode.Return generates `Action.Escape` in some contexts. This needs careful testing. The safe approach: handle Enter/Return via `HandleUnboundKey` if it's not bound, or via `HandleKeyDown` checking `e.Controller.GetKeyDown(KKeyCode.Return)`.

**Update after further analysis:** Looking at ONI's `BindingEntry` system, Return/Enter likely has NO default Action binding (it's not in `GameInputMapping.KeyBindings` for standard gameplay). This means it would need to be polled via KeyPoller. Add `KeyCode.Return` to the poll list.

### Finding 6: CharacterContainer Widget Structure
**Source:** `CharacterContainer.SetInfoText()` decompiled
**Confidence:** HIGH

Each duplicant's CharacterContainer has:
- `characterNameTitle` (EditableTitleBar) -- dupe name
- `characterJob` (LocText) -- not visibly used in starter selection
- Trait entries: dynamically created LocText elements (good/bad traits with ToolTip descriptions)
- Aptitude entries: LocText labels with attribute values
- `description` (LocText) -- personality description
- `reshuffleButton` (KButton) -- reroll this dupe
- `archetypeDropDown` (DropDown) -- interest filter
- `modelDropDown` (DropDown) -- minion type filter (Standard/Bionic, DLC3 only)
- `selectButton` (KToggle) -- select/deselect for Printing Pod use (disabled for starters)
- Outfit selector (expand/collapse with next/previous buttons)

The attributes are in `iconGroups` -- dynamically created with LocText containing "+3 Athletics" style text and ToolTip with full description.

### Finding 7: Save/Load Screen Structure
**Source:** `LoadScreen` decompiled
**Confidence:** HIGH

LoadScreen has two views:
1. **Colony list** (`colonyListRoot`): List of colonies, each a `HierarchyReferences` entry with:
   - `HeaderTitle` (LocText) -- colony name
   - `HeaderDate` (LocText) -- last save date
   - `SaveTitle` (LocText) -- save count and size info
   - `Button` (KButton) -- click to view colony saves
2. **Colony save view** (`colonyViewRoot`): Individual saves for a colony:
   - Each save entry has `SaveText`, `DateText`, `AutoLabel`, `NewestLabel`, `LoadButton`
   - Colony-level info: Title, Date, World info, Cycles survived, Duplicants alive, File size

Per decision, speak: colony name, cycle number, duplicant count, date. Omit file size.

### Finding 8: OfflineWorldGen Progress Access
**Source:** `OfflineWorldGen` decompiled
**Confidence:** HIGH

`OfflineWorldGen` has:
- `currentPercent` (float, 0-1) -- overall progress
- `currentConvertedCurrentStage` (StringKey) -- current stage label (localized)
- `updateText` (LocText) -- displays stage name
- `percentText` (LocText) -- displays percent

Both `currentPercent` and `currentConvertedCurrentStage` are private fields. Access via Harmony `Traverse`. The `Update()` method on OfflineWorldGen updates `percentText.text` with `GameUtil.GetFormattedPercent(currentPercent * 100f)`.

Alternative approach: Instead of accessing private fields, read `percentText.text` and `updateText.text` directly from the LocText components. This avoids reflection entirely -- just get the component references from the WorldGenScreen's hierarchy.

## Open Questions

1. **Enter/Return key binding status**
   - What we know: Return/Enter may or may not have an ONI Action binding
   - What's unclear: Whether it generates a KButtonEvent or needs KeyPoller polling
   - Recommendation: Add `KeyCode.Return` to KeyPoller. If it turns out to also generate KButtonEvents, the handler can consume the KButtonEvent and KeyPoller will be redundant (not harmful). Test in-game.

2. **ClusterCategorySelectionScreen (game mode select) widget structure**
   - What we know: It's a `NewGameFlowScreen` with `MultiToggle[]` buttons for Survival/No Sweat/Custom
   - What's unclear: Exact widget hierarchy for reading mode descriptions alongside names
   - Recommendation: Research this screen during plan 03-03 implementation. Use `GetComponentInChildren<LocText>()` to find labels and descriptions.

3. **Shift+I key detection method**
   - What we know: Shift is a modifier, I has a KKeyCode
   - What's unclear: Whether `I` generates a KButtonEvent when Shift is held (Shift+I might be interpreted differently)
   - Recommendation: Handle in `HandleKeyDown` by checking `e.Controller.GetKeyDown(KKeyCode.I)` while `UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift)`. This is the same pattern used for Ctrl+Shift+F12 in KeyPoller.

4. **Multiple handlers for same screen type**
   - What we know: `ConfirmDialogScreen` can appear anywhere -- from main menu, options, pause, save/load
   - What's unclear: Whether the confirm dialog handler should stack on top of the current handler or replace it
   - Recommendation: Stack (push) -- the confirm dialog is truly modal and temporary. When it deactivates, our patch pops it, revealing the previous handler underneath. This is exactly how `HelpHandler` works.

## Sources

### Primary (HIGH confidence)
- ONI decompiled Assembly-CSharp: KScreen, KScreenManager, KModalScreen, KModalButtonMenu, KButtonMenu, MainMenu, OptionsMenuScreen, ColonyDestinationSelectScreen, MinionSelectScreen, CharacterContainer, CharacterSelectionController, LoadScreen, WorldGenScreen, OfflineWorldGen, ConfirmDialogScreen, PauseScreen, NewGameFlow, NewGameFlowScreen, NewGameSettingsPanel, KButton, KSlider, KToggle, KInputTextField, ToolTip, AudioOptionsScreen
- ONI decompiled Assembly-CSharp-firstpass: KScreen, KScreenManager, KButton, KSlider, KToggle, ToolTip, KInputTextField
- Existing mod code: IAccessHandler, HandlerStack, ModInputRouter, KeyPoller, ContextDetector, HelpHandler, WorldHandler, SpeechPipeline, TextFilter, InputArchPatches, Mod.cs, OniAccessStrings
- TypeAheadSearch.cs (repo root) -- proven search implementation to port

### Secondary (MEDIUM confidence)
- Sound name analysis from `GlobalAssets.GetSound()` calls across decompiled codebase: "HUD_Click", "HUD_Click_Close", "HUD_Click_Deselect", "HUD_Mouseover", "Negative"

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all components are existing game systems or existing mod infrastructure
- Architecture: HIGH -- patterns verified against decompiled screen code
- Pitfalls: HIGH -- identified from concrete code analysis, not speculation
- Widget interaction: HIGH -- KButton.SignalClick, KToggle.Click, KSlider.value all verified from decompiled source
- Discretion recommendations: MEDIUM-HIGH -- sound choice needs in-game verification

**Research date:** 2026-02-11
**Valid until:** 2026-03-11 (stable -- ONI update schedule is infrequent)
