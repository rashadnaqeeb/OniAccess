# Solar Panels

How solar panels convert light intensity into electrical power. Derived from decompiled source code.

## Building: Solar Panel

A 7-wide, 3-tall stationary generator that converts sunlight (and any light) into power.

| Property | Value |
|----------|-------|
| Size | 7 x 3 |
| Max output | 380 W |
| Material | Glass (200 kg) |
| Build time | 120 s |
| HP | 10 |
| Overheat temp | 2400 K (default) |
| Heat output | 0 kDTU/s |
| Decor | -15 (radius 3) |
| Noise | Tier 5 (Noisy) |
| Tech | Renewable Energy |
| Power distribution order | 9 (last priority) |

The bottom row (7 cells at y=0) becomes solid via `MakeBaseSolid`, serving as its own foundation. The building can be placed anywhere (`BuildLocationRule.Anywhere`).

**Source:** `SolarPanelConfig.cs`

## Power Calculation

Power is calculated every 200ms in `SolarPanel.EnergySim200ms()`:

```
total_watts = sum(Grid.LightIntensity[cell] * 0.00053 for each solar cell)
total_watts = clamp(total_watts, 0, 380)
```

The constant `WATTS_PER_LUX = 0.00053` converts light intensity (lux) to watts.

### Solar Cell Offsets

The panel samples 14 cells arranged in two rows across its 7-wide top surface:

| Row | Y offset | X offsets |
|-----|----------|-----------|
| Top | +2 | -3, -2, -1, 0, +1, +2, +3 |
| Middle | +1 | -3, -2, -1, 0, +1, +2, +3 |

These offsets are relative to the building's origin cell. The top two rows of the building are the light-collecting surface; the bottom row is the solid base.

### Light Intensity Source

`Grid.LightIntensity[cell]` combines sunlight and artificial light (`Grid.cs`):

```
light_intensity = (ExposedToSunlight[cell] / 255) * currentSunlightIntensity + LightCount[cell]
```

- `ExposedToSunlight` is a 0-255 byte computed by the native sim. Any solid, non-transparent tile blocks sunlight for cells below it.
- `currentSunlightIntensity` is the world's sunlight level (see table below).
- `LightCount` is artificial light from Ceiling Light, Sun Lamp, etc.

Solar panels respond to any light source, not just sunlight.

## Sunlight Intensity by Asteroid

Each asteroid has a fixed sunlight intensity set by world traits. Default is `VERY_HIGH` (80,000 lux).

| Level | Lux | Max panel output (14 cells) |
|-------|-----|----------------------------|
| NONE | 0 | 0 W |
| VERY_VERY_LOW | 10,000 | 74.2 W |
| VERY_LOW | 20,000 | 148.4 W |
| LOW | 30,000 | 222.6 W |
| MED_LOW | 35,000 | 259.7 W |
| MED | 40,000 | 296.8 W |
| MED_HIGH | 50,000 | 371.0 W |
| HIGH | 60,000 | 380 W (capped) |
| VERY_HIGH (default) | 80,000 | 380 W (capped) |
| VERY_VERY_HIGH | 100,000 | 380 W (capped) |
| VERY_VERY_VERY_HIGH | 120,000 | 380 W (capped) |

At `HIGH` (60,000 lux) and above, a fully exposed panel hits the 380 W cap. Below that, output scales linearly with sunlight.

**Source:** `TUNING/FIXEDTRAITS.cs` (`SUNLIGHT` class)

## Obstructions and Partial Output

Each of the 14 solar cells is evaluated independently. A cell blocked from sunlight contributes 0 watts from sunlight (but can still receive artificial light). This means:

- Partial coverage reduces output proportionally. Blocking 7 of 14 cells halves sunlight contribution.
- Each cell's `ExposedToSunlight` value (0-255) can be partially reduced, not just fully blocked. The native sim computes this based on tiles above.
- Solid tiles (natural or constructed) block sunlight. Glass tiles are transparent (`setTransparent = true`, `BlockTileIsTransparent = true`) and pass sunlight through.

### Space Exposure

Solar panels need to be at the surface of the asteroid with a clear path to space. The `ExposedToSunlight` grid value is computed by the native simulation and drops to 0 for any cell with solid non-transparent material above it in the world column.

There is no explicit "space exposure" requirement in the `SolarPanel` class itself. The requirement is implicit: `Grid.LightIntensity` returns 0 for underground cells with no sunlight and no artificial light.

## Glass Tiles

Glass tiles (`GlassTileConfig`) are the key building for routing sunlight:

| Property | Value |
|----------|-------|
| Size | 1 x 1 |
| Material | Transparent (100 kg) |
| Transparent | Yes |
| Max temp | 800 K |
| HP | 100 (destroyed on damage) |

Glass tiles set `setTransparent = true` on their `SimCellOccupier`, telling the native sim to pass sunlight through. This allows building floors above solar panels without blocking their output.

**Source:** `GlassTileConfig.cs`

## Rocket Solar Panel Module

A smaller solar panel for rockets. Requires Spaced Out DLC.

| Property | Value |
|----------|-------|
| Size | 3 x 1 |
| Max output | 60 W |
| Material | Glass (200 kg, hollow tier 1) |
| Build time | 30 s |
| Tech | Space Power |
| Rocket burden | Insignificant |
| Solar cells | 3 (at y=0: x=-1, 0, +1) |

Uses the same `WATTS_PER_LUX = 0.00053` conversion constant. The module has `PartialLightBlocking`, which sets cell property flags (value 48) on its placement cells to partially block light for modules below it.

When the rocket is in flight (invalid grid cell or invalid world), the module produces a flat 60 W regardless of actual light conditions. On the ground, it samples `Grid.LightIntensity` like the stationary panel.

The module operates on the rocket's virtual power circuit (`IsVirtual = true`) and does not need physical wire connections (`wireConnectedFlag` and `generatorConnectedFlag` are forced true).

**Source:** `SolarPanelModuleConfig.cs`, `ModuleSolarPanel.cs`, `PartialLightBlocking.cs`
