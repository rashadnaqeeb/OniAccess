# Tool System Review Findings

Post-implementation review. All previous findings (14 items) were resolved during development.

## Significant deviations from design

### 1. Filter scoping missing from 5 tool sections — FIXED

All 5 filtered-tool sections now check the active filter via FilteredDragTool.IsActiveLayer/GetFilterLayerFromGameObject. Cancel, Deconstruct, and Prioritize also expanded from Building+FoundationTile to all building-relevant layers (wires, conduits, logic, backwall, gantry). Prioritize walks the Pickupable linked list for sweep/mop/storage targets.

**Files:** CancelToolSection.cs, DeconstructToolSection.cs, PrioritizeToolSection.cs, EmptyPipeToolSection.cs, DisconnectToolSection.cs

### 2. Overlay-filter coupling not implemented

The design says: "If the player changes overlays while a filtered tool is active, the filter auto-switches to match [...] An overlay-triggered filter change clears any existing selection and pending first corner. The new filter is announced, followed by 'Selection cleared' if a selection existed."

ToolHandler doesn't subscribe to overlay changes. The game internally updates the filter via `FilteredDragTool.OnOverlayChanged()`, but the mod never detects this, never announces the new filter, and never clears the selection. A blind player could have a stale selection that no longer matches the new filter.

**Files:** ToolHandler.cs

### 3. NO_VALID_CELLS path not implemented

The string `NO_VALID_CELLS` exists in OniAccessStrings but is never referenced. The design says: "If no valid cells were acted on, the mod announces 'No valid cells,' plays the game's error/deactivate sound, and deactivates the tool." `SubmitRectangles()` always speaks the confirm summary regardless of outcome.

**Files:** ToolHandler.cs, OniAccessStrings.cs

### 4. DeconstructToolSection missing material prefix — NON-ISSUE

`sel.GetName()` returns the building name, which is sufficient. Material prefix is not needed for tool readouts.

## Minor oddities

### 5. Filter change doesn't announce the new filter name — FIXED

Speaks filter name on change, appends "selection cleared" only if selection existed.

**Files:** ToolFilterHandler.cs, ToolHandler.cs, OniAccessStrings.cs

### 6. CaptureToolSection doesn't check allowCapture — FIXED

Non-capturable creatures now get the game's NOT_CAPTURABLE string.

**Files:** CaptureToolSection.cs

### 7. Inline string literal in PrioritizeToolSection — FIXED

Removed defensive fallback; KSelectable.GetName() called directly.

**Files:** PrioritizeToolSection.cs

### 8. ConduitName default case returns type.ToString() — FIXED

Default case now returns UNKNOWN_ELEMENT LocString.

**Files:** DisconnectToolSection.cs
