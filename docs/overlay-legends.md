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

No legend entries. Uses a filter menu to select element categories (Metal, Buildable, Liquid, Gas, etc.). Each tile is colored by its element's `substance.uiColour`. All element colors are listed below.

### Solids

| Element | Color (RGBA) |
|---------|-------------|
| Aerogel | (0, 217, 255) |
| Algae | (0, 255, 47) |
| Aluminum | (127, 181, 202) |
| AluminumOre | (204, 67, 36) |
| Amber | (208, 88, 27) |
| Bitumen | (77, 77, 77) |
| BleachStone | (0, 161, 255) |
| Brick | (250, 35, 0) |
| BrineIce | (141, 247, 255) |
| Carbon | (195, 167, 209) |
| CarbonFibre | (96, 96, 96) |
| Cement | (149, 149, 149) |
| CementMix | (161, 161, 161) |
| Ceramic | (236, 214, 174) |
| Cinnabar | (255, 78, 31) |
| Clay | (153, 156, 85) |
| Cobalt | (0, 33, 236) |
| Cobaltite | (0, 33, 236) |
| Copper | (255, 156, 8) |
| Corium | (7, 96, 22) |
| Creature | (71, 69, 6) |
| CrushedIce | (97, 219, 255, 231) |
| CrushedRock | (140, 140, 140) |
| Cuprite | (217, 91, 99) |
| DepletedUranium | (129, 176, 0) |
| Diamond | (203, 197, 233) |
| Dirt | (171, 125, 0) |
| DirtyIce | (126, 204, 214) |
| Electrum | (52, 110, 243) |
| EnrichedUranium | (129, 176, 0) |
| FabricatedWood | (101, 86, 41) |
| Fertilizer | (42, 173, 229) |
| FoolsGold | (255, 163, 13) |
| Fossil | (255, 50, 0) |
| FrozenPhytoOil | (152, 168, 47) |
| Fullerene | (63, 63, 63) |
| Glass | (57, 255, 213) |
| Gold | (255, 201, 26) |
| GoldAmalgam | (255, 201, 26) |
| Granite | (67, 67, 67, 51) |
| Graphite | (67, 67, 67) |
| Gunk | (81, 35, 96) |
| HardPolypropylene | (0, 138, 207) |
| Ice | (0, 238, 255) |
| IgneousRock | (118, 118, 118) |
| Iridium | (176, 160, 186) |
| Iron | (204, 67, 36) |
| IronOre | (204, 67, 36) |
| Isoresin | (255, 163, 0) |
| Katairite | (50, 49, 77) |
| Lead | (255, 201, 26) |
| Lime | (255, 255, 255) |
| MaficRock | (163, 158, 173) |
| MilkFat | (0, 229, 255) |
| MilkIce | (204, 238, 232) |
| Mud | (99, 42, 24) |
| NaturalSolidResin | (156, 95, 18) |
| Nickel | (162, 241, 208) |
| NickelOre | (145, 221, 184) |
| Niobium | (94, 253, 173) |
| Obsidian | (19, 20, 24) |
| OxyRock | (4, 132, 134) |
| Peat | (82, 61, 6) |
| Phosphorite | (200, 61, 128) |
| Phosphorus | (200, 61, 128) |
| PhosphateNodules | (195, 41, 115) |
| Polypropylene | (0, 229, 255) |
| Radium | (129, 176, 0) |
| RefinedCarbon | (67, 67, 67) |
| Regolith | (149, 118, 173) |
| Rust | (188, 80, 79) |
| Salt | (255, 255, 255) |
| Sand | (207, 140, 42) |
| SandCement | (204, 174, 130) |
| SandStone | (152, 130, 53) |
| SedimentaryRock | (70, 114, 204) |
| Shale | (195, 104, 172) |
| Slabs | (122, 140, 180) |
| SlimeMold | (144, 214, 92) |
| Snow | (180, 209, 255) |
| StableSnow | (180, 209, 255) |
| Steel | (81, 82, 43) |
| Sucrose | (255, 255, 255) |
| Sulfur | (255, 255, 0) |
| SuperInsulator | (255, 0, 207) |
| Tallow | (255, 207, 0) |
| TempConductorSolid | (210, 157, 168) |
| ToxicMud | (32, 82, 2) |
| ToxicSand | (34, 99, 0) |
| Tungsten | (255, 247, 0) |
| Unobtanium | (255, 0, 255) |
| UraniumOre | (129, 176, 0) |
| Wolframite | (137, 183, 255) |
| WoodLog | (101, 71, 41) |
| Yellowcake | (255, 240, 0) |
| SolidCarbonDioxide | (31, 31, 31) |
| SolidChlorine | (162, 221, 16) |
| SolidCrudeOil | (0, 0, 0) |
| SolidEthanol | (0, 255, 106) |
| SolidHydrogen | (103, 5, 76, 161) |
| SolidMercury | (49, 72, 176, 215) |
| SolidMethane | (137, 110, 34) |
| SolidNaphtha | (214, 197, 183) |
| SolidNuclearWaste | (129, 176, 0) |
| SolidOxygen | (89, 234, 226) |
| SolidPetroleum | (229, 233, 10) |
| SolidPropane | (121, 144, 209) |
| SolidResin | (156, 95, 18) |
| SolidSuperCoolant | (55, 173, 147) |
| SolidSyngas | (255, 0, 30) |
| SolidViscoGel | (79, 16, 183) |

### Liquids

| Element | Color (RGBA) |
|---------|-------------|
| Brine | (173, 233, 229) |
| Chlorine (liquid) | (58, 195, 48) |
| CrudeOil | (48, 18, 101) |
| DirtyWater | (71, 69, 6) |
| Ethanol | (0, 255, 106) |
| LiquidCarbonDioxide | (135, 142, 183) |
| LiquidGunk | (81, 35, 96) |
| LiquidHelium | (148, 138, 183) |
| LiquidHydrogen | (255, 0, 110) |
| LiquidMethane | (255, 157, 0) |
| LiquidOxygen | (87, 154, 169) |
| LiquidPhosphorus | (144, 139, 183) |
| LiquidPropane | (110, 149, 179) |
| LiquidSulfur | (255, 255, 0) |
| Magma | (174, 0, 0) |
| Mercury | (115, 147, 180) |
| Milk | (204, 238, 232) |
| MoltenAluminum | (95, 152, 173) |
| MoltenCarbon | (128, 128, 128) |
| MoltenCobalt | (0, 29, 206) |
| MoltenCopper | (224, 57, 14) |
| MoltenGlass | (121, 230, 255) |
| MoltenGold | (241, 83, 0) |
| MoltenIridium | (204, 120, 142) |
| MoltenIron | (95, 152, 173) |
| MoltenLead | (241, 83, 0) |
| MoltenNickel | (191, 217, 204) |
| MoltenNiobium | (94, 253, 173) |
| MoltenSalt | (245, 137, 112) |
| MoltenSteel | (238, 44, 81) |
| MoltenSucrose | (255, 255, 255) |
| MoltenSyngas | (0, 255, 116) |
| MoltenTungsten | (229, 41, 14) |
| MoltenUranium | (162, 221, 16) |
| Naphtha | (176, 0, 255) |
| NaturalResin | (204, 139, 24) |
| NuclearWaste | (129, 176, 0) |
| Petroleum | (255, 195, 37) |
| PhytoOil | (152, 168, 47) |
| RefinedLipid | (225, 203, 105) |
| Resin | (204, 139, 24) |
| SaltWater | (206, 210, 236) |
| SugarWater | (255, 255, 255) |
| SuperCoolant | (55, 173, 147) |
| ViscoGel | (79, 16, 183) |
| Water | (0, 99, 255) |

### Gases

| Element | Color (RGBA) |
|---------|-------------|
| AluminumGas | (209, 237, 248) |
| CarbonDioxide | (0, 0, 0) |
| CarbonGas | (186, 162, 198) |
| ChlorineGas | (4, 167, 120) |
| CobaltGas | (0, 33, 236) |
| ContaminatedOxygen | (34, 79, 64) |
| CopperGas | (255, 156, 8) |
| EthanolGas | (0, 255, 106) |
| Fallout | (0, 255, 44) |
| GoldGas | (255, 226, 141) |
| Helium | (200, 159, 185) |
| Hydrogen | (197, 31, 139) |
| IridiumGas | (176, 160, 186) |
| IronGas | (204, 67, 36) |
| LeadGas | (255, 226, 141) |
| Methane | (255, 110, 15) |
| MercuryGas | (77, 109, 202) |
| NickelGas | (162, 241, 208) |
| NiobiumGas | (94, 253, 173) |
| Oxygen | (183, 255, 255) |
| PhosphorusGas | (166, 79, 121) |
| Propane | (121, 144, 209) |
| RockGas | (178, 178, 178) |
| SaltGas | (255, 255, 255) |
| SourGas | (118, 36, 93) |
| Steam | (133, 201, 227) |
| SteelGas | (203, 158, 178) |
| Sulfur Gas | (255, 255, 0) |
| SuperCoolantGas | (55, 173, 147) |
| Syngas | (243, 255, 0) |
| TungstenGas | (255, 0, 125) |

### Special

| Element | Color (RGBA) |
|---------|-------------|
| Vacuum | (120, 91, 137) |
| Void | (255, 255, 0) |
| COMPOSITION | (255, 0, 9) |

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
