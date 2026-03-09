# Plumbing

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## DevPumpLiquid (Dev Pump Liquid)

Debug-only building; not available in normal gameplay.

- **Size**: 2x2
- **HP**: 100
- **Build time**: 60 s
- **Materials**: 400 kg Any Metal
- **Decor**: -10, radius 2
- **Max temp**: 9999 K (effectively indestructible; Invincible flag set)
- **Power**: None
- **Output**: Liquid conduit at offset (1,1)
- **Storage**: 20 kg (insulated)
- **Mechanic**: DevPump component pumps any liquid from the world. Always operational, corrosion-proof, not floodable, not entombable, not overheatable.
- **Research**: None (debug building)

---

## Outhouse

- **Name**: Outhouse
- **Description**: "The colony that eats together, excretes together."
- **Effect**: "Gives Duplicants a place to relieve themselves. Requires no Piping. Must be periodically emptied of Polluted Dirt."
- **Size**: 2x3
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 200 kg Raw Minerals or Wood
- **Decor**: -20, radius 5 (PENALTY.TIER4)
- **Max temp**: 800 K
- **Power**: None
- **Exhaust heat**: 0.25 kDTU/s
- **Pipe connections**: None
- **Uses per fill**: 15
- **Dirt per fill**: 200 kg (13 kg consumed per use)
- **Output per use**: Polluted Dirt (ToxicSand), mass = PEE_PER_TOILET_PEE (6.7 kg), at dupe internal body temperature (310.15 K)
- **Disease**: Food Poisoning, 100,000 germs emitted per flush, 100,000 germs added to dupe per flush
- **Clean time**: 90 s (requires Duplicant labor)
- **Clogged by gunk**: No
- **Assignable**: Toilet slot (can be public)
- **Room tag**: ToiletType
- **Research**: None (available from start)

---

## FlushToilet (Lavatory)

- **Name**: Lavatory
- **Description**: "Lavatories transmit fewer germs to Duplicants' skin and require no emptying."
- **Effect**: "Gives Duplicants a place to relieve themselves. Spreads very few Germs."
- **Size**: 2x3
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 400 kg Raw Metals
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 800 K
- **Power**: None
- **Exhaust heat**: 0.25 kDTU/s
- **Self heat**: 0 kDTU/s
- **Pipe input**: Liquid conduit at offset (0,0) -- consumes Water
- **Pipe output**: Liquid conduit at offset (1,1) -- outputs everything except Water (Polluted Water)
- **Rotation**: FlipH
- **Water consumed per use**: 5 kg
- **Mass emitted per use**: 11.7 kg (5 kg water + 6.7 kg from dupe)
- **Output temperature**: 310.15 K (dupe internal body temp)
- **Disease**: Food Poisoning, 100,000 germs per flush, 5,000 germs on dupe per flush (1/20 of base)
- **Storage**: 25 kg
- **Clean time**: 90 s
- **Clogged by gunk**: Yes (Bionic Duplicants can clog it)
- **Assignable**: Toilet slot (can be public)
- **Room tags**: ToiletType, FlushToiletType
- **Requires output pipe**: Yes (ignores full pipe)
- **Research**: SanitationSciences

---

## WallToilet (Wall Toilet)

- **Name**: Wall Toilet
- **Description**: "Wall Toilets transmit fewer germs to Duplicants and require no emptying."
- **Effect**: "Gives Duplicants a place to relieve themselves. Empties directly on the other side of the wall. Spreads very few Germs."
- **Size**: 1x3
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 100 kg Plastic
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 800 K
- **Power**: None
- **Exhaust heat**: 0.25 kDTU/s
- **Self heat**: 0 kDTU/s
- **Pipe input**: Liquid conduit at offset (0,0) -- consumes Water
- **Placement**: WallFloor (mounted on wall)
- **Rotation**: FlipH
- **Water consumed per use**: 2.5 kg
- **Mass emitted per use**: 9.2 kg (2.5 kg water + 6.7 kg from dupe)
- **Output temperature**: 310.15 K (dupe internal body temp)
- **Disease**: Food Poisoning, 100,000 germs per flush, 20,000 germs on dupe per flush (1/5 of base)
- **Storage**: 12.5 kg
- **Output**: No pipe output; uses AutoStorageDropper to drop non-Water liquids at offset (-2,0) on the other side of the wall. Blocked by substantial liquid at drop location.
- **Requires output pipe**: No (requireOutput = false)
- **Clean time**: 90 s
- **Clogged by gunk**: Yes
- **Assignable**: Toilet slot (can be public)
- **Room tags**: ToiletType, FlushToiletType
- **Research**: LiquidDistribution
- **DLC**: Spaced Out! (EXPANSION1)

---

## Shower

- **Name**: Shower
- **Description**: "Regularly showering will prevent Duplicants spreading germs to the things they touch."
- **Effect**: "Improves Duplicant Morale and removes surface Germs."
- **Size**: 2x4
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 400 kg Raw Metals
- **Decor**: +10, radius 2 (BONUS.TIER1)
- **Max temp**: 1600 K
- **Power**: None
- **Exhaust heat**: 0.25 kDTU/s
- **Pipe input**: Liquid conduit at offset (0,0) -- consumes Water (5 kg capacity)
- **Pipe output**: Liquid conduit at offset (1,1) -- outputs everything except Water
- **Element conversion**: 1 kg/s Water -> 1 kg/s Dirty Water (while in use)
- **Work time**: 15 s
- **Disease removal**: 95% fractional removal plus 2,000 absolute germ removal
- **Storage**: 10 kg
- **Requires output pipe**: Yes (ignores full pipe)
- **Room tags**: WashStation, AdvancedWashStation
- **Research**: SanitationSciences

---

## GunkEmptier (Gunk Extractor)

- **Name**: Gunk Extractor
- **Description**: "Bionic Duplicants are much more relaxed after a visit to the gunk extractor."
- **Effect**: "Cleanses stale Gunk build-up from Duplicants' bionic parts."
- **Size**: 3x3
- **HP**: 30
- **Build time**: 60 s
- **Materials**: 400 kg Raw Metals
- **Decor**: -15, radius 3 (PENALTY.TIER2)
- **Max temp**: 800 K
- **Power**: None
- **Exhaust heat**: 0.125 kDTU/s
- **Self heat**: 0 kDTU/s
- **Pipe output**: Liquid conduit at offset (-1,0) -- outputs Liquid Gunk only
- **Storage**: 120 kg (GunkMonitor.GUNK_CAPACITY 80 kg x 1.5)
- **Rotation**: Unrotatable
- **Assignable**: Toilet slot (can be public)
- **Room tags**: ToiletType, FlushToiletType
- **Tags**: BionicBuilding
- **Research**: SanitationSciences
- **DLC**: DLC3 (Bionic Booster Pack)

---

## LiquidPumpingStation (Pitcher Pump)

- **Name**: Pitcher Pump
- **Description**: "Pitcher pumps allow Duplicants to bottle and deliver liquids from place to place."
- **Effect**: "Manually pumps Liquid into bottles for transport. Duplicants can only carry liquids that are bottled."
- **Size**: 2x4
- **HP**: 100
- **Build time**: 10 s
- **Materials**: 400 kg Raw Minerals
- **Decor**: 0 (NONE)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe connections**: None (manual operation)
- **Manually operated**: Yes
- **Tail length**: 4 cells (visual guide for liquid detection range below the pump)
- **Placement**: Anywhere
- **Not floodable**: Yes
- **Tags**: CorrosionProof, LiquidSource
- **Storage**: Insulated, allows item removal
- **Research**: None (available from start)

---

## BottleEmptier (Bottle Emptier)

- **Name**: Bottle Emptier
- **Description**: "A bottle emptier's Element Filter can be used to designate areas for specific liquid storage."
- **Effect**: "Empties bottled Liquids back into the world."
- **Size**: 1x3
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 400 kg Raw Minerals
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe connections**: None
- **Storage**: 200 kg (filters: liquids only)
- **Rotation**: FlipH
- **Not overheatable**: Yes
- **Filterable**: Yes (TreeFilterable, lets player choose which liquids)
- **Mechanic**: Dumps bottled liquids into the environment
- **Research**: None (available from start)

---

## BottleEmptierConduitLiquid (Bottle Drainer)

- **Name**: Bottle Drainer
- **Description**: "A bottle drainer's Element Filter can be used to designate areas for specific liquid storage."
- **Effect**: "Drains bottled Liquids into Liquid Pipes."
- **Size**: 1x2
- **HP**: 30
- **Build time**: 60 s
- **Materials**: 100 kg Refined Metal
- **Decor**: -15, radius 3 (PENALTY.TIER2)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe output**: Liquid conduit at offset (0,0) -- outputs any liquid
- **Rotation**: FlipH
- **Storage**: 200 kg (filters: liquids only)
- **Empty rate**: 5 kg/s (internal emptying into storage for conduit output)
- **Requires output pipe**: Yes (ignores full pipe)
- **Filterable**: Yes (TreeFilterable)
- **Tags**: IndustrialMachinery
- **Research**: LiquidDistribution

---

## LiquidBottler (Bottle Filler)

- **Name**: Bottle Filler
- **Description**: "Bottle fillers allow Duplicants to manually deliver liquids from place to place."
- **Effect**: "Automatically stores piped Liquids into bottles for manual transport."
- **Size**: 3x2
- **HP**: 100
- **Build time**: 120 s
- **Materials**: 400 kg Any Metal
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 800 K
- **Power**: None
- **Pipe input**: Liquid conduit at offset (0,0) -- consumes any liquid
- **Storage**: 200 kg (sealed, hidden; filters: liquids only)
- **Conduit consumer**: Capacity 200 kg, always consumes, ignores min mass check
- **Bottler work time**: 9 s (to drop bottles for pickup)
- **User max capacity**: 200 kg
- **Tags**: LiquidSource
- **Research**: FlowRedirection

---

## LiquidConduit (Liquid Pipe)

- **Name**: Liquid Pipe
- **Description**: "Liquid pipes are used to connect the inputs and outputs of plumbed buildings."
- **Effect**: "Carries Liquid between Outputs and Intakes. Can be run through wall and floor tile."
- **Size**: 1x1
- **HP**: 10
- **Build time**: 3 s
- **Materials**: 100 kg Plumbable or Metal
- **Decor**: 0 (NONE)
- **Max temp**: 1600 K
- **Power**: None
- **Max flow**: 10 kg/s (standard liquid pipe capacity per packet)
- **Thermal conductivity**: Default (standard heat exchange with environment)
- **Not floodable, not overheatable, not entombable**: Yes
- **Drag build**: Yes (can draw continuous pipe runs)
- **Replaceable**: Yes (can be replaced by other pipe types in-place)
- **Base repair time**: -1 (no repair)
- **Research**: LiquidPiping

---

## InsulatedLiquidConduit (Insulated Liquid Pipe)

- **Name**: Insulated Liquid Pipe
- **Description**: "Pipe insulation prevents liquid contents from significantly changing temperature in transit."
- **Effect**: "Carries Liquid with minimal change in Temperature. Can be run through wall and floor tile."
- **Size**: 1x1
- **HP**: 10
- **Build time**: 10 s
- **Materials**: 400 kg Plumbable
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: None
- **Thermal conductivity**: 0.03125 (1/32 of default -- greatly reduces heat exchange)
- **Not floodable, not overheatable, not entombable**: Yes
- **Drag build**: Yes
- **Replaceable**: Yes
- **Base repair time**: -1 (no repair)
- **Research**: ImprovedLiquidPiping

---

## LiquidConduitRadiant (Radiant Liquid Pipe)

- **Name**: Radiant Liquid Pipe
- **Description**: "Radiant pipes pumping cold liquid can be run through hot areas to help cool them down."
- **Effect**: "Carries Liquid, allowing extreme Temperature exchange with the surrounding environment. Can be run through wall and floor tile."
- **Size**: 1x1
- **HP**: 10
- **Build time**: 10 s
- **Materials**: 50 kg Refined Metal (TIER1)
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 3200 K
- **Power**: None
- **Thermal conductivity**: 2.0 (greatly increased heat exchange with surroundings)
- **Not floodable, not overheatable, not entombable**: Yes
- **Drag build**: Yes
- **Replaceable**: Yes
- **Base repair time**: -1 (no repair)
- **Research**: LiquidTemperature

---

## LiquidConduitBridge (Liquid Bridge)

- **Name**: Liquid Bridge
- **Description**: "Separate pipe systems help prevent building damage caused by mingled pipe contents."
- **Effect**: "Runs one Liquid Pipe section over another without joining them. Can be run through wall and floor tile."
- **Size**: 3x1
- **HP**: 10
- **Build time**: 3 s
- **Materials**: 100 kg Raw Minerals
- **Decor**: 0 (NONE)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe input**: Liquid conduit at offset (-1,0)
- **Pipe output**: Liquid conduit at offset (1,0)
- **Rotation**: R360 (full rotation)
- **Not floodable, not overheatable, not entombable**: Yes
- **Base repair time**: -1 (no repair)
- **Mechanic**: ConduitBridge, allows pipes to cross over each other
- **Research**: LiquidPiping

---

## LiquidConduitPreferentialFlow (Priority Liquid Flow)

**Deprecated building** (Deprecated = true; may not appear in build menu in current versions).

- **Name**: Priority Liquid Flow
- **Description**: "Priority flows ensure important buildings are filled first when on a system with other buildings."
- **Effect**: "Diverts Liquid to a secondary input when its primary input overflows."
- **Size**: 2x2
- **HP**: 10
- **Build time**: 3 s
- **Materials**: 50 kg Raw Minerals (TIER1)
- **Decor**: 0 (NONE)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe input**: Liquid conduit at offset (0,0)
- **Pipe output**: Liquid conduit at offset (1,0)
- **Secondary input**: Liquid conduit at offset (0,1) (ConduitSecondaryInput)
- **Rotation**: R360
- **Not floodable, not entombable**: Yes
- **Base repair time**: -1 (no repair)
- **Mechanic**: ConduitPreferentialFlow -- flow goes to primary output first, secondary input is the overflow source
- **Research**: ImprovedLiquidPiping

---

## LiquidConduitOverflow (Liquid Overflow Valve)

**Deprecated building** (Deprecated = true; may not appear in build menu in current versions).

- **Name**: Liquid Overflow Valve
- **Description**: "Overflow valves can be used to prioritize which buildings should receive precious resources first."
- **Effect**: "Fills a secondary Liquid output only when its primary output is blocked."
- **Size**: 2x2
- **HP**: 10
- **Build time**: 3 s
- **Materials**: 50 kg Raw Minerals (TIER1)
- **Decor**: 0 (NONE)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe input**: Liquid conduit at offset (0,0)
- **Pipe output**: Liquid conduit at offset (1,0) (primary)
- **Secondary output**: Liquid conduit at offset (1,1) (ConduitSecondaryOutput)
- **Rotation**: R360
- **Not floodable, not entombable**: Yes
- **Base repair time**: -1 (no repair)
- **Mechanic**: ConduitOverflow -- flow goes to primary output first, overflows to secondary
- **Research**: ImprovedLiquidPiping

---

## LiquidPump (Liquid Pump)

- **Name**: Liquid Pump
- **Description**: "Piping a pump's output to a building's intake will send liquid to that building."
- **Effect**: "Draws in Liquid and runs it through Pipes. Must be submerged in Liquid."
- **Size**: 2x2
- **HP**: 100
- **Build time**: 60 s
- **Materials**: 400 kg Any Metal
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 1600 K
- **Power**: 240 W
- **Self heat**: 2 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Pipe output**: Liquid conduit at offset (1,1)
- **Power input offset**: (0,1)
- **Pump rate**: 10 kg/s (consumption rate from world)
- **Consumption radius**: 2 cells
- **Storage**: 20 kg
- **Logic input**: Operational control at offset (0,1)
- **Tags**: IndustrialMachinery, CorrosionProof
- **Research**: LiquidPiping

---

## LiquidMiniPump (Mini Liquid Pump)

- **Name**: Mini Liquid Pump
- **Description**: "Mini pumps are useful for moving small quantities of liquid with minimum power."
- **Effect**: "Draws in a small amount of Liquid and runs it through Pipes. Must be submerged in Liquid."
- **Size**: 1x2
- **HP**: 100
- **Build time**: 60 s
- **Materials**: 100 kg Plastic
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 1600 K
- **Power**: 60 W
- **Self heat**: 0.5 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Pipe output**: Liquid conduit at offset (0,1)
- **Power input offset**: (0,0)
- **Rotation**: R360 (full rotation)
- **Pump rate**: 1 kg/s (consumption rate from world)
- **Consumption radius**: 2 cells
- **Storage**: 2 kg
- **Logic input**: Operational control at offset (0,1)
- **Tags**: IndustrialMachinery, CorrosionProof
- **Research**: ValveMiniaturization

---

## LiquidVent (Liquid Vent)

- **Name**: Liquid Vent
- **Description**: "Vents are an exit point for liquids from plumbing systems."
- **Effect**: "Releases Liquid from Liquid Pipes."
- **Size**: 1x1
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 400 kg Any Metal
- **Decor**: -10, radius 2 (PENALTY.TIER1)
- **Max temp**: 1600 K
- **Power**: None
- **Not overheatable**: Yes
- **Pipe input**: Liquid conduit at offset (0,0)
- **Overpressure mass**: 1000 kg (will not output if liquid mass in cell exceeds this)
- **Logic input**: Operational control at offset (0,0)
- **Mechanic**: Vent (Sink endpoint), uses Exhaust and SimpleVent components
- **Conduit consumer**: Ignores min mass check
- **Research**: LiquidPiping

---

## LiquidFilter (Liquid Filter)

- **Name**: Liquid Filter
- **Description**: "All liquids are sent into the building's output pipe, except the liquid chosen for filtering."
- **Effect**: "Sieves one Liquid out of a mix, sending it into a dedicated Filtered Output Pipe. Can only filter one liquid type at a time."
- **Size**: 3x1
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 200 kg Raw Metals
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: 120 W
- **Self heat**: 4 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Pipe input**: Liquid conduit at offset (-1,0)
- **Pipe output**: Liquid conduit at offset (1,0) (unfiltered output -- everything except the selected element)
- **Filtered output**: Liquid conduit at offset (0,0) (ConduitSecondaryOutput -- the selected element)
- **Rotation**: R360
- **Tags**: IndustrialMachinery
- **Mechanic**: ElementFilter with Filterable (ElementState.Liquid). Player selects one liquid element; that element goes to the filtered output port, everything else goes to the main output.
- **Research**: AdvancedFiltration

---

## LiquidValve (Liquid Valve)

- **Name**: Liquid Valve
- **Description**: "Valves control the amount of liquid that moves through pipes, preventing waste."
- **Effect**: "Controls the Liquid volume permitted through Pipes."
- **Size**: 1x2
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 200 kg Raw Metals
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: None
- **Pipe input**: Liquid conduit at offset (0,0)
- **Pipe output**: Liquid conduit at offset (0,1)
- **Rotation**: R360
- **Max flow**: 10 kg/s (adjustable from 0 to 10 kg/s)
- **Animation ranges**: 0-3 kg/s "lo", 3-7 kg/s "med", 7-10 kg/s "hi"
- **Manually operated**: Yes (work time 5 s to adjust)
- **Mechanic**: ValveBase + Valve. Player sets desired flow rate; Duplicant must physically interact to change it.
- **Research**: PressureManagement

---

## LiquidLogicValve (Liquid Shutoff)

- **Name**: Liquid Shutoff
- **Description**: "Automated piping saves power and time by removing the need for Duplicant input."
- **Effect**: "Connects to an Automation grid to automatically turn Liquid flow on or off."
- **Size**: 1x2
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 50 kg Refined Metal (TIER1)
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: 10 W
- **Pipe input**: Liquid conduit at offset (0,0)
- **Pipe output**: Liquid conduit at offset (0,1)
- **Power input offset**: (0,1)
- **Rotation**: R360
- **Max flow**: 10 kg/s (full flow when open, zero when closed)
- **Logic input**: At offset (0,0). Green = allow flow, Red = prevent flow. Default (unnetworked) = 0 (closed).
- **Mechanic**: OperationalValve controlled by LogicOperationalController. No enable/disable button (BuildingEnabledButton destroyed). Requires power but not conduit input.
- **Research**: ImprovedLiquidPiping

---

## LiquidLimitValve (Liquid Meter Valve)

- **Name**: Liquid Meter Valve
- **Description**: "Meter Valves let an exact amount of liquid pass through before shutting off."
- **Effect**: "Connects to an Automation grid to automatically turn Liquid flow off when the specified amount has passed through it."
- **Size**: 1x2
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 25 kg Refined Metal + 50 kg Plastic
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: 10 W
- **Pipe input**: Liquid conduit at offset (0,0)
- **Pipe output**: Liquid conduit at offset (0,1)
- **Power input offset**: (0,1)
- **Rotation**: R360
- **Max limit**: 500 kg (adjustable; default 0 kg)
- **Logic input (Reset)**: At offset (0,1). Green signal resets the counted amount to zero.
- **Logic output**: At offset (0,0). Sends Green when the limit has been reached (all specified mass has passed through), Red otherwise.
- **Mechanic**: LimitValve + ConduitBridge. Passes liquid through until the cumulative mass reaches the configured limit, then stops. Reset via automation signal. No enable/disable button. Requires power but not conduit input.
- **Research**: LiquidTemperature

---

## LiquidConduitElementSensor (Liquid Pipe Element Sensor)

- **Name**: Liquid Pipe Element Sensor
- **Description**: "Element sensors can be used to detect the presence of a specific liquid in a pipe."
- **Effect**: "Sends a Green Signal when the selected Liquid is detected within a pipe."
- **Size**: 1x1
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 25 kg Refined Metal (TIER0)
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: None
- **Not overheatable, not floodable, not entombable**: Yes
- **Always operational**: Yes
- **Logic output**: At offset (0,0). Green when selected liquid is detected in pipe, Red otherwise.
- **Mechanic**: ConduitElementSensor on liquid conduit. Filterable (ElementState.Liquid) -- player selects which element to detect. Default state: off.
- **Research**: LiquidTemperature

---

## LiquidConduitDiseaseSensor (Liquid Pipe Germ Sensor)

- **Name**: Liquid Pipe Germ Sensor
- **Description**: "Germ sensors can help control automation behavior in the presence of germs."
- **Effect**: "Sends a Green Signal or Red Signal based on the internal Germ count of the pipe."
- **Size**: 1x1
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 25 kg Refined Metal + 50 kg Plastic
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: None
- **Not overheatable, not floodable, not entombable**: Yes
- **Always operational**: Yes
- **Logic output**: At offset (0,0). Threshold-based (default threshold: 0 germs, activate above threshold). Green when germ count is within configured range.
- **Mechanic**: ConduitDiseaseSensor on liquid conduit. Default state: off.
- **Research**: MedicineIII

---

## LiquidConduitTemperatureSensor (Liquid Pipe Thermo Sensor)

- **Name**: Liquid Pipe Thermo Sensor
- **Description**: "Thermo sensors disable buildings when their pipe contents reach a certain temperature."
- **Effect**: "Sends a Green Signal or Red Signal when pipe contents enter the chosen Temperature range."
- **Size**: 1x1
- **HP**: 30
- **Build time**: 30 s
- **Materials**: 25 kg Refined Metal (TIER0)
- **Decor**: -5, radius 1 (PENALTY.TIER0)
- **Max temp**: 1600 K
- **Power**: None
- **Not overheatable, not floodable, not entombable**: Yes
- **Always operational**: Yes
- **Logic output**: At offset (0,0). Threshold-based (default threshold: 280 K / 6.85 C, activate above threshold).
- **Range**: 0 K to 9999 K
- **Mechanic**: ConduitTemperatureSensor on liquid conduit. Default state: off.
- **Research**: LiquidTemperature

---

## ModularLaunchpadPortLiquid (Liquid Rocket Port Loader)

- **Name**: Liquid Rocket Port Loader
- **Description**: "Rockets must be landed to load or unload resources."
- **Effect**: "Loads Liquids to the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the liquid filters set on the rocket's cargo bays."
- **Size**: 2x2 (overridden from default 2x3)
- **HP**: 1000
- **Build time**: 60 s
- **Materials**: 400 kg Refined Metal
- **Decor**: 0 (NONE)
- **Max temp**: 9999 K
- **Overheat temp**: 2273.15 K (2000 C)
- **Power**: 240 W
- **Self heat**: 0 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Pipe input**: Liquid conduit at offset (0,0)
- **Power input offset**: (1,0)
- **Storage**: 10 kg (insulated, sealed, hidden)
- **Conduit consumer**: Capacity 10 kg, accepts any liquid
- **Mode**: Loader (fills rocket)
- **Logic input**: Operational control
- **Tags**: IndustrialMachinery, ModularConduitPort, NotRocketInteriorBuilding
- **Chained building**: Links to LaunchPad and other ModularLaunchpadPorts
- **Research**: FlowRedirection
- **DLC**: Spaced Out! (EXPANSION1)

---

## ModularLaunchpadPortLiquidUnloader (Liquid Rocket Port Unloader)

- **Name**: Liquid Rocket Port Unloader
- **Description**: "Rockets must be landed to load or unload resources."
- **Effect**: "Unloads Liquids from the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the liquid filters set on this unloader."
- **Size**: 2x3
- **HP**: 1000
- **Build time**: 60 s
- **Materials**: 400 kg Refined Metal
- **Decor**: 0 (NONE)
- **Max temp**: 9999 K
- **Overheat temp**: 2273.15 K (2000 C)
- **Power**: 240 W
- **Self heat**: 0 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Pipe output**: Liquid conduit at offset (1,2)
- **Power input offset**: (1,0)
- **Storage**: 10 kg (insulated, sealed, hidden; filters: liquids)
- **Conduit dispenser**: Outputs any liquid, always dispense
- **Filterable**: Yes (TreeFilterable)
- **Mode**: Unloader (empties rocket)
- **Logic input**: Operational control
- **Tags**: IndustrialMachinery, ModularConduitPort, NotRocketInteriorBuilding
- **Chained building**: Links to LaunchPad and other ModularLaunchpadPorts
- **Research**: FlowRedirection
- **DLC**: Spaced Out! (EXPANSION1)

---

## ContactConductivePipeBridge (Conduction Panel)

- **Name**: Conduction Panel
- **Description**: "It can transfer heat effectively even if no liquid is passing through."
- **Effect**: "Carries Liquid, allowing extreme Temperature exchange with overlapping buildings. Can function in a vacuum. Can be run through wall and floor tiles."
- **Size**: 3x1
- **HP**: 30
- **Build time**: 10 s
- **Materials**: 100 kg Refined Metal
- **Decor**: 0 (NONE)
- **Max temp**: 2400 K
- **Power**: None
- **Thermal conductivity**: 2.0 (high heat exchange)
- **Pipe input**: Liquid conduit at offset (-1,0)
- **Pipe output**: Liquid conduit at offset (1,0)
- **Rotation**: R360
- **Not floodable, not overheatable, not entombable**: Yes
- **Base repair time**: -1 (no repair)
- **Pump rate**: 10 kg/s (ContactConductivePipeBridge.Def.pumpKGRate)
- **Uses structure temperature**: Yes (exchanges heat with overlapping buildings at BUILDING_TO_BUILDING_TEMPERATURE_SCALE = 0.001)
- **Temperature exchange with storage modifier**: 50 (TEMPERATURE_EXCHANGE_WITH_STORAGE_MODIFIER)
- **No-liquids cooldown**: 1.5 s (NO_LIQUIDS_COOLDOWN, delay before heat exchange resumes after pipe empties)
- **Mechanic**: Acts as a pipe bridge (like LiquidConduitBridge) but also exchanges heat between pipe contents and overlapping buildings via StructureToStructureTemperature. Works even in vacuum.
- **Research**: LiquidTemperature
