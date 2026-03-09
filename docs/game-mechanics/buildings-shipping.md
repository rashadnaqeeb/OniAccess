# Shipping

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

Conveyor rails carry up to 20 kg per packet (SolidConduitFlow.MAX_SOLID_MASS).

---

## Dev Pump Solid (DevPumpSolid)

- **Name:** Dev Pump Solid
- **Description:** Piping a pump's output to a building's intake will send solids to that building.
- **Effect:** Generates chosen Solid Materials and runs it through Conveyor Rail.
- **Size:** 2 wide x 2 tall
- **Placement:** Anywhere
- **Construction:** 50 kg Metal (any) [TIER1]
- **HP:** 100
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 65 dB (radius 10) [NOISY.TIER2]
- **Power:** None
- **Overheat temp:** Not overheatable
- **Melting point:** 9999 K
- **Floodable:** No
- **Entombable:** No
- **Invincible:** Yes
- **Corrosion proof:** Yes
- **Technology:** None (debug-only building)
- **DLC:** Base game
- **Debug only:** Yes
- **Conveyor output:** Offset (1, 1)
- **Storage:** 20 kg
- **Special:** Always operational. Generates any solid element chosen via DevPump. Dispenses onto conveyor rail continuously (alwaysDispense = true).

---

## Auto-Sweeper (SolidTransferArm)

- **Name:** Auto-Sweeper
- **Description:** An auto-sweeper's range can be viewed at any time by clicking on the building.
- **Effect:** Automates Sweeping and Supplying errands by sucking up all nearby Debris. Materials are automatically delivered to any Conveyor Loader, Conveyor Receptacle, storage, or buildings within range.
- **Size:** 3 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 200 kg Refined Metal [TIER3]
- **HP:** 10
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Noise:** 45 dB (radius 10) [NOISY.TIER0]
- **Power:** 120 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 2 kDTU/s
- **Overheat temp:** 1600 K (default)
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Smart Storage
- **DLC:** Base game
- **Logic input:** Automation port at offset (0, 0)
- **Pickup range:** 4 cells in each direction (9x9 area centered on origin)
- **Range visualizer:** Min (-4, -4) to Max (4, 4) from origin (0, 0)
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Special:** Picks up debris and delivers to nearby buildings, storage, conveyor loaders, and receptacles. Blocks line-of-sight (BlockingTileVisible = true in visualizer).

---

## Conveyor Rail (SolidConduit)

- **Name:** Conveyor Rail
- **Description:** Rails move materials where they'll be needed most, saving Duplicants the walk.
- **Effect:** Transports Solid Materials on a track between Conveyor Loader and Conveyor Receptacle. Can be run through wall and floor tile.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 100 kg Metal (raw) [TIER2]
- **HP:** 10
- **Decor:** 0 (radius 1) [NONE]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Technology:** Solid Transport
- **DLC:** Base game
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Special:** Drag-buildable utility tile. Always operational. Can be emptied by Duplicants (EmptySolidConduitWorkable). Placed on SolidConduit object layer. Base repair time: 0 (instant). Does not require foundation.

---

## Conveyor Bridge (SolidConduitBridge)

- **Name:** Conveyor Bridge
- **Description:** Separating rail systems helps ensure materials go to the intended destinations.
- **Effect:** Runs one Conveyor Rail section over another without joining them. Can be run through wall and floor tile.
- **Size:** 3 wide x 1 tall
- **Placement:** On existing conduit (BuildLocationRule.Conduit)
- **Construction:** 400 kg Metal (any) [TIER4]
- **HP:** 10
- **Decor:** 0 (radius 1) [NONE]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Transport
- **DLC:** Base game
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Conveyor input:** Offset (-1, 0)
- **Conveyor output:** Offset (1, 0)
- **Special:** Always operational. Does not require foundation. Base repair time: -1 (never breaks).

---

## Conveyor Loader (SolidConduitInbox)

- **Name:** Conveyor Loader
- **Description:** Material filters can be used to determine what resources are sent down the rail.
- **Effect:** Loads Solid Materials onto Conveyor Rail for transport. Only loads the resources of your choosing.
- **Size:** 1 wide x 2 tall
- **Placement:** Anywhere
- **Construction:** 200 kg Refined Metal [TIER3]
- **HP:** 100
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** None
- **Power:** 120 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 2 kDTU/s
- **Overheat temp:** 1600 K (default)
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Transport
- **DLC:** Base game
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Logic input:** Automation port at offset (0, 1)
- **Power input:** Offset (0, 1)
- **Conveyor output:** Offset (0, 0)
- **Storage:** 1000 kg (filterable, accepts STORAGE_LOCKERS_STANDARD + FOOD categories)
- **Special:** Automatable. Only transfers from lower-priority storage (onlyTransferFromLowerPriority = true). Shows capacity status. Items cannot be manually removed from storage. Uses TreeFilterable for element selection.

---

## Conveyor Receptacle (SolidConduitOutbox)

- **Name:** Conveyor Receptacle
- **Description:** When materials reach the end of a rail they enter a receptacle to be used by Duplicants.
- **Effect:** Unloads Solid Materials from a Conveyor Rail into storage.
- **Size:** 1 wide x 2 tall
- **Placement:** Anywhere
- **Construction:** 200 kg Metal (any) [TIER3]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Control
- **DLC:** Base game
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Conveyor input:** Offset (0, 0)
- **Storage:** 100 kg (items can be manually removed)
- **Special:** Always operational. Automatable. Consumes items from conveyor rail via SolidConduitConsumer and vents them via SimpleVent.

---

## Solid Filter (SolidFilter)

- **Name:** Solid Filter
- **Description:** All solids are sent into the building's output conveyor, except the solid chosen for filtering.
- **Effect:** Separates one Solid Material from the conveyor, sending it into a dedicated Conveyor Rail.
- **Size:** 3 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 50 kg Metal (raw) [TIER1]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** 55 dB (radius 10) [NOISY.TIER1]
- **Power:** 120 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 0 kDTU/s
- **Overheat temp:** 1600 K (default)
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Management
- **DLC:** Base game
- **Conveyor input:** Offset (-1, 0)
- **Conveyor output (primary):** Offset (1, 0) - non-matching solids
- **Conveyor output (secondary):** Offset (0, 0) - filtered solid
- **Special:** Filters one chosen solid element to the secondary output. All other solids pass through to the primary output. Uses ElementFilter with Filterable (solid state).

---

## Conveyor Chute (SolidVent)

- **Name:** Conveyor Chute
- **Description:** When materials reach the end of a rail they are dropped back into the world.
- **Effect:** Unloads Solid Materials from a Conveyor Rail onto the floor.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 200 kg Metal (any) [TIER3]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Floodable:** No
- **Technology:** Solid Transport
- **DLC:** Base game
- **Skill requirement:** Conveyor Build (ConveyorBuild perk)
- **Logic input:** Automation port at offset (0, 0)
- **Conveyor input:** Offset (0, 0)
- **Storage:** 100 kg
- **Special:** Consumes items from conveyor rail and drops them on the ground via SolidConduitDropper.

---

## Conveyor Shutoff (SolidLogicValve)

- **Name:** Conveyor Shutoff
- **Description:** Automated conveyors save power and time by removing the need for Duplicant input.
- **Effect:** Connects to an Automation grid to automatically turn Solid Material transport on or off.
- **Size:** 1 wide x 2 tall
- **Placement:** Anywhere
- **Construction:** 100 kg Refined Metal [TIER2]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** 55 dB (radius 10) [NOISY.TIER1]
- **Power:** 10 W
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Control
- **DLC:** Base game
- **Power input:** Offset (0, 1)
- **Conveyor input:** Offset (0, 0)
- **Conveyor output:** Offset (0, 1)
- **Logic input:** At offset (0, 0) - "Open/Close". Green Signal: Allow material transport. Red Signal: Prevent material transport.
- **Special:** When unpowered or no automation wire, defaults to closed (unNetworkedValue = 0). Internally uses SolidConduitBridge to pass items through. Base repair time: -1 (never breaks).

---

## Conveyor Meter (SolidLimitValve)

- **Name:** Conveyor Meter
- **Description:** Conveyor Meters let an exact amount of materials pass through before shutting off.
- **Effect:** Connects to an Automation grid to automatically turn material transfer off when the specified amount has passed through it.
- **Size:** 1 wide x 2 tall
- **Placement:** Anywhere
- **Construction:** 25 kg Refined Metal + 50 kg Plastic [TIER0 + TIER1]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** 55 dB (radius 10) [NOISY.TIER1]
- **Power:** 10 W
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Solid Control
- **DLC:** Base game
- **Power input:** Offset (0, 1)
- **Conveyor input:** Offset (0, 0)
- **Conveyor output:** Offset (0, 1)
- **Logic input:** Reset port at offset (0, 1). Green Signal: Reset the amount. Red Signal: Nothing.
- **Logic output:** At offset (0, 0) - "Limit Reached". Green Signal: Limit has been reached. Red Signal: Otherwise.
- **Limit range:** 0 to 500 kg (displayed as item count, not mass; displayUnitsInsteadOfMass = true)
- **Slider ranges:** 0-50 (50% of slider), 50-200 (30% of slider), 200-500 (20% of slider) - NonLinearSlider
- **Special:** Default limit is 0. No enable/disable button (BuildingEnabledButton destroyed). Internally uses SolidConduitBridge. Base repair time: -1 (never breaks). RequireOutputs ignores full pipe.

---

## Conveyor Rail Germ Sensor (SolidConduitDiseaseSensor)

- **Name:** Conveyor Rail Germ Sensor
- **Description:** Germ sensors can help control automation behavior in the presence of germs.
- **Effect:** Sends a Green Signal or a Red Signal based on the internal Germ count of the object on the rail.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 25 kg Refined Metal + 50 kg Plastic [TIER0 + TIER1]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Technology:** Solid Management
- **DLC:** Base game
- **Logic output:** At offset (0, 0) - germ count threshold
- **Special:** Always operational. Default threshold: 0 germs. Activates above threshold by default. Uses ConduitDiseaseSensor for solid conduit type.

---

## Conveyor Rail Element Sensor (SolidConduitElementSensor)

- **Name:** Conveyor Rail Element Sensor
- **Description:** Element sensors can be used to detect the presence of a specific item on a rail.
- **Effect:** Sends a Green Signal when the selected item is detected on a rail.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 25 kg Refined Metal [TIER0]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Technology:** Solid Management
- **DLC:** Base game
- **Logic output:** At offset (0, 0) - item presence
- **Special:** Always operational. Uses Filterable (solid state) to select which element to detect. Default state: off.

---

## Conveyor Rail Thermo Sensor (SolidConduitTemperatureSensor)

- **Name:** Conveyor Rail Thermo Sensor
- **Description:** Thermo sensors disable buildings when their rail contents reach a certain temperature.
- **Effect:** Sends a Green Signal or a Red Signal when rail contents enter the chosen Temperature range.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere
- **Construction:** 25 kg Refined Metal [TIER0]
- **HP:** 30
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Noise:** None
- **Power:** None
- **Overheat temp:** Not overheatable
- **Entombable:** No
- **Floodable:** No
- **Technology:** Solid Management
- **DLC:** Base game
- **Logic output:** At offset (0, 0) - temperature threshold
- **Special:** Always operational. Default threshold: 280 K (6.85 C). Activates above threshold by default. Range: 0 to 9999 K.

---

## Robo-Miner (AutoMiner)

- **Name:** Robo-Miner
- **Description:** A robo-miner's range can be viewed at any time by selecting the building.
- **Effect:** Automatically digs out all materials in a set range.
- **Size:** 2 wide x 2 tall
- **Placement:** On foundation, rotatable (OnFoundationRotatable)
- **Construction:** 200 kg Refined Metal [TIER3]
- **HP:** 10
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Noise:** 45 dB (radius 10) [NOISY.TIER0]
- **Power:** 120 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 2 kDTU/s
- **Overheat temp:** 1600 K (default)
- **Floodable:** No
- **Rotatable:** 360 degrees
- **Technology:** Robotic Tools
- **DLC:** Base game
- **Logic input:** Automation port at offset (0, 0)
- **Mining area:** x=-7, y=0, width=16, height=9 (relative to building origin)
- **Vision offset:** (0, 1)
- **Range visualizer:** Min (-7, -1) to Max (8, 7) from origin offset (0, 1)
- **Special:** Requires line of sight from vision offset to target cell (BlockingTileVisible = false, uses AutoMiner.DigBlockingCB). Does not output to conveyor rails; mined materials drop as debris.

---

## Solid Rocket Port Loader (ModularLaunchpadPortSolid)

- **Name:** Solid Rocket Port Loader
- **Description:** Rockets must be landed to load or unload resources.
- **Effect:** Loads Solids to the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the solid material filters set on the rocket's cargo bays.
- **Size:** 2 wide x 2 tall
- **Placement:** On floor
- **Construction:** 400 kg Refined Metal [TIER4]
- **HP:** 1000
- **Decor:** 0 (radius 1) [NONE]
- **Noise:** 65 dB (radius 10) [NOISY.TIER2]
- **Power:** 240 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 0 kDTU/s
- **Overheat temp:** 2273.15 K (2000 C)
- **Melting point:** 9999 K
- **Floodable:** No
- **Technology:** Solid Transport
- **DLC:** Spaced Out! (EXPANSION1)
- **Conveyor input:** Offset (0, 0)
- **Power input:** Offset (1, 0)
- **Storage:** 20 kg (sealed, insulated, hidden)
- **Special:** Loads solids from conveyor rail into linked rocket cargo. Uses SolidConduitConsumer. Must be built adjacent to a Rocket Platform (LaunchPad) or another Rocket Port (chained building system). Does not use structure temperature. Scene layer: BuildingBack.

---

## Solid Rocket Port Unloader (ModularLaunchpadPortSolidUnloader)

- **Name:** Solid Rocket Port Unloader
- **Description:** Rockets must be landed to load or unload resources.
- **Effect:** Unloads Solids from the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the solid material filters set on this unloader.
- **Size:** 2 wide x 3 tall
- **Placement:** On floor
- **Construction:** 400 kg Refined Metal [TIER4]
- **HP:** 1000
- **Decor:** 0 (radius 1) [NONE]
- **Noise:** 65 dB (radius 10) [NOISY.TIER2]
- **Power:** 240 W
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 0 kDTU/s
- **Overheat temp:** 2273.15 K (2000 C)
- **Melting point:** 9999 K
- **Floodable:** No
- **Technology:** Solid Transport
- **DLC:** Spaced Out! (EXPANSION1)
- **Conveyor output:** Offset (1, 2)
- **Power input:** Offset (1, 0)
- **Storage:** 20 kg (sealed, insulated, hidden; accepts STORAGE_LOCKERS_STANDARD filter)
- **Special:** Unloads solids from linked rocket cargo onto conveyor rail. Uses SolidConduitDispenser. Supports TreeFilterable for element selection (autoSelectStoredOnLoad = false, dropIncorrectOnFilterChange = false). Must be built adjacent to a Rocket Platform or another Rocket Port. Does not use structure temperature. Scene layer: BuildingBack.
