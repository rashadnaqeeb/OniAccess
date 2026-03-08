# Storage

How storage buildings work, how items are fetched and delivered, and how stored items interact with temperature and the errand system. Derived from decompiled source code.

## Storage Component

`Storage` (extends `Workable`) is the core component attached to any entity that holds items. Every storage building, fabricator input/output, and duplicant inventory uses this component.

### Key Properties

| Property | Default | Purpose |
|----------|---------|---------|
| `capacityKg` | 20,000 kg | Maximum mass the storage can hold |
| `storageFullMargin` | 0 (0.5 for lockers) | Mass buffer subtracted from capacity when deciding whether to request more deliveries |
| `fetchCategory` | `Building` | Controls fetch eligibility (see below) |
| `allowItemRemoval` | false | Whether duplicants can manually remove items |
| `onlyTransferFromLowerPriority` | false | When true, only fetches from lower-priority sources |
| `storageNetworkID` | -1 | Items are never transferred between storages sharing the same network ID |
| `onlyFetchMarkedItems` | false | The "Sweep Only" toggle |

### FetchCategory

Three categories determine what kind of fetch errands a storage participates in:

- **`Building`** -- default for fabricator inputs. Not affected by the Sweep Only toggle.
- **`GeneralStorage`** -- used by all player-facing storage buildings (bins, fridges, ration boxes, conveyor loaders). Duplicants will fetch any matching loose item.
- **`StorageSweepOnly`** -- when the "Sweep Only" checkbox is enabled on a `GeneralStorage` building, its category switches to this. Only items explicitly marked for sweeping (tagged `Garbage`) are fetched.

The toggle is in `SetOnlyFetchMarkedItems()`. When enabled, the building's sweep icon becomes visible on the sprite.

### Capacity Checking

`MassStored()` sums `PrimaryElement.Units * MassPerUnit` across all held items, rounded to 0.001 kg precision. `RemainingCapacity()` is simply `capacityKg - MassStored()`. `IsFull()` returns true when `RemainingCapacity() <= 0`.

The `storageFullMargin` (0.5 kg for standard lockers) creates a buffer: `FilteredStorage` subtracts this margin when deciding whether to issue new fetch orders, so the building reports "full" slightly before hitting the hard cap. The status item display rounds stored mass up to capacity when within this margin to avoid showing misleading decimals.

### Stored Item Modifiers

When an item enters storage, `ApplyStoredItemModifiers()` applies a configurable set of modifiers. When removed, the same modifiers are reversed.

| Modifier | Effect |
|----------|--------|
| `Hide` | Disables the item's `KAnimControllerBase` and `KSelectable` (invisible, unselectable) |
| `Insulate` | Disables `SimTemperatureTransfer` on the item. The item's temperature freezes at whatever it was when stored |
| `Seal` | Adds `GameTags.Sealed` to the item. Prevents off-gassing (e.g., slime emitting polluted oxygen) |
| `Preserve` | Adds `GameTags.Preserved` to the item. Prevents rot progression |

Each storage building chooses its modifier set:

| Modifier Set | Buildings |
|-------------|-----------|
| `[Hide]` (default) | Storage Bin, Ration Box |
| `[Hide, Seal]` | Standard sealed storage |
| `[Hide, Seal, Insulate]` | Storage Tile |
| `[Hide, Preserve]` | Fabricator storage |

### Disease Transfer

On store and drop, 5% of the item's germs transfer to the storage building, and 5% of the building's germs transfer to the item (`TransferDiseaseWithObject()`). This can be disabled per-storage via `doDiseaseTransfer = false`.

## Storage Building Types

### Storage Bin (`StorageLocker`)

| Property | Value |
|----------|-------|
| Capacity | 20,000 kg (default, no override in config) |
| Size | 1x2 |
| Power | None |
| Filters | `STORAGEFILTERS.STORAGE_LOCKERS_STANDARD` (all solid materials) |
| Modifiers | `[Hide]` (default) |
| Chore type | `StorageFetch` (uses "Storage" skill group) |
| Logic output | None |

Items exchange heat with the environment through the building's cell, not with the bin itself. The bin does not apply `Insulate`, so `SimTemperatureTransfer` remains active on stored items. This means ice can melt inside a bin, releasing water. Items that off-gas (slime, oxylite) continue to do so unless the bottom cell is submerged in liquid.

Implements `IUserControlledCapacity` allowing the player to set a max capacity lower than 20,000 kg via slider.

### Smart Storage Bin (`StorageLockerSmart`)

| Property | Value |
|----------|-------|
| Capacity | 20,000 kg |
| Size | 1x2 |
| Power | 60 W |
| Filters | `STORAGEFILTERS.STORAGE_LOCKERS_STANDARD` |
| Modifiers | `[Hide]` (default) |
| Chore type | `StorageFetch` |
| Logic output | Green when full AND operational |

Extends `StorageLocker`. The logic output sends green (1) only when `filteredStorage.IsFull()` returns true AND the building is operational. If unpowered, the logic port sends red regardless of fill level.

### Ration Box (`RationBox`)

| Property | Value |
|----------|-------|
| Capacity | 150 kg |
| Size | 2x2 |
| Power | None |
| Filters | `STORAGEFILTERS.FOOD` |
| Modifiers | `[Hide]` (default) |
| Chore type | `FoodFetch` (uses "Hauling" skill group) |
| Forbidden tags | `Compostable` |

Implements `IRottable` with `RotTemperature = 277.15 K` (4 C) and `PreserveTemperature = 255.15 K` (-18 C). Does not apply `Insulate` or `Preserve` modifiers, so food continues to exchange heat with the environment and rots at its normal rate. The ration box is available from game start and does not require research. Auto-discovers `FieldRation` as an edible during prefab init.

### Refrigerator

| Property | Value |
|----------|-------|
| Capacity | 100 kg |
| Size | 1x2 |
| Power | 120 W (active cooling) / 20 W (power saver) |
| Filters | `STORAGEFILTERS.FOOD` |
| Modifiers | `[Hide]` (default) |
| Chore type | `FoodFetch` |
| Forbidden tags | `Compostable` |
| Logic output | Green when full AND operational |
| Self-heat | 0.125 kDTU/s (from building def) |
| Cooling exhaust | 0.375 kDTU/s (when actively cooling) |

Uses `RefrigeratorController` state machine with two operational states:

- **Cooling**: Active when any item's temperature >= `simulatedInternalTemperature + activeCoolingStartBuffer` (274.15 + 2 = 276.15 K / 3 C). Draws full 120 W. Exhausts 0.375 kDTU/s into the building's structure temperature.
- **Steady**: Entered when all items are below `simulatedInternalTemperature + activeCoolingStopBuffer` (274.15 + 0.1 = 274.25 K / ~1.1 C). Drops to 20 W power saver mode. Exhausts `steadyHeatKW` (0 kDTU/s in default config).

Cooling is achieved via `SimulatedTemperatureAdjuster` which creates a virtual thermal mass at `simulatedInternalTemperature` (274.15 K / 1 C) with a heat capacity of 400 and conductivity of 1000. This doesn't truly insulate; it actively pulls item temperatures toward 1 C. The fridge does not apply the `Insulate` modifier.

### Object Dispenser

| Property | Value |
|----------|-------|
| Capacity | 20,000 kg (default) |
| Size | 1x2 |
| Power | 60 W |
| Filters | `STORAGEFILTERS.STORAGE_LOCKERS_STANDARD` |
| Modifiers | `[Hide]` (default) |
| Chore type | `StorageFetch` |
| Logic input | Controls open/close |
| Item removal | Not allowed (items dispensed via automation) |

State machine cycles: idle -> load_item -> load_item_pst -> drop_item. When the logic input is green (or manual toggle is on), the dispenser drops all held items at `dropOffset` (1 cell to the right by default, respects rotation). Items are removed from storage and placed in the world.

### Storage Tile (`StorageTile`)

| Property | Value |
|----------|-------|
| Capacity | 1,000 kg |
| Size | 1x1 (tile) |
| Power | None |
| Filters | `STORAGEFILTERS.STORAGE_LOCKERS_STANDARD` |
| Modifiers | `[Insulate, Seal, Hide]` |
| Chore type | `StorageFetch` |
| Construction | 100 kg Refined Metal + 100 kg Glass |

The only standard storage building that applies both `Insulate` and `Seal`. Items inside do not exchange heat with the environment and do not off-gas. Behaves as a foundation tile (walkable, blocks gas/liquid flow).

Stores a single item type at a time. Changing the target item requires a duplicant to perform a `Toggle` chore at the tile. Special size multipliers exist for certain tags: `AirtightSuit` (0.5x), `Dehydrated` (0.6x), `MoltShell` (0.5x).

The `TreeFilterable` on storage tiles has `copySettingsEnabled = false`, `dropIncorrectOnFilterChange = false`, and `preventAutoAddOnDiscovery = true`, so it does not auto-accept newly discovered materials.

## Filtering System

### TreeFilterable

`TreeFilterable` maintains a `HashSet<Tag>` of accepted item tags. This is the player-facing filter UI (the category tree side screen). Key behaviors:

- **Auto-discovery**: When a new resource is discovered and all previously known resources in the same category are already accepted, the new resource is auto-added. Disabled for storage tiles (`preventAutoAddOnDiscovery = true`).
- **Drop on filter change**: When `dropIncorrectOnFilterChange = true` (default), removing a tag from the filter immediately drops any matching items from storage.
- **Forbidden tags**: `ForbiddenTags` are always excluded from accepted tags. Used by food storage to exclude `Compostable` items.

### FilteredStorage

`FilteredStorage` bridges `TreeFilterable` and `Storage`. It owns the `FetchList2` that creates the actual fetch errands.

When the filter changes (`OnFilterChanged`):
1. Cancel any existing `FetchList2`
2. Calculate remaining capacity using `GetMaxCapacityMinusStorageMargin() - GetAmountStored()` (this accounts for the storage full margin)
3. If margin-adjusted capacity > 0 and at least one tag is selected and the building is functional:
   - Recalculate remaining capacity as `GetMaxCapacity() - GetAmountStored()` (without the margin, so the fetch targets the true remaining space)
   - Create a new `FetchList2` targeting the storage
   - Add all accepted tags with the required/forbidden tag constraints
   - Submit the fetch list, which creates `FetchChore` instances

The capacity slider (`IUserControlledCapacity.UserMaxCapacity`) is clamped to `storage.capacityKg`. When the user changes it, `FilterChanged()` is called to recalculate fetch orders.

`FilteredStorage` also manages the visual fill meter and the logic meter (for smart bins and fridges).

## Fetch and Delivery System

### FetchList2

Created by `FilteredStorage` when a storage building needs items. Contains one or more `FetchOrder2` entries, each representing a request for items matching specific tags up to a mass limit. The fetch list creates `FetchChore` instances that duplicants can pick up.

### FetchChore

A `FetchChore` represents a single fetch errand. Key matching logic in `FetchManager.IsFetchablePickup()`:

1. Item must have `UnreservedFetchAmount > 0`
2. If item's `MassPerUnit > 1`, it must not exceed the chore's `originalAmount` (prevents fetching items too heavy for the request; items with MassPerUnit <= 1 skip this check)
3. Item must match tags (by ID or by tag depending on `MatchCriteria`)
4. Item must have `requiredTag` if set
5. Item must not have any `forbiddenTags`
6. Item must not be tagged `MarkedForMove`
7. **Priority gate**: If the pickup is already in a storage, and the destination has `ShouldOnlyTransferFromLowerPriority` (true when `onlyTransferFromLowerPriority` or `allowItemRemoval` is set), the destination's priority must be strictly greater than the source's priority. This prevents items from ping-ponging between equal-priority storages.
8. Items in the same `storageNetworkID` are never transferred between each other

### FetchManager

`FetchManager` is the per-world-instance system that tracks all pickupable items and efficiently matches them to fetch chores. It runs on `ISim1000ms`.

**Pickup sorting** (when priority is included): Items are ranked by:
1. Tag bits hash (grouping)
2. Source priority (higher first)
3. Path cost (lower first)
4. Food quality (higher first)
5. Freshness (higher first, updated every 1000ms from `Rottable.RotValue`)

**Pickup sorting** (no priority): Path cost, then food quality, then freshness.

After sorting, duplicates with the same tag hash and priority are deduplicated, keeping only the best path-cost option per group.

### Chore Types

| Chore Type | Skill Group | Used By |
|------------|-------------|---------|
| `StorageFetch` | Storage | Storage Bin, Smart Storage Bin, Object Dispenser, Storage Tile, Conveyor Loader |
| `FoodFetch` | Hauling | Ration Box, Refrigerator |

Both are in the fetch chore ordering at implicit priority 5000.

## Sweep and Store Commands

### Sweep (Mark for Clearing)

The "Sweep" command (`Clearable.MarkForClear()`) adds `GameTags.Garbage` to the item and registers it with `GlobalChoreProvider.RegisterClearable()`. The clearable manager creates a fetch chore for the item.

A swept item shows a status indicator. `ClearableHasDestination()` checks whether any reachable storage building is currently requesting that item's tag via a `StorageFetch` or `FoodFetch` chore. If no destination exists, the item shows "Pending Clear (No Storage)" instead of "Pending Clear."

When a swept item is stored, `OnStore` fires and calls `CancelClearing()`, removing the garbage tag and unregistering the clearable.

### Interaction with Sweep Only

When a storage bin has "Sweep Only" enabled (`onlyFetchMarkedItems = true`):
- Its `fetchCategory` changes to `StorageSweepOnly`
- Only items tagged `Garbage` (via sweep command) are fetched
- Loose items not explicitly swept are ignored

This is useful for preventing duplicants from constantly hauling debris to bins while still providing a destination for explicitly swept items.

## Conveyor System

### Conveyor Loader (`SolidConduitInbox`)

| Property | Value |
|----------|-------|
| Capacity | 1,000 kg |
| Power | 120 W |
| Self-heat | 2 kDTU/s |
| Filters | `STORAGE_LOCKERS_STANDARD` + `FOOD` (combined) |
| Chore type | `StorageFetch` |
| `onlyTransferFromLowerPriority` | true |
| `allowItemRemoval` | false |

The loader accepts items via fetch errands and feeds them onto the solid conduit rail. Because `onlyTransferFromLowerPriority = true`, items are only fetched from sources with strictly lower priority than the loader. This means a loader at priority 5 will not pull items from a storage bin also at priority 5.

Uses `SolidConduitDispenser` to push items onto the rail network.

### Conveyor Receptacle (`SolidConduitOutbox`)

| Property | Value |
|----------|-------|
| Capacity | 100 kg |
| Power | None |
| `allowItemRemoval` | true |

The receptacle receives items from the rail and makes them available for pickup. Because `allowItemRemoval = true`, its `ShouldOnlyTransferFromLowerPriority` returns true, meaning other storage buildings must have higher priority to pull items from it. The receptacle uses `SimpleVent` to eject items when storage overflows.

Always operational (no power required). Uses `SolidConduitConsumer` to pull items from the rail.

## Priority Integration

Storage priority is managed through the `Prioritizable` component. Each storage building has a `masterPriority` (a `PrioritySetting` with a `priority_value` from 1-9).

Key priority behaviors:

1. **Fetch order priority**: When `FilteredStorage` creates a `FetchList2`, the fetch chores inherit the storage building's priority. Higher-priority storages get serviced first.

2. **Source priority gating**: `ShouldOnlyTransferFromLowerPriority` returns true when either `onlyTransferFromLowerPriority` or `allowItemRemoval` is set. Most player-facing storage buildings have `allowItemRemoval = true`, so they all enforce this rule. This means items in a priority-5 bin will not be fetched into another priority-5 bin. The destination must be strictly higher.

3. **Ignore source priority**: Some storages set `ignoreSourcePriority = true`, bypassing the priority comparison entirely. This is used by storages that should always be able to pull items regardless of where they currently sit.

4. **Storage network isolation**: Storages sharing the same `storageNetworkID` never transfer items between each other, regardless of priority.

5. **Sweep interaction**: When "Sweep Only" is enabled, the storage's fetch category changes and it only responds to items with the `Garbage` tag. Priority still applies for ordering among multiple sweep-only destinations.

6. **Priority change propagation**: When a storage's priority changes (`OnPriorityChanged`), all stored items are notified so that the `FetchManager` can update their `masterPriority` field for pickup sorting.

See also: [priorities-and-errands.md](priorities-and-errands.md) for the broader errand priority system.

## Temperature Interaction Summary

Storage buildings vary significantly in how they handle item temperature:

| Building | Insulate | Seal | Active Cooling | Items Exchange Heat With Environment |
|----------|----------|------|----------------|--------------------------------------|
| Storage Bin | No | No | No | Yes |
| Smart Storage Bin | No | No | No | Yes |
| Ration Box | No | No | No | Yes |
| Refrigerator | No | No | Yes (to 1 C) | Yes (but actively cooled) |
| Object Dispenser | No | No | No | Yes |
| Storage Tile | Yes | Yes | No | No |

Only the Storage Tile truly insulates its contents. All other storage buildings leave `SimTemperatureTransfer` enabled, meaning items exchange heat with the building's cell. The Refrigerator counteracts this by actively cooling items toward 1 C via a simulated thermal mass, but it does not prevent heat exchange -- it just wins the thermal race while powered.

Items that emit gas or liquid (slime, oxylite, bleach stone) continue to do so in non-sealed storage. Submerging the bottom cell of the storage building in liquid blocks off-gassing for buildings that don't apply `Seal`.
