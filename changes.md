# Changelog

- Extension cells below Pitcher Pump and Water Trap now announce as "intake pipe" or "lure" instead of repeating the building name
- Utility ports now use descriptive labels: filters say "filtered gas output" instead of "gas output 1", overflow valves say "overflow output", preferential flow valves say "priority input"
- Gas, liquid, and solid filter side screens now present a flat alphabetical list of elements instead of confusing folded categories
- Door access control now says "passing from right to left" instead of just "left" to clarify the direction of travel
- Action buttons now include their tooltip in speech (e.g. wash basin direction buttons explain the travel direction)
- Pressing backslash on the details screen activates copy settings for the selected building, with an error message if the entity has no copyable settings
- Fixed switching tools (e.g. pressing X for deconstruct while in build mode) not activating the new tool's handler
- Tile cursor help (F12) now lists the 10 base game management screen hotkeys (Priorities, Consumables, Vitals, etc.)
- Build menu now announces material costs inline (e.g. "copper 400 kg") so you can compare buildings without opening the info screen
- Fixed dig tool announcing gas or liquid element behind buildings
- Fixed dig tool announcing "N/A" hardness for gas and liquid elements
- Fixed details screen speaking extraneous periods in Category line (e.g., "Category:. . Toilet" now reads "Category:. Toilet")
- Tile cursor now announces "unreachable" for dig, mop, and sweep orders that no duplicant can reach
- Fixed utility placement (wires, pipes) rejecting cells with existing utilities, so you can now start drags on existing lines and drag over them to reconnect
- Fixed copy building key (B) announcing "tool tool" instead of the building name
- Place tool (cargo lander placement) is now accessible: navigate with tile cursor, Space/Enter to confirm, Escape to cancel
- Copy settings tool is now accessible: Space to apply, Enter to apply and exit
- README now lists base game tool hotkeys (noting I and K are overwritten) and management screen hotkeys
