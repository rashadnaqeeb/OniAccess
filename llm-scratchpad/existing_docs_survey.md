# Existing Documentation Survey — OniAccess

Survey conducted 2026-02-27.

---

## 1. docs/ directory — Overview

Six files exist:

| File | Purpose |
|---|---|
| `CODEBASE_INDEX.md` | Namespace reference for decompiled ONI source |
| `oni_accessibility_audit.md` | Exhaustive catalog of every ONI UI element |
| `factorio-access concepts.md` | Design reference synthesizing FactorioAccess architecture for ONI |
| `hotkey-reference.md` | Complete ONI key binding map with mod assignments |
| `inspection-panels.md` | Deep-dive into DetailsScreen/side screen architecture |
| `details-screen-handler.md` | Implementation plan for DetailsScreenHandler |

---

## 2. docs/CODEBASE_INDEX.md — Assessment

**Length**: ~528 lines. Covers both Assembly-CSharp (~4,194 files) and Assembly-CSharp-firstpass (~2,444 files).

**What it covers well:**

- All named namespaces in Assembly-CSharp (`Klei.AI`, `Klei.AI.DiseaseGrowthRules`, `Database`, `Klei.CustomSettings`, `Klei.Actions`, `Klei.Input`, `Klei`, `KMod`, `ProcGen`, `ProcGenGame`, `Rendering`, `Rendering.World`, `STRINGS`, `TUNING`, `TemplateClasses`, `ImGuiObjectDrawer`, `FoodRehydrator`, `EventSystem2Syntax`, `UnityEngine.EventSystems`)
- All named namespaces in Assembly-CSharp-firstpass with class-level detail
- High-level categorization of the ~3,700+ global-namespace files grouped by function: building configs, creature configs, item configs, UI screens, side screens, building components, game systems, state machines, logic/automation, core utilities
- Third-party libraries (ClipperLib, Delaunay, FuzzySharp, YamlDotNet, etc.)
- Platform SDKs (Rail, Epic.OnlineServices) marked as not relevant for modding
- A "Key Patterns for Modders" summary table

**What it does NOT cover:**

- Per-class method signatures. It lists class names only, not what methods/properties each class exposes.
- The global namespace classes are summarized by category with about 10-15 representative examples each, then "~N more". So anything beyond the listed examples requires searching the decompiled files directly.
- No field-level documentation on any class.
- No cross-references showing which classes use which others.

**Verdict**: Good as a discovery tool — answers "does class X exist and which file is it in?" and "what namespace does Y live in?". Not a substitute for reading the actual decompiled source when you need method signatures, field names, or event hashes. Coverage of the global namespace is by representative sample, not exhaustive.

---

## 3. docs/oni_accessibility_audit.md — Assessment

**Length**: ~677 lines. Ordered walkthrough of every player-visible UI element in the game.

**What it covers:**

- Every screen and panel in encounter order: launch, main menu, options, load game, colony summaries, new game flow (mode select, asteroid select, game settings, duplicant selection), full gameplay HUD (top bar, overlay buttons, resource panel, time controls), bottom bar (build menu with all categories, tool commands), notification system, game world grid, detail panels (building, duplicant, critter, plant, geyser, tile, debris), all full-screen management screens (research, skills, codex), printing pod, automation system (overlay, sensors, logic gates), rocketry and starmap (base game and Spaced Out DLC), sandbox mode, building placement interaction model, room system, pause menu, Bionic Booster Pack DLC additions, story traits and events
- Phase 15 provides a consolidated inventory of all UI types grouped by interaction pattern (full-screen modals, HUD panels, overlays, detail panel variants, management grids, spatial interactions, recurring popups)
- Marks DLC-exclusive elements

**Quality**: High. This is a thorough reference for scope. Every interactive element that needs to be made accessible is listed here. It describes *what exists*, not how to access it in code — that is its intended scope.

**Gaps**: No code references. Does not map UI elements to their game class names. That mapping lives in `inspection-panels.md` and the decompiled source.

---

## 4. docs/factorio-access concepts.md — Assessment

**Length**: ~696 lines. A design reference document in ten parts.

**What it covers:**

- Core design philosophy (audio-first, five lessons from FactorioAccess)
- Screen reader UX principles (variable information first, terse, consistent phrasing, hierarchical drill-down)
- Technical foundation: platform comparison, what translates directly from FactorioAccess to C#/Unity, world model divergence
- Core architecture: information pipeline, MessageBuilder state machine with C# translation, entity description handler chain pattern, graph-based UI navigation (KeyGraph), tab system, event management with priorities, state management
- Scanner design: three-level hierarchy, ONI category mapping, backend polymorphism and clustering, performance strategies
- Audio systems: speech output, spatial audio, sonification, audio overload prevention, sound design
- ONI-specific systems: cursor and navigation, overlay and environmental awareness system (the three-layer design: togglable speech overlays, atmospheric sonification, game-state problem detection), duplicant management, building and construction, rooms, colony management screens, resource and inventory differences
- Concept-by-concept mapping: direct mappings, adapted concepts, entirely new inventions required
- Testing strategy and quality
- Development phases (1-6)
- Suggested project architecture (directory tree)
- Appendix: 18 key principles

**Quality**: This is the highest-quality document in the project. It is a complete design reference that an LLM could use to understand *why* the mod is architected the way it is and *what* it is trying to accomplish. The C# code examples are concrete and complete.

**Relationship to current code**: The mod's actual implementation tracks this design closely but has made specific concrete decisions (e.g., `HandlerStack` instead of `UIRouter`, `SpeechPipeline` instead of a raw `Speech.Speak()`, `IAccessHandler`/`BaseScreenHandler` instead of the generic interfaces). The document is more of a founding design spec than a current architecture description.

---

## 5. docs/hotkey-reference.md — Assessment

**Length**: ~233 lines. Complete hotkey map.

**What it covers:**

- ONI key binding groups (Root, Navigation, Tool, BuildMenu, Building, Sandbox, Debug, CinemaMode, Management)
- All critical game bindings with notes on mod conflict risk
- Camera/navigation bindings and why WASD is off-limits for the mod cursor
- Management screen hotkeys (V, F, L, ., J, R, E, U, Z, N)
- Tool command keys (all marked safe to overwrite)
- Building interaction keys
- Build menu keys
- Cinema mode, debug mode, Steam overlay keys
- Keys NOT used by ONI — confirms arrow keys are free, page up/down are free
- Screen reader keys to avoid (Insert/CapsLock as NVDA/JAWS modifiers)
- Mod hotkeys assigned so far (toggle, context help, read coords, cycle coord mode, read tooltip, browse tooltip, open tool menu, user menu)
- Known key conflict table

**Quality**: Excellent operational reference. Well-maintained — the "Mod Hotkeys Assigned So Far" table is specific and actionable. The screen reader avoidance notes are critical safety information.

---

## 6. docs/inspection-panels.md — Assessment

**Length**: ~371 lines. Deep-dive into the DetailsScreen architecture.

**What it covers:**

- Selection flow (SelectTool -> DetailsScreen.Refresh)
- DetailsScreen layout: title bar, main tabs (SIMPLEINFO/PERSONALITY/BUILDINGCHORES/DETAILS), side screen tab categories (Config, Errands, Material, Blueprints)
- UserMenuScreen mechanics
- Detailed coverage of each main tab: SimpleInfoScreen sections (status items, storage, vitals, effects, stress, fertility, world traits, process conditions), AdditionalDetailsPanel (properties, disease, immune system, energy), MinionPersonalityPanel, BuildingChoresPanel
- Complete side screen inventory: 87 concrete classes, organized by entity type, with class hierarchy noting which ones are never shown and why
- Interface-driven generic patterns (ISingleSlider -> SingleSliderSideScreen, etc.)
- What each entity type shows (duplicant, building completed, building under construction, creature, item/debris, tile, plant)
- Codex entry button logic
- Data flow diagram showing which game components feed which panels
- Key event hashes (-1503271301 SelectObject, -1514841199 RefreshUserMenu, etc.)
- Key source files with line counts

**Quality**: Very high. This is a deep technical reference with actual class names, field names, event hashes, and line numbers. The kind of document that makes Harmony patch development practical without constant decompiled-source archaeology.

---

## 7. docs/details-screen-handler.md — Assessment

**Length**: ~199 lines. Implementation plan for DetailsScreenHandler.

**What it covers:**

- Design summary: two-level nested navigation, tab reader architecture
- Full tab list with game string references, source panel classes, what entity types they show for
- User menu design (bracket key overlay)
- What is built (Phases 1-5): handler core, IDetailTab interface, all four informational tabs (PropertiesTab, StatusTab, PersonalityTab, ChoresTab), side screen infrastructure (ConfigSideTab, ErrandsSideTab, SideScreenWalker with all its detection logic), tab-body panel readers (MaterialTab, BlueprintTab)
- Remaining work: Category C (4 screens with custom components), Category D (5 hierarchical screens, 1 done), Category E (5 secondary screen pairs)
- Remaining phases 6-9 with scoped goals
- Known issues: TMPro `SetText()` vs `.text` divergence, `GetParsedText()` as the correct read method, two exceptions to that rule

**Quality**: Excellent. This is an accurate implementation status document. The "Known Issues" section captures real gotchas discovered during implementation that would cause bugs if forgotten.

---

## 8. .planning/ directory

Two files, both about the schedule screen:

### .planning/schedule-screen-research.md

**Length**: ~468 lines. Pre-implementation research note.

**What it covers:**

- What a sighted player sees (complete UI walkthrough)
- Screen class hierarchy (ScheduleScreen, ScheduleScreenEntry, ScheduleBlockButton, ScheduleBlockPainter, ScheduleMinionWidget)
- Data model: Schedule, ScheduleBlock, ScheduleManager (with field lists and key method signatures)
- The 4 schedule groups (Hygene/Worktime/Recreation/Sleep) with their IDs, display names, colors, allowed activities
- The 5 block types (Sleep/Eat/Work/Hygiene/Recreation)
- UI component details: ScheduleScreenEntry fields, ScheduleBlockButton behavior, ScheduleBlockPainter drag mechanics, ScheduleMinionWidget two-mode operation
- Hour timing explanation (600 seconds/cycle, EarlyBird/NightOwl trait thresholds)
- Edge cases: delete-with-1-schedule silently does nothing, optionsButton declared but never wired, unused STRINGS
- Supporting components: EditableTitleBar (with correct data flow advice), DropDown (with both usage modes), MultiToggle
- Existing handler patterns (registration pattern, handler architecture pattern)
- Key interactions for accessibility (painting problem, hour block state access code, multi-timetable handling, input handling quirks)
- Complete relevant STRINGS with keys and values
- File references for both game code and mod code
- Analogous existing handlers to study

**Quality**: Thorough research that shows evidence of reading the actual decompiled code. The edge cases and "never wired" notes are exactly the kind of thing that prevents wasted implementation time. The presence of `OniAccessStrings.cs:512` as a reference confirms this was written after the strings were already added.

### .planning/schedule-screen-design.md

**Length**: ~167 lines. UX/keybinding design for the implemented schedule screen handler.

**What it covers:**

- Two-tab structure: Schedules tab and Duplicants tab
- Schedules tab: navigation (Up/Down rows, Left/Right cells, Home/End, Tab), cell announcement format, row header logic (when to announce schedule name), brush selection (number keys 1-4), painting (Space, Shift+Left/Right, Shift+Home/End), reordering (Ctrl+Up/Down schedule, Shift+Up/Down row, Ctrl+Left/Right rotate), schedule options submenu design (Enter opens, options table with Rename/Alarm/Duplicate/Delete/Add row/Delete row)
- Duplicants tab: layout, navigation (Up/Down, Left/Right to cycle assignment), announcement format (name, traits EarlyBird/NightOwl, schedule name)
- Activation announcement format
- Opening state (cursor position from current game hour, brush default)

**Quality**: Clean design spec. Maps directly to the implementation in `ScheduleScreenHandler.cs`, `SchedulesTab.cs`, and `DupesTab.cs`.

---

## 9. llm-docs/ directory

Does not exist. No llm-docs directory was found at `C:/Users/rasha/Documents/Oni-access/llm-docs/`.

---

## 10. OniAccessStrings.cs — Assessment

**Location**: `C:/Users/rasha/Documents/Oni-access/OniAccess/OniAccessStrings.cs`

**Length**: 734 lines. All mod-authored user-facing text.

**Namespaces covered (under STRINGS.ONIACCESS)**:

| Class | Content |
|---|---|
| `SPEECH` | Mod load/toggle messages, error format, no-commands fallback |
| `SEARCH` | Search cleared, no-match |
| `HOTKEYS` | Toggle mod label |
| `HANDLERS` | Display names for all screen handlers (Loading, Help, Main Menu, Pause Menu, Audio/Graphics/Game/Data Options, Colony Summary, World Gen, Minion Select, Save/Load, Mods, Translations, Key Bindings, Supply Closet, Item Drop, Welcome Message, Story Message, Video, Colony View, Tooltip Browser, Entity Picker, Entity Details) |
| `SUPPLY_CLOSET` | No items, offline |
| `HELP` | All generic help overlay strings (navigate, close, navigate items, jump, select, adjust values, type-search, switch panel/section, move cursor, read coords, coord mode, tooltip, game speed, select entity, open/back/jump group) |
| `HELP.TOOLS_HELP` | Tool-specific help (set corner, clear rect, confirm, cancel, set priority, filter, jump selection) |
| `PANELS` | Seed, Achievements, Victory conditions, Stats, Buttons, Planetoids, DLC, News, Colony name, Select duplicants, Rename, Shuffle name |
| `DLC` | Active, Owned not active, Not owned |
| `TEXT_EDIT` | Editing, Confirmed, Cancelled |
| `STATES` | Selected, guaranteed, forbidden, on, off, mixed, enabled, disabled, any, none, condition met/not met, input field, slider, available |
| `RECEPTACLE` | Item count |
| `BUTTONS` | Accept, Manage, View other colonies, Toggle all |
| `WORLD_GEN` | Complete, Percent |
| `INFO` | Difficulty, Story traits, Setting, Achievement, Interest, Interest filter, Trait, Positive/Negative trait, Bionic upgrade/bug, Slot |
| `SAVE_LOAD` | Save info, Convert cloud/local, Delete, Newest, Auto-save |
| `KEY_BINDINGS` | Unbound, Press key prompt, Reset all, Bindings reset |
| `TILE_CURSOR` | Unexplored, Coords format, Coord off/append/prepend, Overlay none, No room, Nothing to select, Select object |
| `VIDEO` | Playing |
| `GAME_STATE` | Unpaused, Cycle, Cycle status, Read cycle status |
| `TOOLTIP` | No tooltip, Closed |
| `TOOLS` | All tool-layer strings (picker name, filter name, item singular/plural, corner set, rect summary, selected, no change, canceled, confirm messages for dig/mop/disinfect/sweep/deconstruct/cancel/prioritize/harvest/attack/capture/empty pipe/disconnect, priority formats, order types, pipe contents, activation formats, help entries) |
| `TEMPERATURE` | Near freezing, Near boiling |
| `GLANCE` | All tile-cursor glance strings (under construction, power/gas/liquid/solid I/O ports, order types, conduit types, wire, overlay decor, disease) |
| `SCANNER` | Refreshed, empty, invalid, announcement formats, cluster/order formats, instance-of, direction tokens, distance templates, CATEGORIES (9 categories), SUBCATEGORIES (40 subcategories), HELP entries |
| `COLORS` | 50 color name strings (dark gray through cream) |
| `PIXEL_PACK` | Palette, active/standby colors, pixel slot, in use, palette count |
| `DETAILS` | No errands, Schedule, Current task |
| `TABLE` | Sort ascending/descending/cleared, navigation help |
| `PRIORITY_SCREEN` | Handler name, toolbar, skill, disabled trait, row/column increase/decrease/set, proximity on/off, affected errands, help entries |
| `VITALS_SCREEN` | Handler name, focused, focus duplicant, change, eaten today, fullness header, disease header |
| `CONSUMABLES_SCREEN` | Handler name, permitted/forbidden/restricted, morale, mixed, all permitted/forbidden, help entries |
| `RESEARCH` | Handler name, browse/queue/tree tabs, available/locked/completed/active, needs, unlocks, queued/canceled, cascade removed, queue empty, banked points, dead end, root node, bucket labels, help entries, progress format |
| `SKILLS` | Handler name, dupes/skills/tree tabs, points, morale-of, no hat, interests, XP progress, hat queued, bucket labels, available/mastered/locked/granted, morale deficit, interested, no skill points, needs, blocked by, morale need, mastered by, learned, cannot learn, dead end, root node, booster strings (slots, assigned, available, hint, assigned/unassigned, no boosters, none assigned, no empty slots), hat selected/select hat, help entries |
| `SCHEDULE` | Handler name, schedules/dupes tabs, add schedule, cannot delete last (schedule and row), schedule deleted, timetable row added/deleted, options submenu labels (rename, duplicate, delete, add/delete row), block label format, block already format, brush active, painted range, row label, schedule row, moved up/down, all help entries |
| `BUILD_MENU` | All build menu strings (action menu, tools category, placed, placed no material, not rotatable, not buildable, canceled, no construction, must be straight, invalid, line cells, start set/cleared, info panel, obstructed, material formats, extent directions, facing/orient, description/effects/requirements/category/attributes/material effects/facade labels, help entries) |

**Assessment**: Comprehensive. The strings are well-organized by feature area. The file is at 734 lines and has no obvious gaps relative to currently implemented features. Every string follows the "variable part first" principle and avoids inline string literals.

---

## 11. Mod Source Overview

**Total source .cs files**: Approximately 105 files (excluding obj/ build artifacts). Organized as follows:

### OniAccess/ top level (4 files)
- `Mod.cs` — mod entry point
- `ModConfig.cs` — configuration model
- `ConfigManager.cs` — config load/save
- `ModToggle.cs` — on/off toggle

### Speech/ (2 files)
- `SpeechPipeline.cs` — central dispatch, deduplication, enabled toggle
- `SpeechEngine.cs` — Tolk P/Invoke backend

### Input/ (3 files)
- `ModInputRouter.cs` — routes Unity input to handler stack
- `InputUtil.cs` — key utilities
- `KeyPoller.cs` — per-frame key detection

### Util/ (2 files)
- `LogHelper.cs` — Log.Info/Debug/Warn/Error
- `LogUnityBackend.cs` — Unity Debug.Log backend

### Widgets/ (3 files)
- `Widget.cs` — widget abstraction (KSlider, KToggle, LocText, KButton, etc.)
- `WidgetDiscoveryUtil.cs` — recursive widget walker
- `ColorNameUtil.cs` — color-to-name mapping (used for pixel pack)

### Patches/ (3 files)
- `GameLifecyclePatches.cs`
- `HoverTextDrawerPatches.cs`
- `DragToolPatches.cs`

### Handlers/ (top level, 8 files)
- `IAccessHandler.cs` — handler interface
- `BaseScreenHandler.cs` — base class for screen handlers
- `BaselineHandler.cs` — always-active baseline
- `HandlerStack.cs` — handler stack management
- `HelpHandler.cs` — F12 context help overlay
- `ISearchable.cs` — search interface
- `HelpEntry.cs` — help entry data
- `ConsumedKey.cs` — key consumption marker

### Handlers/Build/ (4 files)
- `BuildInfoHandler.cs`, `BuildMenuData.cs`, `FacadePickerHandler.cs`, `MaterialPickerHandler.cs`

### Handlers/Tools/ (2 files)
- `ToolPickerHandler.cs`, `ToolFilterHandler.cs`

### Handlers/Tiles/ (many files across subdirectories)

**Tiles/ top level** (~10 files): `TileCursor.cs`, `GlanceComposer.cs`, `CellContext.cs`, `GameStateMonitor.cs`, `TooltipCapture.cs`, `TooltipBrowserHandler.cs`

**Tiles/Sections/** (~10 files): `AutomationSection.cs`, `BuildingSection.cs`, `ConveyorSection.cs`, `DebrisSection.cs`, `DecorSection.cs`, `DiseaseSection.cs`, `ElementSection.cs`, `EntitySection.cs`, `LightSection.cs`, `PlumbingSection.cs`, `PowerSection.cs`, `RadiationSection.cs`, `TemperatureSection.cs`, `VentilationSection.cs`

**Tiles/Overlays/** (2 files): `OverlayProfile.cs`, `OverlayProfileRegistry.cs`

**Tiles/ToolProfiles/** (~15 files): `ToolProfile.cs`, `ToolProfileRegistry.cs`, plus Sections/ with one file per tool (Attack, Cancel, Capture, Deconstruct, Dig, Disconnect, Disinfect, EmptyPipe, Harvest, Mop, Prioritize, Selection, Sweep, BuildExtent, Build)

**Tiles/Scanner/** (multiple files): `GridScanner.cs`, `ScannerNavigator.cs`, `ScannerTaxonomy.cs`, `ScanEntry.cs`, `IScannerBackend.cs`, `AnnouncementFormatter.cs`, `UnionFind.cs`; Scanner/Routing/ (~6 files: LifeRouter, DebrisRouter, OrderRouter, TileRouter, BuildingRouter, ElementRouter, NetworkLayerConfig); Scanner/Backends/ (GeyserBackend.cs, EntityBackend.cs)

### Handlers/Screens/ (top level, ~20 files)

Currently implemented screen handlers:
- `DetailsScreenHandler.cs` — entity inspection panel (extensive, multi-phase)
- `MainMenuHandler.cs`
- `PauseMenuHandler.cs`
- `OptionsMenuHandler.cs`
- `ColonySetupHandler.cs` — new game flow
- `MinionSelectHandler.cs` — duplicant selection
- `WorldGenHandler.cs` — world generation
- `SaveLoadHandler.cs` / `SaveScreenHandler.cs`
- `ModsHandler.cs`
- `KeyBindingsHandler.cs`
- `ConfirmDialogHandler.cs`
- `FileNameDialogHandler.cs`
- `ColonySummaryHandler.cs`
- `WattsonMessageHandler.cs`
- `StoryMessageHandler.cs`
- `VideoScreenHandler.cs`
- `TranslationHandler.cs`
- `KleiItemDropHandler.cs`
- `LockerMenuHandler.cs`
- `BaseWidgetHandler.cs`
- `ConsumablesScreenHandler.cs`
- `PriorityScreenHandler.cs`
- `VitalsScreenHandler.cs`
- `ResearchScreenHandler.cs`
- `SkillsScreenHandler.cs`
- `ScheduleScreenHandler.cs` — most recently added

### Handlers/Screens/Details/ (12 files)
`DetailSection.cs`, `IDetailTab.cs`, `CollapsiblePanelReader.cs`, `StatusTab.cs`, `PersonalityTab.cs`, `PropertiesTab.cs`, `ChoresTab.cs`, `ConfigSideTab.cs`, `ErrandsSideTab.cs`, `MaterialTab.cs`, `BlueprintTab.cs`, `StubTab.cs`

### Handlers/Screens/Research/ (4 files)
`IResearchTab.cs`, `BrowseTab.cs`, `QueueTab.cs`, `TreeTab.cs`, `ResearchHelper.cs`

### Handlers/Screens/Skills/ (5 files)
`ISkillsTab.cs`, `DupeTab.cs`, `SkillsTab.cs`, `TreeTab.cs`, `SkillsHelper.cs`

### Handlers/Screens/Schedule/ (4 files)
`IScheduleTab.cs`, `SchedulesTab.cs`, `DupesTab.cs`, `ScheduleHelper.cs`

---

## 12. ONI-Decompiled/ — Assessment

**Organization**: Two flat directories — `Assembly-CSharp/` and `Assembly-CSharp-firstpass/`. No subdirectory organization. All files are at the top level within each directory.

**File count**: The CODEBASE_INDEX notes 4,194 files in Assembly-CSharp and 2,444 in Assembly-CSharp-firstpass (total ~6,638 .cs files). The Glob tool truncates results after ~100 entries, confirming this is a very large flat collection.

**Decompilation quality**: Examined `ScheduleScreen.cs` as a representative sample. The decompiled output is clean and readable C# with correct types, field names, method signatures, and logic. `[SerializeField]` attributes are preserved. Local variables have reasonable names. The output is suitable as an authoritative API reference — it accurately reflects the compiled bytecode.

**Version**: `KleiVersion.cs` shows `ChangeList = 707956u`, branch `"release"`, non-debug build. This pins the decompilation to a specific release build.

**Usability as API reference**: Yes, usable. To find method signatures or field names for any class, navigate directly to its file. The CODEBASE_INDEX tells you which file to look for and which assembly it is in. The flat layout means there is no ambiguity about file location once you know the class name.

**What is missing from the decompile**: No XML doc comments (these are stripped at compile time), no original source formatting, and decompiled names for some generated code (delegates, closures) may be opaque. However, for Harmony patching purposes, what matters — class names, method names, field names, return types, parameter types — is all present and correct.

---

## 13. Documentation Gaps and Coverage Summary

### Well-covered

- **Overall game UI scope**: `oni_accessibility_audit.md` is exhaustive. No significant game UI element is undocumented in terms of what it is and what players can do with it.
- **ONI namespace/API discovery**: `CODEBASE_INDEX.md` provides good first-pass lookup.
- **Design principles and architecture intent**: `factorio-access concepts.md` is comprehensive. Any LLM working on this codebase should read it.
- **Hotkey safety**: `hotkey-reference.md` is complete and current.
- **DetailsScreen internals**: `inspection-panels.md` and `details-screen-handler.md` together form an excellent implementation reference for the most complex handler.
- **ScheduleScreen**: Both planning files together constitute a thorough reference for that specific screen.
- **Mod string inventory**: `OniAccessStrings.cs` is comprehensive for all currently implemented features.

### Not covered (gaps)

- **No architectural overview of the mod itself**. There is no document that explains the handler stack, context detector, speech pipeline, tile cursor, scanner, and how they relate to each other at a high level. The `factorio-access concepts.md` describes the intended design, but the actual implemented architecture (HandlerStack, IAccessHandler, BaseScreenHandler, ContextDetector, SpeechPipeline, TileCursor with its sections and overlays, GridScanner with its routers) is only documented in code comments and CLAUDE.md.
- **No per-handler documentation** for the ~25 implemented screen handlers beyond DetailsScreen and Schedule. MainMenuHandler, PauseMenuHandler, PriorityScreenHandler, VitalsScreenHandler, ConsumablesScreenHandler, ResearchScreenHandler, SkillsScreenHandler, etc. have no design docs.
- **No documentation of the tile cursor system** beyond what can be inferred from the source files. TileCursor, GlanceComposer, OverlayProfile, ToolProfile, and the Scanner with its 6 routers and 2 backends are complex subsystems with no standalone documentation.
- **No documentation of the widget/SideScreenWalker system** beyond the details-screen-handler.md coverage.
- **No documentation of game screens not yet implemented** (Vitals, Priority, Consumables, Reports have handlers but no research docs; Codex/Database, Starmap, diagnostic panels have no coverage at all beyond the audit).
- **ContextDetector** — the registration mechanism described in CLAUDE.md but never documented in docs/.
- **Testing patterns** — the test project exists (`OniAccess.Tests`) but is not surveyed here and has no documentation in docs/.
- **Build/deployment process** — documented only in CLAUDE.md, not in docs/.

### Redundancy / Overlap

- Minimal. The six docs/ files each occupy a distinct role with almost no overlap. The two .planning/ files are narrowly scoped to schedule screen and complement each other cleanly (research then design).
- The `factorio-access concepts.md` architecture section partially overlaps with what is now real implemented code, but the overlap is additive (design context is useful even when implementation exists).
