# OniAccess

An accessibility mod for Oxygen Not Included that makes the game playable for blind users. All game information is delivered through speech output via your screen reader (using the Tolk bridge). The mod is read-only -- it doesn't change game behavior, only adds speech.

## Install

The build script handles this automatically for development. Full install instructions will be added when the install process is finalized.

## Quick start

- **Ctrl+Shift+F12** toggles the mod on/off
- **?** (Shift+/) opens context-sensitive help listing every key available on the current screen
- Arrow keys navigate everything -- tile cursor in gameplay, items in menus
- Enter activates, Escape closes, Tab cycles sections

## Context help and mod toggle

**?** (Shift+/) opens an interactive help list tailored to whatever screen you're on. The list changes depending on context -- the help you see in the colony view is different from the help inside a details screen or the build menu. The list is navigable and supports type-ahead search, so you can type part of a key name to jump to it.

**Ctrl+Shift+F12** toggles the entire mod off. All speech stops and every key passes through to the game as if the mod weren't installed. Press it again to re-enable. This is the only key that works while the mod is disabled.

## Tile cursor

The tile cursor is your primary way of exploring the map. Arrow keys move one tile at a time. Each tile announces its element (with mass), any building, entities, active orders, and debris -- in that order. Gases are suppressed when a building or foundation covers the cell.

Most overlays prepend one extra reading before the standard information. Temperature adds the temperature (with a warning near phase transitions), Light adds lux, Radiation adds rads, Decor adds the value with sign, and Disease adds germ counts by type or "clean." The utility overlays (Power, Plumbing, Ventilation, Conveyor) add network/conduit data, and Automation adds signal state. Rooms prepends the room name, announced once per room rather than every tile.

### Skip

**Ctrl+Arrow** skips in a direction until something changes, then announces how many tiles were crossed. What counts as a "change" depends on the active overlay:

- **Default view**: different building, tile type, or element
- **Temperature**: different temperature band (8 bands from below freezing to above 1800 C)
- **Power / Plumbing / Ventilation / Conveyor**: follows a pipe/wire, stops at junctions or at the end of the pipe/wire. Doesn't jump between networks.
- **Rooms**: different room
- **Disease**: transition between clean and infected
- **Light / Radiation / Decor**: different value band, set by the game

Skip also stops at the alignment ruler if one is placed, and at world boundaries.

### Big cursor

**Shift+Up/Down** cycles the cursor size: 1x1 (default), 3x3, 5x5, 9x9, 21x21. The size resets to 1x1 on world load.

When the cursor is larger than 1x1, arrow keys move by the full cursor width (e.g., 5 tiles at 5x5), tiling areas edge-to-edge. The cursor stops where the full area fits inside the world. Ctrl+Arrow skip is unaffected by cursor size.

Landing on a tile speaks an area scan summary instead of the single-tile glance. The scan adapts to the active overlay:

- **Default / utility overlays**: solid/liquid/gas/vacuum percentages, buildings by type, dupes, critters, pending orders by type
- **Materials**: full element breakdown by percentage
- **Oxygen**: O2 and polluted O2 percentages with total mass
- **Temperature / Light / Decor / Radiation**: area average
- **Disease**: average germ count per type
- **Rooms**: room types intersecting the area
- **Crops**: plant count by type with average growth percentage

All scans report unexplored percentage first if any tiles in the area haven't been revealed. Coordinate reading (K) always reports the center cell.

### Coordinates

**K** reads the cursor's X,Y position relative to the Printing Pod. **Shift+K** cycles coordinate mode between Off, Append, and Prepend. In Append or Prepend mode, coordinates are included in every tile announcement automatically. The setting persists across sessions.

If your Printing Pod was somehow destroyed, 0, 0 becomes the center of the map.

### Tooltip

**I** reads the most relevant tooltip block for the current tile. The mod picks which block based on the overlay -- in utility overlays it prioritizes conduit data, in others it prioritizes the building.

### Entity picker

If a tile has multiple selectable objects, **Enter** opens a picker listing them all. Single-entity tiles select directly. When available, an item's full tooltip is also read in this menu, falling back to just the name if no tooltip is available.

## Scanner

The scanner catalogs everything on the current asteroid into a four-level hierarchy: category, subcategory, item, instance.

**End** performs a full scan. This scan is not live-updated, except to prune no-longer-valid data. It must be manually refreshed with End. This is because in the late game, the scan can take several hundred milliseconds, and so refreshing it on every key press would be annoying. Results are organized into categories (Solids, Liquids, Gases, Buildings, Networks, Automation, Debris, Zones, Life), each with subcategories and an "all" subcategory containing everything in that category. Navigate with:

- **Ctrl+PageUp/Down** -- cycle categories
- **Shift+PageUp/Down** -- cycle subcategories
- **PageUp/Down** -- cycle items
- **Alt+PageUp/Down** -- cycle instances of the current item

Each item announces its name, distance from you (vertical then horizontal), and position within the list. Items are sorted by distance from your cursor. If an entity has been destroyed since the last scan, it's silently removed and the next result is shown.

**Home** teleports the cursor to the current instance. **Shift+Home** toggles auto-move: when enabled, the cursor teleports automatically as you cycle. When auto-move is on, distances are measured from where you were when you scanned rather than the cursor's current position.

### Clustering

Most scanner results are spatially clustered: adjacent cells of the same type merge into a single entry showing the cell count (e.g., "47 Granite"). This applies to elements, constructed tiles, biome zones, and orders like dig or mop. Two pools of the same liquid that aren't touching appear as separate entries.

Utility networks (Power, Plumbing, Ventilation, Conveyor, Automation) cluster differently. Segments are grouped by the game's network ID and segment type, so all regular wire on the same electrical network is one entry, but insulated wire on that same network is a separate entry. Bridge buildings (connectors, transformers, etc.) are listed individually rather than clustered.

### Scanner search

**Ctrl+F** opens a text input. Type a query and press Enter. Results are grouped in an all category and under their original categories as subcategories, sorted by match quality (prefix matches rank highest). Escape cancels the search.

The scanner clears automatically when you switch asteroids.

## Tools

Selecting a tool (Dig, Deconstruct, Mop, etc.) enters tool mode. The cursor still moves normally, but you now place orders instead of inspecting.

### Rectangle selection

**Space** sets the first corner. Move to the opposite corner and press **Space** again to complete the rectangle. A drag sound plays whose pitch reflects the selection size. The confirmation announces dimensions and valid cell count. You can place multiple rectangles before confirming.

**Enter** confirms all pending rectangles. If no rectangle is set, Enter confirms a single cell under the cursor. **Shift+Space** clears the rectangle under the cursor.

With a big cursor active, **Space** sets both corners at once, creating a rectangle the size of the cursor area. You can move and press Space again to add more rectangles. The disconnect tool always uses single-cell selection regardless of cursor size.

### Priority

**0-9** sets the priority for future placements. 0 is emergency (top priority/yellow alert), 1-9 are normal priorities. The priority is announced when you activate a tool and when you change it. Confirmation messages include the priority (e.g., "marked 12 items for digging at priority 5").

### Filters

**F** opens a filter picker for the active tool, showing available filter layers. Changing the filter clears any pending selection.

Switching overlays while a tool is active automatically changes the tool's filter to match. For example, switching to the plumbing overlay while deconstructing changes the filter to target pipes.

### Base game tool hotkeys

The game assigns letter keys to activate tools directly from the colony view. Since the mod activates tools through its own build menu, these hotkeys are extra but still work. **I** and **K** are overwritten by the mod (tooltip and coordinates). All of these can be remapped from the game's Input Bindings options menu -- the number row is a good alternative if you want them back.

- **G** -- Dig
- **C** -- Cancel construction
- **X** -- Deconstruct
- **P** -- Prioritize
- **M** -- Mop
- **K** -- Sweep (overwritten by mod -- coordinates)
- **I** -- Disinfect (overwritten by mod -- tooltip)
- **T** -- Attack
- **N** -- Capture / Wrangle
- **Y** -- Harvest
- **B** -- Copy building

## Building

**Tab** from the colony view opens the build menu. It has three levels: categories (Tools, Housing, Food, Power, etc.), subcategories, and individual buildings.

At the building level, navigation wraps across subcategories and categories -- you can scroll continuously through every building without backing out. **Ctrl+Up/Down** jumps between subcategory boundaries. Type-ahead search works across all buildings and tools regardless of category. All nested menus in the mod work like this.

Selecting a building enters placement mode:

- **Space** -- place one copy at the cursor
- **Enter** -- place and return to the map immediately.
- **R** -- rotate (announces new orientation and extent for multi-tile buildings). Not all buildings are rotatable. Some have only 2 orientations.
- **Tab** -- return to the building list at the same position
- **I** -- read building description, effects, and material requirements. Material and facade can be changed from here.
- **0-9** -- set construction priority
- **Shift+Space** -- cancel existing construction at the cursor

Utility buildings (pipes and wires) use line placement: Space sets the start, then move in a straight line and Space again to complete the run. If any tiles along the line are invalid, the placement will fail.

## Colony status

These readouts are available from the colony view:

**Q** -- cycle number, current schedule block, and alert state (red/yellow alert if active).

**Shift+Q** -- total hours played.

**S** -- colony summary: duplicant count (local and cluster-wide if Spaced Out), sick count (if any), rations with trend, max stress with trend, and electrobank energy with trend (only if bionic duplicants are present). Trends are "rising" or "falling" based on 10-minute history, with per-resource thresholds to filter out noise.

**Shift+P** -- all pinned resource amounts.

**Backtick (`)** -- cycle game speed (1x, 2x, 3x).

**Ctrl+R** -- toggle red alert.

The mod also announces automatically without input: pause/unpause (with speed on unpause), speed changes, new cycles, and red/yellow alert transitions. During initial game load, notifications are suppressed until you first unpause.

## Duplicant tracking

**[** and **]** cycle through all living duplicants on the current asteroid. Each announcement includes the dupe's name, current task (with target building), and any critical statuses. The status checks cover: incapacitated, critical health, injured, severe wounds, suffocating, holding breath, nervous breakdown, stressed, scalding, hypothermia, sick, starving, entombed, fleeing, and bionic battery states.

**Backslash** is a two-stage key. First press jumps the cursor to the current dupe's location. Second press (when already on their tile) selects them and opens their details screen.

## Details screen

The details screen has three sections: main tabs (Status, Personality, etc.), side screens (building-specific config panels), and action buttons. **Tab/Shift+Tab** cycles within the current section. **Ctrl+Tab/Ctrl+Shift+Tab** jumps between sections.

### Sliders

Left/Right adjusts values. Modifier keys control step size:

- **Plain** -- 1 step (or 1% of range for fractional sliders)
- **Shift** -- 10 (or 10%)
- **Ctrl** -- 100 (or 25%)
- **Ctrl+Shift** -- 1000 (or 50%)

A boundary sound plays at minimum and maximum values.

### Dropdowns and radio groups

Left/Right cycles through options directly, announcing the new value.

### Move-to-location

From an entity's details, the move-to command enters a cursor mode. Navigate to the destination and press Space or Enter to confirm.

### Recipe queue

When a fabricator has a recipe queue side screen, it shows the recipe info, material slots, and queue. Tab/Shift+Tab cycles recipes. Left/Right adjusts the queue count using the same step sizes as sliders.

## Notifications

New notifications are batched over a short window and collapsed by title (e.g., "Stress Alert x3"). During game load, all notifications are held until you first unpause.

**Shift+N** opens the notification menu. Groups are listed by title with count. **Enter** on a single notification activates it (focuses the camera on the source, selects the entity, or opens a message dialog depending on type). Groups with multiple members drill into a submenu listing individual notifications with their source name and location. **Delete** dismisses a group.

## Spatial tools

### Ruler

**Ctrl+B** places an alignment ruler at the cursor. The ruler provides audio feedback in three zones as you move: a click at the exact crosshair (same row and column), a higher-pitched tone on the same row or column, and a lower-pitched tone one tile away from the line. Skip movement stops at ruler lines.

**Ctrl+Shift+B** clears the ruler.

### Bookmarks

**Ctrl+1-0** saves the current position (uses the game's native bookmark system). **Shift+1-0** jumps to a saved bookmark. **Alt+1-0** reports direction and distance to a bookmark without moving.

### Jump home

**H** teleports the cursor to the Printing Pod.

## Priority screen

The priority screen is a 2D grid with duplicants as rows and chore groups as columns. Each cell shows the priority level and the dupe's skill for that chore. Trait-disabled chores are announced as such.

- **0-5** -- set priority (0=disabled, 1=very low through 5=very high)
- **Shift+0-5** -- set the entire column (all dupes for that chore)
- **Ctrl+Left/Right** -- adjust all priorities in the current row by 1
- **Ctrl+Up/Down** -- adjust all priorities in the current column by 1

A toolbar row at the top provides Reset Settings and an Advanced (proximity) Mode toggle.

## Schedule screen

The schedule screen has two tabs: Schedules and Duplicants.

### Schedules tab

A 2D grid of schedules and 24 hour blocks. Each cell is a block type: Work, Hygiene, Recreation, or Sleep.

- **1/2/3/4** -- select a brush (Work, Hygiene, Recreation, Sleep)
- **Space** -- paint the current cell with the selected brush
- **Shift+Left/Right** -- paint while moving
- **Shift+Home/End** -- paint from cursor to start or end of the row
- **Ctrl+Left/Right** -- rotate hour blocks within the row (i.e., current hour 1 cell becomes hour 2)
- **Ctrl+Up/Down** -- reorder schedules
- **Enter** -- open options (Rename, Alarm, Duplicate, Delete, Add/Delete Row)

Schedules can have multiple timetable rows. Shift+Up/Down moves rows within a schedule.

### Duplicants tab

A flat list of duplicants showing their name, schedule trait (Early Bird, Night Owl), and assigned schedule. Left/Right changes the assignment.

## Research screen

The research screen has three tabs: Browse, Queue, and Tree.

### Browse tab

Techs grouped into three buckets: Available, Locked, and Completed. Each tech announces its name, state, research cost (or progress if partially complete), what it unlocks, and prerequisites if locked. Enter selects a tech for research. Space jumps to it in the Tree tab.

### Queue tab

Shows banked research points at the top, then queued techs in order. Enter cancels all techs. The queue is really not a queue in the traditional sense. it exists to allow you to queue a research while missing the prerequisites, but doesn't allow you to queue multiple things in any other context. cancel one thing and it cancels everything.

### Tree tab

Navigates the tech tree as a graph. Up moves to a prerequisite, Down to a dependent, Left/Right cycles siblings. Enter selects a tech. Announces "root node" or "dead end" at boundaries.

All three tabs support type-ahead search across the full tech database.

## Resource browser

**Shift+I** opens the resource browser. Categories are listed at the top level, with a synthetic "Pinned" category if any resources are pinned. Each category shows its total amount and trend.

Drill into a category to see individual resources. Each resource shows: total amount, reserved amount, available (or overdrawn), and trend. **Space** toggles pin status. **Shift+C** clears all pins.

**Enter** on a resource shows all world instances, each with amount, container name, and location. Enter on an instance jumps the cursor there. Instances are sorted by amount.

## Type-ahead search

Most menu screens support type-ahead: start typing to filter. Matches are ranked in five tiers from start-of-string exact matches down to substring matches. Typing a single letter repeatedly cycles through items starting with that letter. The buffer clears after 1.5 seconds of inactivity. Backspace edits the query. Escape clears the search.

## Management screen hotkeys

These are base game hotkeys that open management screens from the colony view. They are not mod keys, but the game does not allow remapping them.

- **L** -- Priorities
- **F** -- Consumables
- **V** -- Vitals
- **R** -- Research
- **.** -- Schedule
- **J** -- Skills
- **E** -- Colony report
- **U** -- Database (Codex)
- **Z** -- Starmap

## Known limitations

- Diagnostics screen is not supported.
- Base game starmap is not accessible.
- Spaced Out DLC content is not yet supported but will be.

## Troubleshooting

- **Player log location**: `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`
- **Mod log lines** are prefixed with `[OniAccess]`
- If a screen isn't being read, check that your screen reader is running and try ? (Shift+/) to see if the mod recognizes the current context.
