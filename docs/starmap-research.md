# Starmap Screen Technical Research

## Two Versions

The game has two completely separate starmap implementations selected at registration time by `DlcManager.FeatureClusterSpaceEnabled()`, which returns `true` when the Spaced Out DLC (Expansion 1) is active.

| Aspect | Base Game | Spaced Out DLC |
|--------|-----------|----------------|
| Class | `StarmapScreen` | `ClusterMapScreen` |
| Base class | `KModalScreen` | `KScreen` |
| Singleton | `StarmapScreen.Instance` | `ClusterMapScreen.Instance` |
| Toggle info field | `starmapInfo` | `clusterMapInfo` |
| Toggle method | `ManagementMenu.ToggleStarmap()` | `ManagementMenu.ToggleClusterMap()` |
| Cancel handler | `null` | `clusterMapScreen.TryHandleCancel` |
| Availability gate | Requires Telescope (`Components.Telescopes.Count > 0`) | Always available (no gate) |
| Data source | `SpacecraftManager.instance.destinations` (list of `SpaceDestination`) | `ClusterGrid.Instance.cellContents` (hex grid of `ClusterGridEntity`) |

Both use `Action.ManageStarmap` (default key Z) and share the button label `UI.STARMAP.MANAGEMENT_BUTTON` ("STARMAP").

---

## Registration and Opening

### ManagementMenu.OnPrefabInit()

Both screens are obtained via `ScheduledUIInstantiation`:
```csharp
starmapScreen = component.GetInstantiatedObject<StarmapScreen>();
clusterMapScreen = component.GetInstantiatedObject<ClusterMapScreen>();
```

The DLC check determines which gets registered in `ScreenInfoMatch`:
```csharp
if (DlcManager.FeatureClusterSpaceEnabled())
{
    ScreenInfoMatch.Add(clusterMapInfo, new ScreenData { screen = clusterMapScreen, ... });
}
else
{
    ScreenInfoMatch.Add(starmapInfo, new ScreenData { screen = starmapScreen, ... });
}
```

Only the registered version appears in the toggle button list. They never coexist.

### Opening Flow

1. Player presses Z (Action.ManageStarmap) or clicks the Starmap button in the management toolbar
2. `ManagementMenu.OnButtonClick()` -> `ToggleScreen(ScreenInfoMatch[toggleInfo])`
3. `ToggleScreen()` calls `screen.Activate()` then `screen.Show()`
4. For base game, `CheckStarmap()` disables the button if no Telescopes exist
5. Can also be opened from `CommandModuleSideScreen` "Open Starmap" button, or `TelescopeSideScreen` button

---

## Non-DLC StarmapScreen Deep Dive

### Class: `StarmapScreen : KModalScreen`

KModalScreen means it consumes all input when active (modal overlay).

### Lifecycle

- `OnPrefabInit()`: Creates all BreakdownList instances (status, checklist, mass, range, storage, fuel, oxidizer, passengers for rockets; analysis, research, mass, composition, resources, artifacts for destinations). Calls `LoadPlanets()` to populate the map.
- `OnActivate()`: Sets `Instance = this`
- `OnShow(bool show)`: When showing, starts audio snapshots and music, calls `UpdateDestinationStates()` and `Refresh()`. On hide, stops audio.
- `OnCleanUp()`: Unsubscribes from selection events

### Declares `OnShow` (not `Show`)

The class declares `protected override void OnShow(bool show)`. Harmony patch should target `StarmapScreen` directly.

### UI Layout (Three Panels)

#### Left Panel: Rocket List (`listPanel`) / Rocket Details (`rocketPanel`)

These two panels are mutually exclusive. `listPanel` is shown when no rocket is selected; `rocketPanel` when a CommandModule is selected.

**Rocket List Panel (`listPanel`)**:
- `listHeaderLabel` - header
- `listHeaderStatusLabel` - "NO ROCKETS" or "ROCKETS: {n}"
- `listNoRocketText` - help text when no rockets exist
- `rocketListContainer` - scrollable container of `listRocketTemplate` rows
- Each row contains:
  - `EditableTitleBar` for rocket name (editable)
  - `BreakdownList` with status, passengers, modules, max range, storage, destination mass
  - `LaunchRocketButton` (MultiToggle) - active only when grounded
  - `LandRocketButton` (MultiToggle) - debug only
  - `ProgressBar` - shown for in-flight rockets
- Rows are stored in `listRocketRows` dictionary (Spacecraft -> HierarchyReferences)

**Rocket Details Panel (`rocketPanel`)**:
- `rocketHeaderLabel` / `rocketHeaderStatusLabel`
- Multiple BreakdownLists:
  - `rocketDetailsStatus` - "Mission Status"
  - `rocketDetailsChecklist` - "Launch Checklist" (all ProcessConditions)
  - `rocketDetailsRange` - "Max Range" (fuel, efficiency, booster, thrust, range)
  - `rocketDetailsMass` - "Mass" (dry, wet, total)
  - `rocketThrustWidget` - visual thrust display
  - `rocketDetailsStorage` - "Storage"
  - `rocketDetailsFuel` - "Fuel"
  - `rocketDetailsOxidizer` - "Oxidizer"
  - `rocketDetailsDupes` - "Passengers"
- `launchButton` (MultiToggle) - launches rocket to selected destination
- `showRocketsButton` (MultiToggle) - returns to list view
- `analyzeButton` (MultiToggle) - analyze/suspend analysis of selected destination

#### Center Panel: Star Map (`Map`)

- `Map` (RectTransform) - the main map viewport
- `rowsContiner` (RectTransform) - vertical layout of distance rows
- `rowPrefab` - template for each distance row (colored bands)
- `planetPrefab` (StarmapPlanet) - template for each destination icon
- Distance rows are arranged vertically, closest at bottom
- Each `SpaceDestination` becomes a `StarmapPlanet` widget positioned horizontally by `startingOrbitPercentage`
- `distanceOverlay` (Image) - gray overlay showing rocket's max range
- `visualizeRocketImage/Trajectory/Label/Progress` - in-flight rocket visualization
- Planet labels appear on hover (name + distance if analyzed, analysis % if not)
- Clicking a planet calls `SelectDestination()`

#### Right Panel: Destination Details

- `destinationHeaderLabel` / `destinationStatusLabel`
- `destinationNameLabel` - destination name (or "Destination Unknown")
- `destinationTypeValueLabel` - destination type (or "Type Unknown")
- `destinationDistanceValueLabel` - distance in km
- `destinationDescriptionLabel` - flavor description
- Multiple BreakdownLists:
  - `destinationDetailsAnalysis` - "Analysis" + progress bar
  - `destinationDetailsResearch` - "Research" (research opportunities, checkmarks)
  - `destinationDetailsMass` - "Destination Mass" (current, max, min, replenish rate)
  - `destinationDetailsComposition` - "World Composition" (elements with %, icons)
  - `destinationDetailsResources` - "Resources" (recoverable entities like eggs)
  - `destinationDetailsArtifacts` - "Artifacts" (artifact tier chances)

### State Management

- `selectedDestination` (SpaceDestination) - currently selected destination on the map
- `currentSelectable` (KSelectable) - currently selected game object
- `currentCommandModule` (CommandModule) - selected rocket's command module (null if none)
- `currentLaunchConditionManager` (LaunchConditionManager) - selected rocket's LCM
- `currentRocketHasGasContainer/LiquidContainer/SolidContainer/EntitiesContainer` - cargo bay flags
- `planetWidgets` (Dictionary<SpaceDestination, StarmapPlanet>) - map from data to UI widget
- `listRocketRows` (Dictionary<Spacecraft, HierarchyReferences>) - map from spacecraft to list row

### Key Methods

#### `SelectDestination(SpaceDestination destination)`
Central method for destination selection. Updates `selectedDestination`, highlights planet widget, sets spacecraft destination if a rocket is selected, shows destination panel, validates travel ability.

#### `OnSelectableChanged(object data)`
Called when game selection changes. If a CommandModule is selected, shows rocket details and syncs destination. Otherwise, shows rocket list.

#### `Refresh(object data = null)`
Master refresh: `FillRocketListPanel()`, `RefreshAnalyzeButton()`, `ShowDestinationPanel()`, `FillRocketPanel()`, `ValidateTravelAbility()`.

#### `UpdateDestinationStates()`
Iterates all destinations, updates planet widget visuals based on analysis state: sprite (known/unknown/far), label (name+distance or analysis%), rocket icons, click handlers, hover handlers.

#### `FillRocketListPanel()`
Rebuilds the entire rocket list from `SpacecraftManager.instance.GetSpacecraft()`. Each row shows name, status, passengers, modules, range, storage, destination mass, and launch/progress controls.

#### `ShowDestinationPanel()`
Populates the destination details panel for `selectedDestination`: name, type, distance, description, research opportunities, mass data, element composition with cargo bay compatibility, recoverable entities, artifact drop rates.

#### `ValidateTravelAbility()`
Enables/disables the launch button based on `CheckReadyToLaunch()`.

#### `LaunchRocket(LaunchConditionManager lcm)`
Calls `lcm.Launch(spacecraftDestination)`, then refreshes the list.

### Input Handling

- `OnKeyDown(KButtonEvent e)`: Checks `CheckBlockedInput()` (blocks if editing a rocket name), then delegates to `KModalScreen.OnKeyDown` which handles Escape.
- No custom keyboard navigation. Map interaction is mouse-only (click planets, hover for labels).
- `Update()`: Calls `PositionPlanetWidgets()` and handles initial scroll-to-bottom.

### Data Sources

#### SpacecraftManager
- `SpacecraftManager.instance` - singleton
- `.destinations` (List<SpaceDestination>) - all space destinations
- `.GetSpacecraft()` - list of all Spacecraft
- `.GetSpacecraftFromLaunchConditionManager(lcm)` - find spacecraft by LCM
- `.GetSpacecraftDestination(lcm)` - get assigned destination for a spacecraft
- `.SetSpacecraftDestination(lcm, dest)` - assign destination
- `.GetDestinationAnalysisState(dest)` - Hidden/Discovered/Complete
- `.GetDestinationAnalysisScore(dest)` - float progress
- `.GetStarmapAnalysisDestinationID()` - which dest the telescope is analyzing
- `.SetStarmapAnalysisDestinationID(id)` - set telescope analysis target
- `.GetSpacecraftsForDestination(dest)` - spacecraft targeting this dest

#### SpaceDestination
- `.id` (int) - unique ID
- `.type` (string) - destination type ID
- `.distance` (int) - 0-indexed distance tier
- `.OneBasedDistance` - distance + 1
- `.GetDestinationType()` - returns `SpaceDestinationType` (name, description, element table, etc.)
- `.AnalysisState()` - shorthand for `SpacecraftManager.instance.GetDestinationAnalysisState(this)`
- `.recoverableElements` - Dictionary<SimHashes, float>
- `.researchOpportunities` - List<ResearchOpportunity>
- `.CurrentMass` / `.AvailableMass` - resource amounts
- `.startingOrbitPercentage` - horizontal position in row (0-1)
- `.startAnalyzed` - some destinations are pre-analyzed

#### SpaceDestinationType
- `.Id` - string identifier
- `.typeName` / `.Name` - display name (localized)
- `.description` - flavor text
- `.spriteName` - icon asset name
- `.iconSize` - sprite size (128 default)
- `.elementTable` - Dictionary<SimHashes, MinMax> of recoverable elements
- `.recoverableEntities` - Dictionary<string, int> of entities (e.g., eggs)
- `.artifactDropTable` - artifact tier chances
- `.visitable` - whether rockets can visit
- `.maxiumMass` / `.minimumMass` - mass bounds
- `.cyclesToRecover` - regeneration rate

#### Spacecraft
- `.id` (int) - unique ID
- `.rocketName` (string) - display name (editable)
- `.state` (MissionState) - Grounded/Launching/Underway/WaitingToLand/Landing/Destroyed
- `.launchConditions` (LaunchConditionManager) - reference
- `.GetTimeLeft()` / `.GetDuration()` - mission timing
- `.controlStationBuffTimeRemaining` - boost time

#### Destination Types (from SpaceDestinationTypes)
Complete list: Satellite, MetallicAsteroid, RockyAsteroid, CarbonaceousAsteroid, IcyDwarf, OrganicDwarf, DustyMoon, TerraPlanet, VolcanoPlanet, GasGiant, IceGiant, Wormhole, SaltDwarf, RustPlanet, ForestPlanet, RedDwarf, GoldAsteroid, HydrogenGiant, OilyAsteroid, ShinyPlanet, ChlorinePlanet, SaltDesertPlanet, Earth.

Fixed destinations (always present): 2x CarbonaceousAsteroid (d=0), MetallicAsteroid (d=1), RockyAsteroid (d=2), IcyDwarf (d=3), OrganicDwarf (d=4). Plus Earth (d=4) and Wormhole (d=16). Rest are randomly generated, 15-25 total random destinations distributed across distance tiers 1-16.

### Event Subscriptions

- `Game.Instance` event `-1503271301` (selection changed) -> `OnSelectableChanged`
- `SpacecraftManager.instance` event `532901469` -> `RefreshAnalyzeButton()` + `UpdateDestinationStates()`
- `SpacecraftManager.instance` event `611818744` -> `OnSpaceDestinationAdded()` + `UpdateDestinationStates()`
- `currentLaunchConditionManager` event `1655598572` -> `Refresh`

### Relevant Strings (UI.STARMAP namespace)

Top-level:
- `TITLE` = "STARMAP"
- `MANAGEMENT_BUTTON` = "STARMAP"
- `DEFAULT_NAME` = "Rocket"
- `NO_ROCKETS_TITLE` = "NO ROCKETS"
- `ROCKET_COUNT` = "ROCKETS: {0}"
- `UNKNOWN_DESTINATION` = "Destination Unknown"
- `UNKNOWN_TYPE` = "Type Unknown"
- `ANALYSIS_AMOUNT` = "Analysis {0} Complete"
- `ANALYSIS_COMPLETE` = "ANALYSIS COMPLETE"
- `NO_ANALYZABLE_DESTINATION_SELECTED` = "No destination selected"
- `ANALYZE_DESTINATION` = "ANALYZE OBJECT"
- `SUSPEND_DESTINATION_ANALYSIS` = "PAUSE ANALYSIS"
- `STATUS` = "SELECTED"
- `DESTINATIONTITLE` = "Destination Status"
- `DISTANCE_OVERLAY` = "TOO FAR FOR THIS ROCKET"
- `ROCKETLIST` = "Rocket Hangar"
- `LAUNCH_MISSION` = "LAUNCH MISSION"
- `CANT_LAUNCH_MISSION` = "CANNOT LAUNCH"
- `SEE_ROCKETS_LIST` = "See Rockets List"
- `CURRENT_MASS` = "Current Mass"
- `MAXIMUM_MASS` = "Maximum Mass"
- `MINIMUM_MASS` = "Minimum Mass"
- `REPLENISH_RATE` = "Replenished/Cycle:"

Sub-classes: DESTINATIONSTUDY, COMPONENT, MISSION_STATUS, LISTTITLES, ROCKETWEIGHT, DESTINATIONSELECTION, ROCKETSTATUS, ROCKETSTATS, STORAGESTATS, LAUNCHCHECKLIST (with many sub-classes for each condition).

---

## DLC ClusterMapScreen Summary

### Class: `ClusterMapScreen : KScreen`

Not modal (KScreen, not KModalScreen). Uses a hex grid model instead of distance rows.

### Key Differences from Base Game

- **Data model**: Hex grid (`ClusterGrid`) with `AxialI` coordinates, not distance-based rows
- **Entity types**: Asteroids, Craft, POI, Telescope targets, Payloads, Meteors, FX, Debris (via `EntityLayer` enum)
- **Fog of war**: Cells have reveal levels (Hidden/Peeked/Visible)
- **Modes**: Default, SelectDestination, FinishingSelectDestination
- **Selection model**: Select hex -> select entity within hex (cycles through multiple entities on same hex)
- **Destination selection**: Path-based (ClusterGrid.GetPath), shows range and path preview
- **Visual components**: ClusterMapVisualizer prefabs for cells, terrain, mobile, static entities
- **Zoom**: 50-150% scale with scroll wheel
- **No rocket list panel**: Rocket management is through the normal game selection + side screens
- **ClusterDestinationSelector**: Component on rockets that handles destination assignment

### Lifecycle

- Declares `OnShow(bool show)` - same pattern as base game
- `OnSpawn()`: Generates grid vis, creates hex cells from `ClusterGrid.Instance.cellContents`
- `OnShow(true)`: Subscribes to fog-of-war, telescope, location-changed events; activates ClusterMapSelectTool; hides normal HUD; starts starmap music
- `OnShow(false)`: Restores normal HUD, re-activates SelectTool

### Registration Path

Completely separate from StarmapScreen. Both are instantiated but only one is registered in ManagementMenu. The DLC version has a cancel handler (`TryHandleCancel()`) that reverts from SelectDestination mode.

### Hex Cell Inventory (DLC feature)

`StarmapHexCellInventory` and `StarmapHexCellInventoryInfoPanel` provide item/resource display for hex cells with debris or resources. This is DLC-only functionality.

---

## Accessibility Considerations for Non-DLC StarmapScreen

### Navigation Challenges

1. **Planet selection is mouse-only**: Planets are positioned arbitrarily within distance rows. No keyboard navigation exists. Need to provide keyboard-driven list navigation.

2. **Three-panel layout**: Left (rockets), Center (map), Right (destination details). All three need to be navigable and their content spoken.

3. **Destination data is rich**: Each analyzed destination has type, distance, mass, element composition with percentages, research opportunities, recoverable entities, artifact drop rates. This is a lot to speak concisely.

4. **Rocket list is complex**: Each rocket has name, status, passenger count, modules, range, storage, destination mass. In-flight rockets show progress instead.

5. **State coupling**: Selecting a rocket changes which destination is highlighted. Selecting a destination assigns it to the selected rocket. These side effects need clear speech feedback.

6. **Analysis system**: Telescope analysis target is set from this screen. Current analysis target and progress need to be conveyed.

7. **Launch conditions**: The checklist has many items (engine, fuel, oxidizer, pilot, nosecone, etc.) each with Ready/Warning/Failure status.

### Available Data for Speech

All data is queryable at speech time from live game objects:
- `SpacecraftManager.instance` for destinations and spacecraft
- `selectedDestination` for current selection
- `currentCommandModule` / `currentLaunchConditionManager` for selected rocket
- `SpaceDestinationType` properties for destination details
- Element names via `ElementLoader.FindElementByHash()`
- Artifact tier names via `Strings.Get(rate.first.name_key)`

### Potential Approach

The screen has distinct functional areas that could map to a tabbed or sectioned handler:
1. **Destination list**: All destinations sorted by distance, with analysis state and name
2. **Destination details**: Full details of selected destination
3. **Rocket list**: All spacecraft with status
4. **Rocket details**: Full details of selected rocket
5. **Actions**: Analyze destination, Launch mission

The map visualization (center panel) is purely visual and would be replaced entirely by list-based navigation of destinations.
