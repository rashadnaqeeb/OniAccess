# Furniture

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

## Beds

### Cot
- **ID:** Bed
- **Size:** 2x2
- **HP:** 10
- **Materials:** Raw Minerals or Wood, 200 kg
- **Decor:** none
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Effects:** BedStamina, BedHealth
- **Tags:** BedType (counts for Barracks/Bedroom rooms)
- **Assignable:** Bed slot (one duplicant)
- **Tech:** none (available from start)
- **Notes:** Also functions as a DefragmentationZone for bionics (DLC3). Can be used in rockets (RocketUsageRestriction).

### Comfy Bed
- **ID:** LuxuryBed
- **Size:** 4x2
- **HP:** 10
- **Materials:** Plastic, 200 kg
- **Decor:** +15, radius 3
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Effects:** LuxuryBedStamina (restores more stamina than Cot), BedHealth
- **Tags:** BedType, LuxuryBedType (counts for Luxury Barracks/Private Bedroom)
- **Assignable:** Bed slot (one duplicant)
- **Tech:** Luxury
- **Notes:** Also functions as a DefragmentationZone for bionics (DLC3). Can be used in rockets.

### Ladder Bed
- **ID:** LadderBed
- **Size:** 2x2
- **HP:** 10
- **Materials:** Refined Metal, 200 kg
- **Decor:** none
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Effects:** LadderBedStamina, BedHealth
- **Tags:** BedType
- **Assignable:** Bed slot (one duplicant)
- **Placement:** OnFloor or BuildingAttachPoint (stackable via LadderBed tag)
- **Tech:** RefinedObjects
- **DLC:** Spaced Out (DLC1)
- **Ladder:** Also functions as a ladder. Movement speed multiplier: 0.75x up, 0.75x down. Sleep is interrupted if another duplicant uses the ladder.
- **Notes:** Can attach another LadderBed at offset (0, 2) above. Flippable horizontally. Also functions as a DefragmentationZone for bionics (DLC3).

## Lights

### Lamp (Floor Lamp)
- **ID:** FloorLamp
- **Size:** 1x2
- **HP:** 10
- **Materials:** Metal, 50 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 8 W
- **Self-heat:** 0.5 kDTU/s
- **Max temp:** 800 K
- **Light:** 1000 lux, range 4, circle shape
- **Tech:** InteriorDecor

### Ceiling Light
- **ID:** CeilingLight
- **Size:** 1x1
- **HP:** 10
- **Materials:** Metal, 50 kg
- **Decor:** none
- **Build time:** 10 s
- **Power:** 10 W
- **Self-heat:** 0.5 kDTU/s
- **Max temp:** 800 K
- **Light:** 1800 lux, range 8, cone shape (angle 2.6)
- **Placement:** OnCeiling
- **Tech:** InteriorDecor

### Sun Lamp
- **ID:** SunLamp
- **Size:** 2x4
- **HP:** 10
- **Materials:** Refined Metal 200 kg + Glass 50 kg
- **Decor:** -20, radius 4 (penalty)
- **Build time:** 60 s
- **Power:** 960 W
- **Self-heat:** 4 kDTU/s
- **Exhaust heat:** 1 kDTU/s
- **Max temp:** 800 K
- **Light:** 40000 lux (SUNLAMP_LUX = HIGH_LIGHT * 4 = 10000 * 4), range 16, cone shape
- **Tech:** GlassFurnishings
- **Notes:** Provides sunlight-level lux. Can be paired with Beach Chairs for the lit tanning bonus.

### Dev Light Generator
- **ID:** DevLightGenerator
- **Size:** 1x1
- **HP:** 100
- **Materials:** Metal, 25 kg
- **Decor:** -15, radius 3 (penalty)
- **Build time:** 3 s
- **Power:** none
- **Max temp:** 2400 K
- **Light:** 1800 lux, range 8, circle shape
- **Placement:** Anywhere
- **Tech:** none
- **Notes:** Debug-only building (DebugOnly = true). Not available in normal gameplay.

### Mercury Ceiling Light
- **ID:** MercuryCeilingLight
- **Size:** 3x1
- **HP:** 30
- **Materials:** Metal, 200 kg
- **Decor:** none
- **Build time:** 30 s
- **Power:** 60 W
- **Self-heat:** 1 kDTU/s
- **Max temp:** 800 K
- **Light:** 60000 lux (max), range 8, quad shape (directional south, width 3, falloff rate 0.4)
- **Placement:** OnCeiling
- **Input:** Liquid pipe (Mercury)
- **Mercury consumption:** 0.13 kg/s
- **Mercury storage:** 0.26 kg
- **Charging delay:** 60 s to reach full brightness
- **Tech:** PrecisionPlumbing
- **DLC:** The Frosty Planet Pack (DLC2)
- **Notes:** Has logic port. Light ramps up over time after power-on. Mercury is consumed to maintain brightness.

## Dining

### Mess Table
- **ID:** DiningTable
- **Size:** 1x1
- **HP:** 10
- **Materials:** Metal, 200 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Work time:** 20 s (eating duration)
- **Assignable:** MessStation slot (one duplicant, can be public)
- **Tags:** DiningTableType (counts for Mess Hall/Great Hall rooms)
- **Storage:** Table Salt, capacity 0.005 kg, refill at 0.001 kg
- **Tech:** FineDining
- **Notes:** Can be used in rockets. Tileable animation.

### Communal Table
- **ID:** MultiMinionDiningTable
- **Size:** 5x1
- **HP:** 10
- **Materials:** Wood, 400 kg
- **Decor:** +15, radius 3
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Work time:** 20 s
- **Seats:** 3 (simultaneous diners)
- **Tags:** DiningTableType
- **Storage:** Table Salt, capacity 0.015 kg (3x single table), refill at 0.003 kg
- **Tech:** Luxury
- **Notes:** Sharing a meal with companions provides a morale boost.

## Recreation Buildings

All recreation buildings are tagged RecBuilding (counts for Recreation Room). All recommend placement in a Rec Room (RoomTracker.Requirement.Recommended). Morale values are from YAML effect data, validated against wiki.

### Water Cooler
- **ID:** WaterCooler
- **Size:** 2x2
- **HP:** 30
- **Materials:** Raw Minerals, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Overheat:** disabled
- **Max temp:** 1600 K
- **Power:** none
- **Input:** Manual delivery of Water or Milk, 10 kg capacity, refill at 9 kg
- **Work time:** 5 s
- **Morale:** +1 (Water), also grants DuplicantGotMilk effect with Milk
- **Tech:** Jobs
- **Notes:** Provides a socializing point during Downtime. Also provides HeatImmunity (cold immunity). Beverage options: Water (default), Milk. Can be used in rockets.

### Jukebot (Phonobox)
- **ID:** Phonobox
- **Size:** 5x3
- **HP:** 30
- **Materials:** Raw Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 960 W
- **Self-heat:** 1 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Work time:** 15 s per dancer
- **Simultaneous users:** up to 5 (offsets -2 to +2)
- **Effect:** Danced (morale +2, from YAML)
- **Tracking effect:** RecentlyDanced
- **Tech:** Acoustics
- **Notes:** Can be used in rockets.

### Arcade Cabinet
- **ID:** ArcadeMachine
- **Size:** 3x3
- **HP:** 30
- **Materials:** Refined Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 1200 W
- **Self-heat:** 2 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Work time:** 15 s per player
- **Simultaneous users:** up to 2
- **Effect:** PlayedArcade (morale +3, from YAML)
- **Tracking effect:** RecentlyPlayedArcade
- **Tech:** DupeTrafficControl
- **Notes:** Can be used in rockets.

### Espresso Machine
- **ID:** EspressoMachine
- **Size:** 3x3
- **HP:** 30
- **Materials:** Refined Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 480 W
- **Self-heat:** 1 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Input:** Liquid pipe (Water), capacity 2 kg + manual delivery of Pincha Peppernut (SpiceNut), 10 kg capacity, refill at 5 kg
- **Storage:** 20 kg total
- **Per use:** 1 kg Water + 1 kg SpiceNut
- **Work time:** 30 s
- **Effect:** Espresso (morale +4, from YAML)
- **Tracking effect:** RecentlyRecDrink
- **Tech:** PrecisionPlumbing

### Hot Tub
- **ID:** HotTub
- **Size:** 5x2
- **HP:** 30
- **Materials:** Metal 200 kg + Wood 200 kg
- **Decor:** +15, radius 3
- **Build time:** 10 s
- **Power:** 240 W
- **Self-heat:** 4 kDTU/s
- **Exhaust heat:** 1 kDTU/s
- **Max temp:** 1600 K (overheatable, overheat temperature 310.85 K / 37.7 C)
- **Input:** Liquid pipe (Water), capacity 100 kg
- **Output:** Liquid pipe (drains water)
- **Consumable:** Bleach Stone, 100 kg capacity, refill at 10 kg, consumption rate 7/60 kg/s (~0.117 kg/s)
- **Water temperature:** minimum 310.85 K (37.7 C) required to operate
- **Water cooling rate:** 15 kDTU/s (water cools during use)
- **Work time:** 90 s
- **Effect:** HotTub (morale +5, from YAML)
- **Tracking effect:** RecentlyHotTub
- **Cold immunity:** WarmTouch effect, duration 1800 s (3 cycles)
- **Priority:** TIER4 (40)
- **Tech:** MedicineIV
- **Notes:** Can be used in rockets.

### Mechanical Surfboard
- **ID:** MechanicalSurfboard
- **Size:** 2x3
- **HP:** 30
- **Materials:** Raw Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 60 s
- **Power:** 480 W
- **Self-heat:** 1 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Input:** Liquid pipe (Water), tank capacity 20 kg
- **Water spill rate:** 0.05 kg/s (splashes water on floor during use)
- **Minimum operational water:** 2 kg
- **Work time:** 30 s
- **Effect:** MechanicalSurfboard (morale +4, from YAML)
- **Tracking effect:** RecentlyMechanicalSurfboard
- **Priority:** TIER3 (30)
- **Tech:** FlowRedirection
- **Notes:** Can be used in rockets.

### Sauna
- **ID:** Sauna
- **Size:** 3x3
- **HP:** 30
- **Materials:** Metal 100 kg + Wood 100 kg
- **Decor:** +10, radius 2
- **Build time:** 60 s
- **Power:** 60 W
- **Self-heat:** 0.5 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Input:** Gas pipe (Steam), capacity 50 kg
- **Output:** Liquid pipe (Water), output temperature 353.15 K (80 C)
- **Steam consumption:** 25 kg per use
- **Work time:** 30 s
- **Effect:** Sauna (morale +4, from YAML)
- **Tracking effect:** RecentlySauna
- **Cold immunity:** WarmTouch effect, duration 1800 s (3 cycles)
- **Priority:** TIER3 (30)
- **Tech:** RenewableEnergy
- **Notes:** Converts Steam to Water. Can be used in rockets.

### Juicer
- **ID:** Juicer
- **Size:** 3x4
- **HP:** 30
- **Materials:** Raw Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 120 W
- **Self-heat:** 0.5 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Input:** Liquid pipe (Water), capacity 2 kg + manual delivery of Mushroom, Bristle Berry (PrickleFruit), and Meal Lice (BasicPlantFood), each 10 kg capacity
- **Water per use:** 1 kg
- **Food per use:** calculated from calorie ratios (Mushroom: 300000 kcal worth, Bristle Berry: 600000 kcal worth, Meal Lice: 500000 kcal worth)
- **Work time:** 30 s
- **Effect:** Juicer (morale +4, from YAML)
- **Tracking effect:** RecentlyRecDrink
- **Priority:** TIER5 (50)
- **Tech:** FoodRepurposing
- **Notes:** Can be used in rockets.

### Soda Fountain
- **ID:** SodaFountain
- **Size:** 2x2
- **HP:** 30
- **Materials:** Refined Metal, 200 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 480 W
- **Self-heat:** 1 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Input:** Liquid pipe (Water), capacity 20 kg + manual delivery of Carbon Dioxide (solid), 4 kg capacity, refill at 1 kg
- **CO2 per use:** 1 kg
- **Water per use:** 5 kg
- **Work time:** 30 s
- **Effect:** SodaFountain (morale +4, from YAML)
- **Tracking effect:** RecentlyRecDrink
- **Priority:** TIER5 (50)
- **Tech:** Catalytics

### Beach Chair
- **ID:** BeachChair
- **Size:** 2x3
- **HP:** 30
- **Materials:** Raw Minerals 400 kg + Building Fiber 2 kg
- **Decor:** +25, radius 5
- **Build time:** 60 s
- **Power:** none
- **Max temp:** 1600 K (overheatable)
- **Work time:** 150 s
- **Effect (lit, >= 10000 lux for 75%+ of work time):** BeachChairLit (morale +8, from YAML)
- **Effect (unlit):** BeachChairUnlit (lower morale bonus)
- **Tracking effect:** RecentlyBeachChair
- **Light threshold:** HIGH_LIGHT = 10000 lux (from DUPLICANTSTATS). If lit for >= 75% of work time, grants the lit effect.
- **Priority:** TIER4 (40)
- **Tech:** EnvironmentalAppreciation
- **Notes:** Tileable. Can be used in rockets.

### Vertical Wind Tunnel
- **ID:** VerticalWindTunnel
- **Size:** 5x6
- **HP:** 30
- **Materials:** Plastic, 1200 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 1200 W
- **Self-heat:** 2 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Work time:** 90 s
- **Effect:** VerticalWindTunnel (morale +4, from YAML)
- **Tracking effect:** RecentlyVerticalWindTunnel
- **Gas displacement:** 3 kg/s (consumes gas from below, emits above; two ElementConsumers at 3 kg/s each, radius 2)
- **Priority:** TIER4 (40)
- **Tech:** TravelTubes
- **Notes:** Area below must be left vacant. Can be used in rockets.

### Party Line Phone (Telephone)
- **ID:** Telephone
- **Size:** 1x2
- **HP:** 30
- **Materials:** Raw Metal, 400 kg
- **Decor:** +10, radius 2
- **Build time:** 10 s
- **Power:** 120 W
- **Self-heat:** 0.5 kDTU/s
- **Max temp:** 1600 K (overheatable)
- **Work time:** 40 s (caller)
- **Ring time:** 15 s (rings before call connects)
- **Call time:** 25 s
- **Effects:** TelephoneBabble (solo call, lower morale), TelephoneChat (call with another dupe on another phone, higher morale), TelephoneLongDistance (cross-asteroid call, highest morale)
- **Tracking effect:** RecentlyTelephoned
- **Priority:** TIER5 (50)
- **Tech:** NotificationSystems
- **DLC:** Spaced Out (DLC1)
- **Notes:** Can be used in rockets.

## Pixel Pack
- **ID:** PixelPack
- **Size:** 4x1
- **HP:** 30
- **Materials:** Glass 100 kg + Refined Metal 25 kg
- **Decor:** +20, radius 4
- **Build time:** 10 s
- **Power:** 10 W
- **Self-heat:** 0 kDTU/s
- **Max temp:** 1600 K
- **Overheat:** disabled
- **Rotation:** 360-degree
- **Placement:** NotInTiles (interior wall layer, acts as backwall)
- **Logic input:** Ribbon input port for color selection (4 bits for 4 pixels)
- **Tech:** Screens
- **Notes:** Not entombable, not floodable, not replaceable. Four individually colored pixels controlled by automation ribbon.

## Flower Pots

### Flower Pot
- **ID:** FlowerVase
- **Size:** 1x1
- **HP:** 10
- **Materials:** Raw Minerals, 50 kg
- **Decor:** none (decor comes from the planted plant)
- **Build time:** 10 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnFloor
- **Tags:** Decoration, accepts DecorSeed
- **Tech:** InteriorDecor
- **Notes:** Houses one decorative plant. Off-ground planter (no irrigation).

### Wall Pot
- **ID:** FlowerVaseWall
- **Size:** 1x1
- **HP:** 10
- **Materials:** Raw Metal, 50 kg
- **Decor:** none (decor comes from the planted plant)
- **Build time:** 10 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnWall (flippable horizontally)
- **Tags:** Decoration, accepts DecorSeed
- **Tech:** Artistry

### Hanging Pot
- **ID:** FlowerVaseHanging
- **Size:** 1x2
- **HP:** 10
- **Materials:** Raw Metal, 50 kg
- **Decor:** none (decor comes from the planted plant)
- **Build time:** 10 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnCeiling
- **Tags:** Decoration, accepts DecorSeed
- **Tech:** Artistry

### Aero Pot
- **ID:** FlowerVaseHangingFancy
- **Size:** 1x2
- **HP:** 10
- **Materials:** Transparent (Glass), 50 kg
- **Decor:** +10, radius 4 (custom: TIER1 amount with TIER3 radius; provides decor even when empty)
- **Build time:** 10 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnCeiling
- **Tags:** Decoration, accepts DecorSeed
- **Tech:** GlassFurnishings

## Sculptures

All sculptures are artable (isArtable = true). Quality levels increase decor. Requires CanArt skill perk unless noted. All are flippable horizontally. BaseTimeUntilRepair = -1 (never auto-repairs).

### Sculpting Block (Small Sculpture)
- **ID:** SmallSculpture
- **Size:** 1x2
- **HP:** 10
- **Materials:** Raw Minerals, 200 kg
- **Decor:** +5, radius 4 (unsculpted base)
- **Build time:** 60 s
- **Max temp:** 1600 K
- **Skill:** CanArt
- **Tech:** Artistry

### Large Sculpting Block (Sculpture)
- **ID:** Sculpture
- **Size:** 1x3
- **HP:** 30
- **Materials:** Raw Minerals, 400 kg
- **Decor:** +10, radius 8 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Skill:** CanArt
- **Tech:** FineArt

### Ice Block (Ice Sculpture)
- **ID:** IceSculpture
- **Size:** 2x2
- **HP:** 10
- **Materials:** Ice, 400 kg
- **Decor:** +35, radius 8 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 273.15 K (0 C; melts above freezing)
- **Build temperature:** 253.15 K (-20 C)
- **Skill:** CanArt
- **Tech:** Artistry
- **Notes:** Prone to melting. Must be kept below 0 C.

### Wood Block (Wood Sculpture)
- **ID:** WoodSculpture
- **Size:** 1x1
- **HP:** 10
- **Materials:** Wood, 400 kg
- **Decor:** +3, radius 4 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 800 K
- **Placement:** Anywhere (interior wall layer)
- **Skill:** CanArt
- **Tech:** RefractiveDecor
- **DLC:** The Frosty Planet Pack (DLC2)
- **Notes:** Uses LongRangeSculpture component (different from standard Sculpture).

### Marble Block (Marble Sculpture)
- **ID:** MarbleSculpture
- **Size:** 2x3
- **HP:** 10
- **Materials:** Precious Rock, 400 kg
- **Decor:** +20, radius 8 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Skill:** CanArt
- **Tech:** RenaissanceArt

### Metal Block (Metal Sculpture)
- **ID:** MetalSculpture
- **Size:** 1x3
- **HP:** 10
- **Materials:** Refined Metal, 400 kg
- **Decor:** +20, radius 8 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Skill:** CanArt
- **Tech:** RefractiveDecor

### Fossil Block (Fossil Sculpture)
- **ID:** FossilSculpture
- **Size:** 3x3
- **HP:** 100
- **Materials:** Fossils, 400 kg
- **Decor:** +35, radius 6 (unsculpted base, from DECOR.BONUS.TIER5)
- **Build time:** 240 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Skill:** CanArtGreat (requires higher art skill)
- **Tech:** RenaissanceArt
- **DLC:** Biomes Upgrade Pack (DLC4)
- **Notes:** Not repairable. Not floodable. Uses standard Sculpture component with CanArtGreat requirement.

### Hanging Fossil Block (Ceiling Fossil Sculpture)
- **ID:** CeilingFossilSculpture
- **Size:** 3x2
- **HP:** 100
- **Materials:** Fossils, 400 kg
- **Decor:** +35, radius 6 (unsculpted base, from DECOR.BONUS.TIER5)
- **Build time:** 240 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnCeiling
- **Skill:** CanArtGreat
- **Tech:** RenaissanceArt
- **DLC:** Biomes Upgrade Pack (DLC4)
- **Notes:** Not repairable. Uses LongRangeSculpture component.

## Trim

### Ceiling Trim (Crown Moulding)
- **ID:** CrownMoulding
- **Size:** 1x1
- **HP:** 10
- **Materials:** Raw Minerals, 100 kg
- **Decor:** +5, radius 3
- **Build time:** 30 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnCeiling
- **Tags:** Decoration
- **Tech:** Artistry
- **Notes:** Tileable animation.

### Corner Trim (Corner Moulding)
- **ID:** CornerMoulding
- **Size:** 1x1
- **HP:** 10
- **Materials:** Raw Minerals, 100 kg
- **Decor:** +5, radius 3
- **Build time:** 30 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** InCorner (flippable horizontally)
- **Tags:** Decoration
- **Tech:** Artistry

## Paintings

All canvases are artable. Requires CanArt skill perk. Placed on interior wall layer (Anywhere placement). All are flippable horizontally. BaseTimeUntilRepair = -1.

### Blank Canvas
- **ID:** Canvas
- **Size:** 2x2
- **HP:** 30
- **Materials:** Metal 400 kg + Building Fiber 1 kg
- **Decor:** +10, radius 6 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Overheat:** disabled
- **Skill:** CanArt
- **Tech:** FineArt

### Landscape Canvas
- **ID:** CanvasWide
- **Size:** 3x2
- **HP:** 30
- **Materials:** Metal 400 kg + Building Fiber 1 kg
- **Decor:** +15, radius 6 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Overheat:** disabled
- **Skill:** CanArt
- **Tech:** RefractiveDecor

### Portrait Canvas
- **ID:** CanvasTall
- **Size:** 2x3
- **HP:** 30
- **Materials:** Metal 400 kg + Building Fiber 1 kg
- **Decor:** +15, radius 6 (unsculpted base)
- **Build time:** 120 s
- **Max temp:** 1600 K
- **Overheat:** disabled
- **Skill:** CanArt
- **Tech:** RenaissanceArt

## Display

### Display Shelf
- **ID:** Shelf
- **Size:** 1x1
- **HP:** 30
- **Materials:** Wood, 50 kg
- **Decor:** +5, radius 1 (TIER0, from building itself; displayed item adds its own decor)
- **Build time:** 10 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnBackWall
- **Accepts:** Ornaments, Suits, Clothes, Eggs, Seeds, Edibles, Bionic Upgrades, Solids, Liquids, Gases, PedestalDisplayable items
- **Tech:** FineArt
- **Notes:** Drag-buildable. Tiles horizontally with adjacent shelves. Storage preserves and seals items.

### Pedestal
- **ID:** ItemPedestal
- **Size:** 1x2
- **HP:** 10
- **Materials:** Raw Minerals, 100 kg
- **Decor:** +5, radius 1 (TIER0; doubles displayed item's decor value)
- **Build time:** 30 s
- **Max temp:** 800 K
- **Overheat:** disabled
- **Placement:** OnFloor
- **Accepts:** Same tags as Shelf (Ornaments, Suits, Clothes, Eggs, Seeds, Edibles, Bionic Upgrades, Solids, Liquids, Gases, PedestalDisplayable)
- **Tech:** Artistry
- **Notes:** Doubles the decor value of displayed objects. Objects with negative decor gain some positive decor when displayed.

## Monument

Three-piece monument (Bottom, Middle, Top). Building all three completes a Great Monument, required for the Colonize Imperative. Each piece is artable with customizable designs. All pieces are invincible, flippable horizontally, and cannot be moved after placement.

### Monument Base
- **ID:** MonumentBottom
- **Size:** 5x5
- **HP:** 1000
- **Materials:** Steel 7500 kg + Obsidian 2500 kg
- **Decor (incomplete):** +10, radius 5. **(complete):** +40, radius 10
- **Build time:** 60 s
- **Max temp:** 9999 K
- **Overheat:** 2273.15 K (2000 C)
- **Placement:** OnFloor
- **Tech:** Monuments
- **Notes:** Provides attach point for MonumentMiddle at offset (0, 5) above. Invincible.

### Monument Midsection
- **ID:** MonumentMiddle
- **Size:** 5x5
- **HP:** 1000
- **Materials:** Ceramic 2500 kg + Polypropylene 2500 kg + Steel 5000 kg
- **Decor (incomplete):** +10, radius 5. **(complete):** +40, radius 10
- **Build time:** 60 s
- **Max temp:** 9999 K
- **Overheat:** 2273.15 K (2000 C)
- **Placement:** BuildingAttachPoint (attaches to MonumentBottom)
- **Tech:** Monuments
- **Notes:** Provides attach point for MonumentTop at offset (0, 5) above. Invincible.

### Monument Top
- **ID:** MonumentTop
- **Size:** 5x5
- **HP:** 1000
- **Materials:** Glass 2500 kg + Diamond 2500 kg + Steel 5000 kg
- **Decor (incomplete):** +10, radius 5. **(complete):** +40, radius 10
- **Build time:** 60 s
- **Max temp:** 9999 K
- **Overheat:** 2273.15 K (2000 C)
- **Placement:** BuildingAttachPoint (attaches to MonumentMiddle)
- **Tech:** Monuments
- **Notes:** Invincible.

## Park Sign
- **ID:** ParkSign
- **Size:** 1x2
- **HP:** 10
- **Materials:** Any Buildable, 50 kg
- **Decor:** none
- **Build time:** 10 s
- **Max temp:** 1600 K
- **Tags:** Park (classifies area as Park or Nature Reserve)
- **Tech:** Agriculture
- **Notes:** Does not provide decor itself. Establishes the room type for morale bonuses (Park: +3, Nature Reserve: +6).
