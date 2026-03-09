# Oxygen

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## Oxygen Diffuser (MineralDeoxidizer)

- **Name:** Oxygen Diffuser
- **Description:** Oxygen diffusers are inefficient, but output enough oxygen to keep a colony breathing.
- **Effect:** Converts large amounts of Algae into Oxygen. Becomes idle when the area reaches maximum pressure capacity.
- **Size:** 1 wide x 2 tall
- **Placement:** On floor
- **Construction:** 200 kg Metal (any)
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 75 dB (radius 15) [NOISY.TIER3]
- **Power:** 120 W
- **Exhaust heat:** 0.5 kDTU/s
- **Self-heat:** 1 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Breakable:** Yes
- **Technology:** None (available from start)
- **DLC:** Base game
- **Logic input:** Automation port at offset (0, 1)
- **Element conversion:**
  - Input: 0.55 kg/s Algae
  - Output: 0.5 kg/s Oxygen at 303.15 K (30 C)
- **Storage:** 330 kg (Algae only, manual delivery, refill at 132 kg)
- **Overpressure:** Stops emitting when gas mass at emission cell exceeds 1.8 kg (checked via 3-cell flood fill from offset (0, 1))
- **Emission offset:** (0, 1)
- **Meter:** None

---

## Sublimation Station (SublimationStation)

- **Name:** Sublimation Station
- **Description:** Sublimation is the sublime process by which solids convert directly into gas.
- **Effect:** Speeds up the conversion of Polluted Dirt into Polluted Oxygen. Becomes idle when the area reaches maximum pressure capacity.
- **Size:** 2 wide x 1 tall
- **Placement:** On floor
- **Construction:** 200 kg Metal (any)
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 75 dB (radius 15) [NOISY.TIER3]
- **Power:** 60 W
- **Exhaust heat:** 0.5 kDTU/s
- **Self-heat:** 1 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Breakable:** Yes
- **Technology:** None (available from start)
- **DLC:** Spaced Out! (EXPANSION1)
- **Logic input:** Automation port at offset (0, 0)
- **Element conversion:**
  - Input: 1 kg/s Polluted Dirt (ToxicSand)
  - Output: 0.66 kg/s Polluted Oxygen (ContaminatedOxygen) at 303.15 K (30 C)
- **Storage:** 600 kg (Polluted Dirt only, manual delivery, refill at 240 kg)
- **Overpressure:** Stops emitting when gas mass at emission cell exceeds 1.8 kg (checked via 3-cell flood fill from offset (0, 0))
- **Emission offset:** (0, 0)
- **Meter:** None

---

## Oxylite Sconce (Oxysconce)

- **Name:** Oxylite Sconce
- **Description:** Sconces prevent diffused oxygen from being wasted inside storage bins.
- **Effect:** Stores a small chunk of Oxylite which gradually releases Oxygen into the environment.
- **Size:** 1 wide x 1 tall
- **Placement:** Anywhere (wall, floor, ceiling)
- **Construction:** 25 kg Metal (any) [TIER0], build time 3 s
- **HP:** 10
- **Decor:** +5 (radius 1) [BONUS.TIER0]
- **Noise:** 45 dB (radius 10) [NOISY.TIER0]
- **Power:** None
- **Exhaust heat:** 0 kDTU/s
- **Self-heat:** 0 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Breakable:** Yes
- **Technology:** PortableGasses
- **DLC:** DLC2 (Frosty Planet Pack)
- **No logic port**
- **Mechanism:** No element converter. Stores Oxylite which off-gasses Oxygen naturally (Oxylite's innate sublimation). The sconce simply holds the Oxylite exposed to the environment rather than sealed in storage.
- **Storage:** 240 kg (Oxylite/OxyRock only, manual delivery, refill at 96 kg). Shows capacity as main status.
- **Has meter:** Yes (StorageMeter)

---

## Algae Terrarium (AlgaeHabitat)

- **Name:** Algae Terrarium
- **Description:** Algae colony, Duplicant colony... we're more alike than we are different.
- **Effect:** Consumes Algae to produce Oxygen and remove some Carbon Dioxide. Gains a 10% efficiency boost in direct Light.
- **Size:** 1 wide x 2 tall
- **Placement:** On floor
- **Construction:** 400 kg Farmable [TIER4]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 45 dB (radius 10) [NOISY.TIER0]
- **Power:** None
- **Exhaust heat:** 0 kDTU/s (not set)
- **Self-heat:** 0 kDTU/s (not set)
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 1600 K
- **Floodable:** No
- **Technology:** FarmingTech
- **DLC:** Base game
- **No logic port**
- **Element conversion (primary converter):**
  - Input: 0.03 kg/s Algae + 0.3 kg/s Water
  - Output: 0.04 kg/s Oxygen at 303.15 K (30 C)
- **Element conversion (secondary converter):**
  - Output: 0.29033336 kg/s Polluted Water at 303.15 K (30 C), stored internally
- **CO2 consumption:** ElementConsumer absorbs 0.0003333 kg/s Carbon Dioxide (radius 3, sample offset (0, 1)). Not required for operation.
- **Passive water intake:** PassiveElementConsumer absorbs Water at 1.2 kg/s (radius 1), capacity 360 kg, stored directly. This supplements piped/manual delivery.
- **Storage (primary):** Algae: 90 kg capacity, manual delivery, refill at 18 kg. Water: 360 kg capacity, manual delivery, refill at 72 kg.
- **Storage (secondary):** 360 kg for Polluted Water output (sealed, hidden, not removable by player)
- **Polluted Water emptying:** Requires dupe errand (AlgaeHabitatEmpty), work time 5 s
- **Light bonus:** 1.1x multiplier (10% boost) when lit
- **Pressure sample offset:** (0, 1)

---

## Deodorizer (AirFilter)

- **Name:** Deodorizer
- **Description:** Oh! Citrus scented!
- **Effect:** Uses Sand to filter Polluted Oxygen from the air, reducing Disease spread.
- **Size:** 1 wide x 1 tall
- **Placement:** On floor
- **Construction:** 100 kg Raw Minerals (BuildableRaw) [TIER2]
- **HP:** 30
- **Decor:** 0 (radius 1) [NONE]
- **Noise:** 45 dB (radius 10) [NOISY.TIER0]
- **Power:** 5 W
- **Exhaust heat:** 0.125 kDTU/s
- **Self-heat:** 0.5 kDTU/s
- **Overheatable:** No
- **Melting point:** 1600 K
- **Technology:** DirectedAirStreams
- **DLC:** Base game
- **No logic port**
- **Polluted Oxygen intake:** ElementConsumer pulls ContaminatedOxygen at 0.5 kg/s (radius 3), capacity 0.5 kg, stored internally. Not required for operation. Ignores active state changes.
- **Element conversion:**
  - Input: 0.13333 kg/s Filterable (Sand) + 0.1 kg/s Polluted Oxygen (ContaminatedOxygen)
  - Output: 0.14333335 kg/s Clay (stored internally, dropped at 10 kg) + 0.09 kg/s Oxygen
  - Output temperatures: use entity temperature (0 K flag means inherit)
  - Output disease transfer: Clay gets 25% of disease, Oxygen gets 75%
- **Storage:** 200 kg total capacity (sealed). Sand (Filter): 320 kg manual delivery capacity, refill at 32 kg.
- **Clay drop:** Emits 10 kg Clay when internal storage accumulates that much (ElementDropper)

---

## Carbon Skimmer (CO2Scrubber)

- **Name:** Carbon Skimmer
- **Description:** Skimmers remove large amounts of carbon dioxide, but produce no breathable air.
- **Effect:** Uses Water to filter Carbon Dioxide from the air.
- **Size:** 2 wide x 2 tall
- **Placement:** On floor
- **Construction:** 100 kg Raw Metal (Metal) [TIER2]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 75 dB (radius 15) [NOISY.TIER3]
- **Power:** 120 W
- **Exhaust heat:** 0 kDTU/s (not set)
- **Self-heat:** 1 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Technology:** DirectedAirStreams
- **DLC:** Base game
- **Rotation:** FlipH (horizontally flippable)
- **Logic input:** Automation port at offset (1, 0)
- **Room tag:** IndustrialMachinery
- **Liquid input:** Conduit at offset (0, 0), Water only, consumption rate 2 kg/s, capacity 2 kg. Wrong element is stored (not dumped).
- **Liquid output:** Conduit at offset (1, 1), dispenses everything except Water (i.e., outputs Polluted Water)
- **CO2 intake:** PassiveElementConsumer pulls CarbonDioxide at 0.6 kg/s (radius 3), capacity 0.6 kg, stored internally. Not required for operation. Ignores active state changes.
- **Element conversion:**
  - Input: 1 kg/s Water + 0.3 kg/s Carbon Dioxide
  - Output: 1 kg/s Polluted Water (stored internally, dispensed via liquid output conduit)
  - Output temperature: uses entity temperature (0 K flag means inherit)
- **Storage:** 30,000 kg capacity (sealed)

---

## Electrolyzer (Electrolyzer)

- **Name:** Electrolyzer
- **Description:** Water goes in one end, life sustaining oxygen comes out the other.
- **Effect:** Converts Water into Oxygen and Hydrogen Gas. Becomes idle when the area reaches maximum pressure capacity.
- **Size:** 2 wide x 2 tall
- **Placement:** On floor
- **Construction:** 200 kg Metal (any) [TIER3]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 75 dB (radius 15) [NOISY.TIER3]
- **Power:** 120 W
- **Power input offset:** (1, 0)
- **Exhaust heat:** 0.25 kDTU/s
- **Self-heat:** 1 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Technology:** ImprovedOxygen
- **DLC:** Base game
- **Logic input:** Automation port at offset (1, 1)
- **Room tag:** IndustrialMachinery
- **Liquid input:** Conduit at offset (0, 0), Water only, consumption rate 1 kg/s. Wrong element is dumped.
- **Element conversion:**
  - Input: 1 kg/s Water
  - Output: 0.888 kg/s Oxygen at 343.15 K (70 C) + 0.112 kg/s Hydrogen at 343.15 K (70 C)
- **Storage:** 2 kg capacity (hidden, insulated)
- **Overpressure:** Stops emitting when gas mass at emission cell exceeds 1.8 kg (checked via 3-cell flood fill from offset (0, 1))
- **Emission offset:** (0, 1)
- **Meter:** Yes (water level meter)

---

## Rust Deoxidizer (RustDeoxidizer)

- **Name:** Rust Deoxidizer
- **Description:** Rust and salt goes in, oxygen comes out.
- **Effect:** Converts Rust into Oxygen and Chlorine Gas. Becomes idle when the area reaches maximum pressure capacity.
- **Size:** 2 wide x 3 tall
- **Placement:** On floor
- **Construction:** 200 kg Metal (any) [TIER3]
- **HP:** 30
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Noise:** 75 dB (radius 15) [NOISY.TIER3]
- **Power:** 60 W
- **Power input offset:** (1, 0)
- **Exhaust heat:** 0.125 kDTU/s
- **Self-heat:** 1 kDTU/s
- **Overheat temp:** 348.15 K (75 C)
- **Melting point:** 800 K
- **Technology:** ImprovedOxygen
- **DLC:** Base game
- **Logic input:** Automation port at offset (1, 1)
- **Room tag:** IndustrialMachinery
- **Element conversion:**
  - Input: 0.75 kg/s Rust + 0.25 kg/s Salt (total 1 kg/s)
  - Output: 0.57 kg/s Oxygen at 348.15 K (75 C) + 0.03 kg/s Chlorine Gas at 348.15 K (75 C) + 0.4 kg/s Iron Ore at 348.15 K (75 C, stored internally)
- **Storage (sealed):**
  - Rust: 585 kg capacity, manual delivery, refill at 193.05 kg
  - Salt: 195 kg capacity, manual delivery, refill at 64.35 kg
- **Overpressure:** Stops emitting when gas mass exceeds 1.8 kg (checked via 3-cell flood fill from the cell above the building origin)
- **Iron Ore drop:** Emits 24 kg Iron Ore when internal storage accumulates that much (ElementDropper, drop offset (0, 1))
