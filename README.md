# OniAccess

An accessibility mod for Oxygen Not Included that makes the game playable for blind users. All game information is delivered through speech output via your screen reader (using the Tolk bridge). The mod is read-only -- it doesn't change game behavior, only adds speech. Toggle it on or off at any time with Ctrl+Shift+F12.

## Install

Copy the mod DLL to your local mods directory. The build script handles this automatically for development. Full install instructions will be added when the install process is finalized.

## Quick start

- **Ctrl+Shift+F12** toggles the mod on/off
- **F12** opens context-sensitive help anywhere (lists available keys for the current screen)
- **Arrow keys** navigate everything -- tile cursor in gameplay, list items in menus
- **Enter** activates or inspects
- **Escape** closes screens
- **Tab** opens the action/build menu from gameplay
- Speech is interrupt-based: moving quickly cuts off the previous announcement

## Hotkey reference

### Global

| Key | Action |
|---|---|
| Ctrl+Shift+F12 | Toggle mod on/off |
| F12 | Open context help |

### Gameplay - Tile Cursor

| Key | Action |
|---|---|
| Arrow keys | Move cursor one tile |
| Ctrl+Arrow | Skip to next change |
| Tab | Open action/build menu |
| Enter | Inspect entity at cursor |
| I | Read tile tooltip summary |
| Shift+I | Browse full tile tooltip |
| K | Read coordinates |
| Shift+K | Cycle coordinate mode (Off / Append / Prepend) |
| Q | Read cycle status |
| Shift+Q | Read time played |
| S | Read colony status (dupe count, rations, stress, etc.) |
| Shift+P | Read pinned resources |
| BackQuote (`) | Cycle game speed |
| Ctrl+R | Toggle red alert |
| Ctrl+B | Place alignment ruler at cursor |
| Ctrl+Shift+B | Clear alignment ruler |
| Shift+N | Open notification menu |
| H | Jump cursor to Printing Pod |
| Shift+1-0 | Jump to camera bookmark |
| Alt+1-0 | Read direction/distance to bookmark |
| Ctrl+1-0 | Set camera bookmark (game-native) |
| [ / ] | Cycle through duplicants |
| \\ | Jump to duplicant / select if already there |

### Scanner (during gameplay)

| Key | Action |
|---|---|
| Ctrl+PageUp/Down | Cycle category |
| Shift+PageUp/Down | Cycle subcategory |
| PageUp/Down | Cycle item |
| Alt+PageUp/Down | Cycle instance |
| Home | Teleport to current instance |
| Shift+Home | Toggle auto-move |
| End | Refresh entries |
| Ctrl+F | Search within category |

### Tool Mode (after selecting a tool)

| Key | Action |
|---|---|
| Space | Set rectangle corner |
| Enter | Confirm selection (or single-cell order) |
| Delete/Backspace | Clear rectangle |
| 0-9 | Set priority |
| F | Open filter menu |
| Escape | Cancel |

### Menu Screens

| Key | Action |
|---|---|
| Up/Down | Navigate items |
| Home/End | First/last item |
| Enter | Activate |
| Left/Right | Adjust or drill in/back |
| Tab/Shift+Tab | Cycle sections/tabs |
| Ctrl+Up/Down | Jump between groups |
| A-Z | Type-ahead search |
| Escape | Close (or clear search) |

## Settings

- **Coordinate mode** (Shift+K): Off, Append, or Prepend. Controls whether tile coordinates are spoken with cell glances.
- **Auto-move cursor** (Shift+Home): When the scanner is active, automatically teleports the cursor on item cycle.
- **Mod toggle** (Ctrl+Shift+F12): Disables all speech. The game behaves as if the mod isn't installed.

## Supported screens

- Main Menu
- Colony Setup (mode, cluster, destination selection)
- Duplicant Selection (new game)
- Tile Cursor (primary gameplay)
- Entity Inspection (details screen with tabs: Status, Personality, Chores, Properties, Config, Errands, Material, Blueprint, Actions)
- Action/Build Menu (3-level hierarchy: categories > subcategories > buildings)
- Tool Mode (dig, cancel, deconstruct, prioritize, etc. with rectangle selection)
- Priority Screen (L) -- 2D grid
- Vitals Screen (V) -- 2D grid
- Consumables Screen (F) -- 2D grid
- Research Screen (R) -- Browse, Queue, Tree tabs
- Skills Screen (J) -- Duplicants, Skills, Tree tabs
- Schedule Screen (.)
- Report Screen (E)
- Codex/Database (U)
- Starmap (Z)
- Diagnostics (N)
- Printing Pod / Immigration
- Notification Menu (Shift+N)
- Pause Menu
- Options/Settings
- Context Help (F12)

## Known limitations

This section will be updated as the mod matures.

## Troubleshooting

- **Player log location**: `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`
- **Mod log lines** are prefixed with `[OniAccess]`
- If speech stops working, try toggling the mod off and on with Ctrl+Shift+F12
- If a screen isn't being read, check that your screen reader is running and try F12 to see if the mod recognizes the current context
