# Tool System Review Findings

Written after Phase 6 quality review. Work through each item: verify bug, verify fix, apply fix.

## Bugs

### 1. Double-announcement on Escape/Enter-cancel
**Status:** TODO
HandleKeyDown(Escape) speaks "canceled" + plays sound, then calls SelectTool.Instance.Activate() which triggers OnActiveToolChanged which speaks "canceled" + plays sound again. Same issue in ConfirmOrCancel() empty path and SubmitRectangles().
**Files:** ToolHandler.cs lines 147-155, 199-204, 79-85

### 2. Harvest mode choice discarded
**Status:** TODO
ToolFilterHandler.ActivateCurrentItem() applies filter before activating the tool, but HarvestTool.OnActivateTool() calls PopulateMenu() which overwrites currentParameters. User's mode selection is always lost.
**Files:** ToolFilterHandler.cs lines 99-116

### 3. Double-push race from ToolPickerHandler
**Status:** TODO
When picker calls ActivateTool(), ActiveToolChanged fires while picker is still on stack. TileCursorHandler.OnActiveToolChanged pushes ToolHandler before picker pops. Final state is accidentally correct but produces an audible close-sound artifact after the tool announcement.
**Files:** TileCursorHandler.cs lines 123-128, ToolPickerHandler.cs lines 56-59

### 4. Wrong string on Delete/Backspace
**Status:** FIXED
ClearRectAtCursor speaks FILTER_CHANGED ("selection cleared") instead of a rectangle-cleared announcement.
**Files:** ToolHandler.cs line 267

### 5. PrioritizeToolSection nested format string
**Status:** FIXED
Passes UNDER_CONSTRUCTION ("constructing {0}") as an argument to ORDER_PRIORITY ("{0} priority {1}"), producing "constructing {0} priority 5" with a literal {0} in speech.
**Files:** PrioritizeToolSection.cs lines 33-36

### 6. Attack/Capture sections missing order state
**Status:** FIXED
Design spec requires "marked for attack" / "marked for capture" when creatures are already queued. Both sections omit this.
**Files:** AttackToolSection.cs, CaptureToolSection.cs

## DRY / Conventions

### 7. Inline string literals
**Status:** FIXED
"Wire", "Unknown", "tool", ConduitType.ToString() all spoken to user without LocString backing.
**Files:** DisconnectToolSection.cs line 23, EmptyPipeToolSection.cs line 24, ToolHandler.cs lines 347/377, EmptyPipeToolSection.cs line 17, DisconnectToolSection.cs line 17

### 8. GetNeighbor duplicated
**Status:** FIXED
Identical static method in both TileCursor.cs (line 181) and ToolHandler.cs (line 297).
**Files:** TileCursor.cs, ToolHandler.cs

### 9. Rectangle bounds unpacking duplicated
**Status:** FIXED
minX/maxX/minY/maxY extraction from RectCorners copy-pasted in IsCellSelected (244), ClearRectAtCursor (258), BuildConfirmSummary (443).
**Files:** ToolHandler.cs

### 10. GetConfirmString 12-branch dispatch
**Status:** FIXED
12-branch type dispatch mirrors BuildAllTools(). Should be a property on ModToolInfo.
**Files:** ToolHandler.cs lines 471-502, ToolInfo.cs

### 11. PlaySound duplicated
**Status:** FIXED
Identical PlaySound helper in ToolPickerHandler and ToolFilterHandler.
**Files:** ToolPickerHandler.cs lines 102-108, ToolFilterHandler.cs lines 137-143

### 12. Null-conditional on ToolProfileRegistry.Instance
**Status:** FIXED
ToolHandler.OnActivate uses ?. on ToolProfileRegistry.Instance which should never be null at that point. Violates project rule against null-conditional abuse.
**Files:** ToolHandler.cs line 57

### 13. HelpEntries duplicated
**Status:** FIXED
Both ToolPickerHandler and ToolFilterHandler define identical help entry lists instead of using BuildHelpEntries().
**Files:** ToolPickerHandler.cs lines 15-22, ToolFilterHandler.cs lines 20-27

### 14. Silent failure in ToolFilterHandler
**Status:** FIXED
When parameters null for non-Harvest tool, filter menu opens with no items and no announcement. No Log.Warn.
**Files:** ToolFilterHandler.cs lines 69-91
