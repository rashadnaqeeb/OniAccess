# Tile System — Design and Architecture

Design and architecture for the tile cursor, progressive disclosure speech, and supporting systems.

---

## Tile Cursor

Arrow keys move a cell index across the grid, one cell per press. The cursor is the foundation layer — always active when the map is visible. Tool-specific handlers (build placement, dig marking, etc.) layer on top of it and may add their own key bindings, but the underlying cursor and glance remain available.

### Position pipeline

The cell index is converted to screen space via `Camera.main.WorldToScreenPoint(Grid.CellToPos(cell))` and written to `KInputManager.lockedMousePos` (`KInputManager.isMousePosLocked = true`). This redirects the entire game input pipeline — hover tooltips, tool actions, selection — to the cursor cell with no additional patches.

### Camera follow

The camera follows the cursor. When the cursor moves to a cell outside the current viewport, the camera pans to keep it visible via `CameraController.Instance.SetTargetPos` (smooth pan) or `SnapTo` (immediate jump, e.g., on world switch).

### World bounds

The cursor is clamped to the current world's playable area using `WorldContainer.minimumBounds` and `WorldContainer.maximumBounds`, minus `Grid.TopBorderHeight` for the top border. `Grid.WorldIdx[cell]` tracks which world a cell belongs to for multi-world (Spaced Out DLC) awareness.

### Initial position

On first entering the game world, the cursor starts at the Printing Pod. The Printing Pod is locatable via the `Telepad` component: `Components.Telepads[0]` gives the active world's pod, and `Grid.PosToCell()` converts its position to a cell index. On switching asteroids, it starts at the center of the target world's viewport.

### Fog of war

`Grid.Visible[cell] == 0` gates all downstream reading. The cursor can move onto fog cells, but the glance reads "unexplored" and all three depths (glance, tooltip, inspect) skip cell content. ONI fog is binary: unexplored (black) or permanently revealed with live simulation data. There is no "previously explored but currently unseen" state.

### Coordinates

Read on demand via backtick (`` ` ``). Speaks the current cell's X, Y position within the world.

Shift+Backtick cycles a per-session toggle: Off (default) -> Append -> Prepend -> Off. When set to Append or Prepend, coordinates are automatically attached to the end or beginning of every glance announcement.

---

## Progressive Disclosure

Three depths of information, mirroring how sighted players process tile data: glance, hover for detail, click for full inspection.

### Depth 1 — Glance (automatic on cursor move)

Spoken automatically each time the cursor lands on a new cell. The glance line is built fresh from grid state on every read — never cached. Each new cell interrupts any in-progress speech from the previous cell.

#### Default glance (no overlay active)

Content is spoken in this order:

1. **Buildings + status**: Checks `ObjectLayer.Building`, `ObjectLayer.FoundationTile`, `ObjectLayer.Backwall`. Reads all buildings found at the cell — including every cell of a multi-tile building. Each building includes its active status items from `StatusItemGroup` (Flooded, Entombed, Overheated, Working, Idle, etc.). Doors include access state (open, locked, auto). Under-construction buildings are flagged via `Constructable` component. For multi-tile buildings, the glance also indicates:
   - **Work cell**: If this cell is a `Workable` offset (where a duplicant stands to operate the building).
   - **Material input/output**: If this cell is where materials are delivered or where products come out.
   - **Utility ports**: Specific input/output cells for pipes, wires, power, automation, and conveyor are spoken **only when the corresponding overlay is active**.

2. **Element**: `Grid.Element[cell].name`. Handles Vacuum and Void as special cases. **Suppressed when a foreground building or foundation tile is present** — the important cases (submerged, entombed) are already covered by building status items. Depth 2 gives full element detail if needed. Element is still spoken when only a Backwall (drywall, tempshift plate) is present, since sighted players see the element through background buildings.

3. **Entities**: `ObjectLayer.Minion` (duplicant name), `ObjectLayer.Critter` (species name), `ObjectLayer.Plants` (name + growth state from `Growing` component).

4. **Pending orders**: Dig (`Diggable` on layer 7), mop (`Moppable` on layer 8), build (`Constructable`), sweep (`Clearable.IsMarkedForClear()` on debris). Includes work priority from `Prioritizable.GetMasterPriority()`.

5. **Debris** (always last): `ObjectLayer.Pickupables`, traversing the `ObjectLayerListItem` linked list. All items listed by name. Because debris is last, the user can interrupt at any point once they've heard enough.

This ordering puts the most spatially distinguishing content first (building or element name), so even a partial utterance during fast navigation is useful.

#### Overlay glance profiles

When an overlay is active, the overlay's profile provides its own ordered list of sections. Each profile is self-contained — it lists exactly which sections to include and in what order. A profile that wants "overlay info first, then the full default" simply lists its overlay section followed by all five default sections. A profile that wants to drop irrelevant defaults just omits them.

Non-exhaustive examples:

- **Power**: Wire/building name, wattage consumed (-) or generated (+), circuit status (unpowered / overloading / straining / safe).
- **Liquid Conduits**: Pipe name, pipe type (normal / insulated / radiant), conduit contents via `ConduitFlow.GetContents(cell)` — element, mass, temperature, germs.

Overlay profiles for all 19 gameplay overlays will be defined during implementation.

### Depth 2 — Tooltip (Q key)

Reads the game's own hover tooltip text by intercepting `HoverTextDrawer` via Harmony patch. As `SelectToolHoverTextCard` calls `DrawText()`, `BeginShadowBar()`, `NewLine()`, etc., the strings are captured into a buffer. SpeechPipeline handles filtering out non-speech formatting (icons, colors).

This gives us the exact text the sighted player sees in the tooltip, already localized and overlay-sensitive, without duplicating the game's tooltip logic.

#### HoverTextDrawer patch approach (verified)

`HoverTextDrawer` is a plain non-sealed class (`HoverTextDrawer.cs`). All drawing methods are regular public instance methods — no static, abstract, or sealed modifiers. `HoverTextScreen` holds a single reusable instance (`HoverTextScreen.drawer`) returned by `HoverTextScreen.Instance.BeginDrawing()`.

`DrawText` has two overloads. Overload 1 (`string, TextStyleSetting`) delegates to overload 2 (`string, TextStyleSetting, Color, bool`), so patching only overload 2 captures all text.

Patch targets:
- **`DrawText` (4-param overload)** — prefix captures the `string text` argument into a per-block buffer.
- **`BeginShadowBar`** — prefix resets the buffer (marks start of a new logical group: entity or overlay section).
- **`EndShadowBar`** — postfix flushes the buffer (one complete block of tooltip text).
- **`EndDrawing`** — optional postfix to detect end of the full hover frame.

### Depth 3 — Inspect (Enter key)

Enter opens the game's inspect panel at the current cell via `SelectTool.Instance.Select()`. While the panel is open, Tab / Shift+Tab cycles through overlapping selectables at that cell (replacing the game's repeated-click cycling with a standard screen reader pattern). Tab is consumed only while the panel is active.

Selectables at a cell are sorted by a priority ranking (e.g., Duplicants > Critters > Buildings > Plants > Debris — exact tiers determined during implementation). Within a priority tier, ties are broken by a stable key (e.g., instance ID). This ordering is deterministic regardless of creation order, so cycling is predictable across visits to the same cell.

Reads content from the `DetailsScreen` including status items, errands, and material tabs.

---

## Architecture

### Folder layout

```
OniAccess/
  Handlers/
    Screens/
      ...existing screen handlers...
    Tiles/
      TileCursorHandler.cs       -- BaseScreenHandler bound to Hud KScreen; owns TileCursor,
                                    GlanceComposer; routes Q/Enter/backtick keys
      TileCursor.cs              -- Cell index, arrow key movement, camera follow, world bounds,
                                    coordinate reading and coordinate mode toggle
      GlanceComposer.cs          -- Runs section list, sends result to SpeechPipeline
      TooltipInterceptor.cs      -- HoverTextDrawer Harmony patches (Depth 2)
      TileInspector.cs           -- Enter/Tab inspect panel (Depth 3)

      Sections/
        ICellSection.cs          -- Interface: Read(int cell) -> speech fragments
        BuildingSection.cs       -- Buildings + status items + construction state.
                                    Dispatches to per-building-type describers as complexity
                                    grows (e.g., generators speak wattage, doors speak access
                                    state, storage bins speak contents summary)
        ElementSection.cs        -- Element name (suppressed behind foreground buildings)
        EntitySection.cs         -- Duplicants, critters, plants
        OrderSection.cs          -- Pending dig/mop/build/sweep orders
        DebrisSection.cs         -- Pickupables linked list

      Overlays/
        OverlayProfile.cs        -- Data class: ordered list of ICellSection for one overlay
        OverlayProfileRegistry.cs -- Maps overlay HashedString IDs to profiles
        (per-overlay sections as needed, e.g., PowerOverlaySection.cs)
```

### Handler integration

#### Stack position

`TileCursorHandler` extends `BaseScreenHandler` and binds to the `Hud` KScreen. `Hud` is a minimal `KScreen` subclass (10 lines) created by `Game.SpawnPlayer()` when the game world loads, and destroyed on scene teardown (return to main menu). Its lifecycle is exactly "game world is active." No code calls `Hud.Deactivate()`, `Hud.Show(false)`, or destroys it mid-session — overlays, menus, and tool changes do not touch it.

`BaselineHandler` remains at the bottom of the stack as a permanent sentinel. It is active during screen transitions, loading, and menus where no game world exists. `TileCursorHandler` pushes on top of it via the standard ContextDetector lifecycle.

```
Stack when game world is active, no menus open:
[bottom]  BaselineHandler        (permanent sentinel, CapturesAllInput=false)
[top]     TileCursorHandler      (CapturesAllInput=false, active)

Stack when a menu is open on top of the game world:
[bottom]  BaselineHandler        (permanent sentinel)
[middle]  TileCursorHandler      (CapturesAllInput=false, shadowed)
[top]     SomeMenuHandler        (CapturesAllInput=true)

Stack before game world loads (main menu, world gen, etc.):
[bottom]  BaselineHandler        (permanent sentinel)
[top]     MainMenuHandler / etc. (pushed by ContextDetector)
```

Because `TileCursorHandler` has `CapturesAllInput=false`, it receives `Tick()` calls only when no capturing handler sits above it. When a menu pushes on top with `CapturesAllInput=true`, the tile cursor is shadowed automatically — no suspend/resume logic needed.

#### Activation

Registration in `ContextDetector.RegisterMenuHandlers()`:

```csharp
Register<Hud>(screen => new TileCursorHandler(screen));
```

This is the only registration needed. `Hud.Activate()` fires through the standard `KScreen.Activate` postfix patch, pushing `TileCursorHandler`. `Hud.Deactivate()` fires through the standard `KScreen.Deactivate` prefix patch, popping it. The stale-handler guard in `KeyPoller` also works for free since `BaseScreenHandler.Screen` points to the `Hud` instance.

`ContextDetector.DetectAndActivate()` (called on mod toggle-ON) already walks `KScreenManager.screenStack` and will find the `Hud` screen if the game world is active, pushing the handler automatically. No special-casing needed.

#### Mod toggle

The Ctrl+Shift+F12 toggle lives in `KeyPoller.Update()`, outside the handler stack entirely. It works regardless of which handler is active or whether the mod is enabled. `VanillaMode.Toggle()` calls `HandlerStack.DeactivateAll()` on disable (which pops TileCursorHandler along with everything else) and `ContextDetector.DetectAndActivate()` on enable (which rediscovers `Hud` and repushes TileCursorHandler). No reimplementation needed.

#### Map visibility

The map is visible when `TileCursorHandler` is the active handler (top of stack). This is guaranteed by the existing stack mechanics:
- Menu handlers push on top with `CapturesAllInput=true`, shadowing the cursor
- When menus pop, `TileCursorHandler` becomes active again automatically
- `KeyPoller`'s stale-handler cleanup ensures menu handlers don't linger if their screen is destroyed without firing `Deactivate`

No explicit "is map visible" check is needed. The handler stack already encodes this.

### Class responsibilities

#### TileCursor

Owns the cell index and coordinate reading. Provides:
- `Move(Direction)` — arrow key movement, clamps to world bounds
- `Cell` (int) — current cell index
- `WorldPosition` — `Grid.CellToPos(Cell)` for camera targeting
- `GetCoordinateText()` — returns "X, Y" string for the current cell (world-relative coordinates)
- `CoordinateMode` — per-session toggle: `Off` (default), `Append`, `Prepend`
- `CycleCoordinateMode()` — advances Off -> Append -> Prepend -> Off, speaks the new mode name

Writes `KInputManager.lockedMousePos` on every move. Tells `CameraController` to follow. Knows nothing about speech except for coordinate text generation.

#### GlanceComposer

Holds two things:
- A default section list (the five standard sections in order)
- An `OverlayProfileRegistry` for overlay-specific section lists

On cell change, determines the active section list:
1. If an overlay is active and has a registered profile, use that profile's section list
2. Otherwise, use the default list

Runs each section's `Read(cell)` in order, collects non-empty results, sends the concatenated text to `SpeechPipeline.SpeakInterrupt()`. Checks `TileCursor.CoordinateMode` and attaches coordinate text to the beginning or end of the output if enabled.

Listens for `OverlayScreen.Instance.OnOverlayChanged` to know when to switch profiles.

#### ICellSection

```csharp
public interface ICellSection
{
    IEnumerable<string> Read(int cell);
}
```

Each section queries live game state for the given cell and returns zero or more speech fragments. Returning empty means the section is suppressed for this cell (e.g., `ElementSection` returns nothing when a foreground building is present).

Sections are stateless — no cached data between calls. They are shared instances reused across profiles.

#### OverlayProfile

```csharp
public class OverlayProfile
{
    public IReadOnlyList<ICellSection> Sections { get; }
}
```

Just an ordered list of sections. A Power overlay profile might contain `[PowerOverlaySection, BuildingSection, EntitySection]`. An Oxygen overlay profile might contain `[OxygenOverlaySection, BuildingSection, ElementSection, EntitySection, OrderSection, DebrisSection]` (nearly the full default, with an overlay section prepended).

Profiles are registered in `OverlayProfileRegistry` keyed by overlay `HashedString` ID.

#### TileCursorHandler

The `BaseScreenHandler` subclass that wires everything together. Bound to the `Hud` KScreen. Owns:
- `TileCursor` instance
- `GlanceComposer` instance

In `Tick()`:
- Arrow keys -> `TileCursor.Move()` -> `GlanceComposer.Speak(cursor.Cell)`
- Q -> trigger `TooltipInterceptor` read
- Enter -> trigger `TileInspector` open
- Backtick -> speak coordinates
- Shift+Backtick -> cycle coordinate mode

`CapturesAllInput = false` so game hotkeys (overlays, tools, WASD camera, pause) pass through.

#### TooltipInterceptor

Static Harmony patches on `HoverTextDrawer`:
- `DrawText` (4-param overload) prefix: appends `text` to a static buffer
- `BeginShadowBar` prefix: resets the buffer (new logical block)
- `EndShadowBar` postfix: flushes buffer to a completed-blocks list
- `EndDrawing` postfix (optional): marks frame complete

Exposes a method like `GetTooltipText()` that returns the captured text from the last frame. `TileCursorHandler` calls this when Q is pressed.

The patches run every frame (the game updates hover text continuously), but the captured text is only read on demand.

#### TileInspector

Handles Depth 3. On Enter:
1. Gets the `KSelectable` at the current cell via `Grid.Objects` lookup
2. Calls `SelectTool.Instance.Select(selectable)` to open `DetailsScreen`
3. While the panel is open, consumes Tab/Shift+Tab to cycle through overlapping selectables at the cell
4. Reads panel content from `DetailsScreen`

---

## Implementation Order

### Phase 1 — Cursor ✓

- [x] `TileCursor` — cell index, movement, bounds clamping, `KInputManager.lockedMousePos` write, camera follow
- [x] `TileCursorHandler` — register with `Hud`, arrow keys call `TileCursor.Move()`
- [x] Speak element name directly as a hardcoded proof-of-life (no section system yet)
- [x] Coordinate reading (backtick) and coordinate mode toggle (Shift+backtick)

### Phase 2 — Glance sections ✓

- [x] `ICellSection` interface
- [x] `GlanceComposer` (runs section list, replaces the hardcoded element speak from Phase 1)
- [x] `ElementSection` — suppressed behind foreground buildings/foundation tiles
- [x] `EntitySection` — dupes via ObjectLayer.Minion, critters via CreatureBrain on Pickupables, plants with growth state
- [x] `OrderSection` — dig, mop, sweep with priority
- [x] `DebrisSection` — filters out dupes/critters to avoid double-reporting
- [x] `BuildingSection` — status items, multi-tile, construction state, door access (via status items), overlay-gated utility/power/automation ports
- [x] Work cell annotation evaluated and removed (Workable offsets overlap building cells, too noisy)
- [x] Material input/output evaluated and skipped (input and output are almost always the building origin cell)

### Phase 3 — Tooltip (Depth 2) ✓

- [x] `TooltipCapture` — static service capturing HoverTextDrawer output per frame
- [x] `HoverTextDrawerPatches` — 6 thin Harmony postfix patches (BeginDrawing, BeginShadowBar, DrawText, DrawIcon, NewLine, EndDrawing)
- [x] Q key wired in `TileCursorHandler` with fog-of-war gate
- [x] Strings: `READ_TOOLTIP` help entry, `NO_TOOLTIP` fallback
- [x] 8 offline unit tests for TooltipCapture

### Phase 4 — Overlay profiles

- `OverlayProfile`, `OverlayProfileRegistry`
- `GlanceComposer` hooks `OverlayScreen.OnOverlayChanged`
- Build one or two overlay sections (Power is a good first candidate) to prove the system, then fill in the rest incrementally

### Phase 5 — Inspect (Depth 3)

- `TileInspector` — Enter opens panel, Tab cycles selectables
- Side screen accessibility work

---

## Design Rationale

### Why no explicit passability call-out (solid/open)
The element name (Granite, Oxygen) and building name (Insulated Tile, Mesh Tile) already convey passability. An explicit "solid" or "open" label would be redundant.

### Why element is suppressed behind buildings
A sighted player can't see the gas behind a building either. The normal case (building sitting in oxygen) is noise. The important cases — building submerged in liquid or entombed in solid — are surfaced by the game's own status items (Flooded, Entombed), which are read as part of the building info.

### Why all debris items are listed
With speech interruption, length only matters for content that precedes what the user cares about. Debris is always last in the ordering, so the user interrupts as soon as they've heard enough. No truncation needed.

### Why overlay profiles are just section lists
Each profile is an ordered list of sections — no "prepend vs replace" mode flag. A profile that wants the full default glance with overlay info prepended simply lists all sections in that order. A profile that wants to drop irrelevant defaults omits them. This is simpler and more explicit than a mode flag, and every profile can be tuned independently.

### Why no MessageBuilder between sections and speech
Sections return `IEnumerable<string>` and GlanceComposer concatenates them. If formatting bugs (double separators, trailing commas, inconsistent spacing) become a pattern, the fix belongs in `SpeechPipeline` — it already handles text filtering and is the single chokepoint for all speech output. No new assembly layer is needed.
