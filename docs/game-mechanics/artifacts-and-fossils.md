# Artifacts and Fossils

Collectible items that provide decor, research payloads, and victory conditions. Artifacts come from space missions and buried pedestals; fossils are part of the Fossil Hunt story trait. Derived from decompiled source code (`ArtifactConfig.cs`, `SpaceArtifact.cs`, `ArtifactTier.cs`, `ArtifactSelector.cs`, `ArtifactFinder.cs`, `ArtifactAnalysisStationWorkable.cs`, `ArtifactPOIConfig.cs`, `ArtifactPOIStates.cs`, `FossilDigSiteConfig.cs`, `FossilHuntInitializer.cs`, `MajorFossilDigSite.cs`, `MinorFossilDigSite.cs`, `FossilBits.cs`, `FossilMine.cs`).

## Artifacts

### Overview

Artifacts are 25 kg loose entities tagged `Artifact`, `PedestalDisplayable`, and `Ornament`. Each artifact has a tier (determining decor and analysis rewards), a type (Space, Terrestrial, or Any), and a "charmed" state that must be removed by analysis before full decor applies.

### Artifact Tiers

Each tier maps to a `DECOR.BONUS` level and a `payloadDropChance` used during analysis. Unanalyzed ("charmed") artifacts always have `DECOR.BONUS.TIER0` (+10, radius 1) regardless of their true tier.

| Artifact Tier | Name | Decor Amount | Decor Radius | Payload Drop Chance | Databanks Dropped |
|---|---|---|---|---|---|
| TIER_NONE | Nothing | 0 | 0 | 0% | 0 |
| TIER0 | Rarity 0 | +10 | 1 | 25% | 5 |
| TIER1 | Rarity 1 | +20 | 3 | 40% | 8 |
| TIER2 | Rarity 2 | +30 | 5 | 55% | 11 |
| TIER3 | Rarity 3 | +35 | 6 | 70% | 14 |
| TIER4 | Rarity 4 | +50 | 7 | 85% | 17 |
| TIER5 | Rarity 5 | +80 | 7 | 100% | 20 |

Databanks dropped = `floor(payloadDropChance * 20)`. The payload drop chance also determines whether a Gene Shuffler Recharge is spawned (one random roll per analysis, checked against the chance).

### Artifact Types

Artifacts belong to one of three categories that control where they can appear:

- **Space**: Only from space artifact POIs (Spaced Out) or space missions (base game). Examples: Obelisk, Moldavite, Saxophone, Amelia's Watch, TeaPot, Robot Arm, Solar System, Moonmoonmoon.
- **Terrestrial**: Only from Gravitas Pedestals found buried in the map. Examples: Sink, Rubik's Cube, Blender, VHS, Brick Phone, Stethoscope, DNA Model.
- **Any**: Can appear in either pool. Examples: Sandstone, Office Mug, Modern Art, Egg Rock, Hatch Fossil, Rock Tornado, Rainbow Egg Rock.

In Spaced Out, the type distinction is enforced: space POIs draw from the Space pool, pedestals draw from the Terrestrial pool, and the Any pool serves as overflow for both. Without Spaced Out, all artifacts are treated as `ArtifactType.Any`.

### Complete Artifact List

| Artifact | Tier | Type | DLC | Special |
|---|---|---|---|---|
| Sandstone | 0 | Any | - | - |
| Sink | 0 | Terrestrial | - | - |
| Rubik's Cube | 0 | Terrestrial | - | - |
| Office Mug | 0 | Any | - | - |
| Obelisk | 1 | Space | - | - |
| Okay X-Ray | 1 | Terrestrial | - | - |
| Blender | 1 | Terrestrial | - | - |
| Moldavite | 1 | Space | - | - |
| VHS | 1 | Terrestrial | - | - |
| Saxophone | 1 | Space | - | - |
| Modern Art | 1 | Any | - | - |
| Honey Jar | 1 | Terrestrial | Spaced Out | - |
| Amelia's Watch | 2 | Space | - | - |
| TeaPot | 2 | Space | - | - |
| Brick Phone | 2 | Terrestrial | - | - |
| Robot Arm | 2 | Space | - | - |
| Shield Generator | 2 | - | - | Looping sound |
| Bioluminescent Rock | 2 | Space | - | Emits light (range 2, cone) |
| Grub Statue | 2 | - | Spaced Out | - |
| Stethoscope | 3 | Terrestrial | - | - |
| Egg Rock | 3 | Any | - | - |
| Hatch Fossil | 3 | Any | - | - |
| Rock Tornado | 3 | Any | - | - |
| Pacu Percolator | 3 | Space | - | - |
| Magma Lamp | 3 | - | - | Emits light (range 2, cone) |
| Oracle | 3 | Terrestrial | Spaced Out | - |
| DNA Model | 4 | Terrestrial | - | - |
| Rainbow Egg Rock | 4 | Any | - | - |
| Plasma Lamp | 4 | - | - | Emits light (range 2, circle), looping sound |
| Mood Ring | 4 | - | Spaced Out | - |
| Solar System | 5 | Space | - | Looping sound |
| Moonmoonmoon | 5 | Space | - | - |
| Reactor Model | 5 | - | Spaced Out | - |

### Charmed State (Spaced Out)

When Spaced Out is active, newly spawned artifacts start with the `CharmedArtifact` tag. While charmed:
- Decor is forced to `BONUS.TIER0` (+10, radius 1) instead of the artifact's true tier
- The artifact displays the `ArtifactEntombed` status item
- The artifact uses an "entombed" animation variant
- The artifact's info panel shows its payload drop chance

Analysis at the Artifact Analysis Station removes the charm, unlocking full decor and triggering payload rewards. Without Spaced Out, artifacts spawn already analyzed.

### Artifact Selection (Uniqueness)

The `ArtifactSelector` singleton tracks which artifacts have been placed to avoid duplicates:
1. When an artifact is needed, `GetUniqueArtifactID(type)` picks randomly from the unplaced artifacts of that type
2. If the typed pool is exhausted, it falls back to the `Any` pool
3. If all pools are exhausted, it defaults to `artifact_officemug`
4. Each placed artifact ID is recorded per-type to prevent re-selection

## Where Artifacts Come From

### Gravitas Pedestals (Terrestrial)

`GravitasPedestalConfig` defines a building (Spaced Out only) with a `PedestalArtifactSpawner`. On spawn, it automatically creates one Terrestrial artifact via `ArtifactSelector.GetUniqueArtifactID(Terrestrial)`, deposits it, and tags it `TerrestrialArtifact`. These pedestals are pre-placed in ruins during world generation.

### Artifact POIs (Spaced Out)

Artifact POIs are cluster map entities that produce Space-type artifacts. Defined in `ArtifactPOIConfig`:

| POI | ID | Guaranteed First Artifact | Destroys After Harvest | Recharge (s) | Databanks |
|---|---|---|---|---|---|
| Gravitas Station 1 | GravitasSpaceStation1 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 2 | GravitasSpaceStation2 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 3 | GravitasSpaceStation3 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 4 | GravitasSpaceStation4 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 5 | GravitasSpaceStation5 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 6 | GravitasSpaceStation6 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 7 | GravitasSpaceStation7 | None (random) | No | 30,000-60,000 | 50 |
| Gravitas Station 8 | GravitasSpaceStation8 | None (random) | No | 30,000-60,000 | 50 |
| Russell's Teapot | RussellsTeapot | TeaPot | Yes | 30,000-60,000 | 0 |

**Artifact POI state machine** (`ArtifactPOIStates`):
1. POI starts fully charged (charge = 1.0)
2. Spawns an artifact into the hex cell inventory
3. Waits for the artifact to be picked up by a rocket's Artifact Cargo Bay
4. Recharges at a rate of 600s per game day (one `NewDay` event = 600s of charge)
5. Once fully recharged, spawns another artifact

The recharge time is randomized per-instance between the min/max values, seeded from world seed + position. Russell's Teapot always gives the TeaPot artifact first, then self-destructs.

Harvestable POIs (resource asteroids) with `canProvideArtifacts = true` also produce artifacts through the same hex cell inventory mechanism.

### Artifact Cargo Bay (Spaced Out)

`ArtifactCargoBayConfig` defines a 3x1 rocket module that holds one artifact. The `ArtifactHarvestModule` state machine automatically picks up artifacts from the hex cell inventory when the rocket is parked at a POI hex. It checks every 4000ms (`SIM_4000ms`) while in the harvesting state.

### Base Game Space Missions

In the base game, `ArtifactFinder` is attached to the `CommandModule`. When the rocket lands after a mission:
1. For each stored duplicant, `SearchForArtifact` is called
2. The destination's `artifactDropTable` (an `ArtifactDropRate`) determines the tier via weighted random roll
3. If the tier is `TIER_NONE`, no artifact is found
4. Otherwise, a random artifact of that tier is instantiated at the landing position

**Archaeologist trait**: Duplicants with the `Archaeologist` trait skip the `TIER_NONE` weight entirely, guaranteeing they always find an artifact. The remaining tier weights are unchanged.

### Base Game Destination Artifact Drop Rates

Each `SpaceDestinationType` has an `ArtifactDropRate` that determines artifact quality. Drop rates are weighted random tables:

| Drop Rate | TIER_NONE | TIER0 | TIER1 | TIER2 | TIER3 | TIER4 | TIER5 |
|---|---|---|---|---|---|---|---|
| None | 1.0 | - | - | - | - | - | - |
| Bad | 10 | 5 | 3 | 2 | - | - | - |
| Mediocre | 10 | - | 5 | 3 | 2 | - | - |
| Good | 10 | - | - | 5 | 3 | 2 | - |
| Great | 10 | - | - | - | 5 | 3 | 2 |
| Amazing | 10 | - | - | - | 3 | 5 | 2 |
| Perfect | 10 | - | - | - | - | 6 | 4 |

Values are weights, not percentages. To get probabilities, divide by total weight. For example, "Bad" has total weight 20, so TIER_NONE = 50%, TIER0 = 25%, TIER1 = 15%, TIER2 = 10%.

| Destination | Drop Rate |
|---|---|
| Satellite | Bad |
| Metallic Asteroid | Mediocre |
| Rocky Asteroid | Good |
| Carbonaceous Asteroid | Mediocre |
| Icy Dwarf | Great |
| Organic Dwarf | Great |
| Dusty Moon | Amazing |
| Terra Planet | Amazing |
| Volcano Planet | Amazing |
| Gas Giant | Perfect |
| Ice Giant | Perfect |
| Salt Dwarf | Bad |
| Rust Planet | Perfect |
| Forest Planet | Mediocre |
| Red Dwarf | Amazing |
| Gold Asteroid | Bad |
| Hydrogen Giant | Mediocre |
| Oily Asteroid | Mediocre |
| Shiny Planet | Good |
| Chlorine Planet | Bad |
| Salt Desert Planet | Bad |
| Wormhole | Perfect |
| Earth | None |

## Artifact Analysis

### Artifact Analysis Station

`ArtifactAnalysisStationConfig` defines a 4x4 building (Spaced Out only) that removes the charmed state from artifacts:

| Property | Value |
|---|---|
| Size | 4 wide x 4 tall |
| Power consumption | 480 W |
| Self-heat | 1 kW |
| Construction mass | TIER5 (800 kg) |
| Materials | All metals |
| Work time | 150 s (base) |
| Required skill | Arting2 (`CanStudyArtifact` perk) |
| Attribute converter | Art Speed |
| Skill experience group | Research |
| Input | One charmed artifact (25 kg) via `ManualDeliveryKG` |

### Analysis Process

When a charmed artifact is delivered to the station and a qualified duplicant operates it:
1. The duplicant works for 150s (modified by Art Speed attribute)
2. On completion, `ConsumeCharm()` runs:
   - `YieldPayload()` spawns rewards based on tier
   - `RemoveCharm()` strips the `CharmedArtifact` tag, restoring full-tier decor
3. The artifact is dropped from storage as an analyzed item

### Analysis Rewards

Each analysis yields two types of rewards:

**Gene Shuffler Recharge**: A random roll (0 to 1) is compared against the tier's `payloadDropChance`. If the roll is at or below the chance, one Gene Shuffler Recharge (`GeneShufflerRecharge`) is spawned. The roll is pre-computed (saved between sessions) and re-rolled after each analysis.

**Orbital Research Databanks**: `floor(payloadDropChance * 20)` databanks are spawned. This is deterministic per tier:

| Tier | Databanks | Gene Shuffler Chance |
|---|---|---|
| TIER0 | 5 | 25% |
| TIER1 | 8 | 40% |
| TIER2 | 11 | 55% |
| TIER3 | 14 | 70% |
| TIER4 | 17 | 85% |
| TIER5 | 20 | 100% |

### Analysis Side Screen

The `ArtifactAnalysisSideScreen` displays a log of all previously analyzed artifacts. Clicking an entry opens an `ArtifactReveal` gameplay event popup showing the artifact's name and lore description (from `STRINGS.UI.SPACEARTIFACTS.<ID>.ARTIFACT`).

## Victory Condition

Analyzing artifacts contributes to the "Completion of Study" colony achievement (`CollectedArtifacts`):
- Requires 10 unique analyzed terrestrial artifacts (`AnalyzedArtifactCount >= 10`)
- A parallel achievement requires 10 unique analyzed space artifacts (`AnalyzedSpaceArtifactCount >= 10`, tracked by `CollectedSpaceArtifacts`)
- Completing the terrestrial artifact achievement triggers the `ArtifactSequence` victory cinematic

The victory cinematic pans the camera to up to 3 analyzed, stored artifacts spread across different worlds, plays `Music_Victory_02_NIS`, then shows the colony achievement video.

## Fossils (Story Trait: Fossil Hunt)

Fossils are part of the "Fossil Hunt" story trait. The system involves discovering and excavating dig sites scattered across the map, completing a quest, and unlocking a fossil-processing fabricator.

### Story Trait Structure

The Fossil Hunt story trait uses:
- **One major dig site** (`FossilDigSiteConfig`, building ID `FossilDig`): A 5x3 building made of Fossil element. This is the main quest hub and becomes a fabricator when the quest completes.
- **Three minor dig sites** (`FossilSiteConfig_Rock`, `FossilSiteConfig_Resin`, `FossilSiteConfig_Ice`): 2x2 placed entities made of Fossil element, each representing a different specimen type.
- **Fossil bits** (`FossilBitsSmallConfig`, `FossilBitsLargeConfig`): Decorative fossil debris (1x2 at 1500 kg, 2x2 at 2000 kg) that can be excavated for raw Fossil material.

### Discovery and Excavation

When the major dig site is first selected:
1. A popup (`BEGIN_POPUP`) introduces the Fossil Hunt story
2. The story state advances to `IN_PROGRESS`
3. All major dig sites are revealed on the map (radius 8 tiles of fog-of-war cleared)
4. All minor dig sites are revealed (radius 3 tiles cleared)

Each dig site starts in a "covered" state. Players mark sites for excavation via the Excavate button. Excavation is a work chore:

| Workable | Work Time | Required Skill | Chore Type |
|---|---|---|---|
| Major Dig Site | 90 s | `CanArtGreat` (Master Artist) | ExcavateFossil |
| Minor Dig Site | 90 s | `CanArtGreat` (Master Artist) | ExcavateFossil |
| Fossil Bits | 30 s | `CanArtGreat` (Master Artist) | ExcavateFossil |

All fossil excavation workables use the Art Speed attribute converter and grant art skill experience. Light efficiency bonus applies (work proceeds faster in lit areas).

### Quest Progression

The quest tracks 4 discovered dig sites (`FossilDigSiteConfig.DiscoveredDigsitesRequired = 4`):
- Each minor site has a unique quest criteria ID: `LostRockFossil`, `LostResinFossil`, `LostIceFossil`
- The major site has criteria `LostSpecimen`

When a site is excavated (work completed):
1. The site transitions to "revealed" state
2. Minor sites drop their Fossil mass as loose material (in 400 kg chunks)
3. Quest progress is updated
4. A lore notification fires (the first 3 minor sites each unlock a lore entry via `story_trait_fossilhunt_poi{N}`)

When all 4 sites are excavated, the quest completes:
1. The `CompleteStorySignal` fires on the major dig site
2. The major dig site unlocks its `FossilMine` fabricator
3. The story trait completes, spawning a Fossil Hunt keepsake
4. The major dig site becomes demolishable and gets standard building controls

### Minor Dig Site Types

| Type | ID | Mass | Temperature | Decor | Quest Criteria |
|---|---|---|---|---|---|
| Rock Fossil | FossilRock | 4,000 kg | 315 K (42 C) | +30, radius 5 | LostRockFossil |
| Resin Fossil | FossilResin | 4,000 kg | 315 K (42 C) | +30, radius 5 | LostResinFossil |
| Ice Fossil | FossilIce | 4,000 kg | 230 K (-43 C) | +30, radius 5 | LostIceFossil |

Minor dig sites have a decor penalty while unexcavated (a -1 multiplier modifier on their decor attribute). After excavation, the penalty is removed and they display at their full decor value.

### Fossil Mine (Fabricator)

Once the quest completes, the major dig site's `FossilMine` activates. It functions as a `ComplexFabricator` with one recipe:

| Input | Output | Work Time |
|---|---|---|
| 1 kg Diamond | 100 kg Fossil | 80 s |

The fabricator requires `CanArtGreat` (Master Artist) skill perk and uses Art Speed for work speed. It uses the Art chore type and grants art skill experience.

### Fossil Bits

Fossil bits are smaller debris entities near dig sites. They can be individually marked for excavation via their side screen button. When excavated:
- The work takes 30 s
- The entity drops its Fossil mass as raw material (in 400 kg chunks)
- The entity is destroyed

| Type | Size | Mass |
|---|---|---|
| Small | 1x2 | 1,500 kg |
| Large | 2x2 | 2,000 kg |

### Fossil Material Processing

The Rock Crusher can process raw Fossil into useful materials:
- **Input**: 100 kg Fossil
- **Output**: 5 kg Lime + 95 kg Sedimentary Rock
- **Work time**: 40 s

### Fossil Sculptures (DLC4)

Two building types use Fossil as construction material (require DLC4 "Bionic Booster Pack"):

| Building | ID | Size | Placement | Decor | Construction | Required Skill |
|---|---|---|---|---|---|---|
| Fossil Sculpture | FossilSculpture | 3x3 | Floor | +35, radius 6 | TIER4 (200 kg) Fossil | `CanArtGreat` |
| Ceiling Fossil Sculpture | CeilingFossilSculpture | 3x2 | Ceiling | +35, radius 6 | TIER4 (200 kg) Fossil | `CanArtGreat` |

Both are artable buildings (can be worked by a Master Artist to improve their appearance). The floor version is a `Sculpture`; the ceiling version is a `LongRangeSculpture`.

## Display

### Pedestal Display

Artifacts (and keepsakes) can be placed on pedestals for display. All artifacts have the `PedestalDisplayable` tag. When displayed, the artifact's decor provider contributes to the room's decor score. The Gravitas Pedestal (`GravitasPedestalConfig`, Spaced Out only) is a 1x2 building that accepts one `PedestalDisplayable` item.

### Artifact Cargo Bay Display

The Artifact Cargo Bay (`ArtifactCargoBayConfig`) also functions as a pedestal display. It accepts one artifact, showing it visually on the module. The `ArtifactModule` component tracks the module's position each tick to keep the displayed artifact aligned. Capacity: 1 artifact.
