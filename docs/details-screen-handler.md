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

The game's right-click context menu (deconstruct, cancel, priority, enable/disable, etc.) is accessed via a **separate hotkey overlay**, not as a tab. A dedicated key (`]` right bracket) opens the action list as a temporary overlay while the details screen is active. Escape or activating an action closes the overlay and returns to tab navigation. Overwrites Building Utility 3 (a context-dependent building action that would itself appear in the user menu). No screen reader conflict.

The user menu is populated dynamically per entity — components register buttons sorted by priority constants. The overlay reads live from `UserMenuScreen`'s button list.

## What's Built (Phases 1–4)

### Handler core

`DetailsScreenHandler` extends `NestedMenuHandler`. Show patch on `DetailsScreen.OnShow(bool)` pushes/pops the handler (registered in `_showPatchedTypes` so generic `KScreen.Activate` patch skips it). Target change detection in `Tick()` compares `DetailsScreen.Instance.target` to `_lastTarget`; on change, rebuilds `_activeTabs`, resets to tab 0, repopulates, speaks entity name + tab name + first section.

### IDetailTab interface

```csharp
interface IDetailTab {
    string DisplayName { get; }
    string GameTabId { get; }  // game's DetailTabHeader ID (null for side screens)
    int StartLevel { get; }    // 0 = root (section headers), 1 = drilled into first section
    bool IsAvailable(GameObject target);
    void Populate(GameObject target, List<DetailSection> sections);
    void OnTabSelected();  // clicks game's SidescreenTab for side screen tabs
}

class DetailSection {
    public string Header;
    public readonly List<Widget> Items = new List<Widget>();
}
```

`Populate` (via `RebuildSections()`) is called on **every state-changing action** — tab switches, section changes, target changes, toggle activations, slider adjustments, and text input confirmations. This guarantees the user never hears stale data after interacting. Cursor stability across refreshes: track the current widget by its underlying Unity component reference (not index), find it in the new list after re-Populate, clamp to nearest valid index if gone.

### Informational tab readers (tabs 0–3)

All four main info tabs read live from the game's UI panels:

- **PropertiesTab** (tab 3): Walks `AdditionalDetailsPanel`'s `CollapsibleDetailContentPanel` fields. Each active panel → section header, then `DetailLabel` children → items (label.text for speech, toolTip.toolTip for tooltip).
- **StatusTab** (tab 0): Most complex. Reads status items from `statusItemsFolder` children (Button if clickable, Label if not), storage from `StoragePanel.Content` (collapsable groups become nested sections), vitals from `MinionVitalsPanel` (AmountLine/AttributeLine/CheckboxLine), effects/requirements from `DescriptorPanel`, stress/fertility/info/process-conditions/world-panels from their respective `CollapsibleDetailContentPanel` containers.
- **PersonalityTab** (tab 1): Reads `MinionPersonalityPanel`'s six `CollapsibleDetailContentPanel` sections (bio, traits, attributes, resume, amenities, equipment) via same `SetLabel`/`Commit` pattern as Properties.
- **ChoresTab** (tab 2): Reads `BuildingChoresPanel` chore rows. Each `BuildingChoresPanelDupeRow` is a Button — Enter calls `GameUtil.FocusCamera()` on that dupe.

### Side screen infrastructure (tabs 4–7)

`ConfigSideTab` and `ErrandsSideTab` enumerate active `SideScreenRef` entries for their tab type via `GetActiveScreens()` (shared helper walking `DetailsScreen.sideScreens`).

**SideScreenWalker** recursively walks active child GameObjects for KSlider, KToggle, MultiToggle, KNumberInputField, KInputField, KButton, LocText, ReceptacleToggle, SingleItemSelectionRow. Uses `GetParsedText()` for all LocText reads. Features:

- HierarchyReferences container detection: "HeaderLabel"+"GridLayout" (receptacle categories) and "Label"+"Entries" (selection categories) emit drillable parent widgets
- Radio-style KToggle group collapse into Dropdown widgets
- Per-screen special handling: `CollapseAlarmTypeButtons()` (AlarmSideScreen), `CollapseFewOptionRows()` (FewOptionSideScreen), `WalkPixelPackScreen()` (PixelPackSideScreen), PlayerControlledToggleSideScreen toggle state
- Selection category support via `TryAddSelectionCategoryContainer()` for FilterSideScreen / SingleItemSelectionSideScreen

**Widget interaction** in `DetailsScreenHandler`: Toggle (KToggle.Click, ClickMultiToggle), Slider (HandleLeftRight for value adjustment with step sizing and boundary sounds), TextInput (TextEditHelper Begin/Confirm, Enter to confirm, Escape to cancel, navigation suppressed while editing). Re-walk after Enter handled by existing Populate-on-every-keypress flow.

**ErrandsSideTab** has dedicated handling for `MinionTodoSideScreen` with snapshot-based speech (prevents stale data from continuous re-sorting).

This covers **70 of 82** player-visible side screens (68 Category A + 2 Category B with walker enhancements).

## Remaining Side Screens (12 screens)

### Category C: Custom interactive components (4 screens)

Key interactive elements use non-standard components the walker can't read or activate. Each needs per-screen reading code.

| # | Screen | Custom Component | What Fails |
|---|--------|-----------------|------------|
| 71 | `ClusterDestinationSideScreen` | `DropDown` | Destination selection. Walker finds KButton (change/clear/repeat) and LocText (info labels), but the DropDown opens in the overlay canvas — not in the side screen hierarchy |
| 72 | `CheckboxListGroupSideScreen` | `Image.enabled` as checkbox | No click handler via standard widgets. Check state stored as `Image.enabled` property. Walker finds LocText labels but can't interact with checkboxes |
| 73 | `ModuleFlightUtilitySideScreen` | `DropDown`, `CrewPortrait` | Walker finds KButtons for actions, but DropDown for crew assignment opens in overlay canvas (same issue as ClusterDestination) |
| 74 | `TagFilterScreen` | `KTreeControl`, `KTreeItem` | Entirely custom tree widget with recursive tree items. No standard widgets at all. Walker finds nothing actionable |

### Category D: Hierarchical — needs multi-level navigation (5 screens, 1 done)

Generic walking finds the individual widgets but doesn't capture the semantic grouping. Need dedicated reading logic using NestedMenuHandler's multi-level navigation (sections at level 0, items at level 1).

| # | Screen | Structure | Status |
|---|--------|-----------|--------|
| 75 | `TreeFilterableSideScreen` | Master "all" toggle, then sections per category (category name + state at level 0, element toggles at level 1). Walker misses category rows entirely (custom `TreeFilterableSideScreenRow` pooled objects) | |
| 76 | `ComplexFabricatorSideScreen` | Sections per recipe category (category name at level 0, recipe toggles at level 1). Walker finds KToggles but no category headers (implicit in Dictionary, not rendered). Also opens secondary `SelectedRecipeQueueScreen` | |
| 77 | `AccessControlSideScreen` | Sections per group (Standard/Bionic/Robot at level 0, per-dupe permission toggles at level 1). Walker finds MultiToggles but no section grouping | |
| 78 | `MinionTodoSideScreen` | **Done.** Handled by `ErrandsSideTab` with snapshot-based speech | |
| 79 | `BionicSideScreen` | Upgrade slots grouped by category. Walker finds MultiToggle + LocText but loses category structure. Also opens secondary `OwnablesSecondSideScreen` | |
| 80 | `OwnablesSidescreen` | Items grouped by category (Suit/Outfit/Bed/etc.). Walker finds MultiToggle + LocText but flattens hierarchy. Also opens secondary `OwnablesSecondSideScreen` | |

### Category E: Secondary screens (5 screen pairs)

These open a second side screen panel via `DetailsScreen.SetSecondarySideScreen`. The main view of some is handled by earlier categories; the secondary screen needs its own reading logic.

| Main Screen (category) | Secondary Screen |
|------------------------|-----------------|
| `ComplexFabricatorSideScreen` (D) | `SelectedRecipeQueueScreen` — recipe queue management |
| `BionicSideScreen` (D) | `OwnablesSecondSideScreen` — upgrade slot picker |
| `OwnablesSidescreen` (D) | `OwnablesSecondSideScreen` — item assignment picker |
| `AssignPilotAndCrewSideScreen` (A) | `AssignmentGroupControllerSideScreen` — crew selection (MultiToggle rows, walker finds widgets but loses available/off-world grouping) |
| `RocketModuleSideScreen` (A) | `SelectModuleSideScreen` — module picker (MultiToggle grid with category headers, plus MaterialSelectionPanel/FacadeSelectionPanel custom components) |

### Tab-body panels (not SideScreenContent) — Done (Phase 5)

- **`DetailsScreenMaterialPanel`** (Material tab) — Done. `MaterialTab` reads directly.
- **`CosmeticsPanel`** (Blueprints tab) — Done. `BlueprintTab` reads directly.

## What's Built (Phase 5)

### Tab-body panel readers (tabs 6–7)

- **MaterialTab** (tab 6): Reads `DetailsScreenMaterialPanel` directly via Traverse. Shows current material name, description, and property modifiers. Change button expands `MaterialSelectionPanel` with material toggles (Tag → MultiToggle, speech includes mass and selected state) and order button.
- **BlueprintTab** (tab 7): Reads `CosmeticsPanel` directly via Traverse. Two modes detected by `MinionIdentity` presence:
  - **Building mode**: Single section with current facade name + description labels, then facade selection toggles from `FacadeSelectionPanel.activeFacadeToggles` (private struct accessed via `IDictionary` + Traverse on boxed values). Toggle names from `ToolTip` (first line only — full tooltip contains name + description).
  - **Dupe mode**: Info section with outfit name, edit button, and outfit category toggles (Clothing/Atmo Suit/Jet Suit mapped to `BLUEPRINT_TAB.SUBCATEGORY_*` game strings — LocText `.text`/`.GetParsedText()` returns prefab placeholder for dynamically recreated buttons). Second section headed by selected category name with outfit selection grid.

## Remaining Phases

### Phase 6: Hierarchical Side Screens (1 of 5 done)

**Goal**: The remaining 4 hierarchical side screens (Category D) get dedicated multi-level reading logic.

Each needs NestedMenuHandler's multi-level navigation (sections at level 0, items at level 1):

1. `TreeFilterableSideScreen` — master toggle, then sections per category with element toggle children. Walker misses category rows entirely (custom `TreeFilterableSideScreenRow`).
2. `AccessControlSideScreen` — sections per group with per-dupe permission row children. Walker finds MultiToggles but not the Standard/Bionic/Robot grouping.
3. `ComplexFabricatorSideScreen` — sections per category with recipe toggle children. Walker finds KToggles but no category headers.
4. `BionicSideScreen` — upgrade slots grouped by category. Walker finds MultiToggle+LocText but loses category structure. Also opens secondary screen.
5. `OwnablesSidescreen` — items grouped by category. Walker finds MultiToggle+LocText but flattens hierarchy. Also opens secondary screen.

### Phase 7: Custom-Component Side Screens

**Goal**: The 4 side screens with non-standard interactive components (Category C) get per-screen readers.

1. `ClusterDestinationSideScreen` — DropDown for destination picking. Walker reads KButton/LocText parts; dedicated reader adds DropDown interaction (opens in overlay canvas, not in side screen hierarchy).
2. `CheckboxListGroupSideScreen` — checkboxes are `Image.enabled`, not toggles. Needs custom read of check state and click dispatch.
3. `ModuleFlightUtilitySideScreen` — DropDown for crew assignment (same overlay canvas issue as ClusterDestination). Walker reads KButtons; dedicated reader adds DropDown.
4. `TagFilterScreen` — KTreeControl/KTreeItem custom tree widget. Walker finds nothing actionable. Entirely custom tree system.

### Phase 8: Secondary Side Screens

**Goal**: The 5 secondary screen pairs (Category E) are accessible.

Secondary screens are opened via `DetailsScreen.SetSecondarySideScreen` when the user activates certain elements. UX for secondary screen navigation to be designed during this phase.

1. `ComplexFabricatorSideScreen` → `SelectedRecipeQueueScreen` (recipe queue management)
2. `BionicSideScreen` → `OwnablesSecondSideScreen` (upgrade slot picker). Note: main view is Phase 6 (Cat D).
3. `OwnablesSidescreen` → `OwnablesSecondSideScreen` (item assignment picker). Note: main view is Phase 6 (Cat D).
4. `AssignPilotAndCrewSideScreen` → `AssignmentGroupControllerSideScreen` (crew selection — MultiToggle rows, walker finds widgets but loses available/off-world grouping)
5. `RocketModuleSideScreen` → `SelectModuleSideScreen` (module picker — MultiToggle grid with category headers, plus MaterialSelectionPanel/FacadeSelectionPanel custom components)

### Phase 9: Actions Section — Done

**Goal**: Entity actions are accessible via Ctrl+Tab section.

`ActionsTab` implements `IDetailTab` as a third Ctrl+Tab section (after main info tabs and side screen tabs). One flat section containing:

1. **User menu buttons** — read live from `UserMenuScreen.buttonInfos` via Traverse. Each becomes a `UserMenuButtonWidget` that fires `ButtonInfo.onClick` directly. Stale-target guard compares `UserMenuScreen.selected` to `DetailsScreen.target` to prevent reading old-entity buttons during target transitions.
2. **Priority widget** — when entity has `Prioritizable`. `PriorityWidget` is adjustable via Left/Right across basic 1-9 and emergency. Speech uses existing `PRIORITY_BASIC`/`PRIORITY_EMERGENCY` mod strings. Logs unrecognized priority values.
3. **Title bar buttons** — Codex Entry (`CodexEntryButton`), Pin Resource (`PinResourceButton`), Rename (`editNameButton`), Random Name (`randomNameButton`). Each only shown when its `GameObject.activeInHierarchy` is true.

Section grouping in `RebuildActiveTabs` extended from two-way (game tab vs side screen) to three-way via `GetTabSectionKind()` type-check. Empty Actions tab speaks "No actions".

## Known Issues / Lessons Learned

### TMPro SetText() vs .text divergence

`LocText` extends `TextMeshProUGUI`. The game uses two paths to set text:
- `.text =` — updates `m_text`. Reading `.text` back works.
- `SetText(string)` — updates TMPro's internal render buffer but not `m_text`. Reading `.text` returns empty string or stale content.

**Always use `GetParsedText()`** to read LocText values. It reads from the render buffer and works regardless of how the text was set.

**Exception 1**: `GetParsedText()` returns prefab placeholder text for components that were previously inactive (mesh never regenerated with real content). In those cases, call the game's data function directly (e.g., `label_text_func` for `MinionVitalsPanel.CheckboxLine`).

**Exception 2**: Components that are destroyed and recreated each `Refresh()` (e.g., `CosmeticsPanel.outfitCategories`) have LocText children where neither `.text` nor `GetParsedText()` returns real content on the same frame — TMP hasn't parsed yet and `.text` returns the prefab placeholder (e.g., "_label"). Map to known game strings instead of reading the LocText.
