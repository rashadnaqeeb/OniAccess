# ONI Hotkey Reference

Reference of all ONI key bindings for deciding what to overwrite in each phase.
Source: Phase 1 research (decompiled `Action` enum and `BindingEntry` system).

## Key Binding Groups

ONI organizes bindings into groups. Each group represents a context where those bindings are active:

| Group | Context | Examples |
|-------|---------|---------|
| Root | Always active | Escape, Mouse, Zoom, Pause, Speed |
| Navigation | Camera panning | WASD (PanUp/Down/Left/Right), Home |
| Tool | When tool is selected | Mouse clicks, Drag |
| BuildMenu | Build menu navigation | BuildMenuUp/Down/Left/Right, letter keys A-Z |
| Sandbox | Sandbox mode | Various sandbox tools |
| Debug | Debug mode | Various debug commands |
| CinemaMode | Cinema camera | Camera controls |
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
| H | Help | Available in other contexts |
| Ctrl+1-0 | SetUserNav1-10 | Camera bookmarks |
| Shift+1-0 | GotoUserNav1-10 | Camera bookmark recall |

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
| B | CopyBuilding | |
| O | RotateBuilding | |
| Ctrl+S | ScreenShot1x | |

## Build Menu

| Key | Action | Notes |
|-----|--------|-------|
| 1-0 | Plan1-10 (build categories) | Active only when build menu visible |
| A-Z | BuildMenuKeyA-Z | Active only in build submenu |

## Keys NOT Used by ONI (safe for mod)

| Key | Notes |
|-----|-------|
| Arrow keys | NOT bound to any Action. Camera uses WASD. Safe for mod cursor. |
| Insert | Not bound (also screen reader modifier -- avoid) |
| CapsLock | Modifier enum includes it but not bound as action (also screen reader modifier -- avoid) |
| Backtick/Tilde | Listed as Modifier but not bound to actions |
| Page Up/Down | Not bound to any Action |
| Ctrl+Arrow keys | Not bound |
| Alt+Arrow keys | Not bound |
| Most letter keys with Alt | Not bound to standard actions |

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
| Toggle mod on/off | Ctrl+Shift+F12 | 1 | No game conflict, works in all contexts |
| Context help | F12 | 1 | Not used by overlays (F1-F11), not used by game |

Further hotkeys assigned per-phase as features are built.
