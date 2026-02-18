# Tool System Architecture

This document describes how the tool interaction system integrates with the existing OniAccess architecture. Read `tool-interaction-design.md` for the user-facing interaction model and `tool-system.md` for the game's tool internals.

## Reading Pipeline

The tile reading system manages a profile priority stack: tool > overlay > default. When a tool is active, its profile is used. When no tool is active, the existing overlay/default behavior applies. TileCursorHandler calls "read cell" and gets the right tokens regardless of context, without knowing whether a tool is active.

Each tool has its own profile in the tile reading system, built from ICellSection implementations following the same pattern as overlay profiles. Tool-specific sections (dig validity/hardness, mop liquid info, disinfectable objects, sweepable items, etc.) live alongside existing sections like BuildingSection and ElementSection. A SelectionSection queries the tool handler's selection state to prepend "Selected" when the cursor is on a selected cell.

Tool profiles replace overlay profiles entirely while active. Overlay-specific sections (Disease, Decor, Light, Radiation) do not appear during tool mode. Tool mode is a focused interaction where the player is acting on the world, not exploring it. Each tool's readout is self-contained and matches the per-tool specifications in `tool-interaction-design.md`. If a specific tool later needs overlay information, the section can be added to that tool's profile explicitly.

### Profile Priority

The tile reading system checks tool profile first, then overlay profile, then default. This is a single check inserted into the existing code path that selects a composer: if the tool profile registry has an active profile, use it; otherwise fall back to the existing overlay/default logic. The tool profile registry exposes the active profile (or null) based on the current game tool state. No unified registry needed -- the tool check is a short-circuit before the existing overlay lookup.

## Input Routing

The tool handler is a non-modal handler (`CapturesAllInput = false`) that consumes only its specific keys: Space (set corners), Enter (confirm), Escape (cancel), number row (priority), F (filter menu), Delete/Backspace (clear rectangle), and Ctrl+Arrow (jump to selection boundary). All other keys, including plain arrow keys, pass through to TileCursorHandler below for standard navigation.

Plain arrow movement stays in TileCursorHandler. Since the reading system knows the active tool profile, TileCursorHandler reads cells correctly without any awareness of the tool system. Ctrl+Arrow is the only movement the tool handler owns, because it depends on rectangle selection boundaries that only the tool handler knows about.

## TileCursor as Shared Service

TileCursor is a shared service rather than an internal detail of TileCursorHandler. It represents "where is the player's attention in the world" and is the single source of truth for cursor position. TileCursorHandler uses it for standard navigation. The tool handler uses it for Ctrl+Arrow boundary jumping and for reading the current cell when setting rectangle corners.

TileCursor owns a cell index and, as a side effect, locks the game's mouse position to that cell and snaps the camera. All movement logic operates on cell indices. The mouse lock is an implementation detail that makes the game's mouse-driven systems cooperate with the mod's keyboard-driven cursor.

## File Organization

### Interaction (Handlers)

- **Tool picker menu** -- BaseMenuHandler subclass. Opens from a hotkey, lists all standard toolbar tools, supports type-ahead search. Selection activates the game's tool.
- **Tool filter menu** -- BaseMenuHandler subclass. Opens from F during tool mode or as the Harvest mode prompt before activation. Takes a list of filter/mode options, returns the selection.
- **Tool handler** -- Non-modal handler. Manages rectangle state, priority tracking, Ctrl+Arrow navigation, and confirmation. Delegates all cell reading to the tile reading system.

### Reading Pipeline (Tile System)

- **Tool profile registry** -- Analogous to OverlayProfileRegistry. Maps active tool types to GlanceComposer instances. Queried by the tile reading system when determining which profile to use.
- **Tool-specific ICellSection implementations** -- Per-tool sections that read tool-relevant data (dig hardness, mop liquid info, disinfectable objects, sweepable items, capturable creatures, order state, etc.). Live alongside existing sections like BuildingSection and ElementSection.
- **SelectionSection** -- Queries the tool handler's rectangle selection state. Returns "Selected" when the cursor is on a selected cell, nothing otherwise.

### Stack Flows

Tool activation from picker:
```
[..., TileCursor, ToolPicker] -> select -> [..., TileCursor, ToolHandler]
```

Filter change during tool mode:
```
[..., TileCursor, ToolHandler, ToolFilterMenu] -> select -> [..., TileCursor, ToolHandler]
```

Harvest from picker (mode prompt before activation):
```
[..., TileCursor, ToolPicker] -> select Harvest -> [..., TileCursor, ToolFilterMenu] -> select mode -> [..., TileCursor, ToolHandler]
```

The tool handler also activates in response to the game's `ActiveToolChanged` event (hash 1174281782), so it works regardless of how the tool was activated -- mod menu, game hotkey, or any other source.

## Tool Handler Lifecycle

The tool handler subscribes to the game's `ActiveToolChanged` event. When a non-SelectTool activates, the handler pushes itself onto the stack. When SelectTool activates (meaning any tool was deactivated), the handler pops itself. This covers all deactivation sources: Enter/Escape from the handler itself, but also external deactivation via game hotkey re-press, overlay conflict, or right-click. The handler does not need to enumerate deactivation causes -- it reacts to the resulting event.

On push, the handler reads the active tool's state (name, filter, priority) from the game and announces it. On pop, it discards rectangle state and clears the tool profile from the reading system.

## Prerequisites

**TileCursor extraction** -- TileCursor is currently created and owned by TileCursorHandler. Before the tool system can be built, TileCursor must be extracted into a shared service. This is a small refactor: TileCursor gets a static Instance property, created when Hud activates, cleared when Hud deactivates. TileCursorHandler and the tool handler both consume it. No behavioral change to existing code.
