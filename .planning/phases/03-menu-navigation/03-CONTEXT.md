# Phase 3: Menu Navigation - Context

**Gathered:** 2026-02-11
**Status:** Ready for planning

<domain>
## Phase Boundary

A blind player can navigate all menus to start and manage games -- main menu, game settings, asteroid selection, world customization, duplicant picking, save/load -- entirely through keyboard and speech. This phase builds a generic KScreen accessibility layer (plan 03-01) that later phases reuse for their management screens.

</domain>

<decisions>
## Implementation Decisions

### Navigation Model
- Arrow keys are the primary navigation (Up/Down between items, Left/Right for value adjustment)
- Wrap-around at list edges (after last item, Down goes to first) with a wrap sound (investigate game's built-in UI sounds for an appropriate click)
- Home/End for jumping to top/bottom of lists
- Nested screens are NOT manually managed -- hook into ONI's existing KScreen lifecycle (Harmony patches on OnActivate/OnDeactivate). Enter triggers the game's own button clicks, Escape triggers the game's own close behavior. Our handlers react to screen state changes, not drive them
- Tabs within a single screen use Tab/Shift+Tab. Arrow keys navigate within the active tab's content
- Focus lands on first interactive widget when a screen activates. Widget announcement is SpeakQueued behind the screen title SpeakInterrupt, so both are heard in order (same pattern as HelpHandler)
- No section jumping (Ctrl+Up/Down). Screens aren't long enough to need it
- Type-ahead search using the existing TypeAheadSearch class (ISearchable interface). A-Z keys start/extend search, Escape clears, same-letter cycling. Each screen handler implements ISearchable
- A base menu handler class is needed to avoid boilerplate across the many screens. Common patterns: arrow nav with wrap, Home/End, Enter to activate, Escape handled by game, focus tracking, ISearchable support, speech queuing on entry

### Speech Feedback Style
- Widget readout: label and value only, no type announcement. "Master Volume, 80 percent" not "Master Volume, slider, 80 percent"
- Screen entry: screen name only via SpeakInterrupt, first widget queued behind it
- No position info ever ("3 of 7"). Not even on demand
- Strip all Unity Rich Text markup before speaking. Consistent with Phase 1's text filtering pipeline
- Shift+I reads tooltip/hover info -- specifically for content that sighted players only see on mouse hover. All visually-present info is spoken during normal navigation without extra key presses. This applies to the ENTIRE phase
- Use ONI's labels as-is. No reordering or rephrasing of game strings
- No movement earcons for Up/Down navigation. Button clicks come naturally from simulating game button clicks. Investigate game's built-in sounds for a wrap-around earcon
- Value changes on sliders/dropdowns use SpeakInterrupt (cuts off previous speech for rapid adjustment)
- Filter out mouse-only UI controls (merge, hide, resize, drag handles) that are irrelevant for keyboard navigation -- researcher needs to identify these across ONI's screens
- Action button confirmation: deferred until concrete examples are encountered during research

### Widget Interaction
- Sliders: Left/Right for minor adjustment (1 step), Shift+Left/Right for increments of 10. Value spoken on each change via SpeakInterrupt
- Dropdowns: Left/Right cycles options in-place. No separate open/browse step
- Toggles/Checkboxes: Enter toggles state. New state spoken: "Fullscreen, on"
- Confirmation dialogs: treated as vertical list. Focus starts on text element (dialog message). Down navigates to OK, Cancel, etc. buttons. Wrap-around back to text. Enter activates focused button
- Text input fields: Enter to activate (clears field), type new content, Enter to confirm, Escape to cancel (restores original value). Must cache pre-edit value for Escape rollback

### Colony Setup Flow
- Mode Select (Survival/No Sweat): name and short description read together since description is visually present. "Survival, full gameplay with all challenges"
- Asteroid selection screen (ColonyDestinationSelectScreen): tabbed sections. Tab/Shift+Tab between panels: cluster selection, world seed, settings
- Cluster list entries: name, difficulty, AND world traits all read on navigation. Full info upfront
- Game Settings panel: flat list. Each setting on one line: "Disease, Default". Left/Right cycles difficulty levels
- Duplicant selection: Tab/Shift+Tab switches between the 3 dupe slots. Up/Down reads items within each slot one at a time
- Attribute readout: one per arrow press: "Athletics 3", "Cooking 0". Shift+I for description of what the attribute affects
- Trait readout: full info upfront without extra key: name, effect, and description all spoken together. "Mole Hands, +2 Digging, Moves through tiles faster"
- Interest filter dropdown and Reroll button at the bottom of each dupe's list. Left/Right on filter to cycle. Enter on Reroll
- After rerolling: speak new dupe name, interests, and traits. User navigates to read full attributes
- Save/Load screen: read colony name, cycle number, duplicant count, and date per entry. File size omitted. All visually-present info, no Shift+I needed
- World generation: periodic progress updates. "Generating world, 25 percent... 50 percent... Done"
- No extra Embark confirmation -- sighted players don't get one, neither do we
- Printing Pod selection (recurring every 3 cycles): same duplicant reading approach as initial selection, but no rerolling. Tab between candidates plus care package options

### Claude's Discretion
- Exact step size for slider adjustments (should be based on slider.wholeNumbers and range)
- Which game sounds to reuse for wrap-around earcon
- How to handle the WorldGenScreen progress polling interval
- Exact base class design for menu handlers (interface vs abstract class, which methods to make virtual)
- How to structure the KScreen handler registry (screen type -> handler mapping)

</decisions>

<specifics>
## Specific Ideas

- The TypeAheadSearch class from another project (placed at repo root) defines the exact search pattern: ISearchable interface, word-start matching, same-letter cycling, buffer timeout. Port this into the OniAccess namespace
- HelpHandler's speech pattern (SpeakInterrupt for title, SpeakQueued for first item) is the template for all screen entry announcements
- Base menu class must eliminate boilerplate -- with dozens of screens across all phases, copy-paste would be unmaintainable
- Filter out mouse-only Unity UI controls (merge, hide, resize handles) that sighted players use but have no keyboard equivalent

</specifics>

<deferred>
## Deferred Ideas

None -- discussion stayed within phase scope

</deferred>

---

*Phase: 03-menu-navigation*
*Context gathered: 2026-02-11*
