# Chemical Reactions

How elements interact with each other in the simulation, and how buildings mediate element conversions. Derived from decompiled source code.

For building-specific details (power, dimensions, heat), see `building-catalog.md`. For phase transitions (melting, boiling, freezing), see `state-transitions.md`. For germ infection mechanics and sickness effects, see `disease-and-germs.md`.

## ElementInteraction System

The simulation supports element interactions via the `ElementInteraction` struct in `SimMessages.cs`:

```
struct ElementInteraction {
    interactionType              // Reaction type (uint hash)
    elemIdx1, elemIdx2           // Reactant elements
    elemResultIdx                // Product element
    minMass                      // Minimum mass threshold for both reactants
    interactionProbability       // 0.0-1.0 chance per frame
    elem1MassDestructionPercent  // Fraction of element 1 consumed
    elem2MassRequiredMultiplier  // How much element 2 needed relative to element 1
    elemResultMassCreationMultiplier  // Output mass multiplier
}
```

When two elements meet in adjacent cells with sufficient mass, they react probabilistically. Reactions are registered at game startup via `SimMessages.CreateElementInteractions()`.

Three reaction types are defined in `ElementInteractionHashes.cs`:

| Type | Hash | Description |
|------|------|-------------|
| GasObliteration | 1747383933 | Gas element is destroyed on contact |
| LiquidObliteration | 764054208 | Liquid element is destroyed on contact |
| LiquidConversion | 1537054354 | Liquid element converts to another element |

The specific element pairings and parameter values for these reactions are configured in the native C++ sim (SimDLL.dll) and are not visible in the decompiled C# source.

## Element convertId

Each element has a `convertId` field loaded from YAML data (`ElementLoader.cs` line 254). This is passed to the native sim as `convertIndex` (`Sim.cs` line 232). Like sublimation and off-gassing parameters, the actual conversion behavior is handled by the native sim. The C# side only stores and passes the value.

## Building-Mediated Conversions

Buildings use `ElementConverter` components to consume input elements and produce output elements at defined rates. The converter runs once per second (SIM_1000ms) when the building is operational, scaled by machinery speed.

### Conversion System Details (ElementConverter.cs)

- Input mass is consumed proportionally when available mass is less than requested
- Output temperature is the maximum of `minOutputTemperature` and the mass-weighted average temperature of consumed inputs (unless `useEntityTemperature` is set)
- Disease germs from consumed inputs are distributed across outputs weighted by `diseaseWeight`
- Conversion rate scales with the Machinery Speed attribute

### Oxygen Generation Conversions

**Electrolyzer** (`ElectrolyzerConfig.cs`):
- 1 kg/s Water -> 0.888 kg/s Oxygen + 0.112 kg/s Hydrogen

**Algae Terrarium** (`AirFilterConfig.cs`):
- 0.1 kg/s Polluted Oxygen + 0.133 kg/s Filter -> 0.09 kg/s Oxygen + 0.143 kg/s Clay

**Algae Deoxidizer** (`MineralDeoxidizerConfig.cs`):
- 0.55 kg/s Algae -> 0.5 kg/s Oxygen

**Rust Deoxidizer** (`RustDeoxidizerConfig.cs`):
- 0.75 kg/s Rust + 0.25 kg/s Salt -> 0.57 kg/s Oxygen + 0.03 kg/s Chlorine Gas + 0.4 kg/s Iron Ore

**Sublimation Station** (`SublimationStationConfig.cs`, DLC1 only):
- 1 kg/s Polluted Dirt -> 0.66 kg/s Polluted Oxygen

**Oxyfern plant** (`OxyfernConfig.cs`):
- 0.000625 kg/s CO2 -> 0.03125 kg/s Oxygen (50x output multiplier)
- Also consumes 0.0317 kg/s Water and 0.00667 kg/s Dirt via irrigation/fertilization

### CO2 Removal Conversions

**Carbon Skimmer** (`CO2ScrubberConfig.cs`):
- 0.3 kg/s CO2 + 1 kg/s Water -> 1 kg/s Polluted Water

**Algae Habitat** (`AlgaeHabitatConfig.cs`):
- 0.03 kg/s Algae + 0.3 kg/s Water -> 0.04 kg/s Oxygen + 0.29 kg/s Polluted Water
- Also consumes 0.000333 kg/s CO2 from atmosphere (non-required, radius 3)

### Water Processing Conversions

**Water Sieve** (`WaterPurifierConfig.cs`):
- 5 kg/s Polluted Water + 1 kg/s Filter -> 5 kg/s Water + 0.2 kg/s Polluted Dirt

**Desalinator** (`DesalinatorConfig.cs`, two separate converters):
- Salt Water mode: 5 kg/s Salt Water -> 4.65 kg/s Water + 0.35 kg/s Salt
- Brine mode: 5 kg/s Brine -> 3.5 kg/s Water + 1.5 kg/s Salt

### Oil and Petroleum Conversions

**Oil Well** (`OilWellCapConfig.cs`):
- 1 kg/s Water -> 3.333 kg/s Crude Oil (output at 363.15 K)
- Also emits Natural Gas (Methane) at 0.033 kg/s, 573.15 K, up to 80 kg max pressure

**Oil Refinery** (`OilRefineryConfig.cs`):
- 10 kg/s Crude Oil -> 5 kg/s Petroleum + 0.09 kg/s Natural Gas (Methane)
- Output temperature: 348.15 K (75 C)

**Polymer Press** (`PolymerizerConfig.cs`):
- 0.833 kg/s Petroleum (or Naphtha) -> 0.5 kg/s Plastic (Polypropylene) + 0.00833 kg/s Steam + 0.00833 kg/s CO2

### Organic Processing Conversions

**Ethanol Distillery** (`EthanolDistilleryConfig.cs`):
- 1 kg/s Lumber -> 0.5 kg/s Ethanol + 0.333 kg/s Polluted Dirt + 0.167 kg/s CO2

**Algae Distillery** (`AlgaeDistilleryConfig.cs`):
- 0.6 kg/s Slime -> 0.2 kg/s Algae + 0.4 kg/s Polluted Water

**Compost** (`CompostConfig.cs`):
- 0.1 kg/s Compostables -> 0.1 kg/s Dirt (at 348.15 K / 75 C)

**Fertilizer Synthesizer** (`FertilizerMakerConfig.cs`):
- 0.039 kg/s Polluted Water + 0.065 kg/s Dirt + 0.026 kg/s Phosphorite -> 0.12 kg/s Fertilizer
- Also emits 0.01 kg/s Natural Gas (Methane) via BuildingElementEmitter at 349.15 K

### Specialty Conversions

**Oxylite Refinery** (`OxyliteRefineryConfig.cs`):
- 0.6 kg/s Oxygen + 0.003 kg/s Gold -> 0.6 kg/s Oxylite (OxyRock)

**Milk Fat Separator** (`MilkFatSeparatorConfig.cs`):
- 1 kg/s Milk -> 0.09 kg/s Milk Fat + 0.81 kg/s Brine + 0.1 kg/s CO2

**Campfire** (`CampfireConfig.cs`, DLC2 only):
- 0.025 kg/s Lumber -> 0.004 kg/s CO2 (at 303.15 K)

### Mass Balance Notes

Some conversions do not conserve mass. The Electrolyzer outputs exactly 1 kg for 1 kg input (0.888 + 0.112 = 1.0). But the Algae Deoxidizer consumes 0.55 kg/s Algae and produces only 0.5 kg/s Oxygen -- 0.05 kg/s is destroyed. The Oil Well creates mass: 1 kg Water becomes 3.333 kg Crude Oil. These imbalances are intentional game design.

## Plant and Creature Element Exchange

The `EntityElementExchanger` component (`EntityElementExchanger.cs`) allows plants and creatures to consume one element from the environment and emit another. It consumes mass from the cell via `SimMessages.ConsumeMass` at the `consumeRate`, then emits `emittedElement` at `exchangeRatio` times the consumed mass.

This pauses when the entity wilts (via WiltCondition). The exchange happens every SIM_1000ms. Unlike ElementConverter, this system directly consumes from and emits to the simulation grid, not from internal storage.

## Germ-Element Interactions

Each disease defines growth and death rates per element. Rules are applied in order: base GrowthRule (all elements), then StateGrowthRule (by phase), then ElementGrowthRule (specific elements). Later rules override earlier ones for matching elements.

A positive `populationHalfLife` means germs die (halving time). A negative value means germs grow (doubling time). `float.PositiveInfinity` means the population is stable.

### Food Poisoning (FoodGerms.cs)

**Temperature tolerance:** 248.15-348.15 K (-25 to 75 C), optimal 278.15-313.15 K (5-40 C).

| Element/State | Half-life (s) | Effect |
|---------------|---------------|--------|
| Default (all) | 12,000 | Slow die-off |
| Any solid | 300 | Fast die-off |
| Polluted Dirt (ToxicSand) | infinity | Stable (no growth, no death) |
| Creature | infinity | Stable |
| Bleach Stone | 10 | Extremely fast die-off |
| Any gas | 1,200 | Die-off |
| Polluted Oxygen | 12,000 | Slow die-off (overrides gas rule) |
| Chlorine Gas | 10 | Extremely fast die-off |
| Any liquid | 12,000 | Slow die-off |
| Polluted Water (DirtyWater) | -12,000 | Grows (doubles every 3.3 hours) |
| Edible food (tag) | -12,000 | Grows on food |
| Pickled food (tag) | 10 | Extremely fast die-off |

### Slimelung (SlimeGerms.cs)

**Temperature tolerance:** 283.15-373.15 K (10-100 C), optimal 293.15-363.15 K (20-90 C).

| Element/State | Half-life (s) | Effect |
|---------------|---------------|--------|
| Default (all) | 12,000 | Slow die-off |
| Any solid | 3,000 | Die-off |
| Slime (SlimeMold) | -3,000 | Grows |
| Bleach Stone | 10 | Extremely fast die-off |
| Any gas | 12,000 | Slow die-off |
| Polluted Oxygen | -300 | Fast growth (doubles every 5 min) |
| Clean Oxygen | 1,200 | Die-off |
| Chlorine Gas | 10 | Extremely fast die-off |
| Any liquid | 1,200 | Die-off |

### Zombie Spores (ZombieSpores.cs)

**Temperature tolerance:** 168.15-563.15 K (-105 to 290 C), optimal 258.15-513.15 K (-15 to 240 C). Very wide range.

| Element/State | Half-life (s) | Effect |
|---------------|---------------|--------|
| Default (all) | 12,000 | Slow die-off |
| Any solid | 3,000 | Die-off |
| Carbon, Diamond | infinity | Stable (carbon is a comfortable medium) |
| Bleach Stone | 10 | Extremely fast die-off |
| Any gas | 12,000 | Slow die-off |
| CO2, Methane, Sour Gas | infinity | Stable |
| Chlorine Gas | 10 | Extremely fast die-off |
| Any liquid | 1,200 | Die-off |
| Crude Oil, Petroleum, Naphtha, Liquid Methane | infinity | Stable |
| Liquid Chlorine | 10 | Extremely fast die-off |

### Pollen (PollenGerms.cs)

**Temperature tolerance:** 263.15-373.15 K (-10 to 100 C), optimal 273.15-363.15 K (0-90 C).

**Radiation kill rate:** 0.0 (immune to radiation).

| Element/State | Half-life (s) | Effect |
|---------------|---------------|--------|
| Default (all) | 3,000 | Die-off |
| Any solid | 10 | Extremely fast die-off |
| Any gas | 10 | Extremely fast die-off |
| Clean Oxygen | 200 | Moderate die-off (survives longer than other gases) |
| Any liquid | 10 | Extremely fast die-off |

Pollen dies almost everywhere except in Oxygen gas, where it persists somewhat longer.

### Radiation Sickness (RadiationPoisoning.cs)

**Temperature/pressure:** All idempotent (temperature and pressure have no effect).

| Element/State | Half-life (s) | Effect |
|---------------|---------------|--------|
| All elements | 600 | Universal die-off (10 minutes to halve) |

Radiation germs die everywhere at the same rate. They do not diffuse (diffusionScale = 0).

### Universal Sterilizers

Two elements kill all germ types extremely fast:

**Chlorine Gas (and Liquid Chlorine for Zombie Spores):** 10-second half-life for Food Poisoning, Slimelung, and Zombie Spores. Pollen already dies fast everywhere.

**Bleach Stone:** 10-second half-life for Food Poisoning, Slimelung, and Zombie Spores.

### Germ Exposure Rules

Separate from growth rules, exposure rules determine how germ counts change when a duplicant is exposed. Each disease file also defines `ElementExposureRule` entries:

**Food Poisoning:** Germs are stable on duplicant by default (infinity half-life). Grow in Polluted Water and Polluted Oxygen exposure (-12,000s). Die fast in Chlorine Gas (10s).

**Slimelung:** Germs are stable on duplicant by default. Grow in Polluted Water and Polluted Oxygen (-12,000s). Die slowly in Oxygen (3,000s). Die fast in Chlorine Gas (10s).

**Zombie Spores:** Germs are stable on duplicant by default. Die fast in Chlorine (liquid) and Chlorine Gas (both 10s).

**Pollen:** Germs die on duplicant by default (1,200s). Stable in Oxygen (infinity).

## Sublimation and Off-Gassing Data Source

The specific sublimation pairs (which solid emits which gas) and off-gassing pairs (which liquid emits which gas) are defined in YAML files loaded from `Application.streamingAssetsPath + "/elements/"` (`ElementLoader.cs` line 146). These values are not present in the decompiled C# source.

What the C# source does verify (`ElementLoader.ValidateElements`, line 526):
- Liquids with `sublimateId` use `offGasPercentage`, not `sublimateRate`
- Solids with `sublimateId` use `sublimateRate`, not `offGasPercentage`
- Solid sublimation rate times efficiency must exceed 0.001 to avoid producing gas below the simulation's minimum mass threshold

The sublimation and off-gassing mechanics (rate calculations, overpressure blocking, temperature blocking, sealed containers) are documented in `state-transitions.md`.
