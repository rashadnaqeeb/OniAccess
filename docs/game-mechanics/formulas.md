# Formula Reference

Quick-reference for key simulation formulas. Each formula links to its detailed explanation in the relevant topic file.

## Heat Transfer (thermal-systems.md)

| Formula | Purpose |
|---------|---------|
| `EnergyFlow = (T1 - T2) * min(k1, k2) * (A / d)` | Core heat transfer between cells |
| `deltaT = kJ / (SHC * mass)` | Temperature change from energy input |
| `FinalTemp = (m1*T1 + m2*T2) / (m1 + m2)` | Standard mixing (SimUtil, clamped to [min,max]) |
| `FinalTemp = sum(m*T*SHC) / sum(m*SHC)` | SHC-weighted mixing (ElementConverter only) |
| `HeatCapacity = mass * SHC` | Thermal mass of a material |
| `ExhaustKJ = (kW * dt / cells) * (min(mass, 1.5) / 1.5)` | Building exhaust heat distribution |
| `CellWeight = 1 + (maxDist - abs(dx) - abs(dy))` | Area heater weighting |
| `DeltaT_clamped = clamp(T + deltaT, T, Ttarget)` | Clamped heating/cooling (no overshoot) |

## Disease (disease-and-germs.md)

| Formula | Purpose |
|---------|---------|
| `ContractionChance = 0.5 - 0.5 * tanh(0.25 * rating)` | Disease contraction probability |
| `Resistance = base + attribute + tier_bonus` | Total resistance rating |
| `GermTransfer = totalGerms * (massFraction)` | Germs transferred with mass |
| `DiseaseWinner = argmax(strength * count)` | Disease mixing competition |

## Radiation (radiation.md)

| Formula | Purpose |
|---------|---------|
| Constructed: `absorption = radiationAbsorptionFactor * 0.8` | Constructed tile radiation shielding |
| Natural: `absorption = factor * 0.3 + (mass/2000) * factor * 0.7` | Natural material radiation shielding |
| `Exposure = cellRad * (1 - resistance) / 600 * dt` | Duplicant radiation exposure per frame |
| `HEPCharge = (cellRad / 600) * sampleRate * 0.1` | HEP spawner absorption rate |

## Power (electrical-power.md)

| Formula | Purpose |
|---------|---------|
| `Joules = Watts * dt` | Energy per tick |
| `OverloadThreshold = maxWattage + 0.5` | Wire overload detection |
| `OverloadDamageTime = 6 seconds` | Time to first wire damage |
| `ConsumerLockout = 6 seconds` | Brownout recovery delay |

## Pressure (fluid-physics.md)

| Formula | Purpose |
|---------|---------|
| `Pressure = mass * element.defaultPressure` | Mass to pressure conversion |
| `Mass = pressure / element.defaultPressure` | Pressure to mass conversion |
| `EarDamageThreshold = 4 kg gas mass` | Popped Ear Drums condition |

## Pipe Flow (piping-and-conduits.md)

| Formula | Purpose |
|---------|---------|
| `FlowRate = min(availableMass, targetCapacity)` | Per-tick mass transfer |
| `MaxLiquid = 10 kg per cell` | Liquid pipe capacity |
| `MaxGas = 1 kg per cell` | Gas pipe capacity |
| `MixTemp = (m1*T1 + m2*T2) / (m1 + m2)` | Pipe content temperature mixing |

## Light, Decor, and Noise (environment-quality.md)

| Formula | Purpose |
|---------|---------|
| `Lux = intensity / max(1, round(falloffRate * max(dist, 1)))` | Light intensity at distance |
| `LitDecorBonus = +15 if LightIntensity > 0` | Automatic decor bonus from lighting |
| `NoiseFalloff = sourceDB - (sourceDB * dist * 0.05)` | Noise decay with distance |

## Element Conversion (building-mechanics.md)

| Formula | Purpose |
|---------|---------|
| `OutputMass = rate * multiplier * speed * dt * fraction` | Converter output calculation |
| `OutputTemp = sum(m*T*SHC) / sum(m*SHC)` | Weighted input temperature for outputs |
| `DiseaseShare = weight / totalWeight * inputCount` | Disease distribution across outputs |

## Falling Physics (fluid-physics.md)

| Formula | Purpose |
|---------|---------|
| `ParticleScale = lerp(0.25, 1.0, clamp01(mass / 75))` | Liquid particle visual size |
| `LiquidDragVariance = (instanceID % 1000) / 1000 * 0.25` | Per-object drag randomization |
| `MaxVelInLiquid = base * (1.0 + variance)` | Terminal velocity in liquid |
| `LandingEpsilon = 0.07 units` | Ground detection buffer |
| `MaxRenderParticles = 16249` | Unity vertex limit / 4 |

## Navigation (navigation.md)

| Formula | Purpose |
|---------|---------|
| `SubmergedCost = baseCost * 2` | Penalty for unprotected underwater travel |
| `TubeSpeed = 18 * (1 + waxBonus)` | Transit tube velocity (waxBonus = 0.25) |
| `BuildingPressure = prev * 0.7 + current * 0.3` | Pressure reading momentum blend |
| `JetFuelRate = 0.2 kg/s` | Jet suit fuel consumption |

## Creature Metabolism (creature-ai.md)

| Formula | Purpose |
|---------|---------|
| `Amount = prev + (baseRate * (1 + sumMultipliers) + sumAdditives) * dt` | Generic amount update |
| `OutputMass = consumedMass * conversionRate` | Diet excretion mass |
| `WildnessDecay = groomingReduction per interaction` | Taming progress |

## Duplicant Health (duplicant-health.md)

| Formula | Purpose |
|---------|---------|
| `MaxHP = 100` | Base duplicant hit points |
| `ScaldingThreshold = 345 K (71.85 C)` | Temperature contact damage (hot) |
| `ScoldingThreshold = 183 K (-90.15 C)` | Temperature contact damage (cold) |
| `IncapacitationBleedout = 600 seconds` | Time before incapacitated dupe dies |

## Geysers (geysers-and-vents.md)

| Formula | Purpose |
|---------|---------|
| `OnDuration = iterationLength * iterationPercent` | Eruption duration per cycle |
| `OffDuration = iterationLength * (1 - iterationPercent)` | Rest duration per cycle |
| `PreErupt = offDuration * 0.1` | Shaking phase duration |
| `PostErupt = offDuration * 0.05` | Cooldown phase duration |

## Colony (colony-progression.md)

| Formula | Purpose |
|---------|---------|
| `CycleLength = 600 seconds` | One full day/night cycle |
| `NightStart = 87.5% of cycle (525s)` | Night boundary |
| `ScheduleBlock = 25 seconds` | Duration per schedule block (600/24) |
| `ImmigrationTimeout = 120 seconds` | Auto-reject if player ignores printing pod |

## Stress and Morale (stress-and-morale.md)

| Formula | Purpose |
|---------|---------|
| `MoraleRequirement = sum(skill_morale_costs)` | Morale needed based on learned skills |
| `MoraleBalance = totalMorale - moraleRequirement` | Positive = stress reduction, negative = stress gain |
