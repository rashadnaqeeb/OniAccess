# Oxygen Not Included: Comprehensive Accessibility Audit

## Every User-Facing Mechanic and Interface Element

*Ordered by Encounter Sequence*

> **Note:** This document catalogues every interface, screen, panel, overlay, menu, and interactive element a player encounters in Oxygen Not Included (base game, Spaced Out DLC, and Bionic Booster Pack DLC). It is organized in the order a player first encounters each element, progressing from launch through early game, mid game, and late game. The goal is to identify every component that must be made accessible for a blind player using a screen reader.

> **Scope:** Covers DLC content as well as the base game. DLC-exclusive elements are marked where applicable. This document describes what exists in the UI, not how to make it accessible.

---

# Phase 1: Application Launch and Main Menus

## 1.1 Splash and Title Screen

The game opens with a Klei Entertainment logo animation, followed by the main title screen. The title screen has a background animation of duplicants and colony elements.

- Klei logo splash (animated, non-interactive, auto-advances)
- Main title screen with animated background

## 1.2 Main Menu

The main menu presents the primary navigation options for the game. These are buttons arranged vertically.

- Resume Game (loads most recent save)
- New Game
- Load Game
- Colony Summaries (achievements viewer)
- Mods (mod manager)
- Options
- Quit

> **Note:** The mod manager is a separate screen allowing toggling of installed Steam Workshop and local mods, with options buttons for individual mods that support configuration.

## 1.3 Options Menu

Accessible from main menu and in-game pause menu. Contains multiple tabs.

- **Graphics:** resolution, fullscreen/windowed, UI scale, V-sync
- **Audio:** master volume, music volume, SFX volume, ambience volume
- **Game:** autosave frequency, temperature units (Celsius/Fahrenheit/Kelvin), enable/disable tutorial messages
- **Controls:** full key rebinding interface with action categories and a scrollable list of bindings
- **Accounts:** Klei account linking

> **Note:** The controls rebinding screen lists every action and its current key. Each row has the action name and a clickable binding field that enters listening mode for a new key.

## 1.4 Load Game Screen

A scrollable list of save files. Each entry shows colony name, cycle number, duplicant count, date saved, and the save file size. Buttons to load, delete, or view details.

## 1.5 Colony Summaries Screen

Lists all colonies with their achievement progress. Achievements are divided into locked and unlocked categories with descriptions and completion criteria.

---

# Phase 2: New Game Setup

## 2.1 Game Mode Selection

The first step in creating a new game. Two primary modes presented as large selectable cards.

- **Survival**: Full gameplay with all challenges active. The standard experience.
- **No Sweat**: Simplified mode. Duplicants cannot die from starvation, suffocation, or disease. Stress effects are disabled.

> **Note:** These are presets for more granular settings available in the next step.

## 2.2 Asteroid and World Selection

The second setup screen. The player selects which asteroid cluster to start on. This is the core world generation choice.

- **Asteroid cluster selection:** a list of asteroid types, each with a name, description, and difficulty indicator. Examples include Terra, Oceania, Rime, Verdante, Arboria, Volcanea, The Badlands, Aridio, and Oasisse. Spaced Out DLC adds multi-asteroid cluster options.
- **World seed input:** a numeric field that determines the random generation of the world
- **World Traits display:** after selecting a seed, the randomly assigned traits for that world are shown (such as Frozen Core, Magma Channels, Metal Rich, etc.)
- **Game Settings button:** opens the detailed settings panel

## 2.3 Game Settings Panel

A detailed settings screen allowing granular control over difficulty parameters. Each setting is a dropdown or slider with multiple named difficulty levels.

- **Disease:** Total Immunity, Germ Resistant, Default, Germ Susceptible, Outbreak Prone
- **Hunger:** Tummyless, Fasting, Default, Rumbly Tummies, Ravenous
- **Morale:** Totally Blasé, Default, A Bit Persnickety, Draconian
- **Stress:** Chill, Default, Stressful, Meltdown
- **Suit Durability:** Indestructible, Default, Threadbare, Paper Thin
- **Radiation (Spaced Out DLC):** various levels from none to extreme
- **Meteor Showers:** various intensity levels
- **Sandbox Mode toggle:** can be enabled at game start, unlocking sandbox tools

## 2.4 Duplicant Selection Screen

The final pre-game screen. Three duplicant slots are presented. For each slot, a randomly generated duplicant is shown with their stats, traits, and interests. The player can reroll any individual duplicant indefinitely.

- Colony name text input field
- Three duplicant selection panels, each containing:
  - Duplicant name (editable text field)
  - Duplicant portrait (animated character preview)
  - Interest filter dropdown: locks a guaranteed interest category for rerolls
  - Bionic filter (Bionic Booster DLC): filters for bionic duplicants
  - Attributes list: numeric values for Athletics, Cooking, Creativity, Digging, Machinery, Medicine, Science, Strength, Husbandry, Tinkering
  - Interests: one to three interests shown with bonus amounts
  - Positive traits (green): such as Quick Learner, Grease Monkey, Mole Hands, etc.
  - Negative traits (red): such as Mouth Breather, Destructive, Ugly Crier, Narcoleptic, etc.
  - Stress response type: Destructive, Ugly Crier, Binge Eater, Vomiter
  - Overjoyed response type: Super Productive, Sticker Bomber, Balloon Artist, Sparkle Streaker
- Reroll button per duplicant
- Embark button to start the game

> **Note:** The interest filter per slot is a dropdown containing categories like Mining, Building, Research, Cooking, Doctoring, Farming, Ranching, Art, Operating, Rocketry, Suit Wearing, and others. Setting it guarantees the rerolled duplicant will have that interest.

---

# Phase 3: Core Gameplay HUD

Once the game starts, the player is presented with the main gameplay interface. This is a 2D side-view of the asteroid interior. The HUD is arranged around the edges of the screen.

## 3.1 Top Bar

### 3.1.1 Management Tabs (Top Left)

A row of icon buttons that open management screens. These are the primary colony management panels.

- **Vitals**: duplicant health, calories, stress, morale, bladder, breath, stamina, decor exposure, disease status
- **Consumables**: per-duplicant food permissions. A grid of duplicant rows vs food type columns with checkboxes. Also includes medicine assignments.
- **Schedule**: per-duplicant daily schedules. A 24-segment timeline (one per in-game hour) that can be set to Work, Sleep, Bathtime, or Downtime. Multiple schedule groups can be created and duplicants assigned to them.
- **Priorities**: per-duplicant task priority matrix. A grid of duplicant rows vs errand category columns. Each cell can be toggled through priority levels (disabled, low, default, high, very high). Categories include: Life Support, Farming, Ranching, Cooking, Combat, Art, Research, Rocketry, Suit Wearing, Operating, Medicine, Supplying, Tidying, Hauling, Building, Digging, Toggle, Storage.
- **Reports**: daily colony reports. Each cycle gets a report showing statistics such as calories produced/consumed, stress changes, health events, power generation/consumption, and travel time. Reports can be scrolled through by cycle.
- **Research**: full-screen research tree. See section 5.1 for details.
- **Skills**: full-screen duplicant skills assignment. See section 5.2 for details.
- **Starmap**: full-screen space map. See section 8 for details.

### 3.1.2 Overlay Buttons (Top Left, beside management tabs)

A row of icon buttons that toggle visual overlays on the game world. Each overlay changes what information is displayed on the map. The overlays are listed here in their toolbar order. Some only appear after relevant research is completed.

1. **Oxygen Overlay (F1)**: shows breathable gas density per cell. Color-coded from red (vacuum/unbreathable) to blue (high oxygen).
2. **Power Overlay (F2)**: shows electrical wiring, generators, consumers, batteries, and their connection states. Displays wattage and circuit load information.
3. **Temperature Overlay (F3)**: color-codes every cell by temperature. Gradient from blue (cold) through green (comfortable) to red (hot).
4. **Materials Overlay (F4)**: color-codes cells by element type, showing what material each cell is made of.
5. **Light Overlay (F5)**: shows illumination levels per cell. Relevant for duplicant morale and certain plant growth requirements.
6. **Liquid Plumbing Overlay (F6)**: shows liquid pipe networks, pumps, valves, vents, bridges, and flow direction. Displays pipe contents and throughput.
7. **Gas Plumbing Overlay (F7)**: shows gas pipe networks, pumps, valves, vents, bridges, and flow direction. Displays pipe contents and throughput.
8. **Decor Overlay (F8)**: shows decor values per cell. Positive decor shown in green, negative in red. Decor affects duplicant morale.
9. **Germ Overlay (F9)**: shows germ contamination levels and types across cells, pipes, and objects. Types include Food Poisoning, Slimelung, Zombie Spores, and Radiation (Spaced Out).
10. **Farming Overlay (F10)**: shows plant growth status, planter conditions, and irrigation/fertilization states.
11. **Room Overlay (F11)**: shows recognized rooms, their boundaries, types, and requirements. Lists what each Miscellaneous room could become if conditions are met.
12. **Exosuit Overlay (Shift+F1)**: shows checkpoint docks, suit status, and suit assignment. Appears after Hazard Protection research.
13. **Automation Overlay (Shift+F2)**: shows automation wire networks, logic gates, sensors, and their signal states (green/red). See section 7 for details.
14. **Conveyor Overlay (Shift+F3)**: shows conveyor rail networks, loaders, receptacles, and items in transit.
15. **Radiation Overlay (Shift+F4, Spaced Out DLC)**: shows radiation levels per cell in rads/cycle.

### 3.1.3 Resource Panel (Top Right)

A summary bar showing current stockpiles of key resources. Clicking expands to a detailed resources list.

- **Collapsed view:** shows counts of key categories (kcal of food, oxygen generation, power status, etc.)
- **Expanded view:** a categorized, scrollable list of every resource in the colony with quantities. Categories include Edible, Agriculture, Filtration, Consumable Ore, Metal Ore, Refined Metal, Minerals, Industrial Gases, Liquids, and others.
- Each resource entry shows name, total mass, and can be clicked to highlight where that resource exists in the world.

### 3.1.4 Time Controls and Cycle Display (Top Center-Right)

- Current cycle number (day counter)
- Time of day indicator (visual segment showing position in the current cycle)
- Pause button (Space bar)
- Speed controls: 1x, 2x, 3x speed (Tab to cycle, numpad +/- to adjust)

## 3.2 Bottom Bar

### 3.2.1 Build Menu (Bottom Left)

The primary construction interface. A row of category buttons, each opening a submenu of buildings. Buildings are arranged in a scrollable submenu panel within each category.

Building categories in order:

1. **Base**: Tiles, Doors (Manual, Pneumatic, Mechanized Airlock, Bunker Door), Ladders, Fire Pole, Plastic Ladder, Transit Tube, Storage Bin, Smart Storage Bin, etc.
2. **Oxygen**: Algae Terrarium, Algae Deoxidizer, Electrolyzer, Rust Deoxidizer, Sublimation Station, Oxygen Diffuser, etc.
3. **Power**: Manual Generator, Coal Generator, Hydrogen Generator, Natural Gas Generator, Petroleum Generator, Solar Panel, Steam Turbine; Wire, Heavi-Watt Wire, Wire Bridges; Batteries, Smart Battery; Power Transformer, Large Power Transformer.
4. **Food**: Microbe Musher, Electric Grill, Gas Range; Planter Box, Farm Tile, Hydroponic Farm; Ration Box, Refrigerator; Critter Feeder, Fish Feeder.
5. **Plumbing**: Outhouse, Lavatory, Sink, Wash Basin, Shower; Liquid Pipe, Liquid Vent, Liquid Bridge, Liquid Valve, Liquid Filter, Liquid Pump; Bottle Emptier, Pitcher Pump.
6. **Ventilation**: Gas Pipe, Gas Vent, Gas Bridge, Gas Valve, Gas Filter, Gas Pump, Gas Bottler, Canister Emptier, High Pressure Gas Vent.
7. **Refinement**: Compost, Water Sieve, Desalinator, Algae Distiller, Fertilizer Synthesizer, Ethanol Distiller, Metal Refinery, Glass Forge, Kiln, Rock Crusher, Molecular Forge, Supermaterial Refinery.
8. **Medicine**: Apothecary, Disease Clinic, Massage Table, Hand Sanitizer, Ore Scrubber, Sick Bay, Germ Sensor.
9. **Furniture**: Cot, Comfy Bed, Mess Table, Decor items (Paintings, Sculptures, Flower Pots, various monument pieces), Lighting (Ceiling Light, Sun Lamp), Recreation buildings (Water Cooler, Arcade Cabinet, Juicer, Espresso Machine, Hot Tub, Mechanical Surfboard, Sauna, Beach Chair).
10. **Stations**: Research Station, Super Computer, Virtual Planetarium, Skill Scrubber, Telescope, Space Cadet Centrifuge, Power Control Station, Farm Station, Ranching Station, Grooming Station, Shearing Station.
11. **Utilities**: various special-purpose buildings
12. **Automation**: Automation Wire, Automation Wire Bridge, Logic Gates (AND, OR, XOR, NOT, Buffer, Filter, Memory Toggle, Signal Counter), Sensors (Pressure, Temperature, Germ, Critter, Duplicant Motion, Clock, Atmo Suit, Weight Plate, etc.), Ribbon Wire and Ribbon Reader/Writer.
13. **Conveyor**: Conveyor Rail, Conveyor Bridge, Conveyor Loader, Conveyor Receptacle, Auto-Sweeper, Conveyor Rail Shutoff, Conveyor Chute.
14. **Rocketry**: Rocket Platform, Command Capsule, various engine types, fuel tanks, cargo bays, modules. See section 8.
15. **Radiation (Spaced Out DLC)**: Radbolt Generator, Radbolt Joint Plate, Research Reactor, Manual Radbolt Generator, etc.

When selecting a building to place:

- Building info tooltip: name, description, size in tiles, resource requirements, power consumption/generation, input/output specifications
- Material selection dropdown: choose which material variant to construct from
- Rotation button (O key): rotates building orientation
- Placement ghost on the grid: shows where the building will be placed, turning red if placement is invalid

### 3.2.2 Tool Commands (Bottom Right)

A row of tool buttons for issuing orders to duplicants and interacting with the world.

- **Dig (G)**: marks tiles for excavation
- **Cancel (C)**: cancels pending orders in a selected area
- **Deconstruct (X)**: marks buildings for deconstruction
- **Prioritize (P)**: opens priority brush to set sub-priority (1-9 or yellow alert) on objects
- **Disinfect**: marks buildings/tiles for germ removal
- **Mop (M)**: marks liquid spills for cleanup
- **Sweep**: marks debris for pickup and storage
- **Capture**: marks critters for wrangling
- **Harvest**: marks plants for immediate harvest or toggles auto-harvest
- **Attack**: marks critters for combat
- **Empty Pipe**: marks pipe segments for draining
- **Copy Building (B)**: copies a placed building to re-place the same type
- **Toggle**: enables/disables buildings
- **Relocate**: marks movable objects for relocation

### 3.2.3 Alert Controls (Bottom Center)

- **Red Alert**: forces all duplicants to work continuously, ignoring schedules and personal needs
- **Yellow Alert**: can be placed on individual buildings/tasks to mark them as urgent

## 3.3 Notification System (Left Side)

Notifications appear as a scrollable stack on the left side of the screen. They are categorized by severity.

- **Red notifications**: critical emergencies (suffocation, starvation, flooding, extreme temperatures)
- **Yellow notifications**: warnings (low resources, unassigned buildings, blocked pipes)
- **Green/Blue notifications**: informational (research complete, new duplicant available, colony achievements)
- Each notification can be clicked to zoom the camera to the relevant location.
- Printing Pod ready notification: appears every 3 cycles when a new duplicant or care package can be chosen.

> **Note:** Notifications are transient and stack. Old ones scroll up and eventually disappear. There is no notification history log outside of the Daily Reports screen.

## 3.4 The Game World Grid

The core play area is a 2D tile grid representing the asteroid cross-section. Every cell has properties the player can inspect.

- Camera panning: WASD keys or middle-mouse-drag
- Camera zoom: mouse scroll wheel
- Cell hover tooltip: shows element type, mass, temperature, and germ count for the cell under the cursor
- Left-click on empty cell: shows detailed cell info panel (element, state, mass, temperature, germs, hardness)
- Left-click on building: opens building details panel (see section 4)
- Left-click on duplicant: opens duplicant details panel (see section 4)
- Left-click on critter: opens critter details panel (see section 4)
- Left-click on plant: opens plant details panel (see section 4)
- Left-click on debris: shows item info (element, mass, temperature)
- Right-click: deselects current selection/tool

---

# Phase 4: Detail and Side Panels

When any entity in the game world is clicked, a detail panel opens on the right side of the screen. These panels have multiple tabs and contextual side screens.

## 4.1 Building Detail Panel

Appears when any placed building is selected. Contains tabbed information.

### 4.1.1 Status Tab

- Building name and current status (idle, working, waiting, entombed, overheated, flooded, broken, etc.)
- Status items: list of current conditions affecting the building
- Operational toggle (enable/disable)
- Priority sub-priority setting (1 through 9, or yellow alert)

### 4.1.2 Information Tab

- Building description
- Effects: what the building does (outputs, inputs, power draw, etc.)
- Requirements: room requirements, skill requirements, temperature limits
- Construction properties: size, materials used, mass

### 4.1.3 Contents Tab (where applicable)

- For storage buildings: list of stored items with mass and temperature
- For pipe-connected buildings: pipe input/output contents
- For generators: fuel level and output wattage

### 4.1.4 Errands Tab

- List of pending errands for this building
- Which duplicant is assigned to each errand
- Errand priority level

### 4.1.5 Config Tab

- Equipment and assignables for the building
- Material and blueprint options (reconstruct with different material, change cosmetic skin)

### 4.1.6 Contextual Side Screens

Many buildings have specialized side screens that appear alongside the detail panel. Examples:

- **Fabrication buildings** (Electric Grill, Kiln, etc.): recipe queue with a list of available recipes, queue management, and continuous/limited production toggle
- **Storage buildings**: filter checklist of which resource types to accept
- **Doors**: access permission toggle (open, locked, auto)
- **Planter buildings**: plant selection dropdown showing available seeds and growth requirements
- **Critter Drop-Off/Pick-Up**: species filter and critter count settings
- **Sensors**: threshold sliders and above/below toggle
- **Logic gates**: timing settings for Buffer and Filter gates
- **Valves**: flow rate slider
- **Smart Battery**: activation/deactivation threshold sliders
- **Incubators**: egg selection and continuous lullaby toggle
- **Telescopes**: destination analysis progress
- **Painting/Sculpture buildings**: art style preview and reassignment

## 4.2 Duplicant Detail Panel

Appears when a duplicant is selected. Multiple tabs with comprehensive duplicant information.

### 4.2.1 Status Tab

- Name, current activity, location
- Current status effects (wet, scalding, sopping wet, popped eardrums, etc.)
- Current errand and path

### 4.2.2 Bio Tab

- All attribute values (Athletics, Cooking, Creativity, etc.) with current modifiers
- Traits (positive and negative)
- Interests
- Stress response and overjoyed response types
- Assigned skills (from Skills screen)

### 4.2.3 Needs/Morale Tab

- Morale requirement vs current morale
- Breakdown of all morale modifiers: food quality, decor, room bonuses, recreation, skills penalty
- Stress level and stress change rate
- Calorie intake and metabolism

### 4.2.4 Health Tab

- Hit points
- Immunity level
- Active diseases with progression
- Germ exposure status
- Radiation exposure (Spaced Out DLC)

### 4.2.5 Errands Tab

- Current errand
- Queue of upcoming errands
- Errand priority resolution details

### 4.2.6 Equipment Tab

- Currently worn equipment (Atmo Suit, Jet Suit, Lead Suit, Oxygen Mask)
- Assigned bed, mess table, toilet, locker

## 4.3 Critter Detail Panel

- Species name and variant (morph)
- Age and lifecycle state (baby, adult, old)
- Happiness/wildness level
- Diet information
- Egg production progress
- Current status effects (confined, overcrowded, hungry, etc.)
- Reproduction and incubation status

## 4.4 Plant Detail Panel

- Species name
- Growth stage and progress
- Required conditions (temperature range, pressure, light, irrigation, fertilization)
- Current condition satisfaction (which requirements are met/unmet)
- Harvest readiness
- Disease/wilt status

## 4.5 Geyser/Vent Detail Panel

- Type (Steam Geyser, Natural Gas Geyser, etc.)
- Analysis progress (requires field research)
- Once analyzed: output element, average output rate, active period, dormant period, eruption cycle

## 4.6 Tile/Cell Detail Panel

- Element name and state (solid, liquid, gas)
- Mass (kg)
- Temperature
- Hardness (for solid tiles, determines dig time)
- Germ type and count
- Thermal conductivity

---

# Phase 5: Full-Screen Management Screens

## 5.1 Research Tree

A large, scrollable, pannable tree diagram of all technologies. Opens as a full-screen overlay. The tree has approximately 10 tiers of depth, read left to right.

- Each node shows: technology name, icon, research point costs (novice, advanced, interstellar/applied/data), prerequisite connections, and whether it is completed, in progress, or locked
- Clicking a node sets it as the active research target
- Completed nodes are visually distinct from incomplete ones
- Research categories visible as branches: Food/Agriculture, Power, Colony Development, Medicine, Gases, Liquids, Suits, Decor, Computing, Rocketry, Radiation (DLC)
- A search/filter bar at the top allows text search and category filtering

> **Note:** The tree is navigated by dragging to pan and scrolling to zoom, making it inherently visual and spatial. This is one of the most complex screens to make accessible.

## 5.2 Skills Screen

Full-screen duplicant skill assignment. Shows each duplicant and available skill trees.

- Left column: list of all duplicants
- Center: skill tree for the selected duplicant, organized as a branching tree with tiers
- Each skill node shows: name, attribute bonus, prerequisites, mastery progress
- Skills include: Improved Digging, Hard Digging, Super-Hard Digging, Advanced Research, Field Research, Astronomy, Advanced Cooking, Grilling, Skilled Building, Mechatronics Engineering, Electrical Engineering, Medicine Compounding, Artistry, Husbandry, Critter Ranching, Rocket Piloting, Suit Durability, and many more
- Skill points are earned passively via duplicant experience
- Hat/role indicator changes based on assigned skills

## 5.3 Codex/Database

An in-game encyclopedia accessible from the top bar. A categorized reference of all game content.

- Searchable by name
- Categories: Buildings, Food, Critters, Plants, Germs, Geysers, Elements, Equipment, Artifacts, Biomes, Status Effects, Colony Initiatives
- Each entry contains: description, properties, recipes, input/output ratios, temperature ranges, and other relevant stats
- Some entries are unlocked progressively through gameplay (inspecting objects)

---

# Phase 6: Printing Pod (Recurring Event)

Every 3 cycles, the Printing Pod becomes ready and triggers a notification. When interacted with, it opens a selection screen.

- Three duplicant options OR care packages are displayed
- Each duplicant shows the same information as the initial selection screen (attributes, traits, interests, stress/overjoyed responses)
- Care packages offer resources (seeds, food, materials, critter eggs) as alternatives to new duplicants
- The player can choose one option or reject all
- Unlike the initial selection, these cannot be rerolled

---

# Phase 7: Automation System

Unlocked via research. Uses a dedicated overlay (Shift+F2) and its own layer of wire networks.

## 7.1 Automation Overlay

- Shows all automation wires, gates, sensors, and building ports
- Wire color indicates signal: green (active/on) or red (inactive/off)
- Building ports are visible with arrows indicating input vs output
- Ribbon wires show 4-bit state

## 7.2 Sensors

Each sensor has a configurable threshold and an above/below toggle. Sensor types include:

- Atmo Pressure Sensor, Hydro Pressure Sensor
- Thermo Sensor (gas/liquid/pipe variants)
- Germ Sensor (gas/liquid/pipe variants)
- Element Sensor (gas/liquid/pipe variants)
- Clock Sensor (time-of-day percentage)
- Duplicant Motion Sensor, Critter Sensor
- Weight Plate
- Smart Battery (built-in output port)
- Various storage buildings (built-in output port for content level)

## 7.3 Logic Gates

- AND, OR, XOR, NOT: standard boolean logic
- Buffer Gate: delays signal turn-off by configurable time
- Filter Gate: delays signal turn-on by configurable time
- Memory Toggle (RS latch): set/reset memory cell
- Signal Counter: counts pulses up to a configurable target
- Ribbon Reader/Writer: interface between 1-bit wire and 4-bit ribbon

---

# Phase 8: Rocketry and Starmap

## 8.1 Starmap (Base Game)

Full-screen interface showing space destinations arranged by distance from the home asteroid.

- Destinations arranged in concentric rings by distance (10,000 km increments)
- Each destination has: name, type, analysis status, available resources, research targets, and artifact chances
- Destinations must be analyzed via Telescope before they can be visited
- Right panel shows selected destination details and mission controls
- Rocket mission configuration: select crew, cargo, research targets

## 8.2 Starmap (Spaced Out DLC)

Completely redesigned. A hex-grid map representing nearby space. Significantly more complex.

- Hex tiles that are revealed through telescope observation and rocket exploration
- Multiple planetoids visible, each a separate playable colony
- Space POIs (Points of Interest) that can be harvested for resources
- Rocket paths shown between hexes
- Planetoid switching: clicking a planetoid switches the main view to that colony
- Resource transfer between planetoids via rockets

## 8.3 Rocket Interior (Spaced Out DLC)

In Spaced Out, rockets have modular interiors that function as tiny bases.

- Interior view: a small, enclosed tile grid that the player can build inside
- Module side screens: each rocket module has configuration options
- Launch/landing controls on the rocket platform
- Rocket status: fuel level, oxidizer level, cargo manifest, crew list
- Orbital controls: commands for harvesting space POIs, deploying rovers

## 8.4 Multi-World Management (Spaced Out DLC)

- World selector: a panel allowing switching between discovered planetoids
- Each planetoid has its own independent tile grid, duplicants, buildings, and resources
- A small asteroid header bar shows which world is currently viewed
- Resource status summaries per world

---

# Phase 9: Sandbox Mode

Sandbox mode can be toggled with Shift+S once unlocked. It adds a new toolbar of special creative tools.

- **Spawn Entity**: spawns duplicants, critters, plants, eggs, and other entities
- **Spawn Element**: paints solid, liquid, or gas elements at a chosen temperature and mass
- **Heat**: directly modifies cell temperature
- **Destroy**: instantly removes cells, buildings, and entities
- **Fill**: fills an area with a chosen element
- **Clear Floor**: removes debris from an area
- **Reveal**: reveals fog of war
- **Place Building**: instant construction without resources
- All research is unlocked while sandbox is active
- Each sandbox tool has parameter panels (element selector, temperature input, mass slider, etc.)

---

# Phase 10: Building Placement Interaction Model

One of the most fundamental interaction patterns in the game. When a building is selected from the build menu:

- The cursor becomes a placement ghost showing the building footprint on the grid
- Valid placement locations are indicated (ghost turns green/white)
- Invalid placement is indicated (ghost turns red) with tooltip explaining why (no foundation, overlapping, wrong element, etc.)
- Left-click or click-drag to place the building order
- Right-click or Escape to cancel placement mode
- O key rotates the building (for asymmetric buildings)
- F key flips the building horizontally (for some buildings)
- Material selector dropdown in the build menu footer changes construction material
- For pipe and wire placement: click-drag draws connected segments along the grid
- For area tools (dig, mop, etc.): click-drag defines a rectangular selection area

---

# Phase 11: Room System

Rooms are created by fully enclosing an area with tiles and doors. The Room Overlay (F11) shows recognized rooms.

- A room must be between 12 and 120 tiles in size (depending on type)
- Each room type has specific building requirements. Examples:
  - Barracks: room with at least one Cot, no industrial machinery, 12-64 tiles
  - Bedroom: room with at least one Comfy Bed, no industrial machinery, 12-64 tiles
  - Latrine: room with at least one Outhouse and a Wash Basin, 12-64 tiles
  - Washroom: room with at least one Lavatory and a Sink, 12-64 tiles
  - Great Hall: room with a Mess Table and a Decor item, at least 32 tiles
  - Kitchen: room with a cooking station and Refrigerator
  - Hospital: room with a Disease Clinic and Medical Cot
  - Recreation Room: room with recreational building, 12-64 tiles
  - Nature Reserve: room with no buildings except lighting and doors, with plants
  - Park: simpler version of Nature Reserve
  - Laboratory: room with Research Station, 12-64 tiles
  - Stable: room with a Critter Drop-Off, 12-96 tiles
  - And many more specialized types
- Room bonuses provide morale boosts and other effects to duplicants
- Room overlay shows which requirements are met/unmet for each enclosed space

---

# Phase 12: In-Game Pause Menu

Accessed by pressing Escape during gameplay.

- Resume
- Save (shows "Already Saved" if recently saved)
- Save As
- Load
- Options (same options as main menu)
- Controls
- Colony Summaries
- Retire Colony
- Quit to Main Menu (with Save and Quit confirmation dialog)
- Quit to Desktop

---

# Phase 13: Bionic Booster Pack DLC Additions

This DLC adds bionic duplicants with different needs and UI elements.

- Bionic filter on duplicant selection screen (new game and printing pod)
- Bionic duplicant detail panel additions: battery charge level, gear oil level, internal oxygen tank, power bank inventory
- New buildings: Biobot Builder, Power Bank Fabricator, Gunk Extractor, Remote Worker terminal buildings
- Power bank management: bionic duplicants consume rechargeable power banks instead of food
- Remote Worker interface: allows duplicants to operate buildings on other parts of the asteroid remotely

---

# Phase 14: Story Traits and Special Events

- **World Story Traits**: selected during world generation. These add special narrative events and buildings to the world, such as the Mysterious Hermit, Crashed Satellite, or others. Each triggers unique popup notifications and special buildings that can be interacted with.
- **Meteor Showers**: periodic events with warning notifications. Require Bunker Doors and Tiles for protection. The notification system shows incoming shower warnings.
- **Colony Achievements/Initiatives**: tracked in Colony Summaries. Include goals like reaching certain cycle counts, launching rockets, taming critters, and producing specific foods.
- **Seasonal Events**: occasional special themed events by Klei
- **Victory Conditions**: the game has several soft victory conditions such as launching a rocket to the Temporal Tear or the Great Escape achievement

---

# Phase 15: Summary of All Interactive Elements

The following is a consolidated inventory of every major UI component that must be made accessible, grouped by interaction type.

## 15.1 Full-Screen Modal Screens

- Main Menu
- Options (Graphics, Audio, Game, Controls, Accounts)
- Load Game
- Colony Summaries
- Mod Manager
- New Game Flow (Mode Select, Asteroid Select, Game Settings, Duplicant Selection)
- Research Tree
- Skills Screen
- Starmap (Base Game and Spaced Out variants)
- Codex/Database
- Pause Menu
- Printing Pod Selection Dialog

## 15.2 HUD Panels (Always Visible or Toggle-Visible)

- Top Bar: Management Tabs, Overlay Buttons, Resource Panel, Time/Cycle Display
- Bottom Bar: Build Menu Categories and Submenus, Tool Commands, Alert Controls
- Left Side: Notification Stack
- Right Side: Entity Detail Panel (context-dependent)

## 15.3 Overlay Modes (15 total)

- Oxygen, Power, Temperature, Materials, Light, Liquid Plumbing, Gas Plumbing, Decor, Germ, Farming, Room, Exosuit, Automation, Conveyor, Radiation

## 15.4 Detail Panel Variants

- Building details (with contextual side screens per building type)
- Duplicant details (Status, Bio, Needs, Health, Errands, Equipment)
- Critter details
- Plant details
- Geyser/Vent details
- Tile/Cell details
- Debris details

## 15.5 Management Grids

- Vitals (duplicant rows with stat columns)
- Consumables (duplicant rows vs food/medicine columns with checkboxes)
- Priorities (duplicant rows vs errand category columns with priority toggles)
- Schedule (duplicant assignments vs 24-hour time blocks)
- Reports (cycle-by-cycle stats with expandable categories)

## 15.6 Spatial Interactions

- World grid navigation (pan, zoom)
- Cell inspection (hover and click)
- Building placement (ghost positioning, rotation, material selection)
- Area selection tools (drag rectangles for dig, mop, sweep, cancel, etc.)
- Pipe/Wire drawing (connected segment placement along grid)
- Sandbox tools (element painting, entity spawning, etc.)

## 15.7 Recurring Popups and Dialogs

- Printing Pod selection (every 3 cycles)
- Notification click-to-zoom
- Confirmation dialogs (quit, retire colony, overwrite save)
- Tutorial/Guidance popups (can be disabled in options)

---

*End of Document*
