# Tool Interaction System - Implementation Summary

## What was built

A complete keyboard-driven tool system that replaces ONI's mouse-driven drag-to-apply tools with a screen-reader-accessible workflow. The user flow: press T to open the tool picker, select a tool, define rectangles using Enter (anchor) + arrow keys, review the selection, press Enter to confirm.

## Files created (20)

**Handlers:**
- `OniAccess/Handlers/Tools/ToolHandler.cs` - Main tool handler with rectangle selection, priority/filter management, jump navigation, confirm/cancel flow
- `OniAccess/Handlers/Tools/ToolPickerHandler.cs` - Modal menu listing all 12 tools with type-ahead search
- `OniAccess/Handlers/Tools/ToolFilterHandler.cs` - Modal menu for tool filter/mode selection
- `OniAccess/Handlers/Tools/ToolInfo.cs` - ModToolInfo data record describing each tool's properties

**Tool-specific cell readout sections (12):**
- `Sections/DigToolSection.cs`, `MopToolSection.cs`, `DisinfectToolSection.cs`, `SweepToolSection.cs`
- `Sections/AttackToolSection.cs`, `CaptureToolSection.cs`, `HarvestToolSection.cs`, `DeconstructToolSection.cs`
- `Sections/CancelToolSection.cs`, `PrioritizeToolSection.cs`, `EmptyPipeToolSection.cs`, `DisconnectToolSection.cs`

**Registry:**
- `OniAccess/Handlers/Tiles/Tools/ToolProfileRegistry.cs` - Maps tool types to GlanceComposer instances

## Files modified (3)

- `OniAccess/Handlers/Tiles/TileCursorHandler.cs` - T key opens tool picker, OnActiveToolChanged pushes ToolHandler, guards for picker/filter handlers
- `OniAccess/Handlers/Tiles/TileCursor.cs` - GetNeighbor made internal static for sharing
- `OniAccess/OniAccessStrings.cs` - ~25 new LocStrings for tool announcements, confirmations, priorities, conduit names

## Key decisions

- **Rectangle model over cell-by-cell**: User defines rectangles (anchor + extend), not individual cells. Matches how sighted users drag.
- **ConfirmFormat on ModToolInfo**: Confirmation strings are data on the tool descriptor, not a type-dispatch switch.
- **Stateless ICellSection pipeline**: Each tool section re-queries game state on every read. No caching.
- **DeactivateToolAndPop pattern**: Unsubscribe from ActiveToolChanged before calling SelectTool.Activate() to prevent double-firing.

## Unexpected things during implementation

1. **ActiveToolChanged race condition** - When ToolPickerHandler calls ActivateTool(), the game fires ActiveToolChanged synchronously while the picker is still on the handler stack. TileCursorHandler sees the event and pushes a ToolHandler before the picker replaces itself. The fix required guards in three places. This kind of synchronous event ordering is invisible without tracing.

2. **HarvestTool overwrites its own filter** - HarvestTool.OnActivateTool() calls PopulateMenu() which resets currentParameters, wiping out whatever filter the user just chose. The fix was to reorder: activate the tool first (letting it populate), then apply the filter on top. Counterintuitive -- you'd expect to set the filter then activate.

3. **Game's Direction enum namespace conflict** - When deduplicating GetNeighbor, the game has its own global Direction enum that shadows OniAccess.Handlers.Tiles.Direction even through a using directive. Required explicit Tiles.Direction qualification in ToolHandler.

4. **string.Format nesting trap** - UNDER_CONSTRUCTION is "constructing {0}" and ORDER_PRIORITY is "{0} priority {1}". Passing one as an argument to the other produces "constructing {0} priority 5" -- the inner {0} becomes a literal. Easy to write, hard to catch without hearing the output.

## Quality review

14 findings identified and fixed across three review passes (simplicity/DRY, bugs/correctness, conventions). See review-findings.md for the full list.
