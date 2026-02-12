# FactorioAccess Feature Documentation

FactorioAccess is a comprehensive mod that makes Factorio fully playable by blind and visually impaired players. It replaces Factorio's visual interface with an audio-first experience built on screen reader integration, spatial audio sonification, and keyboard-only controls. Every feature exists to bridge the gap between a deeply visual game and players who cannot see the screen.

This document catalogs every major feature, explains the accessibility problem it solves, and describes how it works.

---

## Table of Contents

1. [Core Architecture](#1-core-architecture)
2. [Speech and Screen Reader Integration](#2-speech-and-screen-reader-integration)
3. [Spatial Audio and Sonification](#3-spatial-audio-and-sonification)
4. [Cursor and Navigation System](#4-cursor-and-navigation-system)
5. [Scanner System](#5-scanner-system)
6. [Accessible UI Framework](#6-accessible-ui-framework)
7. [Building and Construction](#7-building-and-construction)
8. [Build Lock (Auto-Build While Walking)](#8-build-lock-auto-build-while-walking)
9. [Blueprints](#9-blueprints)
10. [Mining and Deconstruction](#10-mining-and-deconstruction)
11. [Inventory Management](#11-inventory-management)
12. [Crafting System](#12-crafting-system)
13. [Technology and Research](#13-technology-and-research)
14. [Equipment and Armor](#14-equipment-and-armor)
15. [Combat System](#15-combat-system)
16. [Transport Belts](#16-transport-belts)
17. [Pipes and Fluids](#17-pipes-and-fluids)
18. [Electrical Network](#18-electrical-network)
19. [Circuit Networks](#19-circuit-networks)
20. [Train System](#20-train-system)
21. [Virtual Train Driving and Rail Building](#21-virtual-train-driving-and-rail-building)
22. [Syntrax Rail Description Language](#22-syntrax-rail-description-language)
23. [Vehicles](#23-vehicles)
24. [Spidertron Remote Control](#24-spidertron-remote-control)
25. [Logistics and Robot Networks](#25-logistics-and-robot-networks)
26. [Alerts and Warnings](#26-alerts-and-warnings)
27. [Fast Travel](#27-fast-travel)
28. [Audio Rulers](#28-audio-rulers)
29. [Settings System](#29-settings-system)
30. [Vanilla Mode](#30-vanilla-mode)
31. [Tutorial System](#31-tutorial-system)
32. [Help System](#32-help-system)
33. [Launcher and External Communication](#33-launcher-and-external-communication)
34. [Keybinding System](#34-keybinding-system)
35. [Quality System Support](#35-quality-system-support)
36. [Multiplayer Support](#36-multiplayer-support)

---

## 1. Core Architecture

### Problem
Factorio is a deeply visual game. Blind players cannot see the map, entities, GUIs, or any visual feedback. A bolt-on accessibility layer would be insufficient; the entire interaction model must be reimagined from the ground up.

### Solution
FactorioAccess is built as a complete parallel interface layer with these core systems:

- **Speech system**: Converts all game information to screen reader output via an external launcher
- **Sonification system**: Renders spatial game state as positional audio (enemy positions, inserter activity, health levels)
- **Cursor system**: A virtual tile-based pointer that replaces the mouse for world interaction
- **Scanner system**: A streaming database that categorizes and indexes all nearby entities for structured browsing
- **UI framework**: A keyboard-driven, graph-based UI system that replaces all visual menus with audio-navigable interfaces
- **Event manager**: Multi-handler event dispatch with priority ordering (test > UI > world)
- **Storage manager**: Per-player, per-module state management with lazy initialization and version tracking

### Key Files
- `control.lua` - Main entry point and event registration (~4000 lines)
- `scripts/speech.lua` - MessageBuilder and text-to-speech output
- `scripts/launcher-audio.lua` - Spatial audio command builder
- `scripts/scanner/entrypoint.lua` - Scanner public API
- `scripts/ui/router.lua` - Central UI manager
- `scripts/event-manager.lua` - Multi-handler event dispatch
- `scripts/storage-manager.lua` - Per-module storage allocation

### Design Principles
- **Audio-first**: Not a translation layer over visual UI, but an interface designed for how blind players interact with games
- **Keyboard-only**: No mouse required; all features accessible via 195 custom keybindings
- **Explicit feedback**: Every action produces speech and/or sound confirmation
- **Let it crash**: Internal code avoids defensive coding; bugs surface immediately rather than hiding behind silent fallbacks

---

## 2. Speech and Screen Reader Integration

### Problem
Blind players use screen readers (NVDA, JAWS, Narrator, VoiceOver) to interact with computers. The mod must convert all game state into natural language output that these screen readers can vocalize.

### Solution
A two-layer speech system:

**MessageBuilder** (`scripts/speech.lua`): A fluent API for building localized messages. It automatically handles spacing between fragments and comma separation for lists. It deliberately crashes if you pass a space fragment, preventing a common class of bugs.

```lua
local msg = MessageBuilder.new()
  :fragment("transport belt")
  :list_item("north")
  :list_item("5 items")
  :build()
Speech.speak(pindex, msg)
```

**stdout Protocol**: Messages are sent to an external launcher process via stdout:
```
out <player_index> <localised_string>
```
The launcher captures this output and routes it to the OS screen reader.

### Screen Reader Design Rules
- The sooner a message varies, the faster the user can keep going ("anchored cursor" vs "cursor anchored" - the former lets the user move on after the first syllable)
- Less punctuation is better; many screen reader setups read `:`, `(`, `)` verbatim
- Prefer commas and periods over colons and parentheses
- Parameterized locale keys allow word order to change in translations

### Key Files
- `scripts/speech.lua` - MessageBuilder and speak function
- `scripts/localising.lua` - Localization support functions
- `locale/en/locale.cfg` - Main locale strings

---

## 3. Spatial Audio and Sonification

### Problem
Speech alone is too slow for real-time game feedback. Players need continuous, non-verbal awareness of factory activity, enemy positions, and health status without waiting for screen reader announcements.

### Solution
A comprehensive sonification system that encodes game state as spatial audio:

**LauncherAudio** (`scripts/launcher-audio.lua`): Sends audio commands to the launcher via a JSON protocol:
```
acmd <player_index> <json_command>
```
Supports waveform synthesis (sine, square, triangle, sawtooth), envelope-based volume/pan control, low-pass filters, and sound file playback with spatial positioning.

**Grid Sonifier** (`scripts/sonifiers/grid-sonifier.lua`): Manages all entities in view range and coordinates multiple audio backends:

- **Inserter sonifier**: Plays tones when inserters pick up or drop items, so players can hear their factory working
- **Crafting sonifier**: Signals when crafting machines complete products
- **Enemy radar** (`scripts/sonifiers/combat/enemy-radar.lua`): Continuously pings nearby enemies with positional audio. Pitch encodes health (higher = healthier), pan encodes left/right position, volume encodes distance. Enemies are clustered to prevent audio overload.
- **Spawner radar** (`scripts/sonifiers/combat/spawner-radar.lua`): Distinct pinging sound for enemy nests, cycling through them one at a time to avoid saturation
- **Health bar** (`scripts/sonifiers/health-bar.lua`): Plays beeps with stereo panning to indicate health/shield levels (right = full, left = empty)
- **Vehicle sonifier** (`scripts/sonifiers/vehicle.lua`): Audio feedback for vehicle movement direction and orientation
- **Battle notice** (`scripts/sonifiers/battle-notice.lua`): Alert sound when player-owned structures are damaged or destroyed

**Audio Cues** (`scripts/audio-cues.lua`): Registry system for periodic audio checks at varying tick intervals (15, 30, 60, 120 ticks), distributed across ticks to avoid CPU spikes.

### Key Files
- `scripts/launcher-audio.lua` - Audio command builder
- `scripts/sonifiers/` - All sonifier backends
- `scripts/audio-cues.lua` - Periodic audio check system
- `scripts/sound-model.lua` - Spatial audio positioning model

---

## 4. Cursor and Navigation System

### Problem
Sighted players use the mouse to point at things on the map. Blind players have no mouse equivalent. They need a way to explore the world, select entities, and position buildings using only the keyboard.

### Solution
A virtual cursor that operates on the tile grid, independent of the player character's position.

### Cursor Movement
- **W/A/S/D**: Move cursor one tile north/west/south/east
- **J**: Jump cursor back to character position
- **Shift+B / B**: Save and load cursor bookmarks for quick return to build sites

When the cursor moves, the mod immediately announces what's at the new position: entities, tile type, walkability, and resources.

### Cursor Skip (Intelligent Fast Navigation)
- **Shift+W/A/S/D**: Skip the cursor up to 100 tiles, stopping at the first "change" in the map

Skip detects several kinds of changes:
- Different entity type or direction
- Belt shape changes (turns, merges)
- Water/land boundaries
- Audio ruler crossings
- Underground belt/pipe connections (jumps directly to the other end)

This makes exploring vast areas feasible without moving tile-by-tile.

### Cursor Anchoring
- **I**: Toggle anchor mode

When anchored, the cursor automatically moves one tile ahead of the character during walking, announcing entities as the player approaches them. When unanchored, the cursor stays in place while the character moves freely. Anchored mode is essential for hands-free exploration.

### Entity Selection and Cycling
- **Shift+F**: Cycle through multiple entities on the same tile

At any cursor position there may be multiple overlapping entities. The mod sorts them by priority (characters > vehicles > buildings > resources) and allows cycling through them with stable ordering.

### Walking and Bump Detection
Players walk with arrow keys. The mod adds:

- **Bump detection** (`scripts/bump-detection.lua`): Compares intended movement direction with actual movement to detect collisions. Plays distinct sounds for sliding along walls, hitting corners, or being stuck.
- **Movement history** (`scripts/movement-history.lua`): Ringbuffer tracking positions for "go back" features, teleport detection, and build lock coordination.

### Nudging (Fine Positioning)
- **Ctrl+Arrow keys** (when not holding item): Nudge character one tile in a direction for precise positioning in constrained spaces
- **Shift+Arrow keys** (when holding item): Nudge the building preview for fine-tuning placement

### Coordinate Reading
- **Ctrl+K**: Announce character's current coordinates
- **Alt+K**: Announce cursor position relative to character

### Key Files
- `scripts/viewpoint.lua` - Cursor position, size, anchoring state
- `scripts/entity-selection.lua` - Entity cycling at current tile
- `scripts/cursor-changes.lua` - Cursor change event handling
- `scripts/movement-history.lua` - Position tracking ringbuffer
- `scripts/bump-detection.lua` - Collision audio feedback
- `scripts/walking.lua` - Walking state and tile crossing announcements

---

## 5. Scanner System

### Problem
The world has thousands of entities spread across a vast map. Moving the cursor tile-by-tile to find things is impractical. Players need a structured way to browse what's nearby.

### Solution
The scanner is a streaming database that discovers, categorizes, and indexes all entities within a 2500-tile radius. It organizes them into a three-level hierarchy:

1. **Categories**: ALL, RESOURCES, ENEMIES, PRODUCTION, LOGISTICS_AND_POWER, CONTAINERS, MILITARY, VEHICLES, SPIDERTRONS, TRAINS, GHOSTS, PLAYERS, CORPSES, OTHER, TERRAIN
2. **Subcategories**: Grouped by entity type or metadata (e.g., "iron-ore patches", "train #12345", "assembling machine producing iron gear wheels")
3. **Entries**: Individual entities, sorted by distance from the player

### Navigation
- **Ctrl+PageUp/Down**: Move between categories
- **PageUp/Down**: Move between subcategories
- **Shift+PageUp/Down**: Move between individual entries within a subcategory
- **End**: Refresh scanner (all directions)
- **Shift+End**: Refresh scanner in character's facing direction only
- **Home**: Re-announce current scanner selection

When navigating to an entry, the cursor automatically moves to that entity's position and announces its details.

### How It Works
- **Backend system**: Each entity type has a backend that handles categorization, validation, and readout. ~200+ entity types are mapped to backends.
- **Specialized backends**: Trees are clustered into forests, water tiles are grouped by proximity, resource patches are aggregated. These prevent thousands of individual entries from overwhelming the list.
- **Incremental processing**: New entities are queued and processed gradually (100/tick) to avoid CPU spikes.
- **Entry caching**: The simple backend caches entries for ~2x scan performance.
- **Memosort**: Maintains pre-sort order, only re-sorting changed elements.

### Key Files
- `scripts/scanner/entrypoint.lua` - Public API and refresh logic
- `scripts/scanner/surface-scanner.lua` - Multi-surface entity tracking
- `scripts/scanner/scanner-consts.lua` - Category definitions
- `scripts/scanner/backends/` - All scanner backends (simple, trees, water, resources, etc.)

---

## 6. Accessible UI Framework

### Problem
Factorio's GUI is entirely visual: windows, buttons, grids, tabs, tooltips. Blind players cannot interact with any of it. The mod must provide keyboard-driven, audio-first UIs for every game interaction.

### Solution
A graph-based UI framework where UIs are state machines rather than visual renderers.

### Router (`scripts/ui/router.lua`)
Central dispatcher for all UI events. Maintains a registry of 50+ UI implementations and routes keyboard events to the appropriate handler.

### KeyGraph (`scripts/ui/key-graph.lua`)
The "UI assembly language." UIs are modeled as directed graphs where:
- **Nodes** = controls (buttons, checkboxes, labels, sliders)
- **Edges** = transitions (up/down/left/right)
- Each node has a `label()` function that generates speech, an `on_click()` handler, and focus gain/loss callbacks
- Edges can have transition messages ("lane separator") and transition sounds

This unified model handles vertical menus, horizontal rows, 2D grids, and trees with different movement patterns.

### TabList (`scripts/ui/tab-list.lua`)
Multi-tab container (like an inventory with tabs for contents, settings, logistics). Each tab has persistent state. Tab transitions play sounds and announce the new tab title.

### Builders
- **MenuBuilder** (`scripts/ui/menu.lua`): Fluent builder for vertical/horizontal menus
- **FormBuilder** (`scripts/ui/form-builder.lua`): Higher-level builder for dialog-like forms with checkboxes, sliders, textboxes, and item choosers
- **Controls** (`scripts/ui/controls.lua`): Factory functions for individual controls (checkbox, slider, textbox, list selector)

### Navigation
- **W/A/S/D**: Navigate within UIs (up/left/down/right)
- **Tab/Shift+Tab**: Move between subtabs
- **Ctrl+Tab/Ctrl+Shift+Tab**: Move between major tabs
- **[**: Click/activate the focused control
- **E**: Close UI
- **Ctrl+F**: Set search filter
- **Shift+/**: Open context-sensitive help

### Design Decisions
- No visual rendering; UI is purely state + speech output
- Explicit re-render is not possible because the UI has no painting step
- Controls announce their new value when changed (e.g., a checkbox says "checked" or "unchecked")
- Search is built into the graph traversal system

### Key Files
- `scripts/ui/router.lua` - Central UI manager
- `scripts/ui/key-graph.lua` - Graph-based UI model
- `scripts/ui/tab-list.lua` - Multi-tab container
- `scripts/ui/menu.lua` - Menu builder
- `scripts/ui/form-builder.lua` - Form builder
- `scripts/ui/controls.lua` - Control factories
- `scripts/ui/menus/` - 20+ specific menu implementations

---

## 7. Building and Construction

### Problem
Placing buildings requires seeing the map, identifying valid positions, checking for obstacles, understanding connection points (pipes, belts, wires), and positioning precisely. None of this visual information is available to blind players.

### Solution
A comprehensive building system with audio previews, obstacle identification, and connection awareness.

### Basic Building
- **[**: Place the item in hand at the cursor position
- **Shift+[**: Place as ghost (construction robot marker)
- **Ctrl+Shift+[**: Superforce build (maximum placement override)

### Build Preview Checks
Before placing, the system announces what will happen:
- **Underground belts**: Connected entrance and distance
- **Pipes-to-ground**: Connection target and distance
- **Heat pipes**: Connection directions (up to 4)
- **Electric poles**: Nearby poles that will connect (up to 5), with distance and direction
- **Roboports**: Logistics network connections
- **Logistic chests**: Whether in a logistics network or distance to nearest
- **Power-consuming entities**: Power connection status and source direction
- **Reach**: Warning if cursor is out of build reach

### Rotation and Flipping
- **R / Shift+R**: Rotate clockwise/counterclockwise (supports 2-way, 4-way, and 8-way rotation)
- **H / V**: Flip horizontally/vertically (for blueprints)

### Obstacle Handling
- Automatically clears trees, rocks, and remnants in the build area
- If placement fails, announces exactly what's blocking and where
- Optionally teleports the player out of the build area if they're standing in it

### Entity Nudging
- **Ctrl+Arrow keys** (with entity selected): Moves a placed entity by 1 tile
- Temporarily relocates the entity, checks validity at the new position, and reverts if blocked

### Offshore Pump Placement
Special handling: searches a 21x21 area around the player for all valid water-adjacent positions, sorts by distance, and opens a selection UI.

### Key Files
- `scripts/building-tools.lua` - Core build function with obstacle clearing and previews
- `scripts/graphics.lua` - Build cursor footprint visualization

---

## 8. Build Lock (Auto-Build While Walking)

### Problem
Placing long stretches of belts, pipes, or walls one tile at a time is tedious. Sighted players drag with the mouse. Blind players need an equivalent.

### Solution
Build lock automatically places the held item as the player walks or moves the cursor.

- **Ctrl+B**: Toggle build lock on/off

### How It Works
1. Monitors player movement (walking) and cursor movement
2. Generates an L-shaped path between tile movements
3. Queues tiles to build at
4. Attempts placement at each queued tile with backend-specific logic

### Smart Backends
Different item types have specialized placement logic:
- **Transport belts** (`scripts/build-lock-backends/transport-belts.lua`): Faces belts in the movement direction, detects and prevents invalid corners
- **Electric poles** (`scripts/build-lock-backends/electric-poles.lua`): Spaces poles at maximum wire distance for efficient coverage
- **Tiles** (`scripts/build-lock-backends/tiles.lua`): Places floor tiles
- **Simple** (`scripts/build-lock-backends/simple.lua`): Generic fallback for other entity types

### Safety
Build lock automatically disables on: cursor stack change, item count reaching 0, teleport detected, or character bumps into an obstacle.

### Key Files
- `scripts/build-lock.lua` - Core build lock state machine
- `scripts/build-lock-backends/` - Type-specific placement logic

---

## 9. Blueprints

### Problem
Blueprints are a core Factorio mechanic for copying and replicating factory sections. The visual blueprint interface (dragging, rotating, examining) is inaccessible.

### Solution

### Blueprint Creation
- **Shift+B**: Opens area selector to capture a rectangular region as a blueprint
- Preserves previous blueprint's name, description, and icons when updating
- Reports entity count and warns if selection is empty

### Blueprint Placement
- Works with both single blueprints and blueprint books
- Auto-selects active blueprint from books
- Supports rotation (R) and flipping (H/V)
- Three build modes: normal, forced, superforced
- Clears obstacles in blueprint area and reports blocking entities

### Copy/Paste/Cut
- **Ctrl+C**: Copy tool (captures area as blueprint)
- **Ctrl+X**: Cut tool (captures area and marks entities for deconstruction)
- Both activate paste mode automatically with audio feedback on entity counts

### Key Files
- `scripts/blueprints.lua` - Blueprint creation, placement, and management

---

## 10. Mining and Deconstruction

### Problem
Mining entities requires clicking on them. Blind players need keyboard alternatives and area-based mining.

### Solution
- **X (hold)**: Mine selected entity (continuous)
- **Shift+X**: Instantly mine all mineable entities within 10 tiles (accessibility cheat)
- **Ctrl+Shift+X**: Clear all ghost entities within 100 tiles

The building system also includes automatic obstacle clearing: `clear_obstacles_in_rectangle()` forces mining of trees, rocks, remnants, and ground items before placement. Items beyond mining range are marked for deconstruction instead.

### Key Files
- `scripts/player-mining-tools.lua` - Mining functions and obstacle clearing

---

## 11. Inventory Management

### Problem
Factorio's inventory is a visual grid. Blind players cannot see item icons, stack counts, or filter indicators. They need a structured way to browse, transfer, and manage items.

### Solution

### Inventory Grid UI
A 10-column grid display with keyboard navigation. Each slot announces: item name, quality (if non-normal), count, lock indicator (if part of active recipe), and filter info.

- **W/A/S/D**: Navigate the grid
- **[/]**: Transfer items between paired inventories (player <-> chest)
- **K**: Read detailed information about the focused slot

### Item Transfer System
`quick_transfer()` in `scripts/inventory-utils.lua` moves items between inventories with full validation. If transfer fails, it announces the specific reason ("chest is full", "incompatible item type") because blind players can't see error icons.

### Try-Before-Commit Pattern
The `Deductor` class tentatively checks if the player can afford to build something (checks inventory + cursor) before committing the action. This prevents confusing state changes from partial failures.

### Quickbar (Toolbar)
- **1-0**: Select quickbar slot 1-10
- **Ctrl+1-0**: Assign current hand item to quickbar slot
- **Shift+1-0**: Switch quickbar pages (10 pages x 10 slots = 100 slots)

Each slot announces its contents and whether it's selected, plus total count in inventory.

### Hand Item
- **Q**: Empty hand or pipette (pick up appropriate item from cursor tile)
- **Shift+Q**: Read what's currently in hand
- **Z**: Drop 1 item from hand

### Key Files
- `scripts/ui/inventory-grid.lua` - Grid display and navigation
- `scripts/inventory-utils.lua` - Safe inventory access, transfers, Deductor
- `scripts/inventory-transfers.lua` - Multi-stack ratio-based transfers
- `scripts/item-stack-utils.lua` - Aggregation by name and quality
- `scripts/quickbar.lua` - Quickbar management

---

## 12. Crafting System

### Problem
Crafting requires browsing hundreds of recipes, checking ingredient availability, and managing the crafting queue. The visual crafting grid with icons is unusable without sight.

### Solution

### Crafting Menu
A CategoryRows-based recipe browser opened from the main menu:
- **W/S**: Navigate between recipe categories (logistics, production, intermediate, combat, etc.)
- **A/D**: Navigate between recipes within a category
- **[**: Craft 1 batch
- **Shift+[**: Craft 5 batches
- **Ctrl+Shift+[**: Craft all possible
- **K**: Read ingredients, products, crafting time, and which machines can craft this recipe

Each recipe label shows the recipe name and how many the player can currently craft.

### Failure Feedback
`recipe_cannot_craft_reason()` returns a detailed localized reason: "requires machine", "requires fluids", or "missing 5 iron plate, 2 copper plate" listing each missing ingredient.

### Crafting Queue
A separate menu to view and cancel items currently being crafted.

### Key Files
- `scripts/crafting.lua` - Recipe discovery, validation, and queue management
- `scripts/recipe-helpers.lua` - Shared helpers for recipe display
- `scripts/ui/menus/crafting.lua` - Crafting menu UI

---

## 13. Technology and Research

### Problem
Factorio's technology tree is a complex dependency graph with hundreds of technologies. Navigating it visually requires clicking through tree branches and reading tooltip details.

### Solution

### Research Menu
A CategoryRows-based technology browser:
- **Categories**: Researchable (prerequisites met), Locked (prerequisites not met), Researched (completed)
- **[**: Enqueue technology at end of research queue
- **Ctrl+Shift+[**: Enqueue at front
- **K**: Read full details

### Detailed Information
For each technology, the system reports:
- **Cost**: Science pack requirements or trigger conditions (Factorio 2.0 trigger-based research like "craft 10 of item X")
- **Prerequisites**: Other technologies needed first
- **Rewards**: Recipe unlocks, direct successor technologies, indirect unlocks, and bonuses (damage, speed, productivity with percentage formatting)

### Research Queue Management
Players can view the queue, reorder entries, and track progress percentage.

### Key Files
- `scripts/research.lua` - Technology discovery, filtering, queue management, reward formatting
- `scripts/ui/menus/research.lua` - Research menu UI

---

## 14. Equipment and Armor

### Problem
Factorio's equipment grid is a 2D spatial puzzle where equipment pieces of different sizes must be placed. This is inherently visual. Blind players need structured access to armor stats, equipment placement, and grid navigation.

### Solution

### Equipment Grid UI
A 2D grid with cursor navigation:
- Arrow keys move within the grid, announcing coordinates
- Equipment labels include name, quality, ordinal position ("2nd shield"), category, and stats
- **[**: Remove equipment to inventory
- **Backspace**: Mark for removal

### Equipment Information
- **K on equipment**: Announces dimensions, capacity, movement bonus percentage, inventory bonus slots, stored energy percentage
- **G**: Check health and shield levels immediately

### Equipping Items
`equip_it()` handles context-specific placement: armor swaps with existing armor, guns go to gun inventory, ammo to ammo inventory, equipment finds the first empty grid spot.

### Armor Stats Summary
`read_armor_stats()` provides a complete overview: shield status, battery level, power generation (generator + solar), movement bonus (exoskeleton count x 30%), and equipment count.

### Key Files
- `scripts/equipment.lua` - Equipment management, stats, grid navigation
- `scripts/ui/tabs/equipment-grid.lua` - Equipment grid UI

---

## 15. Combat System

### Problem
Combat in Factorio requires visually tracking enemy positions, manually aiming weapons, and managing health. None of this works without sight. Blind players need automatic targeting, spatial enemy awareness, and non-visual health feedback.

### Solution

### Combat Mode
- **Ctrl+Shift+I**: Toggle combat mode

Combat mode changes the entire control scheme: WASD now sets aim direction instead of moving the cursor, the audio reference point switches from cursor to character, and all open UIs close to focus on combat.

### Aim Assist (Automatic Targeting)
In combat mode, the player never manually aims. The aim assist system selects the best target based on configurable preferences:

- **W/A/S/D** (in combat): Set preferred aim direction
- **Shift+R**: Toggle "spawners first" (target nests before units)
- **Ctrl+R**: Toggle "strongest first" vs "closest first"
- **Ctrl+Shift+R**: Toggle safe mode (avoid self-damage from area weapons)

The targeting algorithm finds all enemies in weapon range, filters by range constraints, applies safe mode buffers for area-damage weapons, and scores targets by direction alignment, spawner status, and health/distance preference.

If no valid target exists, the system announces the reason: "no weapon equipped", "no enemies in range", "all enemies too close for safe shot", etc.

### Capsules (Grenades, Combat Robots)
- **Shift+W/A/S/D** (in combat): Throw capsule in direction
- **Ctrl+Shift+W/A/S/D**: Force-fire at max range regardless of targets
- **[**: Use healing capsule on self

Capsules are categorized: SELF (healing), AREA_DAMAGE (grenades), SPAWNER (combat robots), REMOTE (artillery flare), CLIFF (cliff explosives). Each has appropriate safety constraints.

### Audio Feedback
- **Enemy radar**: Continuous positional pinging (see Sonification section)
- **Spawner radar**: Distinct sound for nests
- **Health bar**: Stereo-panned beeps for health/shield levels
- **Battle notice**: Alert when structures are damaged

### Repair System
- One-click repair of selected entity
- Area repair of all structures within radius
- Reports count repaired, area, and repair packs consumed

### Gun Menu
A 3x2 grid UI for managing equipped weapons and ammo, with automatic announcements when gun selection changes.

### Key Files
- `scripts/combat.lua` - Combat mode state, repair functions
- `scripts/combat/aim-assist.lua` - Automatic targeting logic
- `scripts/combat/player-weapon.lua` - Weapon property queries
- `scripts/combat/combat-data.lua` - Weapon/ammo/enemy data extraction from prototypes
- `scripts/combat/capsules.lua` - Capsule handling
- `scripts/ui/menus/gun-menu.lua` - Weapon management UI

---

## 16. Transport Belts

### Problem
Belt networks are fundamentally visual: you watch items flow, spot bottlenecks, and trace paths by following the belt with your eyes. Blind players need structural and content analysis of belt networks.

### Solution

### Belt Analysis
The belt system wraps entities in `Node` objects and analyzes their geometry:
- **Shape detection**: Straight, corner (left/right turn), merge, sideload, splitter, pouring
- **Lane contents**: Items in each lane (left/right) at each slot position
- **Upstream/downstream walking**: Traces belt paths in both directions, stopping at mergers or sideloads
- **Carries heuristic**: Determines what a belt "might be carrying" by searching up/downstream for source entities

### Belt Analyzer UI
Opens a detailed view of belt contents showing left/right lane items, upstream/downstream aggregates, and total inventory.

### Underground Belt Intelligence
- Cursor skip automatically jumps to the other end of underground sections
- Build preview shows whether a new underground belt would connect and at what distance
- Build system auto-rotates underground belts to form correct entrance/exit orientation

### Splitter Configuration
Accessible UI for setting input/output priority (left/right/none) and output filter (item to prioritize).

### Key Files
- `scripts/transport-belts.lua` - Belt network analysis, shape detection, lane walking
- `scripts/ui/belt-analyzer.lua` - Belt contents UI
- `scripts/ui/tabs/splitter-config.lua` - Splitter settings UI

---

## 17. Pipes and Fluids

### Problem
Pipe networks are visual; you trace connections and check fluid types by hovering. Underground pipes are especially confusing without being able to see the connection lines.

### Solution

### Connection Analysis
`get_connection_points()` enumerates all fluid connections on an entity with direction, type constraints, and open/closed status.

### Pipe Shapes
Shape detection classifies pipes as: straight (vertical/horizontal), end (1 connection), corner (L-shaped), T-junction, cross (4 connections), or alone (0 connections). This is announced to help players understand their pipe layout.

### Underground Pipe Preview
Build preview checks whether a new pipe-to-ground would connect to an existing one and announces the target and distance. Cursor skip jumps directly to underground connections.

### Fluid Descriptors
Summaries of all fluids in an entity: name, amount, production type (input/output/bidirectional), and lock status (recipe-locked on crafting machines).

### Key Files
- `scripts/fluids.lua` - Connection analysis, pipe shapes, fluid descriptors

---

## 18. Electrical Network

### Problem
Factorio's power grid is visual: blue lines show wire connections, green areas show power coverage. Blind players can't see any of this.

### Solution

### Build Preview for Poles
When placing an electric pole, the system announces up to 5 nearby poles that will connect, showing distance and direction to each. If no poles are in range, it shows the nearest one.

### Power Status
Build preview for power-consuming entities shows whether they'll be connected to the grid and the direction to the supplying pole.

### Build Lock for Poles
The electric pole build lock backend automatically spaces poles at maximum wire distance for efficient coverage while walking.

### Key Files
- `scripts/electrical.lua` - Electric pole connection analysis

---

## 19. Circuit Networks

### Problem
Circuit networks allow complex automation logic using wires, combinators, and conditions. The visual wire overlay and combinator GUIs are inaccessible.

### Solution

### Wire Management
- **Wire dragging**: Connect red, green, or copper wires between entities with audio feedback showing the connected entity name and direction
- **Alt+N**: Remove wires (circuit first, then copper)

### Circuit Network Navigator
- **Ctrl+N**: Opens a navigator to trace circuit connections between entities

### Combinator Configuration UIs
Accessible configuration for:
- **Arithmetic combinators**: Set operations and signals
- **Decider combinators**: Set conditions and outputs
- **Selector combinators**: Configure selection logic
- **Constant combinators**: Set signal values

Each has a keyboard-driven form with signal pickers, comparator cycling, and condition editing.

### Control Behavior Descriptors
`scripts/control-behavior-descriptors.lua` describes what properties each entity type can control via circuits: input/output definitions, condition types, and field types (boolean, signal, condition, choice).

### Key Files
- `scripts/circuit-network.lua` - Wire dragging and removal
- `scripts/control-behavior-descriptors.lua` - Entity circuit properties
- `scripts/ui/tabs/` - Combinator configuration UIs

---

## 20. Train System

### Problem
Trains are one of Factorio's most complex systems: scheduling, signals, pathfinding, multi-stop routes. The visual schedule editor and map-based route planning are completely inaccessible.

### Solution

### Train Identification
Each train gets a stable reference via its lowest-unit-number locomotive. Trains can be named (applied to all locomotives via backer_name). State is always reported: "manual mode", "headed to Iron Unload", "waiting at station".

### Schedule Reader
`scripts/rails/schedule-reader.lua` recursively parses train schedules into speech, handling every condition type: inventory (full, empty, not_empty), circuit (item_count, fluid_count), station (destination_full, at_station), time-based (seconds, inactivity), damage, and AND/OR logic.

### Schedule Editor
Text-based UI for creating and editing schedules without visual editing. Supports condition type cycling, signal/item/fluid pickers, station selectors, and comparator cycling for circuit conditions.

### Train Overview
Browse all trains on a surface or globally. Lists trains by name with position and state. Can move cursor to any train's location or open detailed configuration.

### Train Stop Configuration
- Station name editing
- Train limit (0 to unlimited)
- Priority (0-255)
- Read-only train count

### Locomotive Configuration
- Manual/automatic mode toggle
- Train name editing
- "Go to stop" temporary schedule entries
- Train contents and fuel inventory display

### Key Files
- `scripts/rails/train-helpers.lua` - Train management and state
- `scripts/rails/schedule-reader.lua` - Schedule parsing to speech
- `scripts/ui/schedule-editor.lua` - Schedule editing UI
- `scripts/ui/tabs/trains-overview.lua` - Train overview UI
- `scripts/ui/tabs/locomotive-config.lua` - Locomotive settings
- `scripts/ui/tabs/train-stop.lua` - Train stop settings

---

## 21. Virtual Train Driving and Rail Building

### Problem
Rail building in Factorio requires precise visual placement of curved and straight rail segments. This is perhaps the most visually demanding construction task in the game.

### Solution
A turtle graphics system for rail building. A virtual "train" drives along rails, placing new segments as it moves.

### How It Works
1. Lock onto an existing rail or start from scratch
2. Use movement commands: extend forward, turn left, turn right
3. The system determines the correct rail type (straight, curved A, curved B, half-diagonal)
4. Rails are placed using synthesized blueprints to avoid hand conflicts
5. Cost is calculated in normal mode; ghost placement in forced mode

### Additional Features
- **Signal placement**: Left/right signal or chain signal beside current rail with automatic direction
- **Speculation mode**: Preview moves without building (changes cursor only)
- **Bookmarks**: Mark rail positions for quick return
- **Undo**: Destroy last placed segment and revert position

### Key Files
- `scripts/rails/virtual-train-driving.lua` - Turtle graphics rail builder
- `scripts/rails/build-helpers.lua` - Ghost placement and reviving

---

## 22. Syntrax Rail Description Language

### Problem
Even turtle graphics is slow for complex rail layouts. Players who understand what they want need a faster way to describe it.

### Solution
Syntrax is a domain-specific text language for describing rail layouts. Players type rail descriptions and the system compiles them into placements.

- Supports straight, curved, and diagonal rails with directions
- Signal placement in patterns
- Respects build costs in normal mode, places ghosts in forced mode
- Reports placement failures with group/position info

### Key Files
- `scripts/rails/syntrax-runner.lua` - Syntrax execution engine
- `scripts/rails/syntrax-custom-programs.lua` - Built-in programs
- `scripts/ui/internal/syntrax-input.lua` - Text input UI for Syntrax code

---

## 23. Vehicles

### Problem
Driving vehicles (cars, tanks) requires seeing the road, obstacles, and surroundings. Blind players need spatial awareness while driving.

### Solution

### Driving Feedback
- **Proximity alerts**: Scans for obstacles ahead, with configurable trigger distances (1, 3, 10, 25, 50 tiles). Rate-limited and speed-aware.
- **Vehicle sonifier**: Audio feedback encoding direction and orientation via stereo panning and pitch variation
- **K**: Announce vehicle heading and speed

### Vehicle Cycling
- **Shift+V**: Cycle through all cars and spidertrons within 100 tiles
- Sorted by distance, announces vehicle type and position in list

### Key Files
- `scripts/driving.lua` - Vehicle state and proximity alerts
- `scripts/vehicle-cycler.lua` - Nearby vehicle discovery and cycling

---

## 24. Spidertron Remote Control

### Problem
Spidertrons are remote-controlled walking vehicles. Managing them requires visual map interaction.

### Solution

### Remote Group Management
- Add/remove spidertrons to remote selection
- Toggle individual spidertrons on/off
- Cycle through selected spidertrons (moves cursor to each)

### Autopilot
- Add positions to autopilot queue for all selected spidertrons
- Clear autopilot with optional clear-first mode
- Multiple waypoint support

### Configuration UI
- Entity naming
- Autopilot destination picker
- Follow target selector
- Autotarget settings (with/without gunner)

### Key Files
- `scripts/spidertron-remote.lua` - Group management and autopilot
- `scripts/ui/tabs/spidertron-config.lua` - Spidertron settings UI

---

## 25. Logistics and Robot Networks

### Problem
Logistics networks use visual overlays to show coverage zones and request/provide states. Blind players need structured access to network configuration and status.

### Solution

### Personal Logistics
- Toggle personal logistics requests
- Toggle robot dispatch (allow_dispatching_robots)
- State change announcements

### Logistics Configuration UI
Accessible management for logistics points on any entity:
- **Overview tab**: All logistic points and compiled state
- **Section editor tabs**: Per-section manual configuration
- **Groups**: Organize sections into named groups

### Logistics Filters
Detailed filter management with item name, quality, min/max counts. Formatted readout for each filter.

### Network Information
- Roboport charging/charged robot counts
- Network member counts (construction + logistic robots)
- Storage chest breakdown by type
- Closest network discovery with distance and direction

### Entity Types with Logistics
Characters, spider-vehicles, cars, logistic containers, roboports, rocket silos, and space platform hubs all have accessible logistics point configuration.

### Key Files
- `scripts/worker-robots.lua` - Logistics network interaction
- `scripts/logistic-descriptors.lua` - Entity-to-logistics metadata mapping
- `scripts/ui/logistics-config.lua` - Logistics configuration UI
- `scripts/ui/tabs/logistics-unified.lua` - Unified logistics overview
- `scripts/ui/tabs/logistics-section-editor.lua` - Section editing

---

## 26. Alerts and Warnings

### Problem
Factorio generates alerts (structure destroyed, turret out of ammo, train issues) as small visual icons. Blind players miss all of them.

### Solution

### Alert System
`scripts/ui/tabs/alerts.lua` integrates Factorio's native alert system with an accessible UI:
- **Overview tab**: Alert count per type with mute/unmute controls
- **Per-type tabs**: Browse individual alerts
- **Alert types**: entity_destroyed, entity_under_attack, turret_fire, turret_out_of_ammo, train_no_path, train_out_of_fuel, no_material_for_construction, not_enough_repair_packs, no_storage, custom
- Deduplication, sorting by surface/distance/position, and muting support

### Warning System
`scripts/warnings.lua` proactively scans nearby production structures for problems:
- **NO_FUEL**: Burner entities with empty fuel inventory
- **NO_MINABLE_RESOURCES**: Mining drills on depleted patches
- **NO_POWER**: Electric entities with zero energy
- **NOT_CONNECTED**: Entities not connected to power grid
- **NO_RECIPE**: Crafting machines with no recipe set

### Battle Notice
Plays an alert sound to all team members when player-owned structures are damaged or destroyed. Uses both event-based detection (immediate) and polling-based detection (periodic checks).

### Death and Respawn
When a player dies, ALL players are notified with the killer, location, and corpse position. On respawn, the cursor resets to the spawn location.

### Key Files
- `scripts/alerts.lua` - Alert utilities
- `scripts/ui/tabs/alerts.lua` - Alert UI
- `scripts/warnings.lua` - Factory problem scanning
- `scripts/sonifiers/battle-notice.lua` - Structure damage alerts

---

## 27. Fast Travel

### Problem
Factorio's world is enormous. Navigating between distant locations by walking or scrolling is impractical for blind players who can't see the map to plan routes.

### Solution
A bookmark-based teleportation system:
- **Ctrl+T**: Open fast travel menu
- Create labeled travel points at any location with custom names and descriptions
- Teleport to any saved point instantly
- Relocate or delete travel points
- Move cursor to point location for inspection

Safety checks prevent teleporting in vehicles, near enemies (unless overridden), or to the current location.

### Key Files
- `scripts/travel-tools.lua` - Travel point management
- `scripts/teleport.lua` - Teleportation with safety checks
- `scripts/ui/menus/fast-travel-menu.lua` - Fast travel menu UI

---

## 28. Audio Rulers

### Problem
Grid alignment is critical for Factorio construction (e.g., spacing assembling machines, lining up belts). Sighted players use the visual grid. Blind players need an audio equivalent.

### Solution
Invisible alignment guides that play sounds when crossed:

- **Ctrl+Alt+B**: Place an audio ruler at the current cursor position
- **Alt+Shift+B**: Delete the ruler

When the cursor or character crosses a ruler line, an alignment sound plays. Cursor skip also stops at ruler boundaries. This allows players to set up reference lines for grid-based building.

### Key Files
- `scripts/rulers.lua` - Audio ruler state and crossing detection

---

## 29. Settings System

### Problem
Players need to customize the mod's behavior, particularly audio feedback levels, without visual menus.

### Solution
Four core boolean settings accessible via an in-game form:

- **Ctrl+Alt+Shift+M**: Open settings menu

| Setting | Default | Purpose |
|---------|---------|---------|
| fa-inserter-sonification | ON | Tone feedback when inserters move items |
| fa-crafting-sonification | ON | Sound when crafting machines complete |
| fa-combat-enemy-sonification | ON | Spatial audio radar for enemies |
| fa-combat-spawner-sonification | ON | Ping sounds for enemy spawners |

Settings are declared in `scripts/settings-decls.lua`, rendered dynamically in a FormBuilder UI, and applied immediately.

### Key Files
- `scripts/settings-decls.lua` - Setting declarations
- `scripts/ui/menus/settings-menu.lua` - Settings UI
- `locale/en/settings.cfg` - Setting labels and descriptions

---

## 30. Vanilla Mode

### Problem
In multiplayer, sighted and blind players may want to share the same save file. The mod's cursor graphics and input overrides interfere with standard mouse controls.

### Solution
- **Ctrl+Alt+Shift+V**: Toggle vanilla mode

When enabled:
- Disables all mod cursor graphics
- Re-enables mouse selection
- Silences all TTS output (checked by `VanillaMode.is_enabled()`)
- Closes open UIs
- Allows a sighted player to play normally

The blind player can toggle it back on at any time.

### Key Files
- `scripts/vanilla-mode.lua` - Vanilla mode toggle and state

---

## 31. Tutorial System

### Problem
New blind players need to learn both Factorio's complex gameplay AND the mod's entirely custom control scheme. There is no visual tutorial they can follow.

### Solution
A 15-chapter built-in tutorial:

- **Ctrl+T**: Open tutorial

### Structure
- **15 chapters** covering: getting started, mining, building, factory organization, combat, trains, circuit networks, rockets, and more
- Each chapter has **text content** (navigated with W/S) and optional **example blueprints** that can be imported directly
- Players can close and return later, keeping their place
- Tab key switches between chapter text and blueprints

### Pedagogical Approach
- Front-loads "how to think about Factorio" for blind players
- Emphasizes repeating patterns and modular design
- Explains why organization is even more critical without visual feedback
- Progressive complexity from basics to advanced topics

### Key Files
- `scripts/tutorial-data.lua` - Chapter definitions and blueprint strings
- `scripts/ui/menus/tutorial.lua` - Tutorial UI
- `locale/en/tutorial/` - Tutorial message list text files

---

## 32. Help System

### Problem
With 195 keybindings and dozens of UIs, players need instant access to contextual help without leaving their current task.

### Solution

### Context-Sensitive Help
- **Shift+/**: Toggle help for the current UI
- Navigate help messages with W/S
- Each UI declares its own help metadata

### Help Content
14+ help files covering: general UI controls, menu navigation, crafting, inventory management, blueprint books, research, train overview, and more. Stored as `.txt` files under `locale/en/ui-help/` and `locale/en/help/`.

### Message List System
Help content uses a message list format: messages separated by blank lines in `.txt` files. `build_message_lists.py` processes these into locale keys. Content is lazy-loaded and cached per player on first access.

### Key Files
- `scripts/ui/help.lua` - Help UI and message list integration
- `scripts/message-lists.lua` - Message list loading and caching
- `locale/en/ui-help/` - UI-specific help files
- `locale/en/help/` - General help files

---

## 33. Launcher and External Communication

### Problem
Lua mods in Factorio run in a sandbox with no access to OS-level audio APIs, screen readers, or file system. The mod needs a way to produce speech and spatial audio.

### Solution
An external Python launcher (`launch_factorio.py`) that:

1. **Launches Factorio** with appropriate arguments
2. **Captures stdout** in real-time, parsing two protocols:
   - `out <pindex> <message>` - Routes to OS screen reader (NVDA, JAWS, Narrator, VoiceOver, speechd)
   - `acmd <pindex> <json>` - Routes to spatial audio subsystem
3. **Provides development tools**: code formatting (stylua), linting (lua-language-server), test running, annotation validation

### Supported Screen Readers
- NVDA (Windows) - primary target
- JAWS (Windows)
- Windows Narrator
- speechd (Linux)
- VoiceOver (macOS)

### Key Files
- `launch_factorio.py` - External launcher and TTS bridge

---

## 34. Keybinding System

### Problem
Factorio is designed for mouse + keyboard. Blind players need everything accessible from the keyboard alone, with a consistent and learnable scheme.

### Solution
195 custom input definitions using a systematic naming convention:
- `fa-<key>`: Base key (e.g., `fa-w`)
- `fa-s-<key>`: Shift modifier (e.g., `fa-s-w`)
- `fa-c-<key>`: Ctrl modifier (e.g., `fa-c-w`)
- `fa-a-<key>`: Alt modifier (e.g., `fa-a-w`)
- `fa-cs-<key>`, `fa-ca-<key>`, `fa-as-<key>`, `fa-cas-<key>`: Combined modifiers

### Ergonomic Design
Controls are designed for two-handed "drum roll" operations:
- Left hand on WASD (movement), right hand on brackets (actions)
- Example: D then [ repeatedly for building a line of entities

### Context Sensitivity
The same key has different meanings depending on mode:
- **W**: Cursor north (normal), aim north (combat), navigate up (in UI)
- **R**: Rotate (building), toggle spawners-first (combat mode)
- **[**: Build (map), click (UI), craft (crafting menu)

### Key Categories
| Category | Keys | Purpose |
|----------|------|---------|
| Cursor movement | W/A/S/D | Tile-by-tile navigation |
| Cursor skip | Shift+W/A/S/D | Fast navigation to changes |
| Building | [ / Shift+[ / R / H / V | Place, ghost-place, rotate, flip |
| Information | K / Y / ] | Entity info, hand info, status |
| Inventory | 1-0, Ctrl+1-0, Shift+1-0 | Quickbar select, set, page |
| UI navigation | Tab, Ctrl+Tab, E | Tab switching, open/close |
| Scanner | Home, End, PageUp/Down | Browse categorized entities |
| Combat | Ctrl+Shift+I, Space | Toggle mode, fire |
| Tools | Q, X, F, Z | Pipette, mine, pickup, drop |

### Key Files
- `data/input.lua` - All 195 custom input definitions
- `locale/en/controls.cfg` - Keybinding labels for settings menu

---

## 35. Quality System Support

### Problem
Factorio 2.0 introduced item quality levels (normal, uncommon, rare, epic, legendary). Items with the same name but different qualities have different stats and must be distinguished.

### Solution
Quality is integrated throughout the entire mod:

- **Item aggregation**: `aggregate_inventory()` groups by both name AND quality
- **Presentation**: `present_list()` shows quality in names ("legendary solar panel x5")
- **Transfers**: `quick_transfer()` preserves quality when moving items
- **Equipment**: Equipment quality affects stats; selection UI sorts by quality (highest first)
- **Research**: Craft-item triggers can specify quality requirements
- **Filters**: Quality-aware filter setting on inserters and splitters

### Key Files
- `scripts/item-stack-utils.lua` - Quality-aware aggregation

---

## 36. Multiplayer Support

### Problem
Blind players may want to play with sighted friends, or multiple blind players together.

### Solution
The mod supports multiplayer through:
- Per-player indexed storage for independent state
- Force-level resources shared among team members
- All output routed to individual player indexes
- Vanilla mode allows sighted players to use the same save
- Death notifications sent to all players with location details

### Current Limitations
Multiplayer is acknowledged as "laggy and terrible" due to network latency affecting audio feedback timing. Combat is especially problematic. Limited development resources make multi-player debugging difficult.

---

## Appendix: Custom Entities

### Access Radar
A modified radar entity with accessibility-optimized parameters:
- **Faster scanning**: 5-second sectors vs 33 seconds for vanilla radar
- **Larger scan radius**: 1024 tiles vs 448
- **Higher power usage**: 600kW vs 300kW
- Available in accessibility-focused map presets

### Map Presets
- **faccess-compass-valley**: Peaceful, resource-rich, structured layout
- **faccess-enemies-off**: No enemies
- **faccess-peaceful**: Peaceful mode enabled
