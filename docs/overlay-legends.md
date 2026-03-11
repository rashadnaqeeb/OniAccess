# Overlay Legend Contents

Every overlay in ONI can show a legend panel on the right side of the screen via `OverlayLegend`. Some are static (serialized in Unity prefab), some are programmatically generated. This document lists the actual text and color values of each.

All colors are RGBA 0-255. Where alpha is 255 (fully opaque) it is omitted for brevity.

---

## Oxygen

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Very Breathable | (108, 204, 229) | High Oxygen concentrations |
| Breathable | (78, 79, 221) | Sufficient Oxygen concentrations |
| Barely Breathable | (176, 75, 136) | Low Oxygen concentrations |
| Unbreathable | (206, 58, 58) | Extremely low or absent Oxygen concentrations. Duplicants will suffocate if trapped in these areas |

Note: The STRINGS namespace defines 6 entries (including Slightly Toxic and Very Toxic) but only 4 are configured on the prefab.

## Power (Electrical)

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| **BUILDING POWER** | white (header) | Displays whether buildings use or generate Power |
| Producer | (107, 211, 132) | These buildings generate power for a circuit |
| Consumer | white | These buildings draw power from a circuit |
| Switch | white | Activates or deactivates connected circuits |
| **CIRCUIT POWER HEALTH** | white (header) | Displays the health of wire systems |
| Inactive | (78, 79, 221) | There is no power activity on these circuits |
| Safe | white | These circuits are not in danger of overloading |
| Strained | white | These circuits are close to consuming more power than their wires support |
| Overloaded | (244, 74, 71) | These circuits are consuming more power than their wires support |

## Temperature

Has multiple sub-modes controlled by a filter menu.

### Absolute Temperature

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Molten | (251, 2, 0, 191) | Temperatures reaching {threshold} |
| Scorching | (251, 83, 80, 191) | Temperatures reaching {threshold} |
| Hot | (255, 169, 36, 191) | Temperatures reaching {threshold} |
| Warm | (239, 255, 0, 191) | Temperatures reaching {threshold} |
| Temperate | (59, 254, 74, 191) | Temperatures reaching {threshold} |
| Chilled | (31, 161, 255, 191) | Temperatures reaching {threshold} |
| Cold | (43, 203, 255, 191) | Temperatures reaching {threshold} |
| Absolute Zero | (128, 254, 240, 191) | Temperatures reaching {threshold} |
| Heat Source | white | Elements displaying this symbol can produce heat |
| Heat Sink | white | Elements displaying this symbol can absorb heat |

### Adaptive Temperature (Thermal Tolerance)

Shows the same programmatic temperature entries as Absolute, plus these static prefab entries:

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Body Heat Retention | (233, 66, 37) | Uncomfortably warm. Duplicants absorb more heat in toasty surroundings than they can release |
| Comfort Zone | (79, 79, 79) | Comfortable area. Duplicants can regulate their internal temperatures in these areas |
| Body Heat Loss | (64, 161, 231) | Uncomfortably cold. Duplicants lose more heat in chilly surroundings than they can absorb. Warm Coats help Duplicants retain body heat |

### Heat Flow

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Body Heat Retention | (234, 21, 21, 191) | Uncomfortably warm. Duplicants absorb more heat in toasty surroundings than they can release |
| Comfort Zone | (0, 0, 0, 191) | Comfortable area. Duplicants can regulate their internal temperatures in these areas |
| Body Heat Loss | (31, 161, 255, 191) | Uncomfortably cold. Duplicants lose more heat in chilly surroundings than they can absorb. Warm Coats help Duplicants retain body heat |
| Heat Source | white | Elements displaying this symbol can produce heat |
| Heat Sink | white | Elements displaying this symbol can absorb heat |

### State Change

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| High energy state change | (251, 2, 0, 191) | Nearing high energy state change |
| Stable | (59, 254, 74, 191) | Not near any state changes |
| Low energy state change | (128, 254, 240, 191) | Nearing a low energy state change |
| Heat Source | white | Elements displaying this symbol can produce heat |
| Heat Sink | white | Elements displaying this symbol can absorb heat |

### Relative Temperature

No legend entries (empty list).

## Crop (Farming)

Programmatically generated. Colors from `ColorSet`.

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Fully Grown | (107, 212, 133, 191) | These plants have reached maturation. Select the Harvest Tool to batch harvest |
| Growing | (250, 176, 59, 191) | These plants are thriving in their current conditions |
| Halted Growth | (242, 64, 71, 191) | Substandard conditions prevent these plants from growing |

## Decor

Programmatically generated. Colors from `ColorSet`.

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Positive Decor | (0, 255, 0, 204) | Area with sufficient Decor values. Lighting and aesthetically pleasing buildings increase decor |
| Negative Decor | (255, 0, 0) | Area with insufficient Decor values. Resources on the floor are considered "debris" and will decrease decor |

## Logic (Automation)

Programmatically generated. Port entries use white with sprite icons. Status colors from `ColorSet`.

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Input Port | white, icon: `logicInput` | Receives a signal from an automation grid |
| Output Port | white, icon: `logicOutput` | Sends a signal out to an automation grid |
| Ribbon Input Port | white, icon: `logic_ribbon_all_in` | Receives a 4-bit signal from an automation grid |
| Ribbon Output Port | white, icon: `logic_ribbon_all_out` | Sends a 4-bit signal out to an automation grid |
| Reset Port | white, icon: `logicResetUpdate` | Reset a Logic Memory's internal Memory to Red |
| Control Port | white, icon: `control_input_frame_legend` | Control the signal selection of a Logic Gate Multiplexer or Logic Gate Demultiplexer |
| **GRID STATUS** | white, no icon | *(section header)* |
| Green | (87, 185, 94) | This port is currently Green |
| Red | (244, 74, 71) | This port is currently Red |
| DISCONNECTED | (255, 255, 255) | This port is not connected to an automation grid |

## Plumbing (Liquid)

Only 3 entries on the prefab (Connected/Disconnected/Network entries from STRINGS are not present).

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Output Pipe | (107, 211, 132) | Outputs send liquid into pipes. Must be on the same network as at least one Intake |
| Building Intake | white | Intakes send liquid into buildings. Must be on the same network as at least one Output |
| Filtered Output Pipe | (251, 176, 59) | Filtered Outputs send filtered liquid into pipes. Must be on the same network as at least one Intake |

## Ventilation (Gas)

Same structure as Plumbing.

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Output Pipe | (107, 211, 132) | Outputs send Gas into Pipes. Must be on the same network as at least one Intake |
| Building Intake | white | Intakes send gas into buildings. Must be on the same network as at least one Output |
| Filtered Output Pipe | (251, 176, 59) | Filtered Outputs send filtered Gas into Pipes. Must be on the same network as at least one Intake |

## Conveyor (Shipping)

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Receptacle | white | Receives material from a Conveyor Rail and stores it for Duplicant use |
| Loader | (107, 211, 132) | Loads material onto a Conveyor Rail for transport to Receptacles |

## Exosuit (Suit)

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Exosuit | (0, 255, 0) | Highlights the current location of equippable exosuits |

## Light

Only 2 entries on the prefab (the 7 range indicators from STRINGS are not present).

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Lit Area | (214, 214, 88, 171) | Working in well-lit areas improves Duplicant Morale |
| Unlit Area | (69, 69, 69, 171) | Working in the dark has no effect on Duplicants |

## Disease (Germ)

Programmatically generated from `Db.Get().Diseases`. Has one static prefab entry plus dynamic disease entries.

Static entry:

| Entry | Color (RGBA) | Font color | Tooltip |
|-------|-------------|------------|---------|
| Germ Source | white | (223, 223, 223) | *(none)* |

Dynamic entries (colors from `ColorSet`):

| Entry | Color (RGBA) | Tooltip |
|-------|-------------|---------|
| Food Poisoning | (255, 231, 47) | Food Poisoning Germs present |
| Slimelung | (59, 254, 149) | Slimelung Germs present |
| Floral Scent | (228, 155, 241) | Floral Scent allergens present |
| Zombie Spores | (148, 183, 255) | Zombie Spores present |
| Radioactive Contaminants | (134, 226, 86) | Area Causes Radiation Sickness |

All disease tooltips also append: "Select an infected object for more details"

## Materials

No legend entries. Uses a filter menu to select element categories (Metal, Buildable, Liquid, Gas, etc.). Each tile is colored by its element's `substance.uiColour` — a per-element color defined in the element data. There are hundreds of elements, each with a unique color.

## Radiation

No legend entries. Tiles are colored with a single hue: `(51, 230, 77)` green, with alpha proportional to `sqrt(rads) / 30` clamped to 0-1. Low radiation is nearly transparent, high radiation is solid green.

---

## Room Overlay

Programmatically generated from `Db.Get().RoomTypes`, sorted by `sortKey`. Colors come from `RoomTypeCategories` via `colorSet.GetColorByName(category.colorName)`.

### Room Category Colors

| Category | Color (RGBA) |
|----------|-------------|
| None | (128, 128, 128) |
| Food | (255, 226, 132) |
| Sleep | (163, 255, 132) |
| Recreation | (66, 164, 244) |
| Bathroom | (132, 255, 244) |
| Hospital | (255, 132, 142) |
| Industrial | (244, 197, 66) |
| Agricultural | (205, 242, 72) |
| Park | (172, 255, 189) |
| Science | (189, 89, 202) |
| Bionic | (208, 138, 73) (DLC3) |

### Room Types

#### Miscellaneous Room
- **Category**: None -- (128, 128, 128)
- **Effect**: No effect
- **Criteria**: Enclosed by wall tile

#### Latrine
- **Category**: Bathroom -- (132, 255, 244)
- **Effect**: Morale bonus
- **Requires**: Toilet, Wash Station
- **Restrictions**: No Industrial Machinery
- **Size**: 12-64 tiles

#### Washroom
- **Category**: Bathroom -- (132, 255, 244)
- **Effect**: Morale bonus
- **Requires**: Flush Toilet, Advanced Wash Station
- **Restrictions**: No Outhouses, No Industrial Machinery
- **Size**: 12-64 tiles

#### Barracks
- **Category**: Sleep -- (163, 255, 132)
- **Effect**: Morale bonus
- **Requires**: Bed
- **Restrictions**: No Industrial Machinery
- **Size**: 12-64 tiles

#### Luxury Barracks
- **Category**: Sleep -- (163, 255, 132)
- **Effect**: Morale bonus
- **Requires**: Luxury Bed, Decorative Item
- **Restrictions**: No Cots, No Industrial Machinery
- **Size**: 12-64 tiles, Ceiling Height 4

#### Private Bedroom
- **Category**: Sleep -- (163, 255, 132)
- **Effect**: Morale bonus
- **Requires**: Single Luxury Bed, 2 Decorative Items, Backwall Tiles
- **Restrictions**: No Cots, No Industrial Machinery
- **Size**: 24-64 tiles, Ceiling Height 4

#### Mess Hall
- **Category**: Food -- (255, 226, 132)
- **Effect**: Morale bonus
- **Requires**: Dining Table
- **Restrictions**: No Industrial Machinery
- **Size**: 12-64 tiles

#### Great Hall
- **Category**: Food -- (255, 226, 132)
- **Effect**: Morale bonus
- **Requires**: Dining Table, Decorative Item, Recreational Building
- **Restrictions**: No Industrial Machinery
- **Size**: 32-120 tiles

#### Banquet Hall
- **Category**: Food -- (255, 226, 132)
- **Effect**: Morale bonus
- **Requires**: Multi-Minion Dining Table, Decorative Item, Recreational Building, Ornament Displayed
- **Restrictions**: No Industrial Machinery, No Basic Mess Stations
- **Size**: 32-120 tiles

#### Kitchen
- **Category**: Food -- (255, 226, 132)
- **Effect**: Enables Spice Grinder use
- **Requires**: Spice Station, Cook Top, Refrigerator
- **Restrictions**: No Mess Station
- **Size**: 12-96 tiles

#### Hospital
- **Category**: Hospital -- (255, 132, 142)
- **Effect**: Quarantine sick Duplicants
- **Requires**: Clinic, Toilet, Mess Station
- **Restrictions**: No Industrial Machinery
- **Size**: 12-96 tiles

#### Massage Clinic
- **Category**: Hospital -- (255, 132, 142)
- **Effect**: Massage stress relief bonus
- **Requires**: Massage Table, Decorative Item
- **Restrictions**: No Industrial Machinery
- **Size**: 12-64 tiles

#### Power Plant
- **Category**: Industrial -- (244, 197, 66)
- **Effect**: Enables Power Station Tune-ups on heavy-duty generators
- **Requires**: Power Station
- **Size**: 12-120 tiles

#### Machine Shop
- **Category**: Industrial -- (244, 197, 66)
- **Effect**: Increased fabrication efficiency
- **Requires**: Machine Shop
- **Size**: 12-96 tiles

#### Greenhouse
- **Category**: Agricultural -- (205, 242, 72)
- **Effect**: Enables Farm Station use
- **Requires**: Farm Station
- **Size**: 12-96 tiles

#### Stable
- **Category**: Agricultural -- (205, 242, 72)
- **Effect**: Critter taming and mood bonus
- **Requires**: Ranch Station
- **Size**: 12-96 tiles

#### Recreation Room
- **Category**: Recreation -- (66, 164, 244)
- **Effect**: Morale bonus
- **Requires**: Recreational Building, Decorative Item
- **Restrictions**: No Industrial Machinery
- **Size**: 12-96 tiles

#### Park
- **Category**: Park -- (172, 255, 189)
- **Effect**: Morale bonus
- **Requires**: Wild Plant, Recreational Building
- **Restrictions**: No Industrial Machinery
- **Size**: 12-64 tiles

#### Nature Reserve
- **Category**: Park -- (172, 255, 189)
- **Effect**: Morale bonus
- **Requires**: Park Building, Wild Plants
- **Restrictions**: No Industrial Machinery
- **Size**: 32-120 tiles

#### Laboratory
- **Category**: Science -- (189, 89, 202)
- **Effect**: Efficiency bonus
- **Requires**: Science Buildings
- **Restrictions**: No Industrial Machinery
- **Size**: 32-120 tiles

---

## Full ColorSet Dump

All `Color32` fields from `GlobalAssets.Instance.colorSet` (default color mode).

### Logic
| Field | RGBA |
|-------|------|
| logicOn | (87, 185, 94, 0) |
| logicOff | (244, 74, 71, 0) |
| logicDisconnected | (255, 255, 255) |
| logicOnText | (87, 185, 94) |
| logicOffText | (244, 74, 71) |
| logicOnSidescreen | (87, 185, 94) |
| logicOffSidescreen | (244, 74, 71) |

### Decor
| Field | RGBA |
|-------|------|
| decorPositive | (0, 255, 0, 204) |
| decorNegative | (255, 0, 0) |
| decorBaseline | (38, 0, 0) |
| decorHighlightPositive | (0, 204, 0, 204) |
| decorHighlightNegative | (255, 0, 0, 102) |

### Crop
| Field | RGBA |
|-------|------|
| cropHalted | (242, 64, 71, 191) |
| cropGrowing | (250, 176, 59, 191) |
| cropGrown | (107, 212, 133, 191) |

### Harvest
| Field | RGBA |
|-------|------|
| harvestEnabled | (107, 211, 132) |
| harvestDisabled | (244, 74, 71) |

### Gameplay Events
| Field | RGBA |
|-------|------|
| eventPositive | (67, 196, 57) |
| eventNegative | (196, 57, 57) |
| eventNeutral | (196, 194, 57) |

### Notifications
| Field | RGBA |
|-------|------|
| NotificationNormal | white |
| NotificationNormalBG | (49, 57, 88, 162) |
| NotificationBad | white |
| NotificationBadBG | (218, 62, 59, 147) |
| NotificationEvent | white |
| NotificationEventBG | (114, 166, 82, 162) |
| NotificationMessage | white |
| NotificationMessageBG | (49, 57, 88, 162) |
| NotificationMessageImportant | white |
| NotificationMessageImportantBG | (141, 108, 192, 162) |
| NotificationTutorial | (251, 176, 59) |
| NotificationTutorialBG | (0, 0, 0, 0) |

### Priorities
| Field | RGBA |
|-------|------|
| PrioritiesNeutralColor | (255, 255, 255, 128) |
| PrioritiesLowColor | white |
| PrioritiesHighColor | (184, 113, 145) |

### Status Items
| Field | RGBA |
|-------|------|
| statusItemBad | (244, 74, 71) |
| statusItemEvent | (137, 49, 180) |
| statusItemMessageImportant | (0, 0, 0) |

### Germs
| Field | RGBA |
|-------|------|
| germFoodPoisoning | (255, 231, 47) |
| germPollenGerms | (228, 155, 241) |
| germSlimeLung | (59, 254, 149) |
| germZombieSpores | (148, 183, 255) |
| germRadiationSickness | (134, 226, 86) |

### Rooms
| Field | RGBA |
|-------|------|
| roomNone | (128, 128, 128) |
| roomFood | (255, 226, 132) |
| roomSleep | (163, 255, 132) |
| roomRecreation | (66, 164, 244) |
| roomBathroom | (132, 255, 244) |
| roomHospital | (255, 132, 142) |
| roomIndustrial | (244, 197, 66) |
| roomAgricultural | (205, 242, 72) |
| roomScience | (189, 89, 202) |
| roomBionic | (208, 138, 73) |
| roomPark | (172, 255, 189) |

### Power
| Field | RGBA |
|-------|------|
| powerConsumer | white |
| powerGenerator | (105, 214, 128) |
| powerBuildingDisabled | (128, 128, 128) |
| powerCircuitUnpowered | (78, 79, 221, 0) |
| powerCircuitSafe | (255, 255, 255, 0) |
| powerCircuitStraining | (251, 176, 59, 0) |
| powerCircuitOverloading | (255, 49, 49, 0) |

### Light
| Field | RGBA |
|-------|------|
| lightOverlay | (255, 242, 0, 0) |

### Conduit
| Field | RGBA |
|-------|------|
| conduitNormal | (201, 201, 201, 0) |
| conduitInsulated | (78, 79, 221, 0) |
| conduitRadiant | (231, 193, 68, 0) |

### Temperature
| Field | RGBA |
|-------|------|
| temperatureThreshold0 | (128, 254, 240, 191) |
| temperatureThreshold1 | (43, 203, 255, 191) |
| temperatureThreshold2 | (31, 161, 255, 191) |
| temperatureThreshold3 | (59, 254, 74, 191) |
| temperatureThreshold4 | (239, 255, 0, 191) |
| temperatureThreshold5 | (255, 169, 36, 191) |
| temperatureThreshold6 | (251, 83, 80, 191) |
| temperatureThreshold7 | (251, 2, 0, 191) |
| heatflowThreshold0 | (31, 161, 255, 191) |
| heatflowThreshold1 | (0, 0, 0, 191) |
| heatflowThreshold2 | (234, 21, 21, 191) |
