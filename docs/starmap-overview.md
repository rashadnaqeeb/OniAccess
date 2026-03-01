# Starmap Screen Overview

## What It Is

The Starmap is a full-screen management screen where players manage space missions. It opens with the Z key (Action.ManageStarmap) from the management toolbar at the top of the screen. The game has two completely separate implementations: one for the base game and one for the Spaced Out DLC. Only one version is ever active in a given save. The DLC check is `DlcManager.FeatureClusterSpaceEnabled()` (returns true when Spaced Out is active).

This document focuses on the base game version (`StarmapScreen`), with a brief comparison to the DLC version (`ClusterMapScreen`) at the end.

---

## Base Game Starmap

### The Concept

In the base game, space is modeled as a series of concentric distance rings around the player's asteroid. Each ring contains several celestial bodies (asteroids, planets, dwarfs, moons, giants). The player builds rockets, assigns them destinations, and launches missions to gather resources.

Before a destination can be visited, it must be analyzed using a Telescope building. The Starmap screen is where the player picks which destination to analyze and monitors the progress.

### Screen Layout

The screen has three main areas arranged left-center-right:

**Left Panel** -- shows either a rocket list or rocket details, depending on whether a rocket's Command Module is selected in the game world.

- **Rocket list mode**: Lists all built rockets with their name (editable), mission status, passenger count, module count, max range, storage capacity, and destination mass. Each rocket row has a Launch button (if grounded) or a progress bar (if in flight). Rocket states are: Grounded, Launching, Underway, WaitingToLand, Landing, Destroyed.

- **Rocket details mode**: Shows when a specific rocket is selected. Displays a launch checklist (all conditions like "has engine", "has fuel", "pilot boarded", etc.), range breakdown (fuel, efficiency, thrust, weight penalty), mass breakdown (dry/wet/total), fuel levels, oxidizer levels, storage capacity, and passenger list.

**Center Panel** -- the visual star map. Distance rows are stacked vertically (closest at bottom, farthest at top). Each row is a colored band at a 10,000 km increment. Destinations appear as icons within their distance row, positioned horizontally by an orbit percentage. The icons show the destination sprite if analyzed, a question mark if being analyzed, or a dimmed icon if too far to even discover. A gray overlay indicates the selected rocket's maximum range. In-flight rockets show a trajectory line and position marker.

Interaction: mouse-click a planet to select it, hover to see its label. No keyboard navigation exists for the map.

**Right Panel** -- destination details for the selected destination. Shows:
- Name (or "Destination Unknown" if not analyzed)
- Type (or "Type Unknown")
- Distance in km
- Description text
- Analysis progress bar
- Research opportunities (5 studies: upper atmosphere, lower atmosphere, magnetic field, surface, subsurface -- each can discover rare resources)
- Mass data (current, maximum, minimum, replenish rate per cycle)
- World composition (list of elements with percentages, highlighting which the rocket can carry based on installed cargo bays)
- Recoverable entities (special items like eggs)
- Artifact drop rates by tier

At the bottom of the right panel: an Analyze button (to set the telescope's analysis target) and a Launch Mission button.

### How a Mission Works

1. Player builds a rocket (Command Module + Engine + optional modules like cargo bays, fuel tanks, etc.)
2. Player opens Starmap, selects a destination that is fully analyzed
3. If a rocket's Command Module was selected in the world, the destination is assigned to that rocket
4. Player clicks Launch (all conditions must be met: engine, fuel, pilot, clear path, etc.)
5. Rocket launches and enters "Underway" state for a duration based on distance and pilot skill
6. When complete, rocket returns with resources from the destination

### Game Data

Destinations come from `SpacecraftManager.instance.destinations`. There are always 6 fixed destinations at close range, plus 15-25 randomly generated ones spanning distance tiers 1-16, plus Earth (tier 4) and a Wormhole (tier 16). Each destination is a `SpaceDestination` with a type from `SpaceDestinationTypes` (about 23 different types like MetallicAsteroid, GasGiant, TerraPlanet, etc.).

Rockets come from `SpacecraftManager.instance.GetSpacecraft()`, each a `Spacecraft` object linked to a `LaunchConditionManager` in the game world.

### Availability

The Starmap button is disabled until the player builds at least one Telescope (or is in sandbox/debug mode). The `ManagementMenu.CheckStarmap()` method controls this.

---

## DLC Starmap (ClusterMapScreen)

The Spaced Out DLC completely replaces the starmap with a hex-grid map of nearby space. Key differences:

- **Hex grid instead of distance rows**: Space is a 2D hex grid with coordinates. Cells are revealed through telescope observation.
- **Multiple entity types**: Asteroids (playable colonies), rockets, POIs (harvestable space objects), telescope targets, payloads, debris.
- **Fog of war**: Cells can be Hidden, Peeked, or Visible.
- **Not modal**: Unlike the base game version (KModalScreen), the DLC version is a regular KScreen.
- **Zoom and pan**: The map supports zooming (50-150%) and scrolling.
- **Path-based travel**: Rockets travel along hex paths. Destination selection shows the path and checks range.
- **No rocket list**: Rocket management uses the normal game selection and side screens, not a built-in list.
- **Separate registration**: Uses `clusterMapInfo` toggle and `ToggleClusterMap()` method. No cancel handler in the base game version; the DLC version has `TryHandleCancel()` for canceling destination selection.

The DLC version is architecturally quite different and would need its own handler. It will not be covered in an initial implementation.

---

## Accessibility Implications

The base game starmap is essentially a data browser with three linked panels. The visual map in the center would be replaced by list-based navigation. The key user flows are:

1. **Browse destinations** -- navigate a list of destinations sorted by distance, hearing name/type/distance/analysis state
2. **View destination details** -- hear the full breakdown of a selected destination
3. **Set analysis target** -- pick an unanalyzed destination for the telescope to study
4. **Browse rockets** -- navigate the rocket list, hearing name/status/progress
5. **View rocket details** -- hear checklist, range, fuel, cargo details
6. **Assign destination to rocket** -- select both a rocket and a destination
7. **Launch** -- confirm all conditions are met and launch

All data is available programmatically from live game objects at speech time. No caching needed.
