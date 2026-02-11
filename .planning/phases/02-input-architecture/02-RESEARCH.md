# Phase 2: Input Architecture - Research

**Researched:** 2026-02-11
**Domain:** ONI input pipeline (KInputHandler, KScreen, KScreenManager), Harmony patching, handler-based input architecture
**Confidence:** HIGH

## Summary

ONI has a sophisticated priority-based input handler system built around `KInputHandler`, `KInputController`, and `KScreenManager`. The controller translates raw Unity `Input.GetKeyDown` calls into `KButtonEvent` objects mapped to ONI's `Action` enum, then dispatches them through a tree of `KInputHandler` children sorted by descending priority. `KScreenManager` manages a stack of `KScreen` instances and dispatches events top-down (highest sort key first), with the `e.Consumed` pattern preventing further propagation.

The existing Phase 1 mod uses a MonoBehaviour `Update()` approach with `Input.GetKeyDown` that runs in parallel to ONI's input system. Phase 2 needs to replace this with a Harmony-patched approach that intercepts events **inside** ONI's dispatch chain, giving the mod the ability to consume events before game handlers see them. The key insight is that we should insert our handler into the `KInputController.inputHandler` child list at a priority higher than any game handler (>20, since PlayerController is 20), allowing our mod to see and optionally consume every `KButtonEvent` before the game does.

**Primary recommendation:** Hook into ONI's existing `KInputHandler` priority system via a Harmony postfix on `InputInit.Awake` to register a high-priority `IInputHandler` that delegates to the mod's active handler stack. Use Harmony patches on `KScreen.Activate`/`KScreen.Deactivate` for context detection. Keep the architecture simple: one `ModInputRouter` registered with the game's input system, internally managing a stack of `IAccessHandler` instances.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Mode announcements (handler convention for Phase 3+)
- Always announce context switches -- every handler activation should speak its name
- Name first, vary early: "Build menu" not "Menu, build"
- Mode announcements interrupt current speech
- Returning to world view is also announced
- Sequencing concern for Phase 3: when a menu opens, the mode announcement interrupts, then the focused-item announcement must queue after it (e.g., "Main menu" then "New game"). The input architecture should not block this sequencing. Capture this as a Phase 3 constraint.

#### VanillaMode redesign
- Toggle OFF: speak "Oni-Access off" then full disable -- all input handlers deactivate, all keys pass through to game, speech stops. Only Ctrl+Shift+F12 remains active.
- Toggle ON: speak "Oni-Access on" only -- no state dump, no context announcement. Immediately detect current game state and activate appropriate handler.
- No background work when mod is off -- full stop. No passive state tracking.
- Toggle key stays Ctrl+Shift+F12

#### Help system (F12)
- F12 opens a navigable list (arrow keys to step through entries), not a single speech dump
- Show only the active handler's keys -- no global keys mixed in
- When mod is on, there is always an active handler (context detection ensures this), so F12 always has content

#### Key claim behavior
- Selective claim by default: each handler declares which keys it wants, everything else passes through to the game
- Full capture for menus: menu handlers block ALL keyboard input to prevent accidental game actions while navigating
- Silent handling: when the mod claims a key, it just handles it -- no feedback about the key being "stolen" from the game
- No passthrough modifier: VanillaMode is the escape hatch for full game access
- WASD/arrow key decisions are Phase 4 (world navigation), not Phase 2

### Claude's Discretion
- Handler architecture approach (IInputHandler stack vs Harmony hooks on KScreen.OnKeyDown vs state machine vs hybrid). Research will determine what works best with ONI's existing input system.
- Context detection mechanism (Harmony patches on OnActivate/OnDeactivate, KScreenManager polling, ToolMenu events, or combination)
- How the help mode handler itself works (it's a handler that takes over input to let arrows navigate the help list)
- Migration strategy from Phase 1's HotkeyRegistry/InputInterceptor to the new system

### Deferred Ideas (OUT OF SCOPE)
- WASD camera panning behavior when cursor is active -- Phase 4 (World Navigation) decision
- Actual mode announcement speech text for specific screens -- Phase 3+ handlers implement this
- Menu-specific full-capture key lists -- Phase 3 (Menu Navigation) defines which keys menus capture
- Speech sequencing for "container then focused item" -- Phase 3 constraint
</user_constraints>

## Standard Stack

### Core
| Library/System | Version | Purpose | Why Standard |
|---|---|---|---|
| ONI's KInputHandler | Game built-in | Priority-based input event dispatch tree | This IS the game's input system -- hooking into it ensures proper event consumption |
| ONI's KScreenManager | Game built-in | Screen stack management, top-down event dispatch | Provides natural context detection via screen push/pop |
| ONI's KButtonEvent | Game built-in | Action-based event with Consumed pattern | The game's event model -- TryConsume(Action) is how all handlers claim input |
| HarmonyLib (0Harmony) | Game-bundled | Runtime method patching for hooks | Only way to intercept game lifecycle methods (InputInit.Awake, KScreen.Activate) |

### Supporting
| Library/System | Version | Purpose | When to Use |
|---|---|---|---|
| UnityEngine.Input | Unity built-in | Raw key detection for toggle hotkey | Only for Ctrl+Shift+F12 when mod is OFF (outside handler system) |
| UnityEngine.KeyCode | Unity built-in | Key identification for handler key declarations | Handlers declare which keys they claim using KeyCode |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|---|---|---|
| Hooking into KInputHandler tree | Separate MonoBehaviour Update() (current Phase 1) | MonoBehaviour can't consume events before the game sees them -- keys still reach ONI. KInputHandler integration gives proper consumption. |
| Harmony prefix on KScreen.OnKeyDown | Harmony postfix on KScreenManager.OnKeyDown | Prefix on individual screens is fragile (must patch many classes). KScreenManager postfix gives one hook point but fires after screens. Better: register as KInputHandler child at high priority. |
| KInputHandler child registration | Harmony prefix on KInputController.Dispatch | Dispatch prefix could intercept all events but is more invasive. KInputHandler.AddInputHandler is the game's own extensibility mechanism. |

### Installation
No additional packages needed. All references already in OniAccess.csproj:
- `Assembly-CSharp.dll` (KScreen, Game, PlayerController, etc.)
- `Assembly-CSharp-firstpass.dll` (KInputHandler, KInputController, KButtonEvent, IInputHandler, etc.)
- `0Harmony.dll` (Harmony patching)
- `UnityEngine.InputLegacyModule.dll` (UnityEngine.Input)

## Architecture Patterns

### ONI Input Pipeline (as discovered from decompiled source)

```
Unity Input.GetKeyDown
    |
    v
KInputController.Update()
    - Polls all registered KeyDefs (key+modifier combos)
    - Checks active modifiers (Ctrl/Shift/Alt/CapsLock/Backtick)
    - Creates KButtonEvent with action flags (bool[] mapping key to all matching Actions)
    - Queues events in mEvents list
    |
    v
KInputController.Dispatch()
    - Iterates mEvents, calls inputHandler.HandleEvent(event) for each
    - inputHandler is a KInputHandler tree rooted at the controller
    |
    v
KInputHandler.HandleKeyDown(KButtonEvent e)
    - First: calls own OnKeyDown delegates (the controller itself)
    - If NOT consumed: iterates mChildren (sorted by DESCENDING priority)
    - Each child.HandleKeyDown(e) -- stops if e.Consumed becomes true
    |
    v
Children (sorted by priority, highest first):
    Priority 20: PlayerController.OnKeyDown  -- tools, mouse, escape
    Priority 10: KScreenManager.OnKeyDown    -- dispatches to screen stack
    Priority  1: CameraController.OnKeyDown  -- WASD pan, speed, overlays
    Priority -1: DebugHandler.OnKeyDown      -- debug actions (only if enabled)
```

### KScreenManager Screen Stack Dispatch

```
KScreenManager.OnKeyDown(KButtonEvent e)
    - Iterates screenStack from END to START (highest sort key first)
    - Calls kScreen.OnKeyDown(e) for each active screen
    - Stops if e.Consumed

Screen Sort Keys (from KScreen subclasses):
    100: KModalScreen (PauseScreen, dialogs) -- gets input first
     50: Editing screens
     21: ManagementMenu
      5: ToolMenu
      0: Default KScreen
```

### KScreen Lifecycle (Context Detection Signals)

```
KScreen.Activate()
    - gameObject.SetActive(true)
    - KScreenManager.Instance.PushScreen(this)  <-- screen enters stack
    - OnActivate()                               <-- virtual hook point
    - isActive = true

KScreen.Deactivate()
    - OnDeactivate()                             <-- virtual hook point
    - isActive = false
    - KScreenManager.Instance.PopScreen(this)    <-- screen leaves stack
    - Object.Destroy(gameObject)
```

### Recommended Architecture: ModInputRouter

```
OniAccess/
  Input/
    IAccessHandler.cs          -- Interface for mod input handlers
    ModInputRouter.cs          -- Single IInputHandler registered with game
    HandlerStack.cs            -- Manages active handler stack
    HelpHandler.cs             -- F12 navigable help list handler
    ContextDetector.cs         -- Harmony patches for screen lifecycle
    InputArchPatches.cs        -- Harmony patches (InputInit, KScreen)
  Toggle/
    VanillaMode.cs             -- Redesigned: full disable/enable
```

### Pattern 1: Single IInputHandler Registration
**What:** Register one `ModInputRouter` as a child of the KInputController's handler tree at priority > 20 (e.g., 50), so it receives every KButtonEvent before PlayerController, KScreenManager, or CameraController.
**When to use:** Always -- this is the core interception point.
**Source:** Discovered from `InputInit.Awake` (ONI-Decompiled/Assembly-CSharp/InputInit.cs) and `KInputHandler.AddInputHandler` (ONI-Decompiled/Assembly-CSharp-firstpass/KInputHandler.cs)

```csharp
// ModInputRouter implements IInputHandler, gets registered via Harmony patch
public class ModInputRouter : IInputHandler
{
    public string handlerName => "OniAccess";
    public KInputHandler inputHandler { get; set; }

    public void OnKeyDown(KButtonEvent e)
    {
        if (!VanillaMode.IsEnabled) return;  // pass everything through

        // Check toggle key first (Ctrl+Shift+F12) -- always active
        if (CheckToggleKey(e)) return;

        // Delegate to active handler
        var handler = HandlerStack.ActiveHandler;
        if (handler != null)
        {
            handler.HandleKeyDown(e);
        }
    }
}

// Registration via Harmony postfix on InputInit.Awake
[HarmonyPatch(typeof(InputInit), "Awake")]
static class InputInit_Awake_Patch
{
    static void Postfix()
    {
        var router = new ModInputRouter();
        var controller = Global.GetInputManager().GetDefaultController();
        // Priority 50 -- above PlayerController (20)
        KInputHandler.Add(controller, router, 50);
    }
}
```

### Pattern 2: IAccessHandler Interface
**What:** Each game context has a handler that declares its keys and processes events.
**When to use:** Every handler (world, menu, build, help) implements this.

```csharp
public interface IAccessHandler
{
    /// Display name spoken on activation (e.g., "World view", "Build menu")
    string DisplayName { get; }

    /// Whether this handler captures ALL keyboard input (true for menus)
    bool CapturesAllInput { get; }

    /// Keys this handler claims (for selective claim mode)
    /// Only checked when CapturesAllInput is false
    IReadOnlyList<KeyCode> ClaimedKeys { get; }

    /// Help entries for F12 help list
    IReadOnlyList<HelpEntry> HelpEntries { get; }

    /// Process a key down event. Return true if consumed.
    bool HandleKeyDown(KButtonEvent e);

    /// Process a key up event. Return true if consumed.
    bool HandleKeyUp(KButtonEvent e);

    /// Called when this handler becomes active
    void OnActivate();

    /// Called when this handler is deactivated
    void OnDeactivate();
}
```

### Pattern 3: Context Detection via Harmony Patches on KScreen Lifecycle
**What:** Patch `KScreen.Activate` and `KScreen.Deactivate` to detect screen transitions and switch the active handler.
**When to use:** For detecting when menus open/close, build mode enters/exits, etc.
**Source:** Discovered from KScreen.Activate/Deactivate (ONI-Decompiled/Assembly-CSharp-firstpass/KScreen.cs)

```csharp
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
static class KScreen_Activate_Patch
{
    static void Postfix(KScreen __instance)
    {
        if (!VanillaMode.IsEnabled) return;
        ContextDetector.OnScreenActivated(__instance);
    }
}

[HarmonyPatch(typeof(KScreen), nameof(KScreen.Deactivate))]
static class KScreen_Deactivate_Patch
{
    static void Prefix(KScreen __instance)
    {
        if (!VanillaMode.IsEnabled) return;
        ContextDetector.OnScreenDeactivating(__instance);
    }
}
```

### Pattern 4: Key Claim with KButtonEvent.TryConsume
**What:** When the mod wants to consume a key, it must call `e.TryConsume(Action)` or set `e.Consumed = true` on the KButtonEvent. This prevents the event from reaching lower-priority handlers.
**When to use:** Every time a handler claims a key.
**Critical insight:** KButtonEvent.TryConsume checks `e.IsAction(action)` -- it matches against the game's Action enum, not raw keycodes. For keys the mod intercepts that ARE mapped to game Actions (like arrow keys in BuildMenu), we must consume the matching Action. For keys NOT mapped to any Action (like F12 without modifiers since it's unbound), we must set `e.Consumed = true` directly.

```csharp
// For a key that maps to a game Action (e.g., Escape):
if (e.TryConsume(Action.Escape))
{
    // Handled -- game won't see it
    return true;
}

// For consuming ALL actions on this event (full capture mode):
e.Consumed = true;
return true;
```

### Pattern 5: Toggle Key Outside the Handler System
**What:** Ctrl+Shift+F12 must work even when the mod is OFF. Since "mod off" means our KInputHandler is inactive, we need a secondary check.
**When to use:** Only for the toggle hotkey.
**Approach:** The ModInputRouter's OnKeyDown is always called (it's in the handler tree), but when VanillaMode.IsEnabled is false, it only checks the toggle key. The key is Ctrl+Shift+F12, which maps to the game's `Action.DebugTriggerError` (Modifier 6 = Ctrl+Shift, KKeyCode.F12). However, DebugHandler only processes this when `DebugHandler.enabled` is true (requires debug_enable.txt). In normal gameplay, this Action fires but goes unconsumed, so our handler at priority 50 sees it first.

**Alternative approach (safer):** Keep a minimal MonoBehaviour that only checks `Input.GetKeyDown(KeyCode.F12)` with Ctrl+Shift modifiers, specifically for the toggle. This avoids any dependency on the game's Action mapping for the toggle.

### Anti-Patterns to Avoid
- **Patching individual KScreen subclasses for input:** There are 150+ KScreen subclasses. Patch KScreen base class or KScreenManager, not individual screens.
- **Using `Input.GetKeyDown` for handled keys:** This runs in parallel to the game's input system and cannot consume events. Use KButtonEvent only.
- **Building a separate event system:** ONI already has a robust one. Hook into it, don't duplicate it.
- **Checking `UnityEngine.Input.GetKey` for modifier detection in handlers:** The KButtonEvent already carries modifier information via its Action mapping. Use `e.IsAction(Action)` or `e.GetAction()`.
- **Using ONI's `Action` enum for mod-only keys:** Arrow keys are unbound in ONI -- they have no Action enum value. For keys without an Action, check `e.Consumed` status and use raw keycode detection only when necessary.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---|---|---|---|
| Event consumption/propagation | Custom event cancellation system | KButtonEvent.TryConsume / e.Consumed | Game's own pattern, all 150+ screens use it |
| Input priority ordering | Custom priority queue | KInputHandler.AddInputHandler(handler, priority) | Game's tree already sorts by priority |
| Screen stack tracking | Manual screen state tracking | KScreenManager.Instance.screenStack (via Harmony) | Game already tracks this |
| Key-to-action mapping | Custom keycode-to-action lookup | KButtonEvent.IsAction(Action) / e.GetAction() | Game's KInputController already resolves this per frame |

**Key insight:** ONI's input system is mature and well-structured. The mod should integrate into it (via one high-priority IInputHandler), not build alongside it. The MonoBehaviour approach from Phase 1 was appropriate for simplicity, but Phase 2 needs proper integration to claim keys.

## Common Pitfalls

### Pitfall 1: Arrow Keys Have No Game Action
**What goes wrong:** Arrow keys (Up/Down/Left/Right) are NOT bound to any `Action` in ONI's default bindings. The `KInputController` only generates `KButtonEvent` objects for keys that appear in `GameInputMapping.KeyBindings`. Arrow keys generate no events at all.
**Why it happens:** ONI uses WASD for panning (Action.PanUp/Down/Left/Right). Arrow keys are simply unregistered.
**How to avoid:** For arrow key detection, the mod must use either (a) raw `UnityEngine.Input.GetKeyDown` in a MonoBehaviour Update or (b) register custom bindings with the KInputController. Option (a) is simpler; option (b) integrates better. Since arrow keys are mod-only and need no consumption (game never sees them anyway), a hybrid approach works: use `Input.GetKeyDown` for arrow detection within the handler, called from the ModInputRouter.
**Warning signs:** If handlers expect to receive KButtonEvents for arrow keys, they will never fire.

### Pitfall 2: F12 Key Conflict with Debug Actions
**What goes wrong:** F12 with Ctrl is `Action.DebugTriggerException`, F12 with Ctrl+Shift is `Action.DebugTriggerError`, F12 with Shift is `Action.DebugTileTest`. If debug mode is enabled, these fire in DebugHandler.
**Why it happens:** These bindings exist in `Global.GenerateDefaultBindings()` under the "Debug" group.
**How to avoid:** Our handler is priority 50 (above DebugHandler at -1), so we consume first. But plain F12 (no modifiers) has NO game binding at all -- no KButtonEvent is generated for it. For F12 without modifiers, we must use `Input.GetKeyDown(KeyCode.F12)` detection (same as Phase 1) since the game never creates an event for unbound keys.
**Warning signs:** F12 help never fires if you only look for KButtonEvents.

### Pitfall 3: KScreen.Activate Called Before Game.Instance Exists
**What goes wrong:** KScreen.Activate is called for main menu screens before `Game.Instance` is set. Context detection that checks `Game.Instance` will throw NullReferenceException.
**Why it happens:** The main menu is a KScreen (MainMenu) that activates during initial load, long before any game save is loaded.
**How to avoid:** Context detection must handle null `Game.Instance` gracefully. Main menu should map to a "main menu" handler or a generic "no-game" handler.
**Warning signs:** NullReferenceException in `ContextDetector.OnScreenActivated` during startup.

### Pitfall 4: VanillaMode Toggle Must Not Leave Stale Handler State
**What goes wrong:** When mod is toggled off, handlers remain "active" internally, possibly holding state. When toggled back on, stale state causes incorrect behavior.
**Why it happens:** Toggle off just stops processing events, doesn't clean up handler state.
**How to avoid:** Toggle OFF must explicitly call `OnDeactivate()` on the current handler and clear the handler stack. Toggle ON must detect current game state fresh and activate the appropriate handler.
**Warning signs:** After toggle off/on cycle, wrong handler is active or stale announcements fire.

### Pitfall 5: ONI's Action Enum Shadows System.Action
**What goes wrong:** `Action` resolves to ONI's game action enum, not `System.Action`. Delegate types silently mismatch.
**Why it happens:** ONI defines `Action` enum in global namespace (Assembly-CSharp-firstpass/Action.cs).
**How to avoid:** Always use `System.Action` for delegates. This is a known constraint from Phase 1 verify -- already documented in prior decisions.
**Warning signs:** Compiler errors about "cannot convert Action to delegate" or wrong overload resolution.

### Pitfall 6: Consuming Events in Full-Capture Mode Breaks Essential Game Input
**What goes wrong:** Setting `e.Consumed = true` for ALL events prevents Escape from closing the screen/game, prevents mouse clicks, prevents everything.
**Why it happens:** Full capture means ALL keyboard input is blocked.
**How to avoid:** Full capture should only apply to keyboard events, not mouse events. Check `e.GetAction()` -- mouse actions are `Action.MouseLeft`, `Action.MouseRight`, `Action.MouseMiddle`. Let those pass through. Also, the mod's own Escape handling must be implemented (to close the current menu handler, etc.) before consuming the Escape action.
**Warning signs:** Player cannot click anything, game appears frozen.

### Pitfall 7: KInputHandler Priority Sorting Is Descending
**What goes wrong:** Adding handler with priority 0 thinking it runs first. It actually runs last (after priority 20, 10, 1).
**Why it happens:** `mChildren.Sort((a, b) => b.priority.CompareTo(a.priority))` sorts highest-first.
**How to avoid:** Use priority > 20 (e.g., 50) to ensure the mod handler runs before PlayerController.
**Warning signs:** Mod handler never sees events because PlayerController consumed them first.

## Code Examples

### Example 1: Complete IInputHandler Implementation
Source: Pattern derived from `PlayerController` and `DebugHandler` in ONI-Decompiled

```csharp
using System.Collections.Generic;

namespace OniAccess.Input
{
    public class ModInputRouter : IInputHandler
    {
        public static ModInputRouter Instance { get; private set; }

        public string handlerName => "OniAccess";
        public KInputHandler inputHandler { get; set; }

        public ModInputRouter()
        {
            Instance = this;
        }

        public void OnKeyDown(KButtonEvent e)
        {
            if (e.Consumed) return;

            // Toggle key: always check, even when mod is off
            // Ctrl+Shift+F12 maps to Action.DebugTriggerError (Modifier 6)
            // But safer to check raw Input for this one key
            if (CheckToggle()) return;

            // When mod is off, don't process anything else
            if (!VanillaMode.IsEnabled) return;

            // Delegate to active handler
            var handler = HandlerStack.ActiveHandler;
            if (handler == null) return;

            if (handler.CapturesAllInput)
            {
                // Full capture: handler processes, then consume regardless
                handler.HandleKeyDown(e);
                if (!e.Consumed)
                {
                    // Don't consume mouse actions in full capture
                    var action = e.GetAction();
                    if (action != Action.MouseLeft && action != Action.MouseRight
                        && action != Action.MouseMiddle && action != Action.ShiftMouseLeft
                        && action != Action.ZoomIn && action != Action.ZoomOut)
                    {
                        e.Consumed = true;
                    }
                }
            }
            else
            {
                // Selective claim: handler decides what to consume
                handler.HandleKeyDown(e);
            }
        }

        public void OnKeyUp(KButtonEvent e)
        {
            if (e.Consumed || !VanillaMode.IsEnabled) return;

            var handler = HandlerStack.ActiveHandler;
            if (handler == null) return;

            handler.HandleKeyUp(e);

            if (handler.CapturesAllInput && !e.Consumed)
            {
                var action = e.GetAction();
                if (action != Action.MouseLeft && action != Action.MouseRight
                    && action != Action.MouseMiddle && action != Action.ShiftMouseLeft)
                {
                    e.Consumed = true;
                }
            }
        }
    }
}
```

### Example 2: Harmony Patch for Handler Registration
Source: Pattern derived from `InputInit.Awake` (ONI-Decompiled/Assembly-CSharp/InputInit.cs)

```csharp
[HarmonyPatch(typeof(InputInit), "Awake")]
static class InputInit_Awake_Patch
{
    static void Postfix()
    {
        var inputManager = Global.GetInputManager();
        var router = new ModInputRouter();

        if (KInputManager.currentController != null)
        {
            KInputHandler.Add(KInputManager.currentController, router, 50);
        }
        else
        {
            KInputHandler.Add(inputManager.GetDefaultController(), router, 50);
        }

        inputManager.usedMenus.Add(router);
    }
}
```

### Example 3: Context Detection via KScreen Lifecycle
Source: Pattern derived from KScreen.Activate/Deactivate (ONI-Decompiled/Assembly-CSharp-firstpass/KScreen.cs)

```csharp
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
static class KScreen_Activate_Patch
{
    static void Postfix(KScreen __instance)
    {
        if (!VanillaMode.IsEnabled) return;
        ContextDetector.OnScreenActivated(__instance);
    }
}

[HarmonyPatch(typeof(KScreen), "Deactivate")]
static class KScreen_Deactivate_Patch
{
    // Prefix because Deactivate() calls PopScreen then Destroy
    static void Prefix(KScreen __instance)
    {
        if (!VanillaMode.IsEnabled) return;
        ContextDetector.OnScreenDeactivating(__instance);
    }
}
```

### Example 4: Handler with Selective Key Claim
Source: Pattern derived from how ToolMenu.OnKeyDown and ManagementMenu.OnKeyDown consume specific actions

```csharp
public class WorldHandler : IAccessHandler
{
    public string DisplayName => "World view";
    public bool CapturesAllInput => false;

    // Phase 2 only needs F12 for help -- Phase 4 adds arrow keys etc.
    public IReadOnlyList<UnityEngine.KeyCode> ClaimedKeys =>
        new[] { UnityEngine.KeyCode.F12 };

    public IReadOnlyList<HelpEntry> HelpEntries => new[]
    {
        new HelpEntry("F12", "Show help"),
        new HelpEntry("Ctrl+Shift+F12", "Toggle Oni-Access"),
    };

    public bool HandleKeyDown(KButtonEvent e)
    {
        // F12 for help -- but F12 has no Action, so we can't use TryConsume
        // This must be detected via raw Input check in ModInputRouter
        return false;
    }

    public bool HandleKeyUp(KButtonEvent e) => false;
    public void OnActivate() { }
    public void OnDeactivate() { }
}
```

### Example 5: VanillaMode Full Disable
Source: Redesign of existing VanillaMode.cs in OniAccess/Toggle/

```csharp
public static class VanillaMode
{
    public static bool IsEnabled { get; private set; } = true;

    public static void Toggle()
    {
        if (IsEnabled)
        {
            // Turning OFF
            // 1. Speak confirmation while pipeline is still active
            SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_OFF);
            // 2. Deactivate all handlers
            HandlerStack.DeactivateAll();
            // 3. Disable speech pipeline
            SpeechPipeline.SetEnabled(false);
            // 4. Set flag last
            IsEnabled = false;
        }
        else
        {
            // Turning ON
            IsEnabled = true;
            // 1. Enable speech pipeline
            SpeechPipeline.SetEnabled(true);
            // 2. Speak confirmation
            SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SPEECH.MOD_ON);
            // 3. Detect current game state and activate appropriate handler
            ContextDetector.DetectAndActivate();
        }
    }
}
```

## State of the Art

| Old Approach (Phase 1) | New Approach (Phase 2) | Why Change | Impact |
|---|---|---|---|
| MonoBehaviour Update() + Input.GetKeyDown | IInputHandler in KInputHandler tree | Can't consume events with MonoBehaviour -- keys still reach game | Keys claimed by mod are invisible to game |
| Flat HotkeyRegistry with static binding list | IAccessHandler stack with per-handler key declarations | Can't do context-sensitive keys in flat registry | Same key does different things in different states |
| AccessContext enum in TryHandle() | Handler activation/deactivation via screen lifecycle | Enum doesn't scale, requires central knowledge | Each handler is self-contained, pluggable |
| VanillaMode only disables speech | VanillaMode deactivates entire handler stack | Keys still intercepted even when "off" | True passthrough when mod is off |
| F12 speaks all help text at once | F12 activates HelpHandler with navigable list | Speech dump is not navigable | Arrow keys step through entries one at a time |

**Deprecated/outdated from Phase 1:**
- `InputInterceptor` MonoBehaviour: Replaced by ModInputRouter IInputHandler
- `HotkeyRegistry` static registry: Replaced by IAccessHandler per-handler key declarations
- `AccessContext` enum: Replaced by handler stack (active handler IS the context)
- `HotkeyBinding` class: Replaced by HelpEntry in IAccessHandler (simpler, handler-owned)
- `HotkeyModifier` flags enum: No longer needed -- modifiers handled by game's Action mapping

## Open Questions

1. **Arrow key event detection without KButtonEvent**
   - What we know: Arrow keys have no `Action` binding in ONI, so `KInputController` never generates events for them. Phase 4 needs arrows for world navigation, Phase 2 needs them for help list.
   - What's unclear: Best approach -- (a) check `UnityEngine.Input.GetKeyDown` from within the ModInputRouter (hybrid approach), (b) register custom `BindingEntry` for arrow keys with custom Actions, or (c) use a minimal MonoBehaviour alongside the handler system just for arrow polling.
   - Recommendation: Use approach (a) -- check `Input.GetKeyDown` for arrow keys from within the handler system. The ModInputRouter already runs every frame via `OnKeyDown`. For frames where no KButtonEvent is generated (no game-mapped key was pressed), the router can poll arrows separately. However, this won't work because `OnKeyDown` is only called when there IS a KButtonEvent. **Better: keep a minimal MonoBehaviour that polls arrow keys and forwards them to the active handler.** This MonoBehaviour only does arrow detection and nothing else -- the actual handling logic lives in the handler.

2. **F12 event detection (same issue as arrows)**
   - What we know: Plain F12 (no modifiers) is not bound to any `Action` in ONI's default bindings (only F12+Shift, F12+Ctrl, F12+Ctrl+Shift are bound to debug actions). So no `KButtonEvent` is generated for unmodified F12.
   - What's unclear: Same approaches as arrow keys.
   - Recommendation: Same solution -- the minimal MonoBehaviour polls F12 and arrow keys specifically. This is a focused, tiny piece that doesn't try to be a general input system. It just bridges keys that ONI doesn't recognize into the handler system.

3. **Timing of InputInit.Awake relative to mod load**
   - What we know: `Mod.OnLoad(Harmony)` runs during mod loading. `InputInit.Awake` runs when the game scene initializes (after mods load). The Harmony patch will be registered during OnLoad but fires later when InputInit.Awake actually runs.
   - What's unclear: Whether InputInit.Awake fires once (at game start) or on every scene load. If it fires again after returning to main menu and starting a new game, we need to handle re-registration.
   - Recommendation: Make the Harmony postfix idempotent -- check if router is already registered before adding again. Track registration state in ModInputRouter.

4. **ManagementMenu screen detection vs Show/Hide pattern**
   - What we know: ManagementMenu doesn't use KScreen.Activate/Deactivate for its child screens (Research, Skills, etc.). Instead it calls `screen.Show(true/false)` to toggle visibility. The screens are pre-activated at startup.
   - What's unclear: Whether patching Show() is sufficient for detecting which management screen is visible, or if we need to also track ManagementMenu.activeScreen.
   - Recommendation: For Phase 2, detecting "a management screen is open" via the ManagementMenu instance (`ManagementMenu.Instance != null && ManagementMenu.Instance.activeScreen != null` via Harmony accessor) is sufficient. Phase 3 will need per-screen detection. The context detector can check ManagementMenu state in addition to KScreen lifecycle.

## Sources

### Primary (HIGH confidence)
- `ONI-Decompiled/Assembly-CSharp-firstpass/KInputHandler.cs` -- Input handler tree, priority dispatch, event consumption
- `ONI-Decompiled/Assembly-CSharp-firstpass/KInputController.cs` -- Key polling, modifier tracking, event generation, action flag table
- `ONI-Decompiled/Assembly-CSharp-firstpass/KInputManager.cs` -- Controller management, dispatch loop
- `ONI-Decompiled/Assembly-CSharp-firstpass/KButtonEvent.cs` -- TryConsume pattern, Action matching
- `ONI-Decompiled/Assembly-CSharp-firstpass/KInputEvent.cs` -- Base event with Consumed property
- `ONI-Decompiled/Assembly-CSharp-firstpass/IInputHandler.cs` -- Interface (handlerName, inputHandler)
- `ONI-Decompiled/Assembly-CSharp-firstpass/KScreen.cs` -- Activate/Deactivate lifecycle, OnKeyDown virtual
- `ONI-Decompiled/Assembly-CSharp-firstpass/KScreenManager.cs` -- Screen stack, top-down dispatch, PushScreen/PopScreen
- `ONI-Decompiled/Assembly-CSharp/InputInit.cs` -- Handler registration (KScreenManager at 10, DebugHandler at -1)
- `ONI-Decompiled/Assembly-CSharp/Game.cs` -- CameraController at 1, PlayerController at 20
- `ONI-Decompiled/Assembly-CSharp/PlayerController.cs` -- IInputHandler implementation example
- `ONI-Decompiled/Assembly-CSharp/ManagementMenu.cs` -- Screen toggling, activeScreen tracking
- `ONI-Decompiled/Assembly-CSharp/KModalScreen.cs` -- Modal sort key 100, consumes all input
- `ONI-Decompiled/Assembly-CSharp/Global.cs` -- Default key bindings, F12 debug bindings
- `ONI-Decompiled/Assembly-CSharp-firstpass/GameInputManager.cs` -- AddKeyboardMouseController, binding setup
- `ONI-Decompiled/Assembly-CSharp-firstpass/Action.cs` -- Full Action enum (283 values)
- `ONI-Decompiled/Assembly-CSharp-firstpass/Modifier.cs` -- ONI modifier flags (None/Alt/Ctrl/Shift/CapsLock/Backtick)
- `OniAccess/Input/InputInterceptor.cs` -- Current Phase 1 input approach
- `OniAccess/Input/HotkeyRegistry.cs` -- Current Phase 1 hotkey system
- `OniAccess/Toggle/VanillaMode.cs` -- Current Phase 1 toggle implementation
- `OniAccess/Mod.cs` -- Current mod entry point and initialization

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- Directly read from decompiled game source code; every class and method verified
- Architecture: HIGH -- Architecture patterns derived directly from how ONI itself implements input handling (InputInit, Game, PlayerController all use the same patterns)
- Pitfalls: HIGH -- Every pitfall identified by reading actual game code (arrow key absence verified by examining Global.GenerateDefaultBindings, F12 conflict verified by examining DebugHandler, priority ordering verified by reading KInputHandler sort)

**Research date:** 2026-02-11
**Valid until:** Indefinite (ONI's input system is stable; game version doesn't change these core classes)
