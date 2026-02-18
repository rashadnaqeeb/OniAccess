# OniAccess Tool Interaction Design

## Overview

This document describes how OniAccess replaces ONI's mouse-driven drag-to-apply tool system with a keyboard-driven, screen-reader-accessible workflow. The core model is: open menu, select tool, define one or more rectangles, review, confirm, tool closes.

## Architecture

The tool interaction system is implemented as a handler that sits on top of the tile cursor. When a tool is active, this handler intercepts keys it needs (Space for corners, Delete/Backspace for clearing, Enter for confirm, Escape for cancel, number row for priority, F for filter, Ctrl+Arrows for jump navigation) and passes everything else down the chain to the tile cursor for standard navigation. This means the tool system's keybindings only exist while a tool is active. When no tool is active, all keys behave normally.

The handler activates in response to the game's `ActiveToolChanged` event (hash `1174281782`), not just the mod's own menu. This means it works regardless of how the tool was activated — whether from the mod's tool menu, a game hotkey, or any other source. For Harvest, if activated externally without a mode choice, the handler prompts for mode before proceeding.

## Tool Selection

A dedicated key opens the tool menu. If a tool is already active, the key deactivates it first, then opens the menu. This menu lists all standard toolbar tools:

- **Standard box tools:** Dig, Mop, Disinfect, Sweep. No filters. Priority supported.
- **Filtered box tools:** Cancel, Deconstruct, Empty Pipe. Filter set by the game on activation (overlay-matched or All), changeable with F. Cancel and Deconstruct support the full filter set (All, Power Wires, Liquid Pipes, Gas Pipes, Conveyor Rails, Buildings, Automation, Background Buildings). Cancel additionally offers Sweep & Mop Orders and Dig Orders filters. Empty Pipe offers a reduced set (All, Liquid Pipes, Gas Pipes, Conveyor Rails). Deconstruct and Empty Pipe support priority; Cancel does not.
- **Errand-filtered box tool:** Prioritize. Filter set is errand types (All, Construction, Digging, Cleaning, Duties). Changeable with F. Supports priority.
- **Mode-toggle box tool:** Harvest. Presents a mode choice (Enable Harvest or Disable Harvest) when selected from the menu, before the tool activates. Changeable with F after activation. Supports priority.
- **Entity box tools:** Attack, Capture. Standard rectangle selection but review and summary report entities, not cells. Both support priority.
- **Special:** Disconnect. Line mode, maximum 2 cells. Layer filters (same as Deconstruct but without Background Buildings). No priority.

Build tools (BuildTool, UtilityBuildTool, WireBuildTool, PlaceTool) and CopySettingsTool are excluded; they are handled by separate systems. Sandbox tools are out of scope.

The menu supports type-ahead search. The player starts typing a tool name and the list narrows. This removes the need to memorize the base game's arbitrary hotkeys (G for Dig, K for Sweep, N for Capture, etc.), freeing those keys for use by the mod.

The tool activates immediately on selection. The mod calls the game's tool activation internally, so all standard game-side effects apply (overlay switching, filter initialization, event firing, hover text updates). For filtered tools, the game sets the initial filter automatically: if a matching overlay is active, the filter locks to that overlay's layer; otherwise it defaults to All. The player can change filters or mode after activation with F.

On activation, the mod reads the tool name, current filter (if any), and current priority from the game's actual tool state and announces them (e.g., "Dig tool, priority 5" or "Deconstruct tool, Liquid Pipes, priority 5"). This confirms the correct tool activated and gives the player the starting state. The tool handler begins intercepting keys.

### Tool Selection Sound

When the player selects a tool from the menu, the mod plays `UISounds.Sound.ClickObject`, matching the base game's toolbar selection sound.

### Overlay-Filter Coupling

If the player changes overlays while a filtered tool is active, the filter auto-switches to match, consistent with the base game's `FilteredDragTool.OnOverlayChanged()` behavior. Switching to a non-matching overlay restores the normal filter set. An overlay-triggered filter change clears any existing selection and pending first corner. The new filter is announced, followed by "Selection cleared" if a selection existed.

### Changing Filters While Tool Is Active

Pressing F while a tool is active reopens the filter menu for that tool. Changing the filter clears any existing selection and any pending first corner, since the filter changes what cells are valid and what order state is shown. The player defines new rectangles under the new filter. For tools without filters, F is swallowed (not passed down the chain) to prevent accidental actions.

## Cursor and Cell Information

While a tool is active, the player navigates the map with arrow keys (handled by the tile cursor, passed through from the tool handler). Each cell's readout follows this order: validity (only if invalid), order state, tool-specific physical state, then unsuppressed standard cursor sections. Tool-specific information is filtered to match the active tool's scope — for example, if Cancel is set to Liquid Pipes, only liquid pipe objects are read, not buildings or wires.

### Physical State and Standard Cursor Suppression (per tool)

Each tool reads specific data and suppresses standard cursor sections that would duplicate or clutter the readout. "Suppress" means the section is omitted from the standard cursor output for the duration of the tool.

Suppressions are always active while the tool is active. If nothing relevant to the tool exists at the cell, no tool-specific info is prepended, but the suppressed sections remain suppressed. The unsuppressed sections (element, building) always provide orientation. Reuse game strings (e.g., `UI.TOOLS.CAPTURE.NOT_CAPTURABLE`) where the game already defines them. Validity rejection reasons (e.g., "Too much liquid," "Not on floor") also come from game strings.

- **Dig:** readout order: element, material category, hardness, building. The tool inserts category and hardness into the standard cursor's existing flow. On a foundation tile, the existing element rules mean only the building is read, which naturally tells the player this cell is not diggable. Suppress: Order (duplicates tool order state), Debris, Entity.
- **Mop:** readout order: element, building. The standard cursor's element section already provides the liquid name and mass. The tool adds nothing new, just suppresses noise. Suppress: Order, Debris, Entity.
- **Disinfect:** readout order: disinfectable objects with disease type and germ count each, element, building. Actable information first. The tool prepends per-object disease info for disinfectable objects with nonzero germs. Objects with zero germs are skipped. Building stays for orientation, but objects already mentioned in the disinfectable readout are not repeated. Suppress: Order, Debris, Entity, Disease (replaced by per-object germ info).
- **Sweep:** readout order: sweepable items (clearable pickupables, excluding duplicants), element, building. Actable information first. Suppress: Order, Debris (replaced by tool readout), Entity.
- **Attack:** readout order: attackable creatures (hostile faction), element, building. Actable information first. Suppress: Order, Debris, Entity (replaced by tool readout).
- **Capture:** readout order: capturable creatures, element, building. Actable information first. Non-capturable creatures use the game's NOT_CAPTURABLE string. Suppress: Order, Debris, Entity (replaced by tool readout).
- **Harvest:** readout order: building, harvest designation, element. Plants are buildings, and the standard Building section already includes growth status. The tool prepends the current harvest designation ("harvest when ready" or "do not harvest") so the player knows what they'd be changing. Suppress: Order, Debris, Entity.
- **Deconstruct:** readout order: built objects with material prefix scoped to active filter, element, building. Actable information first. If filter is All, list all built objects (e.g., "Granite Tile, Gold Amalgam Liquid Pipe, Copper Wire"). If filter is a specific layer, list only matching objects. Building stays for orientation, but objects already mentioned in the tool readout are not repeated. Suppress: Order, Debris, Entity.
- **Cancel:** readout order: cancellable objects/orders scoped to active filter, element, building. Actable information first. For layer filters, same as Deconstruct (built objects matching the filter). For Dig Orders and Sweep & Mop Orders filters, no physical objects are read (the readout is order-focused). Building stays for orientation, but objects already mentioned in the tool readout are not repeated. Suppress: Order (replaced by tool readout), Debris, Entity.
- **Prioritize:** readout order: orders with current priorities scoped to active errand filter, element, building. Actable information first. If filter is All, list all orders. If filter is Construction, only construction and deconstruction orders. If filter is Digging, only dig orders. If filter is Cleaning, only sweep, mop, and storage orders. If filter is Duties, everything else. Suppress: Order (replaced by detailed priority readout), Debris, Entity.
- **Empty Pipe:** readout order: conduit type and contents (element + mass) scoped to active filter, element, building. Actable information first. Empty conduits announced as the conduit type followed by "empty." Building stays for orientation, but conduits already mentioned in the tool readout are not repeated. Suppress: Order, Debris, Entity.
- **Disconnect:** readout order: connection type and directions (e.g., "Liquid Pipe, connecting east and west") scoped to active filter, element, building. Actable information first. Building stays for orientation, but objects already mentioned in the tool readout are not repeated. Suppress: Order, Debris, Entity.
- **All tools:** if the cell is invalid for the active tool, the game's reason string is prepended to everything (e.g., "Too much liquid, Water, 200 kg, ..."). Valid cells have no validity announcement — absence of a reason implies validity.

### Order State (per tool)

Order state is prepended to the physical state and standard cursor sections described above. The full readout order for any cell is: order state, then tool-specific physical state, then unsuppressed standard cursor sections. This surfaces information that sighted players get from visual markers, icons, and colored overlays on the map.

- **Dig:** if a dig order exists at the cell, announce "Dig order, priority [n]."
- **Mop:** if a mop order exists, announce "Mop order, priority [n]."
- **Disinfect:** if already marked for disinfection, announce "Marked for disinfect."
- **Sweep:** if items at the cell are already marked for sweeping, announce "Marked for sweep, priority [n]."
- **Harvest:** covered by harvest designation in the physical state section.
- **Attack:** if a creature is already marked for attack, announce "Marked for attack."
- **Capture:** if a creature is already marked for capture, announce "Marked for capture."
- **Cancel:** announce what orders exist that the current filter would cancel. If filter is Dig Orders, only announce dig orders. If filter is All, announce all cancellable orders.
- **Deconstruct:** if the matching object is already marked for deconstruction, announce "Marked for deconstruct, priority [n]." Scoped to the active filter.
- **Prioritize:** announce the current priority of each matching order at the cell, scoped to the active errand filter. E.g., "Dig order, priority 5" or "Construction order, priority 3." This is critical because PrioritizeTool changes priorities of existing orders rather than creating new ones.
- **Empty Pipe:** if the matching conduit is already marked for emptying, announce "Marked for emptying." Scoped to the active filter.
- **Disconnect:** announce existing connections at the cell relevant to the active filter.

## Rectangle Selection

The player defines selections by setting two corners of a rectangle.

1. Move the cursor to the first corner and press Space. The mod announces "Corner set" and plays the tool's drag sound with `tileCount = 1`.
2. Move the cursor to the second corner and press Space. The mod announces a summary and plays the tool's drag sound with `tileCount` set to the rectangle's area. For cell-based tools, the summary is dimensions and valid/invalid counts (e.g., "5 by 3, 12 valid, 3 invalid"). For entity-based tools (Attack, Capture), the summary is the number of creatures in the rectangle. All cells in the rectangle are selected, including invalid ones. Invalid cells are still announced as "Selected" during review but their invalidity reason is prepended, telling the player they will be skipped on confirm.
3. The player can define additional rectangles by repeating the process. Each new rectangle gets a brief summary on completion.
4. A single cell is selected by pressing Space twice on the same cell.

If both corners share a row or column, the result is a straight line. No special case is needed; a line is just a rectangle with one dimension equal to 1.

### Drag Sounds Per Tool

The drag sound played when setting corners varies by tool, matching the base game:

- **Most tools:** "Tile_Drag"
- **Cancel, Disconnect:** "Tile_Drag_NegativeTool"

The `tileCount` FMOD parameter encodes the number of cells in the selection, providing an auditory sense of selection size.

### Reviewing a Selection

After defining one or more rectangles, the player can arrow through the selected area. Selected cells are announced with "Selected" prepended to their information (including both physical and order state).

Ctrl+Arrow jumps in the arrow's direction: to the next selected cell when on an unselected cell, or to the next unselected cell when on a selected cell. If no change is found before the edge of the current world, the mod announces "No change" and the cursor stays put. This allows quick auditing of selection boundaries and gaps without traversing every cell.

### Clearing a Rectangle

Delete or Backspace clears the most recently defined rectangle that overlaps the cursor's current cell. If the cell belongs to multiple overlapping rectangles, the most recent one is cleared. If the cursor is not on a selected cell, Delete/Backspace does nothing but is still consumed.

### Overlapping Rectangles

Multiple rectangles may overlap. The selection is treated as a union: cells in the overlap are only processed once. Summaries report the union count, not the sum of individual rectangle counts.

## Priority

Priority is set using the number row (1 through 9) at any time while a tool is active. 0 sets emergency priority (topPriority class, which outranks all normal priorities regardless of numeric value). Keys 1 through 9 set basic class at the corresponding value. The default is basic class, value 5.

The current priority is announced when the tool activates and whenever it changes. When priority changes, the mod plays `PlayPriorityConfirmSound()` with the FMOD parameter encoding the new priority value, matching the base game's audio feedback.

All selected cells receive whatever priority is set at the moment the player confirms. Priority is not stored per-rectangle. If different priorities are needed for different areas, the player should use separate tool activations. When confirming, the mod announces the priority being applied as part of the confirmation summary.

Priority applies to: Dig, Deconstruct, Prioritize, Disinfect, Sweep, Mop, Attack, Capture, Harvest, Empty Pipe. It does not apply to Cancel or Disconnect; number keys are swallowed on those tools to prevent accidental actions.

Note: this is the toolbar priority screen. Build tools use a separate, independent priority screen managed by the build system. Changing priority here does not affect build priority.

## Confirmation and Cancellation

Both Enter and Escape deactivate the tool and return to the default state. The tool handler stops intercepting keys and all keybindings return to normal.

### Enter (Confirm)

If one or more rectangles are defined, the mod processes every cell in the union of all defined rectangles through the active tool. Invalid cells are silently skipped by the game, matching base game behavior. If no valid cells were acted on, the mod announces "No valid cells," plays the game's error/deactivate sound, and deactivates the tool. Otherwise, the tool's confirm sound plays and the mod announces a summary of what was applied:

- **Cell-based tools:** total valid cells acted on and the priority if applicable. E.g., "Marked 12 tiles for digging at priority 5" or "Cancelled 4 dig orders" or "Updated 8 orders to priority 7."
- **Entity-based tools:** entities acted on. E.g., "Marked 2 Hatches and 1 Shove Vole for attack."

If no rectangles are defined when Enter is pressed (including when only a first corner is pending with no completed rectangles), the mod announces "Canceled," plays the deactivate sound, and deactivates the tool. This is the same outcome as pressing Escape. A pending first corner is not a rectangle — it is transient state discarded on Enter, Escape, or filter change.

### Escape (Cancel)

The mod announces "Canceled," plays the tool's deactivate sound, discards any defined rectangles, and deactivates the tool.

### External Deactivation

If the tool is deactivated by something other than Enter or Escape (game hotkey re-press, overlay conflict, right-click), the handler detects this via the `ActiveToolChanged` event firing with `SelectTool`. The mod announces "Canceled," plays the deactivate sound, and discards any defined rectangles and pending corners — the same behavior as Escape.

### Sound Responsibilities

Each sound is either played by the game automatically through the APIs the mod calls, or must be played by the mod explicitly. This has been traced through the decompiled source.

**Played by the game automatically:**

- **Tool selection** (UISounds.Sound.ClickObject): Played by `ToolMenu.ChooseTool()`. The mod's tool picker menu should activate tools through `ChooseTool()` rather than `PlayerController.ActivateTool()` directly, so the selection sound fires automatically.

**Played by the mod:**

- **Drag sound** ("Tile_Drag" / "Tile_Drag_NegativeTool"): The game only plays this in `OnMouseMove` when the area visualizer size changes. Since the mod manages selection state without driving the game's drag visualizer, the mod plays this on each corner-set with the `tileCount` FMOD parameter encoding the selection area.
- **Deactivate sound** ("Tile_Cancel"): The game's ToolMenu UI handlers play this manually before deactivation logic — it does not fire from `DeactivateTool` or `OnDeactivateTool`. The mod must play it on Escape, Enter-with-no-rectangles, and external deactivation.
- **Priority change sound** (PriorityScreen.PlayPriorityConfirmSound): `SetScreenPriority()` does not play it. The mod must call `PriorityScreen.PlayPriorityConfirmSound(priority)` explicitly when the player changes priority via number keys.

**Suppressed and replaced by the mod:**

- **Confirm sound** ("Tile_Confirm" / "Tile_Confirm_NegativeTool" / "OutletDisconnected"): The game plays this automatically in `OnLeftClickUp`, which fires once per rectangle submission. For multi-rectangle confirms this would play multiple times. The mod suppresses it via a Harmony prefix on `DragTool.GetConfirmSound()` (virtual method) that returns null while a suppression flag is set — `KMonoBehaviour.PlaySound` safely no-ops on null. After the last rectangle is submitted, the mod clears the flag and plays the confirm sound once. For single-rectangle confirms, the same mechanism applies uniformly.

## Entity-Based Tools

Attack and Capture operate on entities (creatures), not cells. The rectangle selection works the same way mechanically: the player defines corners and the game checks which entities fall within those bounds. When arrowing through the selection, the player hears individual creatures at each cell as defined in the Attack/Capture physical state readouts. The rectangle completion summary reports entities rather than cells (e.g., "2 Hatches, 1 Shove Vole, 1 Hatch already marked for attack").

## Special Cases

### DisconnectTool

Uses Line mode with a maximum length of 2 cells. The player sets the first corner, then sets the second corner on an adjacent cell along the same axis. If the second corner is more than 1 cell away or off-axis, the mod rejects it with an error announcement explaining that the Disconnect tool can only be used in a 2-tile-long line. Uses "Tile_Drag_NegativeTool" for selection and "OutletDisconnected" for confirmation.

### CopySettingsTool

Not accessible through the tool menu. CopySettingsTool is activated from a building's context menu and will be handled as part of the building interaction system.

### Pipe and Wire Building

UtilityBuildTool and WireBuildTool use path tracing rather than rectangle selection. These are part of the build system and are handled separately from this tool interaction design.

## Implementation Notes

### Mapping Rectangles to Game APIs

Rectangle selection state (corners, multiple rectangles) is managed entirely by the mod. The game's drag API is not touched until the player confirms with Enter.

On confirm, each rectangle is submitted as a separate drag operation by calling the game's public drag lifecycle methods: `OnLeftClickDown(corner1Pos)` followed by `OnLeftClickUp(corner2Pos)`, where positions are world coordinates from `Grid.CellToPosCCC`. The game's `DragTool` base class computes the rectangle from those two positions and internally handles per-cell iteration, validity filtering, and `OnDragComplete`. This works uniformly for cell-based tools and entity-based tools (Attack, Capture) without needing to call protected methods like `OnDragTool` directly.

Cells in overlapping rectangles may be processed more than once; this is harmless since the game either skips already-marked cells or re-marks them.

## Key Summary

All keys are only intercepted while a tool is active. When no tool is active, all keys pass through to normal behavior.

| Key | Action |
|-----|--------|
| Space | Set rectangle corner |
| Delete / Backspace | Clear most recent rectangle at cursor (swallowed if no selection) |
| Enter | Confirm selection, announce summary, deactivate tool |
| Escape | Cancel, discard selection, deactivate tool |
| 1-9 | Set priority (basic class); swallowed on Cancel and Disconnect |
| 0 | Set emergency priority (topPriority class); swallowed on Cancel and Disconnect |
| F | Reopen filter/mode menu (clears selection and pending corner); swallowed on tools without filters |
| Ctrl+Arrows | Jump in arrow direction to next selected/unselected cell |
| Arrow keys | Passed to tile cursor for navigation |
| All other keys | Passed down the chain |
