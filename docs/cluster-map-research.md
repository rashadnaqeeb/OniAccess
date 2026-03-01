# ClusterMapScreen Technical Research (Spaced Out DLC)

## Overview

The ClusterMapScreen is the DLC (Spaced Out / Expansion 1) replacement for the base game's StarmapScreen. It presents a hex grid of space, with asteroids, rockets, POIs, meteor showers, and other entities positioned on hex cells. Unlike the base game's distance-row model, this uses a proper 2D hex coordinate system with fog of war, pathfinding, and entity layering.

---

## 1. Class Hierarchy and Singleton

```
ClusterMapScreen : KScreen
```

- **Not modal** (KScreen, not KModalScreen). Other UI remains interactive behind it in theory, though it hides PlanScreen, ToolMenu, and OverlayScreen.
- Singleton: `ClusterMapScreen.Instance` (set in `OnPrefabInit`)
- Sort key: 20 normally, 50 when editing (e.g., renaming)
- Declares `OnShow(bool show)` -- Harmony patch should target `ClusterMapScreen` directly

---

## 2. Registration and Opening

### ManagementMenu Integration

In `ManagementMenu.OnPrefabInit()`:
- `clusterMapScreen = component.GetInstantiatedObject<ClusterMapScreen>()`
- `clusterMapInfo = new ManagementMenuToggleInfo(UI.STARMAP.MANAGEMENT_BUTTON, ...Action.ManageStarmap...)`
- Only registered when `DlcManager.FeatureClusterSpaceEnabled()` is true
- Cancel handler: `clusterMapScreen.TryHandleCancel` (reverts from SelectDestination mode)

### Opening Flow

1. Player presses Z (`Action.ManageStarmap`) or clicks the Starmap button
2. `ManagementMenu.ToggleClusterMap()` -> `ToggleScreen(ScreenInfoMatch[clusterMapInfo])`
3. `ToggleScreen()` calls `Activate()` then `Show()`
4. **No availability gate** -- always available in DLC (unlike base game which requires a Telescope)

### Opening via Side Screen

`ClusterDestinationSideScreen.OnClickChangeDestination()` calls `ClusterMapScreen.Instance.ShowInSelectDestinationMode(targetSelector)`, which opens the map in destination selection mode if not already open.

---

## 3. Lifecycle Methods

### OnPrefabInit()
- Creates `SelectMarker` from prefab, initially inactive
- Sets `Instance = this`

### OnSpawn()
- Asserts hex prefab sizes are 2x2 (radius 1)
- Calls `GenerateGridVis()` to create hex cell visualizers for every cell in `ClusterGrid.Instance.cellContents`
- Sets initial scroll rect size and zoom scale
- Subscribes to event `1980521255` -> `UpdateVis()`
- Calls `Show(false)` to start hidden

### OnShow(true) -- Opening
- Calls `MoveToNISPosition()` (animated camera move)
- Calls `UpdateVis(onShow: true)` -- full visual refresh
- Subscribes to Game events:
  - `-1991583975` -> `OnFogOfWarRevealed`
  - `-1554423969` -> `OnNewTelescopeTarget`
  - `-1298331547` -> `OnClusterLocationChanged`
- Activates `ClusterMapSelectTool.Instance`
- Hides normal HUD (`PlanScreen`, `ToolMenu`, `OverlayScreen`)
- Disables user camera control
- Starts starmap audio snapshot and music

### OnShow(false) -- Closing
- Unsubscribes from Game events
- Resets mode to Default, clears destination selector
- Restores normal HUD
- Re-enables camera control, re-activates `SelectTool.Instance`
- Stops starmap music

---

## 4. Mode System

```csharp
public enum Mode
{
    Default,
    SelectDestination,
    FinishingSelectDestination
}
```

- **Default**: Normal browsing. Click hex to select entities within it.
- **SelectDestination**: Entered via `ShowInSelectDestinationMode()`. Hovering shows path preview and range info. Clicking a reachable hex sets the destination.
- **FinishingSelectDestination**: Transient state during destination commit.

### Mode Transitions
- `ShowInSelectDestinationMode(selector)` -> sets `m_destinationSelector`, enters SelectDestination mode
- `SelectHex()` in SelectDestination mode -> validates path, calls `m_destinationSelector.SetDestination()`, enters FinishingSelectDestination then Default
- `TryHandleCancel()` -> if in SelectDestination mode (and not closeOnSelect), reverts to Default and triggers cancel event on selector
- `m_closeOnSelect` flag: when true, selecting a destination or canceling closes the entire management menu

---

## 5. Hex Grid Model

### AxialI Coordinate System

Axial coordinates `(r, q)` for hex grid. Implements standard hex math:
- **Directions**: NORTHWEST(0,-1), NORTHEAST(1,-1), EAST(1,0), SOUTHEAST(0,1), SOUTHWEST(-1,1), WEST(-1,0)
- **CLOCKWISE**: EAST, SOUTHEAST, SOUTHWEST, WEST, NORTHWEST, NORTHEAST
- `ToCube()`: converts to cube coordinates `(q, -q-r, r)` for distance calculations
- `ToWorld()`: converts to world position via `AxialUtil.AxialToWorld(r, q)`

### AxialUtil

- `AxialToWorld(r, q)`: `x = sqrt(3)*r + sqrt(3)/2*q`, `y = -1.5*q` -- standard pointy-top hex layout
- `GetDistance(a, b)`: Manhattan distance in cube coordinates / 2
- `IsAdjacent(a, b)`: distance == 1
- `GetRing(center, radius)`: all hexes at exactly `radius` distance
- `GetRings(center, min, max)`: all hexes between min and max radius
- `GetAllPointsWithinRadius(center, radius)`: all hexes from 0 to radius
- `SpiralOut(start, maxRings)`: yields hexes in spiral order (useful for iteration)

### ClusterGrid

Singleton: `ClusterGrid.Instance`

**Core data**: `Dictionary<AxialI, List<ClusterGridEntity>> cellContents`

**Grid generation**: `GenerateGrid(rings)` creates all valid hex cells for a grid with `numRings` rings. Cells satisfy `r + q + (-r-q) == 0` (cube coordinate constraint). A 9-ring grid has cells from -8 to +8 in each axis.

**Key methods**:
- `IsValidCell(cell)`: checks if cell is in `cellContents`
- `GetCellRevealLevel(cell)`: delegates to `ClusterFogOfWarManager`
- `IsCellVisible(cell)`: fully revealed
- `GetVisibleEntitiesAtCell(cell)`: entities at cell that are visible AND cell is revealed
- `GetVisibleEntityOfLayerAtCell(cell, layer)`: first visible entity of given layer
- `GetVisibleEntityOfLayerAtAdjacentCell(cell, layer)`: checks all 6 neighbors
- `GetEntitiesOnCell(cell)`: raw list, no visibility filtering
- `GetEntitiesInRange(center, range)`: all entities within hex distance
- `RegisterEntity(entity)`: adds to cellContents, subscribes to location changes
- `UnregisterEntity(entity)`: removes from cellContents
- `GetPosition(entity)`: world position, with spiral offset when multiple entities share a hex
- `GetPath(start, end, selector)`: BFS pathfinding with visibility and obstacle constraints
- `GetLocationDescription(location)`: returns sprite, label, sublabel for a location (asteroid name, POI name, orbit, empty space, or fog of war)
- `GetHexDistance(a, b)`: cube distance
- `GetCellRing(cell)`: ring number from center (0,0)
- `NodeDistanceScale = 600f`: world units per hex (used for travel speed calculations)

### Pathfinding (GetPath)

BFS from start to end. Constraints checked:
1. If `!canNavigateFogOfWar` and destination not visible -> fail with `TOOLTIP_INVALID_DESTINATION_FOG_OF_WAR`
2. If destination has asteroid and `requireLaunchPadOnAsteroidDestination` and no launch pad there -> fail with `TOOLTIP_INVALID_DESTINATION_NO_LAUNCH_PAD`
3. If `requireAsteroidDestination` and no asteroid at destination -> fail with `TOOLTIP_INVALID_DESTINATION_REQUIRE_ASTEROID`
4. If `requiredEntityLayer != None` and no entity of that layer at destination -> fail with `TOOLTIP_INVALID_METEOR_TARGET`
5. BFS expansion: only through valid, visible (or fog-navigable) cells, skipping cells with visible asteroids (can't path through asteroids except start/end)
6. Optional `dodgeHiddenAsteroids`: also avoids hidden asteroids (except peeked ones)
7. If no path found -> `TOOLTIP_INVALID_DESTINATION_NO_PATH`

Returns list of `AxialI` waypoints (excluding start, including end).

---

## 6. Entity System

### EntityLayer Enum

```csharp
public enum EntityLayer
{
    Asteroid,   // Planetoids (worlds)
    Craft,      // Rockets (Clustercraft)
    POI,        // Space Points of Interest (harvestable, artifact)
    Telescope,  // Telescope scan targets
    Payload,    // Launched payloads
    FX,         // Visual effects
    Meteor,     // Meteor showers
    Debri,      // Space debris
    None
}
```

### ClusterGridEntity (Abstract Base)

All entities on the cluster map derive from this. Key abstract members:
- `Name`: display name
- `Layer`: which EntityLayer
- `AnimConfigs`: list of animation configurations for visual representation
- `IsVisible`: whether the entity should be shown at all
- `IsVisibleInFOW`: what reveal level the entity contributes (Hidden/Peeked/Visible)
- `Location`: AxialI position, triggers `ClusterLocationChangedEvent` on change

Virtual members:
- `ShowName()`: whether to display name label (default false)
- `ShowProgressBar()`: whether to show progress bar (default false)
- `GetProgress()`: progress value 0-1
- `SpaceOutInSameHex()`: whether to spread out when multiple entities share a hex
- `ShowPath()`: whether to draw travel path (default true)
- `GetUISprite()`: icon for UI
- `OnClusterMapIconShown(level)`: callback when visualizer is shown

On spawn, registers with `ClusterGrid.Instance` and triggers `ClusterMapScreen` to refresh.

### Concrete Entity Types

**AsteroidGridEntity** (Layer: Asteroid)
- Represents a world/planetoid
- `ShowName()` = true (always shows name)
- `IsVisibleInFOW` = Peeked (shows as question mark in fog)
- Has 3 anim configs: main asteroid, orbit ring, meteor shower effect
- On fog reveal, plays `WorldDetectedMessage` and discovery stinger

**Clustercraft** (Layer: Craft) -- see Section 8 for full detail
- Represents a DLC rocket
- `SpaceOutInSameHex()` = true
- `ShowName()` = true when not grounded
- `ShowPath()` = true when not grounded
- `ShowProgressBar()` when traveling and fueled
- `IsVisibleInFOW` = Visible (always visible even in fog)

**HarvestablePOIClusterGridEntity** (Layer: POI)
- Space resource fields (ore fields, gas clouds, etc.)
- `IsVisibleInFOW` = Peeked

**ArtifactPOIClusterGridEntity** (Layer: POI)
- Artifact collection points (e.g., Gravitas satellite)

**ClusterMapMeteorShowerVisualizer** (Layer: Meteor)
- Visual representation of incoming meteor showers
- Has identified/unidentified states with question mark overlay
- `SpaceOutInSameHex()` = true
- `ShowPath()` only when selected
- `IsVisibleInFOW` = Peeked

**TemporalTear** (via ClusterPOIManager)
- The temporal tear end-game POI

**BallisticClusterGridEntity** (Layer: Payload)
- Launched payloads (e.g., care packages, railgun payloads)
- `SpaceOutInSameHex()` = true
- `ShowPath()` only when selected
- `ShowProgressBar()` only when selected and traveling
- Fixed speed of 10 (very fast)
- `IsVisibleInFOW` = Visible
- `IsVisible` = false when grounded

**ClusterMapLongRangeMissileGridEntity** (Layer: Craft or similar)
- Long-range missiles launched at large impactors
- Has its own animator (`ClusterMapLongRangeMissileAnimator`)

**ClusterFXEntity** (Layer: FX)
- Visual effects on the cluster map
- Has its own animator (`ClusterMapFXAnimator`)

---

## 7. Visual System

### ClusterMapVisualizer

Attached to each entity's visual representation on the map. Manages:
- Animation controllers (`KBatchedAnimController`) -- one per `AnimConfig`
- Path drawing via `ClusterMapPathDrawer`
- Selection state
- Alert vignette (for asteroids)
- Name target transform (for `ClusterNameDisplayScreen`)

**Vis containers** (z-ordering):
1. `cellVisContainer` -- hex cell backgrounds
2. `terrainVisContainer` -- asteroids
3. `mobileVisContainer` -- rockets, payloads, meteors
4. `telescopeVisContainer` -- telescope targets
5. `POIVisContainer` -- POIs
6. `DebriVisContainer` -- debris
7. `FXVisContainer` -- visual effects

**Vis prefabs** (all must be 2x2 = radius 1):
- `cellVisPrefab` -- for hex cell backgrounds
- `terrainVisPrefab` -- for asteroids
- `mobileVisPrefab` -- for rockets/payloads/meteors
- `staticVisPrefab` -- for POIs/telescopes/FX/debris

**Reveal levels**:
- Hidden: game object deactivated
- Peeked: shows `peekControllerPrefab` (silhouette/question mark)
- Visible: shows full animation from entity's `AnimConfigs`

**Path drawing**:
- If entity has `ClusterTraveler` component and is traveling, draws path line
- Selected entities get `rocketSelectedPathColor`, unselected get `rocketPathColor`
- Path is rendered as Catmull-Rom spline via `ClusterMapPath` with `UILineRenderer`

### ClusterMapHex

Extends `MultiToggle`, implements `ICanvasRaycastFilter`. One per hex cell.

**Toggle states**: Unselected(0), Selected(1), OrbitHighlight(2)

**Interaction handlers**:
- `onClick` -> `TrySelect()` -> `ClusterMapScreen.Instance.SelectHex(this)`
- `onDoubleClick` -> `TryGoTo()` -> if single world at cell, switches active world via `CameraController.Instance.ActiveWorldStarWipe(id)`
- `onEnter` -> `OnHover()` -> shows tooltip, notifies screen
- `onExit` -> `OnUnhover()` -> clears hover

**Fog of war display**:
- Hidden: `fogOfWar` image active
- Peeked: `peekedTile` image active
- Visible: both hidden

**Tooltips on hover** (Default mode):
- Hidden cell: "???"
- Peeked cell with hidden entities: "UNKNOWN OBJECT DETECTED!"
- Peeked cell without: "???"
- Visible empty cell: "EMPTY SPACE"
- Visible cell with entities: no hex tooltip (entity hover text handles it)

**Tooltips in SelectDestination mode** (`SetDestinationStatus`):
- Shows path length: "Trip Distance: {pathLength}/{rocketRange}"
- For round trips: path doubled, "Trip Distance: {pathLength}/{rocketRange} (Return Trip)"
- Shows failure reason if path invalid (color-coded via `hoverColorValid`/`hoverColorInvalid`)

**Raycast filtering**: Custom hexagonal hit test via `IsRaycastLocationValid()` -- uses diamond approximation of hex shape.

### ClusterNameDisplayScreen

Manages floating name labels and progress bars above entities:
- Creates `nameAndBarsPrefab` instances for each registered entity
- In `LateUpdate()`, positions labels at entity's `nameTarget` transform
- Only shows when reveal level is Visible AND entity's `ShowName()` returns true AND zoom >= 115% (`ZOOM_NAME_THRESHOLD`)
- Progress bars shown/hidden per `ShowProgressBar()`

### ClusterMapSelectToolHoverTextCard

Hover text card that appears when hovering entities:
- Shows entity name in title style
- Shows status items from `KSelectable.GetStatusItemGroup()`
- "Main" category items shown first, then others
- Warning items get warning color/style

### SelectMarker

Selection indicator visual. Follows the selected entity's visualizer transform.

---

## 8. Rocket System (Clustercraft)

### Clustercraft : ClusterGridEntity, IClusterRange

**Status enum**: Grounded, Launching, InFlight, Landing

**Key properties**:
- `Name`: editable rocket name
- `Speed`: calculated from `EnginePower / TotalBurden * PilotSkillMultiplier` with modifiers for pilot type
- `EnginePower`: sum of all module engine powers
- `FuelPerDistance`: sum of all module fuel consumption rates
- `TotalBurden`: sum of all module burdens
- `ModuleInterface`: `CraftModuleInterface` -- access to all modules
- `Destination`: from `ClusterDestinationSelector`
- `Status`: current `CraftStatus`

**Speed modifiers**:
- Dupe pilot: `PilotSkillMultiplier` (varies by skill)
- Autopilot (no pilot at controls): 0.5x
- Robo-pilot + dupe pilot: 1.5x ("Super Piloted")
- Robo-pilot only (no data banks): 0x (stranded!)
- Mission control buff: +20% while active

**Fuel system**:
- `HasResourcesToMove(hexes)`: checks `BurnableMassRemaining / FuelPerDistance >= 600 * hexes`
- `BurnFuelForTravel()`: consumes fuel and oxidizer proportionally
- `NodeDistanceScale = 600f`: world units per hex tile

**Range**: `ModuleInterface.Range` and `ModuleInterface.RangeInTiles`

**Status items** (shown in hover/info):
- InFlight, RocketStranded, DestinationOutOfRange, WaitingToLand, InOrbit, Normal
- Piloted status: InFlightPiloted, InFlightUnpiloted, InFlightAutoPiloted, InFlightSuperPilot
- Cargo status: FlightAllCargoFull, FlightCargoRemaining

**Launch flow**:
1. `RequestLaunch()` -> checks `CheckPreppedForLaunch()`, sets `LaunchRequested = true`
2. `Launch()` -> checks `CheckReadyToLaunch()`, sets status to Launching, calls `DoLaunch()`, burns fuel, advances path one step
3. Travel proceeds via `ClusterTraveler.Sim200ms()`

**Landing flow**:
- When destination reached: `OnClusterDestinationReached()` finds a `LaunchPad`, calls `Land()`
- `CanLandAtPad(pad)` checks: pad availability, height clearance, flight path clear, operational status

### ClusterDestinationSelector

Base class for destination management:
- `m_destination`: target AxialI
- `assignable`: can user change destination
- `requireAsteroidDestination`: must land on asteroid
- `canNavigateFogOfWar`: can path through unrevealed cells
- `requireLaunchPadOnAsteroidDestination`: needs launch pad at asteroid destination
- `SetDestination(location)`: sets destination and triggers event

### RocketClusterDestinationSelector : ClusterDestinationSelector

Extended version for rockets:
- `m_launchPad`: per-world launch pad preference dictionary
- `Repeat`: round-trip mode flag
- `SetDestinationPad(pad)`: sets specific landing pad
- On destination reached with Repeat enabled: waits for POI harvest if applicable, then sets up return trip
- On launch: remembers previous destination and pad for return trips

### ClusterTraveler

Component that handles movement through the hex grid:
- `getSpeedCB`: callback to get speed (from Clustercraft)
- `getCanTravelCB`: callback to check if can travel
- `onTravelCB`: callback when moving to new hex
- `CurrentPath`: cached path from current location to destination
- `IsTraveling()`: not at destination
- `TravelETA()`: remaining seconds
- `RemainingTravelNodes()`: remaining hex steps
- `GetMoveProgress()`: fractional progress within current hex (0-1)

**Movement in Sim200ms**:
1. Accumulates `m_movePotential` based on `dt * speed`
2. When `m_movePotential >= 600f` (NodeDistanceScale): advances one hex step
3. `AdvancePathOneStep()`: validates, removes first path node, moves entity, reveals fog if applicable
4. Special case: `quickTravelToAsteroidIfInOrbit` -- skips the last hex step to asteroid (instant landing from orbit)

**Fog reveal**: `revealsFogOfWarAsItTravels = true` -- reveals each cell the entity passes through. `peekRadius = 2` -- peeks cells within 2 hexes.

### CraftModuleInterface

Not fully read but referenced extensively. Provides:
- `ClusterModules`: list of `Ref<RocketModuleCluster>`
- `Range`, `RangeInTiles`, `MaxRange`: fuel-based range calculation
- `FuelRemaining`, `OxidizerPowerRemaining`, `BurnableMassRemaining`
- `FuelPerHex`: fuel cost per hex tile
- `EnginePower`, `TotalBurden`, `Speed`
- `RocketHeight`, `MaxHeight`
- `CheckPreppedForLaunch()`, `CheckReadyToLaunch()`
- `GetEngine()`, `GetPassengerModule()`, `GetRobotPilotModule()`
- `GetClusterDestinationSelector()` -> returns `RocketClusterDestinationSelector`
- `GetPreferredLaunchPadForWorld(worldId)`

---

## 9. Fog of War

### ClusterRevealLevel

```csharp
public enum ClusterRevealLevel
{
    Hidden,    // Not visible at all
    Peeked,    // Silhouette/question mark visible
    Visible    // Fully revealed
}
```

### ClusterFogOfWarManager.Instance

State machine instance on SaveGame.

**Data**: `Dictionary<AxialI, float> m_revealPointsByCell` -- accumulated reveal points per cell.

**Reveal logic**:
- `GetRevealCompleteFraction(cell)`: `points / ROCKETRY.CLUSTER_FOW.POINTS_TO_REVEAL`, clamped to 1.0
- `GetCellRevealLevel(cell)`: fraction >= 1.0 -> Visible, > 0 -> Peeked, 0 -> Hidden
- `IsLocationRevealed(cell)`: fraction >= 1.0

**Revealing**:
- `RevealLocation(location, radius, peekRadius)`: instantly reveals all cells within radius, peeks cells within peekRadius
- Asteroids automatically get radius >= 1 (reveals adjacent cells too)
- `EarnRevealPointsForLocation(location, points)`: incremental reveal (used by telescopes)
- `PeekLocation(location, radius)`: sets minimum 0.01 points (peeked state)

**Automatic reveal on discovery**:
- When worlds are discovered, their locations are revealed
- `UpdateRevealedCellsFromDiscoveredWorlds()` called on initialization

### ClusterMapScreen.GetRevealLevel(entity)

Static method combining cell reveal and entity's own FOW visibility:
- If either cell or entity is Visible -> Visible
- If both are Peeked -> Peeked
- Otherwise -> Hidden

---

## 10. Selection Model

### Hex Selection (SelectHex)

In Default mode:
1. Get all visible, selectable entities at the hex
2. If the currently selected entity is among them, cycle to the next one (round-robin)
3. If no entities, deselect
4. Set `m_selectedHex`

In SelectDestination mode:
1. Check if a valid path exists from current position to clicked hex
2. If valid: set destination on `m_destinationSelector`, transition to FinishingSelectDestination
3. If invalid: nothing happens (tooltip shows why)

### Entity Selection (SetSelectedEntity)

1. Unsubscribe from previous entity's events
2. Set `m_selectedEntity`
3. Subscribe to new entity's destination-changed and select-object events
4. Get `KSelectable` component, call `ClusterMapSelectTool.Instance.Select(selectable)`
5. This triggers the game's standard selection system -> `DetailsScreen` opens for the entity

### ClusterMapSelectTool : InterfaceTool

Custom tool active when cluster map is open:
- `ShowHoverUI()`: delegates to `ClusterMapScreen.Instance.HasCurrentHover()`
- `UpdateHoveredSelectables()`: gets all selectable entities at the current hover hex
- `LateUpdate()`: updates hover state, triggers hover events
- `Select(selectable)`: deselects previous, selects new, triggers game selection event
- `SelectNextFrame(selectable)`: deferred selection (avoids same-frame issues)

### What Happens on Selection

When a `ClusterGridEntity` is selected:
1. The game's `DetailsScreen` opens showing the entity's details
2. For rockets: shows side screens including `ClusterDestinationSideScreen`
3. For POIs: shows `SpacePOISimpleInfoPanel` with element composition, mass, artifacts
4. For asteroids: shows world info
5. The select marker follows the entity's visualizer

---

## 11. Side Screens and Info Panels

### ClusterDestinationSideScreen : SideScreenContent

Shown when selecting rockets or rocket-related buildings:
- **Destination section**: Shows current destination icon and name. "Select Destination" button enters SelectDestination mode on the cluster map.
- **Landing Platform section** (rockets only): Dropdown of available launch pads at destination. Can select specific pad or "Any Launch Pad".
- **Round Trip section** (rockets only): Toggle between one-way and round trip.

Valid targets: `ClusterDestinationSelector` with `assignable=true`, `PassengerRocketModule`, `RoboPilotModule`, `RocketControlStation`

### SpacePOISimpleInfoPanel

Shown for `HarvestablePOIClusterGridEntity` and `ArtifactPOIConfigurator`:
- Title: "POINT OF INTEREST"
- Mass remaining with formatted value
- Element composition with percentages and icons
- Artifact availability status

### RocketSimpleInfoPanel

Shown for rockets in the SimpleInfoScreen:
- Range remaining with tooltip breakdown (fuel, oxidizer, fuel per hex)
- Speed with pilot modifier breakdown
- Max height vs current height
- Cargo bay status
- Artifact module contents

---

## 12. Path Visualization

### ClusterMapPathDrawer

Factory for `ClusterMapPath` objects. Attached to `ClusterMapScreen`.
- `AddPath()`: creates a new path visualizer
- `GetDrawPathList(startPos, pathPoints)`: converts AxialI waypoints to world positions

### ClusterMapPath

Line renderer for travel paths:
- Uses `UILineRenderer` with Catmull-Rom spline interpolation (10 points per segment)
- `pathStart` and `pathEnd` images at endpoints
- `pathEnd` rotated to point along final segment direction
- Colors: `rocketPathColor` (normal), `rocketSelectedPathColor` (selected), `rocketPreviewPathColor` (hover preview)

### Path Updates (UpdatePaths)

Called during `UpdateVis()`:
- **In SelectDestination mode with hover**: calculates path from selector's location to hovered hex, draws preview path in preview color
- Shows destination status on hovered hex (path length, range, failure reason)
- Considers round-trip mode (doubles path length display)
- **Otherwise**: clears preview path

Each entity's `ClusterMapVisualizer` also manages its own path via `RefreshPathDrawing()` for active travel paths.

---

## 13. Animation System

### ClusterMapTravelAnimator

State machine for movement animation on the cluster map:
- States: idle, grounded, repositioning, surfaceTransitioning, traveling (orientToPath, move, travelIdle, orientToIdle)
- `MoveTowards(target, dt)`: smoothly moves visualizer toward target position
- `RotateTowards(angle, dt)`: smoothly rotates visualizer to face travel direction
- Transitions based on entity state (traveling, grounded, launching, landing)

### ClusterMapRocketAnimator

State machine for rocket-specific animations:
- States: idle, grounded, moving (takeoff, traveling, landing), utility (collecting), exploding
- **idle**: plays "idle_loop"
- **grounded**: plays "grounded", entity becomes unselectable (rocket is on-world)
- **takeoff**: plays "launching"
- **traveling.regular**: plays "inflight_loop"
- **traveling.boosted**: plays "boosted" (mission control buff active)
- **landing**: plays "landing"
- **utility.collecting**: plays "mining_pre" -> "mining_loop" -> "mining_pst" (harvesting POI resources)
- **exploding**: plays self-destruct animation
- Also manages drill cone sub-animation for resource harvest modules

### FloatyAsteroidAnimation

In `ScreenUpdate()`: asteroids bob up and down using a sine wave offset, creating a floating effect. Each asteroid gets a different phase offset.

---

## 14. Zoom and Pan

### Zoom
- Range: 50% to 150% (`ZOOM_SCALE_MIN` to `ZOOM_SCALE_MAX`)
- Default/start: 75%
- Increment: 25% per scroll tick
- Smooth interpolation: `Mathf.Lerp(current, target, 4 * unscaledDeltaTime)` (capped at 0.9)
- Zoom centers on mouse position (adjusts content position to keep mouse-point fixed)
- Entity names only visible at zoom >= 115% (`ZOOM_NAME_THRESHOLD`)

### Pan
- `KScrollRect mapScrollRect` -- Unity scroll rect for drag panning
- Gamepad: analog stick mapped via `KInputManager.steamInputInterpreter.GetSteamCameraMovement()` with `scrollSpeed = 15f`

### Zoom Input (OnKeyDown)
- Handles `Action.ZoomIn` and `Action.ZoomOut`
- Only when mouse is over the game window and not over a non-cluster-map UI element
- Gamepad: increments by 25f
- Mouse: `Input.mouseScrollDelta.y * 25f`
- Also delegates to `CameraController.Instance.ChangeWorldInput(e)` for world switching

### Focus/NIS Position
- `SetTargetFocusPosition(target, delay)`: starts a coroutine to smoothly move the map to focus on a specific hex
- Used when selecting entities or navigating programmatically
- Lerps position and zoom toward target
- When close enough, selects the target hex

---

## 15. Input Handling

### OnKeyDown(KButtonEvent e)
- Zoom in/out (see above)
- World change input via `CameraController`
- Base `KScreen.OnKeyDown` for Escape handling

### TryHandleCancel()
- Called from ManagementMenu cancel handler
- If in SelectDestination mode (and not closeOnSelect): reverts to Default mode, triggers cancel event on destination selector
- Returns true if handled (prevents screen close)

### Mouse Input
- Hex click: handled by `ClusterMapHex.TrySelect()` (MultiToggle onClick)
- Hex double-click: `ClusterMapHex.TryGoTo()` -- switches to the world at that hex
- Hex hover: `ClusterMapHex.OnHover()` / `OnUnhover()`
- Drag pan: handled by `KScrollRect`
- Scroll zoom: handled in `OnKeyDown`

### No Custom Keyboard Navigation
There is no keyboard navigation for moving between hexes. All hex interaction is mouse-driven (click, hover, scroll). This is the main accessibility gap.

---

## 16. State Management Summary

### Screen-level state
- `m_selectedHex` (ClusterMapHex): currently selected hex cell
- `m_hoveredHex` (ClusterMapHex): currently hovered hex cell
- `m_selectedEntity` (ClusterGridEntity): currently selected entity
- `m_mode` (Mode): Default/SelectDestination/FinishingSelectDestination
- `m_destinationSelector` (ClusterDestinationSelector): active destination selector (in SelectDestination mode)
- `m_closeOnSelect` (bool): whether to close map after destination selection
- `m_currentZoomScale` / `m_targetZoomScale` (float): zoom state
- `m_previewMapPath` (ClusterMapPath): hover path preview in SelectDestination mode

### Visualization dictionaries
- `m_gridEntityVis` (Dictionary<ClusterGridEntity, ClusterMapVisualizer>): entity -> visualizer
- `m_gridEntityAnims` (Dictionary<ClusterGridEntity, ClusterMapVisualizer>): entity -> anim visualizer (same as above but includes anim management)
- `m_cellVisByLocation` (Dictionary<AxialI, ClusterMapVisualizer>): hex location -> cell visualizer

### Event subscriptions (when shown)
- `Game.Instance` event `-1991583975` -> `OnFogOfWarRevealed` -> `UpdateVis()`
- `Game.Instance` event `-1554423969` -> `OnNewTelescopeTarget` -> `UpdateVis()`
- `Game.Instance` event `-1298331547` -> `OnClusterLocationChanged` -> `UpdateVis()`
- Selected entity event `543433792` -> `OnDestinationChanged` -> `UpdateVis()`
- Selected entity event `-1503271301` -> `OnSelectObject` (handles entity deselection)
- New entity spawned event `1980521255` -> `UpdateVis()` (always subscribed from OnSpawn)

---

## 17. UpdateVis() -- Master Refresh

Called frequently (on show, fog reveal, telescope target change, location change, destination change, entity spawn).

1. `SetupVisGameObjects()`: creates/removes entity visualizers for all cells
2. `UpdatePaths()`: refresh path preview in SelectDestination mode
3. For each entity visualizer:
   - Set reveal level (Hidden/Peeked/Visible)
   - Set selection state
   - Update position if dirty or on show
4. Update select marker position/visibility
5. For each cell hex: set reveal state and toggle state (selected/orbit highlight/unselected)
6. `UpdateHexToggleStates()`: orbit highlight for hexes adjacent to a hovered asteroid
7. `FloatyAsteroidAnimation()`: bobbing animation

---

## 18. Relevant STRINGS

### UI.CLUSTERMAP
- `TITLE` = "STARMAP"
- `PLANETOID` = "Planetoid"
- `LANDING_SITES` = "LANDING SITES"
- `DESTINATION` = "DESTINATION"
- `OCCUPANTS` = "CREW"
- `ELEMENTS` = "ELEMENTS"
- `UNKNOWN_DESTINATION` = "Unknown"
- `TILES` = "Starmap Hexes"
- `TILES_PER_CYCLE` = "Starmap hexes per cycle"
- `CHANGE_DESTINATION` = "Click to change destination"
- `SELECT_DESTINATION` = "Select a new destination on the map"
- `TOOLTIP_INVALID_DESTINATION_FOG_OF_WAR` = "Cannot travel to this hex until it has been analyzed..."
- `TOOLTIP_INVALID_DESTINATION_NO_PATH` = "There is no navigable rocket path to this Planetoid..."
- `TOOLTIP_INVALID_DESTINATION_NO_LAUNCH_PAD` = "There is no Launch Pad on this Planetoid..."
- `TOOLTIP_INVALID_DESTINATION_REQUIRE_ASTEROID` = "Must select a Planetoid destination"
- `TOOLTIP_INVALID_DESTINATION_OUT_OF_RANGE` = "This destination is further away..."
- `TOOLTIP_INVALID_METEOR_TARGET` = "This destination does not have an impactor asteroid to target"
- `TOOLTIP_HIDDEN_HEX` = "???"
- `TOOLTIP_PEEKED_HEX_WITH_OBJECT` = "UNKNOWN OBJECT DETECTED!"
- `TOOLTIP_EMPTY_HEX` = "EMPTY SPACE"
- `TOOLTIP_PATH_LENGTH` = "Trip Distance: {0}/{1}"
- `TOOLTIP_PATH_LENGTH_RETURN` = "Trip Distance: {0}/{1} (Return Trip)"

### UI.CLUSTERMAP.STATUS.ROCKET
- GROUNDED = "Normal", TRAVELING = "Traveling", STRANDED = "Stranded", IDLE = "Idle"

### UI.CLUSTERMAP.ASTEROIDS.ELEMENT_AMOUNTS
- LOTS = "Plentiful", SOME = "Significant amount", LITTLE = "Small amount", VERY_LITTLE = "Trace amount"

### UI.CLUSTERMAP.POI
- TITLE = "POINT OF INTEREST"
- MASS_REMAINING = "Total Mass Remaining"
- ROCKETS_AT_THIS_LOCATION = "Rockets at this location"
- ARTIFACTS = "Artifact", ARTIFACTS_AVAILABLE = "Available", ARTIFACTS_DEPLETED = "Collected Recharge: {0}"

### UI.CLUSTERMAP.ROCKETS.*
- Speed, Fuel Remaining, Oxidizer Remaining, Range, Fuel Per Hex, Burden, Power, Max Modules, Max Height, Artifact Module

### UI.SPACEDESTINATIONS
- `ORBIT.NAME_FMT` = "Orbiting {Name}"
- `EMPTY_SPACE.NAME` = "Empty Space"
- `FOG_OF_WAR_SPACE.NAME` = "Unexplored Space"

### UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN
- TITLE = "Destination"
- DESTINATION_LABEL = "Destination: {0}"
- DESTINATION_LABEL_SELECTING = "Selecting new destination..."
- DESTINATION_LABEL_INVALID = "None selected"
- CHANGE_DESTINATION_BUTTON = "Select Destination"
- LANDING_PLATFORM_LABEL = "Landing Site: {0}"
- FIRSTAVAILABLE = "Any Launch Pad"
- ROUNDTRIP_LABEL_ONE_WAY / ROUNDTRIP_LABEL_ROUNDTRIP
- Various tooltip strings

---

## 19. Hex Cell Inventory System

### StarmapHexCellInventory

Represents collectible resources floating at a hex cell (e.g., debris from mining POIs):
- `AllInventories`: static dictionary mapping `AxialI` -> `StarmapHexCellInventory`
- Created by `ClusterGrid.AddOrGetHexCellInventory(cell)`
- Has visual representation via `StarmapHexCellInventoryVisuals`

### StarmapHexCellInventoryInfoPanel

Info panel shown in SimpleInfoScreen for hex cell inventory contents:
- Title: "CONTENTS"
- Lists items with icons and mass

---

## 20. Meteor Shower System

### ClusterMapMeteorShower

State machine managing meteor showers traveling through cluster space:
- Has destination world ID and arrival time
- Travels via `ClusterTraveler`
- Can be identified/unidentified
- Shows question mark when unidentified
- `HasBeenIdentified`: boolean flag
- `IdentifyingProgress`: float 0-1
- Telescopes can identify approaching showers

### ClusterMapMeteorShowerVisualizer

Visual component for meteor showers:
- Shows different animations based on identified/revealed state
- "unknown" anim when unidentified, "idle_loop" when identified
- Question mark overlay when unidentified
- Identify animation plays on identification

---

## 21. Supporting Classes

### ClusterLocationChangedEvent
```csharp
struct ClusterLocationChangedEvent {
    ClusterGridEntity entity;
    AxialI oldLocation;
    AxialI newLocation;
}
```

### IClusterRange
```csharp
interface IClusterRange {
    float GetRange();
    int GetRangeInTiles();
    int GetMaxRangeInTiles();
}
```
Implemented by `Clustercraft`.

### ClusterUtil
Utility methods including `GetAsteroidWorldIdAtLocation(AxialI)`.

### WorldContainer
Component on asteroid entities providing world metadata:
- `id`: world ID
- `IsDiscovered`, `IsDupeVisited`, `IsStartWorld`, `IsModuleInterior`
- `worldName`, `worldType`
- `DiscoveryTimestamp`: for sorting

---

## 22. Relationship to Other Screens

### DetailsScreen
When an entity is selected on the cluster map, `ClusterMapSelectTool.Select()` triggers the game's standard selection event, which opens `DetailsScreen` for the entity. This shows:
- For rockets: side screens including `ClusterDestinationSideScreen`, rocket modules, launch controls
- For POIs: `SpacePOISimpleInfoPanel`
- For asteroids: world details

### SimpleInfoScreen
The right-side info panel. Has specialized sub-panels:
- `RocketSimpleInfoPanel`: speed, range, fuel, cargo
- `SpacePOISimpleInfoPanel`: elements, mass, artifacts

### ManagementMenu
The cluster map is one of the management screens (alongside Research, Skills, Priorities). Opened via the same button bar and hotkey system.

### ClusterDestinationSideScreen
This side screen allows rocket destination management from within the normal game view (not just the cluster map). It can open the cluster map in SelectDestination mode when the user clicks "Select Destination".

---

## 23. Key Differences from Base Game StarmapScreen

| Aspect | Base Game (StarmapScreen) | DLC (ClusterMapScreen) |
|--------|--------------------------|------------------------|
| Base class | KModalScreen (modal) | KScreen (non-modal) |
| Data model | Distance rows with destinations | Hex grid with entities |
| Coordinates | Distance tier + orbital % | AxialI hex coordinates |
| Entity types | SpaceDestination only | 8 entity layers |
| Fog of war | Analysis % per destination | Per-cell reveal with Peeked/Visible |
| Navigation | Select from destination list | Click hexes, BFS pathfinding |
| Rockets | Listed in left panel | Selected directly on map |
| Selection | Destination + Rocket panels | Game standard selection system |
| Destination | Click planet icon | Click hex in SelectDestination mode |
| Side panels | Three fixed panels (list/details/dest) | Standard DetailsScreen + side screens |
| Path display | None (just range overlay) | Animated path lines with preview |
| Zoom | None | 50-150% with scroll wheel |
| Pan | None | Drag/scroll rect |
| Keyboard nav | None (same as DLC) | None (same as base) |
