# Lessons from FactorioAccess for an ONI Accessibility Mod

A comprehensive analysis of the FactorioAccess codebase (~315 Lua files, ~83k lines) and the design patterns, accessibility principles, and architectural decisions that transfer to building an accessibility mod for Oxygen Not Included (C#/Unity/Harmony).

---

## 1. The Core Insight: Audio-First, Not Visual-Adaptation

FactorioAccess doesn't describe what's on screen -- it **reimagines the interaction model** around audio. A blind player's mental model of their factory is fundamentally different from a sighted player's, and the mod leans into that rather than fighting it.

For ONI, this means: don't try to describe the colony visually. Instead, design systems that let players **query what they care about** -- which duplicants are idle, what's the oxygen situation, where are the unbuilt errands. The colony is a set of systems, not a picture.

Key quote from FactorioAccess's CLAUDE.md:
> "Remember: This mod makes a visual game accessible through audio. Every feature must be designed with audio-first interaction in mind!"

---

## 2. The Scanner Pattern: Hierarchical World Browsing

This is FactorioAccess's flagship pattern and will be critical for ONI.

### The Problem

Thousands of entities in a 2D world. You can't list them all. A typical Factorio save has 10,000-20,000 player-placed objects plus thousands of natural features. ONI colonies have hundreds of buildings, dozens of duplicants, and complex pipe/wire networks.

### Their Solution: 3-Level Hierarchy

- **Categories** (Production, Resources, Military...) -- navigate with one keybind
- **Subcategories** (iron furnaces, copper furnaces...) -- navigate with another
- **Instances** sorted by distance -- navigate with a third

The cursor auto-jumps to whatever you've selected. This turns spatial exploration into **list browsing with teleportation**.

### Example Workflow (Factorio)

1. Press END (refresh scanner) -- "Scanner refreshed"
2. Ctrl+PageDown (categories) -- "Resources"
3. PageDown (subcategories) -- "Iron ore, 250 ore, 8 east and 15 south"
4. Shift+PageDown (next instance) -- "Iron ore, 180 ore, 32 east and 12 north"
5. Home (move to selected) -- cursor jumps to that location

### How This Maps to ONI

Categories map naturally to ONI's overlay system:

| FactorioAccess Category | ONI Equivalent |
|---|---|
| Production | Colony buildings (by type) |
| Resources | Natural resources, geysers |
| Logistics | Plumbing, ventilation, conveyors |
| Power | Generators, batteries, consumers |
| Military | Critter threats, entry points |
| Terrain | Biome boundaries, space exposure |

Subcategories could be building types or status types (buildings needing repair, buildings without power, idle duplicants). Instances sorted by priority or proximity.

### Clustering for Information Density

ONI has the same problem Factorio does -- hundreds of identical objects. FactorioAccess clusters them:

- **Trees**: Spatial hash (8x8 chunks) -- nearby trees become "forest, 500 trees"
- **Water**: Incremental floodfill -- "water body, 250 tiles"
- **Resources**: Patch detection -- "iron ore patch, 2400 ore"

Close objects get individual detail; distant objects get summaries.

For ONI: "3 lavatories in bathroom zone" rather than listing each one. A natural gas pocket could be "natural gas, 47kg, 3 right and 8 down."

### Implementation Notes

FactorioAccess's scanner uses several performance strategies:

- **Effect triggers** for immediate new-entity detection (hook into creation events)
- **Work queues** with per-tick budgets (process 100 new entities per tick, not all at once)
- **Lazy updates** -- heavy work happens only when announcing an entry, not during every scan
- **Bitsets** for memory-efficient entity tracking (32x savings over hash tables)

See: `scripts/scanner/` (all files), `ds/clusterer.lua`, `ds/tile-clusterer.lua`

---

## 3. Direction + Distance, Not Coordinates

Players never hear "position 43, 87." They hear **"iron patch, 8 east and 15 south."**

### Direction Bias Algorithm

FactorioAccess uses biased direction detection with:
- 30-degree cones for cardinal directions (N/S/E/W)
- 60-degree cones for diagonals (NE/NW/SE/SW)

The formula: if |diff_x| > 4|diff_y|, it's purely East/West. If |diff_y| > 4|diff_x|, it's purely North/South. Otherwise diagonal.

This makes alignment easy to recognise -- players can tell when two buildings are on the same horizontal or vertical line.

### Alignment Detection

When two positions share the same row or column (or diagonal), the announcement changes to indicate "aligned" -- helping players understand they can build in a straight line.

### For ONI

"Electrolyzer, 3 right and 12 down from your duplicant" is far more useful than tile coordinates. ONI's vertical nature means "above/below" becomes especially important -- and you can exploit this since ONI bases tend to be layered horizontally. Consider also using room names if available ("in the barracks, 4 tiles left").

See: `scripts/fa-utils.lua` (direction/distance calculation)

---

## 4. Message Building: The Handler Chain Pattern

Entity descriptions aren't built in one monolithic function. Instead, a `MessageBuilder` is passed through a **chain of small handler functions**, each of which optionally appends information:

```
ent_info_facing(ctx)        -> "Facing north"
ent_info_power_status(ctx)  -> "No power"
ent_info_belt_contents(ctx) -> (skipped, not a belt)
ent_info_inventory(ctx)     -> "Contains 50 iron plate"
```

### Why This Matters

- Each handler checks its own applicability -- no central routing logic
- Handlers are independently testable
- Easy to add new information without touching existing code
- Different contexts can use different handler chains (brief vs. detailed)

### C# Translation

```csharp
interface IDescriptionContributor {
    void Contribute(DescriptionContext ctx, Building building);
}

// Each contributor checks applicability and appends
class PowerStatusContributor : IDescriptionContributor {
    public void Contribute(DescriptionContext ctx, Building building) {
        if (!building.HasComponent<EnergyConsumer>()) return;
        var consumer = building.GetComponent<EnergyConsumer>();
        if (!consumer.IsPowered)
            ctx.Append("No power");
    }
}
```

### The MessageBuilder State Machine

The builder enforces correct formatting via internal states:
- **FRAGMENT**: Plain text, auto-spaced between fragments
- **LIST_ITEM**: Comma-separated list, punctuation handled automatically
- Prevents double-spaces, missing commas, and leading commas

See: `scripts/speech.lua` (MessageBuilder), `scripts/fa-info.lua` (handler chain)

---

## 5. Speech Optimisation for Screen Readers

These rules are subtle but critical for usability:

### Variable Information First

- Good: "Anchored cursor" vs "Unanchored cursor" -- user hears the first word and knows
- Bad: "Cursor status: anchored" -- user must listen to the whole thing

FactorioAccess's CLAUDE.md states:
> "The sooner a message conveying information varies, the faster the user can keep going"

### No Punctuation That Gets Read Literally

Screen readers often read punctuation aloud. Colons become "colon," parentheses become "left paren."

- Avoid: `:`, `()`, em-dashes, unicode symbols
- Prefer: Commas, periods
- Let the MessageBuilder handle list separators

### Terse is Good

- Good: "Facing north"
- Bad: "This entity is currently facing in the north direction"

Blind players using screen readers often set speech rate very high. Brevity respects their time.

### Consistent Patterns

Use the same phrasing structure for the same type of information everywhere. If power status is "No power" in one place, don't make it "Unpowered" in another.

See: `locale/en/` (30+ localisation files showing terse, consistent phrasing)

---

## 6. Audio Overload Prevention

ONI is a game of cascading crises. Without careful throttling, a blind player would drown in alerts. FactorioAccess uses several strategies:

### a) Cooldown-Based Deduplication

Each warning type has a key. The same warning won't repeat within a cooldown period (typically 2 seconds / 120 ticks):

```csharp
// C# equivalent
Dictionary<string, float> lastWarningTime = new();

bool TryWarn(string key, string message) {
    if (Time.time - lastWarningTime.GetValueOrDefault(key) < 2f) return false;
    lastWarningTime[key] = Time.time;
    Speak(message);
    return true;
}
```

See: `scripts/combat.lua` (lines 311-330)

### b) Tick-Offset Distribution

Periodic checks are staggered so they don't all fire on the same frame:

- Mining checks: every 30 ticks, offset 8
- Crafting checks: every 60 ticks, offset 11
- Driving checks: every 15 ticks, offset 2

This prevents audio pile-ups. In Unity:

```csharp
// Stagger checks across frames
if ((Time.frameCount + offset) % interval == 0) { ... }
```

See: `scripts/audio-cues.lua`

### c) Sonification for Streaming Data, Speech for Events

Health doesn't get *spoken* -- it gets *panned* (left ear = empty, right ear = full). Inserters use audio tones for pickup/dropoff. Speech is reserved for discrete, important events.

For ONI:
- **Sonify**: Temperature (pitch), oxygen level (volume/pan), stress (tone)
- **Speak**: "Duplicant idle," "Building complete," "Research finished"

### d) Context Switching

In combat mode, the audio reference point changes from cursor to character, and passive info goes silent.

For ONI: an "alert mode" vs "building mode" that changes what gets announced. During a crisis (flooding, overheating), suppress low-priority building status updates.

### e) Per-Feature Opt-Out Settings

Every sonifier can be individually disabled. Let players control their own audio density. What's helpful for one player may be overwhelming for another.

See: `scripts/sonifiers/` (health-bar, inserter, combat radar)

---

## 7. Spatial Audio: Pan + Low-Pass Filter

FactorioAccess maps spatial position to audio parameters:

- **Pan** (left/right stereo): Encodes horizontal direction
- **Low-pass filter** (muffled sound): Encodes "behind you" -- things behind the player sound muffled through an 800 Hz cutoff
- **Volume falloff**: Encodes distance via inverse distance attenuation

### For ONI's Vertical World

ONI is more vertical than Factorio. Consider:
- **Pan**: Left/right position (same as Factorio)
- **Pitch**: Elevation -- higher tiles = higher pitch
- **Volume**: Distance
- **LPF**: Things in other rooms or off-screen

### Envelope Support

FactorioAccess supports time-varying audio parameters (fade-ins, fade-outs) via an envelope builder. This allows smooth transitions rather than jarring audio cuts.

See: `scripts/sound-model.lua`, `scripts/launcher-audio.lua`

---

## 8. UI as a Navigable Graph (KeyGraph)

FactorioAccess's most sophisticated system. Every menu/UI is a **directed graph** where nodes are controls and edges are WASD movements.

### The Critical Constraint

> Navigating down and right must eventually visit ALL nodes. Up and left can do whatever they want.

This guarantees:
- Every item is reachable via a predictable path
- Search always works (it follows the down-right total order)
- Focus recovery works when items appear/disappear (move to "closest" in graph order)

### Why a Graph?

Menus are trees. Inventories are grids. Crafting screens are categorised rows of varying lengths. Signal selectors are weird tree-like structures. No single data structure (list, tree, grid) handles all of these.

A directed graph with the down-right constraint handles **all of them** uniformly, with a single navigation system and a single search implementation.

### Router: Stack-Based UI Management

A central router manages a stack of open UIs:
- Opening a child UI pushes to the stack
- Closing pops and optionally returns a result to the parent
- UIs can be bound to game state (auto-close if watched entity is destroyed)

### Navigation Keybindings

| Key | Action |
|---|---|
| WASD | Move in graph (up/down/left/right) |
| Ctrl+WASD | Jump to edge (first/last in direction) |
| Shift+WASD | Drag operations |
| Tab / Shift+Tab | Next/previous tab |
| Ctrl+Tab / Ctrl+Shift+Tab | Next/previous section |
| Space / [ / ] | Click / interact |
| E | Close current UI |
| Y | Read detailed info |
| K | Read coordinates |
| Ctrl+F | Search |
| Shift+Enter / Ctrl+Enter | Next/previous search result |

### For ONI with Harmony

You'd intercept Unity's UI system and provide an alternative navigation layer. The graph approach handles ONI's complex UIs (duplicant schedules, research tree, priority screens, overlay settings) far better than trying to linearise arbitrary layouts.

In C#, a proper graph class with LINQ traversal would be cleaner than the Lua table approach.

See: `scripts/ui/key-graph.lua`, `scripts/ui/router.lua`, `scripts/ui/menu.lua`, `scripts/ui/grid.lua`

---

## 9. The Tab System for Complex Entities

When inspecting an entity, FactorioAccess dynamically generates tabs based on what the entity supports:

- Has inventory? -- Inventory tab
- Has circuit connections? -- Circuit tab
- Is an inserter? -- Inserter config tab
- Has equipment grid? -- Equipment tab

Tabs can be enabled/disabled dynamically as state changes (e.g., circuit tab appears only when wires are connected). Each tab maintains its own persistent state.

### Two-Level Tab Navigation

For entities with many tabs (10+), FactorioAccess uses:
- **Tabstops** (major sections): Ctrl+Tab / Ctrl+Shift+Tab
- **Tabs** (views within a section): Tab / Shift+Tab

This prevents overwhelming flat lists.

### For ONI

Inspecting a building could offer tabs like:
- Status (operational state, temperature, disease)
- Errands (pending/active work)
- Plumbing Connections (input/output pipes)
- Automation (logic gate connections)
- Duplicant Assignment (who's assigned here)

Only showing what's relevant to that building type.

See: `scripts/ui/tab-list.lua`, `scripts/ui/entity-ui.lua`, `scripts/ui/tabs/`

---

## 10. Vanilla Mode: The Sighted/Blind Toggle

A single hotkey (Ctrl+Alt+Shift+V) toggles between full accessibility mode and vanilla mouse play.

### What It Does

When enabled:
- Silences speech output
- Hides the keyboard cursor
- Closes all mod UIs
- Re-enables mouse entity selection

What it preserves:
- All player data, bookmarks, state
- Scanner continues updating silently
- Can toggle on/off freely

### Why This Matters

- Sighted helpers can use the same save
- Partially-sighted players can switch modes
- Streamers/teachers can demonstrate both modes
- Testing is easier (quickly verify game state visually)

### For ONI

Design your Harmony patches so the accessibility layer can be fully toggled without breaking game state. Use a whitelist of essential hooks that always run (state tracking) while suppressing speech and keyboard overlay.

See: `scripts/vanilla-mode.lua`, `scripts/event-manager.lua`

---

## 11. State Management: Declared Storage Modules

FactorioAccess uses a pattern where each system declares its own storage with defaults:

```lua
local my_storage = StorageManager.declare_storage_module('my_module', {
    last_selection = nil,
    mode = "default"
})
-- Access: my_storage[player_index].mode
```

### Benefits

- Automatic lazy initialisation with defaults
- Namespace isolation (no conflicts between systems)
- Clear ownership (storage location obvious from usage)
- Migration support (ephemeral versioning clears old data on mod update)

### C# Translation

```csharp
[Serializable]
public class ScannerState {
    public int currentCategory = 0;
    public int currentSubcategory = 0;
    public int currentInstance = 0;
}

// Registered with a central state manager
StateManager.Register<ScannerState>("scanner");

// Access
var state = StateManager.Get<ScannerState>("scanner");
```

Hook into ONI's save system for persistence.

See: `scripts/storage-manager.lua`

---

## 12. Event Management: Multi-Handler with Priorities

Factorio's native event system is last-one-wins. FactorioAccess wraps it with a multi-handler system supporting priority layers:

1. **TEST** handlers (used by test framework)
2. **UI** handlers (get first dibs, can block further processing)
3. **WORLD** handlers (main game logic)

With Harmony, you get this more naturally through prefix/postfix patches with priority attributes. But the principle holds: **UI events should be able to consume inputs before world handlers see them**, and test hooks should be able to intercept anything.

See: `scripts/event-manager.lua`

---

## 13. Localisation as a First-Class Concern

All player-facing text goes through localisation keys, never hardcoded strings.

### Why This Matters for Accessibility

- Different languages have different word orders (the "variable-first" principle needs per-language tuning)
- Screen readers behave differently per language
- Community translations extend reach to blind players worldwide
- Parameterised keys allow translators to reorder: `"Facing {0}"` can become `"{0} facing"` in another language

### Message Lists for Help

FactorioAccess maintains extensive help text as plain `.txt` files that get compiled into localisation entries. A help system (Shift+/) is available from any menu, providing context-sensitive documentation.

For ONI: build a help system from day one. Blind players can't read tooltips or watch tutorial videos.

See: `locale/en/` (30+ files), `helper-scripts/build_message_lists.py`

---

## 14. Testing with Speech Capture

FactorioAccess can capture all spoken messages during tests:

```lua
Speech.start_capture()
-- ... do things ...
local messages = Speech.stop_capture()
assert(messages[1] == "expected announcement")
```

### For Your C# Mod: Build This From Day One

Being able to assert "when the player inspects this building, the speech output includes 'no power'" is invaluable. It catches regressions where a code change silently breaks what blind players hear.

```csharp
SpeechCapture.Start();
InspectBuilding(electrolyzer);
var messages = SpeechCapture.Stop();
Assert.Contains(messages, m => m.Contains("no power"));
```

See: `scripts/speech.lua` (capture system), `scripts/tests/`

---

## 15. Sound Design: Categories and Centralisation

### Audio File Categories (32 files in FactorioAccess)

| Category | Examples | Purpose |
|---|---|---|
| UI Navigation | inventory-move, wrap-around, edge | Menu feedback |
| Player Actions | mine, walk, turn, teleport, crafting | Action acknowledgment |
| Combat | damaged-character, aim-locked, battle-notice | Combat state changes |
| Vehicles | car-horn, train-alert-high/low | Vehicle proximity |
| Sonifiers | enemy radar, inserter hand, crafting complete | Streaming status data |
| Collision | bump-alert, bump-stuck, bump-slide | Physics feedback |

### Centralised Sound API

All sounds are played through named functions in a single module:

```lua
sounds.play_menu_move(pindex)
sounds.play_scanner_pulse(pindex)
sounds.play_train_alert_high(pindex)
```

Benefits:
- Single point to change/debug sound choices
- Prevents scattered one-off audio calls
- Easy to add test mode with sound history tracking

### For ONI

Design a sound palette early:
- Navigation sounds (menu move, wrap, edge)
- Building inspection sounds (open, close, tab switch)
- Alert sounds (tiered by severity: notice, warning, critical)
- Sonification tones (oxygen, temperature, stress)
- Duplicant action sounds (building, digging, idle)

---

## 16. What Changes for ONI/Unity/Harmony

| FactorioAccess Approach | ONI Equivalent |
|---|---|
| External Python launcher for TTS | Direct TTS via Windows SAPI / `System.Speech.Synthesis` or a Unity TTS asset -- no launcher needed |
| Lua module tables | C# classes/interfaces with dependency injection |
| `storage` table persistence | Serialisable state classes hooked into ONI's save system |
| Custom event manager (Factorio is single-handler) | Harmony postfix/prefix patches on ONI's existing event system |
| `data.lua` prototype phase | Not needed -- Harmony patches modify at runtime |
| KeyGraph in Lua tables | A proper C# graph class, potentially with LINQ for traversal |
| 60-tick game loop timing | Unity's `Update`/coroutines, or ONI's `Sim` tick hooks |
| Factorio's `script.on_event` | Harmony patches on `Game.Update`, component `Sim200ms`, etc. |
| Factorio's rich text `[color=red]...[/color]` | Unity's TextMeshPro rich text -- strip before speaking |

---

## 17. Suggested Architecture for an ONI Mod

Based on FactorioAccess patterns, adapted for C#/Harmony:

```
ONIAccessibility/
    Core/
        SpeechEngine.cs          # TTS output (System.Speech or platform API)
        MessageBuilder.cs        # Fluent message composition
        StateManager.cs          # Per-system state with save/load
        AudioManager.cs          # Centralised sound playback
        SpatialAudio.cs          # Pan/LPF/distance calculations
    Scanner/
        WorldScanner.cs          # Hierarchical world browser
        CategoryDefinitions.cs   # Category/subcategory structure
        Clusterer.cs             # Spatial clustering
        ScannerNavigation.cs     # 3-level keyboard navigation
    UI/
        NavigationGraph.cs       # Graph-based UI navigation
        UIRouter.cs              # Stack-based UI management
        TabSystem.cs             # Dynamic tab generation
        MenuBuilder.cs           # Vertical menu construction
        GridBuilder.cs           # 2D grid navigation
    Description/
        IDescriptionContributor.cs  # Handler chain interface
        BuildingDescribers/         # Per-building-type handlers
        DuplicantDescribers/        # Duplicant status handlers
    Audio/
        Sonifiers/               # Continuous audio feedback
        AlertThrottler.cs        # Cooldown-based deduplication
        SoundPalette.cs          # Named sound registry
    Patches/
        InputPatches.cs          # Keyboard input interception
        UIPatches.cs             # UI system hooks
        GameEventPatches.cs      # Building/duplicant event hooks
        SaveLoadPatches.cs       # State persistence
    Config/
        AccessibilitySettings.cs # Per-feature toggles
        KeyBindings.cs           # Customisable controls
    Testing/
        SpeechCapture.cs         # Test harness for speech output
```

---

## 18. Key Principles Summary

1. **Audio-first, not visual-adaptation** -- reimagine the interaction, don't describe the screen
2. **Hierarchical browsing** -- categories, subcategories, instances replace spatial scanning
3. **Direction + distance** -- "5 right and 3 down" not "position (43, 87)"
4. **Handler chains** -- composable description functions, not god functions
5. **Variable information first** -- "Anchored cursor" not "Cursor: anchored"
6. **Terse, punctuation-free messages** -- respect screen reader speed
7. **Cooldown-based throttling** -- prevent audio storms during crises
8. **Sonify streaming data, speak events** -- tones for continuous, words for discrete
9. **Graph-based UI navigation** -- uniform system for all menu types
10. **Dynamic tab generation** -- show only what's relevant
11. **Vanilla mode toggle** -- full accessibility on/off without breaking state
12. **Spatial audio** -- pan, LPF, and distance encode position
13. **Clustering** -- summarise distant/numerous objects, detail nearby ones
14. **Localisation from day one** -- never hardcode player-facing strings
15. **Speech capture testing** -- assert what blind players hear
16. **Per-feature opt-out** -- let players control their audio density

---

## Reference Files in FactorioAccess

| System | Key Files |
|---|---|
| Speech/Audio | `scripts/speech.lua`, `scripts/launcher-audio.lua`, `scripts/sound-model.lua`, `scripts/audio-cues.lua` |
| Scanner | `scripts/scanner/entrypoint.lua`, `scripts/scanner/surface-scanner.lua`, `scripts/scanner/backends/` |
| UI Framework | `scripts/ui/key-graph.lua`, `scripts/ui/router.lua`, `scripts/ui/menu.lua`, `scripts/ui/grid.lua` |
| Entity Info | `scripts/fa-info.lua`, `scripts/item-info.lua`, `scripts/ui/entity-ui.lua` |
| Tabs | `scripts/ui/tab-list.lua`, `scripts/ui/tabs/` |
| Data Structures | `ds/clusterer.lua`, `ds/tile-clusterer.lua`, `ds/sparse-bitset.lua`, `ds/lru-cache.lua` |
| State Management | `scripts/storage-manager.lua` |
| Events | `scripts/event-manager.lua` |
| Spatial Utils | `scripts/fa-utils.lua`, `scripts/viewpoint.lua` |
| Movement | `scripts/walking.lua`, `scripts/driving.lua` |
| Sonifiers | `scripts/sonifiers/health-bar.lua`, `scripts/sonifiers/inserter.lua` |
| Vanilla Mode | `scripts/vanilla-mode.lua` |
| Design Docs | `CLAUDE.md`, `devdocs/`, `llm-docs/`, `docs/` |
| Localisation | `locale/en/` (30+ files) |
