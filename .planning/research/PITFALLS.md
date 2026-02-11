# Pitfalls Research: ONI Screen Reader Accessibility Mod

> Dimension: What commonly goes wrong in projects like this?
> Scope: Harmony modding for ONI, screen reader integration via Tolk, Unity accessibility, game accessibility mod failures

---

## Critical Pitfall 1: Stale Data from Caching Game State

### The Mistake

Caching game data (building status, duplicant stats, pipe contents, overlay values) in mod-side dictionaries or fields, then reading the cached copy instead of re-querying the game. This creates stale data that gives blind players wrong information -- which is worse than no information. Unlike visual players who can see contradictions, a blind player trusts what speech tells them absolutely.

### Why Projects Fall Into This

Performance anxiety. Developers worry that querying game state every frame or every keypress is expensive, so they preemptively cache. In Factorio Access, the scanner uses explicit caching with background refresh because Factorio's API requires it (surface scanning is expensive). But ONI's internals are C# objects already in memory -- reading `building.GetComponent<Operational>().IsActive` is a pointer dereference, not an API call.

### Warning Signs

- Dictionary fields like `_buildingStatusCache` or `_tileDataCache` appearing in mod code
- Background "refresh" loops that periodically re-sync mod state with game state
- Bug reports where speech says "operating" but the building is actually idle
- Inconsistencies after loading a save or switching between asteroids (Spaced Out DLC)

### Prevention Strategy

- **Design principle already in PROJECT.md**: "Reuse game data, avoid hardcoding. Avoid caching for the same reason -- stale data is worse than a re-query."
- Always read from the game's live objects at the moment of speech output
- If something genuinely requires amortization (world scanning), use the WorkQueue pattern with explicit invalidation, never implicit "it'll refresh eventually"
- For expensive queries (e.g., scanning all buildings of a type), compute on-demand when the player requests it rather than maintaining a live mirror

### Phase Relevance

All phases. This is a design principle, not a one-time fix.

### ONI-Specific Gotcha

ONI's simulation runs even when the player is in a menu. Game state changes continuously -- temperatures shift, duplicants move, pipes fill and empty. Unlike Factorio where the mod can hook creation/destruction events to keep a cache valid, ONI's continuous simulation means any cached value can become stale within a single frame. The 200ms/1000ms sim ticks update hundreds of values simultaneously.

---

## Critical Pitfall 2: Harmony Patch Fragility Across Game Updates

### The Mistake

Writing Harmony patches that target specific private field names, method signatures, or internal class structures that change when Klei releases game updates. A single renamed field or recompiled method breaks the entire mod with a crash-on-launch.

### Why Projects Fall Into This

ONI modding requires patching private internals because there is no official modding API for most systems. Every `AccessTools.FieldRefAccess<OptionsMenuScreen, KButton>("closeButton")` is a bet that Klei will not rename that field. Klei updates ONI regularly (monthly patches, DLC content drops, hotfixes).

### Warning Signs

- Hard-coded field name strings scattered throughout the codebase with no fallback
- `NullReferenceException` or `MissingFieldException` in logs after a game update
- Patches targeting methods that are inlined or refactored by the compiler
- Patches on types that only exist in specific DLC (Spaced Out, Bionic Booster Pack)

### Prevention Strategy

- **Use dynamic type resolution** for anything that might not exist in all versions. The oni-internals.md doc already documents this pattern:
  ```csharp
  var type = AccessTools.TypeByName("GraphicsOptionsScreen");
  return type == null ? null : AccessTools.Method(type, "OnSpawn");
  ```
- **Centralize all reflection access** in a single file or class per screen/system, so when a field name changes, there is exactly one place to update
- **Use lazy initialization** (`??=`) for field refs to avoid startup-time crashes and defer failures to point of use
- **Null-check the result of AccessTools calls** at the point of use rather than assuming they succeeded
- **Add a version check** on mod load that compares the game build number and logs warnings if it is newer than the last tested version
- **Prefer postfix patches over prefix patches** where possible -- postfix patches are less likely to conflict with other mods and survive method body changes better

### Phase Relevance

Phase 0 (Foundation) and all subsequent phases. Every new Harmony patch added increases the surface area for breakage.

### ONI-Specific Gotcha

ONI has three separate DLC expansion packs (Spaced Out, Bionic Booster Pack, and any future ones). Screens and systems may exist in some DLC configurations but not others. The Bionic Booster Pack added entirely new side screen types. The mod must detect which DLC are active and only patch types that exist. `AccessTools.TypeByName` returning null is the normal case, not an error.

Additionally, ONI's modding community has documented that Klei sometimes changes internal class hierarchies during "small" patches. The ONI modding Discord and forums regularly report that field names like `closeButton` vs `CloseButton` vs `m_closeButton` shift without notice. Always test after every game update.

---

## Critical Pitfall 3: Input Conflict with ONI's Extensive Hotkey System

### The Mistake

Binding accessibility mod hotkeys that collide with ONI's existing keybindings, creating situations where pressing a key triggers both the mod action and the game action, or where the mod silently swallows inputs the game needs.

### Why Projects Fall Into This

ONI has an exceptionally large hotkey set: F1-F12 and Shift+F1-F4 for overlays, G/C/X/M/P/B for tools, V/F/L/J/R/E/Z/U for management screens, WASD for camera, Tab for speed, Space for pause, 1-9 for priorities. Almost every key on the keyboard already does something. An accessibility mod needs its own extensive keybindings for cursor movement, scanning, layer queries, and UI navigation. Collision is nearly guaranteed without careful planning.

### Warning Signs

- Player presses a key and gets both the mod response AND a game action (e.g., pressing G to dig also moves the accessibility cursor)
- `KButtonEvent.Consumed` not being set properly, causing input to fall through to the game
- Modifier key combinations (Ctrl+, Alt+, Shift+) clashing with existing ONI modifier uses
- Accessibility keys stop working when certain game screens are open because the screen's `OnKeyDown` consumes them first

### Prevention Strategy

- **Audit every ONI hotkey** before assigning mod bindings. The `InputBindingsScreen` lists all bindings; the decompiled `GameInputMapping.cs` has the complete default set
- **Consume events properly**: In postfix patches on `OnKeyDown`, check `e.Consumed` first, then set `e.Consumed = true` after handling. This prevents double-handling.
- **Use modifier-key namespaces**: Reserve a specific modifier prefix for all accessibility actions (e.g., Ctrl+Alt as the accessibility prefix) to avoid collision with game hotkeys
- **Identify hotkeys that are useless to blind players** and deliberately overwrite them. Per PROJECT.md: "Many are useless to blind players and can be overwritten. But every overwrite is a deliberate decision -- document what the original hotkey did and why it's being replaced."
- **Implement a priority system**: When the accessibility UI layer is active (e.g., cursor mode, menu navigation), the mod's input handler should consume all navigation keys before they reach the game. When inactive, keys should pass through.

### Phase Relevance

Phase 1 (Core Navigation) is where this must be solved. The input architecture established here propagates to every subsequent phase.

### ONI-Specific Gotcha

ONI's input system uses `KButtonEvent` with a `Consumed` flag, but the input propagation order depends on the screen stack. Per oni-internals.md: "Do not poll the screen stack to determine input ownership. Use a registration chain or state machine to manage which handler owns input at any given time." Also, input fires multiple times per frame -- the debounce pattern (`Time.frameCount == lastInputFrame`) is mandatory.

Critically, ONI's `OnKeyDown` handlers run in screen stack order (topmost screen gets first crack). If the mod opens its own screen on top, it naturally gets input priority. But if the mod patches an existing screen's `OnKeyDown`, ordering depends on Harmony patch priority. Other mods may patch the same methods.

---

## Critical Pitfall 4: Speech Spam and Audio Overload

### The Mistake

Generating too many speech utterances in rapid succession, causing the screen reader to queue up a long backlog of announcements, or constantly interrupting itself so the player never hears a complete message. This is the most common failure mode in game accessibility mods.

### Why Projects Fall Into This

ONI generates a torrent of state changes every second: duplicants move, errands change, temperatures fluctuate, notifications fire. A naive "announce every change" approach produces unlistenable output. The problem is especially acute during crises (flooding, suffocation) when the player most needs clear information but the game generates the most events.

### Warning Signs

- Player hears only the first syllable of messages before the next one interrupts
- Speech queue grows unbounded during normal gameplay
- Notification events fire faster than they can be spoken
- Moving the cursor through tiles produces a stutter of overlapping tile descriptions
- Screen reader falls behind real-time game state

### Prevention Strategy

- **One utterance per user action**: The MessageBuilder accumulates all output for a single interaction into one `Speech.Say()` call. Never call `Speech.Say()` from multiple independent handlers for the same user action.
- **Cooldown-based deduplication**: Per oni-accessibility-lessons.md, use time-based deduplication:
  ```csharp
  if (Time.unscaledTime - lastTime < 0.3f) return;
  ```
- **Tick-offset distribution**: Stagger periodic checks so they do not all fire on the same frame
- **Interrupt vs. queue**: Use `interrupt: true` for user-initiated queries (cursor movement) and `interrupt: false` only for background alerts that should queue
- **Throttle cursor movement speech**: When the player holds down a cursor movement key, skip intermediate tile announcements and only announce the final position after a brief pause (150-200ms debounce)
- **Priority-based alert suppression**: During crises, suppress low-priority status updates and only announce critical alerts
- **Per-feature opt-out**: Let players disable specific speech categories they find noisy

### Phase Relevance

Phase 1 (Core Navigation) establishes the speech pipeline. Phase 3 (Duplicant Management) and Phase 4 (Overlays) introduce the highest-volume speech sources.

### ONI-Specific Gotcha

ONI's notification system fires rapidly and stacks. Per the audit doc: "Notifications are transient and stack. Old ones scroll up and eventually disappear." The mod must not try to speak every notification as it appears. Instead, maintain a notification queue that the player explicitly cycles through (similar to FactorioAccess's alert tab).

The "screen stack events fire rapidly" gotcha from oni-internals.md is particularly dangerous: opening one menu may trigger OnActivate on multiple screens in the same frame. Without deduplication, the player hears overlapping screen announcements.

---

## Critical Pitfall 5: FieldRef Startup Lag from Eager Reflection

### The Mistake

Creating all `AccessTools.FieldRefAccess` delegates during mod initialization, causing a 4+ second lag on game startup that makes the game appear frozen.

### Why Projects Fall Into This

The natural instinct is to initialize everything upfront in `OnLoad()`. When there are dozens or hundreds of field accessors (and this mod will need many -- every screen has 5-15 private fields), the cumulative cost of reflection-based delegate compilation becomes visible.

### Warning Signs

- Game takes noticeably longer to start with the mod enabled
- `OnLoad()` method grows beyond a few lines
- Players report "the game hangs for a few seconds after the loading screen"

### Prevention Strategy

This is explicitly documented in oni-internals.md:

```csharp
private static AccessTools.FieldRef<ModeSelectScreen, MultiToggle> _survivalButton;
private static AccessTools.FieldRef<ModeSelectScreen, MultiToggle> SurvivalButton =>
    _survivalButton ??= AccessTools.FieldRefAccess<ModeSelectScreen, MultiToggle>("survivalButton");
```

- **Always use lazy initialization** (`??=`) for field ref accessors
- **Batch reflection per screen**: Only resolve field refs for a screen when that screen is first opened, not at mod load time
- **Log reflection failures** at point of use with the field name and type, so broken fields are diagnosable

### Phase Relevance

Phase 0 (Foundation). This pattern must be established before any screen patching begins.

### ONI-Specific Gotcha

ONI has 20+ distinct screen classes just for the frontend menus (per oni-internals.md listing), plus 100+ side screen variants for buildings. If each screen needs 5-10 field refs, that is 500-1000+ reflection delegates. Eager initialization would cause catastrophic startup delay. The lazy pattern is not optional -- it is required for acceptable startup time.

---

## Critical Pitfall 6: UI Not Ready When Patch Fires

### The Mistake

Reading UI element state (positions, text, child objects) immediately in an `OnActivate` or `OnSpawn` postfix patch, before Unity has completed layout for that frame. This produces null references, zero-size rects, or empty text fields.

### Why Projects Fall Into This

Harmony postfix patches fire synchronously after the patched method returns. But Unity's UI layout system is deferred -- it runs at the end of the frame. When `OnActivate` returns, the screen's GameObjects exist but their `RectTransform` sizes may be zero and dynamically-populated content (lists, grids) may not be filled yet.

### Warning Signs

- `NullReferenceException` when accessing child components of a screen that "should exist"
- Tooltip text is empty when read immediately but correct a frame later
- List/grid elements have zero count in the patch but are populated visually
- Intermittent failures that work "sometimes" (timing-dependent)

### Prevention Strategy

Per oni-internals.md:

```csharp
yield return new WaitForEndOfFrame();
yield return new WaitForEndOfFrame();
// Now safe to read layout
```

- **Use coroutines** to defer UI reading by 1-2 frames after screen activation
- **Start coroutines from patches** via `GameScheduler.Instance.Schedule("desc", 0, _ => { ... })` or by getting a MonoBehaviour reference and calling `StartCoroutine`
- **Check for null defensively at the UI boundary** (this is the one place where null checking IS appropriate, per the anti-defensive coding principle -- UI state is a system boundary)
- **Never assume child counts or content** in the same frame as activation

### Phase Relevance

Phase 1 (Menus) and every subsequent phase that patches UI screens.

### ONI-Specific Gotcha

ONI uses a custom lifecycle (`OnPrefabInit` -> `OnSpawn` -> `OnActivate`) that overlaps with but does not match Unity's (`Awake` -> `Start` -> `OnEnable`). The `OnActivate` method fires when the screen becomes visible, but dynamically-populated content (like the mod list in ModsScreen, or colony list in LoadScreen) is often populated asynchronously. The `entryParent` transform may have zero children in the `OnActivate` postfix even though children appear one frame later.

The `ColonyDestinationSelectScreen` is particularly notorious: it has multiple panels (`destinationMapPanel`, `storyContentPanel`, `mixingPanel`, `newGameSettingsPanel`) that populate independently and asynchronously based on procedural generation data.

---

## Critical Pitfall 7: Hardcoding Text Instead of Using Game Strings

### The Mistake

Writing mod-side strings like `Speech.Say("Manual Generator, consuming coal")` instead of reading from the game's `STRINGS` namespace and `LocText` components. This creates text that becomes stale when the game updates, does not reflect the player's language setting, and duplicates effort.

### Why Projects Fall Into This

It is faster to hardcode a string than to find the correct STRINGS path or LocText component. During prototyping, developers hardcode "just for now" and never go back. The STRINGS namespace has a complex hierarchy (`STRINGS.UI.FRONTEND.PAUSE_SCREEN.TITLE`) that requires decompilation to discover.

### Warning Signs

- Literal English strings in Speech.Say() calls
- Player reports that the mod always speaks English regardless of language setting
- After a game update, speech says "Algae Deoxidizer" but the game now calls it "Algae Diffuser" (hypothetical rename)
- Duplicate or inconsistent names for the same entity

### Prevention Strategy

- **Always read LocText.text** from the actual UI element rather than constructing strings
- **Use STRINGS namespace** for known game strings: `STRINGS.UI.FRONTEND.PAUSE_SCREEN.TITLE`
- **Use Strings.Get()** for dynamic lookups where the key comes from game data
- **Strip rich text markup** before speaking (already handled by `Speech.FilterRichText`)
- **Hardcode only when no game source exists** -- for example, accessibility-specific instructions like "Press Ctrl+Alt+Left to move cursor"
- **Maintain a single file** of all mod-specific hardcoded strings for future localization

### Phase Relevance

All phases. Establish the pattern in Phase 1 and enforce it throughout.

### ONI-Specific Gotcha

ONI's text contains Unity Rich Text tags (`<color=#FF0000>`, `<b>`, `<link>`) and sprite references. The existing `Speech.FilterRichText` method handles this, but some text also contains `{Hotkey}` placeholders from the tooltip system that produce garbage in speech. Per oni-internals.md, strip these:

```csharp
int idx = text.IndexOf("{Hotkey}");
if (idx >= 0) text = text.Substring(0, idx).TrimEnd(' ', ':');
```

Also, `ToolTip.RebuildDynamicTooltip()` must be called before reading tooltip content -- tooltips are lazily evaluated and may contain stale or empty text until rebuilt.

---

## Critical Pitfall 8: Tolk DLL Loading and Platform Issues

### The Mistake

Assuming Tolk DLLs will be found automatically, or failing to handle the case where no screen reader is running, or not setting the DLL search path before calling `Tolk_Load()`.

### Why Projects Fall Into This

Tolk requires its DLLs (Tolk.dll plus screen reader connector DLLs) to be in the DLL search path. Unity/Mono's DLL resolution does not automatically look in the mod's directory. The P/Invoke call fails with `DllNotFoundException` if the path is not configured, and this happens before any error handling can catch it.

### Warning Signs

- `DllNotFoundException: Tolk.dll` on mod load
- Mod works on the developer's machine but not on users' machines
- Mod works with NVDA but crashes with JAWS (or vice versa)
- Silent failure where `Tolk_HasSpeech()` returns false because connector DLLs are missing

### Prevention Strategy

- **Call `SetDllDirectory`** (via P/Invoke to kernel32.dll) before Tolk_Load() to point at the mod's `tolk/dist/` directory. The existing Speech.cs docs note: "Must be called after SetDllDirectory points to the Tolk DLL location."
- **Ship all Tolk connector DLLs** (NVDA, JAWS, SAPI, WindowEyes, etc.) in the mod distribution
- **Handle `DllNotFoundException` gracefully** with a user-visible message (which the existing code does)
- **Handle the "no screen reader" case**: `Tolk_HasSpeech()` may return false if no screen reader is running. SAPI fallback (`Tolk_TrySAPI(true)`) should catch this, but verify on a clean Windows install
- **Test on multiple screen readers**: NVDA (free, most common), JAWS (commercial, different API), and SAPI-only (no screen reader at all)
- **x86 vs x64 mismatch**: ONI is a 64-bit Unity game. Ensure all Tolk DLLs are 64-bit. Mixing 32-bit DLLs with a 64-bit process causes `BadImageFormatException`.

### Phase Relevance

Phase 0 (Foundation). Speech must work before anything else can be tested.

### ONI-Specific Gotcha

ONI mods are loaded from the Steam Workshop directory or `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\local\`. The mod's DLL and its dependencies must be in the right place. ONI's mod loader copies the mod DLL but may not copy native DLLs (Tolk.dll and friends). The mod needs to resolve its own directory at runtime:

```csharp
string dllPath = typeof(MyMod).Assembly.Location;
string modDir = Path.GetDirectoryName(dllPath);
```

Then set the DLL search path to include the subdirectory containing Tolk DLLs. This must happen in `OnLoad()` before any other code runs.

---

## Critical Pitfall 9: Monolithic Controller / God Class

### The Mistake

Putting all accessibility logic into a single large class -- input handling, speech output, screen patching, cursor state, scanner logic, overlay queries -- creating an unmaintainable monolith.

### Why Projects Fall Into This

FactorioAccess's `control.lua` (4183 lines) demonstrates exactly this failure mode. It started small and grew. In their case, Factorio's architecture forced a single entry point. ONI/Harmony has no such constraint, but the temptation to add "just one more handler" to the main mod class is strong, especially during rapid prototyping.

### Warning Signs

- The main mod file exceeds 500 lines
- Multiple concerns (input, speech, state, UI) are mixed in one class
- Adding a new screen requires modifying the central controller
- Merge conflicts on the same file when working on different features

### Prevention Strategy

Per fa-architecture-lessons.md: "Avoid the monolith, adopt the extraction pattern."

- **Each system registers its own Harmony patches**: InputPatches.cs, ScreenPatches.cs, etc.
- **Each system subscribes to its own events**: No central dispatcher routing everything
- **Thin orchestrator only**: The `UserMod2.OnLoad()` should only initialize Tolk, set up logging, and call `harmony.PatchAll()`. It should contain zero game logic.
- **One file per screen handler**: MainMenuHandler.cs, OptionsMenuHandler.cs, etc.
- **Shared utilities in Core/**: Speech.cs, MessageBuilder.cs, CursorState.cs

### Phase Relevance

Phase 0 (Foundation). The architecture must be modular from the start. Refactoring a monolith later is expensive and risky.

### ONI-Specific Gotcha

ONI has 20+ frontend screens, 15 overlay modes, 100+ side screen variants, and 5+ management screens. If each needs its own handler class, the mod will have 150+ classes. This is normal and correct -- it reflects the inherent complexity of the game. A monolith that tries to handle all of these in one place will be unmaintainable before Phase 2 is complete.

---

## Critical Pitfall 10: Overlooking the Multi-Layer Tile Problem

### The Mistake

Designing tile readout as a simple "read what's here" system, then discovering that a single tile has 10+ overlapping data layers and the player is drowning in information or missing critical data.

### Why Projects Fall Into This

FactorioAccess tiles are relatively simple: one entity, one terrain type, maybe an underground belt. ONI tiles simultaneously have: solid/liquid/gas element, temperature, mass/pressure, germ count and type, decor value, light level, a building (maybe), a liquid pipe (maybe), a gas pipe (maybe), an automation wire (maybe), a conveyor rail (maybe), power wire (maybe), and a background building (maybe). Reading all of this on every cursor movement is an unlistenable wall of text.

### Warning Signs

- Cursor movement announcements take 5+ seconds to finish speaking
- Players skip past important information because the readout is too long
- Different data layers get concatenated without structure, making it hard to parse
- Players cannot find the specific information they need without listening to everything

### Prevention Strategy

Per oni-accessibility-analysis.md Section 4: "The player controls information density by choosing which overlays are active at any given time."

- **Default readout is minimal**: Element type, building name if present. Nothing else unless asked.
- **Layer query system**: Modifier keys to query specific layers on demand (oxygen, temperature, plumbing, etc.)
- **Togglable continuous overlays**: When an overlay is active, its data is included in cursor movement readout. When off, it is not.
- **Match game overlay hotkeys**: F1 for oxygen, F2 for power, etc. -- so wiki guides remain applicable
- **Brevity mode**: For multi-overlay readout, offer abbreviated format: "O2 1.8k, pipe PW 32d"
- **Design the information hierarchy before writing code**: Decide exactly what each verbosity level says, document it, and implement to spec

### Phase Relevance

Phase 1 (Core Navigation) for the basic cursor and default readout. Phase 4 (Overlays) for the full layer system. But the architecture must support layers from Phase 1 -- retrofitting layered readout onto a flat system is a major rewrite.

### ONI-Specific Gotcha

The 5 overlapping conduit networks (power wire, liquid pipe, gas pipe, automation wire, conveyor rail) can ALL exist in the same tile simultaneously. A single tile in a well-built ONI base might have a building, two pipe types, an automation wire, and a power wire -- plus the background element, temperature, and germs. The overlay layer system is not a nice-to-have; it is the only way to make this information manageable. Without it, the mod is unusable in mid-to-late game bases.

---

## Critical Pitfall 11: Treating ONI Like a Static UI (Ignoring Autonomous Agents)

### The Mistake

Designing the mod as if ONI were a menu-based game where the player drives all actions. In reality, ONI is a simulation where 3-30+ duplicants make autonomous decisions. The world changes continuously even when the player does nothing. Information must be surfaced proactively, not just on query.

### Why Projects Fall Into This

FactorioAccess modeled a game where the player is the sole agent. Everything happens because the player did it. ONI is fundamentally different: duplicants decide what to do based on priorities, skills, pathfinding, and needs. A blind player who queries a building might find a duplicant working there one moment and gone the next -- not because of anything the player did.

### Warning Signs

- Player is surprised by colony crises because the mod only speaks when asked
- No awareness of duplicant activities unless the player manually queries each one
- Priority system changes have no audible feedback
- Players cannot tell if their commands are being carried out

### Prevention Strategy

- **Proactive problem detection**: Hook into entity status systems to surface problems without being asked. Per oni-accessibility-analysis.md Section 4.3: "The mod surfaces these status flags as an ongoing problem report."
- **Periodic status summaries**: Offer a "colony pulse" keybinding that speaks a brief status: idle duplicant count, active alerts, pending errands
- **Alert queue**: Maintain a navigable list of current notifications that the player can cycle through at will
- **Duplicant activity tracking**: "Follow duplicant" mode that periodically announces what a specific duplicant is doing
- **Build order confirmation**: When the player places a build order, announce when a duplicant picks it up and when it completes

### Phase Relevance

Phase 3 (Duplicant Management) is where this becomes critical. But the alert/notification system should be sketched in Phase 1 so the architecture supports proactive announcements.

### ONI-Specific Gotcha

ONI's priority system is the core mechanism for directing duplicant behavior, and it is extremely complex: per-duplicant task-type priorities (a grid of duplicant rows vs. errand category columns with 5 priority levels) PLUS per-building sub-priorities (1-9 plus yellow alert). A blind player who cannot see the priority grid cannot effectively manage their colony. Making this grid navigable and audible is one of the hardest UI challenges in the mod.

---

## Critical Pitfall 12: Dropdown.RefreshShownValue() Omission

### The Mistake

Programmatically setting a Unity Dropdown's `value` property without calling `RefreshShownValue()`, causing the dropdown's displayed text to be out of sync with its actual value. Speech reads the stale displayed text, giving the player wrong information about what is selected.

### Why Projects Fall Into This

Unity's Dropdown documentation does not prominently warn about this. The `value` setter updates the internal state but does not update the visual (or textual) representation. Most Unity tutorials set value and move on. In a visual context, the discrepancy may be noticed and corrected; in a speech context, it is invisible.

### Warning Signs

- Speech announces the previous dropdown selection after the player changes it
- Dropdown reads correctly on first open but goes stale after programmatic changes
- Resolution, audio device, or color mode dropdowns show wrong values

### Prevention Strategy

Per oni-internals.md:

```csharp
dropdown.value = newIndex;
dropdown.RefreshShownValue(); // MUST call after setting value
```

- **Wrap dropdown manipulation** in a helper method that always calls `RefreshShownValue()`
- **Never set dropdown.value directly** -- always go through the helper

### Phase Relevance

Phase 1 (Menu Navigation), specifically the options screens that contain dropdowns.

### ONI-Specific Gotcha

ONI uses dropdowns for: resolution selection, color mode, audio device, language selection, and various gameplay settings. The `GraphicsOptionsScreen` has `resolutionDropdown` and `colorModeDropdown`; `AudioOptionsScreen` has `deviceDropdown`. These are the screens where this bug is most likely to appear.

---

## Critical Pitfall 13: Mod Compatibility Conflicts

### The Mistake

Assuming the accessibility mod is the only mod patching ONI's classes. In reality, ONI has an active modding community with hundreds of popular mods. Two mods patching the same method can conflict, causing crashes or silent behavior changes.

### Why Projects Fall Into This

During development, the mod is tested in isolation. But users will run it alongside Quality of Life mods, UI mods, gameplay overhauls, and other popular mods. Harmony patch ordering is not guaranteed, and two postfix patches on the same method can interact unpredictably.

### Warning Signs

- Mod works perfectly alone but crashes when another mod is also enabled
- `HarmonyPatchException` or `AmbiguousMatchException` in logs
- Another mod's prefix patch returns `false` (skipping the original method), which also skips the accessibility mod's postfix
- Silent failures where the accessibility mod's patch runs but the game state has been modified by another mod's patch first

### Prevention Strategy

- **Use postfix over prefix** where possible (postfixes cannot be skipped by other mods)
- **Never use prefix patches that return false** (this is the nuclear option in Harmony and should be reserved for extreme cases)
- **Set Harmony priority explicitly** on patches that might conflict: `[HarmonyPriority(Priority.Low)]` for read-only patches that just need to observe, `[HarmonyPriority(Priority.First)]` for input interception
- **Test with popular ONI mods**: Notably, the "Mod Updater" mod, "Settings Change Tool", and any UI-modifying mods
- **Design patches to be purely observational** where possible: read state and produce speech, but do not modify game state. This eliminates most conflict scenarios.
- **Log when patches fire**: Include debug logging that can verify patches are running in the expected order

### Phase Relevance

All phases, but especially important to verify in Phase 1 before the architecture solidifies.

### ONI-Specific Gotcha

ONI's modding community has several popular mods that modify the same screens the accessibility mod will patch. For example, mods that add new overlay buttons, modify the build menu, or add new management screens will touch the same UI classes. The ONI modding Discord is the best resource for identifying which methods are commonly patched.

Also, ONI mods can declare load order dependencies via the mod.yaml file. If conflicts are identified, the accessibility mod should document recommended load order or use Harmony's `[HarmonyBefore]` and `[HarmonyAfter]` attributes.

---

## Critical Pitfall 14: Ignoring the DLC Matrix

### The Mistake

Writing code that assumes all DLC content exists (or that none does), causing crashes or missing functionality depending on which DLC the player owns.

### Why Projects Fall Into This

The developer might own all DLC and test only in that configuration. But players may have: base game only, base + Spaced Out, base + Bionic Booster, or all three. Each combination adds or removes types, screens, and systems.

### Warning Signs

- `TypeLoadException` for DLC-only classes on base-game installations
- Missing overlay modes (Radiation overlay is Spaced Out-only)
- Starmap code crashing because the base game starmap is completely different from the Spaced Out starmap
- Bionic duplicant screens causing errors for players without the Bionic Booster Pack

### Prevention Strategy

- **Check DLC activation at runtime**: Use `DlcManager.IsExpansion1Active()` (Spaced Out) and equivalent checks
- **Use dynamic type resolution** for all DLC-specific types (as described in Pitfall 2)
- **Test in all DLC configurations**: Base only, base + each DLC individually, and all DLC
- **Gate features by DLC**: The mod should gracefully offer less functionality when DLC is not present
- **Never reference DLC types directly in `[HarmonyPatch]` attributes**: Use `TargetMethod()` with `AccessTools.TypeByName` instead

### Phase Relevance

Phase 0 (Foundation) for the detection infrastructure. Every subsequent phase must use it when adding DLC-specific features.

### ONI-Specific Gotcha

The Spaced Out DLC fundamentally changes several systems: the starmap is completely different (hex grid vs. distance rings), multi-asteroid management adds world-switching UI, and rocket interiors are a new gameplay layer. The accessibility mod cannot have a single code path for "starmap" -- it needs completely separate handlers for base game and Spaced Out starmaps.

The Bionic Booster Pack adds bionic duplicants with different needs (power banks instead of food, gear oil, internal oxygen) and new side screens. The duplicant detail panel has additional tabs for bionic-specific stats.

---

## Critical Pitfall 15: Silent Failures for Blind Users

### The Mistake

Catching exceptions and returning empty/default values instead of surfacing errors audibly. A sighted player might notice a missing UI element or wrong display; a blind player hears nothing and assumes the game has no information to give.

### Why Projects Fall Into This

Defensive coding habits from other projects. The instinct to "handle" errors by returning empty strings or null rather than crashing. In a visual application, this produces a blank space that a developer can see and investigate. In a speech-based application, silence is indistinguishable from "nothing to report."

### Warning Signs

- Try-catch blocks that swallow exceptions and return empty strings
- `?.` chains that collapse to null without any fallback speech
- Patterns like `if (text == null) return;` without logging or alternative output
- Players report that "nothing happens" when they try to inspect certain buildings or screens

### Prevention Strategy

Per fa-architecture-lessons.md Section 10:

- **Validate at system boundaries only** (user input, entity existence checks at point of interaction)
- **Internal code should crash** if its preconditions are not met -- a crash produces a log entry that developers can diagnose
- **Never silently return empty text** -- if a description cannot be generated, say so: "No information available" or "Error reading building status"
- **Log all exceptions** with full stack traces, even the ones you handle at boundaries
- **For the Speech.Say boundary**: The existing Speech.cs catches exceptions and logs them, which is correct. But callers should not preemptively catch exceptions before calling Speech.Say.

### Phase Relevance

All phases. This is a coding philosophy that must be enforced from the start.

### ONI-Specific Gotcha

ONI has many optional components on buildings. A building may or may not have `EnergyConsumer`, `Operational`, `Storage`, `BuildingHP`, etc. Checking `GetComponent<T>()` for null at the system boundary (before describing the building) is correct. But once you have confirmed a component exists, internal code that processes its data should not defensively null-check every property -- that hides bugs.

---

## Anti-Pattern Summary from FactorioAccess

The fa-architecture-lessons.md document identifies several anti-patterns that directly apply:

| Anti-Pattern | FactorioAccess Example | ONI Equivalent Risk |
|---|---|---|
| Monolithic controller | control.lua at 4183 lines | Main mod class becoming a dumping ground |
| God function for entity description | fa-info.lua at 2263 lines | Single describer for all 200+ building types |
| Centralized state initialization | Global state setup function | Eager FieldRef initialization crash |
| Defensive null-checking hiding bugs | `if entity and entity.valid then...` | `building?.GetComponent<X>()?.property ?? ""` |
| Flat navigation for hierarchical data | Linear list of all entities | Reading all 10 tile layers sequentially |
| Competing speech calls | Multiple speak() from independent handlers | Notification + cursor + overlay all speaking |

---

## Phase-Specific Pitfall Checklist

### Phase 0 (Foundation)
- [ ] Tolk DLL loading verified on clean install (Pitfall 8)
- [ ] Lazy FieldRef pattern established (Pitfall 5)
- [ ] Modular architecture enforced (Pitfall 9)
- [ ] DLC detection infrastructure in place (Pitfall 14)
- [ ] Anti-defensive coding policy documented (Pitfall 15)

### Phase 1 (Menus & Core Navigation)
- [ ] Input system handles event consumption correctly (Pitfall 3)
- [ ] Debounce for rapid cursor movement (Pitfall 4)
- [ ] UI frame delay pattern established (Pitfall 6)
- [ ] Game strings used instead of hardcoded text (Pitfall 7)
- [ ] Dropdown helper wraps RefreshShownValue (Pitfall 12)
- [ ] Tile readout architecture supports layers from day one (Pitfall 10)

### Phase 2 (Building & Construction)
- [ ] Build menu speech does not spam during category navigation (Pitfall 4)
- [ ] Building placement uses game text for building names (Pitfall 7)
- [ ] Side screen patches handle async population (Pitfall 6)

### Phase 3 (Duplicant Management)
- [ ] Priority grid is navigable without speech overload (Pitfalls 4, 10)
- [ ] Proactive status system exists for autonomous agent updates (Pitfall 11)
- [ ] Schedule editor handles 24-slot grid without overwhelming readout (Pitfall 10)

### Phase 4 (Overlays & Environmental Awareness)
- [ ] Layer query system prevents information overload (Pitfall 10)
- [ ] Overlay data comes from live game state, not cached (Pitfall 1)
- [ ] Overlay-specific readout is toggleable per-layer (Pitfall 4)

### Phase 5+ (Advanced Systems, DLC)
- [ ] DLC content gracefully absent when DLC not owned (Pitfall 14)
- [ ] Mod compatibility tested with popular ONI mods (Pitfall 13)
- [ ] Starmap has separate base/DLC implementations (Pitfall 14)

---

## Research Confidence

| Category | Confidence | Basis |
|---|---|---|
| Harmony patching pitfalls | High | Documented in oni-internals.md, well-known in modding community |
| Tolk/speech integration | High | Existing Speech.cs implementation, known P/Invoke patterns |
| Information overload | High | Extensively analyzed in oni-accessibility-analysis.md |
| FactorioAccess anti-patterns | High | Detailed in fa-architecture-lessons.md |
| ONI community gotchas | Medium | Based on documented patterns; would benefit from ONI modding Discord verification |
| DLC compatibility matrix | Medium | Known DLC structure; specific type availability needs runtime testing |
| Mod compatibility | Medium | General Harmony knowledge; specific ONI mod conflicts need community research |

---

*Generated from: oni-internals.md (Known Gotchas), oni-accessibility-analysis.md (Challenges), fa-architecture-lessons.md (Anti-patterns), oni-accessibility-lessons.md (Design patterns), oni_accessibility_audit.md (UI scope), PROJECT.md (Design principles), Speech.cs (Tolk implementation)*
