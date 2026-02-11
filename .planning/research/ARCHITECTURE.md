# Architecture Research: ONI Accessibility Mod

> Research dimension: Architecture
> Question: How should this accessibility mod be architectured? What are the major components, how do they interact, and what's the suggested build order?
> Status: COMPLETE
> Date: 2026-02-11

---

## Executive Summary

The ONI accessibility mod should follow a **layered pipeline architecture** with five core layers: Input Handling, Game State Reading, Information Processing, Speech Output, and Navigation/Cursor. Each layer has a clean boundary and communicates through well-defined interfaces. The architecture draws heavily from FactorioAccess patterns (centralized cursor, announcement pipeline, state-driven speech) while adapting them to C#/Unity/Harmony constraints. The recommended build order is bottom-up: speech output first, then game state reading, then input handling, then navigation, then UI-specific patches.

---

## 1. Architectural Overview

### 1.1 High-Level Architecture Diagram (Text)

```
+------------------------------------------------------------------+
|                        ONI Game Process                           |
|  +------------------------------------------------------------+  |
|  |  Harmony Patch Layer                                        |  |
|  |  (Prefix/Postfix hooks into ONI classes)                    |  |
|  |  - UI event patches (KScreen, ToolMenu, OverlayMenu...)    |  |
|  |  - Game state patches (SimMessages, WorldContainer...)      |  |
|  |  - Input patches (KInputHandler, PlayerController)          |  |
|  +---------------------------+--------------------------------+  |
|                              |                                    |
|                              v                                    |
|  +------------------------------------------------------------+  |
|  |  Mod Core (Singleton Managers)                              |  |
|  |                                                             |  |
|  |  +----------------+  +----------------+  +---------------+  |  |
|  |  | AccessCursor   |  | StateTracker   |  | InputManager  |  |  |
|  |  | (grid nav,     |  | (reads game    |  | (hotkey       |  |  |
|  |  |  focus track)  |  |  state, caches)|  |  registry)    |  |  |
|  |  +-------+--------+  +-------+--------+  +-------+-------+  |  |
|  |          |                    |                    |          |  |
|  |          v                    v                    v          |  |
|  |  +------------------------------------------------------+   |  |
|  |  |  Announcement Pipeline                                |   |  |
|  |  |  (formats, deduplicates, prioritizes, queues)         |   |  |
|  |  +---------------------------+---------------------------+   |  |
|  |                              |                               |  |
|  |                              v                               |  |
|  |  +------------------------------------------------------+   |  |
|  |  |  Speech Output (Tolk wrapper)                         |   |  |
|  |  |  - Interrupt/queue modes                              |   |  |
|  |  |  - Rate limiting                                      |   |  |
|  |  |  - Screen reader detection                            |   |  |
|  |  +------------------------------------------------------+   |  |
|  +------------------------------------------------------------+  |
+------------------------------------------------------------------+
```

### 1.2 Design Philosophy

Adapted from FactorioAccess principles for C#/Unity:

- **Accessible experience over visual mimicry**: Do not try to describe the screen pixel by pixel. Present game information in a navigable, logical structure optimized for audio consumption.
- **Reuse game text/data over hardcoding**: Hook into ONI's localization system (LocText, STRINGS) and existing data structures rather than maintaining parallel data.
- **Concise announcements**: Prioritize brevity. "Coal Generator, idle, 400kg coal" not "The coal generator building is currently in an idle state and contains four hundred kilograms of coal material."
- **Conscious hotkey management**: Avoid conflicts with screen readers (Insert, CapsLock). Use a dedicated modifier or carefully chosen keys.
- **State-driven, not event-driven**: Read current game state when needed rather than trying to track every event. ONI's simulation is complex; querying state is more reliable than intercepting all mutations.

---

## 2. Core Components

### 2.1 Speech Output Layer (`OniAccess.Speech`)

**Responsibility**: All communication with the screen reader. Single point of output.

**Key Classes**:

| Class | Role |
|-------|------|
| `SpeechEngine` | Wraps Tolk P/Invoke. Handles init/shutdown, screen reader detection. Thin wrapper around existing `Speech.cs`. |
| `SpeechQueue` | Manages announcement priority and deduplication. Prevents speech spam during rapid state changes. |
| `Announcement` | Value type: text, priority level (Critical/High/Normal/Low), interrupt flag, category tag for dedup. |

**Interface**:
```csharp
public interface ISpeechOutput
{
    void Speak(string text, bool interrupt = false);
    void Speak(Announcement announcement);
    void Silence();
    bool IsScreenReaderActive();
}
```

**Design Decisions**:
- **Singleton pattern**: One `SpeechOutput` instance for the entire mod. All speech goes through this.
- **Priority levels**: Critical (errors, alerts) always interrupts. High (navigation results) interrupts Low. Normal (status reads) queues. Low (ambient info) can be dropped if backlogged.
- **Deduplication window**: Same-category announcements within ~200ms collapse to the latest. Prevents "iron ore, iron ore, iron ore" when cursor stutters.
- **Interrupt semantics**: `interrupt=true` calls `Tolk.Silence()` then `Tolk.Speak()`. Default is queue/append.

**Build Order**: FIRST. Everything depends on this. The existing `Speech.cs` Tolk wrapper is the foundation.

---

### 2.2 Game State Reader Layer (`OniAccess.State`)

**Responsibility**: Read-only access to ONI's game state. Translates raw game data into accessibility-relevant information.

**Key Classes**:

| Class | Role |
|-------|------|
| `CellInspector` | Reads cell data at a grid position: element, mass, temperature, buildings, creatures, items. Uses `Grid` class, `ElementLoader`. |
| `BuildingInspector` | Reads building state: type, status, storage contents, operational flags. Uses `BuildingComplete`, `Operational`, `Storage` components. |
| `DuplicantInspector` | Reads dupe state: name, current task, health, stress, skills. Uses `MinionIdentity`, `ChoreDriver`, `Health` components. |
| `WorldInspector` | Reads colony-level state: resource totals, dupe count, cycle number, alerts. Uses `WorldInventory`, `ClusterManager`. |
| `UIInspector` | Reads current UI state: active screen, selected tool, overlay mode, menu items. Uses `ManagementMenu`, `ToolMenu`, `OverlayScreen`. |

**Interface Pattern**:
```csharp
public static class CellInspector
{
    public static CellInfo GetCellInfo(int cell);
    public static string Describe(int cell, Verbosity level = Verbosity.Normal);
    public static BuildingInfo? GetBuilding(int cell);
    public static List<PickupableInfo> GetItems(int cell);
}
```

**Design Decisions**:
- **Static utility classes, not MonoBehaviours**: These are pure readers with no state of their own. Static methods simplify usage from Harmony patches.
- **Verbosity levels**: Compact (name only), Normal (name + key status), Detailed (full readout). Controlled by user setting or explicit request.
- **Return structured data, not strings**: Inspectors return info records. Formatting into speech text happens in the Announcement Pipeline. This enables different output formats without changing readers.
- **No caching by default**: ONI game state changes every sim tick. Read fresh on demand. Cache only expensive operations (e.g., pathfinding queries) with explicit invalidation.

**Build Order**: SECOND. Needed before navigation can announce anything meaningful.

---

### 2.3 Announcement Pipeline (`OniAccess.Announce`)

**Responsibility**: Transforms structured game data into spoken text. Handles formatting, localization, deduplication, and priority assignment.

**Key Classes**:

| Class | Role |
|-------|------|
| `Formatter` | Converts info records to concise text strings. Uses ONI's `STRINGS` system for localized names. |
| `AnnouncementBuilder` | Fluent API for constructing announcements with priority, category, interrupt settings. |
| `ChangeDetector` | Compares current state to previous state for a given context. Only announces what changed. |
| `AnnouncementRouter` | Central dispatcher. Receives announcement requests, deduplicates, prioritizes, sends to `SpeechQueue`. |

**Interface**:
```csharp
public static class Formatter
{
    public static string FormatCell(CellInfo info, Verbosity level);
    public static string FormatBuilding(BuildingInfo info, Verbosity level);
    public static string FormatDuplicant(DuplicantInfo info, Verbosity level);
    public static string FormatAlert(Notification notification);
}
```

**Design Decisions**:
- **Reuse ONI's STRINGS**: All building names, element names, status items come from ONI's localization. `STRINGS.BUILDINGS.PREFABS.COALGENERATOR.NAME` etc. This automatically supports language packs.
- **Change detection pattern** (from FactorioAccess): When the cursor moves to a new cell, announce the full cell description. When the cursor stays on the same cell and state changes, announce only the delta. This requires storing "last announced state" per context.
- **Category-based deduplication**: Each announcement has a category string (e.g., "cursor.cell", "ui.menu", "alert.yellow"). Only the latest announcement per category within the dedup window survives. This prevents stale announcements from reaching the screen reader.

**Build Order**: THIRD. Depends on both Speech Output and Game State Readers.

---

### 2.4 Navigation / Cursor System (`OniAccess.Navigation`)

**Responsibility**: Grid-based cursor navigation independent of the mouse. The primary way blind players explore the game world.

**Key Classes**:

| Class | Role |
|-------|------|
| `GridCursor` | Maintains a cell position on the game grid. Moves N/S/E/W. Wraps at world boundaries. Tracks which world (asteroid) is active. |
| `MenuCursor` | Virtual cursor for navigating UI menus (build menu, research tree, management screens). Tracks position within lists/trees. |
| `CursorManager` | Orchestrates which cursor is active based on game context (grid vs. menu). Handles cursor mode switching. |
| `SearchSystem` | Find-by-name functionality. "Jump to nearest coal generator." Uses ONI's `Components` singletons to enumerate. |

**Interface**:
```csharp
public class GridCursor
{
    public int CurrentCell { get; }
    public Vector2I Position { get; }
    public void Move(Direction direction, int steps = 1);
    public void JumpTo(int cell);
    public void JumpToBuilding(Tag buildingTag);
    public CellInfo Inspect();
}
```

**Design Decisions**:
- **Separate from mouse cursor**: The accessibility cursor is a logical position, not tied to Unity's mouse/pointer system. This avoids conflicts with ONI's existing mouse handling.
- **Dual cursor model** (from FactorioAccess): Grid cursor for world navigation, menu cursor for UI. Only one active at a time. When a KScreen opens, switch to menu cursor. When it closes, return to grid cursor.
- **Announce on move**: Every cursor movement triggers an announcement of the destination. This is the core interaction loop: move cursor, hear what's there, decide what to do.
- **World-aware**: ONI has multiple asteroids (Spaced Out DLC). The cursor must track which WorldContainer it's in and respect world boundaries.
- **Snapping**: Option to snap cursor to nearest building or entity, not just raw grid cells. Many cells in ONI are empty space; snapping to points of interest improves efficiency.

**Build Order**: FOURTH. Depends on State Readers and Announcement Pipeline. This is where the mod becomes genuinely usable.

---

### 2.5 Input Handling Layer (`OniAccess.Input`)

**Responsibility**: Register and manage accessibility hotkeys. Route key presses to appropriate actions.

**Key Classes**:

| Class | Role |
|-------|------|
| `AccessKeyBindings` | Defines all mod keybindings. Uses ONI's `Action` and `BindingEntry` system for proper registration. |
| `InputRouter` | Maps key events to mod actions (cursor move, inspect, read status, etc.). Respects game context (which screen is active). |
| `KeyConflictChecker` | Validates mod keybindings don't conflict with ONI defaults or common screen reader keys. |

**Design Decisions**:
- **Use ONI's input system**: Register through `GameInputMapping.DuplicateKeybindings()` and ONI's `Action` enum extension. This ensures proper integration with the game's key binding UI and conflict detection.
- **Context-sensitive keys**: Same key does different things based on context. Arrow keys navigate the grid in world view but navigate menus when a screen is open. This matches screen reader conventions where arrows mean "navigate current context."
- **Avoid screen reader conflicts**: Never bind Insert, CapsLock, or NVDA/JAWS modifier combos. Prefer unmodified keys (arrows, letters) when game input would be paused anyway (e.g., in menus), and Ctrl/Alt combos in gameplay.
- **Repeatable keys**: Arrow key held down should repeat cursor movement (with audio feedback rate-limited to prevent speech flood).

**Build Order**: EARLY-PARALLEL. Basic input can be set up alongside Speech Output. Full context-sensitive routing comes later.

---

### 2.6 Harmony Patch Layer (`OniAccess.Patches`)

**Responsibility**: All Harmony patches that hook into ONI code. Kept thin -- patches extract events/data and delegate to mod core.

**Key Patch Groups**:

| Patch Group | Target Classes | Purpose |
|-------------|---------------|---------|
| `UIPatches` | `KScreen`, `ManagementMenu`, `ToolMenu`, `OverlayScreen`, `BuildMenu` | Detect screen open/close, menu selection changes. Trigger menu cursor activation. |
| `GameStatePatches` | `Notifier`, `StatusItemGroup`, `Notification` | Intercept alerts and status changes for proactive announcements. |
| `InputPatches` | `KInputHandler`, `PlayerController` | Inject accessibility key handling before normal game input processing. |
| `SimPatches` | `Game`, `SaveGame`, `SpeedControlScreen` | Detect game load/save, pause/unpause, speed changes. |
| `TooltipPatches` | `ToolTip`, `HoverTextScreen`, `SelectTool` | Capture hover text that ONI generates and reroute to speech. |
| `BuildPatches` | `PlanScreen`, `BuildTool`, `CopyBuildingSettings` | Support accessible building placement workflow. |

**Design Decisions**:
- **Thin patches, fat core**: Patches should be 1-5 lines. Extract the data, call a mod core method. Never put business logic in a patch. This minimizes surface area for breakage when ONI updates.
- **Prefer Postfix over Prefix**: Postfixes are less invasive. Use Prefix only when you need to prevent original behavior or inject before processing.
- **Avoid Transpilers unless necessary**: Transpilers are fragile across game updates. Use only when Prefix/Postfix cannot achieve the goal (e.g., modifying a value mid-method).
- **Patch organization**: One file per patch group, not one file per patch. Related patches belong together.
- **Version resilience**: Use `TargetMethod()` with reflection-based fallbacks where method signatures might change between ONI updates.

**Build Order**: INCREMENTAL. Add patches as each feature requires them. Start with the simplest hooks (game load detection, pause/unpause) and expand.

---

### 2.7 Configuration System (`OniAccess.Config`)

**Responsibility**: User-configurable settings for speech behavior, verbosity, keybindings, and feature toggles.

**Key Settings**:

| Setting | Type | Default | Purpose |
|---------|------|---------|---------|
| `Verbosity` | enum | Normal | How much detail in announcements |
| `SpeechRate` | int | 0 (SR default) | Override speech rate if supported |
| `CursorWrap` | bool | true | Wrap cursor at world edges |
| `AutoReadAlerts` | bool | true | Automatically speak new alerts |
| `AutoReadStatusChanges` | bool | false | Speak when building status changes |
| `KeyRepeatDelay` | int (ms) | 250 | Cursor movement repeat rate |
| `AnnouncementDedup` | int (ms) | 200 | Dedup window for same-category speech |

**Design Decisions**:
- **Use ONI's mod config system** if available (PLib's `POptions` is standard for ONI mods), or a simple JSON file in the mod directory.
- **Accessible settings screen**: The config screen itself must be navigable by the mod's own accessibility features. This is a bootstrapping challenge -- consider a simple text-file config for initial versions.
- **Runtime-changeable**: Most settings should take effect immediately without game restart.

**Build Order**: LATE. Start with hardcoded defaults. Add configurability after core features work.

---

## 3. Data Flow

### 3.1 Primary Data Flow: Cursor Navigation

```
User presses arrow key
       |
       v
InputRouter receives key event
       |
       v
InputRouter calls GridCursor.Move(direction)
       |
       v
GridCursor updates internal position (new cell integer)
       |
       v
GridCursor calls CellInspector.GetCellInfo(newCell)
       |
       v
CellInspector reads from Grid, GameObjects at cell, Components
       |
       v
Returns CellInfo { Element, Temperature, Buildings[], Items[], Creatures[] }
       |
       v
Formatter.FormatCell(cellInfo, currentVerbosity) -> "Granite, 45C, Coal Generator idle"
       |
       v
AnnouncementRouter.Announce(text, category: "cursor.cell", priority: Normal, interrupt: true)
       |
       v
SpeechQueue deduplicates, enqueues
       |
       v
SpeechEngine.Speak(text, interrupt: true)  ->  Tolk.Output(text, true)
       |
       v
Screen reader speaks to user
```

### 3.2 Secondary Data Flow: Alert Notification

```
ONI raises a Notification (e.g., "Duplicant idle")
       |
       v
GameStatePatches.Notifier_Postfix intercepts Add()
       |
       v
Calls AnnouncementRouter.AnnounceAlert(notification)
       |
       v
Formatter.FormatAlert(notification) -> "Alert: Meep is idle"
       |
       v
AnnouncementRouter.Announce(text, category: "alert", priority: High, interrupt: false)
       |
       v
SpeechQueue enqueues after current speech
       |
       v
SpeechEngine.Speak(text)
```

### 3.3 Tertiary Data Flow: Menu Navigation

```
User opens Build Menu (or Harmony Postfix detects PlanScreen.OnActivate)
       |
       v
UIPatches.PlanScreen_OnActivate_Postfix fires
       |
       v
CursorManager.ActivateMenuCursor(menuType: BuildMenu)
       |
       v
MenuCursor initializes with menu items from PlanScreen
       |
       v
Announces: "Build menu. Category: Base. Item 1 of 12: Tile"
       |
       v
User presses Up/Down arrows
       |
       v
MenuCursor.Move(direction)
       |
       v
Announces next item: "Insulated Tile, 2 of 12"
       |
       v
User presses Enter
       |
       v
MenuCursor activates selection -> delegates to PlanScreen
```

---

## 4. Module Organization

### 4.1 Recommended Namespace/Folder Structure

```
OniAccessibility/
|
+-- mod.yaml                           # ONI mod manifest
+-- mod_info.yaml                      # Mod metadata for Steam/mod browser
+-- OniAccessibility.csproj            # C# project file
|
+-- Mod.cs                             # Entry point: UserMod2 subclass, Harmony init
|
+-- Speech/
|   +-- SpeechEngine.cs                # Tolk P/Invoke wrapper (evolved from Speech.cs)
|   +-- SpeechQueue.cs                 # Priority queue with deduplication
|   +-- Announcement.cs                # Announcement record type
|   +-- ISpeechOutput.cs               # Interface for testability
|
+-- State/
|   +-- CellInspector.cs               # Grid cell data reading
|   +-- BuildingInspector.cs           # Building component reading
|   +-- DuplicantInspector.cs          # Duplicant state reading
|   +-- WorldInspector.cs              # Colony-wide state reading
|   +-- UIInspector.cs                 # Current UI state reading
|   +-- InfoRecords.cs                 # CellInfo, BuildingInfo, etc. record types
|
+-- Announce/
|   +-- Formatter.cs                   # Info records -> text strings
|   +-- AnnouncementRouter.cs          # Central dispatch + dedup
|   +-- ChangeDetector.cs              # Delta announcements
|   +-- AnnouncementBuilder.cs         # Fluent construction API
|
+-- Navigation/
|   +-- GridCursor.cs                  # World grid navigation
|   +-- MenuCursor.cs                  # UI menu navigation
|   +-- CursorManager.cs              # Context switching between cursors
|   +-- SearchSystem.cs                # Find/jump-to functionality
|
+-- Input/
|   +-- AccessKeyBindings.cs           # Key binding definitions
|   +-- InputRouter.cs                 # Key event routing
|
+-- Patches/
|   +-- UIPatches.cs                   # KScreen, menu, overlay patches
|   +-- GameStatePatches.cs            # Notification, status patches
|   +-- InputPatches.cs                # Input system patches
|   +-- SimPatches.cs                  # Game lifecycle patches
|   +-- TooltipPatches.cs              # Hover text capture patches
|   +-- BuildPatches.cs                # Building placement patches
|
+-- Config/
|   +-- ModConfig.cs                   # Configuration data class
|   +-- ConfigManager.cs               # Load/save/apply settings
|
+-- Util/
|   +-- GridHelper.cs                  # Grid math utilities
|   +-- ComponentHelper.cs             # Safe component access helpers
|   +-- LogHelper.cs                   # Debug logging
|
+-- lib/
    +-- Tolk.dll                       # Tolk native library
    +-- 0Harmony.dll                   # (Shipped with ONI, reference only)
```

### 4.2 Entry Point

```csharp
// Mod.cs - ONI mod entry point
using HarmonyLib;

namespace OniAccessibility
{
    public class Mod : KMod.UserMod2
    {
        public static Mod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);  // Applies all [HarmonyPatch] classes

            SpeechEngine.Initialize();
            ConfigManager.Load();
            AnnouncementRouter.Initialize();
            CursorManager.Initialize();

            LogHelper.Log("OniAccessibility loaded.");
            SpeechEngine.Speak("ONI Accessibility mod loaded.", interrupt: true);
        }
    }
}
```

---

## 5. Harmony Patching Strategy

### 5.1 Patch Discovery and Application

ONI Harmony mods use `KMod.UserMod2` as the entry point. Calling `base.OnLoad(harmony)` automatically discovers and applies all classes decorated with `[HarmonyPatch]` in the assembly. No manual `harmony.PatchAll()` needed.

### 5.2 Patch Pattern: Thin Shell

```csharp
// Example: Detect when a KScreen opens
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
public static class KScreen_Activate_Patch
{
    public static void Postfix(KScreen __instance)
    {
        // Thin: extract data, delegate to core
        CursorManager.OnScreenActivated(__instance);
    }
}
```

### 5.3 Patch Pattern: Capturing Notifications

```csharp
[HarmonyPatch(typeof(Notifier), nameof(Notifier.Add))]
public static class Notifier_Add_Patch
{
    public static void Postfix(Notifier __instance, Notification notification)
    {
        AnnouncementRouter.OnNotification(notification);
    }
}
```

### 5.4 Patch Pattern: Input Interception

```csharp
[HarmonyPatch(typeof(KInputHandler), nameof(KInputHandler.HandleKeyDown))]
public static class KInputHandler_HandleKeyDown_Patch
{
    public static bool Prefix(KButtonEvent e)
    {
        // Return false to consume the input (prevent game from processing it)
        return !InputRouter.HandleKeyDown(e);
    }
}
```

### 5.5 Fragility Mitigation

- **Method existence checks**: Use `AccessTools.Method()` in a static constructor to verify target methods exist. Log warnings if not found (game version mismatch).
- **Try-catch in patches**: Every patch body should be wrapped in try-catch to prevent mod crashes from killing the game. Log exceptions but don't throw.
- **Conditional patches**: Use `HarmonyPatch` with `MethodType` for properties. Use `[HarmonyPrepare]` to skip patches when target doesn't exist.

```csharp
[HarmonyPatch(typeof(KScreen), nameof(KScreen.Activate))]
public static class KScreen_Activate_Patch
{
    [HarmonyPrepare]
    public static bool Prepare()
    {
        return AccessTools.Method(typeof(KScreen), "Activate") != null;
    }

    public static void Postfix(KScreen __instance)
    {
        try
        {
            CursorManager.OnScreenActivated(__instance);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"KScreen.Activate patch failed: {ex}");
        }
    }
}
```

---

## 6. Component Interactions

### 6.1 Dependency Graph

```
                    +--------+
                    | Config |
                    +---+----+
                        |  (read by all)
    +-------------------+-------------------+
    |                   |                   |
    v                   v                   v
+--------+      +-----------+      +-------+
| Speech |<-----| Announce  |<-----| State |
+--------+      +-----------+      +-------+
    ^                   ^              ^
    |                   |              |
    |           +-------+-------+      |
    |           |               |      |
    |      +----+----+    +----+----+  |
    +------| Navig.  |    | Patches +--+
           +---------+    +---------+
                ^              |
                |              |
           +----+----+        |
           |  Input  +--------+
           +---------+
```

**Arrows indicate "depends on" / "calls into".**

### 6.2 Key Interaction Contracts

1. **Patches -> State**: Patches may call State readers to get current info for announcements triggered by game events.
2. **Patches -> Announce**: Patches call AnnouncementRouter to trigger event-driven announcements (screen opened, alert raised).
3. **Patches -> Navigation**: Patches notify CursorManager of context changes (screen open/close).
4. **Navigation -> State**: Cursor movement triggers state inspection.
5. **Navigation -> Announce**: Cursor movement results go through the announcement pipeline.
6. **Input -> Navigation**: Key presses drive cursor movement.
7. **Input -> Patches**: Some input handling works through ONI's input system (patched).
8. **Announce -> Speech**: All text output funnels through the announcement pipeline to speech.
9. **Config -> All**: Configuration values are read by all components.

### 6.3 What Does NOT Talk to What

- **Speech does NOT call back into any other component.** It is a pure output sink.
- **State readers do NOT trigger announcements.** They are passive. Announcements are triggered by Navigation, Input, or Patches.
- **Navigation does NOT directly call Speech.** It always goes through the Announcement Pipeline for formatting and dedup.
- **Patches do NOT directly call Speech** (except maybe a boot message). Always through the pipeline.

---

## 7. Suggested Build Order

### Phase 0: Foundation (Week 1)

**Goal**: Prove the mod loads and can speak.

1. **Project scaffolding**: `mod.yaml`, `.csproj`, reference ONI assemblies, include Tolk.dll.
2. **`Mod.cs` entry point**: `UserMod2` subclass, verify `OnLoad` fires.
3. **`SpeechEngine`**: Evolve existing `Speech.cs` into a proper class. Init Tolk, detect screen reader, provide `Speak()` and `Silence()`.
4. **Smoke test**: Mod loads, speaks "ONI Accessibility loaded" through screen reader.

**Delivers**: Proof of life. Screen reader says something.

### Phase 1: Read the World (Weeks 2-3)

**Goal**: Read game state and speak it on demand.

5. **`CellInspector`**: Read element, temperature, mass at a cell.
6. **`BuildingInspector`**: Read building type, status, storage.
7. **`Formatter` (basic)**: Convert CellInfo/BuildingInfo to text.
8. **`AnnouncementRouter` (basic)**: Simple pass-through to SpeechEngine, with interrupt support.
9. **`GridCursor` (basic)**: Hardcoded test -- inspect cell (0,0), speak result.

**Delivers**: Can read and speak game world data. Not yet navigable.

### Phase 2: Navigate the World (Weeks 3-5)

**Goal**: Arrow-key grid navigation with speech feedback.

10. **`AccessKeyBindings`**: Register arrow keys and basic hotkeys.
11. **`InputRouter`**: Route arrow keys to GridCursor movement.
12. **`GridCursor` (full)**: Proper grid movement, boundary handling, world tracking.
13. **`SpeechQueue`**: Implement priority and deduplication (cursor movement generates rapid input).
14. **`ChangeDetector`**: Track last-announced cell to avoid repeating same info.

**Delivers**: Blind player can navigate the grid and hear what's at each cell. **This is the first genuinely usable milestone.**

### Phase 3: Read the UI (Weeks 5-7)

**Goal**: Navigate menus and screens with speech feedback.

15. **`UIPatches`**: Detect KScreen activation/deactivation.
16. **`UIInspector`**: Read current screen type, extract menu items.
17. **`MenuCursor`**: Navigate within menus (up/down/enter/escape).
18. **`CursorManager`**: Switch between grid and menu cursor based on context.
19. **`TooltipPatches`**: Capture hover text from ONI's tooltip system.

**Delivers**: Can navigate build menus, management screens, overlays.

### Phase 4: Act in the World (Weeks 7-10)

**Goal**: Perform game actions accessibly (build, dig, assign).

20. **`BuildPatches`**: Accessible building placement workflow.
21. **`DuplicantInspector`**: Read dupe status for management.
22. **`WorldInspector`**: Colony-level readouts (resources, dupes, cycle).
23. **`SearchSystem`**: Jump-to-building and jump-to-dupe.
24. **`GameStatePatches`**: Proactive alerts (low resources, idle dupes).

**Delivers**: Can play the game: build, manage, respond to alerts.

### Phase 5: Polish and Configure (Weeks 10+)

**Goal**: User customization and edge cases.

25. **`ConfigManager`**: Settings file, runtime changes.
26. **Verbosity modes**: Let user control detail level.
27. **SimPatches**: Save/load, pause/speed announcements.
28. **Spaced Out DLC support**: Multi-world cursor handling.
29. **Edge case patches**: Tutorials, sandbox mode, debug tools.

**Delivers**: Polished, configurable accessibility experience.

---

## 8. Cross-Cutting Concerns

### 8.1 Error Handling

Every Harmony patch and every public method in the mod core must catch exceptions internally. A crash in the accessibility mod must never crash the game. Pattern:

```csharp
try { /* mod logic */ }
catch (Exception ex) { LogHelper.Error($"Context: {ex}"); }
```

### 8.2 Performance

- **Speech calls are cheap**: Tolk.Output is async from the game's perspective. The screen reader processes speech in its own process.
- **State reading must be fast**: CellInspector reads from ONI's Grid arrays (O(1) array lookups). BuildingInspector uses GetComponent (cached by Unity). No LINQ in hot paths.
- **Deduplication prevents waste**: Rapid cursor movement generates many announcements but only the last one per dedup window actually reaches the screen reader.
- **No per-frame updates**: The mod reacts to input events and game events, not Update(). No MonoBehaviour.Update() polling loop.

### 8.3 Testing Strategy

- **Manual testing with NVDA**: Primary test method. Use NVDA speech viewer for visual confirmation during development.
- **Unit-testable core**: State readers and Formatter take data in, return data out. Can be unit tested outside Unity with mock data.
- **Integration testing**: Load mod in ONI, verify each patch fires. Log-based verification for automated smoke tests.

### 8.4 Game Update Resilience

- **Minimize patch surface area**: Fewer patches = fewer breakage points.
- **Target stable APIs**: `Grid.Element[]`, `KScreen.Activate()`, `Notifier.Add()` are stable across ONI versions.
- **Avoid private field access**: Use public properties and methods where possible. Private internals change without notice.
- **Version detection**: Log ONI build number at startup. Warn if untested version.

---

## 9. Patterns Adapted from FactorioAccess

### 9.1 Centralized Cursor (FA Pattern #1 -> GridCursor + MenuCursor)

FactorioAccess maintains a single cursor position independent of the mouse. Adapted here as `GridCursor` for the 2D grid world and `MenuCursor` for UI navigation, managed by `CursorManager`. Same principle: the cursor is the blind player's "eyes."

### 9.2 Announce-on-Move (FA Pattern #2 -> CellInspector + Formatter)

Every cursor movement triggers a state read and announcement. FactorioAccess does `describe_entity_at_cursor()`. Our equivalent: `CellInspector.GetCellInfo(cell)` -> `Formatter.FormatCell()` -> `SpeechEngine.Speak()`.

### 9.3 Hierarchical Description (FA Pattern #3 -> Verbosity Levels)

FactorioAccess provides summary vs. detailed descriptions. Adapted as `Verbosity.Compact`, `Verbosity.Normal`, `Verbosity.Detailed`. Quick arrow navigation uses Compact; explicit inspect uses Detailed.

### 9.4 Reuse Game Data (FA Pattern #4 -> State Readers + STRINGS)

FactorioAccess reads Factorio's `LuaEntity` properties. We read ONI's `Grid`, `BuildingComplete`, `Storage`, `StatusItemGroup` components. Same principle: never hardcode game data.

### 9.5 Search/Jump-to (FA Pattern #5 -> SearchSystem)

FactorioAccess has find-entity commands. Adapted as `SearchSystem` using ONI's `Components` singletons (e.g., `Components.BuildingCompletes.Items` to enumerate all buildings).

### 9.6 Audio Cues for Categories (FA Pattern #6 -> Announcement Categories)

FactorioAccess uses different audio tones for different entity types. In a screen reader mod, we use announcement prefixes and verbal category cues ("Building:", "Alert:", "Dupe:") rather than audio tones, since the screen reader controls the voice.

### 9.7 Proactive Alerts (FA Pattern #7 -> GameStatePatches)

FactorioAccess announces important events (enemy approaching, research complete). We hook ONI's `Notification` system to announce alerts: duplicant idle, building broken, resource depleted.

### 9.8 State Diffing (FA Pattern #8 -> ChangeDetector)

FactorioAccess tracks entity state changes. Our `ChangeDetector` compares current state to last-announced state and only speaks the delta. "Coal Generator: now active" instead of repeating the full description.

---

## 10. Risk Analysis

### 10.1 Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| ONI update breaks patches | High | Medium | Thin patches, HarmonyPrepare guards, version logging |
| Tolk.dll loading fails on some systems | Medium | Critical | Graceful fallback (log-only mode), clear error message |
| Speech flood during rapid gameplay | Medium | Medium | Deduplication, rate limiting, priority system |
| Input conflicts with screen readers | Medium | High | Avoid Insert/CapsLock, test with NVDA and JAWS |
| KScreen subclasses vary wildly in structure | High | Medium | Per-screen adapters in UIInspector, graceful fallback for unknown screens |
| Spaced Out DLC changes world model | Medium | Medium | Defer multi-world support to Phase 5, design cursor to be world-aware from start |

### 10.2 Design Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Too verbose, player drowns in speech | High | High | Default to Compact, let users increase. Test with blind testers early. |
| Too terse, player misses critical info | Medium | High | Alerts always interrupt. Critical status always included. |
| Navigation too slow for 2D grid | Medium | Medium | Jump-to commands, building snapping, search system |
| Config screen inaccessible (bootstrap) | High | Medium | Text file config initially, accessible config screen in Phase 5 |

---

## 11. Key Technical Decisions Summary

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Entry point | `UserMod2.OnLoad` | Standard ONI mod pattern, automatic Harmony patch discovery |
| Speech library | Tolk via P/Invoke | Supports NVDA, JAWS, SAPI; already prototyped in Speech.cs |
| State access | Static utility classes reading ONI singletons | No state of their own, simple to call from patches |
| Cursor model | Logical grid position (int cell) | Decoupled from mouse, uses ONI's cell system directly |
| Announcement flow | All speech through AnnouncementRouter | Single point for dedup, priority, formatting |
| Patch style | Thin Postfix-preferred, try-catch wrapped | Minimizes breakage, prevents game crashes |
| Configuration | JSON file, runtime-changeable | Simple, no dependency on UI system initially |
| Build system | Single .csproj referencing ONI DLLs | Standard for ONI mods, builds with dotnet/MSBuild |

---

## 12. Open Questions for Future Research

1. **How does ONI's `KInputHandler` priority chain work exactly?** Need to determine the right insertion point for accessibility input to ensure it processes before or after game input as appropriate.
2. **Can we hook into LocText.text setter to capture all UI text changes?** This could be a powerful way to detect any visible text change and announce it.
3. **What is the performance cost of `Grid.Objects[]` lookups per cell?** Need to profile whether inspecting cells during rapid cursor movement causes frame drops.
4. **How does the research tree UI work?** It uses a custom graph layout that may need a specialized navigation approach beyond simple list cursor.
5. **What screen reader behaviors should we account for that Tolk abstracts away?** E.g., does JAWS handle interrupt differently than NVDA? Do we need SR-specific codepaths?

---

## Confidence Assessment

| Aspect | Confidence | Notes |
|--------|-----------|-------|
| Overall architecture (layered pipeline) | **High** | Well-established pattern in game accessibility mods. FactorioAccess validates the approach. |
| Harmony integration | **High** | Standard ONI modding pattern. Many reference mods available. |
| Speech output via Tolk | **High** | Already prototyped. Tolk is the standard for game accessibility. |
| State reading approach | **Medium-High** | ONI's component system is well-documented in decompiled code. Some edge cases unknown. |
| Navigation design | **Medium-High** | Cursor concept proven by FactorioAccess. ONI's 2D grid is simpler than Factorio's in some ways. |
| Menu navigation | **Medium** | KScreen subclasses vary significantly. Will need per-screen adapters. |
| Build order | **High** | Bottom-up dependency chain is clear and logical. |
| Performance assumptions | **Medium** | Need profiling to confirm. Grid lookups should be fast but speech dedup timing needs tuning. |

---

*Research completed: 2026-02-11. This architecture document informs the phase structure in the project roadmap. Components should be built in the order specified in Section 7, with each phase delivering a testable, incremental improvement in accessibility.*
