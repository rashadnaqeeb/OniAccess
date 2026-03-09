# Automation

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## Wires

### Automation Wire
- **ID**: LogicWire
- **Name**: Automation Wire
- **Description**: Automation wire is used to connect building ports to automation gates.
- **Effect**: Connects buildings to Sensors. Can be run through wall and floor tile.
- **Size**: 1x1
- **HP**: 10
- **Construction**: 5 kg Refined Metal (TIER_TINY)
- **Build time**: 3s
- **Decor**: -5 (radius 1)
- **Bit depth**: 1-bit
- **Not floodable, not overheatable, not entombable**
- **Drag-buildable, no foundation required**
- **Tech**: Smart Home (LogicControl)

### Automation Ribbon
- **ID**: LogicRibbon
- **Name**: Automation Ribbon
- **Description**: Logic ribbons use significantly less space to carry multiple automation signals.
- **Effect**: A 4-Bit Automation Wire which can carry up to four automation signals. Use a Ribbon Writer to output to multiple Bits, and a Ribbon Reader to input from multiple Bits.
- **Size**: 1x1
- **HP**: 10
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 10s
- **Decor**: -5 (radius 1)
- **Bit depth**: 4-bit
- **Not floodable, not overheatable, not entombable**
- **Drag-buildable, no foundation required**
- **Tech**: Parallel Automation (ParallelAutomation)

### Automation Wire Bridge
- **ID**: LogicWireBridge
- **Name**: Automation Wire Bridge
- **Description**: Wire bridges allow multiple automation grids to exist in a small area without connecting.
- **Effect**: Runs one Automation Wire section over another without joining them. Can be run through wall and floor tile.
- **Size**: 3x1
- **HP**: 30
- **Construction**: 5 kg Refined Metal (TIER_TINY)
- **Build time**: 3s
- **Decor**: -5 (radius 1)
- **Bit depth**: 1-bit
- **Rotatable**: 360 degrees
- **Ports**: Two input ports at offsets (-1,0) and (1,0), bridged together
- **Not floodable, not overheatable, not entombable**
- **Tech**: Smart Home (LogicControl)

### Automation Ribbon Bridge
- **ID**: LogicRibbonBridge
- **Name**: Automation Ribbon Bridge
- **Description**: Wire bridges allow multiple automation grids to exist in a small area without connecting.
- **Effect**: Runs one Automation Ribbon section over another without joining them. Can be run through wall and floor tile.
- **Size**: 3x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 3s
- **Decor**: -5 (radius 1)
- **Bit depth**: 4-bit (ribbon)
- **Rotatable**: 360 degrees
- **Ports**: Two ribbon input ports at offsets (-1,0) and (1,0), bridged together
- **Not floodable, not overheatable, not entombable**
- **Tech**: Parallel Automation (ParallelAutomation)

### Ribbon Writer
- **ID**: LogicRibbonWriter
- **Name**: Ribbon Writer
- **Description**: Translates the signal from an Automation Wire to a single Bit in an Automation Ribbon.
- **Effect**: Writes a Green or Red Signal to the specified Bit of an Automation Ribbon. Automation Ribbon must be used as the output wire to avoid overloading.
- **Size**: 2x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Rotatable**: 360 degrees
- **Input port**: 1-bit at offset (0,0) - receives signal to write
- **Output port**: 4-bit ribbon at offset (1,0) - writes selected bit
- **Tech**: Parallel Automation (ParallelAutomation)

### Ribbon Reader
- **ID**: LogicRibbonReader
- **Name**: Ribbon Reader
- **Description**: Inputs the signal from a single Bit in an Automation Ribbon into an Automation Wire.
- **Effect**: Reads a Green or Red Signal from the specified Bit of an Automation Ribbon onto an Automation Wire.
- **Size**: 2x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Rotatable**: 360 degrees, initial orientation R180
- **Input port**: 4-bit ribbon at offset (0,0) - ribbon source
- **Output port**: 1-bit at offset (1,0) - outputs selected bit reading
- **Tech**: Parallel Automation (ParallelAutomation)

---

## Switches

### Signal Switch
- **ID**: LogicSwitch
- **Name**: Signal Switch
- **Description**: Signal switches don't turn grids on and off like power switches, but add an extra signal.
- **Effect**: Sends a Green Signal or a Red Signal on an Automation grid.
- **Size**: 1x1
- **HP**: 10
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: 0 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Manually toggled by Duplicants**
- **Not floodable, not overheatable, not entombable**
- **Tech**: Smart Home (LogicControl)

### Weight Plate
- **ID**: FloorSwitch
- **Name**: Weight Plate
- **Description**: Weight plates can be used to turn on amenities only when Duplicants pass by.
- **Effect**: Sends a Green Signal when an object or Duplicant is placed atop of it. Cannot be triggered by Gas or Liquids.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 50 kg Refined Metal (TIER1)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Placement**: Tile (replaces floor, acts as foundation)
- **Movement speed modifier**: BONUS_2
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Mass (LogicMassSensor)
- **Threshold range**: 0 - 2000 kg
- **Default threshold**: 10 kg (activate above)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Generic Sensors (GenericSensors)

---

## Sensors

### Duplicant Motion Sensor
- **ID**: LogicDuplicantSensor
- **Name**: Duplicant Motion Sensor
- **Description**: Motion sensors save power by only enabling buildings when Duplicants are nearby.
- **Effect**: Sends a Green Signal or a Red Signal based on whether a Duplicant is in the sensor's range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Placement**: OnFoundationRotatable
- **Rotatable**: 360 degrees
- **Output port**: 1-bit at offset (0,0)
- **Detection range**: 4 tiles (visualized as x:-2 to x:2, y:0 to y:4)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Smart Home (LogicControl)

### Atmo Sensor
- **ID**: LogicPressureSensorGas
- **Name**: Atmo Sensor
- **Description**: Atmo sensors can be used to prevent excess oxygen production and overpressurization.
- **Effect**: Sends a Green Signal or a Red Signal when Gas pressure enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Gas pressure (LogicPressureSensor)
- **Threshold range**: 0 - 20 kg
- **Default threshold**: 1 kg (activate below)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Improved Ventilation (ImprovedGasPiping)

### Hydro Sensor
- **ID**: LogicPressureSensorLiquid
- **Name**: Hydro Sensor
- **Description**: A hydro sensor can tell a pump to refill its basin as soon as it contains too little liquid.
- **Effect**: Sends a Green Signal or a Red Signal when Liquid pressure enters the chosen range. Must be submerged in Liquid.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Liquid pressure (LogicPressureSensor)
- **Threshold range**: 0 - 2000 kg
- **Default threshold**: 500 kg (activate below)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Improved Plumbing (ImprovedLiquidPiping)

### Thermo Sensor
- **ID**: LogicTemperatureSensor
- **Name**: Thermo Sensor
- **Description**: Thermo sensors can disable buildings when they approach dangerous temperatures.
- **Effect**: Sends a Green Signal or a Red Signal when ambient Temperature enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Temperature (LogicTemperatureSensor)
- **Threshold range**: 0 - 9999 K
- **Not floodable, not overheatable, not entombable**
- **Tech**: HVAC (HVAC)

### Light Sensor
- **ID**: LogicLightSensor
- **Name**: Light Sensor
- **Description**: Light sensors can tell surface bunker doors above solar panels to open or close based on solar light levels.
- **Effect**: Sends a Green Signal or a Red Signal when ambient Brightness enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal + 25 kg Transparent (two materials)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Light (LogicLightSensor)
- **Threshold range**: 0 - 15000 lux
- **Not floodable, not overheatable, not entombable**
- **Tech**: Generic Sensors (GenericSensors)

### Wattage Sensor
- **ID**: LogicWattageSensor
- **Name**: Wattage Sensor
- **Description**: Wattage sensors can send a signal when a building has switched on or off.
- **Effect**: Sends a Green Signal or a Red Signal when Wattage consumed enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Wattage (LogicWattageSensor)
- **Threshold range**: 0 - 75000 W (1.5x max wire capacity)
- **Default**: Activate above threshold
- **Not floodable, not overheatable, not entombable**
- **Tech**: Advanced Power Regulation (AdvancedPowerRegulation)

### Cycle Sensor
- **ID**: LogicTimeOfDaySensor
- **Name**: Cycle Sensor
- **Description**: Cycle sensors ensure systems always turn on at the same time, day or night, every cycle.
- **Effect**: Sets an automatic Green Signal and Red Signal schedule within one day-night cycle.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Generic Sensors (GenericSensors)

### Timer Sensor
- **ID**: LogicTimerSensor
- **Name**: Timer Sensor
- **Description**: Timer sensors create automation schedules for very short or very long periods of time.
- **Effect**: Creates a timer to send Green Signals and Red Signals for specific amounts of time.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Generic Sensors (GenericSensors)

### Germ Sensor
- **ID**: LogicDiseaseSensor
- **Name**: Germ Sensor
- **Description**: Detecting germ populations can help block off or clean up dangerous areas.
- **Effect**: Sends a Green Signal or a Red Signal based on quantity of surrounding Germs.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal + 50 kg Plastic (two materials)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Disease (LogicDiseaseSensor)
- **Threshold range**: 0 - 100000 germs
- **Default threshold**: 0 (activate above)
- **Not floodable, not overheatable, not entombable**
- **Tech**: Pathogen Diagnostics (MedicineIII)

### Gas Element Sensor
- **ID**: LogicElementSensorGas
- **Name**: Gas Element Sensor
- **Description**: These sensors can detect the presence of a specific gas and alter systems accordingly.
- **Effect**: Sends a Green Signal when the selected Gas is detected on this sensor's tile. Sends a Red Signal when the selected Gas is not present.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Element presence, Gas state
- **Filterable**: Player selects target gas element
- **Floodable, entombable, not overheatable**
- **Tech**: Generic Sensors (GenericSensors)

### Liquid Element Sensor
- **ID**: LogicElementSensorLiquid
- **Name**: Liquid Element Sensor
- **Description**: These sensors can detect the presence of a specific liquid and alter systems accordingly.
- **Effect**: Sends a Green Signal when the selected Liquid is detected on this sensor's tile. Sends a Red Signal when the selected Liquid is not present.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Element presence, Liquid state
- **Filterable**: Player selects target liquid element
- **Not floodable, entombable, not overheatable**
- **Tech**: Generic Sensors (GenericSensors)

### Critter Sensor
- **ID**: LogicCritterCountSensor
- **Name**: Critter Sensor
- **Description**: Detecting critter populations can help adjust their automated feeding and care regimens.
- **Effect**: Sends a Green Signal or a Red Signal based on the number of eggs and critters in a room.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Critter count (room-based)
- **Threshold range**: 0 - 64 critters/eggs
- **Sidescreen options**: Count Critters toggle, Count Eggs toggle
- **Not floodable, not overheatable, not entombable**
- **Tech**: Animal Control (AnimalControl)

### Radiation Sensor
- **ID**: LogicRadiationSensor
- **Name**: Radiation Sensor
- **Description**: Radiation sensors can disable buildings when they detect dangerous levels of radiation.
- **Effect**: Sends a Green Signal or a Red Signal when ambient Radiation enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: Radiation (LogicRadiationSensor)
- **Threshold range**: 0 - 5000 rads
- **Not floodable, not overheatable, not entombable**
- **DLC**: Spaced Out! (EXPANSION1) required
- **Tech**: Micro-Targeted Medicine (MedicineIV)

### Radbolt Sensor
- **ID**: LogicHEPSensor
- **Name**: Radbolt Sensor
- **Description**: Radbolt sensors can send a signal when a Radbolt passes over them.
- **Effect**: Sends a Green Signal or a Red Signal when Radbolts detected enters the chosen range.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Output port**: 1-bit at offset (0,0)
- **Sensor type**: HEP (high-energy particle) count
- **Threshold range**: 0 - 500
- **Default**: Activate above threshold
- **Not floodable, not overheatable, not entombable**
- **DLC**: Spaced Out! (EXPANSION1) required
- **Tech**: Radiation Protection (RadiationProtection)

### Space Scanner
- **ID**: CometDetector
- **Name**: Space Scanner
- **Description**: Networks of many scanners will scan more efficiently than one on its own.
- **Effect**: Sends a Green Signal to its automation circuit when it detects incoming objects from space. Can be configured to detect incoming meteor showers or returning space rockets.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Placement**: OnFloor
- **Power**: 120 W (no logic power port)
- **Output port**: 1-bit at offset (0,0)
- **Scan radius**: 15 cells in each direction
- **Coverage required**: 50% sky visibility for full accuracy
- **Best warning time**: 200 seconds
- **Worst warning time**: 1 second
- **Logic signal delay on load**: 3 seconds
- **Floodable, entombable, not overheatable**
- **Spaced Out!**: Uses ClusterCometDetector when expansion active, otherwise CometDetector
- **Tech**: Celestial Detection (SkyDetectors)

---

## Logic Processing

### Signal Counter
- **ID**: LogicCounter
- **Name**: Signal Counter
- **Description**: For numbers higher than ten connect multiple counters together.
- **Effect**: Counts how many times a Green Signal has been received up to a chosen number. When the chosen number is reached it sends a Green Signal until it receives another Green Signal, when it resets automatically and begins counting again.
- **Size**: 1x3
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Rotatable**: FlipV (vertical flip)
- **Input ports**:
  - Count input at offset (0,0): Green increments counter, Red does nothing
  - Reset input at offset (0,1): Green resets counter, Red does nothing
- **Output port**: 1-bit at offset (0,2): Green when counter matches selected value, Red otherwise
- **Not floodable, not overheatable, not entombable**
- **Tech**: Computing (DupeTrafficControl)

### Automated Notifier
- **ID**: LogicAlarm
- **Name**: Automated Notifier
- **Description**: Sends a notification when it receives a Green Signal.
- **Effect**: Attach to sensors to send a notification when certain conditions are met. Notifications can be customized.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Input port**: 1-bit at offset (0,0): Green pushes notification, Red does nothing
- **Not floodable, not overheatable, not entombable**
- **Tech**: Notification Systems (NotificationSystems)

### Hammer
- **ID**: LogicHammer
- **Name**: Hammer
- **Description**: The hammer makes neat sounds when it strikes buildings.
- **Effect**: In its default orientation, the hammer strikes the building to the left when it receives a Green Signal. Each building has a unique sound when struck by the hammer. The hammer does no damage when it strikes.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 10s
- **Decor**: -5 (radius 1)
- **Rotatable**: 360 degrees
- **Power**: 60 W
- **Self-heat**: 0 kDTU/s
- **Exhaust heat**: 0 kDTU/s
- **Input port**: 1-bit at offset (0,0): Green triggers one strike, Red does nothing
- **Not floodable, not overheatable, not entombable**
- **Tech**: Notification Systems (NotificationSystems)

---

## Interasteroid Communication

### Automation Broadcaster
- **ID**: LogicInterasteroidSender
- **Name**: Automation Broadcaster
- **Description**: Sends automation signals into space.
- **Effect**: Sends a Green Signal or a Red Signal to an Automation Receiver over vast distances in space. Both the Automation Broadcaster and the Automation Receiver must be exposed to space to function.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 100 kg Refined Metal (TIER2)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Placement**: OnFloor, not rotatable
- **Input port**: 1-bit at offset (0,0): Signal to broadcast
- **Requires sky visibility** (SkyVisibilityVisualizer, range 0)
- **User-nameable** (default: "Unnamed Broadcaster")
- **Entombable, not floodable, not overheatable**
- **Not AlwaysOperational** (requires operational conditions)
- **DLC**: Spaced Out! (EXPANSION1) required
- **Tech**: Sensitive Microimaging (AdvancedScanners)

### Automation Receiver
- **ID**: LogicInterasteroidReceiver
- **Name**: Automation Receiver
- **Description**: Receives automation signals from space.
- **Effect**: Receives a Green Signal or a Red Signal from an Automation Broadcaster over vast distances in space. Both the Automation Receiver and the Automation Broadcaster must be exposed to space to function.
- **Size**: 1x1
- **HP**: 30
- **Construction**: 100 kg Refined Metal (TIER2)
- **Build time**: 30s
- **Decor**: -5 (radius 1)
- **Placement**: OnFloor, not rotatable
- **Output port**: 1-bit at offset (0,0): Received signal
- **Requires sky visibility** (SkyVisibilityVisualizer, range 0)
- **Entombable, not floodable, not overheatable**
- **Not AlwaysOperational** (requires operational conditions)
- **DLC**: Spaced Out! (EXPANSION1) required
- **Tech**: Sensitive Microimaging (AdvancedScanners)

---

## Duplicant Control

### Duplicant Checkpoint
- **ID**: Checkpoint
- **Name**: Duplicant Checkpoint
- **Description**: Checkpoints can be connected to automated sensors to determine when it's safe to enter.
- **Effect**: Allows Duplicants to pass when receiving a Green Signal. Prevents Duplicants from passing when receiving a Red Signal.
- **Size**: 1x3
- **HP**: 30
- **Construction**: 100 kg Refined Metal (TIER2)
- **Build time**: 30s
- **Decor**: +10 (radius 2) (BONUS.TIER1)
- **Placement**: OnFloor
- **Rotatable**: FlipH (horizontal flip for direction)
- **Power**: 10 W (input at offset (0,2))
- **Self-heat**: 0.5 kDTU/s
- **Input port**: 1-bit at offset (0,2): Green allows passage, Red blocks
- **Prevents idle traversal past building**
- **Not floodable** (overheatable and entombable by default)
- **Tech**: Computing (DupeTrafficControl)

---

## Logic Gates

All logic gates share base properties from LogicGateBaseConfig:
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 3s
- **HP**: 10
- **Decor**: -5 (radius 1)
- **Thermal conductivity**: 0.05
- **Rotatable**: 360 degrees
- **Drag-buildable**
- **Not floodable, not overheatable, not entombable**
- **No foundation required**

### NOT Gate
- **ID**: LogicGateNOT
- **Name**: NOT Gate
- **Description**: This gate reverses automation signals, turning a Green Signal into a Red Signal and vice versa.
- **Effect**: Outputs Green if Input is Red. Outputs Red if Input is Green.
- **Size**: 2x1
- **Input**: 1 port at offset (0,0)
- **Output**: 1 port at offset (1,0)
- **Logic**: Inverts input signal
- **Tech**: Generic Sensors (GenericSensors)

### AND Gate
- **ID**: LogicGateAND
- **Name**: AND Gate
- **Description**: This gate outputs a Green Signal when both its inputs are receiving Green Signals at the same time.
- **Effect**: Outputs Green when both Input A AND Input B are Green. Outputs Red when any Input is Red.
- **Size**: 2x2
- **Inputs**: 2 ports at offsets (0,0) and (0,1)
- **Output**: 1 port at offset (1,0)
- **Logic**: Both inputs must be Green for Green output
- **Tech**: Advanced Automation (LogicCircuits)

### OR Gate
- **ID**: LogicGateOR
- **Name**: OR Gate
- **Description**: This gate outputs a Green Signal if receiving one or more Green Signals.
- **Effect**: Outputs Green if at least one of Input A OR Input B is Green. Outputs Red when neither Input is Green.
- **Size**: 2x2
- **Inputs**: 2 ports at offsets (0,0) and (0,1)
- **Output**: 1 port at offset (1,0)
- **Logic**: At least one input must be Green for Green output
- **Tech**: Advanced Automation (LogicCircuits)

### XOR Gate
- **ID**: LogicGateXOR
- **Name**: XOR Gate
- **Description**: This gate outputs a Green Signal if exactly one of its Inputs is receiving a Green Signal.
- **Effect**: Outputs Green if exactly one Input is Green. Outputs Red if both or neither Inputs are Green.
- **Size**: 2x2
- **Inputs**: 2 ports at offsets (0,0) and (0,1)
- **Output**: 1 port at offset (1,0)
- **Logic**: Exclusive or - exactly one Green input
- **Tech**: Computing (DupeTrafficControl)

### BUFFER Gate
- **ID**: LogicGateBUFFER
- **Name**: BUFFER Gate
- **Description**: This gate continues outputting a Green Signal for a short time after the gate stops receiving a Green Signal.
- **Effect**: Outputs Green while Input is Green. Continues sending Green for a configurable buffer time after Input receives Red.
- **Size**: 2x1
- **Input**: 1 port at offset (0,0)
- **Output**: 1 port at offset (1,0)
- **Logic**: Pass-through with configurable delay on falling edge
- **Component**: LogicGateBuffer
- **Tech**: Advanced Automation (LogicCircuits)

### FILTER Gate
- **ID**: LogicGateFILTER
- **Name**: FILTER Gate
- **Description**: This gate only lets a Green Signal through if its Input has received a Green Signal that lasted longer than the selected filter time.
- **Effect**: Only lets Green through if the Input has received Green for longer than the selected filter time. Continues outputting Red if the Green Signal did not last long enough.
- **Size**: 2x1
- **Input**: 1 port at offset (0,0)
- **Output**: 1 port at offset (1,0)
- **Logic**: Pass-through with configurable delay on rising edge
- **Component**: LogicGateFilter
- **Tech**: Advanced Automation (LogicCircuits)

### Memory Toggle
- **ID**: LogicMemory
- **Name**: Memory Toggle
- **Description**: A Memory stores a Green Signal received in the Set Port (S) until the Reset Port (R) receives a Green Signal.
- **Effect**: Contains an internal Memory. Outputs whatever signal is stored. Signals sent to Inputs only affect the Memory, not the Output directly. Green to Set Port (S) sets memory to Green. Green to Reset Port (R) resets memory to Red.
- **Size**: 2x2
- **HP**: 10
- **Construction**: 25 kg Refined Metal (TIER0)
- **Build time**: 30s
- **Decor**: 0 (radius 1)
- **Rotatable**: 360 degrees, initial orientation R90
- **Input ports**:
  - Set Port (S) at offset (0,0): Green sets memory to Green, Red does nothing
  - Reset Port (R) at offset (1,0): Green resets memory to Red, Red does nothing
- **Output port**: Memory output at offset (0,1): Outputs current memory state
- **Not floodable, not overheatable, not entombable**
- **Tech**: Computing (DupeTrafficControl)

### Signal Selector (Multiplexer)
- **ID**: LogicGateMultiplexer
- **Name**: Signal Selector
- **Description**: Signal Selectors can be used to select which automation signal is relevant to pass through to a given circuit.
- **Effect**: Select which one of four Input signals should be sent out the Output, using Control Inputs. Send Green or Red to the two Control Inputs to determine which Input is selected.
- **Size**: 3x4
- **Inputs**: 4 ports at offsets (-1,3), (-1,2), (-1,1), (-1,0)
- **Output**: 1 port at offset (1,3)
- **Control ports**: 2 ports at offsets (0,0) and (1,0) - 2-bit selection of which input to pass through
- **Logic**: 4-to-1 multiplexer. Control bits select input: RR=input0, RG=input1, GR=input2, GG=input3
- **Tech**: Multiplexing (Multiplexing)

### Signal Distributor (Demultiplexer)
- **ID**: LogicGateDemultiplexer
- **Name**: Signal Distributor
- **Description**: Signal Distributors can be used to choose which circuit should receive a given automation signal.
- **Effect**: Route a single Input signal out one of four possible Outputs, based on the selection made by the Control Inputs. Send Green or Red to the two Control Inputs to determine which Output is selected.
- **Size**: 3x4
- **Input**: 1 port at offset (-1,3)
- **Outputs**: 4 ports at offsets (1,3), (1,2), (1,1), (1,0)
- **Control ports**: 2 ports at offsets (-1,0) and (0,0) - 2-bit selection of which output receives the signal
- **Logic**: 1-to-4 demultiplexer. Control bits select output: RR=output0, RG=output1, GR=output2, GG=output3
- **Tech**: Multiplexing (Multiplexing)
