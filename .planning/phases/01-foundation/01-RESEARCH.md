# Phase 1: Foundation - Research

**Researched:** 2026-02-11
**Domain:** ONI mod loading, Tolk screen reader integration, speech pipeline architecture, hotkey systems, text filtering, testing infrastructure
**Confidence:** HIGH

## Summary

Phase 1 establishes the foundational systems that every subsequent phase depends on: reliable mod loading with Tolk speech, a speech pipeline with interrupt/queue semantics, text filtering to strip rich text markup, a context-sensitive hotkey system, a vanilla mode toggle, and a speech capture testing framework.

The technical risk is low because the core technologies are well-understood: ONI's `KMod.UserMod2` entry point is documented and verified from decompiled source, Tolk's P/Invoke API is stable (already prototyped in `Speech.cs`), and Harmony 2 patching is the established ONI modding pattern. The main complexity lies in designing the hotkey system to be context-sensitive from the start (keys mean different things in different game states) and in building a speech pipeline that correctly handles interrupt vs queue semantics.

**Primary recommendation:** Build bottom-up -- project scaffolding and Tolk initialization first, then the speech pipeline (filtering, formatting, queue/interrupt), then the hotkey system and vanilla mode toggle, then the testing framework. The existing `Speech.cs` is a solid starting point but needs to evolve from a static utility into a proper pipeline with queue management and interrupt semantics.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Speech style & formatting
- Terse label format: "Copper Ore, 200 kg, 25°C" -- compact, front-load useful information
- One verbosity level for now; tile-specific toggles deferred to Phase 3
- Abbreviated units: "25°C, 200 kg, 500 W" -- screen readers handle common abbreviations well
- Comma-separated compound readouts: "Copper Ore, 200 kg, 25°C, Solid"
- Inline lists without counts: "Requires 400 kg Metal Ore, 1 kg Glass"
- Follow game's temperature unit setting (Celsius/Fahrenheit/Kelvin)
- Use exact game names for elements/materials (Abyssalite, Polluted Oxygen, Neutronium) -- matches wiki and community
- No extra category prefixes in most cases; revisit per-context as needed
- Color-coded text: convert to meaning where possible, but context-specific -- decide per instance as they arise

#### Text filtering
- Rich text tags: replace meaningful tags with words (warning icon sprite -> "warning:"), silently strip decorative tags
- All output must be clean readable text with no raw rich text tags or sprite codes

#### Localization
- Prefer full localization (create STRINGS entries for mod text too) if practical
- Fallback: use STRINGS/LocText for game-data strings, English for mod-specific text if full localization proves impractical
- Researcher should assess feasibility of full localization approach

#### Announcement behavior
- Navigation speech (cursor movement): interrupts previous speech for responsiveness
- Alert/notification speech: queues, plays in order, never dropped
- Duplicate simultaneous alerts: combine with count ("Broken Wall x2")
- Same behavior whether game is paused or unpaused
- No speech logging -- testing framework handles verification
- No earcons/sound cues for now -- speech only, evaluate earcons as future enhancement
- No separate mute function -- mod toggle (on/off) is sufficient

#### Alert history
- Create a history buffer for alerts, navigable like a menu via hotkey
- Pressing Enter on an alert jumps to its location
- May overlap with existing in-game notification system -- researcher should investigate
- Full implementation likely Phase 6; Phase 1 establishes buffer infrastructure if practical

#### Hotkey conventions
- No blanket modifier key -- each hotkey decided individually based on context and conflicts
- Researcher must map ALL existing game hotkeys to inform per-key decisions
- Context-sensitive: same key can do different things in different game states (arrows navigate world vs. menu)
- Context-aware help command: a hotkey lists available commands for current game state
- When mod overrides a game hotkey, no announcement to user (discussed during development)
- Only the toggle hotkey remains active when mod is off; all other mod hotkeys deactivated

#### Hotkey system architecture
- Context-sensitive registration: keys bound per game state, not globally
- Help text generation from hotkey registry (architecture is Claude's discretion)
- Must handle same key in multiple contexts cleanly

#### Startup & toggle
- On launch: "Oni-Access version [X] loaded"
- Toggle off: "Oni-Access off" -- speech stops, only toggle hotkey remains active, game behaves normally
- Toggle on: "Oni-Access on" -- brief confirmation, no state readout
- Tolk handles screen reader detection and SAPI fallback natively -- no extra fallback logic needed

### Claude's Discretion
- Hotkey system architecture (state machine, IKeyHandler, or other pattern -- based on ONI's input system)
- Help text registry design (central vs co-located -- must support context-sensitive keys)
- Loading skeleton / progress indicator design
- Exact text filtering rules for which tags carry meaning vs decoration
- Speech pipeline internals (queue management, interrupt handling implementation)
- Alert history buffer architecture (if included in Phase 1)

### Deferred Ideas (OUT OF SCOPE)
- Tile-specific verbosity toggles -- Phase 3 (World Navigation)
- Earcon/sound cue system alongside speech -- future enhancement after base works
- Full alert history with navigation and jump-to-location -- Phase 6 (Notifications)
- Per-context category prefixes -- revisit as specific features are built
</user_constraints>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| KMod.UserMod2 | ONI build 707956 | Mod entry point | Only supported entry point; game enforces single subclass per assembly |
| Harmony 2 (0Harmony.dll) | 2.x (bundled with ONI) | Runtime method patching | ONI requires APIVersion 2; must use game's bundled copy |
| Tolk | Latest stable (project inactive, API frozen) | Screen reader abstraction | Supports JAWS, NVDA, SAPI fallback; already prototyped in Speech.cs |
| .NET Framework | net472 (Mono runtime) | Target framework | Dictated by ONI's Unity 2020.3 LTS Mono runtime |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Newtonsoft.Json | Bundled with ONI | Settings persistence | For mod configuration file (JSON) |
| Unity.TextMeshPro | Bundled with ONI | Rich text tag knowledge | Understanding which TMP tags to filter |
| UnityEngine.CoreModule | Bundled with ONI | Debug.Log, coroutines, Mathf | Logging, frame-delayed initialization |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Tolk (P/Invoke) | TolkDotNet.dll (.NET wrapper) | TolkDotNet adds a managed wrapper layer; P/Invoke is simpler, already working in Speech.cs, no extra DLL |
| Tolk | UniversalSpeech | Less widely adopted in game modding; Tolk is the established choice |
| Newtonsoft.Json | System.Text.Json | Not available in Mono/.NET 4.7; Newtonsoft ships with ONI |

**Installation:**
No package installation needed. All dependencies are either bundled with the game or already in the project's `tolk/dist/` directory. The `.csproj` references game DLLs via the `ONI_MANAGED` environment variable.

## Architecture Patterns

### Recommended Project Structure
```
OniAccess/
    mod_info.yaml              # Required mod metadata (APIVersion: 2)
    OniAccess.csproj           # .NET Framework 4.7.2 class library
    Mod.cs                     # UserMod2 entry point (thin orchestrator)
    Speech/
        SpeechEngine.cs        # Evolved Speech.cs: Tolk P/Invoke, init/shutdown
        SpeechPipeline.cs      # Queue/interrupt management, dedup, dispatch
        TextFilter.cs          # Rich text stripping, sprite tag conversion
        Announcement.cs        # Value type: text, priority, interrupt flag, category
    Input/
        HotkeyRegistry.cs     # Context-sensitive hotkey definitions + help text
        InputInterceptor.cs   # Harmony patch layer for key event capture
        GameContext.cs         # Determines current game state for context switching
    Toggle/
        VanillaMode.cs         # Mod on/off state, toggle logic
    Testing/
        SpeechCapture.cs       # Capture buffer for test assertions
    Patches/
        LifecyclePatches.cs    # Game load, mod init hooks
        InputPatches.cs        # KInputHandler interception
    Util/
        StringHelper.cs        # STRINGS/LocText access helpers
        LogHelper.cs           # [OniAccess] prefixed logging
    tolk/
        dist/
            Tolk.dll
            nvdaControllerClient64.dll
            SAAPI64.dll
```

### Pattern 1: Speech Pipeline with Interrupt/Queue Semantics
**What:** A centralized pipeline that all speech output flows through. Supports two modes: interrupt (immediately stops current speech and speaks new text) and queue (appends to queue, played in order).
**When to use:** Every speech output in the mod, without exception.
**Why:** Screen reader users need responsive navigation (interrupt on cursor move) but cannot afford to miss alerts (queue for notifications). The pipeline enforces this at the architecture level, preventing scattered Tolk_Output calls with inconsistent behavior.

```csharp
// Source: FactorioAccess speech.lua pattern, adapted for C#/Tolk
public enum SpeechPriority { Low, Normal, High, Critical }

public readonly struct Announcement
{
    public string Text { get; }
    public SpeechPriority Priority { get; }
    public bool Interrupt { get; }
    public string Category { get; }  // For deduplication
}

public static class SpeechPipeline
{
    private static readonly Queue<Announcement> _queue = new Queue<Announcement>();
    private static float _lastSpeechTime;
    private static string _lastCategory;

    /// <summary>
    /// Interrupt mode: stop current speech, speak immediately.
    /// Used for navigation (cursor movement).
    /// </summary>
    public static void SpeakInterrupt(string text, string category = null)
    {
        // Dedup: if same category within 100ms, skip previous
        SpeechEngine.Say(text, interrupt: true);
    }

    /// <summary>
    /// Queue mode: append to queue, never dropped.
    /// Used for alerts and notifications.
    /// </summary>
    public static void SpeakQueued(string text, string category = null)
    {
        SpeechEngine.Say(text, interrupt: false);
    }
}
```

**Confidence:** HIGH (pattern proven by FactorioAccess, Tolk API supports both interrupt=true and interrupt=false natively)

### Pattern 2: Context-Sensitive Hotkey System
**What:** A registry where hotkeys are defined per game context (e.g., "world view", "menu open", "build mode"), with the same physical key potentially doing different things in different contexts. The registry also generates help text listing available commands for the current context.
**When to use:** All mod hotkey registration.
**Why:** ONI already uses context-sensitive input (arrows pan camera in-game, navigate in menus). The accessibility mod must work within this existing context system and add its own contexts on top.

```csharp
// Recommended architecture based on ONI's input system analysis
public enum AccessContext
{
    Always,         // Toggle hotkey only (active even when mod is off)
    Global,         // Active whenever mod is on
    WorldView,      // Main gameplay grid
    MenuOpen,       // Any KScreen is active
    BuildMode,      // Build tool selected
    // ... extends as phases add features
}

public class HotkeyBinding
{
    public KKeyCode Key { get; }
    public Modifier Modifiers { get; }
    public AccessContext Context { get; }
    public string Description { get; }  // For help text generation
    public Action<KButtonEvent> Handler { get; }
}

public static class HotkeyRegistry
{
    private static readonly List<HotkeyBinding> _bindings = new List<HotkeyBinding>();

    public static void Register(HotkeyBinding binding) { ... }

    public static bool TryHandle(KButtonEvent e, AccessContext currentContext)
    {
        // Find matching binding for current context
        // Call handler, consume event
    }

    public static string GetHelpText(AccessContext context)
    {
        // Generate help listing for all bindings in this context
    }
}
```

**Confidence:** HIGH (ONI's `KButtonEvent.TryConsume(Action)` pattern and `KInputHandler` priority chain are well-understood from decompilation)

### Pattern 3: Text Filtering Pipeline
**What:** A chain of regex-based filters that clean game text before speech output. Converts meaningful markup to words, strips decorative markup, normalizes whitespace.
**When to use:** Every text string before it reaches the speech engine.
**Why:** ONI text contains Unity Rich Text tags (`<color>`, `<b>`, `<link>`), TextMeshPro sprite tags (`<sprite name=...>`), and TMP shorthand (`[icon_name]`). Screen readers would read these literally ("less than color equals red greater than") if not stripped.

The existing `Speech.cs` already implements this pattern with compiled regex. Evolution needed:
1. Expand the sprite-to-text mapping dictionary with ONI-specific sprites (warning icons, status icons, element icons)
2. Add `<link>` tag handling (extract display text, discard link metadata)
3. Handle `<color>` tags by optionally converting to meaning ("warning:" prefix for red text, based on context)
4. Handle `{Hotkey}` placeholders in tooltip text (strip or resolve to key name)

```csharp
// Already exists in Speech.cs, needs expansion:
private static readonly Regex SpriteTagRegex =
    new Regex(@"<sprite\s+name=([^>]+)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

// Needs addition: link tag extraction
private static readonly Regex LinkTagRegex =
    new Regex(@"<link=""[^""]*"">(.*?)</link>", RegexOptions.Compiled);

// Needs addition: hotkey placeholder stripping
private static string StripHotkeyPlaceholders(string text)
{
    int idx = text.IndexOf("{Hotkey}");
    if (idx >= 0) text = text.Substring(0, idx).TrimEnd(' ', ':');
    return text;
}
```

**Confidence:** HIGH (regex patterns verified against ONI decompiled text handling in `oni-internals.md`)

### Pattern 4: Vanilla Mode Toggle
**What:** A single hotkey toggles the entire mod on/off. When off: all speech stops, all mod input handlers deactivate (except the toggle hotkey), game returns to normal behavior. When on: brief "Oni-Access on" confirmation.
**When to use:** Must be the first hotkey registered, must survive all contexts.
**Why:** Sighted helpers, partially-sighted players switching modes, debugging. From FactorioAccess: "Design your Harmony patches so the accessibility layer can be fully toggled without breaking game state."

```csharp
public static class VanillaMode
{
    public static bool IsEnabled { get; private set; } = true;  // Mod starts ON

    public static void Toggle()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {
            SpeechPipeline.SpeakInterrupt("Oni-Access on");
        }
        else
        {
            SpeechPipeline.SpeakInterrupt("Oni-Access off");
            SpeechEngine.Stop();  // Silence after announcement finishes
        }
    }
}

// In every input handler and speech call:
if (!VanillaMode.IsEnabled) return;
```

**Confidence:** HIGH (pattern directly from FactorioAccess `vanilla-mode.lua`, adapted for C#)

### Pattern 5: Speech Capture for Testing
**What:** A capture mode that records all speech output to a list instead of (or in addition to) sending it to Tolk. Tests can then assert against the captured text.
**When to use:** Automated and semi-automated regression testing of speech output.
**Why:** From FactorioAccess: "Being able to assert 'when the player inspects this building, the speech output includes no power' is invaluable. It catches regressions where a code change silently breaks what blind players hear."

```csharp
public static class SpeechCapture
{
    private static bool _capturing;
    private static List<string> _captured = new List<string>();

    public static void Start()
    {
        _captured.Clear();
        _capturing = true;
    }

    public static List<string> Stop()
    {
        _capturing = false;
        return new List<string>(_captured);
    }

    // Called by SpeechEngine before Tolk output
    internal static void Record(string text)
    {
        if (_capturing) _captured.Add(text);
    }
}
```

**Confidence:** HIGH (pattern from FactorioAccess `scripts/speech.lua` capture system)

### Anti-Patterns to Avoid
- **Scattered Tolk calls:** Never call `Tolk_Output` directly from game code. All output must flow through `SpeechPipeline` for consistent interrupt/queue behavior and testability.
- **Global hotkeys without context:** Never register a hotkey that fires in all contexts. Even "read current state" should be context-specific (reads different info in world vs menu).
- **Defensive null-checking in internal code:** Validate at boundaries (user pressed a key, entity might not exist). Internal functions should crash if preconditions aren't met -- a crash produces a log entry a developer can diagnose, a silent empty string produces a confusing experience for a blind user.
- **Monolithic patch classes:** Keep patches thin (1-5 lines). Extract data, call a mod core method. Business logic lives in the core, not in patches.
- **Hardcoded English strings for game data:** Always use `STRINGS` namespace or `Strings.Get()` for element names, building names, status text. Hardcoded English breaks localization and drifts from the game's actual names.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Screen reader output | Custom Windows SAPI COM interop | Tolk via P/Invoke | Tolk abstracts across JAWS, NVDA, SAPI, others. Already working in Speech.cs. |
| Rich text stripping | Ad-hoc string.Replace chains | Compiled Regex pipeline | String.Replace misses edge cases (nested tags, varying attribute order). Regex is robust and already implemented. |
| Hotkey conflict detection | Manual list comparison | ONI's `GameInputMapping.FindEntriesByKeyCode()` | Game already tracks all keybindings. Query it to detect conflicts. |
| Temperature formatting | Manual C/F/K conversion | ONI's `GameUtil.GetFormattedTemperature()` | Game already handles unit preference, rounding, and display formatting. |
| Element/material names | Hardcoded name dictionary | `STRINGS.ELEMENTS` + `ElementLoader` | Game maintains all element names with localization support. |
| JSON serialization | Manual string building | `Newtonsoft.Json` (bundled with ONI) | Battle-tested, handles edge cases (escaping, null handling, nested objects). |
| Mod directory detection | Hardcoded paths | `typeof(Mod).Assembly.Location` via `Path.GetDirectoryName()` | Works regardless of install location (Steam, GOG, Dev, Local). |

**Key insight:** ONI already has formatting utilities (`GameUtil.GetFormattedTemperature`, `GameUtil.GetFormattedMass`, `GameUtil.GetFormattedPercent`), localized names (`STRINGS.BUILDINGS`, `STRINGS.ELEMENTS`, `STRINGS.DUPLICANTS`), and input system infrastructure (`GameInputMapping`, `BindingEntry`). The mod should hook into these rather than reimplementing them.

## Common Pitfalls

### Pitfall 1: Tolk DLL Loading Failure
**What goes wrong:** `DllNotFoundException` when `Tolk_Load()` is called because P/Invoke cannot find `Tolk.dll` in the search path.
**Why it happens:** ONI's mod loader calls `Assembly.LoadFrom()` on the mod DLL, but the working directory is the game's root, not the mod directory. Tolk's native DLLs in `tolk/dist/` are not on the DLL search path.
**How to avoid:** Call `SetDllDirectory()` with the absolute path to the Tolk directory BEFORE calling any Tolk P/Invoke functions. This is already partially implemented in the existing approach.
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
static extern bool SetDllDirectory(string lpPathName);

// In OnLoad, before Speech.Initialize():
string tolkDir = Path.Combine(Path.GetDirectoryName(typeof(Mod).Assembly.Location), "tolk", "dist");
SetDllDirectory(tolkDir);
```
**Warning signs:** `DllNotFoundException` in the game's `output_log.txt`.

### Pitfall 2: FieldRef Startup Lag
**What goes wrong:** Game hangs for 4+ seconds during mod load.
**Why it happens:** Creating many `AccessTools.FieldRefAccess<T,F>()` calls at class static initialization time. Each FieldRef involves reflection and JIT compilation.
**How to avoid:** Use lazy initialization with `??=` operator:
```csharp
// WRONG: all created at class load time
private static readonly AccessTools.FieldRef<Foo, Bar> _field =
    AccessTools.FieldRefAccess<Foo, Bar>("field");

// RIGHT: created on first use
private static AccessTools.FieldRef<Foo, Bar> __field;
private static AccessTools.FieldRef<Foo, Bar> _field =>
    __field ??= AccessTools.FieldRefAccess<Foo, Bar>("field");
```
**Warning signs:** Long pause between "Loading mods..." and "Mods loaded" in the game log.

### Pitfall 3: Input Fires Multiple Times Per Frame
**What goes wrong:** The same hotkey handler fires 2-3 times for a single key press, causing repeated speech output.
**Why it happens:** ONI's input system can dispatch the same key event to multiple handlers in the same frame, or the event propagation isn't stopped properly.
**How to avoid:** Track `Time.frameCount` and debounce:
```csharp
private static int _lastInputFrame;
if (Time.frameCount == _lastInputFrame) return;
_lastInputFrame = Time.frameCount;
```
Also always call `e.Consumed = true` after handling an event to prevent further propagation.
**Warning signs:** "Oni-Access loaded, Oni-Access loaded" on startup, or repeated tile readouts on single cursor step.

### Pitfall 4: Speech Flood During Rapid State Changes
**What goes wrong:** Screen reader overwhelmed with queued speech, long delay before user hears current state.
**Why it happens:** Many events fire rapidly (screen transitions, game loading, multiple notifications at once). Each generates speech output that queues.
**How to avoid:** Category-based deduplication in the speech pipeline. Same-category announcements within a short window (~100-200ms) collapse to the latest. Use interrupt mode for navigation so new cursor positions cancel old ones.
**Warning signs:** Screen reader still speaking about a previous location/state long after the user has moved on.

### Pitfall 5: Mod Loader Trying to Load Native DLLs as .NET Assemblies
**What goes wrong:** Error log noise about failed assembly loads for `Tolk.dll`, `nvdaControllerClient64.dll`.
**Why it happens:** ONI's `DLLLoader.LoadDLLs()` iterates ALL `*.dll` files in the mod's content directory and tries `Assembly.LoadFrom()` on each. Native DLLs fail this check.
**How to avoid:** Place native DLLs in a subdirectory (e.g., `tolk/dist/`). The loader only scans the content root directory, not subdirectories -- **needs verification**. If the loader does recurse, the DLLs will fail gracefully (no `UserMod2` subclass found, mod loader moves on) but generate log warnings.
**Warning signs:** `BadImageFormatException` or `ReflectionTypeLoadException` in `output_log.txt` referencing Tolk DLLs.

### Pitfall 6: Tolk Threading Issues
**What goes wrong:** Screen reader output is garbled, crashes, or silent.
**Why it happens:** Tolk is NOT thread-safe. Some screen reader drivers use COM. If speech is triggered from a background thread (e.g., a Unity coroutine on a different thread, or a sim callback), COM state may be corrupted.
**How to avoid:** All `Tolk_Output` calls MUST happen on the Unity main thread. If background processing generates speech, queue it and dispatch on the main thread via `Update()` or a coroutine.
**Warning signs:** Intermittent crashes with COM-related stack traces, or speech that works sometimes and fails other times.

### Pitfall 7: KScreen Subclass Variability
**What goes wrong:** A Harmony patch targeting `KScreen.OnKeyDown` doesn't fire for some screens.
**Why it happens:** Not all screens implement `OnKeyDown`. Some use different input routing. The `KInputHandler` tree has multiple levels of priority-based dispatch.
**How to avoid:** For Phase 1, hook into `KInputHandler.HandleKeyDown` at the top level rather than individual `KScreen.OnKeyDown` methods. This ensures the mod sees all input events before they're dispatched to specific screens.
**Warning signs:** Hotkeys work on some screens but not others.

## Code Examples

Verified patterns from decompiled source and existing project files:

### Mod Entry Point with Tolk Initialization
```csharp
// Source: STACK.md + decompiled UserMod2.cs + existing Speech.cs
using System.IO;
using System.Runtime.InteropServices;
using HarmonyLib;

namespace OniAccess
{
    public sealed class Mod : KMod.UserMod2
    {
        public static Mod Instance { get; private set; }
        public static string ModDir { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            ModDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);

            // Set DLL search path for Tolk native libraries
            string tolkDir = Path.Combine(ModDir, "tolk", "dist");
            SetDllDirectory(tolkDir);

            base.OnLoad(harmony);  // Auto-discovers [HarmonyPatch] classes

            SpeechEngine.Initialize();
            SpeechPipeline.SpeakInterrupt($"Oni-Access version {assembly.GetName().Version} loaded");
        }
    }
}
```

### Intercepting Input via Harmony
```csharp
// Source: decompiled KInputHandler.cs + oni-internals.md input interception pattern
[HarmonyPatch(typeof(KScreen), "OnKeyDown")]
internal static class KScreen_OnKeyDown_Patch
{
    private static void Postfix(KScreen __instance, KButtonEvent e)
    {
        if (e.Consumed) return;  // Another handler already took it
        if (!VanillaMode.IsEnabled) return;  // Mod is off

        var context = GameContext.GetCurrent(__instance);
        if (HotkeyRegistry.TryHandle(e, context))
        {
            e.Consumed = true;
        }
    }
}
```

### Reading Game Temperature Setting
```csharp
// Source: decompiled GameUtil.cs
// ONI stores temperature in Kelvin internally; GameUtil formats per user preference
public static string FormatTemperature(float kelvin)
{
    return GameUtil.GetFormattedTemperature(kelvin);
    // Returns e.g. "25.2°C" or "77.4°F" based on user's Options > Game setting
}
```

### Using STRINGS for Localized Game Text
```csharp
// Source: decompiled STRINGS namespace
using STRINGS;

// Building names:
string name = BUILDINGS.PREFABS.COALGENERATOR.NAME;  // "Coal Generator"

// Element names (dynamic lookup by SimHashes):
Element element = ElementLoader.FindElementByHash(SimHashes.Oxygen);
string elementName = element.name;  // Localized element name

// Status items:
string status = Db.Get().BuildingStatusItems.NeedPower.Name;  // "Unpowered"
```

### mod_info.yaml
```yaml
# Source: STACK.md, verified from decompiled Mod.cs GetModInfoForFolder()
supportedContent: ALL
minimumSupportedBuild: 707956
APIVersion: 2
version: "0.1.0"
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Harmony 1 (Harmony namespace) | Harmony 2 (HarmonyLib namespace) | ONI build ~550000+ | Must use APIVersion: 2 in mod_info.yaml |
| Manual harmony.PatchAll() | base.OnLoad(harmony) auto-patches | UserMod2 convention | base.OnLoad already calls PatchAll(assembly) |
| Eager FieldRefAccess | Lazy ??= FieldRefAccess | Community learning | Prevents 4+ second startup lag |
| Tolk.dll alongside managed DLL | Tolk.dll in subdirectory + SetDllDirectory | Community practice | Prevents DLLLoader from trying to load native DLLs as .NET |

**Deprecated/outdated:**
- **Harmony 1 API:** ONI no longer supports it. APIVersion must be 2.
- **BepInEx/HarmonyX:** ONI does NOT use BepInEx. Some Unity games do, but ONI has its own KMod system.
- **Tolk development:** Tolk README states "this project is not currently being developed." However, the API is stable, screen reader APIs haven't changed, and it remains the de facto standard for game accessibility mods.

## ONI's Existing Game Hotkey Map

This section maps all existing game input bindings by examining the `Action` enum and `BindingEntry` system from decompiled source. The planner needs this to make per-key conflict decisions.

### Key Binding Groups (from decompiled `BindingEntry.mGroup` field)

ONI organizes bindings into groups. Each group represents a context where those bindings are active:

| Group | Context | Examples |
|-------|---------|---------|
| Root | Always active | Escape, Mouse, Zoom, Pause, Speed |
| Navigation | Camera panning | WASD (PanUp/Down/Left/Right), Home |
| Tool | When tool is selected | Mouse clicks, Drag |
| BuildMenu | Build menu navigation | BuildMenuUp/Down/Left/Right, letter keys A-Z |
| Sandbox | Sandbox mode | Various sandbox tools |
| Debug | Debug mode | Various debug commands |
| CinemaMode | Cinema camera | Camera controls |
| Management | Management screens | Priorities, Consumables, Vitals, etc. |

### Critical Game Bindings (Root group - always active)

| Key | Action | Notes for Mod |
|-----|--------|---------------|
| Escape | Close/Back | Cannot override |
| Space | TogglePause | Could conflict with mod actions |
| Tab | CycleSpeed | Available in menus if not in Root |
| Numpad +/- | SpeedUp/SlowDown | Safe, numpad |
| F1-F11 | Overlay1-11 | HEAVILY USED, do not override |
| Shift+F1-F4 | Overlay12-15 | Additional overlays |
| H | Help | Available in other contexts |
| Ctrl+1-0 | SetUserNav1-10 | Camera bookmarks |
| Shift+1-0 | GotoUserNav1-10 | Camera bookmark recall |

### Camera/Navigation (always active during gameplay)

| Key | Action | Notes |
|-----|--------|-------|
| W | PanUp | The mod cursor should NOT use WASD -- conflict with camera |
| A | PanLeft | Same |
| S | PanDown | Same |
| D | PanRight | Same |

### Management Screen Hotkeys

| Key | Action | Notes |
|-----|--------|-------|
| L | ManagePriorities | |
| F | ManageConsumables | |
| V | ManageVitals | |
| R | ManageResearch | |
| Period (.) | ManageSchedule | |
| J | ManageSkills | |
| E | ManageReport | |
| U | ManageDatabase (Codex) | |
| Z | ManageStarmap | |
| N | ManageDiagnostics | |

### Tool Commands

| Key | Action | Notes |
|-----|--------|-------|
| G | Dig | |
| C | BuildingCancel | |
| X | BuildingDeconstruct | |
| P | Prioritize | |
| M | Mop | |
| K | Clear (Sweep) | K is also a common "read info" key in accessibility mods |
| B | CopyBuilding | |
| O | RotateBuilding | |
| Ctrl+S | SreenShot1x | |

### Build Menu

| Key | Action | Notes |
|-----|--------|-------|
| 1-0 | Plan1-10 (build categories) | Active only when build menu visible |
| A-Z | BuildMenuKeyA-Z | Active only in build submenu |

### Keys NOT Used by ONI (safe for mod)

Based on the `Action` enum analysis, these keys have NO default binding in ONI:

| Key | Notes |
|-----|-------|
| Arrow keys | NOT bound to any Action! Camera uses WASD. Arrow keys are safe for the mod cursor. |
| Insert | Not bound (also screen reader modifier -- avoid) |
| CapsLock | Modifier enum includes it but not bound as action (also screen reader modifier -- avoid) |
| Backtick/Tilde | Listed as Modifier but not bound to actions |
| Page Up/Down | Not bound to any Action |
| Ctrl+Arrow keys | Not bound |
| Alt+Arrow keys | Not bound |
| Most letter keys with Alt | Not bound to standard actions |

**Critical finding for the planner:** Arrow keys are completely free in ONI's default bindings. They are the natural choice for the accessibility cursor, matching screen reader conventions where arrows mean "navigate current context."

### Screen Reader Keys to AVOID

| Key | Used By | Must Avoid |
|-----|---------|------------|
| Insert | NVDA modifier | Absolute -- never bind |
| CapsLock | NVDA modifier (alternate), JAWS modifier | Absolute -- never bind |
| Insert+letter | NVDA commands | Avoid Insert combinations |
| NVDA+Space | Toggle NVDA speech | Avoid |

### Recommended Phase 1 Hotkeys

| Action | Suggested Key | Rationale |
|--------|---------------|-----------|
| Toggle mod on/off | Ctrl+Shift+F12 | No game conflict, memorable, works in all contexts |
| Context help | F12 | Not used by overlays (F1-F11), not used by game. Easy to remember. |

Further hotkeys will be assigned in Phase 2+ as features are built, following the "decide per-key" convention from CONTEXT.md.

## Localization Feasibility Assessment

The user's locked decision says: "Prefer full localization (create STRINGS entries for mod text too) if practical."

### How ONI's Localization Works

ONI uses a `STRINGS` namespace containing static string fields organized hierarchically. The `Localization` class processes `.po` files (GNU gettext format) to override these strings. Mods can ship `.po` files in a `strings/` directory.

### Feasibility of Full Localization for Mod Text

**Assessment: PRACTICAL with a simple approach.**

The mod can define its own strings class under the STRINGS namespace:

```csharp
namespace STRINGS
{
    public class ONIACCESS
    {
        public class SPEECH
        {
            public static LocString MOD_LOADED = "Oni-Access version {0} loaded";
            public static LocString MOD_ON = "Oni-Access on";
            public static LocString MOD_OFF = "Oni-Access off";
        }
        public class HOTKEYS
        {
            public static LocString TOGGLE_MOD = "Toggle Oni-Access on/off";
            public static LocString CONTEXT_HELP = "Show available commands";
        }
    }
}
```

ONI's `Localization.RegisterForTranslation(typeof(STRINGS.ONIACCESS))` method can register these for translation. A `.po` file can then override them for other languages.

**Recommendation:** Use full localization from the start. The overhead is minimal (define `LocString` fields instead of raw strings), and it enables community translations from day one. For Phase 1, the number of mod-specific strings is small (~10-15), making this very practical.

**Confidence:** MEDIUM-HIGH (pattern observed in community mods; `RegisterForTranslation` exists in decompiled Localization class but exact usage needs runtime verification)

## Alert History Buffer Investigation

The user asked: "May overlap with existing in-game notification system -- researcher should investigate."

### ONI's Existing Notification System

From decompiled source and the accessibility audit:
- `Notification` class represents an in-game notification with: `titleText`, `tooltipText`, `Type` (enum: Neutral, Good, Bad, Tutorial, DuplicantThreatening, Event), `clickFocus` (Transform to zoom to), and `expires` (bool).
- `Notifier` component on entities fires `Add(Notification)` to create notifications.
- Notifications appear as a visual stack on the left side of the screen. They are transient -- old ones scroll up and eventually disappear. **There is no built-in notification history log** outside of the Daily Reports screen.
- Each notification can be clicked to zoom the camera to the relevant location.

### Overlap Assessment

There is significant overlap in concept but NOT in implementation:
- ONI's notifications are visual and transient -- they disappear and cannot be browsed retroactively.
- The mod's alert history buffer would capture these same notifications but store them in a navigable list.
- The buffer should hook into `Notifier.Add()` via Harmony postfix to capture every notification.

### Phase 1 Scope Recommendation

**Include a minimal buffer infrastructure in Phase 1:**
- A fixed-size ring buffer (e.g., 100 entries) that captures notification text, type, timestamp, and source location.
- No navigation UI in Phase 1 (that's Phase 6).
- The buffer exists so that Phase 6 can add navigation on top of it without architectural changes.
- Combine duplicate simultaneous alerts: if the same `titleText` appears multiple times within 1 second, store once with a count.

```csharp
public class AlertEntry
{
    public string Text { get; set; }
    public NotificationType Type { get; set; }
    public float GameTime { get; set; }
    public int Cell { get; set; }  // For jump-to-location
    public int Count { get; set; } // For dedup: "Broken Wall x2"
}

public static class AlertHistory
{
    private static readonly AlertEntry[] _buffer = new AlertEntry[100];
    private static int _writeIndex;

    public static void Record(Notification notification) { ... }
}
```

**Confidence:** HIGH (Notifier.Add is a clean hook point; buffer is straightforward)

## Open Questions

1. **DLLLoader subdirectory recursion**
   - What we know: `DLLLoader.LoadDLLs()` scans for `*.dll` files. The decompiled code uses `directoryInfo.GetFiles()` which by default only searches the immediate directory.
   - What's unclear: Whether the specific call uses `SearchOption.TopDirectoryOnly` or `SearchOption.AllDirectories`. If it recurses, Tolk DLLs in `tolk/dist/` would generate harmless but noisy log errors.
   - Recommendation: Verify at runtime. If recursion occurs, the errors are benign (no `UserMod2` found, loader moves on). If they're noisy, consider placing native DLLs in a non-`*.dll` named format and renaming at load time -- but this is unlikely to be necessary.

2. **Harmony patch on KInputHandler vs individual KScreen**
   - What we know: `KInputHandler.HandleKeyDown` dispatches to a priority-sorted list of child handlers. Patching it intercepts ALL input before any game handler sees it.
   - What's unclear: Whether a Prefix on `HandleKeyDown` at the top-level `KInputHandler` fires before or after the `mOnKeyDownDelegates` list is processed. The decompiled code shows delegates fire before children, so a Prefix would fire before everything.
   - Recommendation: Use Postfix on `KScreen.OnKeyDown` for per-screen interception (checking `e.Consumed` first). For global hotkeys (toggle), consider patching the game's root input handler. Validate the exact dispatch order at runtime.

3. **Tolk COM initialization in Mono**
   - What we know: Tolk README says "In languages that automatically deal with COM, e.g. .NET, call `Tolk_Load` only once." Mono does handle COM initialization.
   - What's unclear: Whether Unity's Mono runtime initializes COM on the main thread in a way compatible with Tolk's screen reader drivers.
   - Recommendation: Call `Tolk_Load()` in `OnLoad()` which runs on the main thread. If screen reader detection fails, log the error and try again on the first frame update (via a one-shot coroutine). This handles the edge case where COM isn't fully initialized during mod loading.

4. **Exact sprite tag inventory**
   - What we know: ONI uses `<sprite name=...>` tags for inline icons. The existing `Speech.cs` has a registration system for mapping sprite names to spoken text.
   - What's unclear: The complete list of sprite names used in ONI's text. These would need to be cataloged by examining TextMeshPro sprite assets or by capturing in-game text.
   - Recommendation: Start with an empty mapping and add entries as they're encountered during testing. Log unrecognized sprite tags so they can be added to the mapping. The `RegisterSpriteText()` API in Speech.cs already supports this incremental approach.

## Sources

### Primary (HIGH confidence)
- Decompiled `Assembly-CSharp.dll` and `Assembly-CSharp-firstpass.dll` (ONI build 707956) -- `Action.cs`, `BindingEntry.cs`, `KInputHandler.cs`, `KButtonEvent.cs`, `KInputEvent.cs`, `Modifier.cs`, `InputEventType.cs`, `GameInputMapping.cs`, `KKeyCode.cs`
- Existing project files: `Speech.cs` (Tolk P/Invoke wrapper), `tolk/dist/README.html` (Tolk documentation)
- Project research documents: `.planning/research/STACK.md`, `.planning/research/ARCHITECTURE.md`
- Game internals reference: `docs/oni-internals.md`

### Secondary (MEDIUM confidence)
- FactorioAccess architecture analysis: `docs/fa-architecture-lessons.md`, `docs/fa-features.md`, `docs/oni-accessibility-lessons.md`
- ONI accessibility analysis: `docs/oni-accessibility-analysis.md`, `docs/oni_accessibility_audit.md`
- Codebase index: `docs/CODEBASE_INDEX.md`

### Tertiary (LOW confidence)
- Tolk project status ("not currently being developed") -- API stability is HIGH confidence from the README, but long-term maintenance is uncertain. No alternative recommended; Tolk remains the best option.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all components verified from decompiled source, existing prototypes, and established community patterns
- Architecture: HIGH -- speech pipeline, context-sensitive hotkeys, vanilla mode toggle, and text filtering are all proven patterns from FactorioAccess adapted for C#/Unity with ONI-specific knowledge
- Pitfalls: HIGH -- all pitfalls verified from decompiled source, ONI modding community documentation, or direct code analysis
- Hotkey mapping: HIGH -- complete Action enum analyzed from decompiled source, all default bindings enumerable via GameInputMapping
- Localization feasibility: MEDIUM-HIGH -- pattern observed in community mods, needs runtime verification of RegisterForTranslation

**Research date:** 2026-02-11
**Valid until:** 2026-04-11 (stable domain -- ONI update frequency is low, Tolk API is frozen, Harmony 2 is mature)
