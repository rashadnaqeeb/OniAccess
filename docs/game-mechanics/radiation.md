# Radiation

How radiation propagates, shields, and affects duplicants. DLC expansion feature. Derived from decompiled source code.

Radiation can be entirely disabled in world settings.

## Propagation Model

Radiation uses a **ray-based propagation model**, not grid diffusion. This is fundamentally different from gas/liquid simulation:

1. Each `RadiationEmitter` casts rays in configured directions
2. For each ray, radiation intensity decreases per cell based on material absorption
3. Ray propagation details (absorption formula, decay per cell, ray limits) are implemented in the native C++ sim (SimDLL.dll) and cannot be verified from decompiled C# source
4. The C#-side shielding display uses these formulas (see Material Shielding section below)

Because radiation is ray-based, it is localized around sources. Distant areas receive minimal radiation unless emitters are powerful or multiple sources converge.

## Radiation Grid

The `RadiationGridManager` maintains a per-cell radiation value in `Grid.Radiation[cell]`. This value lingers with a decay rate of 1/4 per frame, meaning radiation persists for several frames after the source stops.

Configurable simulation parameters:
- `LingerRate` - How long radiation persists in a cell
- `BaseWeight` - Base weighting for cell radiation
- `DensityWeight` - Weight scaling by material density
- `ConstructedFactor` - Enhancement for constructed tiles
- `MaxMass` - Maximum mass threshold for absorption calculations

## Emission Types

`RadiationEmitter.RadiationEmitterType` enum:
- `Constant` - steady emission
- `Pulsing` - pulsed emission
- `PulsingAveraged` - averaged pulsed emission
- `SimplePulse` - simple pulse
- `RadialBeams` - radial beam pattern
- `Attractor` - radiation attractor

Angular spread is controlled by `emitAngle` (default 360 degrees) and `emitDirection` fields, independent of emitter type.

## Material Shielding

Materials with `radiationAbsorptionFactor >= 0.8` are rated "Excellent Radiation Shield" in the UI.

**Shielding calculation for tiles:**
```
For constructed tiles:   absorption = factor * 0.8
For natural materials:   absorption = factor * 0.3 + (mass / 2000) * factor * 0.7
```

Constructed tiles (like lead blocks) provide consistent 0.8x absorption efficiency. Natural materials improve with mass, capped at 2000 kg.

## Duplicant Exposure

Duplicants track radiation exposure through the `RadiationMonitor`:

**Per-frame exposure:**
```
resistanceModifier = clamp01(1 - radiationResistanceAttribute)
exposureDelta = Grid.Radiation[cell] * resistanceModifier / 600 * deltaTime
```

**Sickness thresholds (multiplied by difficulty setting):**

| Level | Threshold | Effect |
|-------|-----------|--------|
| Minor | 100 rads | Warning, minor symptoms |
| Major | 300 rads | Significant debilitation |
| Extreme | 600 rads | Severe symptoms |
| Deadly | 900 rads | Incapacitation risk |

**Difficulty scaling** (multiplier applied to thresholds -- higher = more forgiving):
- Easiest: 100x (radiation sickness practically disabled)
- Easier: 2x
- Normal: 1.0x
- Harder: 0.66x
- Hardest: 0.33x (sickness triggers at 33/99/198/297 rads)

## High-Energy Particles (HEP)

HEP is the game's radiation projectile system:

**HEP Spawners** absorb ambient radiation and convert it to particles:
```
particleStorage += (cellRadiation / 600) * radiationSampleRate * 0.1
```
Power consumption scales: `clamp(radiationPerSecond * 286W, 60W-480W)`

**HEP Detonation** on impact:
```
emitRads = payload * 0.5 * 600 / 9    (radiation burst)
fallout = payload * 0.001 kg           (Fallout element creation)
radiationPoisoning = payload * 0.5 / 0.01   (disease units)
```
Creates radioactive contamination (Fallout element) with an explosion emit radius of 6 cells (`EXPLOSION_EMIT_RADIUS = 6`).

**HEP Payload Decay:** Particles lose 0.1 payload per cell crossed. Maximum travel distance depends on initial payload (e.g., payload of 50 travels 500 cells). Maximum payload is 500 (`MAX_PAYLOAD`). Particles are destroyed when payload reaches 0 or they leave the map.

## Plant Mutations

Many plant mutations modify `MinRadiationThreshold` by +250 rads, allowing mutant plants to thrive in higher radiation environments. Plants use a comfort system with "too dark" (under-irradiated) and "too bright" (over-irradiated) states.
