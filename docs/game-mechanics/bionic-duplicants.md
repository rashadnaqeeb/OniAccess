# Bionic Duplicants

Bionic duplicants are an alternate duplicant model introduced in DLC3 that run on electrobanks instead of food, carry an internal oxygen tank, require oil lubrication, and produce liquid gunk instead of using toilets.

## Model Type

Bionics use `GameTags.Minions.Models.Bionic` and are configured by `BionicMinionConfig`. They share the base minion framework (`BaseMinionConfig`) but replace several biological systems with mechanical equivalents:

- **MAX_CALORIES = 0** -- bionics cannot eat food at all
- **Food Sickness immunity** -- immune to `FoodSickness`
- **Attribute levels capped at 0** -- bionics do not gain attribute experience; they use boosters instead
- **No calorie-based body temperature** -- `WarmBlooded` set to `HomeostasisWithoutCaloriesImpact`

Key state machines registered on bionic duplicants (bionic-specific ones from `BionicMinionConfig.RATIONAL_AI_STATE_MACHINES`, plus shared ones from `BaseMinionConfig`):

| State Machine | Purpose |
|---|---|
| `BionicBatteryMonitor` | Electrobank power management |
| `BionicOilMonitor` | Lubricant tracking |
| `GunkMonitor` | Waste buildup |
| `BionicOxygenTankMonitor` | Internal O2 tank |
| `SuffocationMonitor` | Suffocation/death from no oxygen (from `BaseMinionConfig`) |
| `BionicWaterDamageMonitor` | Water damage stress |
| `BionicUpgradesMonitor` | Booster slot management |
| `BionicBedTimeMonitor` | Sleep-schedule defragmentation |
| `BionicMicrochipMonitor` | Microchip production during bed time |
| `BreathMonitor` | Breath amount (canRecoverBreath = false) |

Source: `BionicMinionConfig.cs`.

## Electrobank System

Bionics consume electrobanks for power instead of eating food. Each electrobank holds **120,000 J** (`ElectrobankConfig.POWER_CAPACITY`) and weighs 20 kg.

### Battery Capacity

| Attribute | Value |
|---|---|
| Default electrobank slots | 4 (`DEFAULT_ELECTROBANK_COUNT`) |
| Extra slots from skill | 2 (`BIONIC_SKILL_EXTRA_BATTERY_COUNT`) |
| Maximum slots | 6 (`MAX_ELECTROBANK_COUNT`) |
| Capacity per bank | 120,000 J |
| Max total charge (4 banks) | 480,000 J |
| Max total charge (6 banks) | 720,000 J |

Capacity is governed by the `BionicBatteryCountCapacity` attribute. Each additional slot beyond the base 4 adds 120,000 J to the max amount.

Source: `BionicBatteryMonitor.cs` lines 207, 510-514.

### Power Consumption

Base wattage is **200 W** (`DEFAULT_WATTS`). Each installed booster adds its own wattage modifier. Difficulty settings also modify wattage:

| Difficulty | Wattage Modifier |
|---|---|
| Very Easy | -150 W |
| Easy | -100 W |
| Default | +0 W |
| Hard | +100 W |
| Very Hard | +200 W |

Power is consumed every tick: `joules = dt * totalWattage`. The battery drains from the lowest-charge electrobank first (banks are sorted by charge ascending).

Source: `BionicBatteryMonitor.DischargeUpdate`, `BionicBatteryMonitor.difficultyWattages`.

### Battery States

The battery monitor has three top-level states:

| State | Condition | Behavior |
|---|---|---|
| `firstSpawn` | Never spawned before | Spawns initial electrobanks (type `DisposableElectrobank_RawMetal`), goes online |
| `online` | Has charge > 0 | Normal operation; subdivides into idle, upkeep, and critical |
| `offline` | Charge = 0 | Incapacitated; tagged `GameTags.Incapacitated`; needs reboot |

**Online substates:**

- `idle` -- normal operation, transitions to upkeep during Eat schedule blocks if banks need replacing
- `upkeep` -- triggers `ReloadElectrobankChore` to fetch and install new electrobanks
- `critical` -- charged bank count <= 1; urgently seeks electrobanks regardless of schedule

**Offline sequence:**

1. `waitingForBatteryDelivery` -- enables `ManualDeliveryKG` for other dupes to deliver charged electrobanks
2. `waitingForBatteryInstallation` -- another dupe must perform `ReanimateBionicWorkable` (a rescue-priority chore restricted to non-robot dupes via `IsNotARobot` precondition)
3. `reboot` -- plays power-up animation, then transitions back to online

**Critical battery notification** triggers when charge falls below **30,000 J** (`ChargeIsBelowNotificationThreshold`).

Source: `BionicBatteryMonitor.InitializeStates`.

### Electrobank Types

| Type | ID | Rechargeable | Notes |
|---|---|---|---|
| Standard | `Electrobank` | Yes | Made of Katairite, 20 kg; when depleted becomes `EmptyElectrobank` |
| Disposable (Metal Ore) | `DisposableElectrobank_RawMetal` | No | Made of Cuprite, 20 kg; destroyed when depleted |
| Disposable (Uranium) | `DisposableElectrobank_UraniumOre` | No | 10 kg; emits 60 RADs; requires Spaced Out DLC |
| Self-Charging | `SelfChargingElectrobank` | N/A | Charges at 60 J/s; has 90,000 s lifetime; explodes on expiry (spawns nuclear waste + radiation) |

Electrobanks take water damage when sitting in liquid (25% chance per second to take 0-1 damage, max health 10; explodes at 0).

Source: `ElectrobankConfig.cs`, `DisposableElectrobankConfig.cs`, `SelfChargingElectrobank.cs`, `Electrobank.cs`.

## Oil System

Bionics have an internal oil tank that depletes over time and must be refilled at an Oil Change Station.

### Oil Parameters

| Parameter | Value | Source |
|---|---|---|
| Oil capacity | 200 kg | `BionicOilMonitor.OIL_CAPACITY` |
| Tank duration | 6000 s (10 cycles) | `BionicOilMonitor.OIL_TANK_DURATION` |
| Base depletion rate | -1/30 kg/s | `BaseOilDeltaModifier` |
| Refill threshold | 20% | `OIL_REFILL_TRESHOLD` |

Oil only depletes while the bionic is online. When offline, the delta modifier is removed.

### Oil Effects

When oil runs out, the bionic gets a penalty effect:

| Effect | Condition | Applied |
|---|---|---|
| `NoLubricationMinor` | Has `EfficientBionicGears` skill perk | Reduced penalty |
| `NoLubricationMajor` | Does not have the perk | Full penalty |

When oil is refilled, a `FreshOil` effect is applied based on the lubricant type:

| Lubricant | Stress Bonus | Morale Bonus | Duration |
|---|---|---|---|
| Tallow | -1/60 /s | +3 | 4800 s (8 cycles) |
| Crude Oil | -1/60 /s | +3 | 4800 s |
| Phyto Oil | -1/120 /s | +2 | 4800 s |

When out of oil, bionics play a `GrindingGears` emote and display a thought bubble.

Source: `BionicOilMonitor.cs`, `OilChangerWorkableUse.cs`.

## Gunk System

Gunk is the bionic equivalent of bladder waste. As oil depletes, an equal mass of gunk accumulates (gunk increases by the absolute value of each oil decrease). The substance is `LiquidGunk`.

### Gunk Parameters

| Parameter | Value | Source |
|---|---|---|
| Gunk capacity | 80 kg | `GunkMonitor.GUNK_CAPACITY` |
| Mild urge threshold | 60% | `Def.SeekForGunkToiletTreshold_InSchedule` |
| Critical urge threshold | 90% | `Def.DesperetlySeekForGunkToiletTreshold` |
| Disease per gunk expulsion | `FoodPoisoning` germs | `DUPLICANTSTATS.BIONICS.Secretions` |

### Gunk States

| State | Trigger | Behavior |
|---|---|---|
| `idle` | Gunk < 60% | No action |
| `mildUrge` | Gunk >= 60% | Seeks Gunk Emptier during Eat or Hygiene schedule blocks |
| `criticalUrge` | Gunk >= 90% | Seeks Gunk Emptier regardless of schedule; applies `GunkSick` effect; slouch animations |
| `cantHold` | Gunk = 100% | Spills gunk on the floor via `BionicGunkSpillChore` |
| `emptyRemaining` | After spill | Expels all remaining gunk; applies `GunkHungover` effect |

Gunk expulsion also removes radiation (up to 300 RADs scaled by the proportion expelled), similar to standard duplicant urination.

Source: `GunkMonitor.cs`.

## Oxygen Tank

Bionics carry an internal oxygen tank rather than breathing from the environment. The tank stores breathable gas and the bionic consumes from it.

### Oxygen Tank Parameters

| Parameter | Value | Source |
|---|---|---|
| Tank capacity (seconds) | 2400 s (4 cycles) | `OXYGEN_TANK_CAPACITY_IN_SECONDS` |
| Tank capacity (kg) | 2400 * OXYGEN_USED_PER_SECOND | `OXYGEN_TANK_CAPACITY_KG` |
| Safe threshold | 85% | `SAFE_TRESHOLD` |
| Critical threshold | 0% | `CRITICAL_TRESHOLD` |
| Initial fill element | Oxygen | `INITIAL_TANK_ELEMENT` |
| Initial fill temperature | Bionic internal ideal temp | `INITIAL_OXYGEN_TEMP` |

### Oxygen Refill Methods

1. **Oxygen canisters** -- bionic seeks the closest oxygen canister item via `ClosestOxygenCanisterSensor` and runs `FindAndConsumeOxygenSourceChore`
2. **Environment absorption** -- if no canister is available, bionic navigates to a breathable cell and absorbs oxygen via `BionicMassOxygenAbsorbChore`

Refill is attempted during Eat schedule blocks when tank is below 85%. When tank reaches 0%, the bionic enters critical mode and seeks oxygen regardless of schedule. The bionic does not emit CO2 (`ShouldEmitCO2` returns false).

### Suffocation

Bionics use the standard `SuffocationMonitor` (registered via `BaseMinionConfig`): when the Breath amount drops to 0, the bionic dies of suffocation. Bionics cannot recover breath (`canRecoverBreath = false` on `BreathMonitor`). Note: `BionicSuffocationMonitor` exists in the codebase but is never registered on any prefab.

Source: `BionicOxygenTankMonitor.cs`, `SuffocationMonitor.cs`, `BaseMinionConfig.cs`.

## Water Damage

`BionicWaterDamageMonitor` applies a `BionicWaterStress` effect when the bionic stands on liquid. Intolerable liquids: Water, Dirty Water, Salt Water, Brine. Wearing an airtight suit prevents the effect. The bionic plays a `WaterDamage` zap emote every 10 seconds while affected.

Source: `BionicWaterDamageMonitor.cs`.

## Bed Time and Microchip Production

During Sleep schedule blocks, bionics run `BionicBedTimeModeChore` (defragmentation). While in bed-time mode:

- The bionic emits a blue light (1800 lux, 3-tile radius)
- `BionicMicrochipMonitor` produces a Power Station microchip every **150 s** (`MICROCHIP_PRODUCTION_TIME`)

Source: `BionicBedTimeMonitor.cs`, `BionicMicrochipMonitor.cs`.

## Booster (Upgrade) System

Bionics use installable booster components instead of skill-based attribute leveling. Managed by `BionicUpgradesMonitor`.

### Booster Slots

| Parameter | Value | Source |
|---|---|---|
| Max possible slots | 8 | `MAX_POSSIBLE_SLOT_COUNT` |
| Unlocked slots | Governed by `BionicBoosterSlots` attribute | `BionicUpgradesMonitor.Instance.UnlockedSlotCount` |

Slots are unlocked via the `BionicBoosterSlots` attribute (modified by skills). Each booster has a wattage cost that increases the bionic's total power consumption.

### Booster Types

Boosters are categorized by `BoosterType`: Basic, Intermediate, Advanced, Sleep, Space, Special. Each booster is a physical item (`BionicUpgradeComponent`) that must be assigned to a bionic and then installed via `SeekAndInstallBionicUpgradeChore`.

Example basic boosters (from `BionicUpgradeComponentConfig`):

| Booster | Attributes | Skill Perks |
|---|---|---|
| `Booster_Dig1` | Digging +5, Athletics +2 | CanDigVeryFirm |
| `Booster_Construct1` | Construction +5, Athletics +2 | CanDemolish |
| `Booster_Carry1` | Strength +5, Athletics +2 | IncreasedCarryBionics |
| `Booster_Research1` | Learning +5, Athletics +2 | (varies) |
| `Booster_Medicine1` | Caring +5, Athletics +2 | (varies) |

Only bionics can have boosters assigned (`AssignablePrecondition_OnlyOnBionics` checks `BionicMinionConfig.MODEL`).

Source: `BionicUpgradeComponent.cs`, `BionicUpgradeComponentConfig.cs`, `BionicUpgradesMonitor.cs`.

## Bionic-Specific Buildings

| Building | ID | Function | Power | Source |
|---|---|---|---|---|
| Electrobank Charger | `ElectrobankCharger` | Charges empty electrobanks at 400 J/s | 480 W input | `ElectrobankChargerConfig.cs` |
| Small Electrobank Discharger | `SmallElectrobankDischarger` | Converts electrobank energy to grid power | 60 W output | `SmallElectrobankDischargerConfig.cs` |
| Large Electrobank Discharger | `LargeElectrobankDischarger` | Converts electrobank energy to grid power | 480 W output | `LargeElectrobankDischargerConfig.cs` |
| Oil Change Station | `OilChanger` | Refills bionic oil from liquid pipe input | 120 W input | `OilChangerConfig.cs` |
| Gunk Emptier | `GunkEmptier` | Bionic toilet; empties gunk to liquid pipe output | No power | `GunkEmptierConfig.cs` |

The Oil Change Station accepts lubricants tagged `LubricatingOil` via liquid conduit (capacity 400 kg). The Gunk Emptier outputs `LiquidGunk` via liquid conduit (capacity 120 kg). Both buildings are bionic-only assignables.

### Electrobank Charger Details

Charges one empty electrobank at a time. Internal charge accumulates at 400 J/s; when it reaches 120,000 J, the empty bank is replaced with a fully charged `Electrobank` (rechargeable type) and dropped. Accepts delivery of `EmptyPortableBattery`-tagged items.

Source: `ElectrobankCharger.cs`, `ElectrobankChargerConfig.cs`.

## Key Classes Reference

| Class | File | Purpose |
|---|---|---|
| `BionicMinionConfig` | `BionicMinionConfig.cs` | Prefab configuration, state machine registration |
| `BionicBatteryMonitor` | `BionicBatteryMonitor.cs` | Electrobank power state machine |
| `BionicOilMonitor` | `BionicOilMonitor.cs` | Oil depletion and refill state machine |
| `GunkMonitor` | `GunkMonitor.cs` | Gunk buildup and expulsion state machine |
| `BionicOxygenTankMonitor` | `BionicOxygenTankMonitor.cs` | Oxygen tank refill state machine |
| `SuffocationMonitor` | `SuffocationMonitor.cs` | Suffocation death logic (shared with standard duplicants) |
| `BionicWaterDamageMonitor` | `BionicWaterDamageMonitor.cs` | Water stress effect |
| `BionicUpgradesMonitor` | `BionicUpgradesMonitor.cs` | Booster slot management state machine |
| `BionicUpgradeComponent` | `BionicUpgradeComponent.cs` | Individual booster item component |
| `BionicUpgradeComponentConfig` | `BionicUpgradeComponentConfig.cs` | Booster definitions and attribute data |
| `BionicBedTimeMonitor` | `BionicBedTimeMonitor.cs` | Sleep/defragmentation state machine |
| `BionicMicrochipMonitor` | `BionicMicrochipMonitor.cs` | Microchip production during bed time |
| `Electrobank` | `Electrobank.cs` | Electrobank charge/discharge, water damage, explosion |
| `SelfChargingElectrobank` | `SelfChargingElectrobank.cs` | Auto-recharging radioactive electrobank |
| `ElectrobankCharger` | `ElectrobankCharger.cs` | Building state machine for charging |
| `ElectrobankDischarger` | `ElectrobankDischarger.cs` | Building state machine for power generation |
