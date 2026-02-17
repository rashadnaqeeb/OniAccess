# ONI Hotkey Reference

Reference of all ONI key bindings for deciding what to overwrite in each phase.
Source: (decompiled `Action` enum and `BindingEntry` system), supplemented by ONI wiki and community guides.
Source: `oni-src/Global.cs` - `GenerateDefaultBindings()` method

## Key Binding Groups

ONI organizes bindings into groups. Each group represents a context where those bindings are active:

| Group | Context | Examples |
|-------|---------|---------|
| Root | Always active | Escape, Mouse, Zoom, Pause, Speed |
| Navigation | Camera panning | WASD (PanUp/Down/Left/Right), Home |
| Tool | When tool is selected | Mouse clicks, Drag, Rotate, Shift for straight lines |
| BuildMenu | Build menu navigation | BuildMenuUp/Down/Left/Right, letter keys A-Z |
| Building | When building is selected | Toggle Open, Toggle Enabled, Utility keys |
| Sandbox | Sandbox mode | Various sandbox tools |
| Debug | Debug mode | Various debug commands |
| CinemaMode | Cinema camera (Alt+S) | Camera controls, zoom, easing |
| Management | Management screens | Priorities, Consumables, Vitals, etc. |

## Critical Game Bindings (Root group - always active)

| Key | Action | Notes for Mod |
|-----|--------|---------------|
| Escape | Close/Back | Cannot override |
| Space | TogglePause | Could conflict with mod actions |
| Tab | CycleSpeed | Available in menus if not in Root |
| Numpad +/- | SpeedUp/SlowDown | Safe, numpad |
| F1-F11 | Overlay1-11 | HEAVILY USED, do not override |
| Shift+F1-F4 | Overlay12-15 | Additional overlays |
| H | CameraHome (Printing Pod) | Navigates camera to the Printing Pod |
| Ctrl+1-0 | SetUserNav1-10 | Camera bookmarks |
| Shift+1-0 | GotoUserNav1-10 | Camera bookmark recall |
| Left Alt / Right Alt | Scroll saved locations | Cycles through saved camera bookmarks |
| Mouse Scroll Up | Zoom In | |
| Mouse Scroll Down | Zoom Out | |
| Alt+S | Toggle Screenshot Mode | Enters Cinema Mode (see CinemaMode section) |

## Camera/Navigation (always active during gameplay)

| Key | Action | Notes |
|-----|--------|-------|
| W | PanUp | The mod cursor should NOT use WASD -- conflict with camera |
| A | PanLeft | Same |
| S | PanDown | Same |
| D | PanRight | Same |

## Management Screen Hotkeys

| Key | Action |
|-----|--------|
| L | ManagePriorities |
| F | ManageConsumables |
| V | ManageVitals |
| R | ManageResearch |
| Period (.) | ManageSchedule |
| J | ManageSkills |
| E | ManageReport |
| U | ManageDatabase (Codex) |
| Z | ManageStarmap |
| N | ManageDiagnostics |

## Tool Commands

| Key | Action | Notes |
|-----|--------|-------|
| G | Dig | |
| C | BuildingCancel | |
| X | BuildingDeconstruct | |
| P | Prioritize | |
| M | Mop | |
| K | Clear (Sweep) | K is also a common "read info" key in accessibility mods |
| I | Disinfect | |
| T | Attack | |
| N | Capture (Wrangle) | NOTE: conflicts with ManageDiagnostics -- context-dependent |
| Y | Harvest | |
| Insert | Empty Pipe | ALSO a screen reader modifier -- see avoidance list below |
| B | CopyBuilding | |
| O | RotateBuilding | Active when placing a building |
| Shift (hold) | Drag Straight | Constrains tool drag to straight lines |
| Ctrl+S | ScreenShot1x | |

## Building Interaction Keys (when a building is selected)

| Key | Action | Notes |
|-----|--------|-------|
| / | Toggle Open | Opens/closes doors, valves, etc. |
| Enter | Toggle Enabled | Enables/disables the selected building |
| \ | Building Utility 1 | Context-dependent (e.g., Copy Settings) |
| [ | Building Utility 2 | Context-dependent |
| ] | Building Utility 3 | Context-dependent |

## Build Menu

| Key | Action | Notes |
|-----|--------|-------|
| 1-0 | Plan1-10 (build categories) | Active only when build menu visible |
| - | Plan 11 | Additional build category |
| = | Plan 12 | Additional build category |
| Shift+- | Plan 13 | Additional build category |
| Shift+= | Plan 14 | Additional build category |
| A-Z | BuildMenuKeyA-Z | Active only in build submenu |

## Cinema Mode Keys (active after Alt+S)

| Key | Action | Notes |
|-----|--------|-------|
| E | Cinema Easing toggle | On/Off |
| T | Cinema Input Lock toggle | On/Off |
| I | Zoom In | |
| O | Zoom Out | |
| P | Cinema Unpause Next Move | On/Off |
| Z | Camera Zoom Speed increase | |
| Shift+Z | Camera Zoom Speed decrease | |
| C | Cinema Cam toggle | Enabled/Disabled |

## Debug Mode Keys (only active when Debug Mode is enabled)

These are only relevant if debug mode is active, but listed for completeness to avoid conflicts.

| Key | Action |
|-----|--------|
| Backspace | Debug Toggle |
| Ctrl+T | Debug Focus |
| Ctrl+U | Debug Ultra Test Mode |
| Ctrl+Q | Debug Goto Target |
| Ctrl+S | Debug Select Material |
| Ctrl+M | Debug Toggle Music |
| Alt+F1 | Debug Toggle UI |
| Alt+F3 | Debug Collect Garbage |
| Alt+F7 | Debug Invincible |
| Alt+F10 | Debug Force Light Everywhere |
| Alt+N | Debug Refresh Nav Cell |
| Alt+Q | Debug Teleport |
| Alt+T | Debug Toggle SelectIn Editor |
| Alt+P | Debug Path Finding |
| Alt+Z | Debug Super Speed |
| Alt+= | Debug Game Step |
| Alt+- | Debug Sim Step |
| Alt+X | Debug Notification |
| Alt+C | Debug Notification Message |
| Alt+5 | Debug Lock Cursor |
| Alt+9 | Debug Next Call |
| Alt+0 | Debug Toggle Personal Priority Comparison |
| Alt+1 | ScreenShot1x |
| Alt+2 | ScreenShot2x |
| Alt+3 | ScreenShot8x |
| Alt+4 | ScreenShot32x |
| Backtick/Tilde | Toggle Profiler |
| Alt+Backtick | Toggle Chrome Profiler |
| Ctrl+F1 | Debug Dump Scene Partitioner Leak Data |
| Ctrl+F2 | Debug Spawn Minion |
| Ctrl+F3 | Debug Place |
| Ctrl+F4 | Debug Instant Build Mode |
| Ctrl+F5 | Debug Slow Test Mode |
| Ctrl+F6 | Debug Dig |
| Ctrl+F8 | Debug Explosion |
| Ctrl+F9 | Debug Discover All Elements |
| Ctrl+F11 | Debug Dump Event Data |
| Ctrl+F12 | Debug Trigger Exception |
| Ctrl+Shift+F12 | Debug Trigger Error |
| Ctrl+10 | Debug Dump GC Roots |
| Alt+Ctrl+F7 | Debug Crash Sim |
| Alt+Ctrl+F10 | Debug Dump Garbage References |
| Shift+F1 | Debug Visual Test |
| Shift+F10 | Debug Element Test |
| Shift+F11 | Debug River Test |
| Shift+F12 | Debug Tile Test |

## Steam Overlay Keys (external to ONI, defined by Steam)

| Key | Action | Notes |
|-----|--------|-------|
| F12 | Steam Screenshot | CONFLICTS with mod's Context Help -- see note below |
| Shift+Tab | Steam Overlay | |

**NOTE:** F12 is bound to Steam Screenshot by default. The mod currently assigns F12 to Context Help. Users may need to rebind Steam's screenshot key, or the mod could use a different key. This is configurable in Steam settings.

## Keys NOT Used by ONI (safe for mod)

| Key | Notes |
|-----|-------|
| Arrow keys | NOT bound to any Action. Camera uses WASD. Safe for mod cursor. |
| Insert | Not bound to a standard Action (only Empty Pipe, which is rarely used). HOWEVER, also a screen reader modifier -- avoid. |
| CapsLock | Modifier enum includes it but not bound as action (also screen reader modifier -- avoid) |
| Backtick/Tilde | Used by Debug Profiler only -- safe in non-debug mode |
| Page Up/Down | Not bound to any Action |
| Ctrl+Arrow keys | Not bound |
| Alt+Arrow keys | Not bound |
| Most letter keys with Alt | Not bound to standard actions (some used by Debug) |

**Arrow keys are completely free in ONI's default bindings.** They are the natural choice for the accessibility cursor, matching screen reader conventions where arrows mean "navigate current context."

## Screen Reader Keys to AVOID

| Key | Used By | Must Avoid |
|-----|---------|------------|
| Insert | NVDA modifier | Absolute -- never bind |
| CapsLock | NVDA modifier (alternate), JAWS modifier | Absolute -- never bind |
| Insert+letter | NVDA commands | Avoid Insert combinations |
| NVDA+Space | Toggle NVDA speech | Avoid |

## Mod Hotkeys Assigned So Far

| Action | Key | Phase | Rationale |
|--------|-----|-------|-----------|
| Toggle mod on/off | Ctrl+Shift+F12 | 1 | No game conflict, works in all contexts. NOTE: Ctrl+Shift+F12 is Debug Trigger Error in debug mode -- low risk since debug mode is rare |
| Context help | F12 | 1 | Not used by overlays (F1-F11), not used by game. NOTE: Conflicts with Steam Screenshot -- users may need to rebind in Steam |
| Read coordinates | ` (backtick) | tile | Overwrites debug profiler toggle (debug mode only, opt-in and rare). Reads current cell X,Y position |
| Cycle coordinate mode | Shift+` | tile | Cycles Off -> Append -> Prepend -> Off. Attaches coordinates to glance announcements |

Further hotkeys assigned per-phase as features are built.

## Key Conflict Notes

This section documents known conflicts and context-dependent double bindings.

| Key | Binding 1 | Binding 2 | Resolution |
|-----|-----------|-----------|------------|
| N | Capture (Wrangle) -- Tool context | ManageDiagnostics -- Management context | Context-dependent, only one active at a time |
| Insert | Empty Pipe -- Tool context | NVDA modifier -- Screen reader | Avoid binding; Empty Pipe rarely used |
| Ctrl+Shift+F12 | Mod toggle | Debug Trigger Error -- Debug context | Low risk; debug mode is opt-in and rare |
| F12 | Mod Context Help | Steam Screenshot -- Steam overlay | Users can rebind Steam screenshot in Steam settings |
| Shift+F1 through Shift+F4 | Overlay 12-15 -- Root context | Debug Visual Test (Shift+F1), etc. -- Debug context | Debug keys only active in debug mode |
