# Research Screen - Complete Reference

This document captures everything about ONI's research screen: what a sighted player sees, how they interact with it, and what game code drives it. Written to inform the OniAccess handler implementation.

## How the Screen Opens

The research screen is a full-screen overlay opened via the management menu bar at the top of the game HUD. The hotkey is `Action.ManageResearch`. It is one of the "fullscreen UIs" alongside Skills, Starmap, and Cluster Map - only one can be open at a time. Opening any of them closes the others via `ManagementMenu.CloseAll()`.

The screen class is `ResearchScreen`, which inherits from `KModalScreen`. It disables the game camera when open and hides the DetailsScreen. Closing restores both.

## Screen Layout

The screen has two major regions:

### Left: Sidebar (`ResearchScreenSideBar`)
A vertical panel containing (top to bottom):
1. **Search box** (`KInputTextField`) - text input for filtering
2. **Filter preset buttons** - 12 category buttons: Oxygen, Food, Water, Power, Morale, Ranching, Filter, Tile, Transport, Automation, Medicine, Rocket. Each pre-fills the search box with its keyword. Strings from `STRINGS.UI.RESEARCHSCREEN.FILTER_BUTTONS.[NAME]`
3. **Completion filter** - three toggle buttons: All, Available, Completed
4. **Tech list** - scrollable list of all technologies, grouped into collapsible categories
5. **Queue section** - `queueContainer` exists but `RefreshQueue()` is a no-op stub

### Right: Tech Tree Grid (`scrollContent`)
A pannable, zoomable 2D canvas displaying all research nodes as cards connected by prerequisite lines. Category title banners (`ResearchTreeTitle`) label major horizontal bands.

**Point display** (`pointDisplayContainer`) at the top shows the player's accumulated research points per type when global point inventory is enabled.

**Zoom controls**: Zoom In and Zoom Out buttons, plus Close button.

## The Tech Tree (Main Grid)

### Tree Structure
Technologies are positioned on a 2D grid using `ResourceTreeNode` coordinates loaded from a tree layout file. Each tech has:
- `center` (Vector2) - position in the tree
- `width`, `height` - card dimensions
- `edges` (List\<Edge\>) - line paths to prerequisites

The tree is organized into horizontal bands by category. Categories (from `STRINGS.RESEARCH.TREES`):
- Food
- Power
- Solid Material
- Colony Development
- Radiation Technologies
- Medicine
- Liquids
- Gases
- Exosuits
- Decor
- Computers
- Rocketry

Category title banners are rendered behind the tech cards via `ResearchTreeTitle` objects.

### Tech Card (`ResearchEntry`)
Each technology is rendered as a card containing:
- **Tech name** (`LocText researchName`) - from `STRINGS.RESEARCH.TECHS.[ID].NAME`
- **Background** (`Image BG`) - color indicates state
- **Title background** (`Image titleBG`)
- **Border highlight** (`Image borderHighlight`) - shown on hover
- **Icon panel** (`iconPanel`) - row of small icons showing unlocked buildings/items
- **Progress bars** (`progressBarContainer`) - one bar per required research type, showing `current/total` points
- **Prerequisite lines** (`lineContainer`) - UILineRenderer lines connecting to prerequisite tech cards

**Tooltip** on the tech name shows: `"{Name}\n{Description}\n\n{UnlocksText}"` where UnlocksText lists all unlocked items formatted via `UI.RESEARCHSCREEN_UNLOCKSTOOLTIP`.

Each unlocked item icon has its own tooltip: `"{ItemName}\n{ItemDescription}"` plus DLC info if applicable.

### Visual States

| State | BG Color | Meaning | Click Action |
|-------|----------|---------|--------------|
| Available | Blue (`defaultColor`) | Prerequisites met, not queued | `OnResearchClicked` - queues this tech |
| Queued/Active | Magenta (`pendingColor`) | In the research queue | `OnResearchCanceled` - cancels this tech |
| Completed | Yellow (`completedColor`) | Already researched | No click handler (cleared) |
| Missing Prerequisites | Blue | Prerequisites not met | `OnResearchClicked` still bound but prereqs auto-queued |

### Prerequisite Lines
- 4-point polyline from this card's left edge to the prerequisite card's right edge
- Elbow offset: 32 units
- Y offset adjustment when cards are vertically offset (20px up or down)
- **Inactive**: thickness 2, grey color (`inactiveLineColor`)
- **Active** (on hover): thickness 6, bright color (`activeLineColor`)

### Hover Behavior
When the mouse enters a ResearchEntry:
1. `researchScreen.TurnEverythingOff()` - dims all entries
2. `OnHover(entered: true, targetTech)` - recursively highlights this entry and all prerequisites
3. `SetEverythingOn()` activates border highlight, thickens lines, brightens colors
4. Recursion walks `targetTech.requiredTech` to highlight the full prerequisite chain

Mouse exit calls `TurnEverythingOff()` again, resetting all entries.

## Navigation & Interaction

### Pan (Mouse)
- Left-click or right-click starts drag tracking (`dragStartPosition`)
- Dragging moves the tree content (`scrollContent.anchoredPosition`)
- Drag inertia applied on release (decays at `4x deltaTime`)
- Right-click release (without drag) closes the screen via `ManagementMenu.CloseAll()`

### Pan (Keyboard)
- `Action.PanUp/PanDown/PanLeft/PanRight` (arrow keys or WASD depending on bindings)
- Smooth panning at `keyboardScrollSpeed = 200` units/sec with easing
- Edge clamping: 250px buffer around content bounds

### Pan (Gamepad)
- Right stick camera movement, scaled by `2x deltaTime * speed`

### Zoom
- **Scroll wheel** (`Action.ZoomIn/ZoomOut`): increments of `0.05` per scroll, zoom follows mouse position
- **Buttons**: increments of `0.5` per click, zoom centered on screen
- Range: `minZoom=0.15` to `maxZoom=1.0`, default `targetZoom=0.6`
- Zoom is lerped smoothly at `effectiveZoomSpeed=5`
- Zoom only works when mouse is to the right of the sidebar (checks `mousePos.x > sideBar.rectTransform().sizeDelta.x`)

### ZoomToTech
`ResearchScreen.ZoomToTech(string techID, bool highlight)` - pans and zooms the tree to center on a specific tech. Called when clicking a tech in the sidebar list. If `highlight=true`, also sets the sidebar search to the tech name.

### Select/Queue Research
Clicking an available `ResearchEntry`:
1. If another research is active, cancels it (`researchScreen.CancelResearch()`)
2. Calls `Research.Instance.SetActiveResearch(targetTech, clearQueue: true)`
3. This clears the queue, then `AddTechToQueue(tech)` recursively adds the tech plus all incomplete prerequisites
4. Queue is sorted by tier (lowest first), so prerequisites are researched first
5. `activeResearch` is set to `queuedTech[0]` (lowest tier = first to research)
6. All entries in queue get `QueueStateChanged(true)` - turn magenta

### Cancel Research
Clicking a queued `ResearchEntry`:
1. Calls `OnResearchCanceled()` which calls `researchScreen.CancelResearch()` and `Research.Instance.CancelResearch(targetTech)`
2. `CancelResearch` recursively removes the tech and any dependent techs from the queue
3. All dequeued entries get `QueueStateChanged(false)` - return to default color

### Close Screen
- `Action.Escape` closes via `ManagementMenu.CloseAll()`
- `CloseButton` click does the same
- Right-click (without drag) also closes

## Sidebar Interactions

### Search
- `KInputTextField` captures keyboard input when focused
- `OnValueChangesPaused` fires after typing pauses, calls `SetTextFilter()`
- When active, all tech widgets are reparented to a "SearchResults" category
- Fuzzy matching via `SearchUtil.TechCache` - scores each tech and its items against the query
- Non-matching techs are hidden (deferred via `QueuedDeactivations`, 5 per frame)
- Matching items within non-matching techs are greyed out (label color dimmed, icon semi-transparent)
- Clear button (`clearSearchButton`) calls `ResetFilter()`

### Filter Preset Buttons
12 buttons for common categories. Clicking one:
1. Deactivates all other filter buttons
2. Toggles this filter on/off
3. Sets `searchBox.text` to the filter's localized string (or empty if toggling off)
4. This triggers the search flow above

### Completion Filter
Three mutually exclusive toggles:
- **All** (`CompletionState.All`) - show everything (default)
- **Available** (`CompletionState.Available`) - hide completed techs and techs with incomplete prerequisites
- **Completed** (`CompletionState.Completed`) - only show completed techs

Combined with text search: both filters apply simultaneously.

### Tech List in Sidebar
Each tech appears as a widget (`techWidgetRootAltPrefab`) containing:
- Tech name label
- Expandable unlock container showing each unlocked item with icon and name
- MultiToggle with 3 states: 0=default, 1=queued, 2=completed
- Hover highlights the corresponding entry in the main tree
- Click calls `researchScreen.ZoomToTech(techID)` to pan the main tree

Techs are grouped under collapsible category headers. Each category has:
- Label from `STRINGS.RESEARCH.TREES.TITLE[CATEGORY]`
- Toggle to expand/collapse the content
- `categoryExpanded` dictionary tracks state

Items within each tech widget are also clickable and zoom to the parent tech.

Even/odd row coloring alternates for visual distinction.

## Data Model

### Tech (technology definition)
- `Id` (string) - unique identifier, e.g. `"FarmingTech"`
- `Name` (LocString) - from `STRINGS.RESEARCH.TECHS.[ID].NAME`
- `desc` (string) - from `STRINGS.RESEARCH.TECHS.[ID].DESC`
- `tier` (int) - research tier, determines queue order (lower = earlier)
- `category` (string) - tree band category ID
- `requiredTech` (List\<Tech\>) - prerequisite technologies
- `unlockedTech` (List\<Tech\>) - technologies that depend on this one
- `unlockedItems` (List\<TechItem\>) - buildings/items unlocked
- `costsByResearchTypeID` (Dictionary\<string, float\>) - cost per research type
- `searchTerms` (List\<string\>) - additional search keywords

Key methods:
- `IsComplete()` - checks `Research.Instance.Get(this)?.IsComplete()`
- `ArePrerequisitesComplete()` - all `requiredTech` are complete
- `CanAfford(ResearchPointInventory)` - player has enough points of each type
- `RequiresResearchType(string)` - tech needs this type and cost > 0
- `CostString(ResearchTypes)` - formatted cost display

### TechInstance (runtime research state)
- `tech` (Tech) - the definition
- `complete` (bool) - purchased/researched
- `progressInventory` (ResearchPointInventory) - accumulated points
- `UnlockedPOITechIds` (List\<string\>) - POI-unlocked item IDs

Key methods:
- `IsComplete()` - returns `complete`
- `Purchased()` - sets `complete = true`
- `GetTotalPercentageComplete()` - average across all required research types
- `PercentageCompleteResearchType(string)` - progress on one type as 0..1

### TechItem (unlocked building/item)
- `Id` (string) - unique ID
- `Name` (string) - localized name
- `description` (string) - localized description
- `parentTechId` (string) - owning Tech ID
- `isPOIUnlock` (bool) - can be unlocked via space exploration
- `requiredDlcIds`, `forbiddenDlcIds` (string[]) - DLC filtering

Key methods:
- `ParentTech` (property) - resolves to `Db.Get().Techs.Get(parentTechId)`
- `IsComplete()` - parent tech complete OR POI-unlocked
- `UISprite()` - icon sprite

### Research (singleton manager)
- `Instance` (static) - global access
- `activeResearch` (TechInstance) - currently being researched (first in queue by tier)
- `queuedTech` (List\<TechInstance\>) - full research queue
- `researchTypes` (ResearchTypes) - all research type definitions
- `UseGlobalPointInventory` (bool) - whether points are pooled globally
- `globalPointInventory` (ResearchPointInventory) - shared point pool

Key methods:
- `SetActiveResearch(Tech, bool clearQueue)` - queue a tech (and its prereqs)
- `CancelResearch(Tech)` - remove tech and dependents from queue
- `GetActiveResearch()` - returns `activeResearch`
- `GetTargetResearch()` - returns last in queue (the user's actual target)
- `GetResearchQueue()` - returns copy of `queuedTech`
- `GetOrAdd(Tech)` - get or create TechInstance
- `AddResearchPoints(string typeID, float points)` - progress research
- `IsBeingResearched(Tech)` - true if tech == activeResearch
- `CompleteQueue()` - debug: instantly complete all queued

### ResearchType (research point type)
Five types defined in `ResearchTypes`:

| ID | Name String | Color | Research Building | Ingredient |
|----|-------------|-------|-------------------|------------|
| `"basic"` | `RESEARCH.TYPES.ALPHA.NAME` | Blue-grey | ResearchCenter | 100 Dirt |
| `"advanced"` | `RESEARCH.TYPES.BETA.NAME` | Purple | AdvancedResearchCenter | 25 Water |
| `"space"` | `RESEARCH.TYPES.GAMMA.NAME` | Orange | CosmicResearchCenter | None |
| `"nuclear"` | `RESEARCH.TYPES.DELTA.NAME` | Yellow | NuclearResearchCenter | None |
| `"orbital"` | `RESEARCH.TYPES.ORBITAL.NAME` | Orange | OrbitalResearchCenter / DLC1CosmicResearchCenter | None |

Each type has `id`, `name`, `description`, `sprite`, `color`.

### ResearchPointInventory
- `PointsByTypeID` (Dictionary\<string, float\>) - accumulated points per type
- `AddResearchPoints(string, float)` / `RemoveResearchPoints(string, float)`

## Research Progress Flow

1. Player selects a tech on the research screen
2. `Research.SetActiveResearch()` queues the tech + all incomplete prerequisites, sorted by tier
3. `activeResearch` = lowest-tier incomplete tech in queue
4. Duplicants assigned to research errand work at a research station
5. Research station calls `Research.Instance.AddResearchPoints(typeID, points)`
6. `CheckBuyResearch()` tests if `activeResearch.tech.CanAfford(inventory)`
7. When affordable: deducts costs, calls `activeResearch.Purchased()`, fires `ResearchComplete` event (-107300940)
8. `GetNextTech()` removes completed tech from queue, sets next as active
9. Repeat until queue empty

## Events

| Hash | Name | Data | When |
|------|------|------|------|
| -1914338957 | ActiveResearchChanged | `List<TechInstance>` (queue) | Queue modified |
| -107300940 | ResearchComplete | `Tech` | Tech purchased |
| -1974454597 | (game deactivation) | null | Forces screen close |

## Notifications
The research system fires game notifications when:
- **No research building**: active research needs a type with no corresponding building placed (e.g., needs Advanced Research Center but none exists). Uses `RESEARCH.MESSAGING.MISSING_RESEARCH_STATION`
- **No researcher skill**: active research needs a type but no duplicant has the required skill perk. Uses `RESEARCH.MESSAGING.NO_RESEARCHER_SKILL`

## Completion Effects
When a tech completes (`ResearchEntry.ResearchCompleted()`):
- Background changes to yellow
- Click handler is cleared (no further interaction)
- Music sting plays: `"Stinger_ResearchComplete"`
- `ResearchCompleteMessage` is queued to the in-game messenger
- Klei metrics event sent

## Key Classes Summary

| Class | Location | Role |
|-------|----------|------|
| `ResearchScreen` | Assembly-CSharp | Main screen (KModalScreen), tree grid, pan/zoom |
| `ResearchScreenSideBar` | Assembly-CSharp | Sidebar (KScreen), search, filters, tech list |
| `ResearchEntry` | Assembly-CSharp | Individual tech card in the tree grid |
| `ResearchTreeTitle` | Assembly-CSharp | Category banner in the tree grid |
| `Tech` | Assembly-CSharp | Technology definition (Resource) |
| `TechInstance` | Assembly-CSharp | Runtime research state per tech |
| `TechItem` | Assembly-CSharp | Unlocked building/item definition |
| `Research` | Assembly-CSharp | Singleton research manager |
| `ResearchType` | Assembly-CSharp | Research point type definition |
| `ResearchTypes` | Assembly-CSharp | All 5 research types + ID constants |
| `TechTreeTitles` | Database | Loads tree layout titles |
| `Techs` | Database | Tech database (ResourceSet) |
| `ManagementMenu` | Assembly-CSharp | Opens/closes the research screen |

## Accessibility-Relevant Observations

### The tree is spatial, not linear
The main grid is a 2D pan/zoom canvas. There is no built-in keyboard navigation between tech entries - only mouse click/hover. The sidebar provides an alternative linear view grouped by category.

### Sidebar as primary navigation surface
The sidebar already provides a categorized, searchable, filterable list of all techs. This is the natural fit for screen reader navigation. Each tech widget shows the name, unlocked items, and click-to-select behavior.

### Queue is implicit
There is no separate queue display - the queue is visible only as magenta-colored entries in the tree. The sidebar `RefreshQueue()` is an empty stub. Queue contents must be read from `Research.Instance.GetResearchQueue()`.

### Progress is passive
Research progresses in the background as duplicants work. Progress bars update when the screen is shown. Current progress per tech is available via `TechInstance.progressInventory.PointsByTypeID`.

### Key data to speak per tech
- Name (`tech.Name`)
- State (completed / queued / available / prerequisites missing)
- Description (`tech.desc`)
- Unlocked items (names and descriptions from `tech.unlockedItems`)
- Cost per research type (`tech.costsByResearchTypeID` + type names)
- Progress if in queue (`TechInstance.progressInventory`)
- Prerequisites (`tech.requiredTech` names and completion state)
- What this tech unlocks (`tech.unlockedTech` names)

### Global point inventory
When `Research.Instance.UseGlobalPointInventory` is true, accumulated points are shown in the `pointDisplayContainer`. These are the player's banked research points that haven't been spent yet.

## Sidebar Widget Hierarchy

The sidebar tech list is built from prefabs with specific child structures accessed via `HierarchyReferences` (a component that maps string keys to Transform/Component references). Understanding this hierarchy is essential for widget discovery.

### Category Widget (`techCategoryPrefabAlt`)
```
Category (GameObject) - has HierarchyReferences, positioned in projectsContainer
├─ "Label" (LocText) - category name, e.g. "Food"
├─ "Toggle" (MultiToggle) - expand/collapse, states: 0=collapsed, 1=expanded
└─ "Content" (RectTransform) - container for child tech widgets, SetActive toggled by expand state
    └─ [tech widgets parented here]
```
- Click on Toggle flips `categoryExpanded[categoryID]` and calls `RefreshCategoriesContentExpanded()`
- Content visibility directly tied to `categoryExpanded` dictionary

### Tech Widget (`techWidgetRootAltPrefab`)
```
TechWidget (GameObject) - has HierarchyReferences, MultiToggle, ToolTip
├─ "Label" (LocText) - tech name
├─ "UnlockContainer" (RectTransform) - holds unlocked item sub-widgets
│   └─ [tech item widgets parented here]
└─ "BarRows" (RectTransform) - progress bar rows (used by RefreshWidgetProgressBars)
    ├─ Child[0] - header/spacer
    └─ Child[1+] (HierarchyReferences) - one per research type cost
        ├─ "Bar" (Image) - fill bar, width scaled by progress ratio
        ├─ "Label" (LocText) - "current/total" text
        └─ "Icon" (Image) - research type icon
```
- **MultiToggle states**: 0=default (available), 1=queued/active (magenta), 2=completed (yellow). Set by `RefreshWidgets()`
- **ToolTip**: set to `tech.desc`
- **onClick**: calls `researchScreen.ZoomToTech(techID)` - pans main tree to this tech
- **onEnter**: highlights the corresponding `ResearchEntry` in the main tree + plays hover sound
- **onExit**: clears all highlights

### Tech Item Widget (`techItemPrefab`)
```
TechItem (GameObject) - has MultiToggle, HierarchyReferences
├─ Image[0] - row background (alternates evenRowColor/oddRowColor)
├─ Image[1] - item icon sprite (from TechItem.UISprite())
├─ "Icon" (Image) - referenced via HierarchyReferences for search dimming
├─ "Label" (LocText) - item name, referenced via HierarchyReferences for search dimming
└─ LocText[0] - item name (set via GetComponentsInChildren)
```
- **onClick**: calls `researchScreen.ZoomToTech(techID)` (same as parent tech widget)
- During search: non-matching items get grey label color and 50% icon opacity
- Even/odd row coloring alternates across all items globally (not per-tech)

### Unused Prefab Fields
`techWidgetUnlockedItemPrefab` and `techWidgetRowPrefab` are declared as serialized fields on `ResearchScreenSideBar` but are never referenced in the decompiled code. They may be legacy or used only in Unity editor wiring.

## Tech Tree Size and Shape

### Total Count
**109 technologies** defined in `Database/Techs.cs`:
- 105 base game techs
- 1 conditional base-game tech (`DataScienceBaseGame` - requires DLC3 without Expansion1)
- 3 Expansion1-only techs (`Bioengineering`, `SpaceCombustion`, `HighVelocityDestruction`)
- 1 Expansion1+DLC3 tech (`DataScience`)

Techs without matching nodes in the GraphML tree layout file are excluded at load time, so the actual displayed count may be slightly lower.

### Tier Range
**Tiers 0 through 11** (12 levels). Tier is calculated dynamically as `max(prerequisite tiers) + 1`. Tier 0 techs have no prerequisites.

### Tier Cost Progression (Base Game)

| Tier | Basic | Advanced | Space |
|------|-------|----------|-------|
| 0 | - | - | - |
| 1 | 15 | - | - |
| 2 | 20 | - | - |
| 3 | 30 | 20 | - |
| 4 | 35 | 30 | - |
| 5 | 40 | 50 | - |
| 6 | 50 | 70 | - |
| 7 | 70 | 100 | - |
| 8 | 70 | 100 | 200 |
| 9 | 70 | 100 | 400 |
| 10 | 70 | 100 | 800 |
| 11 | 70 | 100 | 1600 |

### Tier Cost Progression (Expansion1 / Spaced Out)

| Tier | Basic | Advanced | Orbital | Nuclear |
|------|-------|----------|---------|---------|
| 0 | - | - | - | - |
| 1 | 15 | - | - | - |
| 2 | 20 | - | - | - |
| 3 | 30 | 20 | - | - |
| 4 | 35 | 30 | - | - |
| 5 | 40 | 50 | 0 | 20 |
| 6 | 50 | 70 | 30 | 40 |
| 7 | 70 | 100 | 250 | 370 |
| 8 | 100 | 130 | 400 | 435 |
| 9 | 100 | 130 | 600 | - |
| 10 | 100 | 130 | 800 | - |
| 11 | 100 | 130 | 1600 | - |

Note: Base game uses `space` type at tier 8+. Expansion1 replaces `space` with `orbital` and `nuclear` starting at tier 5. The cost arrays are selected at database init based on `DlcManager.IsExpansion1Active()`.

### Categories (12 bands)
Food, Power, Solid Material, Colony Development, Radiation Technologies, Medicine, Liquids, Gases, Exosuits, Decor, Computers, Rocketry.

### Tree Shape
The tree is a directed acyclic graph loaded from a GraphML layout file. It is wider than it is deep - most techs cluster in tiers 1-6 with the tree narrowing at higher tiers as it branches into specialized late-game paths (rocketry, nuclear, space). Each category band contains multiple techs spanning several tiers horizontally.

## DLC Variation

### Architecture
All DLC filtering happens at database initialization (`Db.Init()`). Tech availability is static after startup - it does not change during gameplay.

### DLC IDs
- `""` (empty) - Vanilla / base game
- `"EXPANSION1_ID"` - Spaced Out expansion
- `"DLC2_ID"`, `"DLC3_ID"`, `"DLC4_ID"` - additional DLC packs
- `"COSMETIC1_ID"` - cosmetic DLC (no gameplay impact)

### How DLC Affects Techs

**Whole techs added/removed by DLC:**
- `Bioengineering`, `SpaceCombustion`, `HighVelocityDestruction` only exist with Expansion1
- `DataScience` requires Expansion1 + DLC3
- `DataScienceBaseGame` requires DLC3 without Expansion1
- Some existing techs are modified: e.g. `HighTempForging` gains `"Gantry"` unlock with Expansion1

**Tech items filtered by DLC:**
`TechItem` has `requiredDlcIds` and `forbiddenDlcIds`. Items silently don't exist if DLC requirements aren't met (`AddTechItem()` returns null). The research screen filters items via `Game.IsCorrectDlcActiveForCurrentSave()` which checks:
1. Any required DLC is active (OR logic for `anyRequiredDlcIds`)
2. All required DLC is active (AND logic for `requiredDlcIds`)
3. No forbidden DLC is active

**Tier cost structures completely swapped:**
The `TECH_TIERS` array in `Techs.cs` is built with a different cost table depending on `IsExpansion1Active()`. This means the same tech at the same tier costs different amounts of different research types in vanilla vs Expansion1.

**Research types gated:**
- Base game tiers use: `basic`, `advanced`, `space`
- Expansion1 tiers use: `basic`, `advanced`, `orbital`, `nuclear`
- The `space` type is base-game-only at tier 8+; `orbital` and `nuclear` replace it in Expansion1

**Cost overrides:**
The `Tech` constructor accepts `overrideDefaultCosts` but only applies them when `IsExpansion1Active()` is true. This allows individual techs to deviate from their tier's default costs, but only in the Expansion1 context.

### Handler Implications
The handler does not need to branch on DLC. All filtering happens before the screen opens. `Db.Get().Techs.resources` already contains only the techs valid for the current save, and `tech.unlockedItems` already excludes DLC-gated items. The handler can treat whatever data the game provides as the complete, authoritative set.
