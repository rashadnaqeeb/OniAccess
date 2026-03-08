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

**Phase 2: Battery Charging (Three passes)**
1. Charge input transformers from regular generators
2. Charge input transformers from output transformers
3. Charge input transformers from regular batteries

Batteries are sorted by (Capacity - JoulesAvailable) ascending, so emptier batteries charge first. Available joules are divided equally among batteries needing charge.

## Overload Detection

Per `ElectricalUtilityNetwork.UpdateOverloadTime`:

1. For each wire tier independently, check if `watts_used > max_wattage + 0.5W`
2. The 0.5W fudge factor prevents false positives from floating-point rounding
3. If overloaded: accumulate `timeOverloaded += dt`
4. After **6 seconds** continuous overload: deal 1 damage to a random wire in that tier
5. If not overloaded: decay timer at `0.95 * dt` per frame (slow decay, not instant reset)

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

## Smart Batteries

Smart batteries have hysteresis-based automation:
- Separate activate and deactivate thresholds
- Sends logic HIGH signal when charge rises above activate threshold
- Sends logic LOW signal when charge drops below deactivate threshold
- Used to control generators via automation wire

## Battery Leakage

All batteries lose energy over time via `joulesLostPerSecond`. This is reported to the energy statistics screen as wasted energy.

## Energy Tracking

All power transfers are logged to `ReportManager` for the colony statistics screen, tracking energy created, consumed, and wasted per cycle.
