# Medicine

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## Dev Life Support

**ID:** `DevLifeSupport`
**Name:** Dev Life Support
**Description:** Keeps Duplicants cozy and breathing.
**Effect:** Generates warm, oxygen-rich air.
**Debug only:** Yes (not available in normal gameplay)

| Property | Value |
|---|---|
| Size | 1 x 1 |
| HP | 30 |
| Construction materials | Raw Minerals, 800 kg |
| Construction time | 30 s |
| Melting point | 800 K (526.85 C) |
| Decor | -20 (radius 4) |
| Noise | None |
| Floodable | No |
| Overheatable | No |

**Mechanics:**
- Emits Oxygen at 50 kg/s at 303.15 K (30 C), max pressure 1.5 kg/tile
- Passively consumes Carbon Dioxide at 50 kg/s, radius 10 tiles
- Storage capacity: 200 kg

---

## Wash Basin

**ID:** `WashBasin`
**Name:** Wash Basin
**Description:** Germ spread can be reduced by building these where Duplicants often get dirty.
**Effect:** Removes some Germs from Duplicants. Germ-covered Duplicants use Wash Basins when passing by in the selected direction.
**Research:** None (available from start)

| Property | Value |
|---|---|
| Size | 2 x 3 |
| HP | 30 |
| Construction materials | Raw Minerals or Metal Ore, 50 kg |
| Construction time | 30 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | +10 (radius 2) |
| Noise | Noisy Tier 0 |

**Mechanics:**
- Consumes Water, outputs Dirty Water
- Water per use: 5 kg
- Max uses per flush: 40
- Storage capacity: 200 kg (refill threshold: 40 kg, minimum delivery: 5 kg)
- Disease removal per use: 120,000 germs (DISEASE_PER_PEE [100,000] + 20,000)
- Work time: 5 s
- Manual delivery (no plumbing)
- Dumps when full
- Directional (pass-through)
- Tags: WashStation
- Delivered via FetchCritical chore

---

## Sink

**ID:** `WashSink`
**Name:** Sink
**Description:** Sinks are plumbed and do not need to be manually emptied or refilled.
**Effect:** Removes Germs from Duplicants. Germ-covered Duplicants use Sinks when passing by in the selected direction.
**Research:** Sanitation (SanitationSciences)

| Property | Value |
|---|---|
| Size | 2 x 3 |
| HP | 30 |
| Construction materials | Metal Ore, 400 kg |
| Construction time | 30 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | +10 (radius 2) |
| Noise | Noisy Tier 0 |

**Mechanics:**
- Consumes Water (liquid input), outputs Dirty Water (liquid output)
- Water per use: 5 kg
- Max uses per flush: 2
- Liquid conduit input at offset (0, 0), output at offset (1, 1)
- Conduit consumer capacity: 10 kg (accepts only Water; wrong element stored)
- Conduit dispenser: outputs everything except Water
- Disease removal per use: 120,000 germs (same as Wash Basin)
- Work time: 5 s
- Storage does not transfer disease
- Directional (pass-through)
- Tags: WashStation, AdvancedWashStation
- Ignores full output pipe
- Rocket usage restriction applies

---

## Hand Sanitizer

**ID:** `HandSanitizer`
**Name:** Hand Sanitizer
**Description:** Hand sanitizers kill germs more effectively than wash basins.
**Effect:** Removes most Germs from Duplicants. Germ-covered Duplicants use Hand Sanitizers when passing by in the selected direction.
**Research:** Medical Equipment (MedicineII)

| Property | Value |
|---|---|
| Size | 1 x 3 |
| HP | 30 |
| Construction materials | Metal, 100 kg |
| Construction time | 30 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | +10 (radius 2) |
| Noise | None |

**Mechanics:**
- Consumes Bleach Stone
- Mass per use: 0.07 kg
- Storage capacity: 15 kg (refill threshold: 3 kg)
- Disease removal per use: 480,000 germs (4x Wash Basin's 120,000)
- Work time: 1.8 s
- Manual delivery (no plumbing), no meters displayed
- Directional (pass-through)
- Tags: WashStation, AdvancedWashStation
- Delivered via FetchCritical chore (requires Functional state)
- Rocket usage restriction applies

---

## Decontamination Shower

**ID:** `DecontaminationShower`
**Name:** Decontamination Shower
**Description:** Don't forget to decontaminate behind your ears.
**Effect:** Uses Water to remove Germs and Radiation. Decontaminates both Duplicants and their Clothing.
**Research:** Advanced Sanitation
**DLC:** Spaced Out! (EXPANSION1)

| Property | Value |
|---|---|
| Size | 2 x 4 |
| HP | 250 |
| Construction materials | Metal + Lead, 800 kg + 100 kg |
| Construction time | 120 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | -20 (radius 4) |
| Noise | Noisy Tier 0 |

**Mechanics:**
- Consumes Water (liquid input), drops Dirty Water on ground at offset (1, 0)
- Water per use: 100 kg
- Max uses per flush: 1
- Liquid conduit input at offset (1, 2)
- Conduit consumer capacity: 100 kg (accepts only Water; wrong element stored)
- Disease removal per use: 1,000,000 germs
- Work time: 15 s
- Can sanitize suit: Yes
- Can sanitize storage: Yes
- Removes irritation: Yes
- Directional (pass-through)
- Material category: RADIATION_CONTAINMENT (Metal + Lead)

---

## Lubrication Station (Oil Changer)

**ID:** `OilChanger`
**Name:** Lubrication Station
**Description:** A fresh supply of oil keeps the ol' joints from getting too creaky.
**Effect:** Uses Gear Oil to keep Duplicants' bionic parts running smoothly.
**Research:** Filtration (AdvancedFiltration)
**DLC:** DLC3 (Bionic Booster Pack)

| Property | Value |
|---|---|
| Size | 3 x 3 |
| HP | 30 |
| Construction materials | Metal Ore, 400 kg |
| Construction time | 60 s |
| Melting point | 800 K (526.85 C) |
| Decor | -15 (radius 3) |
| Noise | None |
| Power | 120 W |
| Exhaust heat | 0.25 kDTU/s |
| Self heat | 0 kDTU/s |
| Overheatable | No |

**Mechanics:**
- Liquid conduit input at offset (1, 2), power input at offset (1, 0)
- Accepts Lubricating Oil only (wrong element dumped)
- Storage capacity: 400 kg
- Resets progress on stop
- Unrotatable
- Tags: BionicUpkeepType, BionicBuilding

---

## Apothecary

**ID:** `Apothecary`
**Name:** Apothecary
**Description:** Some medications help prevent diseases, while others aim to alleviate existing illness.
**Effect:** Produces Medicine to cure most basic Diseases. Duplicants must possess the Medicine Compounding Skill to fabricate medicines. Duplicants will not fabricate items unless recipes are queued.
**Research:** Pharmacology (MedicineI)

| Property | Value |
|---|---|
| Size | 2 x 3 |
| HP | 30 |
| Construction materials | All Metals, 400 kg |
| Construction time | 120 s |
| Melting point | 800 K (526.85 C) |
| Decor | None (0) |
| Noise | None |
| Power | 0 W (no power required) |
| Exhaust heat | 0.125 kDTU/s |
| Self heat | 0.5 kDTU/s |

**Mechanics:**
- Manually operated complex fabricator
- Required skill perk: CanCompound (Medicine Compounding)
- Has logic input port (automation)
- Uses insulated storage for ingredients

### Apothecary Recipes

#### Vitamin Chews (BasicBooster)
- **Type:** Booster (daily supplement)
- **Effect:** Medicine_BasicBooster (minor germ resistance)
- **Ingredients:** 1 kg Carbon
- **Craft time:** 50 s
- **Station required for use:** None (self-administered)
- **Sort order:** 1

#### Immuno Booster (IntermediateBooster)
- **Type:** Booster (daily supplement)
- **Effect:** Medicine_IntermediateBooster (significant germ resistance)
- **Ingredients:** 1 kg Pincha Peppernut (SpiceNut)
- **Craft time:** 100 s
- **Station required for use:** None (self-administered)
- **Sort order:** 5

#### Curative Tablet (BasicCure)
- **Type:** Cure (specific)
- **Cures:** Food Poisoning (FoodSickness)
- **Ingredients:** 1 kg Carbon + 1 kg Water
- **Craft time:** 50 s
- **Station required for use:** None (self-administered)
- **Sort order:** 10

#### Allergy Medication (Antihistamine)
- **Type:** Cure (specific)
- **Cures:** Allergies, Dupe Mosquito Bite
- **Effect:** HistamineSuppression
- **Ingredients:** 1 kg Bristle Berry Seed (PrickleFlowerSeed) OR 10 kg Waterweed (Kelp), + 1 kg Dirt
- **Output:** 10 doses
- **Craft time:** 100 s
- **Station required for use:** None (self-administered)
- **Sort order:** 10

#### Medical Pack (IntermediateCure)
- **Type:** Cure (specific)
- **Cures:** Slimelung (SlimeSickness)
- **Ingredients:** 1 kg Balm Lily Flower (SwampLilyFlower) + 1 kg Phosphorite
- **Craft time:** 100 s
- **Station required for use:** Sick Bay (DoctorStation)
- **Required tech for recipe:** MedicineII
- **Sort order:** 10

#### Basic Rad Pill (BasicRadPill)
- **Type:** Booster (daily supplement)
- **Effect:** Medicine_BasicRadPill (radiation absorption)
- **Ingredients:** 1 kg Carbon
- **Craft time:** 50 s
- **Station required for use:** None (self-administered)
- **DLC:** Spaced Out! (EXPANSION1)
- **Sort order:** 10

#### Serum Vial (AdvancedCure)
- **Type:** Cure (specific)
- **Cures:** Zombie Spores (ZombieSickness)
- **Ingredients:** 1 kg Steel + 1 Shine Bug Orange Egg (LightBugOrangeEgg)
- **Craft time:** 200 s
- **Station required for use:** Disease Clinic (AdvancedDoctorStation)
- **Required tech for recipe:** MedicineIV
- **Sort order:** 20

#### Gear Balm (LubricationStick)
- **Type:** Medical supply (solid lubricant)
- **Ingredients:** 80 kg Liquid Gunk + 200 kg Water
- **Output:** 1 Gear Balm (80 kg) + 200 kg Dirty Water
- **Craft time:** 100 s
- **Station required for use:** None (self-administered)
- **Required tech for recipe:** (linked to TechItems.lubricationStick.parentTechId)
- **DLC:** DLC3 (Bionic Booster Pack)
- **Sort order:** 1

#### Tallow Gear Balm (TallowLubricationStick)
- **Type:** Medical supply (solid lubricant)
- **Ingredients:** 10 kg Tallow + 70 kg Water
- **Output:** 1 Tallow Gear Balm (80 kg)
- **Craft time:** 100 s
- **Station required for use:** None (self-administered)
- **Required tech for recipe:** (linked to TechItems.lubricationStick.parentTechId)
- **DLC:** DLC3 (Bionic Booster Pack)
- **Sort order:** 1

---

## Sick Bay

**ID:** `DoctorStation`
**Name:** Sick Bay
**Description:** Sick bays can be placed in hospital rooms to decrease the likelihood of disease spreading.
**Effect:** Allows Duplicants to administer basic treatments to sick Duplicants. Duplicants must possess the Bedside Manner Skill to treat peers.
**Research:** Medical Equipment (MedicineII)

| Property | Value |
|---|---|
| Size | 3 x 2 |
| HP | 10 |
| Construction materials | Raw Minerals, 200 kg |
| Construction time | 10 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | None (0) |
| Noise | None |
| Overheatable | No |

**Mechanics:**
- Required skill perk to build/use: CanDoctor (Bedside Manner)
- Medicine storage capacity: 10 kg (refill at 5 kg, minimum mass 1 kg)
- Accepts medicines tagged for DoctorStation (e.g., Medical Pack)
- Delivered via DoctorFetch chore (requires Functional state)
- Doctor work time: 40 s
- Doctor required skill: CanDoctor
- Tags: Clinic
- Recommended room: Hospital
- Shows status item when not in Hospital room

---

## Disease Clinic

**ID:** `AdvancedDoctorStation`
**Name:** Disease Clinic
**Description:** Disease clinics require power, but treat more serious illnesses than sick bays alone.
**Effect:** Allows Duplicants to administer powerful treatments to sick Duplicants. Duplicants must possess the Advanced Medical Care Skill to treat peers.
**Research:** Micro-Targeted Medicine (MedicineIV)

| Property | Value |
|---|---|
| Size | 2 x 3 |
| HP | 100 |
| Construction materials | Refined Metal, 200 kg |
| Construction time | 10 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | None (0) |
| Noise | None |
| Power | 480 W |
| Exhaust heat | 0.25 kDTU/s |
| Self heat | 0.5 kDTU/s |

**Mechanics:**
- Required skill perk to build/use: CanAdvancedMedicine (Advanced Medical Care)
- Medicine storage capacity: 10 kg (refill at 5 kg, minimum mass 1 kg)
- Accepts medicines tagged for AdvancedDoctorStation (e.g., Serum Vial, Intermediate Rad Pill)
- Delivered via DoctorFetch chore (requires Functional state)
- Doctor work time: 60 s
- Doctor required skill: CanAdvancedMedicine
- Tags: Clinic
- Recommended room: Hospital
- Shows status item when not in Hospital room

---

## Triage Cot

**ID:** `MedicalCot`
**Name:** Triage Cot
**Description:** Duplicants use triage cots to recover from physical injuries and receive aid from peers.
**Effect:** Accelerates Health restoration and the healing of physical injuries. Revives incapacitated Duplicants.
**Research:** None (available from start)

| Property | Value |
|---|---|
| Size | 3 x 2 |
| HP | 10 |
| Construction materials | Raw Minerals, 200 kg |
| Construction time | 10 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | None (0) |
| Noise | None |
| Overheatable | No |

**Mechanics:**
- Functions as a sleepable (not a normal bed)
- Doctor visit interval: 300 s (5 minutes)
- Health effect (undoctored): "MedicalCot"
- Health effect (doctored): "MedicalCotDoctored"
- Disease effect (undoctored): "MedicalCot"
- Disease effect (doctored): "MedicalCotDoctored"
- Doctored placeholder effect: "DoctoredOffCotEffect"
- Doctor work time: 45 s
- Assignable via Clinic slot
- Tags: Clinic, BedType
- Recommended room: Hospital
- Shows status item when not in Hospital room
- Recuperation disease multiplier: 1.1x (undoctored), 1.2x (doctored) — from MEDICINE tuning

---

## Massage Table

**ID:** `MassageTable`
**Name:** Massage Table
**Description:** Massage tables quickly reduce extreme stress, at the cost of power production.
**Effect:** Rapidly reduces Stress for the Duplicant user. Duplicants will automatically seek a massage table when Stress exceeds breaktime range.
**Research:** None (available from start)

| Property | Value |
|---|---|
| Size | 2 x 2 |
| HP | 10 |
| Construction materials | Raw Minerals, 200 kg |
| Construction time | 10 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | None (0) |
| Noise | Noisy Tier 0 |
| Power | 240 W |
| Exhaust heat | 0.125 kDTU/s |
| Self heat | 0.5 kDTU/s |
| Overheatable | Yes |

**Mechanics:**
- Stress modification (normal): -30%
- Stress modification (in correct room): -60%
- Recommended room: Massage Clinic
- Assignable via MassageTable slot (can be public)
- Tags: DeStressingBuilding

---

## Tasteful Memorial (Grave)

**ID:** `Grave`
**Name:** Tasteful Memorial
**Description:** Burying dead Duplicants reduces health hazards and stress on the colony.
**Effect:** Provides a final resting place for deceased Duplicants. Living Duplicants will automatically place an unburied corpse inside.
**Research:** None (available from start)

| Property | Value |
|---|---|
| Size | 1 x 2 |
| HP | 30 |
| Construction materials | Raw Minerals, 800 kg |
| Construction time | 120 s |
| Melting point | 1600 K (1326.85 C) |
| Decor | +10 (radius 2) |
| Noise | None |
| Overheatable | No |
| Floodable | No |

**Mechanics:**
- Uses GraveStorage (hides and preserves stored items)
- Cannot be repaired (BaseTimeUntilRepair = -1)
- Prioritizable
