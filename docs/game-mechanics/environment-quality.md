# Environment Quality: Light, Decor, and Noise

Three environmental quality systems that affect duplicant morale and stress. All three use line-of-sight checks and have no effect through solid walls. Derived from decompiled source code.

## Light

Light originates from `Light2D` components and propagates via discrete shadowcasting (`DiscreteShadowCaster`). Only cells with line-of-sight to the source receive illumination.

**Lux calculation:**
```
falloff = max(1, round(falloffRate * max(distance_in_cells, 1)))
lux_at_cell = intensity / falloff
```

The integer rounding means lux drops in discrete steps rather than smoothly. At short distances with low falloff rates, rounding can make a noticeable difference.

| Property | Description |
|----------|-------------|
| `intensity` | Source lux value (e.g., 1000 for floor lamp) |
| `falloffRate` | Decay rate per light source (default 0.5, mercury ceiling uses 0.4) |
| `distance` | Cells from source to target |

**Light shapes:**
- **Circle:** 360-degree illumination (floor lamps, light bugs)
- **Cone:** 180-degree downward (ceiling lights, sunlamps)
- **Quad:** Rectangular directional beam (mercury ceiling lights)

**Example source intensities:**

| Source | Lux | Range | Shape |
|--------|-----|-------|-------|
| Floor Lamp | 1,000 | 4 cells | Circle |
| Ceiling Light | 1,800 | 8 cells | Cone |
| Sun Lamp | 40,000 | 16 cells | Cone |
| Mercury Ceiling Light | 60,000 | 8 cells | Quad |
| Light Bug | 1,800 | 5 cells | Circle |
| Fossil Sculpture | 3,000 | 8 cells | Quad |

Grid storage: `Grid.LightCount[cell]` accumulates raw lux. `Grid.LightIntensity[cell]` combines this with sunlight exposure (up to 80,000 lux).

## Decor

`DecorProvider` components add decor values to cells within a radius. Only cells with line-of-sight receive the decor bonus. Stored items have zero decor.

**Decor sampling:** Duplicants sample decor at their current cell each frame via `DecorMonitor`. The value is smoothly accumulated over the cycle, and the daily average is applied nightly as a morale modifier.

**Bonus tiers (TUNING.DECOR.BONUS):** These are used by creatures and items. Buildings use a separate `TUNING.BUILDINGS.DECOR` system with different values (TIER0=+5 through TIER5=+30, no TIER6-8).

| Tier | Decor | Radius |
|------|-------|--------|
| TIER0 | +10 | 1 |
| TIER1 | +15 | 2 |
| TIER2 | +20 | 3 |
| TIER3 | +25 | 4 |
| TIER4 | +30 | 5 |
| TIER5 | +35 | 6 |
| TIER6 | +50 | 7 |
| TIER7 | +80 | 7 |
| TIER8 | +200 | 8 |

**Penalty tiers:** Range from -5 (radius 1) to -25 (radius 6).

**Light-decor interaction:** Any lit cell (LightIntensity > 0) receives an automatic **+15 decor bonus** on top of provider-based decor. This makes lighting functionally useful beyond visibility.

**Room interactions:** Rooms do not directly multiply decor. Room quality is determined by constraint satisfaction (furniture requirements), and rooms apply separate status effects via their `RoomType.effects[]` array. Decor is purely cell-based and independent of room boundaries.

## Noise Pollution

`NoisePolluter` components emit noise in decibels to surrounding cells via `NoiseSplat` objects.

**Falloff formula:**
```
dB_at_cell = sourceDB - (sourceDB * distance * 0.05)
```

Example: 75 dB source at 5 cells = `75 - (75 * 5 * 0.05) = 56 dB`

**Noise tiers (TUNING.NOISE_POLLUTION):**

| Tier | dB | Radius | Typical Source |
|------|----|----|----------------|
| TIER0 | 45 | 10 | Quiet equipment |
| TIER1 | 55 | 10 | Normal equipment |
| TIER2 | 65 | 10 | Moderate equipment |
| TIER3 | 75 | 15 | Loud equipment |
| TIER4 | 90 | 15 | Very loud |
| TIER5 | 105 | 20 | Extremely loud |
| TIER6 | 125 | 20 | Deafening |

Creature noise tiers are quieter (30-105 dB, 5-10 cell radius).

**Cone of Silence:** A special dampener at -120 dB, 5 cell radius, that suppresses nearby noise.

**Grid storage:** `Grid.Loudness[cell]` accumulates loudness converted from dB via `AudioEventManager.DBToLoudness()` (non-linear). Noise is temporal: splats are removed immediately when sources deactivate.

**Effects:** High loudness causes duplicant stress penalties, sampled per-frame at the duplicant's current cell. The stress threshold requires 3 seconds of continuous exposure.
