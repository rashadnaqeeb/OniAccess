# Details Screen Handler — Implementation Plan

How the OniAccess mod will make the details screen (entity inspection panel) accessible. Covers the full scope: informational tabs, side screen tabs, user menu, lifecycle, and phasing.

See `inspection-panels.md` for the raw game architecture this builds on.

## Design Summary

One handler (`DetailsScreenHandler`) manages the entire details screen. It extends `NestedMenuHandler` with two navigation levels: **level 0** is section headers, **level 1** is items within a section. Right/Enter drills into a section, Left goes back, Up/Down navigates items and crosses section boundaries automatically. Tab/Shift+Tab cycles through tabs that are active for the current entity. Type-ahead search operates at level 1 across all sections.

It maintains a list of **tab readers** — small objects that each know how to read one tab's UI into a list of `DetailSection`s (header + items). Main info tabs and side screen tabs live in the same tab list. The player doesn't need to know which are "main" and which are "side" — they're all just tabs about the selected entity.

## Tab List

Fixed order. Tabs that aren't relevant for the current entity are skipped silently — including tabs where `IsAvailable` is true but `Populate` returns zero widgets.

Tab names come from the game's localized `UI.DETAILTABS` strings, no mod strings needed.

| # | Tab | Game String | Source | Shows For |
|---|-----|-------------|--------|-----------|
| 0 | Status | `UI.DETAILTABS.SIMPLEINFO.NAME` | `SimpleInfoScreen` | Everything |
| 1 | Bio | `UI.DETAILTABS.PERSONALITY.NAME` | `MinionPersonalityPanel` | Dupes |
| 2 | Errands | `UI.DETAILTABS.BUILDING_CHORES.NAME` | `BuildingChoresPanel` | Buildings with chores |
| 3 | Properties | `UI.DETAILTABS.DETAILS.NAME` | `AdditionalDetailsPanel` | Everything |
| 4 | Config | `UI.DETAILTABS.CONFIGURATION.NAME` | Side screen tab (Config) | Most buildings, dupes |
| 5 | Errands | `UI.DETAILTABS.BUILDING_CHORES.NAME` | Side screen tab (Errands) | Dupes |
| 6 | Material | `UI.DETAILTABS.MATERIAL.NAME` | Side screen tab (Material) | Reconstructable buildings |
| 7 | Blueprint | `UI.DETAILTABS.COSMETICS.NAME` | Side screen tab (Blueprints) | Buildings with facades, dupes |

The game uses "Errands" for both tab 2 and tab 5. No collision in practice — tab 2 shows for buildings, tab 5 shows for dupes. They never appear together.

## User Menu

The game's right-click context menu (deconstruct, cancel, priority, enable/disable, etc.) is accessed via a **separate hotkey overlay**, not as a tab. A dedicated key opens the action list as a temporary overlay while the details screen is active. Escape or activating an action closes the overlay and returns to tab navigation.

The user menu is populated dynamically per entity — components register buttons sorted by priority constants. The overlay reads live from `UserMenuScreen`'s button list.

## Architecture

### Handler

```
DetailsScreenHandler : NestedMenuHandler
    IDetailTab[] _allTabs          // all 8 tabs, fixed
    List<IDetailTab> _activeTabs   // filtered for current target (zero-section tabs excluded)
    int _tabIndex                  // index into _activeTabs
    DetailSection[] _sections      // current tab's sections (header + items each)
    GameObject _lastTarget         // detect target changes
```

- **Show patch** on `DetailsScreen.OnShow(bool)` — push handler on show, pop on hide. Registered in `_showPatchedTypes` so generic `KScreen.Activate` patch skips it.
- **Target change detection** in `Tick()` — compare `DetailsScreen.Instance.target` to `_lastTarget`. On change, rebuild `_activeTabs`, reset to tab 0, repopulate sections, speak entity name + tab name + first section.
- **Entity name announcement** on target change: `target.GetProperName()` only. For dupes, also speak skill subtitle from `MinionResume.GetSkillsSubtitle()`.
- **Populate** delegates to `_activeTabs[_tabIndex].Populate()`, which fills a `List<DetailSection>`.
- **Tab/Shift+Tab** cycles `_tabIndex` through `_activeTabs`. Speaks tab name + first section.

### Refresh Strategy

`Populate` is called on **every keypress** — before processing the key action, the current tab re-reads its content from live UI. This guarantees the user never hears stale data.

To maintain cursor stability across refreshes:
- Track the current widget by its underlying Unity component reference, not by index
- After re-Populate, find that component in the new list and update cursor to its new index
- If the component is gone (item removed from storage mid-browse, etc.), clamp to nearest valid index

### Tab Interface

```csharp
interface IDetailTab {
    string DisplayName { get; }
    string GameTabId { get; }  // game's DetailTabHeader ID (null for side screens)
    bool IsAvailable(GameObject target);
    void Populate(GameObject target, List<DetailSection> sections);
}

class DetailSection {
    public string Header;
    public readonly List<WidgetInfo> Items = new List<WidgetInfo>();
}
```

Each tab reader gets the target GameObject and fills a `List<DetailSection>`. Each section has a header (spoken at level 0) and items (spoken at level 1). The handler owns the list and passes it in. No caching — `Populate` is called fresh on every keypress.

### Reading From UI

Tab readers walk the game's live UI components rather than querying game data directly. The game panels have already:
- Determined which sections are relevant
- Formatted values with correct units and localization
- Hidden inactive sections
- Sorted content

We read what's there. Section headers become Label widgets in the flat list. Collapsed/expanded state of `CollapsibleDetailContentPanel` is ignored — we always read the content.

## Informational Tab Readers

### AdditionalDetailsPanel (Properties tab)

Uniform structure. All sections are `CollapsibleDetailContentPanel` instances with `DetailLabel` children created via `SetLabel(id, text, tooltip)`.

**Sections**: Element properties, disease source, current germs, immune system, energy overview, generators, consumers, batteries.

**Reading strategy**: Get the panel instance from `DetailsScreen`. Walk each `CollapsibleDetailContentPanel` field. For each active panel, emit a section-header Label, then walk `Content` children for active `DetailLabel` components — read `label.text` for speech, `toolTip.toolTip` for tooltip.

### SimpleInfoScreen (Status tab)

Most complex. Multiple container types:

| Section | Container | Reading Strategy |
|---------|-----------|-----------------|
| Status items | `StatusItemEntry` list in `statusItemsFolder` | Walk children of `statusItemsFolder` transform. Each active child has a `LocText` and `ToolTip`. Some have a `statusItemClickCallback` (e.g., "vent blocked" selects the obstruction, "contact with germs" focuses on exposure source). Items with a callback are Button widgets; items without are Labels. Enter on a Button triggers the callback, which typically focuses the camera — the mod's tile cursor follows. |
| Storage | `CollapsibleDetailContentPanel` with `DetailLabel`, `DetailLabelWithButton`, `DetailCollapsableLabel` | Walk `StoragePanel.Content` children. Read `label.text` from each active child. Collapsable groups (identical items) become nested sections: group summary as section header, individual items as level-1 children. Each item is a Button widget (`DetailLabelWithButton.button`) — Enter calls `SelectTool.Instance.Select(item)` to inspect the stored item, switching the details screen target. |
| Vitals | `MinionVitalsPanel` | Extends `CollapsibleDetailContentPanel`. Uses `AmountLine`/`AttributeLine`/`CheckboxLine` structs, each with a `LocText` + `ToolTip`. Walk active `Content` children. Exception: `CheckboxLine` text set via `label_text_func` — `GetParsedText()` returns prefab text for previously inactive components. |
| Effects | `DescriptorPanel` inside `CollapsibleDetailContentPanel` | Walk descriptor panel's children for active labels. |
| Requirements | `DescriptorPanel` inside `CollapsibleDetailContentPanel` | Same as effects. |
| Stress | `DetailsPanelDrawer` inside `CollapsibleDetailContentPanel` | Walk `Content` children for active labels. |
| Fertility | `CollapsibleDetailContentPanel` with `SetLabel` | Same as AdditionalDetailsPanel sections. |
| Info/Description | `CollapsibleDetailContentPanel` with `SetLabel` | Same pattern. |
| Process conditions | `CollapsibleDetailContentPanel` with custom rows | Walk active child rows. |
| World traits/biomes/etc | `CollapsibleDetailContentPanel` with custom rows | Walk active child rows. |

### MinionPersonalityPanel (Bio tab)

Six `CollapsibleDetailContentPanel` sections (bio, traits, attributes, resume, amenities, equipment), all populated via `SetLabel(id, text, tooltip)` + `Commit()`. Same reading strategy as `AdditionalDetailsPanel`.

### BuildingChoresPanel (Errands tab)

Shows chore assignments with performer info. Each dupe row is a `KButton` (`BuildingChoresPanelDupeRow`) — Enter calls `GameUtil.FocusCamera()` on that dupe, moving the camera and the mod's tile cursor to them.

## Side Screen Tabs

Side screens live inside `SidescreenTab.bodyInstance` containers. Multiple side screens can be active simultaneously within a single tab (e.g., a building might show both `ButtonMenuSideScreen` and `SingleSliderSideScreen` under Config).

### How to find active side screens

`DetailsScreen.sideScreens` is a `List<SideScreenRef>`. Each has:
- `screenInstance` — the instantiated `SideScreenContent` (null if never shown)
- `tab` — which `SidescreenTabTypes` it belongs to (Config/Errands/Material/Blueprints)

Walk the list. For each entry where `screenInstance != null && screenInstance.gameObject.activeSelf`, that side screen is active for the current target. Group by `tab`.

### Side Screen Inventory

87 concrete `SideScreenContent` subclasses exist (83 extend `SideScreenContent` directly, plus 3 that extend `ReceptacleSideScreen` and 2 that extend `SingleItemSelectionSideScreenBase`; 1 abstract base `SingleItemSelectionSideScreenBase` excluded from count). 5 never show to players: `NoConfigSideScreen`, `ConditionListSideScreen`, `HabitatModuleSideScreen`, `RoleStationSideScreen` (all return `false` unconditionally) and `AutoPlumberSideScreen` (debug mode only). That leaves **82 player-visible side screens**.

Plus two tab-body panels that aren't `SideScreenContent` subclasses but occupy the Material and Blueprints tabs: `DetailsScreenMaterialPanel` and `CosmeticsPanel`.

### Side Screen UI Patterns — Verified Per-Screen

Each of the 82 player-visible side screens was individually verified against the game source. The generic walker recursively walks active child GameObjects looking for KButton, KToggle, MultiToggle, KSlider (including NonLinearSlider subclass), KNumberInputField, and LocText. It re-walks after every Enter to catch dynamic rebuilds.

Important corrections from verification:
- **HierarchyReferences**, **List\<T\>**, **C# arrays**, **SliderSet**: these are code-side references to widgets that are still GameObjects in the Unity hierarchy. The recursive walker finds them.
- **NonLinearSlider** extends KSlider — `GetComponent<KSlider>()` catches it.
- **Dynamically instantiated** widgets (from prefabs) ARE in the hierarchy after creation. The walker runs after `SetTarget`, so it finds them.

#### Category A: Generic walker covers (68 screens)

All interactive elements are standard widget types in the hierarchy. Walker finds everything. Re-walk after Enter handles dynamic rebuilds. Some screens have minor semantic gaps noted in parentheses — the walker provides functional access but a dedicated reader could do better.

| # | Screen | Widgets | Notes |
|---|--------|---------|-------|
| 1 | `SingleCheckboxSideScreen` | KToggle, LocText | |
| 2 | `AutomatableSideScreen` | KToggle, LocText | |
| 3 | `PlayerControlledToggleSideScreen` | KButton | |
| 4 | `CritterSensorSideScreen` | KToggle × 2 | |
| 5 | `DoorToggleSideScreen` | KToggle × 3, LocText | |
| 6 | `CapacityControlSideScreen` | KSlider, KNumberInputField, LocText | |
| 7 | `ActiveRangeSideScreen` | KSlider × 2, KNumberInputField × 2, LocText | |
| 8 | `TemperatureSwitchSideScreen` | KToggle × 2, KSlider, LocText | |
| 9 | `ValveSideScreen` | KSlider, KNumberInputField, LocText | |
| 10 | `LimitValveSideScreen` | NonLinearSlider(KSlider), KNumberInputField, KButton, LocText | |
| 11 | `CounterSideScreen` | KButton × 3, KToggle, KNumberInputField, LocText | |
| 12 | `TimerSideScreen` | KSlider × 2, KNumberInputField × 2, KToggle, KButton, LocText | |
| 13 | `TimeRangeSideScreen` | KSlider × 2, LocText | |
| 14 | `ThresholdSwitchSideScreen` | NonLinearSlider(KSlider), KToggle × 2, MultiToggle × 4, KNumberInputField, LocText | |
| 15 | `RailGunSideScreen` | KSlider, KNumberInputField, LocText | |
| 16 | `SingleSliderSideScreen` | KSlider, KNumberInputField, LocText | Via SliderSet; widgets in hierarchy |
| 17 | `DualSliderSideScreen` | KSlider × 2, KNumberInputField × 2, LocText | Via SliderSet |
| 18 | `IntSliderSideScreen` | KSlider, KNumberInputField, LocText | Via SliderSet; wholeNumbers=true |
| 19 | `MultiSliderSideScreen` | KSlider × N, KNumberInputField × N, LocText | Spawned in container |
| 20 | `GeneShufflerSideScreen` | KButton, LocText | |
| 21 | `LoreBearerSideScreen` | KButton, LocText | |
| 22 | `SealedDoorSideScreen` | KButton, LocText | |
| 23 | `ResearchSideScreen` | KButton, LocText | |
| 24 | `ClusterGridWorldSideScreen` | KButton | |
| 25 | `LaunchButtonSideScreen` | KButton, LocText | |
| 26 | `RocketInteriorSectionSideScreen` | KButton, LocText | |
| 27 | `SelfDestructButtonSideScreen` | KButton, LocText | |
| 28 | `WarpPortalSideScreen` | KButton, LocText | |
| 29 | `HighEnergyParticleDirectionSideScreen` | KButton list, LocText | Buttons in hierarchy despite List\<KButton\> field |
| 30 | `PrinterceptorSideScreen` | KButton, LocText | Arrays are hierarchy references |
| 31 | `TemporalTearSideScreen` | KButton, LocText | HierarchyReferences → still in hierarchy |
| 32 | `TelescopeSideScreen` | KButton, LocText | |
| 33 | `BaseGameImpactorImperativeSideScreen` | LocText | Display only |
| 34 | `ProgressBarSideScreen` | LocText (via GenericUIProgressBar.label) | Progress text in LocText child |
| 35 | `NToggleSideScreen` | KToggle × N, LocText | Dynamic KToggles in container |
| 36 | `FewOptionSideScreen` | MultiToggle × N, LocText | Dynamic rows in container |
| 37 | `RocketRestrictionSideScreen` | KToggle × 2 | Toggled containers |
| 38 | `SuitLockerSideScreen` | KButton × 4, LocText | Two toggled container panels |
| 39 | `LogicBitSelectorSideScreen` | MultiToggle × N, LocText | Dynamic rows; HierarchyReferences → in hierarchy |
| 40 | `LogicBroadcastChannelSideScreen` | MultiToggle × N, LocText | Dynamic rows; same pattern |
| 41 | `FlatTagFilterSideScreen` | MultiToggle × N, LocText | |
| 42 | `ConfigureConsumerSideScreen` | MultiToggle × N, LocText | |
| 43 | `CometDetectorSideScreen` | MultiToggle × N, LocText | |
| 44 | `ClusterLocationFilterSideScreen` | MultiToggle × N, LocText | |
| 45 | `LureSideScreen` | MultiToggle × N, LocText | |
| 46 | `MissileSelectionSideScreen` | MultiToggle × N, LocText | |
| 47 | `RemoteWorkTerminalSidescreen` | MultiToggle × N, LocText | |
| 48 | `GeoTunerSideScreen` | MultiToggle × N, LocText | Dynamic rows in container |
| 49 | `DispenserSideScreen` | KButton, MultiToggle × N, LocText | |
| 50 | `GeneticAnalysisStationSideScreen` | KToggle × N, LocText | Dynamic rows in container |
| 51 | `ArtifactAnalysisSideScreen` | KButton × N, LocText | Dynamic rows in container |
| 52 | `RelatedEntitiesSideScreen` | KButton × N, LocText | |
| 53 | `SummonCrewSideScreen` | KButton, LocText | |
| 54 | `ButtonMenuSideScreen` | KButton × N, LocText | Dynamic pool; re-walk after Enter |
| 55 | `ArtableSelectionSideScreen` | MultiToggle × N, KButton × 2 | Dynamic grid; re-walk after Enter |
| 56 | `CommandModuleSideScreen` | MultiToggle, LocText × N | Dynamic condition rows; re-walk |
| 57 | `MonumentSideScreen` | KButton × N | Dynamic buttons in container |
| 58 | `LaunchPadSideScreen` | KButton × N, LocText | Dynamic rows; HierarchyReferences |
| 59 | `TelepadSideScreen` | KButton, LocText × N | Dynamic condition rows |
| 60 | `AssignableSideScreen` | MultiToggle × N, LocText | Rows have dupe name in LocText |
| 61 | `ReceptacleSideScreen` | MultiToggle × N, KButton, LocText | Entity toggles via ReceptacleToggle wrapper (semantic gaps: entity metadata not paired with toggle) |
| 62 | `PlanterSideScreen` | MultiToggle × N, KButton, LocText | Same + subspecies toggles (semantic gaps: mutation metadata) |
| 63 | `IncubatorSideScreen` | MultiToggle × N, KButton, MultiToggle, LocText | Same + continuous toggle |
| 64 | `SpecialCargoBayClusterSideScreen` | MultiToggle × N, KButton, LocText | Same as ReceptacleSideScreen |
| 65 | `FilterSideScreen` | MultiToggle × N, LocText | Category toggles expand/collapse; re-walk |
| 66 | `SingleItemSelectionSideScreen` | MultiToggle × N, LocText | Same pattern as FilterSideScreen |
| 67 | `CargoModuleSideScreen` | LocText (via GenericUIProgressBar.label) | Dynamic panels; display only |
| 68 | `HarvestModuleSideScreen` | LocText (via GenericUIProgressBar.label) | Display only |

#### Category B: Needs walker enhancement (2 screens)

Standard widgets in hierarchy, but uses one widget type not in the current walker list.

| # | Screen | Issue | Enhancement |
|---|--------|-------|-------------|
| 69 | `AlarmSideScreen` | KInputField for name/tooltip text | Add KInputField to walker's widget list |
| 70 | `PixelPackSideScreen` | KButton color swatches in nested containers | Recursive walk finds them; color semantics lost without dedicated logic |

#### Category C: Custom interactive components — needs dedicated logic (6 screens)

Key interactive elements use non-standard components that the walker can't read or activate. Each needs per-screen reading code.

| # | Screen | Custom Component | What Fails |
|---|--------|-----------------|------------|
| 71 | `ClusterDestinationSideScreen` | `DropDown` | Destination selection. Walker finds KButton (change/clear/repeat) and LocText (info labels), but the DropDown for picking a destination is not a standard widget |
| 72 | `CheckboxListGroupSideScreen` | `Image.enabled` as checkbox | No click handler via standard widgets. Check state stored as `Image.enabled` property. Walker finds LocText labels but can't interact with checkboxes |
| 73 | `ModuleFlightUtilitySideScreen` | `DropDown`, `CrewPortrait` | Walker finds KButtons for actions, but DropDown for crew assignment and CrewPortrait for display are custom |
| 74 | `TagFilterScreen` | `KTreeControl`, `KTreeItem` | Entirely custom tree widget with recursive tree items. No standard widgets. Similar to `TreeFilterableSideScreen` but different component |
| 75 | `BionicSideScreen` | `BionicSideScreenUpgradeSlot` | Custom slot components for upgrade management. Also opens secondary `OwnablesSecondSideScreen` |
| 76 | `OwnablesSidescreen` | `OwnablesSidescreenCategoryRow`, `OwnablesSidescreenItemRow` | Custom row components for ownable items. Also opens secondary `OwnablesSecondSideScreen` |

#### Category D: Hierarchical — needs multi-level navigation (4 screens)

Generic walking doesn't capture the semantics. Need dedicated reading logic using NestedMenuHandler's multi-level navigation (sections at level 0, items at level 1).

| # | Screen | Structure |
|---|--------|-----------|
| 77 | `TreeFilterableSideScreen` | Master "all" toggle, then sections per category (category name + state at level 0, element toggles at level 1) |
| 78 | `ComplexFabricatorSideScreen` | Sections per recipe category (category name at level 0, recipe toggles at level 1). Also opens secondary `SelectedRecipeQueueScreen` |
| 79 | `AccessControlSideScreen` | Sections per group (Standard/Bionic/Robot at level 0, per-dupe permission toggles at level 1) |
| 80 | `MinionTodoSideScreen` | Sections per priority class (6 basic + 3 special at level 0, individual chore entries at level 1) |

#### Category E: Secondary screens (5 screen pairs)

These open a second side screen panel via `DetailsScreen.SetSecondarySideScreen`. The main view of some is handled by earlier categories; the secondary screen needs its own reading logic.

| # | Main Screen (main view category) | Secondary Screen |
|---|----------------------------------|-----------------|
| — | `ComplexFabricatorSideScreen` (D) | `SelectedRecipeQueueScreen` — recipe queue management |
| — | `BionicSideScreen` (C) | `OwnablesSecondSideScreen` — upgrade slot picker |
| — | `OwnablesSidescreen` (C) | `OwnablesSecondSideScreen` — item assignment picker |
| 81 | `AssignPilotAndCrewSideScreen` (A, main view: KButton + LocText) | `AssignmentGroupControllerSideScreen` — crew selection |
| 82 | `RocketModuleSideScreen` (A, main view: KButton × 5 + LocText) | `SelectModuleSideScreen` — module picker |

#### Tab-body panels (not SideScreenContent)

- **`DetailsScreenMaterialPanel`** (Material tab) — Shows current building material (name, mass, description, property modifiers) + a "Change Material" button that expands a `MaterialSelectionPanel` with material dropdown/grid + confirm button.
- **`CosmeticsPanel`** (Blueprints tab) — Facade/outfit selection. Category switching with selection grids.

These are the bodies of their respective `SidescreenTab`, not entries in `DetailsScreen.sideScreens`. They need direct readers.

## Phasing

### Phase 1: Skeleton + Properties Tab ✓

**Goal**: Handler lifecycle works. Tab cycling works. One real tab produces content.

- `DetailsScreenHandler` extending `NestedMenuHandler` with Show patch, target detection, tab cycling
- Two-level navigation: level 0 = section headers, level 1 = items within sections
- Level-aware type-ahead search across all section items
- `IDetailTab` interface with `Populate(target, List<DetailSection>)`
- `PropertiesTab` reader for `AdditionalDetailsPanel` (simplest uniform structure)
- `StubTab` for the other 7 that returns `IsAvailable = false`
- Tab names read from game's `UI.DETAILTABS` strings (no mod strings needed except handler display name)
- Registration in `ContextDetector.RegisterMenuHandlers()`

**Why Properties tab first**: All sections use the same `CollapsibleDetailContentPanel` → `DetailLabel` pattern. Proves the full pipeline without fighting complex UI.

### Phase 2: SimpleInfoScreen (Status Tab) ✓

**Goal**: The default landing tab works.

- Status items (walk `statusItemsFolder` children)
- Storage panel (walk `StoragePanel.Content` — `DetailLabel`, `DetailLabelWithButton`, `DetailCollapsableLabel`). Collapsable groups become nested sections: group summary as section header, drill in for individual items.
- Vitals panel (investigate `MinionVitalsPanel` structure)
- Effects and requirements (`DescriptorPanel` children)
- Stress, fertility, info, process conditions
- World panels (traits, biomes, geysers, etc.)

This is the largest single phase. Can be split into sub-phases if needed (status items first, then storage, then the rest).

### Phase 3: Personality + Chores Tabs ✓

**Goal**: Dupe and building-specific info tabs.

- `MinionPersonalityPanel` reader (traits, attributes, equipment, skills)
- `BuildingChoresPanel` reader (chore list with performers)

### Phase 4: Side Screen Infrastructure + Generic Walker ✓

**Goal**: Side screen tabs work. 68 side screens (Category A) are accessible automatically, plus 2 more (Category B) with minor walker enhancements.

- `ConfigSideTab` and `ErrandsSideTab` readers that enumerate active `SideScreenRef` entries for their tab type via `GetActiveScreens()` (shared helper)
- `SideScreenWalker` static utility: recursively walks active child GameObjects for KSlider/KToggle/MultiToggle/KNumberInputField/KInputField/KButton/LocText, emitting `WidgetInfo` with `SpeechFunc` using `GetParsedText()` for all LocText reads
- `IDetailTab.OnTabSelected()` method for side screen tab switching (clicks the game's `SidescreenTab.tabInstance` MultiToggle)
- `DetailsScreenHandler.ActivateLeafItem` extended: Toggle (KToggle.Click, ClickMultiToggle), Slider (re-speak), TextInput (TextEditHelper Begin/Confirm)
- `DetailsScreenHandler.HandleLeftRight` override: slider value adjustment at leaf level with step sizing and boundary sounds
- TextEdit integration: Enter to confirm, Escape to cancel, navigation suppressed while editing
- Re-walk after Enter handled by existing Populate-on-every-keypress flow

This covers sliders, toggles, buttons, radio groups, dynamic lists, selection grids, progress displays, and more — all without per-screen code. Some Category A screens have minor semantic gaps (entity metadata not paired with toggles in ReceptacleSideScreen family, category fold state in FilterSideScreen) but provide functional access.

**Not covered by Phase 4** (12 unique screens across Categories C/D/E):
- 6 custom-component screens that need per-screen readers (Category C)
- 4 hierarchical screens that need multi-level navigation (Category D)
- 2 screens whose main view works but whose secondary screens need logic (Category E only: `AssignPilotAndCrewSideScreen`, `RocketModuleSideScreen`)
- 3 screens already counted in C/D also have secondary screens: `ComplexFabricatorSideScreen`, `BionicSideScreen`, `OwnablesSidescreen`

### Phase 5: Material + Blueprints Tab Bodies

**Goal**: The two non-SideScreenContent tab bodies are accessible.

- `MaterialSideTab` — read `DetailsScreenMaterialPanel` directly (current material label + description + change button + material selection panel when expanded)
- `BlueprintsSideTab` — read `CosmeticsPanel` directly (facade/outfit selection)

These are tab bodies, not entries in `sideScreens`, so they need direct readers rather than the generic walker.

### Phase 6: Hierarchical Side Screens

**Goal**: The 4 hierarchical side screens (Category D) get dedicated multi-level reading logic.

Each needs NestedMenuHandler's multi-level navigation (sections at level 0, items at level 1):

1. `TreeFilterableSideScreen` — master toggle, then sections per category with element toggle children.
2. `AccessControlSideScreen` — sections per group with per-dupe permission row children.
3. `MinionTodoSideScreen` — sections per priority group with chore entry children.
4. `ComplexFabricatorSideScreen` — sections per category with recipe toggle children.

### Phase 7: Custom-Component Side Screens

**Goal**: The 6 side screens with non-standard interactive components (Category C) get per-screen readers.

Each uses custom widget types the generic walker can't handle:

1. `ClusterDestinationSideScreen` — DropDown for destination picking. Walker already reads the KButton/LocText parts; dedicated reader adds DropDown interaction.
2. `CheckboxListGroupSideScreen` — checkboxes are `Image.enabled`, not toggles. Needs custom read of check state and click dispatch.
3. `ModuleFlightUtilitySideScreen` — DropDown for crew, CrewPortrait. Walker reads KButtons; dedicated reader adds DropDown.
4. `TagFilterScreen` — KTreeControl/KTreeItem custom tree widget. Similar problem to `TreeFilterableSideScreen` but different component.
5. `BionicSideScreen` — BionicSideScreenUpgradeSlot custom components for main view.
6. `OwnablesSidescreen` — OwnablesSidescreenCategoryRow/ItemRow custom components for main view.

### Phase 8: Secondary Side Screens

**Goal**: The 5 secondary screen pairs (Category E) are accessible.

Secondary screens are opened via `DetailsScreen.SetSecondarySideScreen` when the user activates certain elements. UX for secondary screen navigation to be designed during this phase.

1. `ComplexFabricatorSideScreen` → `SelectedRecipeQueueScreen` (recipe queue management)
2. `BionicSideScreen` → `OwnablesSecondSideScreen` (upgrade slot picker)
3. `OwnablesSidescreen` → `OwnablesSecondSideScreen` (item assignment picker)
4. `AssignPilotAndCrewSideScreen` → `AssignmentGroupControllerSideScreen` (crew selection)
5. `RocketModuleSideScreen` → `SelectModuleSideScreen` (module picker)

### Phase 9: User Menu Overlay

**Goal**: Entity actions are accessible via hotkey.

- Dedicated key opens an action overlay while the details screen is active
- Read live from `UserMenuScreen`'s button list
- Up/Down navigates actions, Enter activates, Escape closes overlay
- Dynamic — buttons change per entity type and state

## Known Issues / Lessons Learned

### TMPro SetText() vs .text divergence

`LocText` extends `TextMeshProUGUI`. The game uses two paths to set text:
- `.text =` — updates `m_text`. Reading `.text` back works.
- `SetText(string)` — updates TMPro's internal render buffer but not `m_text`. Reading `.text` returns empty string or stale content.

**Always use `GetParsedText()`** to read LocText values. It reads from the render buffer and works regardless of how the text was set.

**Exception**: `GetParsedText()` returns prefab placeholder text for components that were previously inactive (mesh never regenerated with real content). In those cases, call the game's data function directly (e.g., `label_text_func` for `MinionVitalsPanel.CheckboxLine`).

**Where this bit us in Phase 2** (StatusTab.cs):
- Vital amount/attribute lines — `MinionVitalsPanel.TryUpdate()` uses `locText.SetText()`
- Storage group headers — `SimpleInfoScreen` uses `nameLabel.SetText()` / `valueLabel.SetText()`
- Storage child rows — `SimpleInfoScreen` uses `label.SetText()` / `label2.SetText()` / `label3.SetText()`
- World panel rows — `SimpleInfoScreen.RefreshWorldPanel()` uses `SetText()` on name/value/description labels
- Checkbox condition lines — inactive at discovery time, `GetParsedText()` returned prefab text; fixed by calling `label_text_func(target)` directly

**Future phase risk**:
- ~~Phase 3 (Personality, Chores): LOW. Both panels use `SetLabel()` or `.text =` directly. No `SetText()` usage.~~ Confirmed — no issues.
- ~~Phase 4 (Side screen walker): HIGH.~~ Mitigated — `SideScreenWalker` uses `GetParsedText()` for all LocText reads. Three display-only screens (`ProgressBarSideScreen`, `CargoModuleSideScreen`, `HarvestModuleSideScreen`) use `GenericUIProgressBar.label` which may hit the inactive-prefab-text variant; verify in-game.
- Phase 5+ (Side screens): HIGH. Dedicated readers for Categories C/D/E will need `GetParsedText()` consistently. Affects Phases 5 through 8.

## Open Questions

- ~~**MinionVitalsPanel structure**~~: **Resolved.** Extends `CollapsibleDetailContentPanel`. Uses `AmountLine`/`AttributeLine`/`CheckboxLine` structs, each with a `LocText` + `ToolTip` on a `GameObject`. `Refresh()` shows/hides lines per entity. Same reading strategy as `AdditionalDetailsPanel`: walk active `Content` children, read `locText.text` and `toolTip`.
- ~~**MinionPersonalityPanel structure**~~: **Resolved.** Six `CollapsibleDetailContentPanel` sections (bio, traits, attributes, resume, amenities, equipment), all populated via `SetLabel(id, text, tooltip)` + `Commit()`. Same reading strategy as `AdditionalDetailsPanel`.
- ~~**User menu hotkey**~~: **Resolved.** `]` (right bracket). Overwrites Building Utility 3, which is a context-dependent building action that would itself appear in the user menu. No screen reader conflict.
