# Power

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

## Generators

### DevGenerator (Dev Generator)
- Debug building (DebugOnly = true). Not available in normal gameplay.
- Size: 1x1
- HP: 100
- Construction: 25 kg Metal
- Power output: 100,000 W
- Decor: -15 (radius 3)
- No fuel consumption, no heat generation
- No research required

### ManualGenerator (Manual Generator)
- Size: 2x2
- HP: 30
- Construction: 200 kg Metal, build time 30 s
- Power output: 400 W
- Self heat: 1 kDTU/s
- Decor: 0 (radius 1)
- Requires Duplicant operation (isManuallyOperated = true)
- Has automation input port
- Breakable
- Room tag: LightDutyGeneratorType, GeneratorType
- No research required

### Generator (Coal Generator)
- Size: 3x3
- HP: 100
- Construction: 800 kg Metal, build time 120 s
- Power output: 600 W
- Exhaust heat: 8 kDTU/s, self heat: 1 kDTU/s
- Decor: -15 (radius 3)
- Fuel: Coal (Carbon), 1 kg/s, storage capacity 600 kg, refill threshold 100 kg
- Output: Carbon Dioxide, 0.02 kg/s at 383.15 K (not stored, emitted at offset +1,+2)
- Has automation input port
- Power tinkerable
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: Combustion

### WoodGasGenerator (Wood Burner)
- Size: 2x2
- HP: 100
- Construction: 800 kg Metal, build time 120 s
- Power output: 300 W
- Exhaust heat: 8 kDTU/s, self heat: 1 kDTU/s
- Decor: -15 (radius 3)
- Fuel: Lumber (BuildingWood), 1.2 kg/s, delivery capacity 360 kg, refill threshold 180 kg, max stored 720 kg
- Output: Carbon Dioxide, 0.17 kg/s at 383.15 K (not stored, emitted at offset 0,+1)
- Has automation input port
- Power tinkerable
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: Combustion

### PeatGenerator (Peat Burner)
- Size: 3x2
- HP: 100
- Construction: 800 kg Metal, build time 120 s
- Power output: 480 W
- Exhaust heat: 4 kDTU/s, self heat: 0.5 kDTU/s
- Decor: -15 (radius 3)
- Fuel: Peat, 1 kg/s, storage capacity 600 kg, refill threshold 100 kg
- Outputs:
  - Carbon Dioxide: 0.04 kg/s at 383.15 K (not stored, offset 0,+1)
  - Polluted Water: 0.2 kg/s at 313.15 K (not stored, offset +1,+1)
- Has automation input port
- Power tinkerable
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: Combustion
- DLC: Frosty Planet Pack (DLC4) required

### HydrogenGenerator (Hydrogen Generator)
- Size: 4x3
- HP: 100
- Construction: 800 kg Metal, build time 120 s
- Power output: 800 W
- Exhaust heat: 2 kDTU/s, self heat: 2 kDTU/s
- Decor: -15 (radius 3)
- Fuel: Hydrogen (gas conduit input), 0.1 kg/s consumed, conduit consumption rate 1 kg/s, internal buffer 2 kg
- No byproducts
- Gas conduit input at offset -1,0
- Has automation input port
- Power tinkerable
- ignoreBatteryRefillPercent = true
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: AdvancedPowerRegulation

### MethaneGenerator (Natural Gas Generator)
- Size: 4x3
- HP: 100
- Construction: 800 kg Metal (Raw Metals category), build time 120 s
- Power output: 800 W
- Exhaust heat: 2 kDTU/s, self heat: 8 kDTU/s
- Decor: -15 (radius 3)
- Fuel: Natural Gas (Methane via gas conduit), 0.09 kg/s consumed, conduit consumption rate 0.9 kg/s, internal buffer 0.9 kg, storage capacity 50 kg
- Accepts: CombustibleGas tag (Methane and Syngas)
- Outputs:
  - Polluted Water: 0.0675 kg/s at 313.15 K (not stored, offset +1,+1)
  - Carbon Dioxide: 0.0225 kg/s at 383.15 K (stored internally, offset 0,+2)
- Gas conduit input at offset 0,0; gas conduit output at offset +2,+2
- Output conduit filters OUT Methane and Syngas (dispenses CO2)
- Has automation input port
- Power tinkerable
- ignoreBatteryRefillPercent = true
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: ImprovedCombustion

### PetroleumGenerator (Petroleum Generator)
- Size: 3x4
- HP: 100
- Construction: 800 kg Metal (single "Metal" material), build time 480 s
- Power output: 2,000 W
- Exhaust heat: 4 kDTU/s, self heat: 16 kDTU/s
- Decor: -15 (radius 3)
- Fuel: CombustibleLiquid tag (Petroleum, Ethanol, Biodiesel), 2 kg/s consumed, conduit consumption rate 10 kg/s, internal buffer 20 kg
- Outputs:
  - Carbon Dioxide: 0.5 kg/s at 383.15 K (not stored, offset 0,+3)
  - Polluted Water: 0.75 kg/s at 313.15 K (not stored, offset +1,+1)
- Liquid conduit input at offset -1,0
- Has automation input port
- Power tinkerable
- ignoreBatteryRefillPercent = true
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: ImprovedCombustion

### SteamTurbine ([DEPRECATED] Steam Turbine)
- DEPRECATED (Deprecated = true). Cannot be built in new games.
- Size: 5x4
- HP: 30
- Construction: 800 kg Refined Metal + 200 kg Plastic, build time 60 s
- Power output: up to 2,000 W
- Overheat temperature: 1273.15 K (1000 C)
- Decor: 0 (radius 1)
- Input: Steam from tiles below the foundation
- Mechanics (Turbine class):
  - Pump rate: 10 kg/s
  - Required mass flow differential: 3 kg/s
  - Min emit mass: 10 kg
  - Max RPM: 4000, acceleration: 133.33/s, deceleration: 200/s
  - Min generation RPM: 3000
  - Min active temperature: 500 K (226.85 C)
  - Emit temperature: 425 K (151.85 C)
- Has automation input port
- Flippable horizontally
- Makes base solid (5 cells across the bottom row)
- Required skill: CanPowerTinker (for construction)
- Research: N/A (deprecated)

### SteamTurbine2 (Steam Turbine)
- Size: 5x3
- HP: 30
- Construction: 800 kg Refined Metal + 200 kg Plastic, build time 60 s
- Power output: up to 850 W (MAX_WATTAGE)
- Self heat: 4 kDTU/s
- Overheat temperature: 1273.15 K (1000 C)
- Decor: 0 (radius 1)
- Input: Steam from tiles below the foundation (row 2 below building)
- Output: Water via liquid conduit at offset +2,+2; storage capacity 10 kg
- SteamTurbine class mechanics:
  - Source element: Steam; destination element: Water
  - Min active temperature: 398.15 K (125 C)
  - Ideal source temperature: 473.15 K (200 C)
  - Max building temperature: 373.15 K (100 C) -- stops working if building exceeds this
  - Output water temperature: 368.15 K (95 C)
  - Pump rate: 2 kg/s
  - Max self heat: 64 kDTU/s
  - Waste heat to turbine: 10% (wasteHeatToTurbinePercent = 0.1)
  - Power scales linearly: wattage = MAX_WATTAGE * (steamTemp - 368.15) / (473.15 - 368.15)
  - At 125 C steam: ~244 W; at 200 C steam: 850 W (full output)
  - Required mass: 0.001 kg (any trace of steam activates it)
  - Intake range: 5 cells wide, 2 rows below foundation
  - Blocked inputs: cells with liquid or solid block steam intake
- Has automation input port
- Flippable horizontally
- Entombable
- Liquid conduit output
- Required skill: CanPowerTinker (for construction)
- Power tinkerable
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: RenewableEnergy

### SolarPanel (Solar Panel)
- Size: 7x3
- HP: 10 (overridden from default)
- Construction: 200 kg Glass, build time 120 s
- Power output: up to 380 W max
- Conversion: 0.00053 W per lux
- Exhaust heat: 0 kDTU/s, self heat: 0 kDTU/s
- Decor: -15 (radius 3)
- Build location: Anywhere
- Makes base solid (7 cells across bottom row, occupyFoundationLayer = false)
- Expected repair time: 52.5 s
- Room tag: HeavyDutyGeneratorType, GeneratorType
- Research: RenewableEnergy

## Wires

### Wire (Wire)
- Size: 1x1
- HP: 10
- Construction: 25 kg Metal, build time 3 s
- Max wattage: 1,000 W
- Thermal conductivity: 0.05
- Decor: -5 (radius 1)
- Can be built in tiles (BuildLocationRule.Anywhere)
- Not overheatable, not floodable, not entombable
- No research required

### WireBridge (Wire Bridge)
- Size: 3x1
- HP: 30
- Construction: 25 kg Metal, build time 3 s
- Max wattage: 1,000 W
- Decor: -5 (radius 1)
- Not overheatable, not floodable, not entombable
- Rotatable 360 degrees
- Research: PowerRegulation

### HighWattageWire (Heavi-Watt Wire)
- Size: 1x1
- HP: 10
- Construction: 100 kg Metal, build time 3 s
- Max wattage: 20,000 W
- Thermal conductivity: 0.05
- Decor: -25 (radius 6)
- Cannot be built in tiles (BuildLocationRule.NotInTiles)
- Not overheatable, not floodable, not entombable
- Research: AdvancedPowerRegulation

### WireBridgeHighWattage (Heavi-Watt Joint Plate)
- Size: 1x1
- HP: 100
- Construction: 200 kg Metal, build time 3 s
- Max wattage: 20,000 W
- Decor: -25 (radius 6)
- Functions as a foundation tile (SimCellOccupier, doReplaceElement = true)
- Movement speed penalty: PENALTY_3
- Not overheatable, not floodable, not entombable
- Rotatable 360 degrees
- BuildingHP: destroyOnDamaged = true
- Research: AdvancedPowerRegulation

### WireRefined (Conductive Wire)
- Size: 1x1
- HP: 10
- Construction: 25 kg Refined Metal, build time 3 s
- Max wattage: 2,000 W
- Thermal conductivity: 0.05
- Decor: 0 (radius 1)
- Can be built in tiles (BuildLocationRule.Anywhere)
- Not overheatable, not floodable, not entombable
- Research: PrettyGoodConductors

### WireRefinedBridge (Conductive Wire Bridge)
- Size: 3x1
- HP: 30
- Construction: 25 kg Refined Metal, build time 3 s
- Max wattage: 2,000 W
- Decor: -5 (radius 1) (inherits base WireBridge decor)
- Not overheatable, not floodable, not entombable
- Rotatable 360 degrees
- Research: PrettyGoodConductors

### WireRefinedHighWattage (Heavi-Watt Conductive Wire)
- Size: 1x1
- HP: 10
- Construction: 100 kg Refined Metal, build time 3 s
- Max wattage: 50,000 W
- Thermal conductivity: 0.05
- Decor: -20 (radius 4)
- Cannot be built in tiles (BuildLocationRule.NotInTiles)
- Not overheatable, not floodable, not entombable
- Required skill: CanPowerTinker (for construction)
- Research: PrettyGoodConductors

### WireRefinedBridgeHighWattage (Heavi-Watt Conductive Joint Plate)
- Size: 1x1
- HP: 100
- Construction: 100 kg Refined Metal, build time 3 s (mass overridden from parent's 200 kg)
- Max wattage: 50,000 W
- Decor: -25 (radius 6) (inherits from parent WireBridgeHighWattageConfig)
- Functions as a foundation tile
- Movement speed penalty: PENALTY_3
- Not overheatable, not floodable, not entombable
- Rotatable 360 degrees
- Required skill: CanPowerTinker (for construction)
- Research: PrettyGoodConductors

## Batteries

### Battery (Battery)
- Size: 1x2
- HP: 30
- Construction: 200 kg Metal, build time 30 s
- Capacity: 10,000 J (10 kJ)
- Leak rate: 1.6667 J/s (1.667 W, ~0.6 kJ/cycle)
- Exhaust heat: 0.25 kDTU/s, self heat: 1 kDTU/s
- Decor: -10 (radius 2)
- Breakable
- Not entombable
- No research required

### BatteryMedium (Jumbo Battery)
- Size: 2x2
- HP: 30
- Construction: 400 kg Metal, build time 60 s
- Capacity: 40,000 J (40 kJ)
- Leak rate: 3.3333 J/s (3.333 W, ~2 kJ/cycle)
- Exhaust heat: 0.25 kDTU/s, self heat: 1 kDTU/s
- Decor: -15 (radius 3)
- Not entombable
- Research: PowerRegulation

### BatterySmart (Smart Battery)
- Size: 2x2
- HP: 30
- Construction: 200 kg Refined Metal, build time 60 s
- Capacity: 20,000 J (20 kJ)
- Leak rate: 0.6667 J/s (0.667 W, ~0.4 kJ/cycle)
- Exhaust heat: 0 kDTU/s, self heat: 0.5 kDTU/s
- Decor: -15 (radius 3)
- Has automation output port (PORT_ID) for charge level thresholds
- Not entombable
- Research: Acoustics

## Electrobanks

### ElectrobankCharger (Power Bank Charger)
- Size: 2x2
- HP: 30
- Construction: 100 kg Refined Metal, build time 30 s
- Power consumption: 480 W
- Charge rate: 400 W (into the power bank)
- Exhaust heat: 0 kDTU/s, self heat: 1 kDTU/s
- Decor: -10 (radius 2)
- Input: Empty Portable Battery (EmptyPortableBattery tag), storage capacity 1 unit
- Has automation input port
- DLC: Bionic Booster Pack (DLC3) required
- Research: Acoustics

### SmallElectrobankDischarger (Compact Discharger)
- Size: 1x1
- HP: 30
- Construction: 100 kg Metal, build time 10 s
- Power output: 60 W
- Exhaust heat: 0.125 kDTU/s, self heat: 0.5 kDTU/s
- Decor: -10 (radius 2)
- Storage: 20 kg capacity, accepts Power Banks (STORAGEFILTERS.POWER_BANKS)
- Rotatable 360 degrees (BuildLocationRule.OnFoundationRotatable -- can be mounted on walls/ceilings)
- Has automation input port
- DLC: Bionic Booster Pack (DLC3) required
- Research: PowerRegulation

### LargeElectrobankDischarger (Large Discharger)
- Size: 2x2
- HP: 30
- Construction: 400 kg Refined Metal, build time 60 s
- Power output: 480 W
- Exhaust heat: 0.25 kDTU/s, self heat: 1 kDTU/s
- Decor: -15 (radius 3)
- Storage: 20 kg capacity, accepts Power Banks (STORAGEFILTERS.POWER_BANKS)
- Has automation input port
- DLC: Bionic Booster Pack (DLC3) required
- Research: PrettyGoodConductors

## Transformers

### PowerTransformerSmall (Power Transformer)
- Size: 2x2
- HP: 30
- Construction: 200 kg Metal (Raw Metals category), build time 30 s
- Throughput limit: 1,000 W
- Internal battery capacity: 1,000 J (matches wattage rating)
- Exhaust heat: 0 kDTU/s (overridden from 0.25), self heat: 1 kDTU/s
- Decor: -10 (radius 2)
- Power input offset: 0,+1 (top-left); power output offset: +1,0 (bottom-right)
- Flippable horizontally
- Entombable
- Research: AdvancedPowerRegulation

### PowerTransformer (Large Power Transformer)
- Size: 3x2
- HP: 30
- Construction: 200 kg Refined Metal, build time 30 s
- Throughput limit: 4,000 W
- Internal battery capacity: 4,000 J (matches wattage rating)
- Exhaust heat: 0 kDTU/s, self heat: 1 kDTU/s
- Decor: -10 (radius 2)
- Power input offset: -1,+1 (top-left); power output offset: +1,0 (bottom-right)
- Flippable horizontally
- Entombable
- Research: PrettyGoodConductors

## Switches

### Switch (Switch)
- Size: 1x1
- HP: 10
- Construction: 100 kg Metal, build time 30 s
- Decor: 0 (radius 1)
- Not overheatable, not floodable
- Build location: Anywhere
- Manual toggle (CircuitSwitch, manuallyControlled = false in config but the class provides toggle UI)
- Acts on wire layer (ObjectLayer.Wire)
- Research: PowerRegulation

### LogicPowerRelay (Power Shutoff)
- Size: 1x1
- HP: 10
- Construction: 100 kg Metal, build time 30 s
- Decor: 0 (radius 1)
- Not overheatable, not floodable, not entombable
- Build location: Anywhere
- Has automation input port:
  - Green signal: allow power through
  - Red signal: block power
- Acts on wire layer (ObjectLayer.Wire)
- Research: AdvancedPowerRegulation

### TemperatureControlledSwitch (Thermo Switch)
- DEPRECATED (Deprecated = true). Replaced by automation-category sensor.
- Size: 1x1
- HP: 30
- Construction: 200 kg Metal, build time 30 s
- Decor: -5 (radius 1)
- Not overheatable, not floodable
- Build location: Anywhere
- Temperature range: 0 K to 573.15 K (0 to 300 C)
- Acts on wire layer (ObjectLayer.Wire)
- No research (deprecated)

### PressureSwitchLiquid (Hydro Switch)
- DEPRECATED (Deprecated = true). Replaced by automation-category sensor.
- Size: 1x1
- HP: 30
- Construction: 200 kg Metal, build time 30 s
- Decor: -5 (radius 1)
- Not overheatable, not floodable
- Build location: Anywhere
- Pressure range: 0 to 2,000 kg
- Default threshold: 500 kg, activates below threshold
- Monitors liquid state
- Acts on wire layer (ObjectLayer.Wire)
- No research (deprecated)

### PressureSwitchGas (Atmo Switch)
- DEPRECATED (Deprecated = true). Replaced by automation-category sensor.
- Size: 1x1
- HP: 30
- Construction: 200 kg Metal, build time 30 s
- Decor: -5 (radius 1)
- Not overheatable, floodable (unlike other switches)
- Build location: Anywhere
- Pressure range: 0 to 2 kg
- Default threshold: 1 kg, activates below threshold
- Monitors gas state
- Acts on wire layer (ObjectLayer.Wire)
- No research (deprecated)
