# Piping and Conduits

How fluid pipes and solid conveyor rails work. All three conduit systems are simulated entirely in C# (not the native sim). Derived from decompiled source code.

## Fluid Pipe System (Liquid and Gas)

### Capacity

| Type | Max mass per pipe cell |
|------|----------------------|
| Liquid pipes | 10 kg |
| Gas pipes | 1 kg |

### Pipe Physical Properties

All pipe types have 10 HP (`hitpoints: 10` in `BuildingTemplates.CreateBuildingDef`). Melting point is 1600 for all types. All pipes are non-floodable, non-overheatable, and non-entombable.

| Pipe | Material | Mass | Build Time | Decor | Thermal Conductivity |
|------|----------|------|------------|-------|---------------------|
| Liquid Pipe | Plumbable or Metal | 100 kg | 3s | None | 1.0 (default) |
| Gas Pipe | Raw Mineral or Metal | 25 kg | 3s | None | 1.0 (default) |
| Insulated Liquid Pipe | Plumbable | 400 kg | 10s | -5 (radius 1) | 0.03125 (1/32) |
| Insulated Gas Pipe | Raw Mineral | 400 kg | 10s | -5 (radius 1) | 0.03125 (1/32) |

Insulated pipes use TIER4 mass (400 kg) versus TIER2 (100 kg) for standard liquid pipes and TIER0 (25 kg) for standard gas pipes, and take over 3x longer to build (10s vs 3s), but reduce thermal conductivity to 1/32 of default. This makes them essential for transporting fluids at extreme temperatures without exchanging heat with the environment.

**Repair behavior:** Liquid pipes (both standard and insulated) have `BaseTimeUntilRepair = -1f`, meaning they cannot be repaired and must be deconstructed and rebuilt when damaged. Gas pipes (both standard and insulated) have `BaseTimeUntilRepair = 0f`, meaning duplicants will queue a repair errand immediately when damaged.

**Sources:** `LiquidConduitConfig.cs`, `GasConduitConfig.cs`, `InsulatedLiquidConduitConfig.cs`, `InsulatedGasConduitConfig.cs`, `TUNING/BUILDINGS.cs` (CONSTRUCTION_MASS_KG, DECOR), `TUNING/MATERIALS.cs`, `BuildingDef.cs` (BaseTimeUntilRepair default of 600f)

### Network Construction

When pipes are built or modified, the system constructs a flow graph:

1. **BFS from sources** (dispensers) to identify all reachable conduits
2. **BFS from sinks** (consumers) backward to find return paths
3. **Merge graphs** and **break cycles** via DFS to create a directed acyclic graph
4. **Topological sort** determines processing order (sources before sinks)

This graph is **static** until a pipe is built, destroyed, or reconfigured. Blocked pipes do NOT trigger recalculation.

### Flow Direction

Each pipe segment stores:
- **Permitted flow directions** - A bitmask (`Down|Left|Right|Up`) of allowed directions, set during graph analysis
- **Target flow direction** - ONE active direction, updated dynamically as mass moves
- **Source flow direction** - Which direction fluid came from upstream

The flow solver cycles through directions when the target is blocked: Down, Left, Right, Up.

### Flow Algorithm

The pipe system runs **every 1.0 seconds** of game time, with up to **4 passes per tick**:

1. For each conduit in network order:
   - Check available movable mass (`initial_mass - removed_mass`)
   - Try each permitted direction for a valid target
   - Target is valid if: same element (or vacuum) AND has remaining capacity
   - Transfer mass: `min(available_mass, target_capacity)`
   - Mix temperature (weighted average by mass)
   - Transfer disease proportionally
2. If any conduit made progress, repeat (up to 4 passes total)
3. Consolidate mass changes (`ConsolidateMass()` merges `added_mass`/`removed_mass` into `initial_mass`)

### No Element Mixing

**Strict rule:** If pipe cell A contains Water and pipe cell B contains Polluted Water, flow is BLOCKED. No mixing, no displacement. The pipe backs up until the player redirects flow. Only same-element or vacuum cells can receive flow.

### Bridges

Bridges bypass the normal flow solver entirely:
- Read from input cell, write to output cell directly
- Do not participate in permitted flow directions
- Transfer via `AddElement()` / `RemoveElement()` callbacks

### Preferential Flow Valve

Checks primary input FIRST. Only uses secondary input if primary is empty. Always transfers maximum available mass.

### Overflow Valve

Checks if primary output is occupied (any mass). If full, diverts to secondary output. Splits flow based on space availability.

### Pipe Contents State

Each pipe cell tracks per-tick changes:
```
initial_mass    - Mass at tick start
added_mass      - Mass added during this tick
removed_mass    - Mass removed during this tick
mass            = initial_mass + added_mass - removed_mass
movable_mass    = initial_mass - removed_mass
```

`ConsolidateMass()` commits changes after all passes complete, merging `added_mass`/`removed_mass` into `initial_mass`. Between passes, the `movable_mass` property prevents mass added during the current tick from being moved again.

### Blockage Behavior

When pipes are full:
1. No backpressure signal propagates through the graph
2. Fluid simply accumulates upstream of the blockage
3. Dispensers detect full output pipes and set `blocked = true`
4. Eventually the entire pipe network backs up

## Valves

Valves are inline conduit buildings that control flow. All valve types are 1x2 tiles, rotatable 360 degrees, with input at `(0,0)` and output at `(0,1)`. All share 30 HP and 1600 melting point.

### Flow-Limiting Valves (Manual)

**Gas Valve** and **Liquid Valve** let the player set a maximum flow rate via a slider. They use `ValveBase` for the flow logic and `Valve` for the work errand.

| Property | Gas Valve | Liquid Valve |
|----------|-----------|--------------|
| Max flow | 1 kg/s | 10 kg/s |
| Material | Raw Metal (TIER1) | Raw Metal (TIER3) |
| Power | None | None |

**How flow limiting works:** `ValveBase` registers a per-tick callback with `ConduitFlow`. Each tick it reads the input cell contents and transfers `min(contents.mass, currentFlow * dt)` to the output cell via `AddElement()` / `RemoveElement()`. Setting `currentFlow` to 0 blocks flow entirely; setting it to `maxFlow` allows full throughput.

**UI controls:** `ValveSideScreen` presents a `KSlider` (range 0 to `maxFlow`) and a `KNumberInputField` (in grams/s). The slider value maps directly to kg/s internally. Changing the value calls `Valve.ChangeFlow()`, which queues a `WorkChore<Valve>` -- a duplicant must physically visit the valve and perform 5 seconds of work before the new flow rate takes effect. In instant-build debug mode, the change applies immediately.

**Errand queuing:** If the desired flow already matches the current flow, no errand is created. If a new desired value is set while an errand is pending, the existing chore is reused (the chore reads `desiredFlow` on completion). The valve displays `ValveRequest` and `PendingWork` status items while waiting.

**Always operational:** Both configs call `GeneratedBuildings.MakeBuildingAlwaysOperational()` and destroy `RequireInputs`, `ConduitConsumer`, and `ConduitDispenser` components. Valves work without power and without connected pipes on both sides (they simply transfer nothing if a side is missing).

**Sources:** `ValveBase.cs`, `Valve.cs`, `ValveSideScreen.cs`, `GasValveConfig.cs`, `LiquidValveConfig.cs`

### Shutoff Valves (Automation-Controlled)

**Gas Shutoff** and **Liquid Shutoff** are all-or-nothing valves controlled by automation signals. They use `OperationalValve` (extends `ValveBase`) instead of `Valve`.

| Property | Gas Shutoff | Liquid Shutoff |
|----------|-------------|----------------|
| Max flow | 1 kg/s | 10 kg/s |
| Material | Refined Metal (TIER1) | Refined Metal (TIER1) |
| Power | 10 W | 10 W |
| Logic port | 1 input (on/off) | 1 input (on/off) |

**Behavior:** `OperationalValve` subscribes to `OnOperationalChanged`. When operational (logic signal active AND powered), it sets `CurrentFlow = MaxFlow`. When not operational, it sets `CurrentFlow = 0`. There is no slider and no duplicant errand -- the valve responds instantly to automation signals.

**Default state:** `LogicOperationalController.unNetworkedValue = 0`, so shutoff valves default to CLOSED when no automation wire is connected. This prevents accidental flow.

**Solid variant:** `SolidLogicValve` / `SolidLogicValveConfig` provides the same on/off behavior for conveyor rails. It uses a `SolidConduitBridge` internally and a state machine (`off` / `on.idle` / `on.working`) instead of `OperationalValve`.

**Sources:** `OperationalValve.cs`, `GasLogicValveConfig.cs`, `LiquidLogicValveConfig.cs`, `SolidLogicValve.cs`, `SolidLogicValveConfig.cs`

### Limit Valves (Metered Quantity)

**Gas Limit Valve**, **Liquid Limit Valve**, and **Solid Limit Valve** pass a set total quantity of material, then stop. They use `LimitValve` with a `ConduitBridge` (or `SolidConduitBridge` for solid).

| Property | Gas / Liquid Limit Valve | Solid Limit Valve |
|----------|--------------------------|-------------------|
| Max limit | 500 kg | 500 units |
| Default limit | 0 (blocks until set) | 0 |
| Material | Refined Metal + Plastic | Refined Metal + Plastic |
| Power | 10 W | 10 W |
| Logic ports | 1 input (reset), 1 output (limit reached) | 1 input (reset), 1 output (limit reached) |

**How metering works:** `LimitValve` hooks into the bridge's `desiredMassTransfer` delegate, returning `min(mass, RemainingCapacity)` where `RemainingCapacity = Limit - Amount`. Each transfer adds to `Amount`. When `Amount >= Limit`, the valve sets `operational.SetFlag(limitNotReached, false)`, which makes the bridge stop transferring.

**Automation integration:** The output port sends GREEN (1) when the limit is reached, RED (0) otherwise. Sending a GREEN signal to the reset input port resets `Amount` to 0, allowing the valve to pass another batch.

**Solid variant difference:** The solid variant (configured by `SolidLimitValveConfig`, same `LimitValve` class) counts in units (via `displayUnitsInsteadOfMass = true`) rather than kilograms. Each transferred item increments `Amount` by `transferredMass / MassPerUnit`.

**Sources:** `LimitValve.cs`, `LimitValveTuning.cs`, `GasLimitValveConfig.cs`, `LiquidLimitValveConfig.cs`, `SolidLimitValveConfig.cs`

### Check Valves

ONI has no check valve building. One-way flow is an inherent property of the pipe network: the graph construction algorithm (BFS from sources, topological sort) establishes fixed flow directions. To enforce one-way flow at a specific point, players use a bridge, which has a defined input/output direction and bypasses the flow solver.

### How Valves Interact with Conduit Flow

Manual valves and shutoff valves participate directly in the `ConduitFlow` update loop. `ValveBase.ConduitUpdate()` is registered via `Conduit.GetFlowManager(conduitType).AddConduitUpdater()` and runs after the normal flow passes and consolidation. The valve reads from its input cell and writes to its output cell using the same `AddElement()` / `RemoveElement()` API that bridges use. This means:

- Valves obey the **no element mixing** rule: if the output cell contains a different element, `AddElement()` returns 0 and nothing transfers
- Valves do not store fluid internally -- they transfer directly from input to output each tick
- A valve with `currentFlow = 0` acts as a complete blockage, causing upstream backup per the normal blockage behavior
- Valves do not trigger network recalculation; they operate within the existing flow graph

Limit valves work differently: they use a `ConduitBridge` component and hook into its transfer delegates rather than running their own `ConduitUpdate`. The bridge handles the actual input/output transfer, and `LimitValve` constrains how much the bridge is allowed to move.

## Phase Changes in Pipes

Pipe contents exchange heat with the surrounding environment. When the temperature of a fluid inside a pipe crosses its element's phase transition point (freezing point for liquids, boiling point for gases in liquid pipes, or condensation/vaporization thresholds), the native simulation flags that pipe cell as frozen or melted.

### Temperature Simulation

`ConduitTemperatureManager.Sim200ms()` runs every 200ms of game time. For each occupied pipe cell, the native sim calculates heat exchange using:
- **Conduit heat capacity** = pipe building mass x pipe material specific heat capacity
- **Conduit thermal conductivity** = pipe material thermal conductivity x pipe building thermal conductivity multiplier
- **Insulated flag** = `true` when the pipe's `ThermalConductivity` multiplier is less than 1.0 (insulated pipes use 0.03125, standard pipes use 1.0)

The sim returns two lists: pipe cells whose contents froze and pipe cells whose contents boiled (melted in the code, but functionally it means the contents changed state).

### Damage and Contents Loss

When a pipe cell is flagged as frozen or boiled, `ConduitFlow.FreezeConduitContents()` or `MeltConduitContents()` fires. Both apply the same logic:

1. **Mass threshold**: The event is ignored if the pipe contains less than 10% of its maximum capacity (1 kg for liquid pipes, 0.1 kg for gas pipes). Small residual amounts do not cause damage.
2. **Damage**: The pipe takes 1 HP of damage. All pipes have 10 HP, so 10 state-change events destroy the pipe.
3. **Contents dumped**: `EmptyConduit()` ejects the pipe's contents into the world cell as a free substance via `SimMessages.AddRemoveSubstance`. For a liquid pipe whose contents froze, this means a solid debris tile appears in the world. For a liquid pipe whose contents boiled, gas is released into the atmosphere.

The game displays "Cold Damage" for freezing and "Heat Damage" for boiling as damage pop-ups.

### Liquid vs Gas Pipes

Both liquid and gas conduits use the same `ConduitTemperatureManager` and the same freeze/boil logic. The only differences are the visual effects: frozen liquid pipes show an ice effect, while frozen gas pipes (where the gas liquefied) show a liquid leak effect. Boiling in either pipe type shows a gas leak effect.

### Repair After State-Change Damage

Repair behavior follows the same rules as other damage sources (see Pipe Physical Properties above): gas pipes queue a repair errand immediately, while liquid pipes cannot be repaired and must be deconstructed and rebuilt.

### Prevention

- **Insulated pipes** reduce thermal conductivity to 1/32 of default, dramatically slowing heat exchange between pipe contents and the environment. They do not prevent phase changes outright -- if the fluid enters the pipe already near its transition temperature, or the environment is extreme enough, state changes still occur.
- **Material selection** affects heat capacity: higher specific heat capacity means the pipe absorbs or releases heat more slowly. Ceramic insulated pipes are common because ceramic has very low thermal conductivity.
- **Temperature management** is the only complete prevention: ensure the environment around the pipe stays within the fluid's phase-stable range, or pre-cool/pre-heat the fluid before piping it through hostile areas.

**Sources:** `ConduitTemperatureManager.cs` (Sim200ms, Allocate), `Conduit.cs` (OnConduitFrozen, OnConduitBoiling), `ConduitFlow.cs` (FreezeConduitContents, MeltConduitContents, EmptyConduit), `TUNING/BUILDINGS.cs` (DAMAGE_SOURCES), `STRINGS/BUILDINGS.cs` (DAMAGESOURCES)

## Solid Conveyor System

Conveyors are fundamentally different from fluid pipes:

| Aspect | Fluid Pipes | Conveyors |
|--------|-------------|-----------|
| Storage | Float mass (fractional) | One discrete Pickupable per cell |
| Flow rate | Up to 10 kg / 1 kg per tick | 1 item per tick |
| Mixing | Same element only | N/A (single items) |
| Direction | Multi-direction graph | Single-direction paths |
| Animation | Mass-based lerp | Smooth object movement between cells |

**Conveyor direction** is determined by DFS from sources to sinks, creating explicit paths. Items move one cell per tick along these paths.

**Dead-end handling:** When an item reaches a dead end (no valid downstream), it stops. The conveyor system recalculates the target direction for that cell.
