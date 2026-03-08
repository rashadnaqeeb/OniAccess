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

Insulated pipes cost 4x the mass of standard pipes (TIER4 vs TIER2/TIER0) and take over 3x longer to build, but reduce thermal conductivity to 1/32 of default. This makes them essential for transporting fluids at extreme temperatures without exchanging heat with the environment.

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
2. Consolidate mass changes
3. Repeat if any conduit made progress (up to 4 passes)

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

`ConsolidateMass()` commits changes between passes.

### Blockage Behavior

When pipes are full:
1. No backpressure signal propagates through the graph
2. Fluid simply accumulates upstream of the blockage
3. Dispensers detect full output pipes and set `blocked = true`
4. Eventually the entire pipe network backs up

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
