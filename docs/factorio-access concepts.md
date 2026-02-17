# OniAccess: Complete Design Reference

A unified reference for building an accessibility mod for Oxygen Not Included, synthesizing architectural lessons, design patterns, and gameplay analysis derived from the FactorioAccess codebase (~315 Lua files, ~83k lines) and adapted for C#/Unity/Harmony.

---

## Part 1: Core Design Philosophy

### Audio-First, Not Visual Adaptation

FactorioAccess doesn't describe what's on screen — it reimagines the interaction model around audio. A blind player's mental model of their factory is fundamentally different from a sighted player's, and the mod leans into that rather than fighting it.

For ONI, this means: don't try to describe the colony visually. Instead, design systems that let players query what they care about — which duplicants are idle, what's the oxygen situation, where are the unbuilt errands. The colony is a set of systems, not a picture.

### The Five Big Lessons from FactorioAccess

1. **Build a pipeline, not a pile.** Game state flows through describers, then message assembly, then speech output. Each layer has one job. The MessageBuilder's constraints are as important as its capabilities.

2. **Navigation is a graph problem.** Audio UIs need explicit, predictable navigation paths. A directed graph with a total ordering constraint (down-right traversal visits all nodes) is the right foundation. Build higher-level patterns (menus, grids, forms) on top of it.

3. **Scan incrementally, describe specifically.** Never process the whole world at once. Use work queues with per-frame budgets. Give each entity type a specialised describer rather than a generic one.

4. **Let modules own their state.** Declarative state registration beats centralised initialisation. Each module declares what it needs; the system handles creation, defaults, and lifecycle.

5. **Crash loudly, fail never silently.** Internal code should crash on violated preconditions. Validate at system boundaries only. A crash is debuggable; a silent wrong answer to a blind user is not.

### Screen Reader UX Principles

These are universal rules that apply to every piece of speech output in the mod.

**Variable information first.** "Anchored cursor" vs "unanchored cursor" lets the user stop listening after the first word. "Cursor anchored" vs "cursor unanchored" forces them to listen to "cursor" every time. The sooner a message varies, the faster the user can keep going.

**Minimise punctuation.** Screen readers often read colons, parentheses, and dashes verbatim. Use commas and periods only. Let the MessageBuilder handle list separators automatically.

**One utterance per interaction.** The MessageBuilder accumulates all output for an event into a single speak call. No competing or overlapping announcements.

**Terse is good.** Blind players using screen readers often set speech rate very high. "Facing north" beats "This entity is currently facing in the north direction." Brevity respects their time.

**Consistent phrasing.** Use the same structure for the same type of information everywhere. If power status is "No power" in one place, don't make it "Unpowered" in another.

**Hierarchical drill-down over flat lists.** Category, then subcategory, then individual item — rather than a flat list of everything.

---

## Part 2: Technical Foundation

### Platform Comparison

FactorioAccess is written in Lua using Factorio's sandboxed modding API, with an external Python launcher bridging to TTS. ONI is a Unity/C# game where mods are compiled as C# DLLs loaded via Harmony, a runtime method-patching library. Unlike Factorio's defined event hooks, ONI modding gives near-full access to the game's internal classes via reflection and patching — more powerful but more fragile (game updates can break patches).

### What Translates Directly

| FactorioAccess Approach | ONI/C# Equivalent |
|---|---|
| External Python launcher for TTS | Direct TTS via Windows SAPI / System.Speech.Synthesis, or Tolk for cross-screen-reader abstraction — no launcher needed |
| Lua module tables | C# classes/interfaces with dependency injection |
| `storage` table persistence | Serialisable state classes hooked into ONI's save system |
| Custom event manager (Factorio is single-handler) | Harmony postfix/prefix patches on ONI's existing event system |
| `data.lua` prototype phase | Not needed — Harmony patches modify at runtime |
| KeyGraph in Lua tables | A proper C# graph class, potentially with LINQ for traversal |
| 60-tick game loop timing | Unity's Update/coroutines, or ONI's Sim tick hooks |
| Factorio's `script.on_event` | Harmony patches on Game.Update, component Sim200ms, etc. |
| Factorio's rich text | Unity's TextMeshPro rich text — strip before speaking |
| `require()` top-level constraint | No C# equivalent constraint |
| stdout protocol for TTS | P/Invoke or native plugins for direct OS API calls |
| Three loading stages | Unity lifecycle (Awake/Start/Update) |

### World Model: The Fundamental Divergence

This is where the two games diverge most dramatically and where the greatest new invention is needed.

Factorio's world is a flat, top-down grid of square tiles. Everything exists on a single plane. Navigation is north/south/east/west. The cursor moves tile-by-tile and the scanner indexes entities by type, distance, and compass direction.

ONI is also tile-based but uses a side-view (platformer perspective) world with gravity. Duplicants walk on floors, climb ladders, and use poles. Every single tile has multiple overlapping data layers simultaneously: the element occupying it (solid/liquid/gas), its temperature, mass/pressure, germ count and type, decor value, light level, and potentially a building, liquid pipe, gas pipe, automation wire, conveyor rail, and background building all at once.

In FactorioAccess, the cursor reads "Iron ore patch, 47 tiles, 12 tiles northeast." In ONI, a single tile might need to report: "Sandstone tile, 28 degrees, oxygen at 1.8 kilograms, no germs, copper ore debris on ground, insulated liquid pipe carrying polluted water at 32 degrees, automation wire carrying green signal, decor minus 10, in Barracks room." This information density per tile is an order of magnitude greater.

**Consequence:** The FactorioAccess cursor and scanner concepts are a starting point, but ONI requires a fundamentally new approach to information layering and filtering. The player must be able to choose which layer of information they care about at any given moment.

### Direction and Distance, Not Coordinates

Players never hear "position 43, 87." They hear "iron patch, 8 east and 15 south."

FactorioAccess uses biased direction detection: if the absolute X difference exceeds four times the absolute Y difference, it's purely East/West. If the absolute Y difference exceeds four times X, it's purely North/South. Otherwise diagonal. This uses 30-degree cones for cardinals and 60-degree cones for diagonals, making alignment easy to recognise.

**For ONI:** "Electrolyzer, 3 right and 12 down" is far more useful than tile coordinates. ONI's side-view means "north/south" would be confusing — use "left/right/above/below" instead. Consider also using room names when available ("in the barracks, 4 tiles left").

---

## Part 3: Core Architecture

### The Information Pipeline

The central architectural pattern is a layered pipeline transforming raw game state into spoken output:

```
Game State (entities, inventories, positions)
    -> State Readers (Harmony patches or polling that extract game state)
    -> Describers (classes that turn state into structured descriptions)
    -> Message Assembly (MessageBuilder: fluent API for structured text)
    -> Speech Output (SAPI, NVDA, or target screen reader API)
```

No layer reaches past its neighbour. Describers don't call TTS directly. The MessageBuilder doesn't know what an entity is. Speech output doesn't know where its text came from.

### MessageBuilder: Enforcing Audio Constraints at the API Level

The MessageBuilder is a state machine that prevents malformed speech output:

**States:** INITIAL, FRAGMENT, LIST_ITEM, BUILT. Once built, the message cannot be modified.

**Key behaviours:**
- `fragment()` adds space-separated text
- `list_item()` adds comma-separated items
- Passing a bare space crashes deliberately, because double-spacing is always a bug
- `build()` is terminal

The builder's constraints encode domain knowledge about screen reader output — it's not just string concatenation with extra steps.

**C# translation:**

```csharp
public class MessageBuilder {
    private enum State { Initial, Fragment, ListItem, Built }
    private State _state = State.Initial;
    private StringBuilder _sb = new();

    public MessageBuilder Fragment(string text) {
        if (text == " ") throw new ArgumentException("Bare space is always a bug");
        if (_state == State.Fragment || _state == State.ListItem) _sb.Append(' ');
        _sb.Append(text);
        _state = State.Fragment;
        return this;
    }

    public MessageBuilder ListItem(string text) {
        if (_state == State.ListItem) _sb.Append(", ");
        else if (_state == State.Fragment) _sb.Append(' ');
        _sb.Append(text);
        _state = State.ListItem;
        return this;
    }

    public string Build() {
        _state = State.Built;
        return _sb.ToString();
    }
}
```

### Entity Description: The Handler Chain Pattern

Entity descriptions aren't built in one monolithic function. A MessageBuilder is passed through a chain of small handler functions, each of which optionally appends information:

```
ent_info_facing(ctx)        -> "Facing north"
ent_info_power_status(ctx)  -> "No power"
ent_info_belt_contents(ctx) -> (skipped, not a belt)
ent_info_inventory(ctx)     -> "Contains 50 iron plate"
```

Each handler checks its own applicability. Handlers are independently testable. Easy to add new information without touching existing code. Different contexts can use different handler chains (brief vs detailed).

**C# translation:**

```csharp
public interface IDescriptionContributor {
    void Contribute(DescriptionContext ctx, Building building);
}

class PowerStatusContributor : IDescriptionContributor {
    public void Contribute(DescriptionContext ctx, Building building) {
        if (!building.HasComponent<EnergyConsumer>()) return;
        var consumer = building.GetComponent<EnergyConsumer>();
        if (!consumer.IsPowered)
            ctx.Append("No power");
    }
}

// Register per entity type
registry.Register<Furnace>(new FurnaceDescriber());
```

This avoids the 2000-line monolith file. The registry pattern also makes it easy to add describers for modded entities.

### Graph-Based UI Navigation (KeyGraph)

Every menu and UI is modelled as a directed graph where nodes are controls and edges are navigation directions (up/down/left/right).

**The critical constraint:** Down-right traversal must visit ALL nodes. Up and left can do whatever they want. This creates a deterministic total ordering which enables search (find the next node matching text), stable cursor positioning when the graph changes, and consistent tab-cycling.

**Why a graph?** Menus are trees. Inventories are grids. Crafting screens are categorised rows of varying lengths. No single data structure handles all of these. A directed graph with the down-right constraint handles all of them uniformly, with a single navigation system and a single search implementation.

**Higher-level builders construct common patterns on top of the graph:**
- **MenuBuilder**: Vertical lists with optional horizontal rows
- **GridBuilder**: 2D grids with dimension labelling
- **FormBuilder**: Checkboxes, text fields, choice fields, sliders
- **TabList**: Multi-tab containers with shared and per-tab state

**Render-before-use:** Before every event, the graph is re-rendered from scratch. The UI has no painting step — it only describes what to say when asked. This means the graph can change between interactions without stale-state bugs.

**Router: stack-based UI management.** A central router manages a stack of open UIs. Opening a child pushes to the stack. Closing pops and optionally returns a result to the parent. UIs can be bound to game state (auto-close if the watched entity is destroyed).

**Navigation keybindings:**

| Key | Action |
|---|---|
| WASD | Move in graph (up/down/left/right) |
| Ctrl+WASD | Jump to edge (first/last in direction) |
| Tab / Shift+Tab | Next/previous tab |
| Ctrl+Tab / Ctrl+Shift+Tab | Next/previous section |
| Space / [ / ] | Click / interact |
| E | Close current UI |
| Ctrl+F | Search |
| Shift+Enter / Ctrl+Enter | Next/previous search result |

**C# implementation notes:** Use an interface `INavigableNode` with Label, OnClick, and navigation methods. A `NavigationGraph<T>` generic class can enforce the down-right constraint at compile time. Consider caching the total order and invalidating only when structure changes, rather than walking the graph on every render.

### The Tab System for Complex Entities

When inspecting an entity, dynamically generate tabs based on what the entity supports. Has inventory? Inventory tab. Has circuit connections? Circuit tab. Each tab maintains its own persistent state. Tabs can be enabled/disabled dynamically as state changes.

For entities with many tabs, use two-level navigation: tabstops (major sections, Ctrl+Tab) and tabs (views within a section, Tab).

**For ONI,** inspecting a building could offer: Status (operational state, temperature, disease), Errands (pending/active work), Plumbing Connections, Automation, Duplicant Assignment. Only showing what's relevant to that building type.

### Event Management with Priorities

FactorioAccess built an event multiplexer because Factorio only allows one handler per event. C# doesn't have this limitation, but the priority concept is worth keeping:

```
TEST -> UI -> WORLD
```

UI handlers run first and can return "handled" to stop propagation. This solves the core conflict: when a menu is open, pressing W should navigate the menu, not move the cursor. With Harmony, you get this more naturally through prefix/postfix patches with priority attributes.

**Vanilla Mode whitelist:** A set of events that still fire when accessibility features are disabled (tick processing, scanner background work). This lets users toggle the mod without breaking game state. Design your Harmony patches so the accessibility layer can be fully toggled without requiring a game restart.

### State Management: Declared Storage Modules

Each system declares its own storage with defaults rather than relying on centralised initialisation:

```csharp
[Serializable]
public class ScannerState {
    public int currentCategory = 0;
    public int currentSubcategory = 0;
    public int currentInstance = 0;
}

StateManager.Register<ScannerState>("scanner");
var state = StateManager.Get<ScannerState>("scanner");
```

Benefits: automatic lazy initialisation with defaults, namespace isolation, clear ownership, and migration support (version numbers can clear old data on mod update). Hook into ONI's save system for persistence.

### The Anti-Defensive Coding Philosophy

Validation is only allowed at system boundaries (user input, entity validity at the point of user interaction). Internal functions should crash if their preconditions aren't met.

```csharp
// At system boundary (user pressed a key targeting an entity):
if (entity == null || entity.IsDestroyed) { Speak("Nothing here"); return; }

// Internal (entity was already validated):
var behavior = entity.GetBehavior(); // Let it throw if null
foreach (var signal in behavior.Signals) { ... } // Let it throw if null
```

A crash produces a log entry that developers can diagnose. A silent failure (returning empty data) means the user gets incorrect information with no indication that something went wrong. For blind users, this is especially critical — they can't see error messages.

---

## Part 4: The Scanner — Hierarchical World Browsing

This is FactorioAccess's flagship pattern and is critical for ONI.

### The Problem

Thousands of entities in a 2D world. A typical Factorio save has 10,000-20,000 player-placed objects. ONI colonies have hundreds of buildings, dozens of duplicants, and complex pipe/wire networks. You can't list them all, and tile-by-tile exploration is impractical.

### The Solution: Three-Level Hierarchy

- **Categories** (Production, Resources, Military...) — navigate with one keybind
- **Subcategories** (iron furnaces, copper furnaces...) — navigate with another
- **Instances** sorted by distance — navigate with a third

The cursor auto-jumps to whatever you've selected. This turns spatial exploration into list browsing with teleportation.

### Example Workflow

1. Press END (refresh scanner) — "Scanner refreshed"
2. Ctrl+PageDown (categories) — "Resources"
3. PageDown (subcategories) — "Iron ore, 250 ore, 8 east and 15 south"
4. Shift+PageDown (next instance) — "Iron ore, 180 ore, 32 east and 12 north"
5. Home (move to selected) — cursor jumps to that location

### ONI Category Mapping

| FactorioAccess Category | ONI Equivalent |
|---|---|
| Production | Colony buildings by type |
| Resources | Natural resources, geysers |
| Logistics | Plumbing, ventilation, conveyors |
| Power | Generators, batteries, consumers |
| Military | Critter threats, entry points |
| Terrain | Biome boundaries, space exposure |

Subcategories could be building types or status types (buildings needing repair, buildings without power, idle duplicants). Instances sorted by priority or proximity.

### Backend Polymorphism and Clustering

Different entity types need different spatial representations. FactorioAccess clusters them:

- **Trees**: Spatial hash (8x8 chunks) — nearby trees become "forest, 500 trees"
- **Water**: Incremental floodfill — "water body, 250 tiles"
- **Resources**: Patch detection — "iron ore patch, 2400 ore"

Close objects get individual detail; distant objects get summaries. For ONI: "3 lavatories in bathroom zone" rather than listing each one. A natural gas pocket could be "natural gas, 47kg, 3 right and 8 down."

### Performance Strategies

- **Event-driven detection**: Hook into creation events for immediate new-entity awareness
- **Work queues with per-frame budgets**: Process 100 new entities per tick, not all at once. In Unity, use coroutines with `yield return null` for frame-spreading, Jobs/Burst for parallel processing, or async/await with frame-budget checking
- **Lazy updates**: Heavy work happens only when announcing an entry, not during every scan
- **Bitsets** for memory-efficient entity tracking (32x savings over hash tables for dense sequential IDs)
- **Distance-sorted results** using efficient sorting for mostly-sorted data
- **Deferred refresh**: Play an acknowledgement sound immediately, then set a short delay before rebuilding data. The user hears confirmation of their action while the expensive work happens in the background

Unity's physics system gives you spatial queries (OverlapSphere, etc.) that Factorio doesn't have, so you may not need chunk-based scanning at all.

---

## Part 5: Audio Systems

### Speech Output

All speech goes through a single `Speech.Speak()` method. Never sprinkle TTS calls throughout the code.

In Unity you have multiple options: direct OS API calls via P/Invoke (SAPI on Windows, NSAccessibility on Mac), the Tolk library for cross-screen-reader abstraction, or named pipes/IPC if you need a launcher. The principle of an abstraction layer between your mod and the TTS output is sound regardless of implementation.

### Spatial Audio and Sonification

Speech alone is too slow for real-time game feedback. Players need continuous, non-verbal awareness without waiting for screen reader announcements.

**Spatial audio parameters:**
- **Pan** (left/right stereo): Encodes horizontal direction
- **Low-pass filter** (muffled sound): Encodes "behind you" via 800 Hz cutoff
- **Volume falloff**: Encodes distance via inverse distance attenuation

**For ONI's vertical world:**
- **Pan**: Left/right position (same as Factorio)
- **Pitch**: Elevation — higher tiles equal higher pitch
- **Volume**: Distance
- **LPF**: Things in other rooms or off-screen

**The rule:** Sonify streaming data, speak events. Health doesn't get spoken — it gets panned (left ear empty, right ear full). Inserters use audio tones for pickup/dropoff. Speech is reserved for discrete, important events.

### Audio Overload Prevention

ONI is a game of cascading crises. Without careful throttling, a blind player would drown in alerts.

**Cooldown-based deduplication:** Each warning type has a key. The same warning won't repeat within a cooldown period (typically 2 seconds):

```csharp
Dictionary<string, float> lastWarningTime = new();

bool TryWarn(string key, string message) {
    if (Time.time - lastWarningTime.GetValueOrDefault(key) < 2f) return false;
    lastWarningTime[key] = Time.time;
    Speak(message);
    return true;
}
```

**Tick-offset distribution:** Periodic checks are staggered so they don't all fire on the same frame. Mining checks every 30 ticks at offset 8, crafting checks every 60 ticks at offset 11, driving checks every 15 ticks at offset 2. In Unity: `if ((Time.frameCount + offset) % interval == 0) { ... }`

**Context switching:** An "alert mode" vs "building mode" that changes what gets announced. During a crisis (flooding, overheating), suppress low-priority building status updates.

**Per-feature opt-out settings:** Every sonifier can be individually disabled. Let players control their own audio density.

### Sound Design: Categories and Centralisation

All sounds are played through named functions in a single module. This gives a single point to change/debug sound choices, prevents scattered one-off audio calls, and enables test mode with sound history tracking.

**Design a sound palette early:**
- Navigation sounds (menu move, wrap, edge)
- Building inspection sounds (open, close, tab switch)
- Alert sounds (tiered by severity: notice, warning, critical)
- Sonification tones (oxygen, temperature, stress)
- Duplicant action sounds (building, digging, idle)

**Deferred refresh with sound-first feedback:** When the user presses a scanner key, play a sound immediately and set a short delay before rebuilding data. Always acknowledge user input instantly (a click sound, a "searching..." utterance) before doing expensive work. This micro-optimisation makes a huge difference for perceived responsiveness.

---

## Part 6: ONI-Specific Systems

### Cursor and Navigation

The cursor operates on the tile grid, independent of any game character's position. ONI navigation needs up/down movement (ladders, poles, open space) in addition to left/right.

**Default readout** says "Granite tile, Manual Generator." Pressing a layer key adds "Oxygen, 1.8 kg, 24 C." Another layer key adds "Power wire, 960W load." This layer-query system has no parallel in FactorioAccess.

**Cursor skip** (Shift+direction): Skip up to 100 tiles, stopping at the first "change" — different building type, biome boundary, pipe junction, room boundary.

**Cursor anchoring:** When anchored, the cursor automatically moves ahead of the player character's focus. When unanchored, it stays in place.

**Building-to-building navigation (BStride):** Jump between placed buildings as if they were nodes in a grid. Extremely applicable to ONI, where a blind player benefits enormously from jumping between buildings of the same type or within the same pipe/wire network. Needs ONI-specific implementation for vertical layout.

**Entity selection on overlapping tiles:** When multiple entities overlap on a tile, provide a deterministic sort with a priority system and stable total ordering. Cycling through entities is predictable. In ONI, this is complicated by the multiple network layers coexisting on the same tile (building, liquid pipe, gas pipe, automation wire, conveyor rail, power wire).

### The Overlay and Environmental Awareness System

This is the single most critical new invention for ONI and has no FactorioAccess equivalent. It comprises three integrated subsystems.

#### Togglable Speech Overlays

ONI has approximately 15 visual overlays: oxygen, power, temperature, liquid plumbing, gas ventilation, decor, germs, light, rooms, farming, priority, exosuit, automation, conveyor, material, and radiation (DLC).

Each overlay becomes a togglable information layer with two interaction modes:

**Query mode (default):** The overlay is off. The player presses a key to hear the overlay data at the current cursor tile on demand. For example, pressing the oxygen overlay key once reads: "Oxygen, 1.8 kilograms, 25 degrees."

**Continuous mode (toggled on):** The overlay is active. As the cursor moves, overlay data is automatically announced at each new tile. Multiple overlays can be toggled on simultaneously and their information concatenated: "Oxygen 1.8 kilograms. Insulated liquid pipe, polluted water, 32 degrees."

The player controls information density by choosing which overlays are active. Investigating a plumbing problem? Toggle on liquid plumbing and temperature. Checking atmosphere? Toggle on oxygen. Building automation? Toggle on automation.

Overlay hotkeys should match existing game overlay keys (F1 through F12) so guides and wikis remain applicable. A modifier key toggles continuous mode.

#### Atmospheric Sonification Layer

Speech overlays solve the detail problem but not the big-picture problem. Reading tile data sequentially doesn't give the player a gestalt sense of atmospheric distribution. For this, the mod provides sonification — non-speech audio that maps atmospheric conditions to sound.

When active, the cursor becomes a sonic probe. Moving it through the world plays continuous audio reflecting the local atmosphere.

**Gas type as timbre (3 tones plus silence):**
- **Oxygen:** Clean, neutral mid-range hum — the "everything is fine" baseline
- **Carbon dioxide:** Distinct low rumble — the most common threat, immediately recognisable
- **Foreign gas (everything else):** A single shared alert tone. When heard, the player knows something unexpected is present and can press a key for specific identification
- **Vacuum:** Silence

This three-tone-plus-silence design keeps memorisation load minimal.

**Pressure as volume:** High pressure is louder, low pressure is quieter. The player can hear dead zones and overpressurised areas without speech.

**Temperature as texture (optional):** Warm crackle at high temperatures, crystalline shimmer at cold temperatures, layered on top of the gas tone. Toggled independently.

**The sweep pattern:** The player activates sonification and sweeps the cursor horizontally across a level. In a few seconds they hear: full oxygen hum... getting quieter... foreign gas alert... CO2 rumble... silence. They now know: oxygen is present on the left, thinning, something unexpected in the middle, CO2 pooling further right, vacuum beyond. Vertical sweeps reveal gas stratification. This is the closest equivalent to glancing at the oxygen overlay.

**Focused modes:** When working with a specific gas (natural gas power setup, chlorine disinfection), sonification can track that gas specifically. Natural gas mode: hum when present, silence when absent, pressure as volume.

#### Game-State Problem Detection

Neither speech overlays nor sonification solve proactive awareness — knowing something is going wrong before crisis. The mod hooks into ONI's own entity status system.

Every ONI entity already evaluates its own environmental conditions. Plants track atmosphere and temperature and flag "stifled" or "wrong atmosphere." Buildings track overheat thresholds. Pipes know when contents might change state and burst. Duplicants track breath, comfort, and stress. The game already computes all of this internally.

The mod surfaces these status flags as an ongoing problem report, asking each entity whether it considers its conditions problematic using the game's own definitions rather than hardcoded thresholds:

"3 Mealwood plants stifled by temperature." "Electrolyzer reporting overpressure." "2 duplicants holding breath." "Aquatuner approaching overheat, 12 degrees margin remaining."

#### How the Three Layers Work Together

**Sonification (proactive, big picture):** "How is my base's atmosphere right now?" Rapid cursor sweeps give spatial patterns. The primary tool for strategic atmospheric awareness.

**Speech overlays (detailed, investigative):** "What exactly is happening at this tile?" Toggle on relevant overlays and move through an area for precise data. Used when diagnosing a specific problem or planning a build.

**Problem detection (reactive, safety net):** "What is already going wrong?" Continuous monitoring of entity status flags catches problems the player hasn't noticed. Runs in the background.

Together, these replace the visual overlay with a layered awareness model: sonification for spatial intuition, speech for precision, and entity status for safety.

#### Known Limitations and Open Questions

**Discovery gap:** Passive discovery of features outside the player's scan path is weaker than visual. The scanner helps by allowing targeted searches for gas types or resources.

**Information overload:** Toggling three or four speech overlays simultaneously could produce long readouts per tile. The system may need a brevity mode with abbreviated multi-overlay readouts.

**Cursor sweep speed:** Sonification effectiveness depends on cursor movement speed and audio update responsiveness. Sonification mode may need to suppress speech and allow faster cursor traversal.

### Duplicant Management

Factorio has no equivalent — you are a single engineer. ONI requires managing 3 to 30+ autonomous agents. This is an entirely new domain.

**Status monitoring:** Each duplicant has health, stress, calories, bladder, breath, stamina, body temperature, current errand, current location, 11 attributes, skills, traits, morale, and active status effects. The mod needs a "follow duplicant" mode and a "duplicant status query" system.

**Priority system:** ONI's most complex management mechanic. Two levels: per-duplicant task-type priorities (a grid of duplicant rows vs task-type columns with priority levels from disabled through low/medium/high/very high), and per-building/errand sub-priorities (numeric 1-9). The mod must make the grid navigable: "Turner. Research: Very High. Building: Medium. Farming: Disabled."

**Schedule system:** Time blocks (Work, Downtime, Bathtime, Sleep) assigned to schedule groups across 24 time-slots. A new navigable schedule editor is needed.

**Skills and attributes:** Skill trees with point-based assignment. Could partially adapt the research tree navigation concept.

**Duplicant selection (Printing Pod):** Every 3 cycles, the Printing Pod offers a choice of new duplicants with randomised stats, traits, and interests. A new comparison UI is needed.

### Building and Construction

**Build menu:** ONI's build menu is heavily mouse-driven with subcategories (Base, Oxygen, Power, Food, Plumbing, Ventilation, etc.). The mod must convert this into a navigable audio menu.

**Port awareness:** ONI buildings have specific input/output port positions. The mod must announce: "Liquid input on left, liquid output on right, power connection on bottom."

**Multi-network conduit awareness:** ONI has five separate conduit networks that all overlap in the same tile: liquid pipes, gas pipes, automation wire, conveyor rails, and power wire. Each placed independently. The player needs to know what's already in a tile across all networks before placing new conduit.

**Drag-to-build:** ONI heavily uses drag-to-place for tiles, pipes, wires, and dig/mop/sweep commands. The mod needs a "start drag, move cursor, end drag" paradigm with audio feedback for the area being selected. Also needed for priority painting.

### Rooms System

ONI recognises enclosed areas as "rooms" if they meet specific criteria. Rooms provide morale bonuses. The mod needs to: announce when the cursor enters a recognised room, report room type and bonus, report missing requirements ("This Mess Hall needs a Decor item to upgrade to a Great Hall"), and support the room overlay as an information layer.

### Colony Management Screens

ONI has several management screens: Vitals (V), Consumables (F), Priority List (L), Schedule (.), Skills (J), Research (R), Report (E), Starmap (Z), Codex/Journal (U). Each is a complex data-rich screen requiring full audio navigation.

### Resource and Inventory Differences

ONI duplicants don't have traditional inventories. They automatically carry items as needed. The player manages storage buildings (Storage Compactors, Ration Boxes) and their filter settings. All production happens in buildings that duplicants operate based on priority. ONI has a global resource panel showing total colony resources by type. No personal hand-crafting exists.

---

## Part 7: Concept-by-Concept Mapping

### Direct Mappings (Moderate Adaptation Work)

1. TTS/speech bridge architecture
2. Tile-by-tile cursor movement (add vertical axis)
3. Fast travel/bookmarks
4. Building-to-building navigation (BStride)
5. Building placement on grid
6. Pipe/wire tile-by-tile routing
7. Drag-to-build operations
8. Deconstruct/cancel tools
9. Research tree and queue navigation
10. Automation wiring (from circuit network support)
11. Alert notification cycling
12. Building status readout
13. Coordinate and distance reporting
14. Entity description readout

### Adapted Concepts (Significant Rework)

1. Scanner tool — needs ONI-specific categories, vertical awareness, and much richer grouping
2. Cursor information readout — needs multi-layer support
3. Build menu navigation — ONI's category hierarchy is deeper
4. Building configuration — ONI has more complex per-building settings
5. Resource tracking — colony-wide rather than personal inventory
6. Walking/movement modes — duplicants are autonomous; cursor movement replaces player movement

### Entirely New Inventions Required

1. Overlay and environmental awareness system (the three-layer design: togglable speech overlays, atmospheric sonification, game-state problem detection)
2. Duplicant management: status monitoring, following, selection
3. Priority system navigation (the duplicant-by-task-type grid)
4. Schedule editor (24-slot time-block grid)
5. Skills/attribute tree navigation and assignment
6. Room recognition and requirement reporting
7. Duplicant selection at Printing Pod (stat comparison UI)
8. Critter/ranch management
9. Multi-network conduit awareness (5 overlapping networks on same tiles)
10. Colony management screens (Vitals, Consumables, Reports, etc.)
11. Duplicant bio panel navigation
12. Starmap navigation (DLC)

---

## Part 8: Testing and Quality

### Dual Test Strategy

**In-engine tests:** Use Unity Test Framework's play-mode tests. These run inside the game loop and can test Harmony patch behaviour, game state interactions, and timing. The tick-based scheduling pattern translates to `yield return null` in Unity coroutine tests.

**Pure logic tests:** Use NUnit/xUnit for your MessageBuilder, describers, navigation graph, and other pure-logic code.

### Speech Capture Testing

Build this from day one. Being able to assert "when the player inspects this building, the speech output includes 'no power'" catches regressions where a code change silently breaks what blind players hear.

```csharp
SpeechCapture.Start();
InspectBuilding(electrolyzer);
var messages = SpeechCapture.Stop();
Assert.Contains(messages, m => m.Contains("no power"));
```

### Build Tooling

Adopt the principle: a single command (or CI step) that runs every quality gate. A developer should never need to remember "also run the linter." Consider a script that chains: format check, Roslyn analysers, unit tests, play-mode tests, locale lint.

### Localisation

All player-facing text goes through localisation keys, never hardcoded strings. Build (or find) a tool that cross-references localisation keys against usage in code — in a project with hundreds of spoken strings, orphaned and missing keys are constant problems.

Parameterised keys allow word order to change in translations: `"Facing {0}"` can become `"{0} facing"` in another language. Different languages have different word orders, so the "variable-first" principle needs per-language tuning. Build a help system from day one — blind players can't read tooltips or watch tutorial videos.

---

## Part 9: Development Phases

### Phase 1: Core Navigation and Awareness

Implement cursor movement, tile readout with basic layer support, scanner tool, bookmarks, and the TTS bridge. This gets a player moving around the world and understanding what is where.

### Phase 2: Building and Construction

Implement the build menu audio navigation, building placement, pipe/wire routing, and deconstruct. This lets the player actually build things.

### Phase 3: Duplicant Management

Implement duplicant status queries, priority system navigation, schedule editor, and skills. This lets the player manage their colony.

### Phase 4: Environmental Monitoring

Implement the full overlay and environmental awareness system: togglable/combinable speech overlays, atmospheric sonification, and game-state problem detection. Implement room reporting. This is the most critical phase for mid-to-late-game viability and requires the most iteration through playtesting.

### Phase 5: Advanced Systems

Implement automation configuration, sensor sliders, research navigation, critter management, and colony management screens.

### Phase 6: DLC Content

Implement rocketry, starmap, and multi-asteroid management.

---

## Part 10: Suggested Project Architecture

```
OniAccess/
    Core/
        SpeechEngine.cs          # TTS output (System.Speech or platform API)
        MessageBuilder.cs        # Fluent message composition with state machine
        StateManager.cs          # Per-system state with save/load
        AudioManager.cs          # Centralised sound playback
        SpatialAudio.cs          # Pan/LPF/distance calculations
    Scanner/
        WorldScanner.cs          # Hierarchical world browser
        CategoryDefinitions.cs   # Category/subcategory structure
        Clusterer.cs             # Spatial clustering
        ScannerNavigation.cs     # 3-level keyboard navigation
    UI/
        NavigationGraph.cs       # Graph-based UI navigation with down-right constraint
        UIRouter.cs              # Stack-based UI management
        TabSystem.cs             # Dynamic tab generation
        MenuBuilder.cs           # Vertical menu construction
        GridBuilder.cs           # 2D grid navigation
        FormBuilder.cs           # Checkboxes, sliders, text fields
    Description/
        IDescriptionContributor.cs  # Handler chain interface
        BuildingDescribers/         # Per-building-type handlers
        DuplicantDescribers/        # Duplicant status handlers
    Overlays/
        OverlayManager.cs        # Toggle/query mode controller
        SpeechOverlays/          # Per-overlay speech formatters
        AtmosphericSonifier.cs   # 3-tone gas mapping
        ProblemDetector.cs       # Entity status flag monitoring
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
        VanillaMode.cs           # Full accessibility on/off toggle
    Testing/
        SpeechCapture.cs         # Test harness for speech output
    Localisation/
        Keys/                    # All locale key definitions
        HelpContent/             # Context-sensitive help text
```

---

## Appendix: Key Principles Summary

1. **Audio-first, not visual-adaptation** — reimagine the interaction, don't describe the screen
2. **Hierarchical browsing** — categories, subcategories, instances replace spatial scanning
3. **Direction plus distance** — "5 right and 3 down" not "position (43, 87)"
4. **Handler chains** — composable description functions, not god functions
5. **Variable information first** — "Anchored cursor" not "Cursor: anchored"
6. **Terse, punctuation-free messages** — respect screen reader speed
7. **Cooldown-based throttling** — prevent audio storms during crises
8. **Sonify streaming data, speak events** — tones for continuous, words for discrete
9. **Graph-based UI navigation** — uniform system for all menu types
10. **Dynamic tab generation** — show only what's relevant
11. **Vanilla mode toggle** — full accessibility on/off without breaking state
12. **Spatial audio** — pan, LPF, and distance encode position
13. **Clustering** — summarise distant/numerous objects, detail nearby ones
14. **Localisation from day one** — never hardcode player-facing strings
15. **Speech capture testing** — assert what blind players hear
16. **Per-feature opt-out** — let players control their audio density
17. **Crash loudly** — validate at boundaries, let internal code throw
18. **Deferred refresh** — acknowledge input instantly, compute afterward
