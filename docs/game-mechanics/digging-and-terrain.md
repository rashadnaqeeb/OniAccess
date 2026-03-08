# Digging and Terrain

How digging works in ONI: hardness tiers, dig time calculations, skill requirements, mining yields, entombment, and undiggable elements. Derived from decompiled source code.

## Hardness Tiers

Every solid element has a `hardness` byte (0-255). The game groups hardness into named tiers with corresponding skill requirements. The tier constants are defined in `GameUtil.Hardness` and the display labels in `STRINGS.ELEMENTS.HARDNESS`.

| Tier | Hardness Range | Display Label | Skill Perk Required |
|------|---------------|---------------|---------------------|
| Very Soft | 0-9 | "Very Soft" | None |
| Soft | 10-24 | "Soft" | None |
| Firm | 25-49 | "Firm" | None |
| Very Firm | 50-149 | "Very Firm" | CanDigVeryFirm |
| Nearly Impenetrable | 150-199 | "Nearly Impenetrable" | CanDigNearlyImpenetrable |
| Super Duper Hard | 200-250 | (no distinct label, shows "Nearly Impenetrable") | CanDigSuperDuperHard |
| Radioactive Materials | 251-254 | (no distinct label) | CanDigRadioactiveMaterials |
| Impenetrable | 255 | "Impenetrable" | CanDigUnobtanium (impossible) |

Source: `GameUtil.Hardness` constants (GameUtil.cs:107-121), `GameUtil.AppendHardnessString()` (GameUtil.cs:1918-1966), `Diggable.OnSolidChanged()` (Diggable.cs:155-237).

Note on display labels: `AppendHardnessString` uses only five visual thresholds (0, 10, 25, 50, 150, 255), so hardness 200+ and 251+ elements display as "Nearly Impenetrable" even though they require higher-tier skills. The distinction only matters for skill gating, not for the label shown to the player.

## Skill Requirements by Tier

`Diggable.OnSolidChanged()` checks the element hardness and adds a `HasSkillPerk` precondition to the dig chore. The thresholds:

- **hardness < 50**: No skill required. Any duplicant can dig.
- **hardness >= 50**: Requires `CanDigVeryFirm` (Mining skill tier 1, "Apprentice Miner").
- **hardness >= 150**: Requires `CanDigNearlyImpenetrable` (Mining tier 2, "Miner").
- **hardness >= 200**: Requires `CanDigSuperDuperHard` (Mining tier 3, "Master Miner").
- **hardness >= 251**: Requires `CanDigRadioactiveMaterials` (Mining tier 4, "Atomic Miner", DLC1 only).
- **hardness == 255**: Adds `CanDigUnobtanium` precondition, which no skill tree grants. The dig errand is immediately cancelled in `OnSpawn()`.

Source: Diggable.cs:88-91 (OnSpawn cancel), Diggable.cs:155-237 (OnSolidChanged skill checks).

### Mining Skill Tree

Each mining skill level provides a digging attribute bonus (+2 per tier) and unlocks the next hardness tier:

| Skill | Name | Attribute Bonus | Unlocks |
|-------|------|-----------------|---------|
| Mining1 | Apprentice Miner | Digging +2 | CanDigVeryFirm |
| Mining2 | Miner | Digging +2 | CanDigNearlyImpenetrable |
| Mining3 | Master Miner | Digging +2 | CanDigSuperDuperHard |
| Mining4 | Atomic Miner | (none) | CanDigRadioactiveMaterials |

Source: Skills.cs:115-130, SkillPerks.cs:223-230, TUNING/ROLES.cs:19-23 (ATTRIBUTE_BONUS values = 2).

## Dig Time Formula

The dig time for a cell is calculated by `Diggable.GetApproximateDigTime(int cell)` (Diggable.cs:325-337):

```
hardness = Grid.Element[cell].hardness       (cast to float)
if hardness == 255: return float.MaxValue     (undiggable)

iceHardness = ElementLoader.FindElementByHash(SimHashes.Ice).hardness
hardnessRatio = hardness / iceHardness

massFraction = Min(Grid.Mass[cell], 400) / 400
baseTime = 4 * massFraction

approximateDigTime = baseTime + hardnessRatio * baseTime
                   = baseTime * (1 + hardnessRatio)
                   = 4 * massFraction * (1 + hardness / iceHardness)
```

Ice's hardness is the reference point used to normalize all other elements. The mass contribution is capped at 400 kg, so cells with more than 400 kg of material take the same time as a 400 kg cell.

### Actual Dig Speed (with worker efficiency)

The approximate dig time is the *base* time assuming an efficiency multiplier of 1.0. The actual work flow is:

1. `StandardWorker.Work()` calls `Workable.GetEfficiencyMultiplier(worker)` each tick (StandardWorker.cs:232-233).
2. The efficiency multiplier starts at 1.0 and adds the worker's `DiggingSpeed` attribute converter result. The converter is `0.25 * Digging_attribute_level` (AttributeConverters.cs:76). So a dupe with Digging 8 gets 1.0 + (0.25 * 8) = 3.0x efficiency.
3. If `lightEfficiencyBonus` is true and the cell is lit, an additional bonus is added (Workable.cs:426-429).
4. The raw `dt` is multiplied by efficiency: `dt2 = dt * efficiencyMultiplier` (StandardWorker.cs:233).
5. `Workable.WorkTick()` calls `Diggable.OnWorkTick()`, which calls `DoDigTick(cell, dt2)`.
6. `DoDigTick` divides `dt2` by `GetApproximateDigTime(cell)` to get the damage fraction, then applies it via `WorldDamage.ApplyDamage()` (Diggable.cs:318-323).
7. `WorldDamage` accumulates `Grid.Damage[cell]` from 0.0 to 1.0. When it reaches 1.0, the cell is destroyed via `SimMessages.Dig()` (WorldDamage.cs:92-96, 202-207).

The Diggable component uses `multitoolContext = "dig"` for hardness < 50 and `"specialistdig"` for hardness >= 50, which changes the digging animation (Diggable.cs:448-461).

## Mining Yield

When a cell is fully dug out, `WorldDamage.OnDigComplete()` is called (WorldDamage.cs:215-231):

```
droppedMass = cellMass * 0.5
```

**You get exactly 50% of the original cell mass as a resource drop.** The dropped item spawns at a random position within the cell, retains the element type, temperature, and disease of the original cell.

Source: WorldDamage.cs:221 (`float num = mass * 0.5f`).

## Auto-Miner (Robo-Miner)

The Auto-Miner (`AutoMiner`) can only dig cells with hardness < 150. It cannot dig Nearly Impenetrable or harder materials regardless of any skills. It also cannot dig through foundations or closed doors.

Source: AutoMiner.cs:141 (`Grid.Element[cell].hardness >= 150` blocks digging), AutoMiner.ValidDigCell (AutoMiner.cs:356-377).

The Auto-Miner uses the same `Diggable.DoDigTick()` for damage calculation, but passes `DamageType.NoBuildingDamage` so it does not damage buildings in the way (AutoMiner.cs:259).

## Undiggable Elements

`Diggable.Undiggable()` returns true only for `SimHashes.Unobtanium` (the game's name for Neutronium):

```csharp
public static bool Undiggable(Element e) => e.id == SimHashes.Unobtanium;
```

Source: Diggable.cs:396-399.

When a Diggable is placed on a cell with hardness 255 (byte.MaxValue), `OnSpawn()` immediately calls `OnCancel()` to remove the dig errand (Diggable.cs:88-91). The `OnSolidChanged` handler also adds the `CanDigUnobtanium` skill perk precondition, which no duplicant can ever acquire from the skill tree (Diggable.cs:158-160).

Neutronium forms the impenetrable borders at the bottom of the map and around geysers/vents, preventing the player from ever mining through them.

## Entombment

ONI has two distinct entombment systems for different entity types.

### Item Entombment (EntombedItemManager)

When a solid element chunk (a `Pickupable` with an `ElementChunk` component) is in a cell that becomes solid, it can be entombed. `EntombedItemManager.CanEntomb()` checks (EntombedItemManager.cs:52-80):

- The pickupable is not in storage
- Its cell is valid and solid
- No building occupies the cell at layer 9
- It is a solid element with an `ElementChunk` component

If the entombed item's element matches the cell's element and the combined mass would not exceed the element's max mass, the item's mass is merged into the cell. Otherwise, the item is recorded and a visual indicator is placed.

When the cell becomes non-solid again (`OnSolidChanged`), the entombed items are spawned back as pickupables (EntombedItemManager.cs:128-154, 219-231).

### Building Entombment (Structure)

Buildings use the `Structure` component to detect entombment. `Structure.IsBuildingEntombed()` checks whether any of the building's placement cells contains a solid element that is not a foundation tile (Structure.cs:32-47):

```csharp
Grid.Element[cell].IsSolid && !Grid.Foundation[cell]
```

When entombed, the building gets the `GameTags.Entombed` tag and its `Operational` flag `not_entombed` is set to false, which disables the building.

### Creature/Plant Entombment (EntombVulnerable)

Plants and creatures use `EntombVulnerable`, which checks whether all cells in the entity's `OccupyArea` are non-solid (EntombVulnerable.cs:133-145). If any occupied cell is solid, the entity is considered entombed. The `WiltCondition.Condition.Entombed` causes plants to wilt.

### Duplicant Entombment (EntombedChore)

When a duplicant is trapped in solid tiles, `EntombedChore` plays entombment animations. It checks whether the cell above is solid to determine the animation: `entombed_ceiling` (face entombed from above) or `entombed_floor` (body entombed from below). The chore has `PriorityClass.compulsory` priority, overriding all other tasks (EntombedChore.cs:48).

## Dig Diggability Check

`Diggable.IsDiggable(int cell)` determines whether a cell can have a dig errand placed on it (Diggable.cs:349-356):

- If the cell is solid and not a foundation, it is diggable.
- If the cell is not solid, it checks for unstable solid cells above (falling sand/regolith) that would fill it in.

## Unstable Elements and Falling

When a dig target cell becomes non-solid, the code checks for unstable elements above via `GetUnstableCellAbove()` (Diggable.cs:358-389). It walks upward from the dug cell looking for:
- Cells already tracked as falling by `UnstableGroundManager`
- Solid cells with the `IsUnstable` tag (e.g., Sand, Regolith)

If an unstable cell is found above, the Diggable persists and waits for the material to finish falling before marking completion. It rechecks every 2 seconds via a coroutine (Diggable.cs:140-144).
