# World Generation

How the game generates worlds from seeds and settings. Derived from decompiled source code.

## Generation Pipeline

1. **Initialize seeded RNG** (worldSeed, layoutSeed, terrainSeed, noiseSeed)
2. **Load settings** (YAML: worlds, subworlds, biomes, features)
3. **Generate spatial layout** using Voronoi or PowerTree diagrams
4. **Assign biomes** to overworld cells (typically 30-40 major regions)
5. **Subdivide** into terrain cells (3000-4000 leaf nodes)
6. **Generate noise maps** (base element noise, density, heat offset)
7. **Place features** (rooms, caves, geysers) into terrain cells
8. **Apply element bands** (vertical stratification within each biome)
9. **Draw world border** (neutronium walls)
10. **Render to cells** (write element/mass/temperature to cell array)
11. **Physics settling** (500 frames of gravity, pressure, heat)
12. **Place templates** (POIs, geysers, starting base)
13. **Serialize** to save file

## Voronoi Layout

The world is divided into overworld cells using Voronoi diagrams:
- Point sites are generated with configurable density and spacing
- Each site becomes a Voronoi cell representing a biome region
- `OverworldDensityMin/Max` controls site density
- `OverworldAvoidRadius` sets minimum distance between sites

Each overworld cell is further subdivided into leaf nodes (terrain cells) via a recursive Voronoi tree.

## Element Band System

Within each biome, elements are placed using a band system -- a gradient of element types mapped to noise values:

```
noiseValue (0-1) -> lookup in element band table -> element type
```

Example band configuration for a temperate biome:
```
0.0 - 0.3: Sand (30%)
0.3 - 0.6: Sandstone (30%)
0.6 - 0.9: Igneous Rock (30%)
0.9 - 1.0: Granite (10%)
```

A noise sample of 0.45 selects Sandstone. Band sizes are relative and normalized to 0-1 range.

## Three Noise Maps

Each biome can reference three noise sources:

1. **biomeNoise** - Drives element band selection (0-1 range)
2. **densityNoise** - Varies mass per cell: `mass += mass * 0.2 * (density - 0.5)`, giving -10% to +10% variation
3. **overrideNoise** - Special element placement (100+ = katairite, 200+ = unobtanium, 300+ = void)

All noise values are normalized to local min/max before use.

## Erosion

Biomes can be tagged with erosion modifiers that create smooth transitions at boundaries:

| Tag | Effect |
|-----|--------|
| `ErodePointToCentroid` | Elements fade toward biome center |
| `ErodePointToEdge` | Elements fade toward biome edges |
| `ErodePointToBorder` | Elements fade toward world border |
| `ErodePointToWorldTop` | Top-to-bottom fade (38-58 cell transition zone) |
| `*Inv` variants | Inverse of the above |
| `*Weak` variants | Smaller fade range (7 cells vs 20) |

Erosion multiplies the base noise value, creating gradual transitions where band selections thin out near boundaries.

## Neutronium Border

The world boundary is drawn with Unobtanium (Neutronium):
- Applied to all four edges (left, right, top, bottom)
- Base thickness configured via `WorldBorderThickness`
- Thickness varies per row/column via random walk (-2 to +1 cells per step, bounded by `WorldBorderRange`)
- Only overwrites non-void cells unless force mode is enabled
- Border cells are marked to prevent template/POI placement from overwriting them

## Geyser Placement

Geysers are placed via the template rules system:

1. Template rules specify which terrain cells can host geysers (filtered by tags, zone types, temperature ranges)
2. Geyser type is selected pseudo-randomly: `seed = globalWorldSeed + x + y`
3. Each geyser type has fixed parameters:
   - Element, temperature, mass per cycle range, max pressure
   - Eruption timing (pre-erupt, erupt, post-erupt durations)
   - Optional disease payload

**Example geyser types:**
| Type | Element | Temperature | Output |
|------|---------|-------------|--------|
| Steam Vent | Steam | 383 K | 1000-2000 kg/cycle |
| Hot Water | Water | 368 K | 2000-4000 kg/cycle |
| Magma Volcano | Magma | 2000 K | 400-800 kg/cycle |
| Liquid CO2 | Liquid CO2 | 218 K | 100-200 kg/cycle |

## Physics Settling Pass

After all elements are placed, 500 frames of native simulation run to let the world settle:

```
for frame 0..499:
    if frame == 498: place template cells (POIs, geysers)
    SimMessages.NewGameFrame(0.2 seconds)
    read back updated cell states
```

Total simulated time: 100 seconds. During this pass:
- Heavy elements sink, light elements float
- Gases expand and stratify by density
- Liquids pool in low points
- Temperature begins to equilibrate
- State transitions can occur (water freezing in cold biomes, etc.)

Templates are placed at frame 498 (near the end) so they have 2 frames to settle but don't get disrupted by the full settling process.
