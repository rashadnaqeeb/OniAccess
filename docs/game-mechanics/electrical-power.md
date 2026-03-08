# Electrical Power

How circuits, wires, batteries, and transformers work. Derived from decompiled source code.

## Circuit Architecture

The power system uses a circuit-based model where all devices on connected wires share a single circuit. Each circuit tracks:
- Active generators (producing power)
- Consumers (drawing power)
- Batteries (storing power)
- Transformers (connecting circuits at different capacities)

Circuit membership is determined by physical wire connectivity through `UtilityNetworkManager`. Circuit IDs are **not cached** and are queried fresh each tick.

## Wire Tiers

| Tier | Max Wattage |
|------|-------------|
| Wire | 1,000 W |
| Conductive Wire | 2,000 W |
| Heavy-Watt Wire | 20,000 W |
| Heavi-Watt Conductive Wire | 50,000 W |

## Wire Physical Properties

All wire types have 10 HP (`BaseWireConfig.cs` passes `hitpoints: 10` to `BuildingTemplates.CreateBuildingDef`). Wires cannot be repaired (`BaseTimeUntilRepair = -1f`; the default 600f enables repair, -1f disables it). Damaged wires must be deconstructed and rebuilt. All wires have thermal conductivity 0.05 (very low heat transfer).

| Wire | Material | Mass | Decor | Placement |
|------|----------|------|-------|-----------|
| Wire | Any Metal | 25 kg | -5 (radius 1) | Anywhere (behind tiles) |
| Conductive Wire | Refined Metal | 25 kg | None | Anywhere (behind tiles) |
| Heavy-Watt Wire | Any Metal | 100 kg | -25 (radius 6) | Not in tiles |
| Heavi-Watt Conductive Wire | Refined Metal | 100 kg | -20 (radius 4) | Not in tiles |

Heavy-Watt variants use `BuildLocationRule.NotInTiles` -- they cannot be placed inside wall or floor tiles, forcing visible routing. Heavi-Watt Conductive Wire additionally requires the Electrical Engineering skill (`requiredSkillPerk = CanPowerTinker`).

Construction time is 3 seconds for all wire types.

**Sources:** `WireConfig.cs`, `WireRefinedConfig.cs`, `WireHighWattageConfig.cs`, `WireRefinedHighWattageConfig.cs`, `BaseWireConfig.cs`, `TUNING/BUILDINGS.cs` (CONSTRUCTION_MASS_KG, DECOR), `BuildingDef.cs` (BaseTimeUntilRepair default)

## Power Distribution Algorithm

The `CircuitManager.Sim200msLast()` method runs the main distribution every 200ms:

**Phase 1: Consumer Satisfaction**
1. Consumers sorted by `WattsNeededWhenActive` ascending (lowest-power devices first)
2. For each consumer:
   - Try active generators first (sorted by joules available, ascending)
   - Then output transformers
   - Then batteries
   - Mark as Powered if remaining need < 0.01 joules
3. This ordering ensures low-power devices (sensors, doors) stay powered during brownouts

**Phase 2: Battery Charging (Five passes in two loops)**

First loop (batteries and input transformers sorted by `Capacity - JoulesAvailable` ascending, generators sorted by joules available ascending):
1. Charge input transformers from regular generators
2. Charge input transformers from output transformers
3. Charge regular batteries from regular generators
4. Charge regular batteries from output transformers

Second loop (regular batteries re-sorted by joules available ascending):
5. Charge input transformers from regular batteries

The ascending sort by (Capacity - JoulesAvailable) places batteries with the least remaining space (most full) first. The charging algorithm divides available joules equally among batteries that still need charge.

## Overload Detection

Per `ElectricalUtilityNetwork.UpdateOverloadTime`:

1. Scan wire tiers from lowest to highest capacity, find the first tier where wires exist and `watts_used > max_wattage + 0.5W`
2. The 0.5W fudge factor prevents false positives from floating-point rounding
3. If an overloaded tier is found: accumulate `timeOverloaded += dt`
4. After **6 seconds** continuous overload: deal 1 damage to a random wire or bridge in that tier
5. If no tier is overloaded: decay timer at `0.95 * dt` per frame (slow decay, not instant reset)

A sustained overload of just 0.6W over threshold takes 6 seconds to cause first damage. Bouncing in/out rapidly will not accumulate meaningful damage.

## Consumer Power Loss

When a powered consumer loses power:
1. Status changes to `Unpowered`
2. Plays overdraw sound effect
3. Sets **6-second lockout timer** (`circuitOverloadTime = 6`)
4. Consumer cannot reconnect while lockout is active
5. After lockout expires, if power is available, transitions back to Powered

This prevents rapid on/off flickering when power is marginally available.

## Transformers

Transformers are composed of two parts:
- **Input side:** A Battery on the input circuit
- **Output side:** A Generator on the output circuit

The battery charges from the input circuit's generators. The generator provides power to the output circuit's consumers. Transfer is **100% efficient** (no energy loss), though the battery has standby leakage of 3.33 J/s when not operational.

**Loop detection:** If input circuit ID equals output circuit ID, "Power Loop Detected" is flagged.

## Battery Buildings

Three battery buildings store electrical energy on a circuit. All inherit from `BaseBatteryConfig` and use the `Battery` component. They require no power input -- they charge from excess generator output on the same circuit.

| Building | Size | Capacity | Leakage | Material | Mass | Build Time | Breakable |
|----------|------|----------|---------|----------|------|------------|-----------|
| Battery | 1x2 | 10 kJ | 1.67 J/s (1,000 J/cycle) | Any Metal | 200 kg | 30 s | Yes |
| Jumbo Battery | 2x2 | 40 kJ | 3.33 J/s (2,000 J/cycle) | Any Metal | 400 kg | 60 s | No |
| Smart Battery | 2x2 | 20 kJ | 0.67 J/s (400 J/cycle) | Refined Metal | 200 kg | 60 s | No |

The basic Battery is the only one that can break (`Breakable = true`). All three have a melting point of 800 K and place on floor (`BuildLocationRule.OnFloor`).

**Sources:** `BatteryConfig.cs`, `BatteryMediumConfig.cs`, `BatterySmartConfig.cs`, `BaseBatteryConfig.cs`

## Smart Battery Automation

`BatterySmart` extends `Battery` with hysteresis-based automation output. It has two configurable thresholds (0-100%, whole numbers):

- **Activate threshold** (`activateValue`): when charge percentage drops to or below this value, sets `activated = true`
- **Deactivate threshold** (`deactivateValue`): when charge percentage rises to or above this value, sets `activated = false`
- Output signal: sends logic HIGH (1) when `activated && operational`, LOW (0) otherwise

The logic update runs every 200ms via `EnergySim200ms`. Charge percentage is rounded to the nearest integer before comparison (`Mathf.RoundToInt(PercentFull * 100f)`).

Typical use: set activate to 40% and deactivate to 90%. Generators connected via automation wire turn on when the battery drops to 40% and turn off when it reaches 90%, reducing fuel waste.

Smart Battery has `powerSortOrder = 1000` (set both in its config and in `BaseBatteryConfig.DoPostConfigureComplete`), which affects discharge priority in the circuit manager -- higher values are drained later.

**Sources:** `BatterySmart.cs`, `BatterySmartConfig.cs`

## Battery Charge and Discharge Mechanics

The `Battery` component implements both `IEnergyConsumer` (for charging) and `IEnergyProducer` (for discharging). Each 200ms tick (`EnergySim200ms`):

1. **Charge capacity reset:** `ChargeCapacity = chargeWattage * dt`. Default `chargeWattage` is `float.PositiveInfinity`, so batteries accept unlimited charge rate per tick
2. **Meter updated:** the visual fill meter is set to `PercentFull`
3. **Sound events:** battery sounds trigger at specific thresholds (see below)
4. **Previous joules snapshot:** `PreviousJoulesAvailable` is saved for next tick's sound comparisons
5. **Leakage applied:** `ConsumeEnergy(joulesLostPerSecond * dt)` reduces stored joules. This runs regardless of whether the battery is charging or discharging

Battery sounds trigger at specific thresholds:
   - Discharged sound at 0% (was above 0% previous tick)
   - Full sound at 99.9%+ (was below 99.9% previous tick)
   - Warning sound below 25% (was at or above 25% previous tick)

**Adding energy** (`AddEnergy`): joules are clamped to capacity. `ChargeCapacity` decreases by the amount added, limiting how much more can be accepted this tick.

**Consuming energy** (`ConsumeEnergy`): joules are clamped to 0 minimum. When called with `report: true` (leakage path), the lost amount is logged to `ReportManager` as `EnergyWasted`.

**Circuit connection:** batteries connect/disconnect based on tags. A battery with all its `connectedTags` (default: `GameTags.Operational`) connects to the circuit. When disconnected, it retains stored energy but does not participate in distribution.

**Sources:** `Battery.cs`

## Battery Leakage Calculation

Leakage is a fixed per-second drain applied every 200ms tick. The formula is:

```
energy_lost_per_tick = joulesLostPerSecond * dt
```

Where `dt` is 0.2 seconds. The drain applies even when the battery is full and no consumers are drawing power. Leakage rates per building:

| Building | J/s | J/cycle (600s) | % capacity/cycle |
|----------|-----|----------------|------------------|
| Battery | 1.667 | 1,000 | 10.0% |
| Jumbo Battery | 3.333 | 2,000 | 5.0% |
| Smart Battery | 0.667 | 400 | 2.0% |

Smart Battery has the lowest absolute and relative leakage, making it the most efficient storage option per joule.

**Sources:** `Battery.EnergySim200ms()`, `BatteryConfig.cs`, `BatteryMediumConfig.cs`, `BatterySmartConfig.cs`

## Electrobank System (DLC3)

The Electrobank system is an alternative portable power storage introduced in DLC3. Electrobanks are items (not buildings on circuits) that store 120 kJ of energy and are physically carried between chargers and dischargers.

### Electrobank Item

An Electrobank is a loose entity weighing 20 kg, made of Abyssalite (`SimHashes.Katairite`). It has three lifecycle states:
- **Charged** (`Electrobank`, tag `ChargedPortableBattery`): holds up to 120,000 J, rechargeable
- **Empty** (`EmptyElectrobank`, tag `EmptyPortableBattery`): depleted, can be recharged
- **Garbage** (`GarbageElectrobank`): non-rechargeable banks that broke

When an electrobank's charge reaches 0, it auto-replaces itself with an Empty variant (if rechargeable) or is destroyed. Electrobanks take water damage when sitting in liquid -- each second there is a 25% chance of taking random damage (0 to dt). At 0 health (out of 10 HP), the electrobank explodes, converting its remaining charge to heat in the current cell.

**Sources:** `Electrobank.cs`, `ElectrobankConfig.cs`, `EmptyElectrobankConfig.cs`, `GarbageElectrobankConfig.cs`

### Electrobank Charger

The Electrobank Charger is a 2x2 building that converts circuit power into charged electrobanks. It consumes **480 W** from the circuit and charges at **400 J/s** internally (updated every sim tick). Once the internal accumulator reaches 120,000 J, it replaces the empty electrobank with a charged one and resets. Built from 100 kg Refined Metal with 30 HP, 30 s build time, and 1 kDTU/s self-heat.

Duplicants deliver empty electrobanks to the charger's storage (capacity: 1 item at a time, 20 kg). The charger has a logic input for operational control.

**Sources:** `ElectrobankChargerConfig.cs`, `ElectrobankCharger.cs`

### Electrobank Dischargers

Dischargers are generators that drain electrobanks to produce circuit power. Two sizes exist:

| Building | Size | Output | Material | Mass | Build Time |
|----------|------|--------|----------|------|------------|
| Small Electrobank Discharger | 1x1 | 60 W | Any Metal | 100 kg | 10 s |
| Large Electrobank Discharger | 2x2 | 480 W | Refined Metal | 400 kg | 60 s |

Both hold one electrobank (20 kg storage capacity). The `ElectrobankDischarger` component drains up to `wattageRating * dt` joules per tick from the stored electrobank and feeds it into the circuit via `GenerateJoules`. When the electrobank empties during discharge, it is dropped and replaced with an empty variant.

The small discharger supports 360-degree rotation (`PermittedRotations.R360`), allowing wall and ceiling mounting. Both have logic input ports for operational control.

When a discharger is deconstructed, any partially-charged electrobank in storage is fully drained and dropped as empty.

**Sources:** `ElectrobankDischarger.cs`, `SmallElectrobankDischargerConfig.cs`, `LargeElectrobankDischargerConfig.cs`

## HEP Battery (Spaced Out DLC)

The Radbolt Battery stores High Energy Particles (radbolts), not electrical energy. It is a 3x3 building requiring 120 W of electrical power to operate, with a capacity of 1,000 radbolts. When unpowered, stored radbolts decay at 0.05 particles/s.

It has two logic ports:
- **Output port** (`HEP_STORAGE`): signals based on storage fill level (configurable 0-100% threshold)
- **Input port** (`HEP_FIRE`): triggers radbolt launch with minimum 1-second interval between launches

Built from 400 kg Refined Metal, 120 s build time. Not floodable or overheatable.

**Sources:** `HEPBatteryConfig.cs`

## Energy Tracking

All power transfers are logged to `ReportManager` for the colony statistics screen, tracking energy created, consumed, and wasted per cycle.

## Transformer Buildings

| Property | Small Power Transformer | Large Power Transformer |
|----------|------------------------|------------------------|
| Wattage limit | 1,000 W | 4,000 W |
| Internal battery | 1,000 J | 4,000 J |
| Size | 2 x 2 tiles | 3 x 2 tiles |
| Material | Any Metal, 200 kg | Refined Metal, 200 kg |
| Build time | 30 seconds | 30 seconds |
| Self-heating | 1 kDTU/s | 1 kDTU/s |
| Overheat temp | 75 C base + material modifier | 75 C base + material modifier |
| HP | 30 | 30 |
| Decor | -10 (radius 2) | -10 (radius 2) |
| Noise | Tier 5 (Noisy) | Tier 5 (Noisy) |
| Rotation | Horizontal flip | Horizontal flip |

Both transformers are floor-mounted and tagged as Industrial Machinery and Power Building (affects room bonuses).

### Internal Architecture

A transformer is a single building with two separate wire connections. Internally it combines a `Battery` component (input side) and a `Generator` component via the `PowerTransformer` class (output side). The input and output connections attach to different wire networks, creating two isolated circuits.

The `Battery` component charges from the input circuit like any other battery. The `PowerTransformer` class extends `Generator` and reads energy from the internal battery each tick, making it available to consumers on the output circuit. The internal battery's capacity and the generator's wattage rating are both set to the transformer's wattage limit (1,000 J / 1,000 W for small, 4,000 J / 4,000 W for large), so the battery holds exactly one second of throughput.

### Tick Behavior (EnergySim200ms)

Each 200ms tick, `PowerTransformer.EnergySim200ms` calculates available output joules as `min(battery.JoulesAvailable, WattageRating * dt)`. If the transformer is not operational (e.g., entombed or overheated), output is zero. The battery's `chargeWattage` equals the wattage rating, so input charging is also capped at the same rate.

### Circuit Isolation

The input wire connection (`PowerInputOffset`) and output wire connection (`PowerOutputOffset`) connect to separate `UtilityNetwork` circuits. This is the core purpose of transformers: a high-capacity backbone circuit on heavy-watt wire feeds the input, and the output feeds a low-capacity branch circuit on regular wire. The output circuit's total draw is limited by the transformer's wattage rating, protecting the branch wires from overload.

### Standby Leakage

When not operational, the internal battery leaks at 3.33 J/s (`joulesLostPerSecond = 3.3333333f`). When operational, leakage is 0. This is controlled by `PowerTransformer.UpdateJoulesLostPerSecond()` which listens for operational state changes.

### Loop Detection

`PowerTransformer.EnergySim200ms` compares the battery's circuit ID to the generator's circuit ID each tick. If they match (and neither is `ushort.MaxValue`, meaning disconnected), it sets a "Power Loop Detected" status item. This happens when both sides are wired to the same network, which defeats the transformer's purpose.

### Distribution Priority

Transformer batteries use `powerSortOrder = 1000`, placing them after regular batteries (default order) in the charging queue. The generator side uses `powerDistributionOrder = 9`. In `CircuitManager`, the EnergyConsumer component is destroyed in `DoPostConfigureComplete` so the transformer does not register as a consumer on the input circuit.

**Sources:** `PowerTransformerConfig.cs`, `PowerTransformerSmallConfig.cs`, `PowerTransformer.cs`, `Battery.cs`, `Generator.cs`, `TUNING/BUILDINGS.cs` (CONSTRUCTION_MASS_KG.TIER3 = 200 kg), `TUNING/MATERIALS.cs`
