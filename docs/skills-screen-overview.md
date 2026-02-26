# The Skill Screen: How It Works

A plain-language companion to `skills-screen-research.md`. Describes what a sighted player sees and does, what information is available at every point, and how the underlying systems connect.

## What the skill screen is for

The skill screen is where you spend skill points to teach duplicants new abilities. Every duplicant passively earns experience over time. When enough XP accumulates, they gain a skill point. You spend skill points to unlock skills, which grant concrete gameplay benefits: the ability to dig harder materials, operate advanced buildings, carry more, cook better food, and so on.

Skills are organized into a tree. Later skills require earlier ones. Learning skills raises a duplicant's morale expectations, so there is a real cost: a duplicant loaded with skills needs better food, nicer rooms, and more recreation to stay happy. If morale drops below expectations, the duplicant gets stressed.

## Opening the screen

The skill screen opens from the management menu bar at the top of the screen (hotkey: the ManageSkills action). It requires at least one Printing Pod (or similar role station) to exist in the colony. Without one, the button is grayed out.

The screen is modal. It pauses the game and covers everything else. Escape closes it.

## Screen layout

The screen has three main areas:

### Left sidebar: duplicant list

A vertical list of every duplicant in the colony. Each entry shows:

- The duplicant's portrait
- Available skill points (how many unspent points they have). Shown in green bold if they have points to spend, "0" otherwise
- Morale ratio in "current/need" format (e.g., "22/18" means 22 morale, 18 needed)
- A small hat icon showing their current hat

In Spaced Out, duplicants are grouped by asteroid with dividers between groups. Duplicants stored in rockets or other buildings appear faded with "N/A" for skill points and morale.

The list can be sorted three ways via buttons at the top: by name (alphabetical), by morale, or by available skill points. Clicking the same sort button again reverses the order.

Clicking a duplicant selects them. The entire rest of the screen updates to show that duplicant's information.

### Header area: selected duplicant summary

Shows detailed stats for whoever is selected:

- **Animated portrait** of the duplicant
- **Morale bar** with notch marks. Green notches show current morale. When you hover over a skill, the bar previews what the new morale need would be if you learned it
- **Morale label**: "Morale: X"
- **Morale Need label**: "Morale Need: X"
- **Warning indicator**: green icon if morale is safely above need, red icon if morale is below need (duplicant is stressed or will be)
- **XP progress bar**: shows "X / Y" progress toward the next skill point
- **Skill points available**: the number of unspent skill points
- **Hat dropdown**: lets you pick which hat the duplicant wears from the hats they have earned

Hovering over the morale area shows a detailed tooltip breaking down every modifier contributing to morale (food quality, room bonuses, recreation, etc.) and every modifier contributing to morale need (skills learned, job expectations, etc.).

For stored duplicants, most of this is hidden or shows "N/A".

### Main area: the skill tree

The bulk of the screen is a scrollable grid of skills arranged as a tree:

- **Columns** represent tiers (difficulty levels). Tier 0 is on the left, higher tiers to the right
- **Rows** represent skill groups (categories like Mining, Building, Farming, etc.)
- Lines connect skills to their prerequisites, showing the tree structure visually

Each skill is a rectangular widget showing:

- The skill name, with the group name in parentheses below (e.g., "Hard Digging (Dig)")
- A badge icon for the skill
- Color-coded background indicating status for the selected duplicant

## Skill groups

Skills are organized into 13 groups for standard duplicants (plus a bionic group for bionic duplicants). Each group maps to a type of work:

| Group | Work Type | Key Attribute |
|-------|-----------|---------------|
| Mining (Dig) | Digging tiles | Digging |
| Building (Build) | Constructing buildings | Construction |
| Farming | Planting, harvesting | Agriculture |
| Ranching | Animal care | Ranching |
| Cooking (Cook) | Preparing food | Cooking |
| Art | Creating decor | Creativity |
| Research | Using research stations | Science |
| Suits (Suit Wearing) | Wearing exosuits efficiently | Athletics |
| Hauling | Carrying materials | Strength |
| Technicals (Machine Operating) | Operating machinery | Machinery |
| Basekeeping | Tidying, plumbing | Tidying |
| Medical Aid | Doctoring, compounding medicine | Caring |
| Rocketry | Piloting rockets (Spaced Out only) | Piloting |

## The skill tree

Most groups have 2-3 skills in a linear chain. You must learn the earlier skill before the later one. A few notable exceptions break the linear pattern:

- **Ranching** starts at tier 1, not tier 0. Its first skill (Critter Ranching I) requires Improved Farming I from the Farming group as a cross-group prerequisite
- **Mechatronics Engineering** (Technicals group, tier 2) requires both Improved Carrying II (Hauling) and Electrical Engineering (Technicals). Two different groups feed into it
- **Rocket Piloting II** (Rocketry, Spaced Out) requires both Rocket Piloting I and Astronomy (Research group)
- **Research** branches: Advanced Research leads to both Field Research and Astronomy, which each lead to different tier 2 skills

The tree typically has 3-4 tiers per group, maxing out at tier 4 for Mining (Hazmat Digging) and Suits (Rocket Navigation, base game only).

## What each skill gives you

Every skill grants one or more perks. Perks come in three types:

1. **Attribute bonuses**: +2 to the relevant attribute (e.g., +2 Digging). This is the most common perk. A few exceptions: carrying skills give +400 or +800 carry capacity, space flight gives +0.1 navigation
2. **Building/ability unlocks**: the ability to use specific buildings or perform specific actions. For example, Grilling lets you use the Electric Grill, Exosuit Training lets you wear exosuits, Advanced Medical Care lets you use the disease clinic
3. **Immunity perks** (bionic only): grants immunity to specific negative effects

When you hover over a skill, the tooltip lists all its perks. For unlock perks, it names the specific buildings you can now use.

## Morale cost

Every learned skill increases the duplicant's morale expectations. The cost depends on the skill's tier:

| Tier | Morale Cost |
|------|-------------|
| 0 | +1 |
| 1 | +2 |
| 2 | +3 |
| 3 | +4 |
| 4 | +5 |

So a duplicant with three tier-0 skills and one tier-2 skill has +6 morale expectations from skills alone.

**Aptitude bonus**: If a duplicant has an interest (aptitude) in a skill group, they get +1 morale bonus for each skill they learn in that group. This partially offsets the cost. The aptitude indicator appears on skills in groups the duplicant is interested in.

**Granted skills are free**: Skills granted by traits or bionic boosters do not add to morale expectations at all.

## Skill states

For any given duplicant, each skill is in one of these states:

### Mastered
The duplicant has learned this skill. The widget has a green background. They have the perks, can wear the hat, and it counts toward their morale expectations.

### Granted
The skill was given to the duplicant for free by a trait or bionic booster. Green background like mastered, but with a special "granted" icon. Does not cost morale expectations. The duplicant still gets all the perks.

### Available
The duplicant can learn this skill right now. Blue background. All prerequisites are met, and they have at least one skill point to spend. Clicking the skill learns it immediately.

If learning the skill would push morale need above current morale, a stress warning appears in the tooltip, but the skill is still learnable. The game does not prevent you from over-skilling a duplicant.

### Locked: missing prerequisites
Gray background, desaturated icon. The duplicant has not learned one or more prerequisite skills yet. The tooltip says which prerequisites are missing.

### Locked: no skill points
Gray background. Prerequisites are met, but the duplicant has no skill points to spend. They need to earn more XP.

### Locked: trait blocked
Gray background with a special "blocked" icon. The duplicant has a trait that disables the associated chore group entirely (e.g., "Unconstructive" prevents all Building skills). The tooltip names the blocking trait. This is permanent and cannot be overcome.

### Hidden (wrong model)
Bionic duplicants see bionic skills; standard duplicants see standard skills. Skills for the wrong model are completely hidden.

## Hats

Every skill comes with a hat. When a duplicant masters a skill, they earn the associated hat. You can then choose which hat they wear from a dropdown. Hats are purely cosmetic and serve as a visual indicator of a duplicant's role. A duplicant can only wear one hat at a time.

Changing a hat in the dropdown does not take effect instantly. The duplicant will walk to the Printing Pod to put on the new hat when they get a chance.

## The tooltip system

The screen is tooltip-heavy. Here is what each tooltip contains:

### Skill widget tooltip
- List of perks the skill grants (building names or attribute bonuses)
- Duplicant-specific context:
  - If mastered and granted: "Meep has been granted Grilling by a Trait, but does not have increased Morale Requirements from learning it"
  - If can learn: "Meep can learn Grilling"
  - If can learn but stress warning: "Learning Grilling will put Meep into a Morale deficit and cause unnecessary Stress!"
  - If has aptitude: "Meep is interested in Grilling and will receive a Morale bonus for learning it!"
  - If cannot learn: "Meep cannot learn Grilling" plus the specific reason (missing prereq, no points, or blocking trait)

### Mastery count (on each skill widget)
- "Duplicants who have mastered this job: Meep, Bubbles, Stinky" or "No Duplicants have mastered this job"
- This counts across ALL duplicants, not just the selected one

### Duplicant sidebar tooltip
- Duplicant name
- Current morale / morale need
- Stats: every attribute with its current modifier value (Digging: +4, Construction: +2, etc.)

### Morale/expectations tooltip (header area)
- Full morale breakdown: every source of morale listed with its value (food quality, room bonuses, recreation, skill aptitude bonuses, etc.)
- Full expectations breakdown: every source of morale need (skills learned, job tier expectations, etc.)

### XP tooltip
- "X exp to next Skill Point"

## Bionic duplicants (DLC3)

Bionic duplicants have a completely separate skill tree. When you select a bionic duplicant, the standard skill tree is hidden and replaced with the bionic skill tree, which is smaller (4 branches, 2-3 tiers each, all in a single "Bionic Skills" group).

Bionic duplicants also get an additional panel at the bottom of the screen: the booster panel. This shows:

- How many booster slots are unlocked and assigned (e.g., "2/4 boosters assigned")
- Visual slot indicators (locked, empty, or filled)
- Available booster types with increment/decrement buttons to assign or unassign them
- Tooltips with booster descriptions and colony-wide assignment info

The booster panel takes up about 40% of the screen height when visible.

## Experience and skill points

Duplicants gain experience passively just by being alive, and actively by performing skilled work (research, building, farming, etc.). Active XP is further multiplied if the duplicant has aptitude in that skill group.

The XP-to-skill-points curve is exponential. Early skill points come quickly; later ones take much longer. The XP bar in the header shows progress toward the next point.

Skill points are global per duplicant, not per group. A duplicant with 3 unspent skill points can put them all into Mining or spread them across Mining, Cooking, and Art.

## What is NOT on this screen

- **No way to unlearn skills**. Once learned, a skill is permanent. There is no respec button. (The code has an `UnmasterSkill` method, but it is never exposed in the UI. It only runs automatically when a save is loaded with deprecated skills.)
- **No skill descriptions** visible on the widgets themselves. The description only appears in tooltips.
- **No filtering or searching**. You scroll the tree to find what you want.
- **No multi-select**. You manage one duplicant at a time.
