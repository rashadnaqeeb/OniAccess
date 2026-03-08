# Automation and Logic

How automation wires, logic gates, sensors, and signal propagation work. Derived from decompiled source code.

## Signal Model

Automation signals are integers, not booleans. A standard automation wire carries a 1-bit signal: `0` (Red/inactive) or `1` (Green/active). A ribbon cable carries a 4-bit signal: an integer 0-15, where each bit is independently addressable. The constant `VALID_LOGIC_SIGNAL_MASK = 15` (0xF) caps the usable range.

The uninitialized state is `-16` (`UNINITIALIZED_LOGIC_STATE`), distinct from both Red and Green. Buildings receive `0` when disconnected from a network.

## Wire Types

| Type | BitDepth Enum | Bits Carried | Overload Behavior |
|------|---------------|--------------|-------------------|
| Automation Wire | `OneBit` (0) | 1 | Overloads if network carries >1 bit of data |
| Automation Ribbon | `FourBit` (1) | 4 | Cannot overload under normal use |

`LogicWire.BitDepth` enum: `OneBit = 0`, `FourBit = 1`, `NumRatings = 2`.

Wire and ribbon bridges (`LogicWireBridgeConfig`, `LogicRibbonBridgeConfig`) allow crossing networks without connecting them, same as electrical wire bridges.

## Network Architecture

### LogicCircuitNetwork

Each connected group of wires forms a `LogicCircuitNetwork`. A network contains:
- **Senders** (`ILogicEventSender`): sensors, gate outputs, building output ports
- **Receivers** (`ILogicEventReceiver`): gate inputs, building input ports
- **Wire groups**: indexed by `BitDepth` (two groups: OneBit and FourBit)

The network output value is computed by OR-ing all sender values:

```
outputValue = 0;
foreach sender: outputValue |= sender.GetLogicValue();
```

Multiple senders on the same wire OR their signals together. There is no conflict resolution or priority system.

### LogicCircuitManager

Manages all networks. Drives the simulation tick and signal propagation.

**Tick rate**: `ClockTickInterval = 0.1f` seconds (100ms per logic tick). The manager accumulates real time and fires one tick per 100ms of accumulated time, so multiple ticks can fire in a single frame at high game speed.

**Bridge refresh**: Every `BridgeRefreshInterval = 1.0f` seconds, half the networks update their bridge lists (even IDs one cycle, odd IDs the next).

### Signal Propagation

`PropagateSignals()` runs in two phases per tick:

1. **Update phase**: For each network, call `LogicTick()` on every sender (used by Buffer/Filter gates for delay countdown), then OR all sender values to compute `outputValue`.
2. **Send phase**: For each network where `outputValue != previousValue` (or `force_send`), call `ReceiveLogicEvent(outputValue)` on every receiver.

Propagation is **single-pass per tick**. A signal change at one end of a chain of gates takes one tick per gate to reach the other end. There is no within-tick iterative settling.

When the wire network topology changes (`IsDirty`), the system forces a full rebuild and propagation with `force_send_events: true`, resetting `elapsedTime` to zero.

## Overload

When a network's `GetBitsUsed()` exceeds the capacity of any wire group in the network, the overload timer accumulates. `GetBitsUsed()` returns `4` if `outputValue > 1`, otherwise `1`.

- After **6 seconds** continuous overload: 1 damage to a random wire or bridge in the lowest-capacity group
- Decay when not overloaded: `timeOverloaded -= dt * 0.95` (slow decay, not instant reset)
- Notification displayed for at least 5 seconds after overload ends

Practical consequence: connecting a ribbon-output device to a 1-bit wire while the signal uses multiple bits causes overload damage.

## Logic Ports (LogicPorts)

Buildings connect to automation via `LogicPorts`, which defines input and output port arrays. Each port has:
- `id`: HashedString identifier (e.g., `"LogicOperational"`, `"LogicSwitch"`)
- `cellOffset`: position relative to the building's origin
- `spriteType`: `Input`, `Output`, `RibbonInput`, or `RibbonOutput`
- `requiresConnection`: if true, shows missing-wire icon when unconnected

Output ports are backed by `LogicEventSender` (implements `ILogicEventSender`). Input ports are backed by `LogicEventHandler` (implements `ILogicEventReceiver`). When a port's value changes, the building receives event `-801688580` with a `LogicValueChanged` payload containing `portID`, `newValue`, and `prevValue`.

`SendSignal(port_id, value)` sets an output port's value. `GetInputValue(port_id)` reads an input port's current value.

## Operational Control (LogicOperationalController)

The standard way automation controls buildings. Adds a single input port with ID `"LogicOperational"`. On signal change:

1. Gets the network's `OutputValue` (or `unNetworkedValue` if no wire connected)
2. Checks bit 0: `IsBitActive(0, value)`
3. Sets `Operational.Flag("LogicOperational")` accordingly

`unNetworkedValue` defaults to `1`, meaning buildings without a connected wire default to operational. When connected, Green (bit 0 active) = operational, Red = disabled.

Only bit 0 matters for operational control, even on ribbon cables.

## Logic Gates

All gates extend `LogicGateBase` -> `LogicGate`. Gate type is determined by the `Op` enum.

### Two-Input Gates

| Gate | Op Enum | Logic |
|------|---------|-------|
| AND | `Op.And` | `output = inputOne & inputTwo` |
| OR | `Op.Or` | `output = inputOne \| inputTwo` |
| XOR | `Op.Xor` | `output = inputOne ^ inputTwo` |

These operate on the full integer signal value. On ribbon cables, they perform bitwise operations across all 4 bits.

### Single-Input Gates

**NOT** (`Op.Not`): Behavior depends on connected wire type.
- On 1-bit wire (or no wire): `output = (input == 0) ? 1 : 0`
- On 4-bit wire: `output = ~input & 0xF` (bitwise NOT, masked to 4 bits)

The gate checks `Grid.Objects[inputCell, 31]` for a `LogicWire` component to determine wire type.

### Delay Gates (Op.CustomSingle)

Both inherit from `LogicGate` and use `GetCustomValue()` override. Delay is configurable via slider: 0.1 to 200.0 seconds, converted to tick count via `Mathf.RoundToInt(delayAmount / ClockTickInterval)`.

**Buffer Gate** (`LogicGateBuffer`): Holds output Green after input goes Red.
- Input goes Green: output immediately Green, timer resets
- Input goes Red: timer starts counting down ticks
- Timer expires: output goes Red
- Keeps output Green for the configured duration after input drops

**Filter Gate** (`LogicGateFilter`): Requires sustained Green input before passing.
- Input goes Red: output immediately Red, timer resets
- Input goes Green: timer starts counting down ticks
- Timer expires: output goes Green
- Requires input to stay Green for the configured duration before output activates

Both gates implement `LogicTick()` to decrement their delay counters each logic tick.

### Multiplexer (Op.Multiplexer)

4 inputs, 2 control inputs, 1 output. The two control bits select which of the 4 inputs passes to the output:

| Control2 bit0 | Control1 bit0 | Selected Input |
|---------------|---------------|----------------|
| 0 | 0 | Input 1 |
| 0 | 1 | Input 2 |
| 1 | 0 | Input 3 |
| 1 | 1 | Input 4 |

### Demultiplexer (Op.Demultiplexer)

1 input, 2 control inputs, 4 outputs. Routes the single input to one of 4 outputs based on control bits:

| Control1 bit0 | Control2 bit0 | Active Output |
|---------------|---------------|---------------|
| 0 | 0 | Output 1 |
| 0 | 1 | Output 2 |
| 1 | 0 | Output 3 |
| 1 | 1 | Output 4 |

Non-selected outputs are set to 0.

## Memory and Counter

### Memory Toggle (LogicMemory)

An SR latch with three ports:
- **Set** (`LogicMemorySet`): input. Green on bit 0 sets stored value to 1.
- **Reset** (`LogicMemoryReset`): input. Green on bit 0 sets stored value to 0. Reset takes priority over Set.
- **Read** (`LogicMemoryRead`): output. Outputs the stored value (0 or 1).

Level-triggered: on any port change event, reads both Set and Reset current values and recomputes stored value. Stored value persists through save/load.

### Signal Counter (LogicCounter)

Counts rising edges on its input:
- **Input** (`LogicCounterInput`): increments count on each Red-to-Green transition (rising edge detection via `wasIncrementing` flag)
- **Reset** (`LogicCounterReset`): resets count to 0 on Green (rising edge)
- **Output** (`LogicCounterOutput`): Green when `currentCount == maxCount`

Configuration:
- `maxCount`: target count (default 10)
- `resetCountAtMax`: automatically reset when max reached
- `advancedMode`: when enabled, output pulses Green for 2 logic ticks at each multiple of `maxCount`, then returns to Red. Count wraps modulo `maxCount`.

Normal mode: output stays Green once count reaches max, until manually reset. Count resets to 0 on the next increment after reaching max or 10 (whichever is lower).

## Sensors

All sensors extend `Switch` and implement `ISim200ms` (or `ISim33ms`, `ISimEveryTick`). They measure an environmental condition and send Green (1) or Red (0) via `LogicPorts.SendSignal()`. All share port ID `LogicSwitch.PORT_ID`.

### Threshold Sensors (IThresholdSwitch)

These sensors have a configurable threshold and an above/below toggle. They sample over a window of readings before toggling, providing noise filtering.

| Sensor | Class | Measures | Sample Window | Sim Rate | Range |
|--------|-------|----------|---------------|----------|-------|
| Thermo Sensor | `LogicTemperatureSensor` | Cell temperature (K) | 8 samples | 200ms | 0 - 9,999 K |
| Atmo Sensor (Gas) | `LogicPressureSensor` | Gas mass in cell (kg) | 8 samples | 200ms | 0 - rangeMax kg |
| Atmo Sensor (Liquid) | `LogicPressureSensor` | Liquid mass in cell (kg) | 8 samples | 200ms | 0 - rangeMax kg |
| Germ Sensor | `LogicDiseaseSensor` | Disease count in cell | 8 samples | 200ms | 0 - 100,000 |
| Light Sensor | `LogicLightSensor` | Light intensity (lux) | 4 samples | 200ms | 0 - 15,000 lux |
| Wattage Sensor | `LogicWattageSensor` | Circuit wattage (W) | none (instant) | 200ms | 0 - 75,000 W |
| Radiation Sensor | `LogicRadiationSensor` | Cell radiation (rads) | 8 samples | 200ms | 0 - 5,000 rads |
| Radbolt Sensor | `LogicHEPSensor` | HEP payload in cell | none (instant) | every tick | 0 - 500 |
| Critter Sensor | `LogicCritterCountSensor` | Critters/eggs in room | none (instant) | 200ms | 0 - 64 |
| Weight Plate | `LogicMassSensor` | Mass on cell above (kg) | none (cooldown 0.15s) | every Update | 0 - rangeMax kg |

Pressure sensors check `Grid.Element[cell].IsState(desiredState)` and only count mass if the cell contains the correct state (gas or liquid). Temperature sensor requires `Grid.Mass > 0` to sample.

### Non-Threshold Sensors

| Sensor | Class | Behavior | Sim Rate |
|--------|-------|----------|----------|
| Duplicant Sensor | `LogicDuplicantSensor` | Green if any duplicant within `pickupRange` cells (default 4) and physically accessible (`Grid.IsPhysicallyAccessible`). Checks `DupeBrain` tag. | 200ms |
| Element Sensor (Gas) | `LogicElementSensor` | Green if selected element present in cell. Uses `Filterable` for element selection. 8-sample all-true window (all 8 must match). Requires `Operational`. | 200ms |
| Element Sensor (Liquid) | `LogicElementSensor` | Same as gas variant but for liquids. | 200ms |
| Cycle Sensor | `LogicTimeOfDaySensor` | Green during configured time window. Uses `startTime` and `duration` as fractions of a cycle (0.0-1.0). Handles wrap-around. | 200ms |
| Timer Sensor | `LogicTimerSensor` | Alternates between Green (`onDuration` seconds) and Red (`offDuration` seconds). Freerunning. | 33ms |
| Cluster Location Sensor | `LogicClusterLocationSensor` | Green when rocket is at a configured map location. Spaced Out DLC only. | 200ms |

### Manual Switch (LogicSwitch)

Player-toggled via duplicant errand. Sends 1 (Green) or 0 (Red). Toggle is requested via `ToggleRequested` and executed on next `Sim33ms` tick. Uses port ID `LogicSwitch.PORT_ID`.

## Automation Effectors

### Alarm (LogicAlarm)

Input-only building. On Green rising edge:
- Creates a notification with configurable name, tooltip, and type
- Optionally pauses the game
- Optionally zooms camera to alarm location

### Hammer (LogicHammer)

Input-only building. On Green rising edge, strikes the adjacent cell (determined by rotation) and plays a sound based on what occupies that cell. Checks for buildings, wires, pipes in priority order. Requires `Operational`.

### Power Shutoff (via LogicOperationalController)

Many buildings use `LogicOperationalController` for simple on/off control. The shutoff is a passthrough device that connects or disconnects a power circuit segment based on automation input.

## Ribbon Cable System

Ribbon cables carry a 4-bit integer (0-15). Two buildings interface between standard wires and ribbons:

### Ribbon Writer (LogicRibbonWriter)

- Input: 1-bit wire (`LogicRibbonWriterInput`)
- Output: 4-bit ribbon (`LogicRibbonWriterOutput`)
- `selectedBit` (0-3): which ribbon bit to write to
- Logic: `outputValue = inputValue << selectedBit`

Writes the input signal into a specific bit position on the ribbon. The output value is the input shifted left by the selected bit index.

### Ribbon Reader (LogicRibbonReader)

- Input: 4-bit ribbon (`LogicRibbonReaderInput`)
- Output: depends on connected wire type
- `selectedBit` (0-3): which ribbon bit to read

Reading behavior depends on output wire type:
- **1-bit output wire**: extracts single bit: `output = (input & (1 << selectedBit)) > 0 ? 1 : 0`
- **4-bit output wire**: shifts ribbon value right: `output = input >> selectedBit`

## Interasteroid Communication (Spaced Out DLC)

`LogicInterasteroidSender` and `LogicInterasteroidReceiver` transmit signals between asteroids via `LogicBroadcaster` / `LogicBroadcastReceiver`. Range: 5 cells for the broadcaster. Uses channel IDs for matching senders to receivers. Requires line of sight to space.

## Timing Summary

| Interval | What Happens |
|----------|-------------|
| Every frame | `LogicCircuitManager.RenderEveryTick()` accumulates dt |
| 100ms (logic tick) | Signal propagation: senders tick, values computed, receivers notified |
| 200ms | Most sensors sample environmental data |
| 33ms | Timer sensor, manual switch poll |
| Every tick | HEP sensor, weight plate |
| 1 second | Bridge list refresh (half of networks) |

All logic tick intervals are in game time, pausing with the game.
