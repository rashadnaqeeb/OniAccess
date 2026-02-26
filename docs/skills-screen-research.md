# SkillsScreen Research

Comprehensive research of the ONI skill screen for accessibility handler implementation.

## 1. Class Hierarchy

```
KMonoBehaviour
  └─ KScreen
       └─ KModalScreen          (pauses game, modal overlay, consumes all input)
            └─ SkillsScreen     (the actual skills management screen)
```

`KModalScreen` key behaviors:
- Creates a dark semi-transparent background overlay
- Pauses the game when shown, unpauses when hidden
- `OnKeyDown` consumes Escape to deactivate, blocks pause/speed toggle
- Consumes ALL key events (`e.Consumed = true` at end of `OnKeyDown`/`OnKeyUp`)
- `IsModal()` returns true
- `GetSortKey()` returns 100 (SkillsScreen overrides to 20 normally, 50 when editing)

## 2. SkillsScreen Class

**File**: `ONI-Decompiled/Assembly-CSharp/SkillsScreen.cs`

### How it opens

- Hotkey: `Action.ManageSkills` (default key not visible in decompiled source, but registered in `ManagementMenu`)
- `ManagementMenu.ToggleSkills()` checks `SkillsAvailable()` first
- `SkillsAvailable()` requires `Components.RoleStations.Count > 0` OR sandbox/debug mode (i.e., need a Printing Pod or similar)
- `ManagementMenu.OpenSkills(MinionIdentity)` sets `CurrentlySelectedMinion` then toggles the screen
- The screen is instantiated via `ScheduledUIInstantiation` and stored in `ManagementMenu.skillsScreen`

### Fields

**UI Prefabs:**
- `Prefab_skillWidget` - template for individual skill entries
- `Prefab_skillColumn` - template for tier columns
- `Prefab_minion` - template for duplicant entries in the left sidebar
- `Prefab_minionLayout` - parent container for minion entries
- `Prefab_tableLayout` - parent container for skill columns
- `Prefab_worldDivider` - divider between worlds (cluster space)

**Sort Toggles:**
- `dupeSortingToggle` - sort by name (MultiToggle)
- `experienceSortingToggle` - sort by available skill points (MultiToggle)
- `moraleSortingToggle` - sort by morale (MultiToggle)
- `activeSortToggle` - currently active sort toggle
- `sortReversed` - whether sort is reversed
- `active_sort_method` - current Comparison delegate

**Duplicant Animation:**
- `minionAnimWidget` (FullBodyUIMinionWidget) - shows the selected dupe's animated portrait

**Progress Bars (Header Area):**
- `expectationsTooltip` (ToolTip) - tooltip for morale/expectations display
- `moraleProgressLabel` (LocText) - shows "Morale: X" text
- `moraleWarning` (GameObject) - shown when morale >= expectations
- `moraleNotch` (GameObject) - template for morale bar notches
- `moraleNotchColor` (Color) - color for filled morale notches
- `expectationsProgressLabel` (LocText) - shows "Morale Need: X" text
- `expectationWarning` (GameObject) - shown when morale < expectations
- `expectationNotch` (GameObject) - template for expectation bar notches
- `expectationNotchColor` (Color) - color for current expectation notches
- `expectationNotchProspectColor` (Color) - color for prospective expectation notches (from hovered skill)
- `experienceBarTooltip` (ToolTip) - tooltip for XP bar
- `experienceProgressFill` (Image) - XP progress fill bar
- `EXPCount` (LocText) - "X / Y" experience text
- `duplicantLevelIndicator` (LocText) - available skill points count

**Hat Dropdown:**
- `hatDropDown` (DropDown) - dropdown for hat selection
- `selectedHat` (Image) - currently selected hat image

**Skills Container:**
- `skillsContainer` (GameObject) - main container for skill widgets
- `scrollRect` (KScrollRect) - scroll rect for skills area
- `scrollSpeed` (float = 7) - analog scroll speed for gamepad

**Bionic Booster Panel (DLC3):**
- `boosterPanel`, `boosterHeader`, `boosterContentGrid` - UI containers
- `boosterPrefab` - template for booster entries
- `boosterWidgets` - Dictionary<Tag, HierarchyReferences> of booster widgets
- `equippedBoostersHeaderLabel`, `assignedBoostersCountLabel` - header labels
- `boosterSlotIconPrefab` - template for booster slot icons
- `boosterSlotIcons` - List of booster slot icon GameObjects

**State:**
- `currentlySelectedMinion` (IAssignableIdentity) - currently selected dupe
- `rows` (List<GameObject>) - minion row GameObjects
- `sortableRows` (List<SkillMinionWidget>) - all minion widgets
- `worldDividers` (Dictionary<int, GameObject>) - world dividers by world ID
- `hoveredSkillID` (string) - ID of skill currently being hovered (used for morale preview)
- `skillWidgets` (Dictionary<string, GameObject>) - skill widgets keyed by skill ID
- `skillGroupRow` (Dictionary<string, int>) - row position offset per skill group
- `skillColumns` (List<GameObject>) - tier column containers
- `dirty` (bool) - needs refresh flag
- `linesPending` (bool) - skill prerequisite lines need redraw
- `layoutRowHeight` (int = 80) - pixel height per row

### Key Methods

**Lifecycle:**
- `OnSpawn()` - subscribes to world removal events
- `OnActivate()` - builds minions, refreshes all, sets up sort toggle callbacks, registers for minion add/remove events
- `OnShow(show)` - if showing: selects first minion if none selected, builds minions, populates boosters, refreshes all, sorts
- `Update()` - refreshes if dirty, redraws lines if pending, handles gamepad scroll

**Refresh Methods:**
- `RefreshAll()` - calls RefreshSkillWidgets, RefreshSelectedMinion, RefreshBoosters
- `RefreshSelectedMinion()` - updates portrait, progress bars, hat
- `RefreshProgressBars()` - complex: calculates morale, expectations, XP, builds tooltip with modifier breakdowns
- `RefreshHat()` - updates hat dropdown and selected hat image
- `RefreshSkillWidgets()` - iterates all SkillGroups, creates/updates skill widgets, handles model-specific skills (bionic vs standard), refreshes minion widgets
- `RefreshBoosters()` - bionic booster panel (DLC3 only)
- `RefreshWidgetPositions()` - positions skill widgets in the grid layout

**Minion Management:**
- `BuildMinions()` - creates SkillMinionWidget for each live + stored minion, adds world dividers
- `SortRows(Comparison)` - sorts minion rows by world, then by comparison within each world
- `GetMinionIdentity(IAssignableIdentity, out MinionIdentity, out StoredMinionIdentity)` - resolves proxy to actual identity

**Skill Queries:**
- `GetSkillsBySkillGroup(string)` - returns all non-deprecated skills in a group
- `GetSkillWidget(string)` - returns SkillWidget for a skill ID
- `GetRowPosition(string)` - calculates pixel Y position for a skill widget
- `HoverSkill(string)` - sets hovered skill, triggers morale preview refresh

**Sort Comparisons (defined as fields):**
- `compareByMinion` - alphabetical by name
- `compareByExperience` - by AvailableSkillpoints
- `compareByMorale` - by QualityOfLife total value

### CurrentlySelectedMinion Property

Setter triggers `RefreshSelectedMinion()`, `RefreshSkillWidgets()`, and `RefreshBoosters()` when screen is active.

## 3. SkillWidget Class

**File**: `ONI-Decompiled/Assembly-CSharp/SkillWidget.cs`

Implements `IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler`.

### Fields

- `Name` (LocText) - skill name display
- `Description` (LocText) - skill description
- `TitleBarBG` (Image) - background color indicates state
- `skillsScreen` (SkillsScreen) - reference to parent screen
- `tooltip` (ToolTip) - skill tooltip
- `lines_left`, `lines_right` (RectTransform) - connection points for prerequisite lines
- `header_color_has_skill` - color when mastered
- `header_color_can_assign` - color when available to learn
- `header_color_disabled` - color when locked/unavailable
- `line_color_default`, `line_color_active` - line colors for prereq lines
- `hatImage` (Image) - skill badge/hat icon
- `borderHighlight` (GameObject) - highlight border on hover
- `masteryCount` (ToolTip) - shows count of dupes who mastered this skill
- `aptitudeBox` (GameObject) - shown when dupe has aptitude for this skill group
- `grantedBox` (GameObject) - shown when skill was granted (not learned)
- `grantedIcon` (Image) - icon showing grant source
- `traitDisabledIcon` (GameObject) - shown when a trait prevents learning
- `prerequisiteSkillWidgets` (List<SkillWidget>) - cached prereq widgets for highlight chaining
- `lines` (UILineRenderer[]) - rendered prerequisite lines
- `defaultMaterial`, `desaturatedMaterial` - materials for hat icon
- `soundPlayer` (ButtonSoundPlayer)

### Refresh(string skillID)

Sets the widget's visual state:

1. Sets `Name.text` to `skill.Name`. If the skill group has a choreGroupID, appends `"\n(skillGroup.Name)"` (e.g., "Hard Digging\n(Dig)")
2. Sets tooltip via `SkillTooltip(skill)`
3. Gets minion identity from screen's `CurrentlySelectedMinion`
4. For live minions: checks mastery conditions, sets TitleBarBG color:
   - **Mastered** -> `header_color_has_skill`
   - **Can assign** -> `header_color_can_assign`
   - **Locked** -> `header_color_disabled`
5. For stored minions: mastered or disabled only
6. Sets hat badge sprite from `skill.badge`
7. Shows/hides `aptitudeBox` (dupe has interest AND skill not granted)
8. Shows/hides `grantedBox` (skill was granted by trait/booster)
9. Shows/hides `traitDisabledIcon` (trait prevents learning)
10. Builds mastery count: lists all dupes (live + stored) who mastered this skill
11. Mastery count tooltip format:
    - Has masters: `"Duplicants who have mastered this job:\n    * Name1\n    * Name2"`
    - No masters: `"No Duplicants have mastered this job"`

### Tooltip Contents (SkillTooltip)

Composed of `SkillPerksString(skill)` + `"\n"` + `DuplicantSkillString(skill)`:

**SkillPerksString**: Lists each perk's description. For each perk:
- Uses `GameUtil.NamesOfBuildingsRequiringSkillPerk(perkId)` first (returns building names that require the perk)
- Falls back to `perk.Name` if no buildings require it
- Result: building names or attribute bonus descriptions

**DuplicantSkillString**: Context for the selected dupe:
- If mastered AND granted: `"{Name} has been granted {SkillName} by a Trait..."`
- If not mastered, cannot learn: `"{Name} cannot learn {SkillName}"` + reason:
  - `UnableToLearn`: `"This Duplicant possesses the {TraitName} Trait and cannot learn this Skill"`
  - `MissingPreviousSkill`: `"Missing prerequisite Skill"`
  - `NeedsSkillPoints`: `"Not enough Skill Points"`
- If not mastered, can learn:
  - `StressWarning`: `"Learning {SkillName} will put {Name} into a Morale deficit and cause unnecessary Stress!"`
  - `SkillAptitude`: `"{Name} is interested in {SkillName} and will receive a Morale bonus for learning it!"`

**CriteriaString** (not used in tooltip directly, but exists on the widget):
- Lists assignment requirements:
  - Relevant attributes from skill group
  - Prior skills (prerequisites)
  - Or "None" if no requirements

### Interaction (OnPointerClick)

Left click on a skill widget:
1. Gets the selected minion's MinionResume
2. In debug mode with 0 points: force-adds a skill point
3. Gets mastery conditions
4. If not mastered AND can master: calls `resume.MasterSkill(skillID)` then `skillsScreen.RefreshAll()`

**OnPointerDown**: Plays positive sound if can learn, negative sound if cannot.

### Hover Behavior

- `OnPointerEnter`: highlights border + prerequisite chain, calls `skillsScreen.HoverSkill(skillID)` to trigger morale preview
- `OnPointerExit`: removes highlight, calls `skillsScreen.HoverSkill(null)`

## 4. SkillMinionWidget Class

**File**: `ONI-Decompiled/Assembly-CSharp/SkillMinionWidget.cs`

Left sidebar entry for each duplicant.

### Fields

- `skillsScreen` (SkillsScreen) - parent screen
- `portrait` (CrewPortrait) - dupe portrait
- `masteryPoints` (LocText) - available skill points display
- `morale` (LocText) - morale/expectation ratio
- `background` (Image) - background color
- `hat_background` (Image)
- `selected_color`, `unselected_color`, `hover_color` (Colors)
- `hatDropDown` (DropDown) - per-minion hat dropdown
- `assignableIdentity` (IAssignableIdentity) - the dupe this widget represents

### Refresh()

1. Sets portrait
2. For live minions:
   - `masteryPoints.text` = available skill points (green + bold if > 0, "0" otherwise)
   - `morale.text` = `"currentMorale/expectedMorale"` format
   - Builds tooltip (see below)
   - Populates hat dropdown with mastered skill hats
   - Gets current/target hat
3. For stored minions:
   - masteryPoints and morale show "N/A"
   - Tooltip shows stored reason
4. Sets background color: selected, unselected, or hover
5. Updates hat image
6. Shows/hides hat dropdown open button (only for live minions)

### Tooltip Contents

For live minions:
- Header: `"{DupeName}\n\n"`
- Morale: `"Current Morale: {morale}/{expectation}"`
- Stats header: `"\nStats\n\n"`
- For each attribute with `Display.Skill`:
  - `"    * {AttributeName}: {+/-Value}"` (colored green/red/white)

### Click Behavior

Clicking a minion widget sets `skillsScreen.CurrentlySelectedMinion = assignableIdentity`, triggering full refresh.

## 5. Data Model Classes

### Skill (`Database.Skill`)

**File**: `ONI-Decompiled/Assembly-CSharp/Database/Skill.cs`

Extends `Resource` (has `Id` and `Name`).

Fields:
- `description` (string)
- `requiredDlcIds`, `forbiddenDlcIds` (string[]) - DLC gating
- `skillGroup` (string) - ID of the SkillGroup this belongs to
- `hat` (string) - hat sprite ID earned when mastering
- `badge` (string) - badge sprite ID shown on the widget
- `tier` (int) - tier level (0-4, determines column and morale cost)
- `deprecated` (bool) - hidden from UI
- `perks` (List<SkillPerk>) - perks granted by this skill
- `priorSkills` (List<string>) - prerequisite skill IDs
- `requiredDuplicantModel` (string) - "Minion" for standard dupes, "Bionic" tag for bionic dupes

Key methods:
- `GetMoraleExpectation()` -> `SKILLS.SKILL_TIER_MORALE_COST[tier]`

### Skills (`Database.Skills`)

**File**: `ONI-Decompiled/Assembly-CSharp/Database/Skills.cs`

ResourceSet of all Skill objects. Constructor creates every skill in the game.

**Complete Skill Tree (standard duplicants, model="Minion"):**

| Group | Tier 0 | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-------|--------|--------|--------|--------|--------|
| Mining | Mining1 (Hard Digging) | Mining2 (Superhard Digging) | Mining3 (Super-Duperhard Digging) | Mining4* (Hazmat Digging) | |
| Building | Building1 (Improved Construction I) | Building2 (Improved Construction II) | Building3 (Demolition) | | |
| Farming | Farming1 (Improved Farming I) | Farming2 (Crop Tending) | Farming3 (Improved Farming II) | | |
| Ranching | | Ranching1** (Critter Ranching I) | Ranching2 (Critter Ranching II) | | |
| Research | Researching1 (Advanced Research) | Researching2 (Field Research), Astronomy* (Astronomy) | AtomicResearch* (Applied Sciences), SpaceResearch* (Data Analysis), Researching3~ (Astronomy base game) | | |
| Cooking | Cooking1 (Grilling) | Cooking2 (Grilling II) | | | |
| Art | Arting1 (Art Fundamentals) | Arting2 (Aesthetic Design) | Arting3 (Masterworks) | | |
| Hauling | Hauling1 (Improved Carrying I) | Hauling2 (Improved Carrying II) | | | |
| Suits | | ThermalSuits (Suit Sustainability Training) | Suits1 (Exosuit Training) | Astronauting1~ (Rocket Piloting base) | Astronauting2~ (Rocket Navigation base) |
| Technicals | Technicals1 (Improved Tinkering) | Technicals2 (Electrical Engineering) | Engineering1 (Mechatronics Engineering)*** | | |
| Basekeeping | Basekeeping1 (Improved Strength) | Basekeeping2 (Plumbing) | Pyrotechnics (Pyrotechnics) | | |
| MedicalAid | Medicine1 (Medicine Compounding) | Medicine2 (Bedside Manner) | Medicine3 (Advanced Medical Care) | | |
| Rocketry* | RocketPiloting1 (Rocket Piloting) | | RocketPiloting2 (Rocket Piloting II)**** | | |

\* = Expansion1 (Spaced Out) only
\*\* = Ranching1 is in the Ranching skill group but has Farming1 as a cross-group prerequisite
\*\*\* = Engineering1 requires BOTH Hauling2 AND Technicals2 (cross-group prereq)
\*\*\*\* = RocketPiloting2 requires BOTH RocketPiloting1 AND Astronomy
~ = deprecated in some DLC configurations

**Bionic Skills (model="Bionic", DLC3):**

| Group | Tier 0 | Tier 1 | Tier 2 |
|-------|--------|--------|--------|
| BionicSkills | BionicsA1 (Booster Processing I) | BionicsA2 (Booster Processing II) | BionicsA3 (Complex Processing) |
| BionicSkills | BionicsB1 (Improved Gears I) | BionicsB2 (Improved Gears II) | BionicsC3 (Power Banking)* |
| BionicSkills | BionicsC1 (Schematics) | BionicsC2 (Advanced Schematics) | |
| BionicSkills | BionicsD1 (Improved Hardware) | BionicsD2 (Climate Control) | |

\* BionicsC3 requires BOTH BionicsB2 AND BionicsC2 (cross-branch prereq)

### SkillGroup (`Database.SkillGroup`)

Fields:
- `Id` (string) - e.g., "Mining", "Building"
- `Name` (string) - localized display name (from STRINGS)
- `choreGroupID` (string) - linked chore group ID (empty string for Suits, null for BionicSkills)
- `relevantAttributes` (List<Attribute>) - attributes relevant to this group
- `requiredChoreGroups` (List<string>) - chore groups needed
- `choreGroupIcon`, `archetypeIcon` (string) - sprite names
- `allowAsAptitude` (bool) - can be selected as dupe interest (false for BionicSkills)

**All SkillGroups:**

| ID | Name Source | ChoreGroup | Relevant Attribute |
|----|-----------|------------|-------------------|
| Mining | DUPLICANTS.CHOREGROUPS.DIG.NAME | Dig | Digging |
| Building | DUPLICANTS.CHOREGROUPS.BUILD.NAME | Build | Construction |
| Farming | DUPLICANTS.CHOREGROUPS.FARMING.NAME | Farming | Agriculture |
| Ranching | DUPLICANTS.CHOREGROUPS.RANCHING.NAME | Ranching | Ranching |
| Cooking | DUPLICANTS.CHOREGROUPS.COOK.NAME | Cook | Cooking |
| Art | DUPLICANTS.CHOREGROUPS.ART.NAME | Art | Creativity |
| Research | DUPLICANTS.CHOREGROUPS.RESEARCH.NAME | Research | Science |
| Rocketry* | DUPLICANTS.CHOREGROUPS.ROCKETRY.NAME | Rocketry | Piloting |
| Suits | DUPLICANTS.ROLES.GROUPS.SUITS ("Suit Wearing") | (none) | Athletics |
| Hauling | DUPLICANTS.CHOREGROUPS.HAULING.NAME | Hauling | Strength |
| Technicals | DUPLICANTS.CHOREGROUPS.MACHINEOPERATING.NAME | MachineOperating | Machinery |
| MedicalAid | DUPLICANTS.CHOREGROUPS.MEDICALAID.NAME | MedicalAid | Caring |
| Basekeeping | DUPLICANTS.CHOREGROUPS.BASEKEEPING.NAME | Basekeeping | Tidying |
| BionicSkills** | (empty string) | null | (none) |

\* Rocketry only in cluster space (Spaced Out)
\*\* BionicSkills only with DLC3

### SkillPerk (`Database.SkillPerk`)

Base class for all skill perks. Extends `Resource` (has `Id` and `Name`).

Fields:
- `OnApply` (Action<MinionResume>) - called when perk is applied
- `OnRemove` (Action<MinionResume>) - called when perk is removed
- `OnMinionsChanged` (Action<MinionResume>) - called when minions change
- `affectAll` (bool) - whether perk affects all minions
- `requiredDlcIds` (string[])

Subclasses:
- **SimpleSkillPerk** - just a name/description, no attribute changes (e.g., "Can dig X material")
- **SkillAttributePerk** - adds an `AttributeModifier` to the minion (e.g., +2 Digging)
  - `modifier` (AttributeModifier) - the actual modifier
  - `Name` is formatted as `"+{value} {AttributeName}"` via `UI.ROLES_SCREEN.PERKS.ATTRIBUTE_EFFECT_FMT`
- **ImmunitySkillPerk** - grants immunity to a specific Effect (bionic only)
  - `Name` formatted as `"Immunity to {EffectName}"` via `UI.ROLES_SCREEN.PERKS.IMMUNITY`

### Attribute Bonus Values

All `SkillAttributePerk` bonuses use TUNING values:
- `ROLES.ATTRIBUTE_BONUS_FIRST` = 2
- `ROLES.ATTRIBUTE_BONUS_SECOND` = 2
- `ROLES.ATTRIBUTE_BONUS_THIRD` = 2

So each tier of skill gives +2 to the relevant attribute. Some exceptions:
- `IncreaseCarryAmountSmall` = +400
- `IncreaseCarryAmountMedium` = +800
- `FasterSpaceFlight` = +0.1 Space Navigation
- Bionic athletics perks = +2 each
- Bionic booster slot perks = +1 or +2 slots
- Bionic battery perk = +2 batteries

### MinionResume

**File**: `ONI-Decompiled/Assembly-CSharp/MinionResume.cs`

The component on each duplicant that tracks skills.

**Key Fields:**
- `MasteryBySkillID` (Dictionary<string, bool>) - which skills are mastered
- `GrantedSkillIDs` (List<string>) - skills granted by traits/boosters (don't cost points)
- `AptitudeBySkillGroup` (Dictionary<HashedString, float>) - interest/aptitude per skill group (from character creation)
- `currentHat`, `targetHat` (string) - current and pending hat sprite IDs
- `totalExperienceGained` (float) - raw XP accumulation
- `ownedHats` (Dictionary<string, bool>) - hats the dupe owns

**Key Properties:**
- `TotalExperienceGained` -> `totalExperienceGained`
- `TotalSkillPointsGained` -> calculated from XP via power curve
- `SkillsMastered` -> count of true entries in MasteryBySkillID
- `AvailableSkillpoints` -> `TotalSkillPointsGained - SkillsMastered + GrantedSkillIDs.Count`

**Skill Mastery Conditions (enum SkillMasteryConditions):**
- `SkillAptitude` - dupe has interest in this skill group (bonus morale)
- `StressWarning` - learning will cause morale deficit
- `UnableToLearn` - a trait blocks this skill (via chore group disabling)
- `NeedsSkillPoints` - no available skill points
- `MissingPreviousSkill` - prerequisite skills not mastered

**CanMasterSkill** returns false only if `UnableToLearn`, `NeedsSkillPoints`, or `MissingPreviousSkill` is present. `StressWarning` and `SkillAptitude` are informational - they don't block learning.

**Key Methods:**
- `HasMasteredSkill(skillId)` - check if skill is mastered
- `HasBeenGrantedSkill(skill)` - check if skill was granted (not learned)
- `IsAbleToLearnSkill(skillId)` - checks trait-based chore group disabling
- `BelowMoraleExpectation(skill)` - checks if learning would exceed morale
- `HasMasteredDirectlyRequiredSkillsForSkill(skill)` - prerequisite check
- `HasSkillPointsRequiredForSkill(skill)` - needs >= 1 available point
- `HasSkillAptitude(skill)` - checks AptitudeBySkillGroup
- `MasterSkill(skillId)` - actually learns the skill (applies perks, updates expectations/morale, triggers events, adds hat)
- `UnmasterSkill(skillId)` - removes skill mastery
- `GrantSkill(skillId)` - grants a skill without costing points
- `GetAllHats()` - returns all hats from mastered skills + additional hats (from boosters etc.)
- `GetSkillGrantSourceIcon(skillId)` - returns icon sprite for grant source
- `ResetSkillLevels(returnSkillPoints)` - unmasters all skills
- `HasPerk(perkId)` - checks if dupe has a specific perk (from skills or additional grants)

**Experience System:**
- Passive XP: `dt * 0.5` every 200ms tick (while alive)
- Active XP: via `AddExperienceWithAptitude(skillGroupId, amount, buildingMultiplier)`
- Aptitude multiplier: `1 + aptitude * 0.5 * buildingMultiplier`
- Active multiplier: `* 0.6`
- Level-up formula: `Pow((points+1)/15, 1.44) * 250 * 600` XP for next point
- Display in screen: `EXPCount.text = (current - prevBar) + " / " + (nextBar - prevBar)`

**HatInfo (inner class):**
- `Source` (string) - name of skill or source that provides the hat
- `Hat` (string) - hat sprite ID
- `count` (int) - reference count for additional hats

### Morale Cost per Tier

From `TUNING.SKILLS.SKILL_TIER_MORALE_COST`:
- Tier 0: +1 morale expectation
- Tier 1: +2 morale expectation
- Tier 2: +3 morale expectation
- Tier 3: +4 morale expectation
- Tier 4: +5 morale expectation
- Tier 5: +6 morale expectation
- Tier 6: +7 morale expectation

Granted skills do NOT add to morale expectations.

Skills in groups where the dupe has aptitude grant +1 morale bonus per mastered skill (aptitude).

### HatListable

Simple wrapper for hat dropdown entries:
- `name` (string) - display name (skill name)
- `hat` (string) - hat sprite ID
- Implements `IListableOption.GetProperName()` -> returns `name`

## 6. UI Layout

### Overall Structure

The screen is divided into:

1. **Left sidebar**: List of duplicant entries (SkillMinionWidget), sorted by world with world dividers, then by the active sort method within each world
2. **Header area** (for selected dupe): animated portrait, morale/expectations bars with notches, XP progress bar, available skill points, hat dropdown
3. **Main area**: Skill tree grid with columns by tier and rows by skill group. Skills are connected by prerequisite lines.
4. **Booster panel** (bionic only, DLC3): Bottom panel showing available boosters and assignment

### Left Sidebar (Duplicant List)

Each `SkillMinionWidget` shows:
- Portrait image
- Available skill points (green bold if > 0)
- Morale ratio ("X/Y" format: current morale / morale need)
- Hat dropdown
- Background color: selected (highlighted), unselected, or hover

World dividers appear between groups of minions on different asteroids (only in cluster space mode).

Stored minions (in rockets, etc.) show:
- "N/A" for skill points and morale
- Reduced opacity (canvas group alpha = 0.6)
- Hat dropdown disabled

### Header Area (Selected Duplicant Info)

For live minions:
- **Animated portrait** (FullBodyUIMinionWidget)
- **Morale bar**: Notches showing current morale value, with green color for actual morale and potential morale bonus from aptitudes
- **Expectations bar**: Notches showing current expectations value, with a different color for prospective additional expectations from the hovered skill
- **Morale label**: "Morale: X" (+ green bonus if hovering skill with aptitude)
- **Expectation label**: "Morale Need: X" (+ colored addition if hovering a skill)
- **Warning icons**: morale warning (green, morale >= expectations) or expectation warning (red, morale < expectations)
- **XP bar**: "X / Y" text showing progress to next skill point
- **Skill points available**: number displayed in `duplicantLevelIndicator`
- **Hat dropdown**: shows currently equipped/targeted hat, dropdown lists all earned hats

For stored minions:
- XP bar hidden
- Tooltip: "Information not available for {StorageReason} {Name}"
- Skill points show "N/A"
- Hat dropdown disabled

### Expectations Tooltip

Built by `RefreshProgressBars()`:
```
Morale: {totalMorale}
    * {modifierName}: +/-{value} (green/red)
    * {modifierName}: +/-{value}
    ...

Morale Need: {totalExpectation}
    * {modifierName}: +/-{value} (red for positive expectations, green for negative)
    ...
```

### Skill Tree Grid

Skills are arranged in a 2D grid:
- **Columns** = tiers (0, 1, 2, 3, 4). Column 0 is leftmost.
- **Rows** = skill groups. Each group can have multiple skills per tier, stacked vertically.
- Even-numbered columns have their background hidden (alternating pattern).
- Skills are positioned via `RefreshWidgetPositions()` using `GetRowPosition()`.
- `layoutRowHeight = 80` pixels per row.

Skill widgets within the same group and tier are stacked vertically within that cell.

**Row positioning**: `skillGroupRow` maps each group ID to a base row number. Within a group, skills at the same tier are offset by row index.

**Prerequisite lines**: Each skill widget draws UILineRenderer lines to its prerequisite skills' `lines_right` positions. Lines are L-shaped (horizontal then vertical then horizontal).

### Each Skill Widget Shows

- **Skill name** + optional skill group name in parentheses (if skill group has a choreGroupID)
- **Badge/hat icon** (skill.badge sprite) - full color if mastered or available, desaturated if locked
- **Title bar color**:
  - Green-ish = mastered (`header_color_has_skill`)
  - Blue-ish = can learn (`header_color_can_assign`)
  - Gray = locked (`header_color_disabled`)
- **Aptitude indicator** (aptitudeBox) - shown if dupe has interest and skill not granted
- **Granted indicator** (grantedBox) - shown if skill was granted by trait/booster, with source icon
- **Trait disabled icon** (traitDisabledIcon) - shown if a trait blocks learning
- **Mastery count** - number of dupes who mastered this skill, with tooltip listing names
- **Border highlight** - activates on hover, chains to prerequisite skills
- **Tooltip** - perks list + dupe-specific status (see Section 3)

### Model-Specific Skills

Skills with `requiredDuplicantModel` set are only visible when the selected minion matches that model:
- Standard skills: `requiredDuplicantModel = "Minion"`
- Bionic skills: `requiredDuplicantModel = GameTags.Minions.Models.Bionic`

`RefreshSkillWidgets()` calls `skillWidget.Value.SetActive(skill.requiredDuplicantModel == SelectedMinionModel())` for each skill with a non-null requiredDuplicantModel.

## 7. Interaction Flow

### Opening the Screen

1. Player presses the Skills hotkey (`Action.ManageSkills`) or clicks the skills button in the management menu
2. `ManagementMenu.ToggleSkills()` checks `SkillsAvailable()` (needs Printing Pod)
3. `SkillsScreen.OnShow(true)` -> selects first minion if none, builds minions, refreshes all

### Selecting a Duplicant

1. Click a `SkillMinionWidget` in the left sidebar
2. `SkillMinionWidget.OnPointerClick` sets `skillsScreen.CurrentlySelectedMinion = assignableIdentity`
3. This triggers `RefreshSelectedMinion()` + `RefreshSkillWidgets()` + `RefreshBoosters()`
4. All skill widgets update their states for the new dupe
5. Header area updates morale, XP, hat

### Browsing Skills

1. Mouse over skill widgets to highlight them and their prerequisite chains
2. Hovering triggers morale preview in the header (shows what morale cost would be)
3. Scroll with mouse wheel or gamepad analog to navigate the skill tree

### Assigning a Skill

1. Left-click on a skill widget
2. `SkillWidget.OnPointerClick` checks:
   - Must be a live minion (not stored)
   - Gets mastery conditions from `MinionResume.GetSkillMasteryConditions()`
   - Calls `MinionResume.CanMasterSkill()` - must not have `UnableToLearn`, `NeedsSkillPoints`, or `MissingPreviousSkill`
   - Must not already be mastered
3. If all conditions met: `resume.MasterSkill(skillID)` then `skillsScreen.RefreshAll()`
4. Sound effect: positive click if learnable, negative if not

### Changing Hat

1. Click the hat dropdown (in header or minion sidebar)
2. Select from list of hats earned from mastered skills + additional hats
3. `OnHatDropEntryClick` sets `resume.SetHats(currentHat, targetHat)`
4. If the dupe already owns the hat, creates a `PutOnHatChore` to go change
5. Select "None" to remove hat

### Sorting

Three sort buttons in the header:
- **Duplicants** (alphabetical by name)
- **Morale** (by QualityOfLife total value)
- **Skill Points** (by AvailableSkillpoints)

Clicking the same sort button toggles reverse. Toggle states cycle: default -> ascending -> descending.

### Closing

- Escape key (handled by KModalScreen.OnKeyDown)
- Close button (calls ManagementMenu.Instance.CloseAll())
- Hotkey toggle (pressing the skills hotkey again)

## 8. Skill States (for a given duplicant)

For each skill widget, the possible states are:

### 1. Mastered (learned by spending points)
- TitleBarBG: `header_color_has_skill`
- Hat icon: default material (full color)
- No aptitude or granted box
- Can select hat earned from this skill

### 2. Granted (given by trait or booster, not learned)
- TitleBarBG: `header_color_has_skill`
- Hat icon: default material
- `grantedBox` visible with source icon
- Does NOT count against morale expectations
- Dupe gets the hat but skill "costs" nothing

### 3. Available to Learn
- TitleBarBG: `header_color_can_assign`
- Hat icon: default material
- Conditions: has available skill points, all prerequisites mastered, no blocking trait
- May show stress warning (morale deficit) but still learnable
- May show aptitude indicator if dupe has interest

### 4. Locked - Missing Prerequisites
- TitleBarBG: `header_color_disabled`
- Hat icon: desaturated material
- Tooltip: "Missing prerequisite Skill"

### 5. Locked - No Skill Points
- TitleBarBG: `header_color_disabled`
- Hat icon: desaturated material
- Tooltip: "Not enough Skill Points"

### 6. Locked - Trait Blocked
- TitleBarBG: `header_color_disabled`
- Hat icon: desaturated material
- `traitDisabledIcon` visible
- Tooltip: "This Duplicant possesses the {TraitName} Trait and cannot learn this Skill"
- This overrides other lock reasons in the tooltip display

### 7. Not Applicable (wrong model)
- Widget hidden entirely (`SetActive(false)`)
- E.g., standard minion viewing bionic skills or vice versa

### No "Unlearn" Mechanic
There is no UI to unlearn individual skills. `MinionResume.ResetSkillLevels()` exists but is not exposed through SkillsScreen. Skills can only be unlearned through `UnmasterSkill()` which is called when deprecated skills are found on save load.

## 9. Relevant STRINGS

### UI.SKILLS_SCREEN
- `CURRENT_MORALE` = "Current Morale: {0}\nMorale Need: {1}"
- `SORT_BY_DUPLICANT` = "Duplicants"
- `SORT_BY_MORALE` = "Morale"
- `SORT_BY_EXPERIENCE` = "Skill Points"
- `SORT_BY_SKILL_AVAILABLE` = "Skill Points"
- `SORT_BY_HAT` = "Hat"
- `SELECT_HAT` = "SELECT HAT"
- `POINTS_AVAILABLE` = "SKILL POINTS AVAILABLE"
- `MORALE` = "**Morale**"
- `MORALE_EXPECTATION` = "**Morale Need**"
- `EXPERIENCE` = "EXPERIENCE TO NEXT LEVEL"
- `EXPERIENCE_TOOLTIP` = "{0}exp to next Skill Point"
- `NOT_AVAILABLE` = "Not available"

### UI.SKILLS_SCREEN.ASSIGNMENT_REQUIREMENTS.MASTERY
- `CAN_MASTER` = "{0} **can learn** {1}"
- `HAS_MASTERED` = "{0} has **already learned** {1}"
- `CANNOT_MASTER` = "{0} **cannot learn** {1}"
- `STRESS_WARNING_MESSAGE` = "Learning {0} will put {1} into a Morale deficit and cause unnecessary Stress!"
- `REQUIRES_MORE_SKILL_POINTS` = "    * Not enough Skill Points"
- `REQUIRES_PREVIOUS_SKILLS` = "    * Missing prerequisite Skill"
- `PREVENTED_BY_TRAIT` = "    * This Duplicant possesses the {0} Trait and cannot learn this Skill"
- `SKILL_APTITUDE` = "{0} is interested in {1} and will receive a Morale bonus for learning it!"
- `SKILL_GRANTED` = "{0} has been granted {1} by a Trait, but does not have increased Morale Requirements from learning it"

### UI.SKILLS_SCREEN.ASSIGNMENT_REQUIREMENTS.SKILLGROUP_ENABLED
- `DESCRIPTION` = "Capable of performing **{0}** skills"

### UI.ROLES_SCREEN.WIDGET
- `NUMBER_OF_MASTERS_TOOLTIP` = "**Duplicants who have mastered this job:**{0}"
- `NO_MASTERS_TOOLTIP` = "**No Duplicants have mastered this job**"

### UI.ROLES_SCREEN.PERKS
- `ATTRIBUTE_EFFECT_FMT` = "**{0}** {1}" (e.g., "+2 Digging")
- `IMMUNITY` = "Immunity to **{0}**"
- Various building usage descriptions (CAN_ELECTRIC_GRILL, etc.)

### UI.ROLES_SCREEN.TIER_NAMES
- ZERO through NINE = "Tier 0" through "Tier 9"

### UI.ROLES_SCREEN.ASSIGNMENT_REQUIREMENTS
- `TITLE` = (used by CriteriaString)
- `NONE` = format string for no requirements

### UI.TABLESCREENS
- `NA` = "N/A" (used for stored minions)
- `INFORMATION_NOT_AVAILABLE_TOOLTIP` = format string for stored minion info

### DUPLICANTS.ROLES (skill names - all use FormatAsLink)
See complete list in Section 5 skill tree table. Each role has a NAME and DESCRIPTION.

### DUPLICANTS.NEEDS.QUALITYOFLIFE
- `EXPECTATION_MOD_NAME` - modifier name for skill morale expectations
- `APTITUDE_SKILLS_MOD_NAME` - modifier name for aptitude morale bonus

### MISC.NOTIFICATIONS.SKILL_POINT_EARNED
- `NAME` = "{Duplicant}" replacement pattern
- `TOOLTIP` = "{Duplicant}" replacement pattern

## 10. Bionic Booster Panel (DLC3)

Only visible when:
- Selected minion has model = `GameTags.Minions.Models.Bionic`
- Selected minion is a live minion (not stored)
- Has a `BionicUpgradesMonitor.Instance` SMI

Shows:
- Header: "{Name}'s Boosters"
- Slot count: "{assigned}/{unlocked} boosters assigned"
- Slot icons (up to 8): locked, available, or assigned with unassign button
- Booster list: each available booster type with:
  - Icon, name, available count, assigned count
  - Increment/decrement buttons
  - Tooltip with description and colony-wide assignment info

When visible, it takes up 40% of screen height, pushing the skills container up.

## 11. Accessibility Handler Design Considerations

### This is NOT a Table Screen

Unlike JobsTableScreen, VitalsTableScreen, and ConsumablesTableScreen, the SkillsScreen is NOT a standard table. It has a unique layout:
- Left sidebar: 1D list of duplicants
- Main area: 2D skill tree with prerequisite connections

BaseTableHandler is not appropriate. This needs a custom handler, probably based on BaseMenuHandler or a new base.

### Key Information to Speak

**When selecting a duplicant:**
- Name
- Available skill points
- Current morale / morale need
- Current hat (if any)

**When navigating skills:**
- Skill name
- Skill group name
- Tier
- Status for selected dupe: mastered, available, locked (with reason)
- Perks list
- Morale cost
- Prerequisites
- Number of dupes who mastered it
- Whether dupe has aptitude (interest)
- Whether skill was granted

**When hovering a skill (morale preview):**
- Current morale vs what it would be after learning
- Whether it would cause a deficit

### Navigation Approach

Possible approaches:
1. **Two-mode navigation**: Tab between dupe list and skill tree. Dupe list = 1D Up/Down. Skill tree = 2D arrows navigating the grid.
2. **Flat skill list**: Skip the 2D layout entirely, present skills as a 1D list grouped by skill group, since the 2D spatial layout is purely visual (connectivity is in the data model).

Option 2 is simpler and more practical for a blind user. The prerequisite tree structure can be conveyed through speech (e.g., "requires Hard Digging"). The tier is implicit in the skill's position within its group chain.

### Key Actions

- **Tab**: Switch between duplicant list and skill tree
- **Up/Down**: Navigate items in current section
- **Enter**: In skill tree, learn the skill (if available)
- **Left/Right**: In dupe list, could cycle hats? Or in skill tree, navigate between groups?
- **Ctrl+Up/Down**: Jump between skill groups in the tree

### Registration

Must register in `ContextDetector.RegisterMenuHandlers()`:
```csharp
Register<SkillsScreen>(screen => new SkillsScreenHandler(screen));
_showPatchedTypes.Add(typeof(SkillsScreen));
```

Uses show patch because ManagementMenu uses `Show()` lifecycle (like other management screens).

### Edge Cases

- **Stored minions**: Most data is N/A, can still view mastered skills
- **Bionic vs standard**: Different skill trees based on model. Need to handle model switching when selecting different dupes.
- **Bionic booster panel**: Additional section only for bionic dupes
- **Skills availability**: Screen won't open without a Printing Pod (RoleStation)
- **Cross-group prerequisites**: Engineering1 requires skills from two different groups. Need clear speech about what's needed.
- **Deprecated skills**: Filtered out of UI but may exist in save data
- **DLC-gated skills**: Some skills only available with Spaced Out or DLC3

### Data Access Patterns

All data can be read live:
- `Db.Get().Skills` / `SkillGroups` / `SkillPerks` for static definitions
- `MinionResume` component on each minion for dynamic state
- `Components.LiveMinionIdentities` for all live dupes
- `Components.MinionStorages` for stored dupes
- Morale: `Db.Get().Attributes.QualityOfLife.Lookup(resume).GetTotalValue()`
- Expectations: `Db.Get().Attributes.QualityOfLifeExpectation.Lookup(resume).GetTotalValue()`

No caching needed - all game state is live-queryable.
