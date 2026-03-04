# OniAccess Guide

OniAccess makes Oxygen Not Included playable for blind users by providing speech output for all game screens, navigation, and interactions.

## Getting Started

**Ctrl+Shift+F12** toggles the mod on and off.

**F12** opens context help on any screen. It lists every available key for whatever you're currently looking at. Use it often, especially when you encounter a new screen.

Speech is interrupt-based. When you navigate quickly, each new announcement cuts off the previous one. This is intentional -- it lets you skim through lists and menus at your own pace without waiting for each item to finish speaking.

## Main Menu

The mod speaks the focused item automatically as you move through the menu.

- **Tab / Shift+Tab** cycles between three sections: Buttons, DLC, News
- **Up / Down** navigates within a section
- **Enter** activates the focused item
- **A-Z** type-ahead search works within each section

Select **New Game** to begin.

## Colony Setup

You go through three screens in sequence:

1. **Game mode** -- choose "No Sweat" for your first game. It gives wider margins on food, oxygen, and stress.
2. **Cluster category** -- pick the default.
3. **Destination** -- choose Terra. It has the most forgiving starting biome.

On each screen, **Up/Down** navigates options and **Enter** selects. The mod reads each option's name and description.

## Choosing Your Duplicants

You start with three duplicants ("dupes"). This screen lets you customize your starting crew.

- **Tab / Shift+Tab** switches between the 3 dupe slots
- Each dupe announces: name, interests, traits (with effects), expectations, and attributes
- **Left / Right** on the interest filter dropdown cycles available filters
- **Reroll** button shuffles the dupe and announces the new name and interests
- Colony name can be edited (Enter on the field)

Brief gameplay advice: avoid negative traits like Flatulent or Narcoleptic. Interests in Digging, Research, and Cooking are useful early. Don't spend too long optimizing -- any reasonable crew can survive.

Press **Embark** to start your colony.

## First Look at Your Colony

You land at the Printing Pod in the center of the starting biome. The game starts paused.

### Tile Cursor

**Arrow keys** move the tile cursor one cell at a time. Each move announces a "cell glance": the element (floor material, gas, liquid), any building, any entity (duplicant or critter), pending orders, and debris.

### Quick Reads

- **H** -- jump back to the Printing Pod
- **Q** -- read cycle status (current cycle number and schedule block)
- **S** -- read colony status (dupe count, rations, stress level)

### Game Speed

**BackQuote** (the key left of 1) cycles speed: paused, slow, medium, fast. The mod announces pause/unpause and speed changes automatically.

## Giving Your First Orders

### The Action Menu

**Tab** opens the action/build menu. It has a 3-level hierarchy:

- Level 0: top categories (Tools section plus building categories)
- Level 1: subcategories
- Level 2: individual buildings or tools

Navigate with **Up/Down**. **Enter** or **Right** drills in. **Left** goes back. **Escape** closes the menu.

**A-Z** type-ahead searches all buildings regardless of your current position in the hierarchy. This is the fastest way to find something.

### First Builds

Your immediate priorities:

1. **Outhouse** -- type "out" to find it, Enter to select, place with arrow keys and Enter. Dupes need a toilet or stress rises fast.
2. **Cots** -- one per dupe. Sleeping on the floor increases stress.
3. **Manual Generator** and **Battery** -- provides power for machines.
4. **Research Station** -- unlocks new buildings. Assign a dupe with Research interest for faster progress.

## Using Tools

Select tools from the action menu (Tab, then the Tools section) -- for example, Dig.

The mod announces: tool name, current priority, and active filter.

### Placing Orders

- **Arrow keys** move the cursor while the tool is active
- **Single-cell order**: press Enter on one tile
- **Rectangle selection**: Space sets the first corner, move to the opposite corner, Space again sets the second corner (announces dimensions and valid target count). Enter confirms all rectangles, and the mod announces how many targets will be affected
- **0-9** sets priority (0 is emergency/highest, 9 is lowest)
- **F** opens the filter menu for tools that support it
- **Escape** cancels the tool

Dig out space around the Printing Pod for your initial base. Stay within the starting biome for now -- don't tunnel into unknown territory yet.

## Getting Around Faster

### Skip Navigation

**Ctrl+Arrow** jumps to the next cell where something changes -- a different element, building edge, or entity. Announces how many tiles were skipped. Much faster than stepping one tile at a time.

### Alignment Ruler

**Ctrl+B** places a crosshair at the cursor. As you move, proximity audio cues tell you when you're approaching the ruler line. Useful for aligning builds. **Ctrl+Shift+B** clears it.

### Bookmarks

- **Ctrl+1 through Ctrl+0** saves a camera bookmark at the current position
- **Shift+1 through Shift+0** jumps to a saved bookmark
- **Alt+1 through Alt+0** reads direction and distance to a bookmark without moving

### Coordinate Mode

**Shift+K** cycles between Off, Append, and Prepend. When active, tile coordinates are added to every cell glance.

## The Scanner

The scanner is a hierarchical search system that finds everything on the map.

- **Ctrl+PageUp / Ctrl+PageDown** cycles top-level category (Solids, Liquids, Gases, Buildings, Networks, Automation, Debris, Zones, Life)
- **Shift+PageUp / Shift+PageDown** cycles subcategory
- **PageUp / PageDown** cycles specific item within subcategory
- **Alt+PageUp / Alt+PageDown** cycles instances of that item

Each announcement includes: name, direction and distance from cursor, and instance count.

- **Home** teleports the cursor to the current instance
- **Shift+Home** toggles auto-move (cursor teleports automatically as you cycle)
- **Ctrl+F** text search within the current category
- **End** refreshes entries for the current item

Stale entries (demolished buildings, dead creatures) are cleaned up automatically.

The scanner is the fastest way to find specific resources, buildings, or creatures anywhere on the map.

## Managing Food

Duplicants eat about 1000 kcal per cycle. Starting rations last roughly 6 cycles.

- Build a **Microbe Musher** (needs power and water) for Mush Bars as emergency food
- Research **Basic Farming** to unlock Planter Boxes -- grow Mealwood, roughly 5 plants per dupe
- Build a **Wash Basin** at the entrance to your food prep area to prevent food poisoning
- Later: research Meal Preparation for the Electric Grill and better food

## Managing Oxygen

Starting oxygen pockets last the first few cycles.

- Build an **Oxygen Diffuser** (needs Algae and power) as your first oxygen source
- Algae is finite -- eventually transition to an **Electrolyzer** (converts water to oxygen and hydrogen)
- Use the **Temperature Overlay** and **Gas Overlay** to monitor. The mod prepends overlay-specific data to cell glances (temperature values, gas types, etc.)

## Inspecting Things

**Enter** on a tile with an entity opens the inspection panel.

- The mod announces the entity name on open
- **Tab / Shift+Tab** cycles through tabs (Status, Properties, Config, Errands, Actions, etc.)
- **Up / Down** navigates items within a tab. Items may be nested -- **Right** to drill in, **Left** to go back
- **Ctrl+Tab / Ctrl+Shift+Tab** jumps between tab sections
- The **Actions tab** is always present -- contains Enable/Disable, Demolish, and other building controls
- The **Config tab** appears for buildings with settings (sliders, toggles, dropdowns). Use **Left/Right** to adjust values
- **Escape** closes the panel

## Research and Skills

### Research (R)

Three tabs: Browse (categorized tech list), Queue (current research queue), Tree (graph navigation).

On open, the mod announces your current research point inventory.

- In **Browse**: navigate categories with Up/Down, Enter to queue a tech, Space to jump to its position in the Tree
- In **Queue**: see what's being researched, Enter to cancel an item

Recommended early research: Basic Farming, then Power Regulation, then whatever you need next.

### Skills (J)

Three tabs: Duplicants (dupe list), Skills (categorized skills for selected dupe), Tree.

Select a dupe first, then browse available skills. Enter on a skill to learn it if requirements are met.

## Priority and Colony Management

### Priority Screen (L)

A 2D grid: duplicants as rows, chore groups as columns.

- **Arrow keys** move between cells
- **0-5** sets priority for the current cell (0 is emergency)
- **Shift+0-5** sets the entire column
- **Ctrl+Left / Ctrl+Right** adjusts all priorities in the current row
- Each cell announces: priority level and skill level. Disabled cells explain why (trait restriction)

### Vitals Screen (V)

A 2D grid: duplicants as rows, vital stats as columns (Stress, Morale, Fullness, Health, Sickness).

- Each cell announces the value plus a full breakdown with modifiers
- Enter on a dupe row jumps to them and opens inspection

### Other Management Screens

- **Period (.)** -- Schedule
- **F** -- Consumables
- **E** -- Report
- **N** -- Diagnostics

## Tracking Your Duplicants

- **] and [** cycle forward and backward through all living duplicants. Each announces: name, current chore, chore target, and any critical statuses
- **\\** (backslash) on the dupe's tile selects them (opens inspection). Otherwise jumps the cursor to the dupe's location

Critical statuses checked: Incapacitated, Critical Health, Injured, Suffocating, Stressed, Scalding, Sick, Starving, Entombed, Fleeing, and more.

## Notifications

**Shift+N** opens the notification menu.

- **Up / Down** navigates notification groups
- Groups with multiple notifications show a count and expand into a submenu on Enter
- **Enter** on a single notification activates it (usually jumps to the relevant location)
- **Delete** dismisses a notification when available
- **Escape** closes the menu

Pay attention to notifications. They warn about critical issues: duplicants trapped, buildings broken, resources depleted.

## The Printing Pod

Every few cycles, the Printing Pod offers new duplicants or care packages.

- The mod announces each option. For dupes: name, interests, traits, expectations, attributes
- **Tab / Shift+Tab** cycles between options
- **Enter** on "Choose" to accept; "Reject All" opens a confirmation

Don't accept too many dupes too fast. Each one needs food, oxygen, and a bed. Grow slowly.

## Overlays

ONI has overlay modes that show specific information layers. The game's native hotkeys activate them (check F12 context help for the current bindings).

When an overlay is active, the mod prepends overlay-specific data to every cell glance:

- **Temperature** -- temperature value
- **Power** -- wattage and charge level
- **Plumbing** (liquid/gas) -- fluid type and flow direction
- **Automation** -- signal state (green/red)
- **Decor** -- decor value
- **Light** -- lux value
- **Radiation** -- radiation level
- **Disease** -- germ type and count
- **Rooms** -- room type (only announced when it changes)

Overlays are essential for diagnosing problems. Use Temperature to find heat leaks, Power to check wiring, Plumbing to trace pipes.

## Heat Management

The starting biome is temperate, surrounded by hotter biomes.

- Use the Temperature Overlay to scout biome boundaries before digging into them
- **Insulated Tiles** and **Airlocks** contain heat when you breach into new biomes
- Look for Abyssalite veins -- they're natural insulation between biomes. Skip navigation (Ctrl+Arrow) can quickly identify biome boundaries
- Don't break into hot biomes until you have a plan for the heat

## Tips

- Use **F12** liberally. Context help shows every available key for the current screen.
- The scanner (PageUp/Down system) is the fastest way to find anything on the map.
- **Ctrl+Arrow** skip navigation is much faster than stepping one tile at a time for exploration.
- Check colony status (**S**) regularly for an at-a-glance summary of your colony's health.
- The mod is read-only. It never changes game behavior. Any action you take goes through the game's normal systems.
