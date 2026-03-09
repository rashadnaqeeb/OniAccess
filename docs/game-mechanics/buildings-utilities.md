# Utilities

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

## Wood Heater (Campfire)

Consumes Wood to heat chilly surroundings.

- Size: 1x2
- HP: 100
- Build time: 10s
- Materials: 100 kg Raw Metal
- Decor: +20 (radius 4) when active; 0 when inactive
- Not overheat-able (OverheatTemperature = 10000 K, Overheatable = false)
- Manually operated
- Floodable, entombable
- Light: 450 lux, range 6
- DLC: The Frosty Planet Pack required
- Tech: Jobs
- POI-unlockable

### Fuel and exhaust

- Fuel: Wood (BuildingWood tag)
- Fuel consumption: 0.025 kg/s
- Fuel storage: 45 kg (refill threshold: 18 kg)
- Initial fuel mass placed on construction: 5 kg
- Exhaust: Carbon Dioxide at 0.004 kg/s, temperature 303.15 K (30 C)

### Heating

- Uses DirectVolumeHeater: 20,000 DTU/s (20 kDTU/s)
- Heating area: 9 wide x 4 tall
- Maximum external temperature: 343.15 K (70 C) -- stops heating above this
- SelfHeatKilowatts and ExhaustKilowatts both 0 (all heat via DirectVolumeHeater)

### Warmth and cold immunity

- Warmth provider range: (-4, 0) to (4, 3)
- Cold immunity range: offsets (-1,0), (1,0) and (0,0)
- Tags: WarmingStation, Decoration

## Dev Heater (DevHeater)

Debug-only heater for testing. Generates on-demand heat.

- Size: 1x1
- HP: 100
- Build time: 3s
- Materials: 25 kg Metal (any)
- Decor: -15 (radius 3)
- No power required
- Not overheat-able
- Not floodable
- Debug only (DebugOnly = true, not available in normal gameplay)
- Uses DirectVolumeHeater (default settings)
- Tags: DevBuilding

## Ice Liquefier (IceKettle)

Consumes Wood to melt Ice into Water, which can be bottled for transport.

- Size: 2x2
- HP: 100
- Build time: 10s
- Materials: 400 kg Raw Metal
- Decor: 0
- Overheat temperature: 1600 K (1326.85 C)
- Not floodable, entombable
- Manually operated
- DLC: The Frosty Planet Pack required
- Tech: Temperature Modulation
- POI-unlockable

### Heat generation

- SelfHeatKilowatts: 1.5 kDTU/s (3.75 * 0.4)
- ExhaustKilowatts: 2.25 kDTU/s (3.75 - 1.5)
- Total: 3.75 kDTU/s

### Ice melting

- Input: Ice (SimHashes.Ice)
- Batch size: 100 kg per batch
- Ice storage: 1000 kg (refill threshold: 100 kg)
- Melt rate: 20 kg/s
- Target output temperature: 298.15 K (25 C)
- Output storage: 500 kg (removable by Duplicants)

### Fuel

- Fuel: Wood (BuildingWood tag)
- Energy per unit of lumber for melting: 4000 kDTU
- Fuel capacity: 153 kg (ceiling of 152.80188)
- Exhaust: Carbon Dioxide at 0.142 kg per unit of lumber burned

## Space Heater (SpaceHeater)

Radiates a moderate amount of Heat. Power adjustable via slider.

- Size: 2x2
- HP: 30
- Build time: 30s
- Materials: 400 kg Metal (any)
- Decor: +10 (radius 2)
- Overheat temperature: 398.15 K (125 C)
- Power: 120-240 W (adjustable via slider)
- Logic input port at offset (1, 0)
- Tech: Temperature Modulation

### Heat generation (adjustable)

The Space Heater uses a slider (0-100%) that interpolates between min and max values:

| Setting | Power | Self-heat | Exhaust heat | Total heat |
|---------|-------|-----------|--------------|------------|
| Minimum (slider 0%) | 120 W | 16 kDTU/s | 2 kDTU/s | 18 kDTU/s |
| Maximum (slider 100%) | 240 W | 32 kDTU/s | 4 kDTU/s | 36 kDTU/s |

- SelfHeatKilowatts and ExhaustKilowatts in BuildingDef are both 0 (heat is generated programmatically by SpaceHeater component)
- Target temperature: 343.15 K (70 C) -- stops heating when average surrounding gas temperature reaches this
- Does NOT heat liquids (heatLiquid = false)
- Monitoring radius: 2 cells

### Warmth and cold immunity

- Warmth provider range: (-4, -4) to (5, 5)
- Cold immunity range: offsets (-1,0), (2,0) and (0,0), (1,0)
- Tag: WarmingStation

## Liquid Tepidizer (LiquidHeater)

Warms large bodies of Liquid. Must be fully submerged.

- Size: 4x1
- HP: 30
- Build time: 30s
- Materials: 400 kg Metal (any)
- Decor: -10 (radius 2)
- Overheat temperature: 398.15 K (125 C)
- Power: 960 W
- Not floodable
- Logic input port at offset (1, 0)
- Tech: Liquid Temperature

### Heat generation

- ExhaustKilowatts: 4000 kDTU/s
- SelfHeatKilowatts: 64 kDTU/s
- Total: 4064 kDTU/s
- Target temperature: 358.15 K (85 C) -- stops heating when average surrounding liquid temperature reaches this
- Minimum cell mass for heating: 400 kg
- Heats liquid only (SetLiquidHeater called)
- Monitoring radius: 2 cells

## Hydrofan (LiquidCooledFan)

Duplicant-operated fan that uses Water to cool surrounding gas. Deprecated -- hidden from build menu but still functional if already built.

- Size: 2x2
- HP: 30
- Build time: 10s
- Materials: 100 kg Metal (any)
- Decor: 0
- Not overheat-able
- Manually operated
- Deprecated (Deprecated = true)
- No tech listed (was part of Temperature Modulation before deprecation)

### Cooling mechanics

- Cooling rate: 80 kDTU/s
- Minimum cooled temperature: 290 K (16.85 C)
- Cooling range: (-2, 0) to (2, 4)
- Minimum environment mass: 0.25 kg per cell
- Water capacity: 500 kg (refill threshold: 50 kg)
- Water storage (internal): 100 kg
- Gas consumption radius: 8 cells
- Water consumption rate: ~0.0412 kg per kJ of heat removed (derived from 580 kcal * 4.184 J/kcal * 0.01 efficiency factor)
- Duplicant work time: 20s per work session
- SelfHeatKilowatts: 0, ExhaustKilowatts: 0

## Ice-E Fan (IceCooledFan)

Duplicant-operated fan that uses Ice to cool surrounding gas.

- Size: 2x2
- HP: 30
- Build time: 30s
- Materials: 400 kg Metal (any)
- Decor: 0
- Not overheat-able
- Manually operated
- Minimum operating temperature: 273.15 K (0 C) -- building itself must be above freezing
- Tech: Temperature Modulation

### Cooling mechanics

- Cooling rate: 32 kDTU/s
- SelfHeatKilowatts: -8 kDTU/s (negative = cooling, 25% of COOLING_RATE)
- ExhaustKilowatts: -24 kDTU/s (negative = cooling, 75% of COOLING_RATE)
- Target temperature: 278.15 K (5 C) -- stops cooling at this temp
- Minimum cooled temperature: 278.15 K (5 C)
- Cooling range: (-2, 0) to (2, 4)
- Minimum environment mass: 0.25 kg per cell
- Ice storage: 50 kg (refill threshold: 10 kg, minimum delivery: 10 kg)
- Accepts any IceOre tag (Ice, Polluted Ice, Snow, etc.)
- Meltwater output storage: 50 kg
- Structure temperature extents overridden to offsets: (-1,-1), (1,-1), (-1,1), (1,1)

## Ice Maker (IceMachine)

Converts Water into Ice or Snow.

- Size: 2x3
- HP: 30
- Build time: 30s
- Materials: 400 kg Metal (any)
- Decor: 0
- Overheat temperature: 1600 K (default)
- Power: 240 W
- Tech: Temperature Modulation

### Conversion mechanics

- Water input storage: 60 kg (refill threshold: 12 kg, minimum: 10 kg)
- Ice/Snow output storage: 300 kg (removable)
- Target ice temperature: 253.15 K (-20 C)
- Heat removal rate: 80 kDTU/s
- Output options: Ice or Snow (selectable)
- SelfHeatKilowatts: 12 kDTU/s
- ExhaustKilowatts: 4 kDTU/s

## Thermo Regulator (AirConditioner)

Cools Gas piped through it, but outputs Heat in its immediate vicinity.

- Size: 2x2
- HP: 100
- Build time: 120s
- Materials: 200 kg Metal (any)
- Decor: 0
- Overheat temperature: 1600 K (default)
- Power: 240 W (power input at offset (1, 0))
- Gas conduit input and output
- Flippable horizontally (PermittedRotations.FlipH)
- Thermal conductivity: 5 W/(m*K)
- Logic input port at offset (0, 1)
- SelfHeatKilowatts: 0 (all heat expelled as exhaust into environment)
- Tech: HVAC

### Cooling mechanics

- Temperature delta: -14 K (cools gas passing through by 14 degrees)
- Maximum environment delta: -50 K
- Gas consumption rate: 1 kg/s (standard gas conduit rate)
- Heat output = mass * specific_heat_capacity * 14 (expelled into surroundings)

## Thermo Aquatuner (LiquidConditioner)

Cools Liquid piped through it, but outputs Heat in its immediate vicinity.

- Size: 2x2
- HP: 100
- Build time: 120s
- Materials: 1200 kg Metal (any)
- Decor: 0
- Overheat temperature: 398.15 K (125 C)
- Power: 1200 W (power input at offset (1, 0))
- Liquid conduit input and output
- Not floodable
- Flippable horizontally (PermittedRotations.FlipH)
- Logic input port at offset (1, 1)
- SelfHeatKilowatts: 0 (all heat expelled as exhaust into environment)
- Tech: Liquid Temperature

### Cooling mechanics

- Temperature delta: -14 K (cools liquid passing through by 14 degrees)
- Maximum environment delta: -50 K
- Liquid consumption rate: 10 kg/s (standard liquid conduit rate)
- Internal storage: 20 kg (2x consumption rate)
- Heat output = mass * specific_heat_capacity * 14 (expelled into surroundings)

## Ore Scrubber (OreScrubber)

Kills a significant amount of Germs present on Raw Ore. Duplicants pass through it carrying items.

- Size: 3x3
- HP: 30
- Build time: 30s
- Materials: 200 kg Metal
- Decor: +10 (radius 2)
- No power required
- Gas conduit input (building def InputConduitType = Gas)
- Directional (DirectionControl -- Duplicants pass through in set direction)
- Tech: Liquid Filtering

### Scrubbing mechanics

- Consumed element: Chlorine Gas
- Mass consumed per use: 0.07 kg
- Disease removal per use: 480,000 germs (4x WashBasin's 120,000)
- Internal chlorine storage: 10 kg (ConduitConsumer capacity)
- Chlorine consumption rate from conduit: 1 kg/s
- Work time per scrub: 10.2 seconds
- Tracks uses

Note: The ConduitConsumer.conduitType is set to Liquid in the code despite InputConduitType being Gas -- this appears to be a code oddity; the building takes Chlorine Gas via gas pipe input.

## Oil Well (OilWellCap)

Extracts Crude Oil using clean Water. Must be built atop an Oil Reservoir.

- Size: 4x4
- HP: 100
- Build time: 120s
- Materials: 200 kg Refined Metal
- Decor: 0
- Overheat temperature: 2273.15 K (2000 C)
- Power: 240 W (power input at offset (1, 1))
- Liquid conduit input at offset (0, 1)
- Not floodable
- Logic input port at offset (0, 0)
- Build rule: must attach to Oil Reservoir (BuildLocationRule.BuildingAttachPoint, tag OilWell)
- Scene layer: BuildingFront
- SelfHeatKilowatts: 2 kDTU/s
- Tech: Plastics

### Element conversion

- Input: 1 kg/s Water (via liquid conduit, capacity 10 kg, wrong element dumped)
- Output: 3.333 kg/s Crude Oil at 363.15 K (90 C), offset (2, 1.5)
- Conduit consumption rate: 2 kg/s

### Pressure mechanics

- Gas element: Methane (Natural Gas)
- Gas temperature: 573.15 K (300 C)
- Gas addition rate: 0.0333 kg/s (1/30)
- Maximum gas pressure: 80 kg
- Overpressure time: 2400s (40 minutes) to reach max from empty at add rate
- Pressure release rate: 0.444 kg/s
- Pressure release time: 180s (3 minutes)

## Tempshift Plate (ThermalBlock)

Accelerates or buffers Heat dispersal based on construction material. Passive -- no power, no operation.

- Size: 1x1
- HP: 30
- Build time: 120s
- Materials: 800 kg Any Buildable material
- Decor: 0
- Not floodable, not entombable, not overheat-able
- No repair (BaseTimeUntilRepair = -1)
- Placement: NotInTiles (cannot overlap tiles)
- Layer: Backwall (behind buildings, replaceable with other backwall/floor tiles)
- No power required
- Tech: Refined Objects

### Thermal mechanics

- Structure temperature extents overridden to offsets: (-1,-1), (1,-1), (-1,1), (1,1) -- affects a 3x3 area centered on the plate
- Thermal properties depend entirely on the construction material chosen (thermal conductivity, specific heat capacity, mass)
- High-conductivity materials (e.g., Diamond, Aluminum) speed heat transfer; insulating materials (e.g., Insulation) slow it

## Sweepy's Dock (SweepBotStation)

Deploys an automated Sweepy Bot to sweep up Solid debris and Liquid spills. Dock stores collected items.

- Size: 2x2
- HP: 30
- Build time: 30s
- Materials: 75 kg Refined Metal (TIER2 100 kg minus Sweepy's 25 kg mass)
- Decor: -10 (radius 2)
- Power: 240 W (recharges Sweepy)
- Not floodable
- SelfHeatKilowatts: 1 kDTU/s
- ExhaustKilowatts: 0
- Tech: Artificial Friends

### Storage

- Sweepy internal storage: 25 kg (not player-removable, used for Sweepy parts/battery)
- Dock output storage: 1000 kg (player-removable, stores swept debris)
- Accepts standard storage locker items (STORAGEFILTERS.STORAGE_LOCKERS_STANDARD)
- Sweepy fetches debris in sweep-only mode
- Shows capacity status item
- Shows Sweepy's name overlay (CharacterOverlay)
