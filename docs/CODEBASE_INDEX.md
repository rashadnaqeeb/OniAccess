# Oxygen Not Included — Codebase Index

Decompiled from `Assembly-CSharp.dll` (4,194 files) and `Assembly-CSharp-firstpass.dll` (2,444 files).

---

## Assembly-CSharp (Main Game Logic)

### Named Namespaces

#### `Klei.AI` — Character attributes, effects, diseases, traits, and behavioral modifiers for duplicants and creatures.
- **Attribute** — Base attribute definition (strength, athletics, etc.)
- **AttributeInstance** — Runtime attribute on a specific entity with active modifiers
- **AttributeModifier** — Modifier applied to an attribute from items, traits, or effects
- **Amount** — Trackable physical amount (Calories, Stamina, Stress, Radiation)
- **AmountInstance** — Runtime value tracker for an amount on an entity
- **Amounts** — Collection of amount trackers on an entity
- **Effect** — Status effect definition (buffs/debuffs)
- **EffectInstance** — Active effect on an entity
- **Effects** — Container managing all active effects on an entity
- **Trait** — Character trait affecting stats and behavior
- **Traits** — Trait container on an entity
- **TraitUtil** — Utility methods for trait manipulation
- **Sickness** — Disease/sickness definition
- **SicknessInstance** — Active disease on an entity
- **Sicknesses** — Disease database
- **Disease** — Core disease definition with growth rules and environmental properties
- **SlimeSickness, ZombieSickness, RadiationSickness** — Specific disease implementations
- **Modifier / Modifiers** — Trait/skill modification system
- **BonusEvent, CreatureSpawnEvent, MeteorShowerEvent, PartyEvent, FoodFightEvent** — Gameplay event types
- **GameplaySeason / GameplaySeasonInstance** — Seasonal event system
- **MeteorShowerSeason** — Seasonal meteor shower events

#### `Klei.AI.DiseaseGrowthRules` — Disease propagation, growth conditions, and environmental factors.
- **GrowthRule** — Base rule for disease growth
- **ElementGrowthRule / ElementExposureRule** — Disease growth from elements
- **TagGrowthRule** — Disease growth from tagged items
- **StateGrowthRule** — Disease growth from game states
- **CompositeGrowthRule / CompositeExposureRule** — Combined growth rules
- **ElemGrowthInfo / ElemExposureInfo** — Element-based disease exposure data

#### `Database` — Game content database: all registered buildings, creatures, techs, skills, achievements, and gameplay definitions.
- **Techs** — Research tree database
- **TechItems / TechTreeTitles** — Tech content and display
- **Skills / SkillGroups / SkillPerks** — Skill definitions and progression
- **Traits / TraitGroup** — Trait definitions
- **Diseases** — Disease database
- **Amounts / Attributes** — Base attribute and amount databases
- **ChoreTypes / ChoreGroups** — Job type and grouping databases
- **StatusItems** — Status indicator database
- **BuildingStatusItems / DuplicantStatusItems / CreatureStatusItems / RobotStatusItems** — Entity-specific status items
- **RoomTypes** — Room category database
- **Personalities** — Duplicant personality database
- **Thoughts** — Morale thought system
- **Emotes** — Character emote animations
- **Accessories / AccessorySlots** — Wearable item slots
- **ClothingItems / ClothingOutfits** — Clothing system
- **EquippableFacades** — Visual equipment overlays
- **Urges** — Duplicant needs/urges
- **ColonyAchievements** — Achievement definitions
- **Spices** — Food spice modifiers
- **Quests** — Quest system
- **Stories / Story** — Narrative content
- **PlantMutations** — Plant mutation system
- **PlantAttributes / CritterAttributes** — Plant and creature characteristic databases
- **SpaceDestinationTypes** — Rocket destination database
- **PermitResources / PermitCategories** — Building upgrade permits
- **MonumentParts** — Monument construction pieces

#### `Klei.CustomSettings` — Difficulty and gameplay customisation for new games.
- **CustomGameSettingConfigs** — All custom game setting configurations
- **SettingConfig** — Base setting class
- **ToggleSettingConfig** — On/off settings
- **ListSettingConfig** — Multi-choice settings
- **SeedSettingConfig** — World seed settings
- **MixingSettingConfig / WorldMixingSettingConfig / SubworldMixingSettingConfig / DlcMixingSettingConfig** — Content mixing options

#### `Klei.Actions` — Action system for building tools and interactions.
- **ActionFactory / DigToolActionFactory** — Action creation factories
- **DigAction / ImmediateDigAction / ClearCellDigAction / MarkCellDigAction** — Digging action types

#### `Klei.Input` — User input processing and tool configuration.
- **InterfaceToolConfig** — Tool input configuration

#### `Klei` — Core engine save/load structures and simulation utilities.
- **SimUtil** — Simulation utilities
- **SimSaveFileStructure** — Save file format definition
- **SaveFileRoot** — Top-level save data container
- **WorldDetailSave** — World-specific save data
- **WorldGenSave** — World generation save data
- **ClusterLayoutSave** — Cluster layout save data
- **CallbackInfo** — Callback system data

#### `KMod` — Mod loading framework and management.
- **Manager** — Central mod manager
- **Mod** — Single mod definition
- **UserMod2** — Base class for user mods
- **Content** — Mod content registration
- **DLLLoader** — Dynamic assembly loading for mods
- **Steam** — Steam Workshop integration
- **LoadedModData** — Loaded mod metadata
- **Label** — Mod labeling and identification
- **KModUtil** — Mod utility functions
- **ModErrorsScreen** — Mod error display UI
- **ModContentCompatability** — Compatibility checking between mods

#### `ProcGen` — Core procedural generation infrastructure.
- **WorldLayout** — World layout generation

#### `ProcGenGame` — Game-specific world and cluster generation.
- **WorldGen** — Main world generation orchestrator
- **Cluster** — Cluster/asteroid configuration
- **TerrainCell** — Individual terrain cell data
- **Border** — Cluster borders
- **River** — River/water body generation
- **TemplateSpawning** — Template-based entity spawning
- **GameSpawnData** — Spawn configuration
- **WorldGenSimUtil** — Generation simulation utilities
- **WorldgenMixing** — World content mixing
- **MobSpawning** — Creature spawning system

#### `Rendering` — Graphics rendering infrastructure.
- **BlockTileRenderer** — Tile rendering system
- **IBlockTileInfo** — Tile information interface
- **BackWall** — Background wall rendering

#### `Rendering.World` — World-space rendering systems.
- **Tile / TileCells** — Tile system
- **TileRenderer / LiquidTileOverlayRenderer** — Tile and liquid renderers
- **DynamicMesh / DynamicSubMesh** — Mesh management
- **Brush** — Drawing brush
- **Mask / SourceMask** — Masking system

#### `STRINGS` — All localised game text.
- **INPUT_BINDINGS** — Key binding display strings
- **SEARCH_TERMS** — Building search keywords
- **BUILDINGS** — Building names and descriptions
- **ELEMENTS** — Element names and descriptions
- **CREATURES** — Creature names and descriptions
- **DUPLICANTS** — Duplicant-related strings
- **ROOMS** — Room names and descriptions
- **EQUIPMENT** — Equipment strings
- **ITEMS** — Item strings
- **RESEARCH** — Research tree strings
- **UI** — UI component strings
- **COLONY_ACHIEVEMENTS** — Achievement strings
- **CODEX** — Encyclopedia/codex strings
- **MISC** — Miscellaneous strings

#### `TUNING` — All numerical game balance constants.
- **BUILDINGS** — Building cost, power, output stats
- **CREATURES** — Creature stats and behavior tuning
- **DUPLICANTSTATS** — Duplicant attribute tuning
- **EQUIPMENT** — Equipment stat values
- **CROPS** — Plant growth and yield values
- **FOOD** — Food spoilage and nutrition values
- **MEDICINE** — Medicine effectiveness values
- **POWER** — Power generation/consumption values
- **STRESS** — Stress mechanics tuning
- **DECOR** — Decoration values
- **DISEASE / GERM_EXPOSURE** — Disease and germ parameters
- **SKILLS / ROLES / TRAITS** — Progression tuning
- **PLANTS / ROBOTS** — Plant and robot tuning
- **LIGHT2D / NOISE_POLLUTION / RADIATION / ROCKETRY / STORAGE** — Miscellaneous system tuning

#### `TemplateClasses` — Serialisable templates for entity creation.
- **Prefab** — Prefab entity template
- **Cell** — Cell template
- **StorageItem** — Storage item template
- **Rottable** — Decay/spoilage template

#### `ImGuiObjectDrawer` — ImGui-based debug UI drawing.
- **CollectionDrawer / ArrayDrawer / IDictionaryDrawer / IEnumerableDrawer** — Collection renderers
- **EnumDrawer / SimpleDrawer / NullDrawer / FallbackDrawer** — Type-specific renderers

#### `FoodRehydrator` — Food rehydration mechanics.
- **DehydratedManager** — Dehydrated food management
- **AccessabilityManager** — Food accessibility tracking
- **ResourceRequirementMonitor** — Resource tracking for rehydration

#### `EventSystem2Syntax` — Event handling framework.
- **IEventData** — Event data interface
- **GameHashes** — Event hash constants

#### `UnityEngine.EventSystems` — Unity input event integration.
- **VirtualInputModule** — Custom input module

---

### Global Namespace (No Namespace Declaration — ~3,700+ files)

The majority of game code lives in the global namespace, categorised below by function.

#### Building Configs (~400+ `IBuildingConfig` implementations)
Each class defines a constructible building: cost, size, ports, and behavior.
- **AirConditionerConfig** — Cooling building
- **AlgaeDistilleryConfig** — Algae processing
- **BatteryConfig / BatteryMediumConfig / BatterySmartConfig** — Power storage tiers
- **BedConfig / LuxuryBedConfig** — Sleeping furniture
- **CampfireConfig** — Basic heat/cooking source
- **CO2ScrubberConfig** — Atmospheric CO2 removal
- **ChemicalRefineryConfig** — Material refining
- **CreatureFeederConfig** — Creature feeding station
- **DecontaminationShowerConfig** — Hygiene building
- **DesalinatorConfig** — Salt water processing
- **ElectrolyzerConfig** — Water-to-oxygen conversion
- **GeneratorConfig / SolarPanelConfig / SteamTurbineConfig** — Power generation
- **ResearchCenterConfig / DLC1CosmicResearchCenterConfig** — Research stations
- **RocketEngineClusterConfig** — Rocket propulsion
- *(~350+ more)*

#### Creature Configs (~150+ `IEntityConfig` implementations)
Each class defines a creature and its baby/egg variants.
- **HatchConfig / BabyHatchConfig** — Rock-eating hatch
- **CrabConfig / BabyCrabConfig** — Pokeshell
- **DreckoConfig / BabyDreckoConfig** — Plastic-producing drecko
- **PacuConfig / BabyPacuConfig** — Fish
- **PuftConfig / BabyPuftConfig** — Gas-consuming puft
- **MoleConfig / BabyMoleConfig** — Burrowing shove vole
- **SquirrelConfig / BabySquirrelConfig** — Pip (tree planter)
- **StaterpillarConfig / BabyStaterpillarConfig** — Electric caterpillar (DLC)
- **BeeConfig / BaseBeeHiveConfig** — Bee and hive (DLC)
- **DeerConfig / BabyDeerConfig** — Bammouth (DLC)
- **RaptorConfig / BabyRaptorConfig** — Plug slug (DLC)
- *(~100+ more)*

#### Item / Equipment / Consumable Configs (~80+ `IEntityConfig` implementations)
- **AtmoSuitConfig** — Oxygen suit
- **JetSuitConfig** — Jetpack
- **LeadSuitConfig** — Radiation protection suit
- **BasicCureConfig / AdvancedCureConfig** — Disease medicine
- **BasicRadPillConfig / AdvancedRadPillConfig** — Radiation medicine
- **MushBarConfig** — Basic food item
- **ArtifactConfig** — Space artifact
- *(~70+ more)*

#### UI Screens & Dialogs (~150+ `KScreen` / `KModalScreen` subclasses)
- **CodexScreen** — Encyclopedia viewer
- **ColonyDiagnosticScreen** — Colony statistics dashboard
- **CrewListScreen / CrewJobsScreen / CrewRationsScreen** — Crew management tabs
- **ClusterMapScreen** — Space/cluster map
- **ControlsScreen** — Input configuration
- **ConfirmDialogScreen / CustomizableDialogScreen** — Dialog boxes
- **AllResourcesScreen** — Resource overview
- **ConsumablesTableScreen** — Food/consumables table
- **ColonyDestinationSelectScreen** — New game destination selection
- **SaveConfigurationScreen** — Save options
- *(~130+ more)*

#### Side Screens (~100+ `SideScreenContent` subclasses)
Context-sensitive panels shown when selecting buildings/entities.
- **AccessControlSideScreen** — Door access control
- **AlarmSideScreen** — Alarm configuration
- **AssignableSideScreen** — Workstation assignment
- **AutomatableSideScreen** — Automation toggle
- **CapacityControlSideScreen** — Storage capacity settings
- **ComplexFabricatorSideScreen** — Fabricator recipe selection
- **LogicBitSelectorSideScreen** — Logic signal selection
- **RocketModuleSideScreen** — Rocket module configuration
- *(~80+ more)*

#### Building Components (~200+ `KMonoBehaviour` subclasses)
Behavior scripts attached to building/entity GameObjects.
- **Building** — Base building behavior
- **BuildingComplete** — Completed building marker and logic
- **Operational** — Tracks whether a building is operational
- **Storage** — Item storage container
- **Workable** — Base class for buildings that require duplicant labor
- **Assignable** — Workstation/bed assignment
- **Automatable** — Automation port support
- **LogicPorts** — Logic circuit input/output interface
- **Rottable** — Item decay/spoilage over time
- **Pickupable** — Item that can be carried by duplicants
- **Activatable** — Toggle activation on a building
- *(~180+ more)*

#### Game Systems & Managers (~80+ singleton/manager classes)
Central coordinators for game subsystems.
- **Game** — Top-level game state and lifecycle
- **SaveLoader** — Save/load orchestration
- **BuildingConfigManager** — Building definition registry
- **EntityConfigManager** — Entity definition registry
- **CircuitManager** — Electrical circuit management
- **ConduitManager** — Pipe network management
- **ConduitDiseaseManager** — Disease propagation in pipes
- **ConduitTemperatureManager** — Temperature in pipes
- **ClusterManager** — Multi-asteroid management
- **GameScheduler** — Timed callback scheduling
- **ChoreGroupManager** — Job group management
- **AssignmentManager** — Crew assignment management
- **AlertStateManager** — Alert/notification management
- **ConversationManager** — NPC conversation management
- *(~60+ more)*

#### State Machines (~80+ `GameStateMachine` subclasses)
Define complex entity behavior as state graphs.
- **AttackStates** — Creature combat behavior
- **EatStates** — Eating behavior
- **SleepStates / CreatureSleepStates** — Sleeping behavior
- **DiggerStates** — Digging behavior
- **DeathStates** — Death and cleanup
- **CropTendingStates** — Duplicant tending plants
- **BeeForageStates / BeeMakeHiveStates** — Bee behavior (DLC)
- **AnimInterruptStates** — Animation interruption handling
- **ApproachBehaviourStates** — NPC approach/navigation
- *(~60+ more)*

#### Logic / Automation (~40+ classes)
- **LogicCircuitManager** — Logic network management
- **LogicGate / LogicGateBuffer / LogicGateNot / LogicGateAnd / LogicGateOr / LogicGateXor** — Logic gate components
- **LogicWire** — Logic wire connections
- **LogicSwitch / LogicPressureSensor / LogicTemperatureSensor** — Sensor components

#### Core Utility & Infrastructure
- **Grid** — World grid cell data and utilities
- **GameTags** — Tag constants used across the codebase
- **Db** — Singleton accessor to all `Database` content
- **BuildingTemplates** — Helper methods for creating building definitions
- **EntityTemplates** — Helper methods for creating entity definitions
- **Element** — Chemical element definition
- **SimHashes** — Hash enum for all simulation elements
- **Tag** — Lightweight identifier used for filtering and matching

---

## Assembly-CSharp-firstpass (Engine, Libraries, Platform SDKs)

### Global Namespace (324 files) — Core engine utilities: animation, input, audio, UI framework, and data structures.

#### Animation & Graphics
- **KAnimFile** — Scriptable asset container for animation data and textures
- **KAnim** — Core animation control with playback modes (Loop/Once/Paused)
- **KAnimBatchManager** — Manages animation rendering batches with spatial partitioning (32x32 chunks)
- **KAnimBatch / KAnimBatchGroup** — Individual animation batch and batch grouping
- **TextureAtlas / TexturePagePool / TextureBuffer** — Texture atlas and GPU texture management
- **SymbolInstanceGpuData / SymbolOverrideInfoGpuData** — GPU data for animation symbols

#### Input & Control
- **KInputManager** — Centralised input event dispatching and controller management
- **GameInputManager** — Game-specific input mapping and action binding
- **KInputController** — Single input controller (keyboard, mouse, gamepad)
- **KInputHandler** — Input event processing and dispatch
- **KInputBinding** — Physical-input-to-game-action mapping
- **GameInputMapping** — Default input bindings
- **Action** — Enum of all game actions

#### Audio
- **KFMOD** — FMOD audio engine integration and sound instance management
- **Audio** — Audio configuration and listener positioning
- **SoundDescription** — Sound effect metadata

#### Application & Platform
- **App** — Main application entry point and lifecycle
- **CPUBudget** — CPU load balancing and performance profiling
- **GameScreenManager** — Screen/scene transition management
- **SteamManager** — Steam platform integration
- **DistributionPlatform** — Platform abstraction (Steam, GOG, etc.)
- **DlcManager** — DLC availability and content restriction management
- **KleiAccount** — Klei account integration
- **KleiMetrics** — Analytics and metrics

#### UI Components
- **KBasicToggle / KImageToggle / KToggleSlider** — Toggle UI components
- **KImageButton / KImage / KPointerImage** — Image-based UI components
- **KTabMenu** — Tab menu system
- **KTreeControl** — Hierarchical tree view
- **KInputField / KNumberInputField / KInputTextField** — Text input components
- **ColorStyleSetting / TextStyleSetting** — Style configuration

#### Utility & Data Structures
- **ContainerPool / DictionaryPool / ListPool** — Object pooling systems
- **BinaryHeap** — Priority queue implementation
- **Vector2I** — Integer 2D vector
- **AxialI / AxialUtil** — Axial (hexagonal) coordinate system
- **CellOffset / CellChangeMonitor** — Grid cell utilities
- **SeededRandom** — Deterministic random number generator
- **EventSystem** — Event dispatching framework
- **AsyncLoader / AsyncLoadManager** — Asynchronous resource loading
- **PerlinNoise / SimplexNoise** — Procedural noise generators
- **CSVUtil** — CSV parsing/writing utilities

---

### `Klei` (13 files) — File I/O, virtual filesystem, and framework utilities.
- **FileSystem** — Virtual file system with pluggable directory backends
- **FileUtil** — Static file operation utilities
- **RootDirectory / ZipFileDirectory / MemoryFileDirectory** — File system implementations
- **CSVReader** — CSV parsing
- **YamlIO** — YAML serialisation integration
- **GenericGameSettings** — Key-value game configuration

### `KSerialization` (18 files) — Custom binary serialisation system for save files.
- **Manager** — Central serialisation orchestrator with template caching
- **Serializer** — Object-to-binary serialisation with versioning
- **Deserializer** — Binary-to-object reconstruction with version migration
- **SerializationTemplate / DeserializationTemplate** — Type metadata templates
- **Serialize** — Attribute marking a field for serialisation
- **SerializationConfig** — OptIn/OptOut control attribute
- **SerializationTypeInfo** — Type metadata with version numbers and converters

### `ProcGen` (47 files) — Procedural generation framework for worlds and clusters.
- **World** — Asteroid/moon definition with biome distribution and element placement
- **Graph\<N,A\>** — Generic directed graph with serialisation
- **ClusterLayout / ClusterLayouts** — Cluster world layout specifications
- **BiomeSettings** — Biome configuration with element distribution
- **WorldGenSettings** — World generation parameters (size, temperature, biome mixing)
- **Feature / FeatureSettings** — Procedural feature definitions
- **Room** — Room specification in cluster layouts
- **River** — River specification and connectivity
- **WeightedRandom\<T\> / WeightedBiome / WeightedSubWorld** — Weighted random selection
- **SettingsCache** — Cached generation settings
- **SpaceMapPOIPlacement** — Point-of-interest placement for space maps
- **HashedString** — String with precomputed hash for fast comparison

### `ProcGen.Map` (4 files) — Voronoi-based map generation.
- **MapGraph\<Cell,Edge\>** — Voronoi diagram specialised for map regions
- **Cell** — Voronoi cell representing a map region
- **Edge** — Map edge connecting cells
- **Corner** — Map corner (Voronoi vertex) with elevation and moisture

### `ProcGen.Noise` (14 files) — Noise module composition tree for terrain generation.
- **Tree** — Node tree for composing noise modules
- **Primitive** — Primitive noise modules (Perlin, Worley)
- **Filter** — Noise filters (Curve, Threshold, Clamp)
- **Modifier** — Noise modifiers (Multiply, Add, Power)
- **Combiner** — Combines multiple noise sources
- **Transformer** — Coordinate transformations (Scale, Translate, Rotate)
- **SampleSettings** — Noise sampling configuration

---

### Third-Party Libraries

#### `ClipperLib` (25 files) — Polygon clipping (Vatti algorithm) for boolean geometry operations.
- **Clipper** — Polygon clipping engine (intersection, union, difference, XOR)
- **ClipperOffset** — Path inset/outset generation
- **PolyTree / PolyNode** — Hierarchical polygon result structure

#### `Delaunay` (15 files) — Delaunay triangulation and Voronoi diagram generation.
- **Voronoi** — Voronoi diagram generator from point sets
- **Site** — Voronoi site with index, weight, and region classification
- **Edge** — Half-edge with site linking
- **Halfedge / HalfedgePriorityQueue** — Half-edge data structures
- **SiteList** — Sorted site list for sweep-line processing

#### `Delaunay.Geo` (4 files) — Geometric primitives for Voronoi results.
- **Polygon** — Closed polygon with area calculations
- **Circle** — Circumcircle representation
- **LineSegment** — Line segment with distance calculations

#### `Satsuma` (65 files) — Graph algorithm library for procedural analysis and traversal.
- **Graph / IGraph / IBuildableGraph** — Graph interfaces and implementations
- **Dfs / Bfs** — Depth-first and breadth-first search
- **AStar / Dijkstra / BellmanFord** — Pathfinding algorithms
- **ConnectedComponents / BiNodeConnectedComponents** — Component analysis
- **DisjointSet** — Union-find data structure
- **ContractedGraph / CustomGraph** — Graph adapters

#### `MIConvexHull` (23 files) — Convex hull computation for 2D/3D geometry.
- **ConvexHull / ConvexHullAlgorithm** — Incremental convex hull computation
- **ConvexFace\<TVertex,TFace\>** — Hull face representation

#### `FuzzySharp` (30+ files) — Fuzzy string matching for search and autocomplete.
- **Fuzz** — String comparison (Ratio, PartialRatio, TokenSort, TokenSet)
- **Process** — Batch extraction and ranking
- **Levenshtein** — Edit distance calculation

#### `Geometry` (5 files) — 2D rectangle utilities and sweep-line algorithms.
- **KRect** — Axis-aligned rectangle with union/intersection

#### `HUSL` (1 file) — HUSL colour space conversion.
- **ColorConverter** — RGB-to-HUSL conversion and colour mixing

#### `VoronoiTree` (6 files) — Spatial partitioning via Voronoi diagrams.
- **Tree** — Voronoi-based spatial tree with Lloyd relaxation
- **PowerDiagram** — Weighted Voronoi variant

#### `NodeEditorFramework` (31 files) — Visual node graph editor for procedural systems.
- **NodeCanvas** — Node graph container with serialisation
- **Node** — Base node with input/output ports
- **NodeEditor** — Main editor interface
- **Connection** — Edge connecting node ports

#### `YamlDotNet` (95+ files) — Full YAML 1.1 serialisation library.
- **Serializer / Deserializer** — High-level serialise/deserialise API
- **SerializerBuilder / DeserializerBuilder** — Fluent configuration builders
- **Scanner / Parser / Emitter** — Low-level YAML parsing and output
- **YamlDocument / YamlNode / YamlScalarNode / YamlSequenceNode / YamlMappingNode** — Document object model

#### `UnityEngine.UI.Extensions` (4 files) — Extended Unity UI components.
- **UILineRenderer** — Line drawing with configurable joints
- **BezierPath** — Bezier curve interpolation

#### `UnityStandardAssets.ImageEffects` (4 files) — Post-processing effects.
- **PostEffectsBase** — Base class for camera post-effects
- **ColorCorrectionLookup** — 3D LUT colour grading

---

### Platform SDKs

#### `Rail` (476 files) — Jinyou Rail platform SDK (Chinese distribution).
Comprehensive SDK covering achievements, friends, cloud storage, leaderboards, browser integration, and workshop content. Not relevant for most modding.

#### `Epic.OnlineServices` (200+ files across 15 sub-namespaces) — Epic Online Services SDK.
Cross-platform integration for achievements, auth, friends, lobbies, leaderboards, cloud saves, presence, and store. Sub-namespaces: `Achievements`, `Auth`, `Connect`, `Ecom`, `Friends`, `Leaderboards`, `Lobby`, `Logging`, `Metrics`, `P2P`, `Platform`, `PlayerDataStorage`, `Presence`, `Sessions`, `Stats`, `TitleStorage`, `UI`, `UserInfo`. Not relevant for most modding.

---

## Key Patterns for Modders

| Pattern | Description |
|---|---|
| `IBuildingConfig` | Implement to define a new building |
| `IEntityConfig` | Implement to define a new creature, item, or equipment |
| `*Config` suffix | Convention for entity/building definition classes |
| `KMonoBehaviour` | Base class for all game components (replaces Unity MonoBehaviour) |
| `GameStateMachine<,,>` | Base class for state machine definitions |
| `SideScreenContent` | Base class for context-sensitive UI panels |
| `KScreen` / `KModalScreen` | Base classes for full-screen UI |
| `Db.Get()` | Singleton access to all `Database` content |
| `GameTags` | Static tag constants for filtering and matching |
| `SimHashes` | Enum of all simulation elements |
| `STRINGS` | All localisable text constants |
| `TUNING` | All balance/tuning constants |
| `KMod.UserMod2` | Base class for mod entry points |
| `Harmony (0Harmony.dll)` | Patching framework used for runtime method hooking |
