# Vacuum and Mass Conservation

How vacuum works and how mass is tracked in the simulation. Derived from decompiled source code.

## Vacuum Cells

Vacuum is a special element (`SimHashes.Vacuum`) representing empty space with zero mass and zero pressure. It is distinct from `SimHashes.Void`, which is a UI/placeholder element used in building logic and never placed in the simulation grid.

**Pressure at vacuum boundaries:**
```
pressure_kPa = mass_kg * 101.3
```
Vacuum cells have exactly 0 pressure. Adjacent gas or liquid cells diffuse into vacuum naturally through the standard pressure-driven flow. There is no special "rush" mechanic; the same pressure gradient logic that moves gas between unequal cells also fills vacuum. Because the pressure difference is maximal (some value vs. 0), flow into vacuum is fast.

## Entombment of Items

Only solid `ElementChunk` pickupables can be entombed. Gases and liquids are always stored as cell data and cannot be entombed.

The `EntombedItemManager` handles burial and release:

**Burial:** When a solid chunk overlaps a solid cell of the same element, and the combined mass is below `maxMass`, the chunk's mass is absorbed into the cell via `SimMessages.AddRemoveSubstance()`. Otherwise, the item is stored separately in an entombed items list with its cell, mass, temperature, and disease data.

**Release:** When a cell transitions from solid to non-solid (mining, melting, etc.), all entombed items at that cell are released as physical `ElementChunk` objects via `Substance.SpawnResource()`. They spawn at the cell position and fall/settle normally. The mass is NOT added directly to the cell grid -- it exists as a pickupable item.

Exception: during save deserialization, if the cell already contains the same element, the entombed mass is absorbed into the cell via `AddRemoveSubstance()` instead of spawning a chunk.

**Minimum mass:** Items must have mass > 0.01 kg to be entombed. Below this threshold, they are ignored.

## Mass Conservation

Mass is strictly conserved in the simulation. There are no silent sinks or sources.

**Mass clamping:** When `SimMessages.ModifyCell()` receives mass exceeding `element.maxMass`, a debug warning is logged. However, the C#-side clamping code contains a logic bug (`if (element.maxMass == 0f && mass > element.maxMass)` -- the condition is only true when maxMass is 0), so clamping for elements with non-zero maxMass (i.e., all real elements) does not actually occur on the C# side. The native sim may enforce its own limits.

**Three cell modification modes:**
1. **None** (AddRemoveSubstance) - Adds or removes mass from a cell. Clamped to maxMass.
2. **Replace** - Replaces the cell's element entirely. Old element is discarded (mass destroyed).
3. **ReplaceAndDisplace** - Replaces the element and physically displaces the old material: solids drop as chunks, liquids become falling particles, gases disperse to neighbors.

Mode 2 (Replace) is the only mechanism that destroys mass. It is used for deliberate replacement operations like building construction.

**ElementConsumers** and negative `AddRemoveSubstance` calls are the standard mechanisms for removing mass from cells. Mass removed from cells always goes somewhere: into building storage, pipe contents, falling particles, or creature inventory.
