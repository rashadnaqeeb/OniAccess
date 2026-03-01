# Cluster Map Overview (Spaced Out DLC)

## What It Is

The Cluster Map is the DLC's replacement for the base game's starmap. It's a hex-grid view of space showing all the asteroids, rockets, space POIs (Points of Interest like gas clouds and ore fields), meteor showers, and other entities in the player's cluster. It opens with the Z key (or the Starmap button in the management toolbar) and is always available -- no telescope requirement like the base game.

## How It Looks

The screen shows a large scrollable, zoomable hex grid. Each hex cell can contain entities -- asteroids show as large animated sprites with floating names, rockets show as small rocket icons with travel paths drawn as curved lines, POIs appear as clouds or stations, and meteor showers appear with question marks if unidentified. Hex cells have three visibility states:

- **Hidden**: dark/fogged, shows nothing
- **Peeked**: slightly visible, shows silhouettes or question marks ("UNKNOWN OBJECT DETECTED!")
- **Visible**: fully revealed with all details

When zoomed in enough (above 115%), entity names float above their icons. Asteroids gently bob up and down. Selected entities get a selection marker ring. Rockets that are traveling show animated paths as curved spline lines from their current position to their destination.

The game's normal build/tool UI is hidden while the cluster map is open, replaced by this full-screen hex view. The standard details panel (right side) still works -- selecting an entity on the map opens its details panel.

## What the Player Does

### Browsing the Map

The player scrolls and zooms the hex map to survey their cluster. They can:
- **Zoom**: mouse scroll wheel (50% to 150%)
- **Pan**: drag the map
- **Click a hex**: selects entities in that hex (cycles through if multiple)
- **Double-click a hex with an asteroid**: switches to that world's view
- **Hover a hex**: shows tooltip (hidden, peeked, empty, or entity hover info)

### Selecting Entities

Clicking a hex selects entities within it. If multiple entities share a hex (common -- a rocket orbiting near a POI), clicking the same hex again cycles to the next entity. Selected entities open in the standard details panel with:

- **Asteroids**: world information
- **Rockets**: destination info, speed, range, fuel, cargo, launch controls, module details
- **POIs**: element composition, mass remaining, artifact status
- **Meteor showers**: shower composition (if identified), ETA, target

### Setting Rocket Destinations

This is the most complex interaction. The flow:

1. Player selects a rocket (or rocket-related building like a command module)
2. In the details panel, the `ClusterDestinationSideScreen` appears showing current destination
3. Player clicks "Select Destination" button
4. The cluster map opens (if not already) in **SelectDestination mode**
5. As the player hovers hexes, a preview path is drawn from the rocket to the hovered hex
6. The hovered hex shows path length, range info, or failure reasons (no launch pad, out of range, fog of war, no path)
7. Player clicks a valid hex to set the destination
8. The map may close automatically (if opened from the side screen) or return to normal mode

The player can cancel destination selection with Escape (the map handles this via `TryHandleCancel`).

### Round Trips

Rockets can be set to round-trip mode via the destination side screen. In this mode, after reaching the destination (and harvesting any POI resources), the rocket automatically returns to its origin. The path display doubles the distance for round trips.

### Launching Rockets

Rockets are launched from their physical rocket platform, not from the cluster map directly. The cluster map is for destination selection and monitoring. Launch happens through the rocket's side screens (accessible by selecting the rocket on the map or on the asteroid view).

### Fog of War and Exploration

Space starts mostly hidden. Telescopes gradually reveal cells by earning reveal points. Each cell needs a threshold of points to become fully visible. Rockets automatically reveal cells they pass through and peek cells within 2 hexes. Discovering an asteroid also reveals the immediate area around it.

The fog has three levels:
- **Hidden**: completely dark, no information
- **Peeked**: the cell is dimly visible, hidden objects show as "UNKNOWN OBJECT DETECTED!"
- **Visible**: fully revealed, all entities shown

### Identifying Meteor Showers

Meteor showers travel across the map toward asteroids. When unidentified, they show as question marks. Telescopes can identify them, revealing their composition and allowing the player to prepare. Identification is progressive (0% to 100%).

## Key Concepts

### The Hex Grid

The cluster is a hex grid with `numRings` rings (typically 9, so hexes range from ring 0 at center to ring 8 at edge). Coordinates use axial notation (r, q). The starting asteroid is usually near the center. Each hex can contain multiple entities from different layers (an asteroid with a rocket orbiting in adjacent hex, a POI on the same hex as debris, etc.).

### Entity Layers

Entities are organized into layers for visual stacking and filtering:
1. Asteroids -- the worlds the player colonizes
2. Craft -- rockets
3. POI -- harvestable resource fields and artifact locations
4. Telescope -- telescope scan targets
5. Payload -- launched payloads
6. FX -- visual effects
7. Meteor -- incoming meteor showers
8. Debris -- space junk

### Travel Mechanics

Rockets move through hexes one at a time. Speed = EnginePower / TotalBurden, modified by pilot status. Each hex costs 600 "distance units" to cross. Fuel is burned per hex crossed. Rockets can't path through asteroids (must go around them). They can't path through unrevealed fog unless the rocket has `canNavigateFogOfWar` enabled.

Range is determined by fuel remaining divided by fuel-per-hex consumption. The game shows remaining range in hex tiles, and the path preview compares path length to range.

### What Gets Shown in Details

When an entity is selected on the cluster map, the standard game `DetailsScreen` opens. For rockets, this includes:
- **Speed**: base speed with pilot modifier breakdown (dupe pilot, autopilot, robo-pilot, super-piloted, mission control boost)
- **Range**: fuel-based range with breakdown of fuel remaining, oxidizer, fuel-per-hex
- **Height**: current module height vs maximum
- **Cargo**: bay fill status
- **Destination**: current destination with change/clear buttons, landing pad selection, round-trip toggle
- **Status items**: in-flight, stranded, destination out of range, waiting to land, in orbit, etc.

For POIs:
- Title ("POINT OF INTEREST")
- Mass remaining
- Element composition with percentages
- Artifact availability

## Accessibility Considerations

### Current Input Model

All hex interaction is mouse-only:
- Click to select
- Double-click to warp to asteroid
- Hover for tooltips and path previews
- Scroll to zoom
- Drag to pan

There is no keyboard navigation between hexes.

### Key Data Sources for Speech

All data is queryable at speech time from live game objects:
- `ClusterGrid.Instance.cellContents` for all entities per hex
- `ClusterGrid.Instance.GetVisibleEntitiesAtCell(location)` for revealed entities
- `ClusterGrid.Instance.GetLocationDescription(location)` for name/type summary
- Entity properties: `Name`, `Layer`, `Location`, `IsVisible`
- Rocket properties via `Clustercraft`: status, speed, range, fuel, destination
- FOW via `ClusterGrid.Instance.GetCellRevealLevel(location)`
- Path validation via `ClusterGrid.Instance.GetPath()`
- Rocket info via `CraftModuleInterface`: all module stats, launch conditions

### Mapping to Accessible Navigation

The hex grid could be navigated with:
- **List-based approach**: enumerate all visible entities grouped by type (asteroids, rockets, POIs, meteors)
- **Spatial approach**: navigate hex-by-hex using the 6 directional neighbors (NW, NE, E, SE, SW, W)
- **Hybrid**: list of entities for quick access, spatial navigation for exploration

Key information to speak per entity type:
- **Asteroid**: name, world type, ring distance from center, discovered status
- **Rocket**: name, status (grounded/in-flight/orbiting/stranded), destination, speed, range, fuel, ETA
- **POI**: name, element composition, mass remaining, artifact status
- **Meteor shower**: identified status, composition if identified, ETA, target asteroid
- **Empty hex**: "Empty Space" or "Unexplored Space"

### Destination Selection Flow

This is the trickiest part. The SelectDestination mode is currently mouse-hover-based. An accessible version would need:
- A way to browse potential destinations
- Path validity checking (shows why a destination is invalid)
- Range comparison (path length vs rocket range)
- Confirmation to set destination

### Double-Click to Warp

The hex double-click to switch active worlds is a useful shortcut that needs an accessible equivalent.
