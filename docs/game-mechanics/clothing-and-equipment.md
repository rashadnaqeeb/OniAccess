# Clothing and Equipment

Covers wearable clothing, exosuits, oxygen masks, suit dock infrastructure, durability, and repair. Derived from decompiled source (`ClothingWearer`, `Equippable`, `Equipment`, `Durability`, `SuitTank`, `JetSuitTank`, `LeadSuitTank`, `SuitLocker`, `SuitMarker`, suit/vest config classes, `TUNING.EQUIPMENT`).

## Equipment Slots

Duplicants have two wearable equipment slots:

- **Outfit** (`EQUIPMENT.CLOTHING.SLOT`): clothing (vests). Purely stat-based, no infrastructure required.
- **Suit** (`EQUIPMENT.SUITS.SLOT`): exosuits and oxygen masks. Require dock/checkpoint buildings to equip and unequip.

A duplicant can wear one item in each slot simultaneously. Clothing occupies Outfit, suits occupy Suit.

## Clothing (Vests)

All clothing is manufactured at the **Textile Loom** (`ClothingFabricator`). Clothing applies two modifiers via `ClothingWearer`: a **decor** aura (3-tile radius around the duplicant) and a **thermal conductivity barrier** that insulates the duplicant from ambient temperature. The `homeostasisEfficiencyMultiplier` field exists in `ClothingInfo` but is not applied in `ChangeClothes()` -- only decor and conductivity barrier take effect.

Default clothing (worn by all duplicants at spawn) uses `BASIC_CLOTHING` stats: -5 decor, +0.0025 conductivity barrier.

### Clothing Stats

| Item | ID | Decor | Conductivity Barrier | Fab Time (s) | Mass (kg) | Ingredients |
|------|-----|-------|---------------------|--------------|-----------|-------------|
| Basic Clothing | (default) | -5 | +0.0025 | -- | -- | Automatic at spawn |
| Warm Sweater | `Warm_Vest` | 0 | +0.008 | 180 | 4 | 4 kg Fabric |
| Snazzy Suit | `Funky_Vest` | +30 | +0.0025 | 180 | 4 | 4 kg Fabric |
| Custom Clothing | `CustomClothing` | +40 | +0.0025 | 180 | 7 | 1 Snazzy Suit + 3 kg Fabric |

The Warm Sweater provides the highest thermal insulation (+0.008 conductivity barrier), making it useful for duplicants working near temperature extremes. The Snazzy Suit and Custom Clothing provide significant decor bonuses. Custom Clothing is crafted at the **Clothing Alteration Station** (`ClothingAlterationStation`) which requires the Clothing Alteration skill perk. The station consumes a Snazzy Suit plus 3 kg Fabric to produce a Custom Clothing item with a cosmetic facade.

There is no Cool Vest config class in the decompiled source. `ClothingInfo.COOL_CLOTHING` is defined (decor -10, conductivity barrier +0.0005) but no corresponding `IEquipmentConfig` creates it. The TUNING values `COOL_VEST_MASS` and `COOL_VEST_FABTIME` exist but are unused.

### Clothing Equip/Unequip

Clothing uses the standard `Equippable` assignment system. When equipped, `ClothingInfo.OnEquipVest()` calls `ClothingWearer.ChangeClothes()`, which sets the decor and conductivity barrier modifiers on the duplicant. When unequipped, `OnUnequipVest()` reverts to default clothing. Clothing has no durability -- it lasts forever.

## Exosuits

Exosuits are manufactured at the **Exosuit Forge** (`SuitFabricator`, 480W). All exosuits are airtight (tag `AirtightSuit`), provide environmental protection, and have durability that degrades over time. They occupy the Suit slot and require dock/checkpoint infrastructure to equip.

### Exosuit Attribute Modifiers

All values from `TUNING.EQUIPMENT.SUITS`:

| Modifier | Atmo Suit | Jet Suit | Lead Suit | Oxygen Mask |
|----------|-----------|----------|-----------|-------------|
| Athletics | -6 | -6 | -8 | -2 |
| Insulation | +50 | +50 | +50 | -- |
| Thermal Cond. Barrier | +0.2 | +0.2 | +0.3 | -- |
| Digging | +10 | +10 | -- | -- |
| Scalding Threshold | +1000 | +1000 | +1000 | -- |
| Scolding Threshold | -1000 | -1000 | -1000 | -- |
| Radiation Resistance | -- | -- | +0.66 (66%) | -- |
| Strength | -- | -- | +10 | -- |
| Mass (kg) | 200 | 200 | 200 | 15 |

The **Exosuit Expertise** skill perk (`ExosuitExpertise`) applies an offsetting Athletics modifier on equip: +6 for Atmo Suit, Jet Suit, and Lead Suit, +2 for Oxygen Mask. This fully negates the penalty for Atmo and Jet suits but only partially offsets the Lead Suit's -8 penalty (net -2 with perk).

### Effect Immunities

Atmo Suit, Jet Suit, and Lead Suit all grant immunity to:
- SoakingWet
- WetFeet
- ColdAir
- WarmAir
- PoppedEarDrums
- RecentlySlippedTracker (slip immunity)

Oxygen Masks grant no effect immunities.

Airtight suits (Atmo, Jet, Lead) store CO2 internally instead of emitting it. Non-airtight suits (Oxygen Mask) emit CO2 normally. The `SuitTank.ShouldStoreCO2()` / `ShouldEmitCO2()` methods check the `AirtightSuit` tag.

### Atmo Suit

**ID**: `Atmo_Suit`
**Recipe**: 300 kg Refined Metal + 2 kg Fabric (40s fab time)
**Oxygen tank**: 75 kg capacity (`OXYGEN_USED_PER_SECOND * 600 * 1.25 = 0.1 * 600 * 1.25`)
**Navigator flag**: `HasAtmoSuit`

The standard exosuit. Provides full environmental protection, +10 Digging, and extreme temperature thresholds. Stores oxygen for breathing and CO2 as waste.

### Jet Suit

**ID**: `Jet_Suit`
**Recipe**: 200 kg Steel + 2 kg Fabric (40s fab time)
**Oxygen tank**: 75 kg capacity (same formula as Atmo Suit)
**Fuel tank**: 100 kg capacity (`JetSuitTank.FUEL_CAPACITY`), burn rate 0.2 kg/s, emits CO2 at 0.25 kg per kg fuel burned at 373.15 K
**Navigator flag**: `HasJetPack`
**Fuel refill threshold**: 20% (`JetSuitTank.REFILL_PERCENT`)

Identical protection to the Atmo Suit plus the ability to hover/fly. Requires petroleum or other combustible liquid for fuel. When fuel is empty, the `JetSuitOutOfFuel` tag is applied and the duplicant reverts to floor navigation.

### Lead Suit (Spaced Out DLC)

**ID**: `Lead_Suit`
**Recipe**: 200 kg Lead + 10 kg Glass (40s fab time)
**Oxygen tank**: 40 kg capacity (`OXYGEN_USED_PER_SECOND * 400 = 0.1 * 400`)
**Battery**: 200s duration (`LeadSuitTank.batteryDuration`), charge stored as 0.0--1.0 float
**Navigator flag**: `HasLeadSuit`
**Battery refill threshold**: 25% (`LeadSuitTank.REFILL_PERCENT`)
**Battery recharge time**: 60s from empty to full (`LeadSuitLocker.batteryChargeTime`)
**Cooling threshold**: 333.15 K (60 C) (`LeadSuitTank.coolingOperationalTemperature`)

Provides 66% radiation shielding and +10 Strength, but has the heaviest Athletics penalty (-8) and a smaller oxygen tank than other airtight suits. The battery powers active cooling. When the battery is depleted, `SuitBatteryLow` and eventually `SuitBatteryOut` tags are applied.

### Oxygen Mask

**ID**: `Oxygen_Mask`
**Crafted at**: Crafting Station (`CraftingTable`), using 50 kg Raw Metal Ore. Also available at the deprecated Oxygen Mask Station
**Oxygen tank**: 20 kg capacity (hardcoded)
**Mass**: 15 kg
**Navigator flag**: `HasOxygenMask`
**Fab time**: 20s (`SUITS.OXYMASK_FABTIME`)

The lightest and least protective option. Provides only breathable oxygen and a small Athletics penalty (-2). No insulation, no temperature protection, no effect immunities. Not airtight -- emits CO2 normally. The Oxygen Mask Station building is deprecated; masks are now fabricated at the Crafting Station.

Note: Oxygen Mask lockers (`OxygenMaskLocker`) use raw metals for construction and only hold 30 kg oxygen (vs 200 kg for Atmo/Jet suit lockers), matching the mask's smaller tank.

## Suit Tank Mechanics

All suits with oxygen use `SuitTank`, which implements `OxygenBreather.IGasProvider`:

- **Recharge threshold**: tanks request recharging at 25% (`SuitTank.REFILL_PERCENT`)
- **Minimum usable charge**: 95% (`EQUIPMENT.SUITS.MINIMUM_USABLE_SUIT_CHARGE`). A suit in a locker must be at least 95% charged to be considered usable by the checkpoint
- **Locker charge rate**: `capacity * 15 * dt / 600` per tick, meaning it takes 40 seconds to fully charge any oxygen tank from empty (`600 / 15 = 40`)
- When equipped, the suit becomes the duplicant's gas provider and the `HasSuitTank` tag is applied
- Safe cell flags are modified so the duplicant does not seek breathable cells while suited

## Durability and Repair

All suits (including oxygen masks) have durability. Durability starts at 1.0 (100%) and decays while equipped.

### Decay Rates

| Suit | Decay Per Cycle | Cycles Until Worn Out |
|------|----------------|----------------------|
| Atmo Suit | -0.1 (10%) | 10 |
| Jet Suit | -0.1 (10%) | 10 |
| Lead Suit | -0.1 (10%) | 10 |
| Oxygen Mask | -0.2 (20%) | 5 |

Durability loss is calculated on unequip: `(currentTime - timeEquipped) * durabilityLossPerCycle`. The `Durability` component tracks time equipped in cycles.

### Difficulty Modifiers

The custom game setting `Durability` applies a multiplier to decay:

| Setting | Multiplier | Atmo/Jet/Lead Lifespan | Mask Lifespan |
|---------|-----------|----------------------|---------------|
| Indestructible | 0.0x | Infinite | Infinite |
| Reinforced | 0.5x | 20 cycles | 10 cycles |
| (Default) | 1.0x | 10 cycles | 5 cycles |
| Flimsy | 1.5x | ~6.7 cycles | ~3.3 cycles |
| Threadbare | 2.0x | 5 cycles | 2.5 cycles |

### Suit Durability Skill Bonus

The `ExosuitDurability` skill perk grants a 25% durability bonus (`SUITS.SUIT_DURABILITY_SKILL_BONUS = 0.25`). On unequip, if the duplicant has this perk, the effective time equipped is reduced by 25%, extending suit lifespan by one-third (from 10 to ~13.3 cycles for standard suits).

### Worn-Out Suits

When durability reaches 0, the suit becomes "worn out" on unequip. `Durability.ConvertToWornObject()` instantiates a worn prefab (e.g., `Worn_Atmo_Suit`) and destroys the original. Worn suits are not usable. The locker automatically requests a new suit when a worn suit is returned.

### Repair Recipes

Worn suits can be repaired:

| Worn Item | Ingredients | Fab Time |
|-----------|-------------|----------|
| Worn Atmo Suit | 1 Worn Atmo Suit + 1 kg Fabric | 40s |
| Worn Jet Suit | 1 Worn Jet Suit + 1 kg Fabric | 40s |
| Worn Lead Suit | 1 Worn Lead Suit + 5 kg Glass | 40s |
| Worn Oxygen Mask | 1 Worn Oxygen Mask (no additional materials) | 20s |

Suit repair recipes are crafted at the Exosuit Forge. Oxygen Mask repair is at the Crafting Station. All repair recipes inherit the element of the worn suit and preserve facade data via `RepairableEquipment.facadeID`.

## Suit Dock/Checkpoint System

Suits are not directly equipped by duplicants. The dock system consists of **lockers** (store and recharge suits) and **checkpoints/markers** (trigger equip/unequip as duplicants pass).

### Building Types

| Building | ID | Size | Power | Gas Input | Locker O2 Cap | Suit Type |
|----------|----|------|-------|-----------|---------------|-----------|
| Suit Dock | `SuitLocker` | 1x3 | 120W | Gas pipe | 200 kg | Atmo Suit |
| Jet Suit Dock | `JetSuitLocker` | 2x4 | 120W | Gas + Liquid | 200 kg | Jet Suit |
| Lead Suit Dock | `LeadSuitLocker` | 2x4 | 120W | Gas pipe | 80 kg | Lead Suit |
| Oxygen Mask Dock | `OxygenMaskLocker` | 1x2 | None | Gas pipe | 30 kg | Oxygen Mask |
| Suit Checkpoint | `SuitMarker` | 1x3 | None | -- | -- | Atmo Suit |
| Jet Suit Checkpoint | `JetSuitMarker` | 2x4 | None | -- | -- | Jet Suit |
| Lead Suit Checkpoint | `LeadSuitMarker` | 2x4 | None | -- | -- | Lead Suit |
| Oxygen Mask Checkpoint | `OxygenMaskMarker` | 1x2 | None | -- | -- | Oxygen Mask |

All checkpoints accept a logic input for operational control and can be flipped horizontally. All dock types use refined metals except Oxygen Mask (raw metals). Jet Suit Docks have a secondary liquid conduit input for fuel (petroleum or other combustible liquid, 100 kg capacity).

### How Docks and Checkpoints Work

1. **Placement**: Lockers must be placed adjacent to a checkpoint on the correct side. The checkpoint scans left or right (based on rotation) for contiguous lockers. `SuitLocker.UpdateSuitMarkerStates()` validates the marker-locker relationship. Lockers without an adjacent checkpoint display a "No Suit Marker" status. Lockers on the wrong side of the checkpoint display "Suit Marker Wrong Side".

2. **Locker states** (`SuitLocker.States`):
   - **Empty**: no suit stored. Sub-states: `notconfigured` (needs player to configure) and `configured` (ready).
   - **Waiting for suit**: locker is configured and has created a fetch chore for a suit.
   - **Charging**: suit is stored and being recharged. Sub-states: `operational` (actively charging), `nooxygen` (no oxygen supply), `notoperational` (building disabled/depowered).
   - **Fully charged**: suit is ready for use.

3. **Equip (passing through checkpoint toward restricted area)**: When a duplicant walks through a checkpoint in the "equip direction" (toward the locker side), the `EquipSuitReactable` fires. It selects the best available locker (preferring fully charged suits, falling back to the highest-charged suit above 95%) and calls `SuitLocker.EquipTo()`, which drops the suit from storage, assigns it to the duplicant, and equips it.

4. **Unequip (passing through checkpoint away from restricted area)**: The `UnequipSuitReactable` fires. It finds an available empty locker via `CanDropOffSuit()` and calls `SuitLocker.UnequipFrom()`. If no empty locker is available, the suit is unassigned and dropped on the ground with a "Suit Dropped" notification.

5. **Return suit chores**: Lockers create two return-suit chores: an urgent one (`ReturnSuitUrgent`, personal needs priority) that triggers when the suit needs recharging, and an idle one (`ReturnSuitIdle`) for when the duplicant is idle. Both require a valid suit marker and matching suit type.

6. **Traversal policy**: Checkpoints have a toggleable setting via user menu:
   - **"Always traverse"**: duplicants can pass the checkpoint even without equipping/unequipping (default).
   - **"Only when room available"**: duplicants can only pass when there is an available locker to drop off their suit (`OnlyTraverseIfUnequipAvailable`). This prevents duplicants from wearing suits past the checkpoint when all lockers are full.

7. **Jet Suit Dock specifics**: `JetSuitLocker` manages both oxygen and fuel charging. Fuel is pumped from the secondary liquid input at 10 kg/s. The fuel meter and oxygen meter are displayed separately.

8. **Lead Suit Dock specifics**: `LeadSuitLocker` manages oxygen and battery charging. The battery charges at `dt / 60` per tick, meaning 60 seconds from empty to full.

## Disease Transfer

When a suit is equipped or unequipped, 33% of germs transfer between the duplicant and the suit (`Equipment.Equip()` / `Equipment.Unequip()`). Suits also have a `SuitDiseaseHandler` component for ongoing disease management while worn.

## Equip Animation

The checkpoint equip/unequip interaction takes 2.8 seconds (`SuitMarkerReactable.Update()` checks `Time.time - startTime > 2.8f`). During this time, the duplicant plays working_pre, working_loop, working_pst animations with the checkpoint's interact anim overlay.
