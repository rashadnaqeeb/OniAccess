# Fluid Physics

How gases and liquids move, pressure works, and things fall. Derived from decompiled source code.

## Pressure-Driven Diffusion

The native C++ simulation uses pressure-driven diffusion to move gases and liquids. There is no momentum or velocity tracking per cell. Flow is entirely determined by pressure differences between adjacent cells.

For each cell, the sim:
1. Calculates pressure difference to each neighbor
2. Applies the element's viscosity and flow modifiers
3. Checks if flow exceeds `minHorizontalFlow` or `minVerticalFlow` thresholds
4. Transfers mass proportional to the time step and pressure gradient
5. Records accumulated flow for visualization

### Flow Properties

- **Low viscosity** (like water) = faster equalization
- **High viscosity** (like crude oil) = slower spreading
- **minFlow thresholds** prevent visually annoying micro-flows where tiny amounts shuttle back and forth

### Gas Behavior

Gases naturally stratify by density:
- **Light gases** (hydrogen) rise to the top of enclosed spaces
- **Heavy gases** (chlorine, CO2) sink to the bottom
- Gases expand to fill available space through pressure equalization
- Two cells of gas equalize in roughly 10-20 simulation frames

### Liquid Behavior

- Liquids are primarily gravity-driven (they fall and pool)
- Horizontal spreading is limited by viscosity
- Liquids settle into the lowest available cells
- When a liquid becomes unstable (unsupported), it enters the falling particle system

### One Element Per Cell

This is the most important constraint. Because each cell holds exactly one element:
- Gas boundaries are sharp, not gradual
- Different gases cannot occupy the same cell
- When two different elements try to occupy the same cell, one displaces the other
- This is why gas sorting happens (light gases push up past heavy ones)

### Element Replacement

When modifying cells, three replacement modes exist:
1. **None** - Simply add mass to the cell
2. **Replace** - Replace the entire cell's element, discarding the old one
3. **ReplaceAndDisplace** - Replace the element and physically displace old material:
   - Displaced solids drop as falling chunks
   - Displaced liquids become falling water particles
   - Displaced gases disperse to adjacent cells

## Pressure

### Pressure Is Mass

ONI does not have a separate pressure simulation. **Pressure is directly proportional to mass in a cell.** The conversion is:

```
pressure = mass * element.defaultValues.pressure
```

Or inversely:
```
mass = pressure / element.defaultValues.pressure
```

This means "high pressure" simply means "high mass per cell." Pressure relief means removing mass (venting gas to space, pumping liquid away).

### Overpressure Effects

**For emitters/sublimates:** When the destination cell's mass exceeds `maxDestinationMass`, emission is blocked. The building shows "Blocked (High Pressure)" status and waits for pressure to drop naturally.

**For duplicants:** The `PressureMonitor` tracks whether a duplicant is in high-pressure gas (any gas cell with mass > 4 kg). If exposed for more than 3 seconds continuously, the duplicant receives the "Popped Ear Drums" effect. This is a pressure-based condition, not germ-based.

### Gas maxMass Property

During element loading, `ElementLoader` overrides every gas element's `maxMass` to **1.8 kg**, regardless of the YAML definition. This value is passed to the native C++ sim via `PhysicsData`. However, it is NOT a hard ceiling. Gas cells routinely exceed 1.8 kg in gameplay (overpressured vents, sealed rooms with emitters, etc.).

The `maxMass` value for gases likely acts as a **flow/diffusion parameter** within the native sim, influencing when a cell resists accepting more mass from pressure-driven flow. Direct emission from buildings (`ElementEmitter`, `AddRemoveSubstance`) can push cells well above this threshold. The C# side has a mass-clamping check in `ModifyCell()`, but it contains a dead-code bug (`if (element.maxMass == 0f && mass > element.maxMass)` -- the condition is always false when maxMass is 1.8), so no C#-side clamping actually occurs.

The related constant `Sim.MAX_SUBLIMATE_MASS = 1.8f` caps the maximum gas mass emitted per sublimation event, which is a separate per-emission limit, not a per-cell cap.

### Building Pressure Vulnerability

Buildings track pressure using a momentum-based reading updated every 1000ms:

```
displayPressure = previousPressure * 0.7 + currentPressure * 0.3
```

This 70/30 blend means pressure changes take multiple seconds to fully register. Buildings enter five pressure states:

| State | Condition |
|-------|-----------|
| LethalLow | Below lethal low threshold |
| WarningLow | Between lethal and warning low |
| Normal | Within operating range |
| WarningHigh | Between warning and lethal high |
| LethalHigh | Above lethal high threshold |

Buildings malfunction in LethalLow or LethalHigh states. The safe atmosphere check requires the building's element in at least 6% of occupied cells.

### Emission Blocking Check

The `GameUtil.IsEmissionBlocked()` function checks all four neighbors (below, left, right, above). Each neighbor is considered "over pressure" if it is either non-gaseous (and not vacuum) OR has gas mass >= 1.8 kg. Emission is blocked only when ALL four neighbors meet this condition. A single neighbor that is vacuum or has gas below 1.8 kg is enough to allow emission.

Note: the `Sublimates` component uses its own separate check with the building's configured `maxDestinationMass` value, which may differ from the hardcoded 1.8 kg used by `IsEmissionBlocked`.

## Falling Solids

Solid materials are not stored in the grid like gases and liquids when they become unsupported. Instead:
- Solids with the `Unstable` flag enter a falling chunk physics system
- They are simulated as discrete particles with gravity
- When they reach a stable surface (floor or other solid), they stick and re-enter the grid as a cell element
- The `do_vertical_solid_displacement` parameter controls whether solids prefer falling downward vs. spreading sideways

### UnstableGroundManager

Solid elements with the `Unstable` flag (0x08 in the state bitfield) enter a separate falling chunk system when they lose support.

**Trigger conditions:**
- Cell below becomes non-solid (mined, melted, etc.)
- Solid placed in an unsupported position
- World generation settling pass

**Physics:** Uses `GravityComponent` for per-frame position updates:
1. Accumulate velocity from gravity acceleration
2. Update position
3. Check if new position overlaps a solid cell (ground detection)
4. On landing: convert back to grid cell via `SimMessages.AddRemoveSubstance()` with `do_vertical_solid_displacement: true`

**Ground detection:** Uses collider bounds to project a check point below the object. Landing is detected when position minus ground offset minus 0.07 units (epsilon buffer) intersects a solid cell.

**Vertical displacement:** The `do_vertical_solid_displacement` flag tells the native sim to push overlapping solids upward when mass is added at the landing cell. This prevents debris from merging into existing terrain.

**Landing collision notification:** On impact, the system searches a 3x3 region above the landing point for colliders and broadcasts a collision event. This notifies nearby creatures and objects of the impact.

## Falling Liquids (FallingWater)

Liquid particles are managed by the `FallingWater` system, which tracks each particle as a physics object with position, velocity, and element properties.

**Particle creation:** When a liquid cell loses support or is displaced, `SpawnFallingLiquidInfo` creates a particle with the liquid's element, mass, temperature, and disease data.

**Physics update (per frame):**
1. Apply gravity to velocity
2. Update position by velocity * dt
3. Check collision with grid cells below
4. If collision: resolve to grid (add mass to landing cell)

**Liquid-on-liquid collision:** When a falling liquid particle hits an existing liquid cell:
- If the target cell's mass is below the element's default mass: continue falling (shallow liquid, particle passes through)
- If the target has significant mass: compare molar masses
  - Denser particle (higher molar mass) sinks through lighter liquid
  - Lighter particle stops and resolves to grid, spawning a splash effect

This molar mass comparison is why molten metal sinks through water rather than pooling on top.

**Particle mass scaling:** Particle visual size scales linearly from 25% (near-zero mass) to 100% (at `particleMassToSplit` threshold, ~75 kg).

**Render limit:** Maximum 16,249 particles can render simultaneously (limited by Unity's 65,536-vertex mesh cap at 4 vertices per particle). Beyond this, excess particles still simulate but don't render.

**Liquid drag:** Objects falling through liquid experience drag. Each object gets a deterministic variance factor based on its instance ID:
```
variance = (instanceID % 1000) / 1000 * 0.25
maxVelocity = tuning.maxVelocityInLiquid * (1.0 + variance)
```
Objects lose 8-16% of excess velocity per frame in liquid, with the exact rate varying per object to prevent lockstep movement of grouped items.

## Digging and Cell-to-Item Conversion

When a cell is mined via `Diggable`:
1. The cell's element, mass, temperature, and disease are recorded
2. `WorldDamage` accumulates damage on the cell (0.0 to 1.0)
3. When damage reaches 1.0, the cell is destroyed
4. The solid material spawns as a droppable `Pickupable` item at the cell position
5. The grid cell becomes Vacuum (mass 0)

The spawned item inherits the exact mass, temperature, and disease of the original cell. No mass is created or destroyed during mining.

## Faller Component State Machine

Physical objects (debris, items) use the `FallerComponents` system:

```
SPAWN:
  If on solid ground with zero velocity -> register listener, wait
  If floating or has velocity -> add gravity, start falling

FALLING:
  Gravity updates position each frame
  CellChangeMonitor fires on cell transitions
  OnSolidChanged checks ground state

LANDED:
  Remove gravity component
  Register solid-change listener (wait for ground to disappear)
```

The listener monitors a 1-cell-wide, 2-cell-tall region at the object's position for `solidChangedLayer` events. When the ground beneath disappears, the object re-enters falling state.

## Creature Falling

Creatures use `FallMonitor` instead of the generic faller system. A creature is falling if:
1. Not currently navigating (not mid-path)
2. Not in a travel tube
3. Not standing on a valid nav cell
4. Not entombed (surrounded by impassable solids)

Entombment is detected when the creature's cell or the cell above is solid and not `DupePassable`. Entombed creatures enter a trapped state rather than falling.

## The Settling Pass (World Generation)

During world generation, a 500-frame physics settling pass runs to let materials find their natural positions:
- Heavy elements sink, light elements float
- Gases expand and stratify
- Liquids pool at the bottom of cavities
- Each frame simulates 0.2 seconds, for a total of 100 seconds of simulated settling time
