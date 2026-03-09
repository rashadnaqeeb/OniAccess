# Ventilation

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

Gas conduits carry a maximum of 1 kg per packet.

---

## DevPumpGas (Dev Pump Gas)

Debug-only building. Infinite gas source for testing.

- **Name:** Dev Pump Gas
- **Description:** Piping a pump's output to a building's intake will send gas to that building.
- **Effect:** Draws in Gas and runs it through Pipes. Must be immersed in Gas.
- **Size:** 2x2
- **HP:** 100
- **Build time:** 30 s
- **Materials:** 50 kg Any Metal (TIER1)
- **Melting point:** 9999 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NOISY.TIER2
- **Power:** None
- **Heat generated:** None
- **Storage:** 20 kg
- **Output:** Gas conduit at offset (1, 1)
- **Invincible:** Yes
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Tech:** None (DebugOnly = true)
- **Special:** Always operational. Tagged DevBuilding and CorrosionProof. Dispenses any gas element.

---

## GasConduit (Gas Pipe)

- **Name:** Gas Pipe
- **Description:** Gas pipes are used to connect the inputs and outputs of ventilated buildings.
- **Effect:** Carries Gas between Outputs and Intakes. Can be run through wall and floor tile.
- **Size:** 1x1
- **HP:** 10
- **Build time:** 3 s
- **Materials:** 25 kg Raw Minerals or Metals (TIER0)
- **Melting point:** 1600 K
- **Decor:** 0 (NONE)
- **Noise:** NONE
- **Power:** None
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Tech:** GasPiping
- **Special:** Drag-buildable utility. BaseTimeUntilRepair = 0 (instant repair). No foundation required. Can be replaced by other conduit types (InsulatedGasConduit, GasConduitRadiant).

---

## InsulatedGasConduit (Insulated Gas Pipe)

- **Name:** Insulated Gas Pipe
- **Description:** Pipe insulation prevents gas contents from significantly changing temperature in transit.
- **Effect:** Carries Gas with minimal change in Temperature. Can be run through wall and floor tile.
- **Size:** 1x1
- **HP:** 10
- **Build time:** 10 s
- **Materials:** 400 kg Raw Minerals (TIER4)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NONE
- **Power:** None
- **Thermal conductivity:** 0.03125 (1/32) -- greatly reduced heat exchange
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Tech:** ImprovedGasPiping
- **Special:** Drag-buildable utility. BaseTimeUntilRepair = 0.

---

## GasConduitRadiant (Radiant Gas Pipe)

- **Name:** Radiant Gas Pipe
- **Description:** Radiant pipes pumping cold gas can be run through hot areas to help cool them down.
- **Effect:** Carries Gas, allowing extreme Temperature exchange with the surrounding environment. Can be run through wall and floor tile.
- **Size:** 1x1
- **HP:** 10
- **Build time:** 10 s
- **Materials:** 25 kg Raw Metals (TIER0)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NONE
- **Power:** None
- **Thermal conductivity:** 2.0 -- greatly increased heat exchange
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Tech:** HVAC
- **Special:** Drag-buildable utility. BaseTimeUntilRepair = 0.

---

## GasConduitBridge (Gas Bridge)

- **Name:** Gas Bridge
- **Description:** Separate pipe systems prevent mingled contents from causing building damage.
- **Effect:** Runs one Gas Pipe section over another without joining them. Can be run through wall and floor tile.
- **Size:** 3x1
- **HP:** 10
- **Build time:** 3 s
- **Materials:** 50 kg Raw Minerals (TIER1)
- **Melting point:** 1600 K
- **Decor:** 0 (NONE)
- **Noise:** NONE
- **Power:** None
- **Input:** Gas conduit at offset (-1, 0)
- **Output:** Gas conduit at offset (1, 0)
- **Rotatable:** 360 degrees
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Tech:** GasPiping
- **Special:** BaseTimeUntilRepair = -1 (no repair). No foundation required. Always operational.

---

## GasConduitPreferentialFlow (Priority Gas Flow)

**Deprecated building.**

- **Name:** Priority Gas Flow
- **Description:** Priority flows ensure important buildings are filled first when on a system with other buildings.
- **Effect:** Diverts Gas to a secondary input when its primary input overflows.
- **Size:** 2x2
- **HP:** 10
- **Build time:** 3 s
- **Materials:** 50 kg Raw Minerals (TIER1)
- **Melting point:** 1600 K
- **Decor:** 0 (NONE)
- **Noise:** NONE
- **Power:** None
- **Primary input:** Gas conduit at offset (0, 0)
- **Primary output:** Gas conduit at offset (1, 0)
- **Secondary input:** Gas conduit at offset (0, 1) (ConduitSecondaryInput)
- **Rotatable:** 360 degrees
- **Floodable:** No
- **Entombable:** No
- **Tech:** Not assigned (deprecated)
- **Special:** Deprecated = true. No foundation required. Always operational. Uses ConduitPreferentialFlow component.

---

## GasConduitOverflow (Gas Overflow Valve)

**Deprecated building.**

- **Name:** Gas Overflow Valve
- **Description:** Overflow valves can be used to prioritize which buildings should receive precious resources first.
- **Effect:** Fills a secondary Gas output only when its primary output is blocked.
- **Size:** 2x2
- **HP:** 10
- **Build time:** 3 s
- **Materials:** 50 kg Raw Minerals (TIER1)
- **Melting point:** 1600 K
- **Decor:** 0 (NONE)
- **Noise:** NONE
- **Power:** None
- **Primary input:** Gas conduit at offset (0, 0)
- **Primary output:** Gas conduit at offset (1, 0)
- **Secondary output:** Gas conduit at offset (1, 1) (ConduitSecondaryOutput)
- **Rotatable:** 360 degrees
- **Floodable:** No
- **Entombable:** No
- **Tech:** Not assigned (deprecated)
- **Special:** Deprecated = true. No foundation required. Always operational. Uses ConduitOverflow component.

---

## GasPump (Gas Pump)

- **Name:** Gas Pump
- **Description:** Piping a pump's output to a building's intake will send gas to that building.
- **Effect:** Draws in Gas and runs it through Pipes. Must be immersed in Gas.
- **Size:** 2x2
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 50 kg Any Metal (TIER1)
- **Melting point:** 1600 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NOISY.TIER2
- **Power:** 240 W
- **Power input offset:** (0, 1)
- **Exhaust heat:** 0 kDTU/s
- **Self heat:** 0 kDTU/s
- **Pump rate:** 0.5 kg/s (ElementConsumer consumptionRate)
- **Consumption radius:** 2 cells
- **Storage:** 1 kg
- **Output:** Gas conduit at offset (1, 1)
- **Floodable:** Yes
- **Logic input:** Operational control at (0, 1)
- **Tech:** GasPiping
- **Special:** Pumps all gas elements. Always dispenses. Tagged IndustrialMachinery.

---

## GasMiniPump (Mini Gas Pump)

- **Name:** Mini Gas Pump
- **Description:** Mini pumps are useful for moving small quantities of gas with minimum power.
- **Effect:** Draws in a small amount of Gas and runs it through Pipes. Must be immersed in Gas.
- **Size:** 1x2
- **HP:** 30
- **Build time:** 60 s
- **Materials:** 50 kg Plastic (TIER1)
- **Melting point:** 1600 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NOISY.TIER2
- **Power:** 60 W
- **Power input offset:** (0, 0)
- **Exhaust heat:** 0 kDTU/s
- **Self heat:** 0 kDTU/s
- **Pump rate:** 0.05 kg/s (ElementConsumer consumptionRate)
- **Consumption radius:** 2 cells
- **Storage:** 0.1 kg
- **Output:** Gas conduit at offset (0, 1)
- **Rotatable:** 360 degrees
- **Floodable:** Yes
- **Logic input:** Operational control at (0, 1)
- **Tech:** ValveMiniaturization
- **Special:** Pumps all gas elements. Always dispenses. Tagged IndustrialMachinery. 1/10th the rate and 1/4 the power of GasPump.

---

## GasVent (Gas Vent)

- **Name:** Gas Vent
- **Description:** Vents are an exit point for gases from ventilation systems.
- **Effect:** Releases Gas from Gas Pipes.
- **Size:** 1x1
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 50 kg Any Metal (TIER1)
- **Melting point:** 1600 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NONE
- **Power:** None
- **Overpressure threshold:** 2 kg -- stops venting when cell gas mass exceeds this
- **Input:** Gas conduit at offset (0, 0)
- **Overheatable:** No
- **Floodable:** No
- **Logic input:** Operational control at (0, 0)
- **Tech:** GasPiping
- **Special:** Uses Exhaust, Vent (Endpoint.Sink), and SimpleVent components. ConduitConsumer ignores min mass check.

---

## GasVentHighPressure (High Pressure Gas Vent)

- **Name:** High Pressure Gas Vent
- **Description:** High pressure vents can expel gas into more highly pressurized environments.
- **Effect:** Releases Gas from Gas Pipes into high pressure locations.
- **Size:** 1x1
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 200 kg Refined Metal + 50 kg Plastic (TIER3[0] + TIER1[0])
- **Melting point:** 1600 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NONE
- **Power:** None
- **Overpressure threshold:** 20 kg -- 10x normal vent
- **Input:** Gas conduit at offset (0, 0)
- **Overheatable:** No
- **Floodable:** No
- **Logic input:** Operational control at (0, 0)
- **Tech:** ImprovedGasPiping
- **Special:** Tagged IndustrialMachinery. Uses Exhaust, Vent (Endpoint.Sink), and SimpleVent components. ConduitConsumer ignores min mass check.

---

## GasFilter (Gas Filter)

- **Name:** Gas Filter
- **Description:** All gases are sent into the building's output pipe, except the gas chosen for filtering.
- **Effect:** Sieves one Gas from the air, sending it into a dedicated Pipe.
- **Size:** 3x1
- **HP:** 30
- **Build time:** 10 s
- **Materials:** 50 kg Raw Metals (TIER1)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NOISY.TIER1
- **Power:** 120 W
- **Self heat:** 0 kDTU/s
- **Exhaust heat:** 0 kDTU/s
- **Input:** Gas conduit at offset (-1, 0)
- **Primary output:** Gas conduit at offset (1, 0) -- non-matching gases
- **Secondary output:** Gas conduit at offset (0, 0) (ConduitSecondaryOutput) -- filtered gas
- **Rotatable:** 360 degrees
- **Floodable:** No
- **Tech:** AdvancedFiltration
- **Special:** Tagged IndustrialMachinery. Uses ElementFilter and Filterable (Gas state). Player selects which gas element to filter. Filtered element goes to secondary output; everything else goes to primary output.

---

## GasValve (Gas Valve)

- **Name:** Gas Valve
- **Description:** Valves control the amount of gas that moves through pipes, preventing waste.
- **Effect:** Controls the Gas volume permitted through Pipes.
- **Size:** 1x2
- **HP:** 30
- **Build time:** 10 s
- **Materials:** 50 kg Raw Metals (TIER1)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NOISY.TIER1
- **Power:** None
- **Input:** Gas conduit at offset (0, 0)
- **Output:** Gas conduit at offset (0, 1)
- **Rotatable:** 360 degrees
- **Max flow:** 1 kg/s (full pipe throughput)
- **Floodable:** No
- **Tech:** PressureManagement
- **Special:** Manually operated (requires Duplicant to adjust). Work time 5 s. No foundation required. Always operational. Animation ranges: lo (<0.25), med (0.25-0.5), hi (0.5-0.75), full (>0.75).

---

## GasLogicValve (Gas Shutoff)

- **Name:** Gas Shutoff
- **Description:** Automated piping saves power and time by removing the need for Duplicant input.
- **Effect:** Connects to an Automation grid to automatically turn Gas flow on or off.
- **Size:** 1x2
- **HP:** 30
- **Build time:** 10 s
- **Materials:** 50 kg Refined Metal (TIER1)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NOISY.TIER1
- **Power:** 10 W
- **Power input offset:** (0, 1)
- **Input:** Gas conduit at offset (0, 0)
- **Output:** Gas conduit at offset (0, 1)
- **Rotatable:** 360 degrees
- **Max flow:** 1 kg/s
- **Floodable:** No
- **Logic input:** at (0, 0) -- Green = allow flow, Red = block flow
- **Tech:** ImprovedGasPiping
- **Special:** Uses OperationalValve. unNetworkedValue = 0 (defaults to closed when not connected to automation). No BuildingEnabledButton.

---

## GasLimitValve (Gas Meter Valve)

- **Name:** Gas Meter Valve
- **Description:** Meter Valves let an exact amount of gas pass through before shutting off.
- **Effect:** Connects to an Automation grid to automatically turn Gas flow off when the specified amount has passed through it.
- **Size:** 1x2
- **HP:** 30
- **Build time:** 10 s
- **Materials:** 25 kg Refined Metal + 50 kg Plastic (TIER0[0] + TIER1[0])
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NOISY.TIER1
- **Power:** 10 W
- **Power input offset:** (0, 1)
- **Input:** Gas conduit at offset (0, 0)
- **Output:** Gas conduit at offset (0, 1)
- **Rotatable:** 360 degrees
- **Max limit:** 500 kg (total gas allowed through before shutoff)
- **Default limit:** 0 kg
- **Floodable:** No
- **Logic input:** Reset port at (0, 1) -- Green signal resets the metered amount
- **Logic output:** Limit Reached at (0, 0) -- Green when limit reached, Red otherwise
- **Tech:** HVAC
- **Special:** Uses LimitValve and ConduitBridge. No BuildingEnabledButton. Slider ranges from LimitValveTuning.GetDefaultSlider().

---

## GasBottler (Canister Filler)

- **Name:** Canister Filler
- **Description:** Canisters allow Duplicants to manually deliver gases from place to place.
- **Effect:** Automatically stores piped Gases into canisters for manual transport.
- **Size:** 3x2
- **HP:** 100
- **Build time:** 120 s
- **Materials:** 400 kg Any Metal (TIER4)
- **Melting point:** 800 K
- **Decor:** -10 (radius 2) (PENALTY.TIER1)
- **Noise:** NOISY.TIER0
- **Power:** None
- **Storage:** 200 kg capacity
- **Default fill level:** 25 kg (userMaxCapacity)
- **Input:** Gas conduit at offset (0, 0)
- **Floodable:** No
- **Placement:** OnFloor
- **Tech:** PortableGasses
- **Special:** Uses Bottler component (workTime 9 s). ConduitConsumer always consumes, ignores min mass check, forceAlwaysSatisfied. Tagged GasSource. Storage filters GASES. Items stored are hidden. DropAllWorkable allows manual emptying.

---

## BottleEmptierGas (Canister Emptier)

- **Name:** Canister Emptier
- **Description:** A canister emptier's Element Filter can designate areas for specific gas storage.
- **Effect:** Empties Gas canisters back into the world.
- **Size:** 1x3
- **HP:** 30
- **Build time:** 60 s
- **Materials:** 100 kg Refined Metal (TIER2)
- **Melting point:** 1600 K
- **Decor:** -15 (radius 3) (PENALTY.TIER2)
- **Noise:** NOISY.TIER1
- **Power:** None
- **Storage:** 200 kg capacity (GASES filter)
- **Empty rate:** 0.25 kg/s
- **Placement:** OnFloor
- **Rotatable:** FlipH (horizontal flip)
- **Overheatable:** No
- **Floodable:** No
- **Tech:** PortableGasses
- **Special:** Uses BottleEmptier (isGasEmptier = true). TreeFilterable allows element selection. Emits gas directly into the world.

---

## BottleEmptierConduitGas (Canister Drainer)

- **Name:** Canister Drainer
- **Description:** A canister drainer's Element Filter can designate areas for specific gas storage.
- **Effect:** Drains Gas canisters into Gas Pipes.
- **Size:** 1x2
- **HP:** 30
- **Build time:** 60 s
- **Materials:** 100 kg Refined Metal (TIER2)
- **Melting point:** 1600 K
- **Decor:** -15 (radius 3) (PENALTY.TIER2)
- **Noise:** NOISY.TIER1
- **Power:** None
- **Storage:** 200 kg capacity (GASES filter)
- **Empty rate:** 0.25 kg/s
- **Output:** Gas conduit at offset (0, 0)
- **Placement:** OnFloor
- **Rotatable:** FlipH (horizontal flip)
- **Overheatable:** No
- **Floodable:** No
- **Tech:** GasDistribution
- **Special:** Uses BottleEmptier (isGasEmptier = true, emit = false). Dispenses into conduit instead of world. ConduitDispenser with no element filter. ignoreFullPipe = true. Tagged IndustrialMachinery.

---

## ModularLaunchpadPortGas (Gas Rocket Port Loader)

**Requires: Spaced Out! DLC (EXPANSION1)**

- **Name:** Gas Rocket Port Loader
- **Description:** Rockets must be landed to load or unload resources.
- **Effect:** Loads Gases to the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the gas filters set on the rocket's cargo bays.
- **Size:** 2x2
- **HP:** 1000
- **Build time:** 60 s
- **Materials:** 400 kg Refined Metal (TIER4)
- **Melting point:** 9999 K
- **Overheat temperature:** 2273.15 K (2000 C)
- **Decor:** 0 (NONE)
- **Noise:** NOISY.TIER2
- **Power:** 240 W
- **Power input offset:** (1, 0)
- **Self heat:** 0 kDTU/s
- **Exhaust heat:** 0 kDTU/s
- **Input:** Gas conduit at offset (0, 0)
- **Storage:** 1 kg (loader storage)
- **Floodable:** No
- **Placement:** OnFloor
- **Tech:** SpaceGas
- **Special:** Loader mode. ConduitConsumer pulls from pipe into rocket. Links to LaunchPad via ChainedBuilding. Tagged IndustrialMachinery, ModularConduitPort, NotRocketInteriorBuilding. UseStructureTemperature = false.

---

## ModularLaunchpadPortGasUnloader (Gas Rocket Port Unloader)

**Requires: Spaced Out! DLC (EXPANSION1)**

- **Name:** Gas Rocket Port Unloader
- **Description:** Rockets must be landed to load or unload resources.
- **Effect:** Unloads Gases from the storage of a linked rocket. Automatically links when built to the side of a Rocket Platform or another Rocket Port. Uses the gas filters set on this unloader.
- **Size:** 2x3 (default width/height for unloader)
- **HP:** 1000
- **Build time:** 60 s
- **Materials:** 400 kg Refined Metal (TIER4)
- **Melting point:** 9999 K
- **Overheat temperature:** 2273.15 K (2000 C)
- **Decor:** 0 (NONE)
- **Noise:** NOISY.TIER2
- **Power:** 240 W
- **Power input offset:** (1, 0)
- **Self heat:** 0 kDTU/s
- **Exhaust heat:** 0 kDTU/s
- **Output:** Gas conduit at offset (1, 2)
- **Storage:** 1 kg (unloader storage, with GASES filter, Hide/Seal/Insulate modifiers)
- **Floodable:** No
- **Placement:** OnFloor
- **Tech:** SpaceGas
- **Special:** Unloader mode. ConduitDispenser pushes from rocket into pipe. TreeFilterable for element selection. Links to LaunchPad via ChainedBuilding. Tagged IndustrialMachinery, ModularConduitPort, NotRocketInteriorBuilding. UseStructureTemperature = false.

---

## GasConduitElementSensor (Gas Pipe Element Sensor)

- **Name:** Gas Pipe Element Sensor
- **Description:** Element sensors can be used to detect the presence of a specific gas in a pipe.
- **Effect:** Sends a Green Signal when the selected Gas is detected within a pipe.
- **Size:** 1x1
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 25 kg Refined Metal (TIER0)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NONE
- **Power:** None
- **Logic output:** at (0, 0) -- Green if configured gas detected, Red otherwise
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Always operational:** Yes
- **Tech:** HVAC
- **Special:** Uses ConduitElementSensor on gas conduit. Filterable for gas element selection. Default state = false (Red). Placed on gas pipe overlay.

---

## GasConduitDiseaseSensor (Gas Pipe Germ Sensor)

- **Name:** Gas Pipe Germ Sensor
- **Description:** Germ sensors can help control automation behavior in the presence of germs.
- **Effect:** Sends a Green Signal or a Red Signal based on the internal Germ count of the pipe.
- **Size:** 1x1
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 25 kg Refined Metal + 50 kg Plastic (TIER0[0] + TIER1[0])
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NONE
- **Power:** None
- **Logic output:** at (0, 0) -- configurable threshold
- **Default threshold:** 0 germs (activates above threshold)
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Always operational:** Yes
- **Tech:** MedicineIII
- **Special:** Uses ConduitDiseaseSensor. Default state = false (Red). Placed on gas pipe overlay.

---

## GasConduitTemperatureSensor (Gas Pipe Thermo Sensor)

- **Name:** Gas Pipe Thermo Sensor
- **Description:** Thermo sensors disable buildings when their pipe contents reach a certain temperature.
- **Effect:** Sends a Green Signal or a Red Signal when pipe contents enter the chosen Temperature range.
- **Size:** 1x1
- **HP:** 30
- **Build time:** 30 s
- **Materials:** 25 kg Refined Metal (TIER0)
- **Melting point:** 1600 K
- **Decor:** -5 (radius 1) (PENALTY.TIER0)
- **Noise:** NONE
- **Power:** None
- **Logic output:** at (0, 0) -- configurable threshold
- **Default threshold:** 280 K (6.85 C), activates above threshold
- **Range:** 0 K to 9999 K
- **Overheatable:** No
- **Floodable:** No
- **Entombable:** No
- **Always operational:** Yes
- **Tech:** HVAC
- **Special:** Uses ConduitTemperatureSensor. Default state = false (Red). Placed on gas pipe overlay.
