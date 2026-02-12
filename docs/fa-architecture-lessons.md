# Architectural Lessons from FactorioAccess

An analysis of FactorioAccess's design for someone building a game accessibility mod in C#/Unity with Harmony patches. Each section identifies a design choice, explains the problem it solves, and recommends whether to adopt, adapt, or avoid it.

---

## 1. The Information Pipeline: Game State to Speech

### What they built

FactorioAccess has a clear, layered pipeline that transforms raw game state into spoken output:

```
Game State (entities, inventories, positions)
    -> Information Layer (fa-info.lua: 2263 lines of entity describers)
    -> Message Assembly (MessageBuilder: fluent API for structured text)
    -> Speech Output (Speech.speak -> stdout -> external launcher -> TTS)
```

No layer reaches past its neighbor. Entity describers don't call TTS directly. The MessageBuilder doesn't know what an entity is. Speech output doesn't know where its text came from.

### Why it's shaped this way

The mod serves screen reader users, where the output is a single serial stream of text. You can't layer information spatially the way a GUI can. Every interaction must produce exactly one coherent utterance, assembled from multiple sources (the entity type, its direction, its status, its contents). The pipeline ensures each layer contributes its piece without stepping on others.

The MessageBuilder enforces audio-first constraints at the API level:
- `fragment()` adds space-separated text
- `list_item()` adds comma-separated items
- Passing a bare space `" "` to `fragment()` crashes deliberately, because double-spacing is always a bug
- `build()` is terminal: the message can't be modified after being finalized
- The state machine (INITIAL -> FRAGMENT -> LIST_ITEM -> BUILT) prevents malformed output

### Recommendation: **Adopt**

This pipeline maps directly to your C#/Unity project. Your layers would be:

1. **State Readers** (Harmony patches or polling that extract game state)
2. **Describers** (classes that turn state into structured descriptions)
3. **Message Assembly** (a C# MessageBuilder equivalent)
4. **TTS Output** (SAPI, NVDA, or whatever your target screen reader API is)

The MessageBuilder pattern is especially worth porting. A C# version with the same state machine and crash-on-space behavior would catch the same class of bugs. The key insight is that the builder's constraints encode domain knowledge about screen reader output — it's not just string concatenation with extra steps.

---

## 2. Graph-Based UI Navigation

### What they built

The UI system is built on **directed graphs** (KeyGraph), where nodes are controls and edges are navigation directions (up/down/left/right). This is described in the code as "UI assembly language." On top of this, higher-level builders construct common patterns:

- **MenuBuilder**: Vertical lists with optional horizontal rows
- **GridBuilder**: 2D grids with dimension labeling
- **FormBuilder**: Checkboxes, text fields, choice fields
- **TabList**: Multi-tab containers with shared and per-tab state

The graph has a critical constraint: **down-right traversal must visit all nodes**. This creates a deterministic total ordering, which enables search ("find the next node matching this text"), stable cursor positioning when the graph changes, and consistent tab-cycling.

### Why it's shaped this way

A sighted user can scan a 2D GUI instantly. A screen reader user navigates one item at a time. The graph structure makes navigation paths explicit — there's no ambiguity about what "press down" does from any position. The total ordering from the down-right constraint means search results are predictable and the system can always find a stable fallback position.

The render-before-use pattern is also notable: before every event, the graph is re-rendered from scratch. This means the graph can change between interactions (entities die, inventories change) without stale-state bugs. There's no explicit "re-render" call because the UI has no painting step — it only describes what to say when asked.

### Recommendation: **Adapt**

The graph concept is excellent, but Lua's lack of type safety makes the raw graph definition verbose and error-prone. In C#, you'd likely want:

- An interface `INavigableNode` with `Label`, `OnClick`, navigation methods
- A `NavigationGraph<T>` generic class that enforces the down-right constraint at compile time
- Builder classes that construct graphs (your equivalents of MenuBuilder/GridBuilder)
- The render-before-use pattern via a `Render()` method called before any event dispatch

The total ordering constraint is the real gem — adopt it exactly. It solves a class of problems (search, cursor stability, tab cycling) that you'll otherwise have to solve individually.

One thing to avoid: their `key_order` is computed by walking the graph on every render. In C# with larger UIs, consider caching the order and invalidating it only when structure changes.

---

## 3. Event Multiplexing with Priority Levels

### What they built

Factorio's API allows only one handler per event (`script.on_event` is last-one-wins). The EventManager wraps this with a multiplexing layer that supports three priority levels per event:

```
TEST -> UI -> WORLD
```

Each event can have at most one handler per priority. Handlers run in priority order and can return `FINISHED` to stop propagation.

### Why it's shaped this way

The priority system solves two problems:
1. **UI vs world conflict**: When a menu is open, pressing W should navigate the menu, not move the cursor. The UI handler runs first and returns FINISHED.
2. **Test isolation**: Tests can register handlers at TEST priority without modifying production handlers.

The "one handler per priority" constraint prevents the accidental stacking that makes traditional event buses hard to debug.

### Recommendation: **Adapt significantly**

C# has proper events, delegates, and can support multiple handlers natively. You don't need the multiplexing workaround. But the priority concept is worth keeping:

```csharp
[EventPriority(Priority.UI)]
void OnMoveUp(MoveEvent e) {
    if (menuOpen) { e.Handled = true; NavigateMenu(); }
}

[EventPriority(Priority.World)]
void OnMoveUp(MoveEvent e) {
    MoveCursor(Direction.North);
}
```

The Vanilla Mode whitelist (a set of events that still fire when accessibility features are disabled) is also worth adopting. It lets users toggle the mod without breaking game state.

---

## 4. Storage Manager: Declarative Per-Player State

### What they built

StorageManager provides namespaced, lazily-initialized per-player (or per-surface) state:

```lua
local my_state = StorageManager.declare_storage_module('scanner', {
    cursor = nil,
    entries = {},
})

-- Access auto-creates and returns: storage.players[pindex].scanner
local data = my_state[pindex]
```

This uses Lua metamagic (`__index`/`__newindex`) to intercept access and lazily initialize from defaults. It also supports:
- **Ephemeral state versioning**: bump a version number to clear all cached state on mod update
- **Root field selection**: `players` vs `surfaces` vs custom
- **Iteration** via `__pairs`/`__ipairs`

### Why it's shaped this way

With 250+ modules and dozens of per-player state fields, the alternative is a massive initialization function that every module must remember to update. StorageManager inverts the dependency: each module declares its own state, and initialization happens on first access. This eliminates ordering bugs and forgotten initialization.

The ephemeral versioning is clever: when the scanner's internal data format changes, bumping the version number clears all scanner state on next load rather than requiring migration code.

### Recommendation: **Adopt the concept, use C# idioms**

In C#, this maps to a generic state container:

```csharp
public class PlayerState<T> where T : new() {
    private readonly Dictionary<int, T> _states = new();
    public T this[int playerId] => _states.GetOrAdd(playerId, _ => new T());
}

// In your scanner module:
private static readonly PlayerState<ScannerState> State = new();
```

The ephemeral versioning pattern could be a `[StateVersion(12)]` attribute on your state class, checked on load. The key lesson is: let each module own its state declaration rather than centralizing it.

---

## 5. The Scanner: A Streaming Spatial Database

### What they built

The scanner is a three-tier system that continuously indexes the game world:

1. **Entrypoint**: User-facing cursor navigation through scan results (category -> subcategory -> individual entry)
2. **Surface Scanner**: Per-surface chunk-based entity tracking with deduplication via SparseBitset
3. **Backends**: Pluggable handlers for different entity types (SimpleBackend, ResourcePatchesBackend, TreeBackend, WaterBackend, etc.)

Key design decisions:
- **Chunk-based incremental scanning** via WorkQueue (4 chunks/tick, prioritized by distance to player)
- **Distance-sorted results** using memosort (O(n) for mostly-sorted data)
- **Deferred refresh**: pressing a scanner key sets a 2-tick delay so sound plays immediately while data rebuilds
- **Three-level navigation**: Category (resources/logistics/enemies) -> Subcategory (iron-ore/copper-ore) -> Individual entries

### Why it's shaped this way

Factorio worlds can have millions of entities. A naive "find everything" approach would freeze the game. The chunk-based work queue spreads the cost across many ticks. The backend polymorphism means trees can be spatially clustered into "forests" while ore deposits cluster into "patches" — each type gets the representation that makes audio sense, not just what the game API gives you.

The three-level navigation is a direct response to the audio serial-stream constraint. A sighted player scans hundreds of entities visually in an instant. An audio user needs hierarchical drill-down: "resources" -> "iron ore" -> "nearest iron ore patch, 47 tiles north."

### Recommendation: **Adapt the architecture, redesign the specifics**

The three principles to carry over:
1. **Incremental processing**: Never scan everything in one frame. Use a work queue pattern with per-frame budgets.
2. **Backend polymorphism**: Different entity types need different spatial representations. Define a backend interface and implement per-type.
3. **Hierarchical navigation**: Category -> subcategory -> instance is the right shape for audio.

What changes for Unity:
- Unity's physics system gives you spatial queries (OverlapSphere, etc.) that Factorio doesn't have. You may not need chunk-based scanning.
- C# async/await or coroutines could replace the tick-based work queue more elegantly.
- The memosort optimization may not be needed if you can use SortedSet or a spatial index.

---

## 6. WorkQueue: Amortized Expensive Operations

### What they built

A simple but effective pattern for spreading work across game ticks:

```lua
local queue = WorkQueue.declare_work_queue({
    name = "scanner_entities",
    per_tick = 100,
    worker_function = process_entity,
    idle_function = repopulate_queue,  -- called when queue empties
})
```

Each tick, up to `per_tick` items are dequeued and processed. When the queue empties, `idle_function` refills it. The queue state persists in save data.

### Why it's shaped this way

Game mods can't block the game loop. Any operation that touches hundreds of entities must be chunked. The WorkQueue abstracts this into a reusable pattern. The `idle_function` creates an infinite background scanning loop: scan chunks -> process entities -> queue empties -> rescan.

The deferred entity processing (100 entities/tick for new entities) also works around a Factorio API limitation: querying the surface during entity creation events can crash.

### Recommendation: **Adapt**

In Unity, you have more options:
- **Coroutines** with `yield return null` for frame-spreading
- **Jobs/Burst** for parallel processing
- **async/await** with frame-budget checking

But the declarative pattern (declare a queue with a budget and a worker) is clean and worth keeping as an abstraction even if the implementation differs. The `idle_function` pattern for continuous background work is particularly elegant.

---

## 7. Module Organization and the Big Controller Problem

### What they built

`control.lua` is 4183 lines and still the main event registration hub. It imports 79 modules and wires them to events. Over time, domain logic has been extracted into modules:

- `scripts/scanner/` — entity discovery
- `scripts/ui/` — all UI components (30+ files)
- `scripts/combat/` — combat system
- `scripts/building-tools.lua` — placement logic
- `scripts/driving.lua` — vehicle control

But control.lua still contains: movement logic, entity cycling, hand/cursor reading, player join/init orchestration, and the majority of keybinding implementations.

### Why it's shaped this way

Factorio requires a single `control.lua` entry point. The mod has evolved over years, and extracting logic is ongoing. The EventManager was built specifically to enable this migration — it lets modules register their own handlers rather than requiring everything to flow through control.lua.

### Recommendation: **Avoid the monolith, adopt the extraction pattern**

In C#/Unity, you have no constraint forcing a single entry point. Use it:

- Each system should register its own Harmony patches
- Each system should subscribe to its own events
- A thin orchestrator (like a MonoBehaviour or static initializer) should only wire systems together, not contain logic

The EventManager pattern of "modules register themselves" is the right end state. Start there rather than migrating toward it.

---

## 8. Entity Description: The fa-info.lua Approach

### What they built

`fa-info.lua` (2263 lines) is the largest module after control.lua. It contains specialized description functions for every entity type: furnaces, assemblers, belts, inserters, trains, circuits, pipes, reactors, etc. Each function produces a structured description using MessageBuilder.

Entity descriptions are built in layers:
- **Facing/direction** (skipped for non-rotatable entities)
- **Status** (idle, working, no power, etc.)
- **Contents** (inventory, fluid, signal values)
- **Connections** (circuit networks, pipe connections)

### Why it's shaped this way

The mod can't use a generic "toString" for entities. A furnace needs to announce its recipe, fuel level, and output. A belt needs to announce its contents and direction. A circuit combinator needs to announce its conditions and signals. Each entity type has a fundamentally different information shape.

The 2263-line file is a consequence of Factorio having hundreds of entity types, each with unique properties. It's not poor factoring — it's inherent complexity that has to live somewhere.

### Recommendation: **Adopt the pattern, improve the organization**

In C#, use a registry of describers:

```csharp
public interface IEntityDescriber<T> {
    void Describe(T entity, MessageBuilder message, DescribeContext ctx);
}

// Register per entity type
registry.Register<Furnace>(new FurnaceDescriber());
registry.Register<Belt>(new BeltDescriber());
```

This avoids the 2000-line file while keeping the same principle: each entity type gets a specialized describer. The registry pattern also makes it easy to add describers for modded entities in your target game.

---

## 9. Entity Selection and Stable Ordering

### What they built

When multiple entities overlap on a tile, `EntitySelection` provides a deterministic sort:

```
Priority 2: Characters, enemies
Priority 1: Vehicles
Priority 0: Most buildings (default)
Priority -1: Rails
Priority -2: Robots, corpses, resources
```

Within a priority, ties are broken by: ghost status -> unit number -> mining target -> prototype name -> item name -> registration order. This gives a **stable total ordering** so that cycling through entities on a tile is predictable.

### Why it's shaped this way

A sighted player sees all entities on a tile simultaneously. An audio user cycles through them one at a time. Without stable ordering, the "next entity" changes unpredictably as entities are created/destroyed, which is disorienting. The priority system ensures the most important entity is announced first.

### Recommendation: **Adopt**

You'll need this in any game with overlapping objects. Implement `IComparable` on your entity wrapper with a priority system. The specific priorities will differ for your game, but the principle — deterministic ordering with the most important entity first — is universal.

---

## 10. The Anti-Defensive Coding Philosophy

### What they built

The CLAUDE.md explicitly forbids excessive null-checking:

```lua
-- WRONG (hides bugs):
if entity and entity.valid then
   local cb = entity.get_control_behavior()
   if not cb then return {} end
   for _, section in ipairs(cb.sections or {}) do ...

-- CORRECT (crashes to expose bugs):
local cb = entity.get_control_behavior()
for _, section in ipairs(cb.sections) do ...
```

Validation is only allowed at system boundaries (user input, entity validity at the point of user interaction). Internal functions should crash if their preconditions aren't met.

### Why it's shaped this way

The mod serves blind users who can't see error messages. A silent failure (returning empty data) means the user gets incorrect information with no indication that something went wrong. A crash produces a log entry that developers can diagnose. The philosophy is: crashes are debuggable, silent failures are not.

### Recommendation: **Adopt enthusiastically**

This is even more natural in C# where you have proper exceptions, stack traces, and can catch at boundaries. Use `NullReferenceException` as your friend in internal code. Guard at the edges:

```csharp
// At system boundary (user pressed a key targeting an entity):
if (entity == null || entity.IsDestroyed) { Speak("Nothing here"); return; }

// Internal (entity was already validated):
var behavior = entity.GetBehavior(); // Let it throw if null
foreach (var signal in behavior.Signals) { ... } // Let it throw if null
```

---

## 11. Audio-First Message Design Principles

### What they built

The project encodes specific screen reader UX principles into its architecture:

1. **Vary information early**: "anchored cursor" / "unanchored cursor" lets the user stop listening after the first word. "cursor anchored" / "cursor unanchored" forces them to listen to "cursor" every time.
2. **Minimize punctuation**: Screen readers read `:` `(` `)` verbatim in many configurations. Use commas and periods only.
3. **One utterance per interaction**: The MessageBuilder accumulates all output for an event into a single `speak()` call. No competing/overlapping announcements.
4. **Hierarchical drill-down over flat lists**: Category -> subcategory -> item, rather than a flat list of everything.

### Why it's shaped this way

Screen reader users process information at the speed of speech (150-200 wpm). Every unnecessary word or symbol costs real time. The "vary early" principle lets power users interrupt as soon as they've heard enough. The single-utterance rule prevents the disorienting experience of overlapping speech.

### Recommendation: **Adopt all of these as hard rules**

These aren't Lua-specific or Factorio-specific. They're universal screen reader UX principles. Encode them:

- In your MessageBuilder (enforce single-build, enforce no bare punctuation)
- In your describer interface (require early-varying output)
- In your event system (one speak call per user action, period)
- In your navigation (hierarchical, not flat)

---

## 12. Testing Inside the Game Engine

### What they built

A custom test framework that runs 161 tests inside Factorio's actual game engine:

- Tests use tick-based scheduling: `ctx:at_tick(5, function() ... end)`
- The game launches in benchmark mode with a lab save file
- Tests can place entities, open UIs, simulate player actions
- Results are written to JSON files; a Python launcher orchestrates everything
- Speech output is capturable for assertions

Separate from the in-game tests, CLI-only unit tests cover pure-logic modules (data structures, rail utilities).

### Why it's shaped this way

Many bugs only manifest inside the game engine — timing issues, entity lifecycle, event ordering, save/load compatibility. CLI-only tests miss these entirely. The tick-based scheduling naturally models the game's temporal behavior.

### Recommendation: **Adopt the dual strategy**

In Unity:
1. **In-engine tests**: Use Unity Test Framework's play-mode tests. These run inside the game loop and can test Harmony patch behavior, game state interactions, and timing.
2. **Pure logic tests**: Use NUnit/xUnit for your MessageBuilder, describers, navigation graph, and other pure-logic code.

The tick-based scheduling pattern translates to `yield return null` in Unity coroutine tests, or frame-counting in play-mode tests.

The Python launcher pattern (external process that launches the game, monitors output, collects results) is also worth considering if your game doesn't have good test runner support built in.

---

## 13. The Launcher Communication Protocol

### What they built

The mod communicates with an external launcher via stdout:
```
out <player_index> <message>
```

The launcher captures this and routes it to the OS screen reader. `Speech.speak()` wraps this protocol. `game.print()` (which goes to the game's GUI) is explicitly forbidden since blind users can't see it.

### Why it's shaped this way

Factorio's modding API has no screen reader integration. The only communication channel is stdout, which the launcher process can capture. This simple protocol works across platforms and screen reader implementations.

### Recommendation: **May not need it**

In Unity, you likely have more options:
- Direct OS accessibility API calls via P/Invoke (SAPI on Windows, NSAccessibility on Mac)
- Tolk library (cross-screen-reader abstraction)
- Named pipes or IPC if you do need a launcher

But the principle of an abstraction layer between your mod and the TTS output is sound. Don't sprinkle TTS calls throughout your code. Have a single `Speech.Speak()` that everything flows through, even if its implementation is a direct API call rather than stdout.

---

## 14. Localization Architecture

### What they built

Multiple layers of localization support:

- **Factorio's native LocalisedString**: `{"entity-name.transport-belt"}` resolved at display time
- **MessageBuilder integration**: fragments can be raw strings or LocalisedString references
- **Message lists**: `.txt` files compiled into locale keys by a Python build step, used for help documentation
- **Localization linter**: Python tool that cross-references defined keys against usage in Lua code, catching unused and missing keys

Locale keys must be in section `fa` and must not contain dots in the key name (`fa.foo-bar` is valid, `fa.foo.bar` is not). This avoids Factorio parser ambiguity.

### Why it's shaped this way

Accessibility text is far more extensive than typical game UI text. Every entity, every status, every navigation action needs text. The linter catches the inevitable drift between code and locale files in a 250-module codebase. The message list system provides structured help documentation that integrates with the help UI (Shift+/ opens help, W/S navigates messages).

### Recommendation: **Adopt the linter, adapt the rest**

Unity has its own localization packages. Use them. But definitely build (or find) a tool that cross-references your localization keys against usage in code. In a project with hundreds of spoken strings, orphaned and missing keys are constant problems.

The message list concept (help documentation as structured text files that compile into localizable entries) is worth adopting for any in-game help system.

---

## 15. Build Tooling: One Command to Rule Them All

### What they built

`launch_factorio.py` (1307 lines) is a unified entry point for all development operations:

```bash
python launch_factorio.py --format --lint --run-tests --timeout 60
```

This single command: applies formatting (stylua), runs three linters (annotation checker, require placement checker, lua-language-server), launches the game in benchmark mode, runs all tests, captures results, and reports failures.

Additional tools:
- `lint_localisation.py`: Cross-references locale keys
- `build_message_lists.py`: Compiles help text into locale entries
- GitHub Actions CI: Blocks PRs on format violations; auto-publishes releases on version tags

### Why it's shaped this way

Lua has no standard build system, package manager, or test runner. The Python launcher fills all these roles. The "one command" approach means developers (including AI assistants) can't forget a quality gate.

### Recommendation: **Adopt the principle, use C# tooling**

You have `dotnet test`, `dotnet format`, Roslyn analyzers, and established CI/CD patterns. Use them. But adopt the principle: a single command (or CI step) that runs every quality gate. A developer should never need to remember "also run the linter" or "also rebuild the help files."

Consider a `Makefile`, `build.cake`, or shell script that chains: format check -> analyzers -> unit tests -> play-mode tests -> locale lint.

---

## 16. Viewpoint: Separating Cursor State from Game State

### What they built

The Viewpoint module is a per-player state container for cursor-related data: position (always integral), size, direction, anchoring, bookmarks, rotation offset. It uses StorageManager for persistence and implements a listener pattern for cursor events (`cursor_moved_continuous`, `cursor_jumped`).

Crucially, the viewpoint is **not** the game's native selection/cursor. It's an independent concept that the mod maintains, positioned at integral tile coordinates regardless of where the actual player character is.

### Why it's shaped this way

Factorio's native cursor is mouse-driven, with sub-tile precision. That's meaningless for audio navigation. The mod needs a tile-snapped, keyboard-driven cursor with its own state (size, direction for building, anchor for reference points). Decoupling from the game's cursor means the mod controls the experience entirely.

### Recommendation: **Adopt**

Your Unity game likely has a mouse/pointer-based selection system. Don't try to repurpose it. Create your own `AccessibilityCursor` with:
- Grid-snapped position (or whatever granularity makes sense for your game)
- Direction state for placement/interaction
- Bookmark system for saved positions
- Listener/event pattern for movement notifications

This separation means your accessibility layer doesn't fight the game's native input system.

---

## 17. Things Shaped by Lua/Factorio (Don't Port These)

A few patterns exist because of platform constraints rather than design wisdom:

- **Metatables for StorageManager**: `__index`/`__newindex` metamagic is Lua's way of faking properties. In C#, use actual properties or indexers.
- **`script.on_event` single-handler limitation**: The entire EventManager exists because Factorio only allows one handler per event. C# has proper multicast delegates.
- **`require()` only at top level**: Factorio enforces this for save/load determinism. C# has no such constraint (but you should still avoid runtime assembly loading in hot paths).
- **Three loading stages (settings/data/runtime)**: Factorio's specific lifecycle. Unity has its own (Awake/Start/Update, or assembly loading order). Map concepts, not stages.
- **`storage` as the global persistence root**: Factorio's save system. In Unity, use your game's save system or PlayerPrefs, with proper serialization.
- **stdout protocol for TTS**: Factorio can't call OS APIs. Unity can, via P/Invoke or native plugins.

---

## 18. Surprising or Clever Details Worth Noting

### Deferred refresh with sound-first feedback
When the user presses a scanner key, the mod plays a sound immediately and sets a 2-tick delay before rebuilding data. The user hears confirmation of their action while the expensive work happens in the background. This is a micro-optimization that makes a huge difference for perceived responsiveness.

**Port this.** Always acknowledge user input instantly (a click sound, a "searching..." utterance) before doing expensive work.

### SparseBitset for entity deduplication
Entity registration numbers are tracked in a sparse bitset to prevent double-processing. This is O(1) for both insert and lookup, unlike a hash set which has amortized O(1) but worse cache behavior for dense sequential IDs.

**Consider this** if your game has dense sequential IDs for objects. Otherwise a HashSet<int> is fine.

### The `cat2()` pattern for composite subcategories
Scanner backends create subcategories like `"assembling-machine/iron-gear-wheel"` using a `cat2(type, detail)` helper. This gives two levels of information in a flat string: the machine type and what it's making.

**Adopt the concept.** Composite keys for categorization are useful. In C#, use tuples or a small struct rather than string concatenation.

### Bind system for entity lifecycle
When a UI opens targeting an entity, the Router registers for that entity's destruction event. If the entity dies while the UI is open, the UI auto-closes. This prevents stale UI state without polling.

**Adopt.** This is event-driven cleanup done right. In Unity, subscribe to your entity's OnDestroyed event and close any associated UI.

### Vanilla Mode as a feature flag
Players can toggle all accessibility features off with a single keybind (Ctrl+Alt+Shift+V). The EventManager checks a whitelist of events that should still fire in vanilla mode (tick processing, scanner background work) so the mod doesn't corrupt game state when disabled.

**Adopt.** Your mod should be fully toggleable without requiring a game restart. Design your event system with a "bypass" mode from the start.

---

## Summary: The Five Big Lessons

1. **Build a pipeline, not a pile.** Game state -> describers -> message assembly -> speech output. Each layer has one job. The MessageBuilder's constraints are as important as its capabilities.

2. **Navigation is a graph problem.** Audio UIs need explicit, predictable navigation paths. The directed graph with a total ordering constraint (down-right traversal) is the right foundation. Build higher-level patterns (menus, grids, forms) on top of it.

3. **Scan incrementally, describe specifically.** Never process the whole world at once. Use work queues with per-frame budgets. Give each entity type a specialized describer rather than a generic one.

4. **Let modules own their state.** Declarative state registration (StorageManager) beats centralized initialization. Each module declares what it needs; the system handles creation, defaults, and lifecycle.

5. **Crash loudly, fail never silently.** Internal code should crash on violated preconditions. Validate at system boundaries only. A crash is debuggable; a silent wrong answer to a blind user is not.
