# ONI Accessibility Mod: Full Requirements Analysis

## Factorio Access as Inspiration, Oxygen Not Included as Target

This document synthesizes the architecture and concepts of the Factorio Access mod with a full breakdown of Oxygen Not Included's gameplay systems. For each accessibility requirement, it identifies whether a direct mapping from Factorio Access exists, whether a concept needs adaptation, or whether an entirely new solution must be invented.

## Technical Foundation Comparison

### Factorio Access

Factorio Access is written in Lua using the official Factorio modding API. It includes an external Python-based launcher that handles text-to-speech (TTS) via the native Windows vocalizer or speechd on Linux. The launcher acts as a bridge between the game and the screen reader, since Factorio's own modding API cannot directly invoke TTS. The mod intercepts game events and converts visual state into narrated audio output.

### Oxygen Not Included

ONI is a Unity/C# game. Modding is done via Harmony, a runtime method-patching library for .NET/Mono. Mods are compiled as C# DLLs and loaded by the game's built-in mod loader. This means the accessibility mod would be a Harmony-based C# mod. Unlike Factorio, where the mod API is sandboxed Lua with defined event hooks, ONI modding gives near-full access to the game's internal classes via reflection and patching. This is both more powerful (you can intercept virtually any method) and more fragile (game updates can break patches).

The TTS approach would likely need a similar external bridge, a companion process that receives text from the mod and speaks it via SAPI/NVDA/JAWS on Windows, or speechd on Linux. Alternatively, the mod could write to a named pipe or use .NET's System.Speech.Synthesis directly on Windows, though cross-platform support would require the external approach.

### Verdict: Partial Direct Map

The launcher/TTS bridge concept from Factorio Access maps directly. The mod architecture differs fundamentally (Lua event hooks versus C# Harmony patches), but the overall pattern of "intercept game state, convert to audio" is identical.

---

## World Model: The Fundamental Divergence

This is where the two games diverge most dramatically and where the greatest amount of new invention is needed.

### Factorio: Top-Down 2D Grid

Factorio's world is a flat, top-down grid of square tiles. Everything exists on a single plane. Entities occupy rectangular footprints on this grid. Navigation is inherently simple: north/south/east/west on a flat plane. The Factorio Access cursor moves tile-by-tile, and the scanner indexes entities within a radius, reporting them by type, distance, and compass direction.

### ONI: Side-View 2D Grid with Layered Simulation

ONI is also tile-based, but it is a side-view (platformer perspective) world. The grid has X (horizontal) and Y (vertical/depth) axes, but gravity matters. Duplicants walk on floors, climb ladders, and use poles. Additionally, every single tile has multiple overlapping data layers simultaneously: the solid/liquid/gas element occupying that tile, its temperature, its mass/pressure, its germ count and germ type, its decor value, its light level, and potentially a building occupying that tile, a pipe running through it (liquid pipe, gas pipe, or both), an automation wire, a conveyor rail, and background buildings. A single tile can contain a staggering amount of simultaneous information.

### What This Means for Accessibility

In Factorio Access, the cursor reads "Iron ore patch, 47 tiles, 12 tiles northeast." In ONI, a single tile might need to report: "Sandstone tile, 28 degrees Celsius, oxygen at 1.8 kilograms, no germs, copper ore debris on ground, insulated liquid pipe carrying 10 kilograms of polluted water at 32 degrees, automation wire carrying green signal, decor minus 10, in Barracks room." This information density per tile is an order of magnitude greater than Factorio.

### Verdict: Major New Invention Required

The Factorio Access cursor and scanner concepts are a starting point, but ONI requires a fundamentally new approach to information layering and filtering. The player must be able to choose which layer of information they care about at any given moment.

---

## Concept-by-Concept Mapping

### Section 1: Navigation and Spatial Awareness

#### 1.1 Tile-by-Tile Cursor Movement

Factorio Access provides a cursor that moves one tile per keypress (telestep mode), reporting what occupies each tile. This maps directly to ONI, but with the critical addition of the Y axis. ONI navigation needs up/down movement (ladders, poles, open space) in addition to left/right.

Mapping: Direct, with vertical axis added. Use arrow keys or WASD for cursor movement on the 2D side-view grid.

#### 1.2 Cursor Information Readout

In Factorio Access, landing on a tile reads the entity name, type, and basic status. In ONI, the readout must be layered. A recommended approach is a brief default readout (element type, building name if present) with modifier keys to query specific layers.

For example: default readout says "Granite tile, Manual Generator." Pressing a layer key adds "Oxygen, 1.8 kg, 24 C." Another layer key adds "Power wire, 960W load." This layer-query system has no direct parallel in Factorio Access.

Mapping: Concept adapted. The base cursor exists but the multi-layer query system is new.

#### 1.3 Scanner Tool (Area Survey)

Factorio Access has a scanner that indexes all entities within roughly 1000 tiles, grouped by type. You cycle through types with Page Up/Down and instances with Shift+Page Up/Down. This concept maps strongly to ONI but needs rethinking because of room-based and vertical architecture.

ONI scanner should support: scan by building category (all generators, all pumps, all farm tiles), scan by duplicant (where is each dupe, what are they doing), scan by resource (where is the nearest water, copper ore, etc.), scan by alert/problem (where is the flooded area, the overpressured vent, the entombed building). ONI's existing notification system already flags some of these; the scanner would formalize querying them.

Mapping: Strong conceptual map. Implementation needs ONI-specific categories and vertical distance reporting.

#### 1.4 Fast Travel / Bookmarks

Factorio Access allows naming and teleporting between saved points. This maps directly. ONI already has camera bookmark positions (Ctrl+1 through 0 to save, Shift+1 through 0 to recall). The accessibility mod would narrate the bookmark system and extend it with cursor teleportation.

Mapping: Direct.

#### 1.5 BStride (Building-to-Building Navigation)

Factorio Access has BStride, which lets you hop between placed buildings as if they were nodes in a grid. This concept is very applicable to ONI, where a blind player would benefit enormously from jumping between buildings of the same type or within the same pipe/wire network.

Mapping: Direct concept, ONI-specific implementation needed for vertical layout.

#### 1.6 Coordinate System and Distance Reporting

Factorio Access reports tile coordinates and compass directions. ONI uses X,Y coordinates. The mod should report coordinates and relative position (e.g., "12 tiles right, 8 tiles below"). Since ONI is side-view, "north" and "south" would be confusing; use "left/right/above/below" instead.

Mapping: Adapted. Replace compass directions with spatial directions appropriate to side-view.

---

### Section 2: Building and Construction

#### 2.1 Placing Buildings

In Factorio, you select an item from inventory, the cursor shows a preview, and you place it. Building rotation is done with R. In ONI, buildings are placed from a build menu (not from inventory). You select a category (Base, Oxygen, Power, Food, Plumbing, Ventilation, etc.), then a specific building, then click to place it on the grid. Buildings have specific placement rules: they need foundation support, clear space, specific orientations for input/output ports.

The build menu is heavily mouse-driven with subcategories. The mod would need to convert this into a navigable audio menu, similar to how Factorio Access handles the crafting menu.

Mapping: Partially mapped. The concept of "select item, place on grid" exists in both, but ONI's build menu hierarchy and placement constraints are more complex. New audio menu system needed for the build categories.

#### 2.2 Building Rotation and Orientation

Factorio buildings rotate in 4 directions. ONI buildings generally have fixed orientation but have specific input/output port positions (left side, right side, top, bottom). Some buildings can be flipped. The mod must announce port positions clearly: "Liquid input on left, liquid output on right, power connection on bottom."

Mapping: Adapted. Less rotation, more port-awareness.

#### 2.3 Pipe and Wire Routing

In Factorio, you drag belts, pipes, and wires across the grid. Factorio Access handles this with cursor-based placement tile by tile. ONI has five separate conduit networks that can all overlap in the same tile: liquid pipes, gas pipes, automation wire, conveyor rails, and power wire. Each is placed independently and occupies the same tile space.

This is a massive accessibility challenge. The player needs to know what is already in a tile across all networks before placing new conduit. The overlay system (see Section 4) becomes critical here.

Mapping: Conceptually similar (tile-by-tile conduit placement), but the overlay complexity is a new challenge.

#### 2.4 Drag-to-Build (Area Operations)

Factorio Access supports placing multiple entities by holding a key and moving the cursor. ONI heavily uses drag-to-place for tiles, pipes, wires, and dig/mop/sweep commands. The mod needs a "start drag, move cursor, end drag" paradigm with audio feedback for the area being selected.

ONI also supports priority painting (dragging priority numbers onto buildings/errands), which has no Factorio parallel.

Mapping: Partial. Drag mechanics exist in both but ONI uses them more pervasively.

#### 2.5 Deconstructing and Canceling

Both games have deconstruct tools. In ONI, you also have cancel (C key), which cancels pending build/dig orders. The mod should announce pending orders on tiles and support both operations.

Mapping: Direct.

---

### Section 3: Resource and Inventory Management

#### 3.1 Personal Inventory

Factorio has a rich personal inventory system: items in slots, equipment grids, toolbelts. ONI duplicants do not have traditional inventories that the player manages. Duplicants automatically pick up and carry items as needed for errands. The player manages storage buildings (Storage Compactors, Ration Boxes, etc.) and their filter settings.

Mapping: No direct equivalent. ONI requires a storage management system rather than personal inventory management.

#### 3.2 Resource Tracking

Factorio Access reads inventory counts when you press keys. ONI has a resource panel showing total colony resources by type. The mod needs to read this panel: "Colony has 4,200 kg of sandstone, 1,800 kg of copper ore, 320 kg of algae" etc.

Mapping: Adapted. Global resource view rather than personal inventory.

#### 3.3 Crafting

Factorio has personal hand-crafting and machine recipes. ONI has no hand-crafting at all. All production happens in buildings (Microbe Musher, Electric Grill, Rock Crusher, etc.) that duplicants operate. The player assigns recipes to buildings and duplicants execute them based on priority.

Mapping: No direct parallel for hand-crafting. Machine recipe assignment is conceptually similar.

---

### Section 4: Overlay and Environmental Awareness System (Entirely New Concept)

This is the single largest new invention required and the most critical system to get right. The overlay/environmental awareness system is what makes ONI the game it is. A sighted player glances at a visual overlay and instantly absorbs a spatial pattern — "oxygen is thin on the right side, CO2 is pooling at the bottom, there's a chlorine pocket in that cavern." This gestalt understanding drives every decision in the game. The accessibility mod must provide equivalent strategic awareness through non-visual means.

ONI has approximately 15 visual overlays that show different data layers on the same world:

1. Oxygen overlay: shows gas type and pressure per tile, color-coded
2. Power overlay: shows wires, loads, generators, circuits
3. Temperature overlay: shows temperature per tile, color-coded
4. Liquid plumbing overlay: shows liquid pipes, flow, contents
5. Gas ventilation overlay: shows gas pipes, flow, contents
6. Decor overlay: shows decor values per tile
7. Germ overlay: shows germ count and type per tile
8. Light overlay: shows light levels
9. Room overlay: shows recognized rooms and their types/bonuses
10. Farming overlay: shows farm tiles and plant status
11. Priority overlay: shows sub-priority numbers on buildings/errands
12. Exosuit overlay: shows exosuit checkpoint configuration
13. Automation overlay: shows automation wires and signal states
14. Conveyor overlay: shows conveyor rails
15. Material overlay: shows what elements make up each tile
16. Radiation overlay (DLC): shows radiation levels

The accessibility system addresses this through three integrated layers: togglable speech overlays, atmospheric sonification, and game-state problem detection.

#### 4.1 Togglable Speech Overlays

Each of the 15 overlays becomes a togglable information layer that reports via speech. Overlays can be individually turned on and off, and multiple overlays can be active simultaneously. There are two interaction modes per overlay:

**Query mode (default):** The overlay is off. The player presses a key to hear the overlay data at the current cursor tile on demand. For example, pressing the oxygen overlay key once reads: "Oxygen, 1.8 kilograms, 25 degrees."

**Continuous mode (toggled on):** The overlay is active. As the cursor moves, the overlay data is automatically announced at each new tile. Multiple overlays can be toggled on simultaneously, and their information is concatenated. For example, with oxygen and plumbing both active, moving to a new tile reads: "Oxygen 1.8 kilograms. Insulated liquid pipe, polluted water, 32 degrees."

The player controls information density by choosing which overlays are active at any given time. Investigating a plumbing problem? Toggle on liquid plumbing and temperature. Checking atmosphere? Toggle on oxygen. Building automation? Toggle on automation. The system naturally scales from minimal (everything off, query as needed) to dense (multiple overlays active for detailed investigation).

Overlay hotkeys should match the existing game overlay keys (F1 through F12 and additional bindings) so that guides and wikis remain applicable. A modifier key (such as Shift) toggles continuous mode on/off, while a plain press performs a single query.

#### 4.2 Atmospheric Sonification Layer

Speech overlays solve the detail problem but not the big-picture problem. Reading tile data sequentially does not give the player a gestalt sense of how their base's atmosphere is distributed. For this, the mod provides a separate **sonification layer** — a non-speech audio system that maps atmospheric conditions to sound, allowing rapid spatial scanning.

When the sonification layer is active, the cursor becomes a sonic probe. Moving it through the world plays continuous audio that reflects the local atmosphere:

**Gas type as timbre (3 tones plus silence):**

- **Oxygen:** A clean, neutral mid-range hum. This is the "everything is fine" baseline sound.
- **Carbon dioxide:** A distinct low rumble. This is the most common threat and must be immediately recognizable.
- **Foreign gas (everything else — hydrogen, chlorine, natural gas, polluted oxygen, steam, sour gas, etc.):** A single shared alert tone, clearly distinct from oxygen and CO2. When the player hears this, they know something unexpected is present and can press a key to get a speech readout identifying the specific gas.
- **Vacuum:** Silence. No gas, no sound.

This three-tone-plus-silence design keeps memorization load minimal while still communicating the critical atmospheric information. The identity of a specific foreign gas rarely matters for big-picture scanning — what matters is knowing it is there. Specific identification happens through the speech overlay query when investigating.

**Pressure as volume:** High gas pressure produces louder sound, low pressure produces quieter sound. This allows the player to hear dead zones (quiet areas where gas is not reaching) and overpressurized areas (unusually loud) without any speech.

**Temperature as texture (optional additional layer):** A subtle warm crackle at high temperatures, a crystalline shimmer at cold temperatures, layered on top of the gas tone. This can be toggled independently so the player controls complexity.

**Usage pattern — the sweep:** The player activates sonification mode and sweeps the cursor horizontally across a level of their base. In a few seconds of audio, they hear: full oxygen hum... getting quieter... foreign gas alert... CO2 rumble... silence. They now know: oxygen is present on the left, thinning out, something unexpected in the middle, CO2 pooling further right, vacuum beyond. A vertical sweep reveals gas stratification: high shimmer at top (hydrogen), clean hum in the middle (oxygen), low rumble at bottom (CO2). This is the closest equivalent to glancing at the oxygen overlay.

**Focused sonification modes:** When working with a specific gas (e.g., setting up natural gas power, managing a chlorine disinfection room), the player can switch sonification to track that gas specifically. In "natural gas mode," the tones become: natural gas present (hum) versus absent (silence), with pressure as volume. Same simple vocabulary, different target.

The sonification layer is always separate from the speech overlay system. Both can be active simultaneously — sonification provides the ambient spatial picture while speech provides precise data at the cursor. They complement rather than compete.

#### 4.3 Game-State Problem Detection

Neither speech overlays nor sonification solve the problem of proactive awareness — knowing that something is going wrong before it becomes a crisis. For this, the mod hooks into ONI's own entity status system.

Every entity in ONI already evaluates its own environmental conditions. Plants track whether they are in the right atmosphere and temperature range and flag "stifled" or "wrong atmosphere" when conditions are bad. Buildings track overheat thresholds. Pipes know when their contents are about to change state and burst. Duplicants track breath, comfort, and stress. The game already computes all of this internally.

The mod surfaces these status flags as an ongoing problem report. Rather than hardcoding thresholds for "bad" gas levels or temperatures (which would be inaccurate because tolerances vary by entity), the mod asks each entity whether it considers its conditions problematic, using the game's own definitions.

This provides alerts such as: "3 Mealwood plants stifled by temperature." "Electrolyzer reporting overpressure." "2 duplicants holding breath." "Aquatuner approaching overheat, 12 degrees margin remaining."

The problem detection layer serves as a safety net beneath the other two systems. If the player misses a developing issue during manual scanning, the problem report catches it when entities start struggling. The trade-off is that this is reactive rather than proactive — entities must already be experiencing problems. The sonification layer fills the proactive gap by letting the player sense atmospheric changes before they cause entity distress.

#### 4.4 How the Three Layers Work Together

The three layers serve different purposes at different timescales:

**Sonification (proactive, big picture):** "How is my base's atmosphere right now?" Rapid cursor sweeps give spatial patterns. The player uses this periodically, like a sighted player opening the oxygen overlay. This is the primary tool for strategic atmospheric awareness.

**Speech overlays (detailed, investigative):** "What exactly is happening at this tile?" Toggle on relevant overlays and move through an area to get precise data. The player uses this when diagnosing a specific problem, planning a build, or investigating an area the sonification flagged as concerning.

**Problem detection (reactive, safety net):** "What is already going wrong?" Continuous monitoring of entity status flags catches problems the player has not yet noticed. This runs in the background and surfaces alerts. The player checks the problem report periodically or is notified of critical issues.

Together, these three systems replace the visual overlay with a layered awareness model: sonification for spatial intuition, speech for precision, and entity status for safety.

#### 4.5 Known Limitations and Open Questions

**Discovery gap:** A sighted player might notice a natural gas geyser or chlorine pocket while scrolling past. The sonification layer partially addresses this (the "foreign gas" alert tone would sound during sweeps), but passive discovery of features outside the player's scan path remains weaker than visual. The scanner tool (Section 1.3) helps by allowing targeted searches for gas types or resources in a radius.

**Sonification design iteration:** The specific tones, volumes, and textures for sonification will require extensive testing. The sounds must be distinct, non-fatiguing, and responsive to cursor movement with minimal latency. This is a design challenge that can only be resolved through playtesting, not on paper.

**Information overload with multiple speech overlays:** Toggling on three or four speech overlays simultaneously could produce long readouts per tile. The system may need a brevity mode that gives abbreviated multi-overlay readouts (e.g., "O2 1.8k, pipe PW 32d, wire green") versus a full mode with complete sentences.

**Cursor sweep speed:** Sonification effectiveness depends on how quickly the player can move the cursor and how responsively the audio updates. If cursor movement has a per-tile speech delay in normal mode, the sonification mode may need to suppress speech and allow faster cursor traversal for sweep scanning.

Mapping: No Factorio Access equivalent. Completely new system required. This is the single most important system in the mod and will require the most iteration.

---

### Section 5: Duplicant Management (Largely New)

Factorio has no equivalent to duplicant management. You are a single engineer. ONI requires managing 3 to 30+ autonomous agents. This is an entirely new domain.

#### 5.1 Duplicant Status Monitoring

Each duplicant has: health, stress level, calories, bladder, breath, stamina, body temperature, current errand, current location, attributes (11 different ones), skills, traits, morale, and active status effects.

The mod needs a "follow duplicant" mode and a "duplicant status query" system. Press a key to cycle through duplicants and hear their current status.

Mapping: No Factorio equivalent. Entirely new.

#### 5.2 Priority System

ONI's priority system is its most complex management mechanic. There are two levels: per-duplicant task-type priorities (a grid of duplicant rows versus task-type columns, with priority levels from disabled through low/medium/high/very high), and per-building/errand sub-priorities (numeric 1-9).

Sighted players interact with this through a spreadsheet-like grid UI. The mod needs to make this navigable: "Turner. Research: Very High. Building: Medium. Farming: Disabled." etc. It also needs to support changing priorities with keyboard controls.

Mapping: No Factorio equivalent. Entirely new, complex grid-navigation UI needed.

#### 5.3 Schedule System

ONI has a schedule system where you assign time blocks (Work, Downtime, Bathtime, Sleep) to different schedule groups, and assign duplicants to groups. This is a grid of 24 time-slots by N schedule groups.

Mapping: No Factorio equivalent. New navigable schedule editor needed.

#### 5.4 Skills and Attributes

Duplicants earn skill points and the player assigns them to skill trees. This is a tree/menu UI that needs audio navigation.

Mapping: No direct equivalent. Could partially adapt Factorio Access's research tree navigation concept.

#### 5.5 Duplicant Selection (Printing Pod)

Every 3 cycles, the Printing Pod offers a choice of new duplicants or resources. Each duplicant has randomized stats, traits, and interests. The player must evaluate and choose (or skip).

Mapping: No Factorio equivalent. New selection/comparison UI needed.

---

### Section 6: Environmental Simulation (Integrated with Overlay System)

#### 6.1 Gas/Liquid Simulation Awareness

ONI simulates gas and liquid physics per-tile. Gases stratify by weight (CO2 sinks, hydrogen rises, oxygen in the middle). Liquids flow downward. Understanding this simulation is critical to gameplay. A sighted player glances at the oxygen overlay and immediately sees "CO2 pooling at the bottom of my base."

Environmental awareness is handled primarily through the overlay and sonification system described in Section 4. The atmospheric sonification layer provides the big-picture spatial understanding (sweep-scanning to hear gas distribution and pressure). The togglable oxygen speech overlay provides precise per-tile data when investigating. The game-state problem detection layer alerts when entities are experiencing atmospheric distress (duplicants holding breath, plants stifled).

For liquids, the speech overlay reports liquid type, mass, and temperature per tile. Liquid behavior is less spatially complex than gas (liquids pool predictably at low points), so speech overlay alone is likely sufficient without a dedicated sonification mode.

Mapping: No Factorio equivalent. Handled by the Section 4 overlay/sonification/problem-detection system.

#### 6.2 Temperature Management

Temperature is a critical mid-to-late-game concern. Every tile, building, pipe, and fluid has a temperature. Heat transfers between adjacent materials. Temperature is integrated into the overlay system in two ways: as a togglable speech overlay reporting per-tile temperature, and as an optional sonification texture layer (warm crackle at high temperatures, crystalline shimmer at cold temperatures) that can be combined with the atmospheric sonification for simultaneous gas and temperature scanning. The problem detection layer catches temperature-related entity distress (buildings approaching overheat, crops outside growth range, pipes at risk of state change).

Mapping: No Factorio equivalent beyond basic pollution tracking. Handled by Section 4 system.

#### 6.3 Germ Tracking

ONI has a disease/germ system. Germs contaminate water, air, food, and surfaces. Germ data is available as a togglable speech overlay reporting germ type and count per tile. The problem detection layer alerts when duplicants are exposed to dangerous germ levels or when food/water contamination reaches concerning levels, using the game's own germ-related status flags on entities.

---

### Section 7: Research and Technology

#### 7.1 Research Tree

Factorio Access has research queue navigation and technology readout. ONI has a research tree with tiers, dependencies, and different research station types (Research Station for basic, Super Computer for advanced, Virtual Planetarium for space research, Nuclear Research Station for nuclear).

Mapping: Strong conceptual map. ONI research tree navigation would work similarly to Factorio Access's approach.

#### 7.2 Research Queue

Both games support research queues. Direct mapping.

Mapping: Direct.

---

### Section 8: Automation

#### 8.1 Automation Wiring

Factorio has a circuit network (wires carrying signals between buildings). ONI has an automation system with boolean signals (green/red), sensors, logic gates, and ribbon cables (4-bit). The mod needs to handle the automation overlay layer, report wire connections and signal states, and allow connecting/disconnecting automation ports.

Mapping: Strong conceptual map from Factorio Access's circuit network support.

#### 8.2 Sensor Configuration

ONI sensors have configurable thresholds (temperature above/below X, pressure above/below Y, etc.). These use slider UIs. The mod needs to make these sliders navigable with keyboard input and announce current values.

Mapping: Partial. Factorio Access handles some entity configuration, but ONI's sensor sliders are a new UI pattern.

---

### Section 9: Alerts and Notifications

#### 9.1 Game Alerts

ONI generates notifications for problems: entombed buildings, flooded buildings, idle duplicants, low food, low oxygen, building damage, pipe blockages, etc. These appear as icons on the left side of the screen.

Factorio Access has partial alert support. ONI's mod needs a robust alert queue that the player can cycle through, with the ability to jump to the location of each alert.

Mapping: Partial map, needs significant enhancement for ONI's broader alert types.

#### 9.2 Status Items on Buildings

ONI buildings display status icons (idle, waiting for delivery, no power, overheated, etc.). These need to be part of the building readout when the cursor lands on them.

Mapping: Adapted from Factorio Access's building status readout.

---

### Section 10: Menus and Panels

#### 10.1 Colony Management Screens

ONI has several management screens accessed by hotkeys: Vitals (V), Consumables (F), Priority List (L), Schedule (.), Skills (J), Research (R), Report (E), Starmap (Z), Codex/Journal (U). Each is a complex data-rich screen.

Factorio Access has some equivalent menus (research, production). The ONI mod needs full audio navigation for each of these screens.

Mapping: Partial. Each screen needs individual implementation.

#### 10.2 Building Side Panels

When you click a building in ONI, a side panel opens showing its status, contents, settings, and connected networks. This is where you assign recipes, set filters, configure storage, view production stats, and so on.

Mapping: Analogous to Factorio Access's entity info readout, but much more complex per-building.

#### 10.3 Duplicant Bio Panel

Clicking a duplicant opens their full bio: attributes, traits, skills, morale breakdown, status effects, equipped items, assigned buildings. This needs full audio navigation.

Mapping: No Factorio equivalent. New.

---

### Section 11: Rooms System (New Concept)

ONI recognizes enclosed areas as "rooms" if they meet specific criteria (size range, required furniture, no prohibited items). Rooms provide morale bonuses to duplicants who use them. The room overlay shows all recognized rooms and what requirements are met/unmet.

The mod needs to: announce when the cursor enters a recognized room, report room type and bonus, report missing requirements ("This Mess Hall needs a Decor item to upgrade to a Great Hall"), and support the room overlay as an information layer.

Mapping: No Factorio equivalent. New.

---

### Section 12: Critters and Ranching (New Concept)

ONI has a critter system with various animals (Hatches, Pufts, Dreckos, Slicksters, etc.) that can be wrangled, relocated, and ranched in Stables for resources. This involves critter status monitoring, ranch management, and egg/resource tracking.

Mapping: Factorio has enemy biters, but ranching is entirely new. The scanner concept could be extended to track critter locations and status.

---

### Section 13: Rocketry and Space (DLC, New Concept)

The Spaced Out DLC adds multi-asteroid gameplay, rocketry, and a starmap. This is an entirely separate navigation and management domain.

Mapping: No Factorio equivalent (Factorio 2.0 Space Age was not yet supported by the mod). Would need a separate starmap navigation system and cross-asteroid management.

---

## Summary: Mapping Classification

### Direct Mappings from Factorio Access (can adapt with moderate work)

1. TTS/launcher bridge architecture
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
14. Entity description readout (Y key equivalent)

### Adapted Concepts (significant rework needed)

1. Scanner tool (needs ONI-specific categories, vertical awareness, and much richer grouping)
2. Cursor information readout (needs multi-layer support)
3. Build menu navigation (ONI's category hierarchy is deeper)
4. Building configuration (ONI has more complex per-building settings)
5. Resource tracking (colony-wide rather than personal inventory)
6. Walking/movement modes (duplicants are autonomous; cursor movement replaces player movement)

### Entirely New Inventions Required

1. Overlay and environmental awareness system — the single most critical new concept, comprising three integrated subsystems:
   - a. Togglable/combinable speech overlays (15+ layers, individually toggled on/off, query or continuous mode)
   - b. Atmospheric sonification layer (3-tone gas mapping: oxygen/CO2/foreign plus silence for vacuum, pressure as volume, optional temperature texture, focused modes for specific gases, designed for rapid cursor sweep scanning)
   - c. Game-state problem detection (hooking entity status flags to surface environmental distress without hardcoded thresholds)
2. Duplicant management: status monitoring, following, selection
3. Priority system navigation (the complex duplicant-by-task-type grid)
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

## Recommended Development Phases

### Phase 1: Core Navigation and Awareness

Implement cursor movement, tile readout with basic layer support, scanner tool, bookmarks, and the TTS bridge. This gets a player moving around the world and understanding what is where.

### Phase 2: Building and Construction

Implement the build menu audio navigation, building placement, pipe/wire routing, and deconstruct. This lets the player actually build things.

### Phase 3: Duplicant Management

Implement duplicant status queries, priority system navigation, schedule editor, and skills. This lets the player manage their colony.

### Phase 4: Environmental Monitoring

Implement the full overlay and environmental awareness system: togglable/combinable speech overlays for all 15+ data layers (query and continuous modes), atmospheric sonification layer (3-tone gas type, pressure as volume, optional temperature texture, focused gas modes), and game-state problem detection via entity status hooks. Implement room reporting. This is the most critical phase for mid-to-late-game viability and will require the most iteration through playtesting.

### Phase 5: Advanced Systems

Implement automation configuration, sensor sliders, research navigation, critter management, and colony management screens.

### Phase 6: DLC Content

Implement rocketry, starmap, and multi-asteroid management.

---

## Key Design Principles (Learned from Factorio Access)

1. Preserve the original game experience as much as possible. Convert visual information to audio; do not simplify the game.
2. Provide contextual information by default, with detailed queries available on demand. Do not overwhelm with data.
3. Use consistent navigation patterns. If Page Up/Down cycles entity types in the scanner, use the same pattern in other list contexts.
4. Support multiple information verbosity levels. Quick summary by default, full detail on request.
5. Maintain spatial awareness through consistent coordinate and distance reporting.
6. Provide unique tools where visual equivalents cannot translate. The scanner tool is the prime example.
7. Build incrementally. A partially accessible game is better than no game. Prioritize core survival mechanics first.
