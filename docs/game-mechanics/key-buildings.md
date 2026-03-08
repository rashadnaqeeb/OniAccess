# Key Buildings Reference

Per-building specs derived from decompiled source code. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius).

---

## Oxygen Production

### Electrolyzer
- **ID:** `Electrolyzer`
- **Source:** `ElectrolyzerConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 120 W consumed
- **Input:** 1 kg/s Water (liquid pipe)
- **Output:** 0.888 kg/s Oxygen + 0.112 kg/s Hydrogen (emitted to world at offset 0,1)
- **Output temperature:** 343.15 K (70 C), fixed -- does not use entity temperature or input temperature
- **Self-heating:** 1 kDTU/s
- **Exhaust heat:** 0.25 kDTU/s
- **Overheat:** 800 K (default, not set explicitly -- uses melting_point param)
- **Storage:** 2 kg internal, max gas mass 1.8 kg before output pressure stalls

### Algae Terrarium (AirFilter)
- **ID:** `AirFilter`
- **Source:** `AirFilterConfig.cs`
- **Dimensions:** 1w x 1h
- **Power:** 5 W consumed
- **Input:** 0.1 kg/s Polluted Oxygen (consumed from atmosphere, radius 3) + 2/15 kg/s (~0.133 kg/s) Sand/Regolith filter (manual delivery)
- **Output:** 0.09 kg/s Oxygen (emitted to world) + 0.143 kg/s Clay (stored, dropped at 10 kg)
- **Output temperature:** passthrough (uses input temperature)
- **Self-heating:** 0.5 kDTU/s
- **Exhaust heat:** 0.125 kDTU/s
- **Overheat:** not overheatable
- **Storage:** 200 kg, filter capacity 320 kg

---

## Liquid Processing

### Aquatuner (Liquid Conditioner)
- **ID:** `LiquidConditioner`
- **Source:** `LiquidConditionerConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 1200 W consumed
- **Input:** liquid pipe (consumption rate 10 kg/s -- full pipe throughput)
- **Output:** same liquid, cooled by 14 C (temperature delta -14)
- **Max environment delta:** -50 C (building heats surroundings by the removed energy)
- **Self-heating:** 0 kDTU/s (all heat goes to environment via AirConditioner component)
- **Overheat:** 398.15 K (125 C)
- **Storage:** 20 kg (2x consumption rate)
- **Notes:** The 1200 W is converted entirely to heat dumped into the building's tile. Total heat moved = mass * SHC * 14. With 10 kg/s water (SHC 4.179): 585 kDTU/s cooling, producing 585 kDTU/s + 1200 W waste heat at the building

### Water Sieve (WaterPurifier)
- **ID:** `WaterPurifier`
- **Source:** `WaterPurifierConfig.cs`
- **Dimensions:** 4w x 3h
- **Power:** 120 W consumed
- **Input:** 5 kg/s Dirty Water (liquid pipe) + 1 kg/s Sand/Filter (manual delivery)
- **Output:** 5 kg/s Water (stored, piped out) + 0.2 kg/s Polluted Dirt (stored, dropped at 10 kg)
- **Output temperature:** passthrough (minOutputTemp 0, uses weighted input average)
- **Self-heating:** 4 kDTU/s
- **Filter capacity:** 1200 kg storage for filter material
- **Conduit input capacity:** 20 kg

### Desalinator
- **ID:** `Desalinator`
- **Source:** `DesalinatorConfig.cs`
- **Dimensions:** 4w x 3h
- **Power:** 480 W consumed
- **Salt Water mode:** 5 kg/s Salt Water in -> 4.65 kg/s Water + 0.35 kg/s Salt out
- **Brine mode:** 5 kg/s Brine in -> 3.5 kg/s Water + 1.5 kg/s Salt out
- **Output temperature:** passthrough (uses input temperature)
- **Self-heating:** 8 kDTU/s
- **Max salt storage:** 945 kg (must be emptied by dupe)
- **Conduit input capacity:** 20 kg

### Carbon Skimmer (CO2 Scrubber)
- **ID:** `CO2Scrubber`
- **Source:** `CO2ScrubberConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 120 W consumed
- **Input:** 0.3 kg/s Carbon Dioxide (consumed from atmosphere, radius 3) + 1 kg/s Water (liquid pipe)
- **Output:** 1 kg/s Dirty Water (stored, piped out on liquid output)
- **Output temperature:** passthrough (minOutputTemp 0)
- **Self-heating:** 1 kDTU/s
- **Storage:** 30,000 kg
- **Notes:** Passive CO2 consumer rate is 0.6 kg/s (buffered), converter consumes 0.3 kg/s. Water is consumed from pipe at 2 kg/s rate

---

## Temperature Control

### Thermo Regulator (Gas Conditioner)
- **ID:** `AirConditioner`
- **Source:** `AirConditionerConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 240 W consumed
- **Input:** gas pipe (consumption rate 1 kg/s)
- **Output:** same gas, cooled by 14 C (temperature delta -14)
- **Max environment delta:** -50 C (building heats surroundings by the removed energy)
- **Self-heating:** 0 kDTU/s (all heat goes to environment via AirConditioner component)
- **Overheat:** default (1600 K melting point of construction metal)
- **Notes:** Gas equivalent of the Aquatuner. Same AirConditioner component with identical -14 C delta and -50 C max environment delta. The 240 W is converted entirely to heat dumped into the building's tile. Lower throughput than the Aquatuner (1 kg/s gas vs 10 kg/s liquid) means less total heat moved per second

### Space Heater
- **ID:** `SpaceHeater`
- **Source:** `SpaceHeaterConfig.cs`, `SpaceHeater.cs`
- **Dimensions:** 2w x 2h
- **Power:** 120-240 W consumed (adjustable via slider)
- **Target temperature:** 343.15 K (70 C) -- stops heating when average nearby gas temperature reaches this
- **Self-heating:** 16-32 kDTU/s (scales linearly with power slider)
- **Exhaust heat:** 2-4 kDTU/s (scales linearly with power slider)
- **Total heat output:** 18-36 kDTU/s
- **Overheat:** 398.15 K (125 C)
- **Effect range:** -4,-4 to 5,5 (9x9 area for warmth provider)
- **Notes:** Player sets power consumption via slider (120 W min, 240 W max). Heat output scales proportionally. Monitors nearby gas cells and stops when average temperature reaches target. Also provides Cold Immunity to dupes adjacent to the building

### Ice-E Fan
- **ID:** `IceCooledFan`
- **Source:** `IceCooledFanConfig.cs`, `IceCooledFan.cs`
- **Dimensions:** 2w x 2h
- **Power:** none (dupe-operated, no electrical power)
- **Cooling rate:** 32 kDTU/s (transferred from environment into stored ice)
- **Self-heating:** -8 kDTU/s (25% of cooling rate, applied as negative self-heat)
- **Exhaust heat:** -24 kDTU/s (75% of cooling rate, applied as negative exhaust)
- **Ice input:** any ice ore (manual delivery), 50 kg capacity, refill at 10 kg
- **Min cooled temperature:** 278.15 K (5 C) -- stops cooling when environment reaches this
- **Cooling range:** -2,0 to 2,4
- **Min environment mass:** 0.25 kg per cell
- **Overheat:** not overheatable
- **Operated by:** dupe (manually operated)
- **Notes:** Heats ice by transferring thermal energy from the environment. When ice warms past its melting point it becomes liquid, which is then dropped on the floor. No power required but ties up a dupe while running

### Liquid Cooled Fan (Deprecated)
- **ID:** `LiquidCooledFan`
- **Source:** `LiquidCooledFanConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** none (dupe-operated, no electrical power)
- **Cooling rate:** 80 kDTU/s
- **Self-heating:** 0 kDTU/s
- **Exhaust heat:** 0 kDTU/s
- **Water input:** Water (manual delivery), 500 kg capacity, refill at 50 kg
- **Min cooled temperature:** 290 K (16.85 C)
- **Cooling range:** -2,0 to 2,4
- **Gas consumption radius:** 8 cells
- **Overheat:** not overheatable
- **Operated by:** dupe (manually operated)
- **Notes:** Deprecated building (`Deprecated = true`). Consumes water to cool nearby gas. Work time per cycle is 20s

---

## Power Generation

### Manual Generator
- **ID:** `ManualGenerator`
- **Source:** `ManualGeneratorConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 400 W generated
- **Input:** dupe labor
- **Output:** power only
- **Self-heating:** 1 kDTU/s

### Coal Generator
- **ID:** `Generator`
- **Source:** `GeneratorConfig.cs`
- **Dimensions:** 3w x 3h
- **Power:** 600 W generated
- **Input:** 1 kg/s Coal/Carbon (manual delivery)
- **Output:** 0.02 kg/s Carbon Dioxide at 383.15 K (110 C)
- **Self-heating:** 1 kDTU/s
- **Exhaust heat:** 8 kDTU/s
- **Storage:** 600 kg coal

### Natural Gas Generator
- **ID:** `MethaneGenerator`
- **Source:** `MethaneGeneratorConfig.cs`
- **Dimensions:** 4w x 3h
- **Power:** 800 W generated
- **Input:** 0.09 kg/s Natural Gas (gas pipe)
- **Output:** 0.0675 kg/s Polluted Water at 313.15 K (40 C) + 0.0225 kg/s Carbon Dioxide at 383.15 K (110 C)
- **Self-heating:** 8 kDTU/s
- **Exhaust heat:** 2 kDTU/s
- **Storage:** 50 kg
- **Conduit consumption rate:** 0.9 kg/s (fills buffer)
- **Notes:** CO2 stored internally then dispensed via gas output pipe (filters out Methane/Syngas). Polluted Water emitted to world

### Hydrogen Generator
- **ID:** `HydrogenGenerator`
- **Source:** `HydrogenGeneratorConfig.cs`
- **Dimensions:** 4w x 3h
- **Power:** 800 W generated
- **Input:** 0.1 kg/s Hydrogen (gas pipe)
- **Output:** power only (no byproducts)
- **Self-heating:** 2 kDTU/s
- **Exhaust heat:** 2 kDTU/s
- **Storage:** 2 kg hydrogen buffer
- **Conduit consumption rate:** 1 kg/s

### Petroleum Generator
- **ID:** `PetroleumGenerator`
- **Source:** `PetroleumGeneratorConfig.cs`
- **Dimensions:** 3w x 4h
- **Power:** 2000 W generated
- **Input:** 2 kg/s Petroleum or other combustible liquid (liquid pipe)
- **Output:** 0.5 kg/s Carbon Dioxide at 383.15 K (110 C) + 0.75 kg/s Polluted Water at 313.15 K (40 C)
- **Self-heating:** 16 kDTU/s
- **Exhaust heat:** 4 kDTU/s
- **Storage:** 20 kg liquid buffer
- **Conduit consumption rate:** 10 kg/s (fills buffer)

### Steam Turbine
- **ID:** `SteamTurbine2`
- **Source:** `SteamTurbineConfig2.cs`, `SteamTurbine.cs`
- **Dimensions:** 5w x 3h
- **Power:** up to 850 W generated (variable based on steam temperature)
- **Input:** Steam from 5 intake cells directly below the building (row at y-2)
- **Output:** Water at 368.15 K (95 C) via liquid output pipe
- **Pump rate:** 2 kg/s total across all 5 intake cells
- **Min active temperature:** 398.15 K (125 C) -- steam must be at least this hot
- **Ideal source temperature:** 473.15 K (200 C)
- **Max building temperature:** 373.15 K (100 C) -- shuts down if building exceeds this
- **Self-heating:** 4 kDTU/s + up to 64 kDTU/s from waste heat (10% of extracted thermal energy)
- **Overheat:** 1273.15 K (1000 C)
- **Storage:** 10 kg liquid
- **Required mass:** 0.001 kg minimum steam per intake cell
- **Notes:** Intake cells are the 5 cells in the row 2 below building origin. Blocked by liquids or solids. Power scales with steam temperature above minimum. Outputs water to liquid pipe, not to world

---

## Refineries

### Metal Refinery
- **ID:** `MetalRefinery`
- **Source:** `MetalRefineryConfig.cs`
- **Dimensions:** 3w x 4h
- **Power:** 1200 W consumed
- **Input:** 100 kg ore per recipe batch + 400 kg minimum coolant (any liquid, via pipe)
- **Output:** 100 kg refined metal per batch (recipe time 40s)
- **Coolant:** liquid in, heated liquid out (via liquid pipe). 80% of recipe heat goes to coolant (`thermalFudge = 0.8`)
- **Self-heating:** 16 kDTU/s
- **Output storage:** 2000 kg
- **Coolant buffer:** 800 kg conduit consumer capacity
- **Steel recipe:** 70 kg Iron + 20 kg Refined Carbon + 10 kg Lime -> 100 kg Steel
- **Operated by:** dupe (manually operated)
- **Notes:** Recipes produce output at average temperature of inputs. The 80% heat-to-coolant means coolant gets very hot -- use a liquid that won't change state

### Oil Refinery
- **ID:** `OilRefinery`
- **Source:** `OilRefineryConfig.cs`
- **Dimensions:** 4w x 4h
- **Power:** 480 W consumed
- **Input:** 10 kg/s Crude Oil (liquid pipe)
- **Output:** 5 kg/s Petroleum (stored, piped out) at 348.15 K (75 C) + 0.09 kg/s Natural Gas at 348.15 K (75 C, emitted to world)
- **Self-heating:** 8 kDTU/s
- **Exhaust heat:** 2 kDTU/s
- **Overpressure:** stops at 5 kg natural gas in tile
- **Operated by:** dupe (manually operated)
- **Conduit input capacity:** 100 kg crude oil buffer

### Kiln
- **ID:** `Kiln`
- **Source:** `KilnConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** none (no electrical power)
- **Self-heating:** 4 kDTU/s
- **Exhaust heat:** 16 kDTU/s
- **Overheat:** not overheatable
- **Output temperature:** 353.15 K (80 C), heated
- **Operated by:** automatic (not dupe-operated)
- **Ceramic recipe:** 100 kg Clay + 25 kg fuel (Lumber/Carbon/Peat) -> 100 kg Ceramic (40s)
- **Refined Carbon recipe:** variable input (200 kg Lumber, 200 kg Carbon, 125 kg Peat, or 300 kg other wood) -> 100 kg Refined Carbon (40s)
- **Storage:** 2400 kg ceramic output capacity
- **Notes:** No power required. Both recipes take 40s. Output is heated to 353.15 K regardless of input temperature. Fuel for ceramic recipe accepts any BasicWoods tag plus Carbon and Peat

### Glass Forge
- **ID:** `GlassForge`
- **Source:** `GlassForgeConfig.cs`
- **Dimensions:** 5w x 4h
- **Power:** 1200 W consumed
- **Self-heating:** 16 kDTU/s
- **Glass recipe:** 100 kg Sand -> 25 kg Molten Glass (40s)
- **Output storage:** 2000 kg
- **Output:** liquid pipe (Molten Glass dispensed via conduit)
- **Operated by:** dupe (manually operated)
- **Notes:** Output is Molten Glass (liquid), not solid Glass. The output uses TemperatureOperation.Melted, so it comes out at glass melting temperature. Dispensed via liquid output pipe at offset 1,3. Only 25% mass conversion (100 kg Sand becomes 25 kg Molten Glass)

### Rock Crusher
- **ID:** `RockCrusher`
- **Source:** `RockCrusherConfig.cs`
- **Dimensions:** 4w x 4h
- **Power:** 240 W consumed
- **Self-heating:** 16 kDTU/s
- **Operated by:** dupe (manually operated)
- **Sand recipe:** 100 kg any Crushable mineral -> 100 kg Sand (40s)
- **Metal ore recipe:** 100 kg metal ore -> 50 kg refined metal + 50 kg Sand (40s, 50% efficiency)
- **Lime recipes:**
  - 5 kg Egg Shell -> 5 kg Lime (40s)
  - 10 kg Pokeshell Molt -> 10 kg Lime (40s)
  - 100 kg Fossil -> 5 kg Lime + 95 kg Sedimentary Rock (40s)
- **Salt recipe:** 100 kg Salt -> 0.005 kg Table Salt + ~100 kg Sand (40s)
- **Fullerene recipe (DLC1):** 100 kg Fullerene -> 90 kg Graphite + 10 kg Sand (40s)
- **Electrobank recipe (DLC3):** 1 Garbage Electrobank -> 100 kg Abyssalite (40s)
- **Notes:** All recipes take 40s. Metal refining at 50% efficiency is worse than the Metal Refinery (100%) but requires no coolant and is available earlier. Lime recipes are important for Steel production

---

## Pumps

### Gas Pump
- **ID:** `GasPump`
- **Source:** `GasPumpConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 240 W consumed
- **Pump rate:** 0.5 kg/s (all gases)
- **Self-heating:** 0 kDTU/s
- **Storage:** 1 kg
- **Consumption radius:** 2

### Mini Gas Pump
- **ID:** `GasMiniPump`
- **Source:** `GasMiniPumpConfig.cs`
- **Dimensions:** 1w x 2h
- **Power:** 60 W consumed
- **Pump rate:** 0.05 kg/s (all gases)
- **Self-heating:** 0 kDTU/s
- **Storage:** 0.1 kg
- **Consumption radius:** 2

### Liquid Pump
- **ID:** `LiquidPump`
- **Source:** `LiquidPumpConfig.cs`
- **Dimensions:** 2w x 2h
- **Power:** 240 W consumed
- **Pump rate:** 10 kg/s (all liquids)
- **Self-heating:** 2 kDTU/s
- **Storage:** 20 kg
- **Consumption radius:** 2

### Mini Liquid Pump
- **ID:** `LiquidMiniPump`
- **Source:** `LiquidMiniPumpConfig.cs`
- **Dimensions:** 1w x 2h
- **Power:** 60 W consumed
- **Pump rate:** 1 kg/s (all liquids)
- **Self-heating:** 0.5 kDTU/s
- **Storage:** 2 kg
- **Consumption radius:** 2

---

## SPOM Reference (Self-Powered Oxygen Module)

Key ratios derived from the building specs above:

- 1 Electrolyzer consumes 1 kg/s Water, produces 0.888 kg/s O2 + 0.112 kg/s H2
- 1 Hydrogen Generator consumes 0.1 kg/s H2, produces 800 W
- 1 Electrolyzer needs 120 W, 1 Gas Pump needs 240 W
- H2 production rate (0.112 kg/s) sustains 1.12 Hydrogen Generators (896 W)
- Typical SPOM: 1 Electrolyzer + 1 Hydrogen Generator + 2 Gas Pumps = 120 + 240 + 240 = 600 W demand, 800 W supply = 200 W surplus
- O2 output of 0.888 kg/s supports ~8.88 dupes (each dupe consumes ~0.1 kg/s O2)

---

## Common Temperature Constants

| Kelvin | Celsius | Used by |
|--------|---------|---------|
| 278.15 | 5 | Ice-E Fan min cooled temperature |
| 290 | 16.85 | Liquid Cooled Fan min cooled temperature |
| 313.15 | 40 | Polluted Water output (Nat Gas Gen, Petrol Gen) |
| 343.15 | 70 | Electrolyzer O2/H2 output, Space Heater target temperature |
| 348.15 | 75 | Oil Refinery Petroleum/Nat Gas output |
| 353.15 | 80 | Kiln output temperature |
| 368.15 | 95 | Steam Turbine Water output |
| 373.15 | 100 | Steam Turbine max building temp |
| 383.15 | 110 | CO2 output (Coal Gen, Nat Gas Gen, Petrol Gen) |
| 398.15 | 125 | Aquatuner overheat, Space Heater overheat, Steam Turbine min active temp |
| 473.15 | 200 | Steam Turbine ideal source temp |
| 1273.15 | 1000 | Steam Turbine overheat temp |
