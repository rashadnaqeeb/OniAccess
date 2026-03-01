# How the Resource Display Works in ONI

## The Big Picture

ONI tracks every physical resource in the colony: ores, metals, food, seeds, gases, liquids, medicine, and more. The game gives sighted players three ways to see this information, all on the right side of the screen. They range from a quick glance to a full detailed breakdown.

## The Three Views

### 1. The Sidebar (ResourceCategoryScreen)

This is a narrow panel in the top-right corner of the screen, always present during gameplay. It shows resources grouped by category.

**What it looks like collapsed:** A small arrow button. Click it and the whole panel hides to save screen space.

**What it looks like expanded:** A vertical list of category rows. Each row shows:

- The category name (e.g., "Refined Metal", "Edible", "Seeds")
- The total available amount (e.g., "1,250 kg", "42,000 kcal", "8")
- A tiny spark chart showing whether the amount is trending up or down

Categories only appear after the player has discovered at least one resource in that category. So at the start of the game, you might only see a few categories, and more appear as you dig and explore.

**Expanding a category:** Clicking a category row expands it to show individual resources within it. For example, expanding "Metal Ore" might show:

```
Metal Ore          2,400 kg
  Iron Ore         1,500 kg
  Copper Ore         600 kg
  Gold Amalgam       300 kg
```

Each individual resource row has the same format: name, quantity, and a spark chart.

**Clicking a resource:** Clicking an individual resource (like "Iron Ore") cycles through all the piles of that resource in the world, panning the camera to each one. If the resource is inside a storage bin, it focuses on the bin instead. This is the sighted player's way to answer "where is my iron?"

**Hovering:** Hovering over a category or resource highlights all matching items in the game world with a visual glow.

**Tooltips:** Hovering over any row shows a tooltip with richer data:

- Available amount (what's free to use)
- Reserved amount (allocated to pending errands like construction)
- Total amount (available + reserved)
- Trend (increased, decreased, or unchanged since last cycle)

### 2. The Pinned Panel (PinnedResourcesPanel)

This is a smaller panel adjacent to the sidebar. It shows a curated shortlist of resources the player cares about.

**What appears here:**

- Resources the player has explicitly pinned (persistent across save/load)
- Newly discovered resources (marked with a "New" label, auto-expire after 3 cycles)
- Resources with notifications enabled

Each row shows the resource icon, name, and current quantity. There are small toggle buttons to unpin or remove notifications.

**Buttons at the bottom:**

- "See All (N)" -- opens the full resource screen (where N is the count of all discovered resources). This is the main way players reach the full detail view.
- "Clear New" -- dismisses the "new" labels
- "Clear All" -- unpins everything and clears new labels

**Clicking a row:** Same as the sidebar -- cycles through that resource's locations in the world.

### 3. The Full Screen (AllResourcesScreen)

This is the big one. It's a full popup screen (not a small sidebar) that gives complete detail on every resource in the colony.

**How to open it:** There is no hotkey. Players reach it by:

1. Clicking the "See All" button on the Pinned Panel
2. Clicking the "Pin Resource" button on an entity's details panel (which opens AllResourcesScreen filtered to that resource)

**What it shows:** A scrollable list of categories, each expandable. Every resource row shows three columns of data:

| Column | Meaning |
|--------|---------|
| Available | Free to use right now |
| Total | Everything that exists (including what's reserved) |
| Reserved | Allocated to pending errands (construction, fabrication) |

Plus a spark chart for each resource and category.

**Search:** There's a text search field at the top. Typing filters the list in real time. If a category name matches, all its resources show. If individual resource names match, only those resources (and their parent categories) show.

**Pin toggle:** Each resource row has a pin button. Pinning adds it to the Pinned Panel for quick access. This persists across save/load.

**Notification toggle:** Each resource row has a bell button. Enabling notifications means the resource appears in the Pinned Panel even when not explicitly pinned. The game uses this for low-supply warnings.

**Closing:** Escape, right-click, or the close button. Opening any management tab (Vitals, Research, Priorities, etc.) also closes it.

## Categories

Resources are organized into these categories, each with a measurement unit:

**Measured by mass (kg/g/t):**
Alloy, Metal Ore, Refined Metal, Raw Mineral, Refined Mineral, Filtration Medium, Liquifiable, Liquid, Breathable Gas, Unbreathable Gas, Consumable Ore, Sublimating Solid, Organic, Farmable, Agriculture, Other, Manufactured Material, Cooking Ingredient, Rare Materials

**Measured by calories (kcal):**
Edible (food)

**Measured by count:**
Medicine, Medical Supplies, Seed, Egg, Clothing, Industrial Ingredient, Industrial Product, Tech Components, Compostable, High Energy Particle, Bioink

There's also a Miscellaneous catch-all category (measured by mass).

## How the Data Gets There

The data flows through several game systems:

1. **WorldInventory** is the core. Every time a physical item (a `Pickupable`) appears in the world -- mined, delivered, produced, whatever -- it registers with WorldInventory. When it's consumed or destroyed, it unregisters. WorldInventory maintains a dictionary mapping each resource tag to the set of all its physical instances. It sums up amounts by processing one resource type per frame in round-robin fashion, so the full inventory refreshes over many frames (not all at once).

2. **DiscoveredResources** tracks which resources have ever been found. A resource's category row only appears in the UI once something in that category has been discovered. It also tracks "new" discoveries that show up in the Pinned Panel.

3. **MaterialNeeds** tracks reservations. When a duplicant is assigned to build something that needs 200 kg of iron, that 200 kg gets reserved. The "available" number is total minus reserved. This is why you might have 500 kg of iron total but only 300 kg available.

4. **ResourceTracker** records historical data points for spark charts and trend calculations. It samples WorldInventory periodically.

5. **RationTracker** handles the special case of food. Food is measured in calories, not mass, so it needs custom logic to multiply calories-per-unit by the number of units.

## What "Available" vs "Total" vs "Reserved" Mean

- **Total** = every unit of that resource that physically exists on this asteroid
- **Reserved** = how much is spoken for by pending errands (a duplicant is going to pick it up for construction, cooking, etc.)
- **Available** = Total minus Reserved (what's genuinely free for new tasks)

If reserved exceeds total (e.g., you queued more construction than you have materials for), the resource turns a warning color in the UI to signal overallocation.

## Update Timing

The data isn't perfectly real-time. Several things update on staggered schedules:

- WorldInventory processes one resource tag per frame
- ResourceCategoryScreen updates one category per frame
- PinnedResourcesPanel refreshes every 1 second
- AllResourcesScreen rows refresh every 1 second
- Spark charts refresh every 4 seconds

This means there can be brief moments where numbers are slightly stale, but for gameplay purposes it's close enough.

## Multi-World (DLC)

With the Spaced Out DLC, each asteroid has its own WorldInventory. The resource displays show data for the currently active world only. Switching asteroids refreshes the panels to show that world's inventory.

## Persistent State

These things save with the game:

- Which resources are pinned (per world)
- Which resources have notifications enabled (per world)
- Which categories are expanded/collapsed in the sidebar
- Which resources have been discovered (global)

## Connection to Other Screens

- **DetailsScreen:** When you select an entity (like a pile of iron ore), the details panel shows a "Pin Resource" button with the current stockpile. Clicking it opens AllResourcesScreen filtered to that resource.
- **AllDiagnosticsScreen:** Mutually exclusive with AllResourcesScreen. Opening one closes the other.
- **ManagementMenu tabs:** Opening any management tab (Vitals, Research, etc.) closes AllResourcesScreen. Opening AllResourcesScreen closes any open management tab.
- **MeterScreen:** The top bar showing duplicant count, stress, and rations. Adjacent to the resource sidebar but separate.
