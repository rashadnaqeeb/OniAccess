# Tool System Implementation Blueprint

This is the implementation reference for the OniAccess tool interaction system. Read `tool-interaction-design.md` for user-facing behavior, `tool-system.md` for game internals, and `tool-architecture.md` for high-level architectural decisions.

## Locked Decisions

- **T key** opens tool picker menu (overwrites ONI's Attack hotkey)
- **Full system** - all 12 standard toolbar tools in one implementation
- **TileCursor.ActiveToolComposer** field injected into BuildCellSpeech for tool profile priority
- **TileCursorHandler** subscribes to ActiveToolChanged event (hash 1174281782), pushes/pops ToolHandler
- **Harmony prefix on DragTool.GetConfirmSound** for confirm sound suppression (static bool flag)

## File List

### New Files (20)

```
OniAccess/Handlers/Tools/ToolInfo.cs                              -- ModToolInfo data record
OniAccess/Handlers/Tools/ToolPickerHandler.cs                     -- BaseMenuHandler, type-ahead tool list
OniAccess/Handlers/Tools/ToolFilterHandler.cs                     -- BaseMenuHandler, filter/mode list
OniAccess/Handlers/Tools/ToolHandler.cs                           -- Non-modal handler, rectangle selection
OniAccess/Handlers/Tiles/Tools/ToolProfile.cs                     -- Name + GlanceComposer pair
OniAccess/Handlers/Tiles/Tools/ToolProfileRegistry.cs             -- Maps tool Type -> GlanceComposer
OniAccess/Handlers/Tiles/Tools/Sections/SelectionSection.cs       -- "Selected" token
OniAccess/Handlers/Tiles/Tools/Sections/DigToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/MopToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/DisinfectToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/SweepToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/AttackToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/CaptureToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/HarvestToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/DeconstructToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/CancelToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/PrioritizeToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/EmptyPipeToolSection.cs
OniAccess/Handlers/Tiles/Tools/Sections/DisconnectToolSection.cs
OniAccess/Patches/DragToolPatches.cs                              -- Confirm sound suppression
```

### Modified Files (3)

```
OniAccess/Handlers/Tiles/TileCursor.cs         -- ActiveToolComposer field, JumpTo method, BuildCellSpeech mod
OniAccess/Handlers/Tiles/TileCursorHandler.cs  -- T key, ActiveToolChanged subscription, push/pop ToolHandler
OniAccess/OniAccessStrings.cs                  -- TOOLS string class
```

## Build Phases

### Phase 1: Data Types and Strings

**Files:** ToolInfo.cs, ToolProfile.cs, OniAccessStrings.cs additions

#### ModToolInfo (OniAccess/Handlers/Tools/ToolInfo.cs)

Namespace: `OniAccess.Handlers.Tools`

```csharp
public sealed class ModToolInfo {
    public string ToolName { get; }       // Game tool name, e.g. "DigTool"
    public string Label { get; }          // Display label from game strings
    public System.Type ToolType { get; }  // Game tool class type
    public bool HasFilterMenu { get; }    // FilteredDragTool subclasses + Harvest
    public bool RequiresModeFirst { get; }// true only for HarvestTool
    public bool SupportsPriority { get; } // Whether number keys set priority
    public bool IsLineMode { get; }       // true only for DisconnectTool
    public string DragSound { get; }      // "Tile_Drag" or "Tile_Drag_NegativeTool"
    public string ConfirmSound { get; }   // "Tile_Confirm", "Tile_Confirm_NegativeTool", "OutletDisconnected"
}
```

#### The 12 tools and their properties

| Tool | ToolName | HasFilter | RequiresModeFirst | SupportsPriority | IsLineMode | DragSound | ConfirmSound |
|------|----------|-----------|-------------------|------------------|------------|-----------|--------------|
| Dig | DigTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Cancel | CancelTool | true | false | false | false | Tile_Drag_NegativeTool | Tile_Confirm_NegativeTool |
| Deconstruct | DeconstructTool | true | false | true | false | Tile_Drag | Tile_Confirm |
| Prioritize | PrioritizeTool | true | false | true | false | Tile_Drag | Tile_Confirm |
| Disinfect | DisinfectTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Sweep | ClearTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Attack | AttackTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Mop | MopTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Capture | CaptureTool | false | false | true | false | Tile_Drag | Tile_Confirm |
| Harvest | HarvestTool | true | true | true | false | Tile_Drag | Tile_Confirm |
| Empty Pipe | EmptyPipeTool | true | false | true | false | Tile_Drag | Tile_Confirm |
| Disconnect | DisconnectTool | true | false | false | true | Tile_Drag_NegativeTool | OutletDisconnected |

Tool labels come from game strings: `STRINGS.UI.TOOLS.DIG.NAME`, `STRINGS.UI.TOOLS.CANCEL.NAME`, etc. The game uses ToolName field on ToolMenu.ToolInfo instances.

#### ToolProfile (OniAccess/Handlers/Tiles/Tools/ToolProfile.cs)

Namespace: `OniAccess.Handlers.Tiles.Tools`

```csharp
public sealed class ToolProfile {
    public string ToolName { get; }
    public GlanceComposer Composer { get; }
}
```

#### OniAccessStrings additions

Add nested class `TOOLS` under `ONIACCESS`:

```
PICKER_NAME = "tool menu"
FILTER_NAME = "tool filter"
CORNER_SET = "corner set"
RECT_SUMMARY = "{0} by {1}, {2} valid, {3} invalid"
ENTITY_RECT_SUMMARY = "{0} creatures"
SELECTED = "selected"
NO_CHANGE = "no change"
CANCELED = "canceled"
NO_VALID_CELLS = "no valid cells"
CONFIRM_DIG = "marked {0} for digging at priority {1}"
CONFIRM_MOP = "marked {0} for mopping at priority {1}"
CONFIRM_DISINFECT = "marked {0} for disinfection at priority {1}"
CONFIRM_SWEEP = "marked {0} for sweeping at priority {1}"
CONFIRM_DECONSTRUCT = "marked {0} for deconstruction at priority {1}"
CONFIRM_CANCEL = "cancelled {0} orders"
CONFIRM_PRIORITIZE = "updated {0} orders to priority {1}"
CONFIRM_HARVEST = "set harvest on {0} plants at priority {1}"
CONFIRM_ATTACK = "marked {0} for attack at priority {1}"
CONFIRM_CAPTURE = "marked {0} for capture at priority {1}"
CONFIRM_EMPTY_PIPE = "marked {0} pipe cells for emptying at priority {1}"
CONFIRM_DISCONNECT = "disconnected {0} segments"
PRIORITY_BASIC = "priority {0}"
PRIORITY_EMERGENCY = "emergency priority"
DIG_ORDER = "dig order"
DIG_ORDER_PRIORITY = "dig order, priority {0}"
MOP_ORDER = "mop order"
MOP_ORDER_PRIORITY = "mop order, priority {0}"
MARKED_DISINFECT = "marked for disinfect"
MARKED_SWEEP = "marked for sweep"
MARKED_SWEEP_PRIORITY = "marked for sweep, priority {0}"
MARKED_ATTACK = "marked for attack"
MARKED_CAPTURE = "marked for capture"
MARKED_DECONSTRUCT = "marked for deconstruct"
MARKED_DECONSTRUCT_PRIORITY = "marked for deconstruct, priority {0}"
MARKED_EMPTY = "marked for emptying"
DISINFECT_OBJECT = "{0}, {1}, {2}"
PIPE_EMPTY = "{0}, empty"
PIPE_CONTENTS = "{0}, {1}, {2}"
DISCONNECT_TOO_FAR = "adjacent cells only"
FILTER_CHANGED = "selection cleared"
ACTIVATION = "{0} tool, {1}"
ACTIVATION_WITH_FILTER = "{0} tool, {1}, {2}"
```

### Phase 2: Profile Infrastructure

**Files:** SelectionSection.cs, ToolProfileRegistry.cs, TileCursor.cs modifications

#### TileCursor.cs modifications

Add field:
```csharp
public GlanceComposer ActiveToolComposer { get; set; }
```

Modify BuildCellSpeech:
```csharp
private string BuildCellSpeech() {
    if (!Grid.IsVisible(_cell))
        return AttachCoordinates((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);

    GlanceComposer composer = ActiveToolComposer;
    if (composer == null) {
        var overlayScreen = OverlayScreen.Instance;
        var mode = overlayScreen != null ? overlayScreen.GetMode() : OverlayModes.None.ID;
        composer = _registry.GetComposer(mode);
    }

    string content = composer.Compose(_cell);
    if (content == null)
        content = $"{Grid.Element[_cell].name}, {Sections.ElementSection.FormatGlanceMass(Grid.Mass[_cell])}";
    return AttachCoordinates(content);
}
```

Add JumpTo method:
```csharp
public string JumpTo(int cell) {
    if (!IsInWorldBounds(cell)) return null;
    _cell = cell;
    LockMouseToCell(_cell);
    SnapCameraToCell(_cell);
    return BuildCellSpeech();
}
```

#### SelectionSection.cs

```csharp
namespace OniAccess.Handlers.Tiles.Tools.Sections
```

Queries `ToolHandler.Instance.IsCellSelected(cell)`. Returns `STRINGS.ONIACCESS.TOOLS.SELECTED` or empty.

#### ToolProfileRegistry.cs

Namespace: `OniAccess.Handlers.Tiles.Tools`

Singleton (`Instance` set in `Build()`). Maps `System.Type` to `ToolProfile`. Each tool's composer section list:

All tool profiles follow pattern: `[Selection, ToolSection, Building, Element]`
- Order, Debris, Entity suppressed by omission from section list
- Tool section handles order state and tool-specific physical state
- Building section for orientation
- Element section for element/mass (auto-suppresses when building present)

Called from `TileCursorHandler.OnActivate` alongside `OverlayProfileRegistry.Build()`.

### Phase 3: Patches

**Files:** DragToolPatches.cs

```csharp
namespace OniAccess.Patches
```

Static flag `DragToolPatches.SuppressConfirmSound`. Harmony prefix on `DragTool.GetConfirmSound()`:
- If flag is true: set `__result = null`, return false (skip original)
- If flag is false: return true (run original)

The VanillaMode guard is NOT needed here because the flag is only set by mod code (ToolHandler.SubmitRectangles). When mod is off, the flag is never set.

### Phase 4: Core Handlers

**Files:** ToolHandler.cs, TileCursorHandler.cs modifications

#### ToolHandler.cs

Namespace: `OniAccess.Handlers.Tools`

Extends BaseScreenHandler. CapturesAllInput = false.

**Key state:**
- `_pendingFirstCorner: int` (Grid.InvalidCell when none)
- `_rectangles: List<RectCorners>` where RectCorners has Cell1, Cell2
- `_toolInfo: ModToolInfo` (identified from PlayerController.Instance.ActiveTool on activation)
- `static Instance: ToolHandler` (for SelectionSection to query)

**Lifecycle:**
- OnActivate: identify tool, set Instance, set TileCursor.ActiveToolComposer, subscribe to ActiveToolChanged (hash 1174281782 on Game.Instance), speak activation announcement
- OnDeactivate: clear Instance, clear ActiveToolComposer, unsubscribe, clear rectangle state

**OnActiveToolChanged(object data):**
- If data is SelectTool: external deactivation. Speak "canceled", play "Tile_Cancel", HandlerStack.Pop()
- If data is another tool: the handler should pop and let TileCursorHandler.OnActiveToolChanged push a new one. Actually this case won't happen because tool-to-tool switch goes through SelectTool first in the game.

**Tick key handling (in order):**
1. Space -> SetCorner()
2. Enter -> ConfirmOrCancel()
3. Delete/Backspace -> ClearRectAtCursor()
4. F -> OpenFilterMenu() or swallow
5. Number keys 0-9 -> SetPriority() or swallow (based on SupportsPriority)
6. Ctrl+Arrow -> JumpToSelectionBoundary()
7. Everything else passes through (arrows reach TileCursorHandler)

**HandleKeyDown:** Consume Escape -> speak "canceled", play "Tile_Cancel", SelectTool.Instance.Activate()

**SetCorner():**
- If no pending corner: set _pendingFirstCorner = TileCursor.Instance.Cell, speak "corner set", play drag sound with tileCount=1
- If pending corner exists: create RectCorners, add to _rectangles, clear pending, speak summary, play drag sound with tileCount=area
- DisconnectTool constraint: if IsLineMode and second corner is not adjacent on same axis, reject with announcement

**ConfirmOrCancel():**
- If _rectangles.Count == 0: same as Escape (canceled, deactivate sound, SelectTool.Activate)
- If rectangles exist: SubmitRectangles()

**SubmitRectangles():**
- Get DragTool from PlayerController.Instance.ActiveTool
- Set DragToolPatches.SuppressConfirmSound = true
- For each rectangle except last: OnLeftClickDown(pos1), OnLeftClickUp(pos2)
- Clear suppression flag
- Last rectangle: OnLeftClickDown(pos1), OnLeftClickUp(pos2) -- game plays sound
- Speak confirm summary
- SelectTool.Instance.Activate() to deactivate tool

**IsCellSelected(int cell):** Checks if cell falls within any rectangle in _rectangles.

**JumpToSelectionBoundary(Direction):** Walk from current cell in direction, find first cell where IsCellSelected differs from current cell's state. Use TileCursor.Instance.JumpTo(cell).

**BuildActivationAnnouncement():** Read tool name, filter (if filtered tool), priority. Format: "Dig tool, priority 5" or "Deconstruct tool, All, priority 5".

**BuildRectSummary(RectCorners):** For cell tools: width, height, valid count, invalid count. For entity tools (Attack, Capture): creature count.

**BuildConfirmSummary():** Per-tool string from STRINGS.ONIACCESS.TOOLS.CONFIRM_*.

**Sound methods:**
- PlayDragSound(int tileCount): KFMOD.BeginOneShot with tileCount parameter
- PlayDeactivateSound(): KFMOD.PlayUISound("Tile_Cancel")

#### TileCursorHandler.cs modifications

In OnActivate (inside _hasActivated guard):
```csharp
Tools.ToolProfileRegistry.Build();
```

In OnActivate (always, after overlay subscription):
```csharp
if (Game.Instance != null)
    Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
```

In OnDeactivate:
```csharp
if (Game.Instance != null)
    Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
```

In Tick, add T key before arrow keys:
```csharp
if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.T) && !InputUtil.AnyModifierHeld()) {
    OpenToolPicker();
    return;
}
```

New methods:
```csharp
private void OpenToolPicker() {
    if (!(PlayerController.Instance.ActiveTool is SelectTool))
        SelectTool.Instance.Activate();
    HandlerStack.Push(new Tools.ToolPickerHandler());
}

private void OnActiveToolChanged(object data) {
    var tool = data as InterfaceTool;
    if (tool == null || tool is SelectTool) return;
    if (HandlerStack.ActiveHandler is Tools.ToolHandler) return;
    HandlerStack.Push(new Tools.ToolHandler());
}
```

Add T key help entry.

### Phase 5: Menu Handlers

**Files:** ToolPickerHandler.cs, ToolFilterHandler.cs

#### ToolPickerHandler.cs

Namespace: `OniAccess.Handlers.Tools`
Extends BaseMenuHandler. CapturesAllInput = true (inherited).

Static `IReadOnlyList<ModToolInfo> _tools` built once by `BuildToolList()`.

ActivateCurrentItem:
- If RequiresModeFirst (Harvest): HandlerStack.Replace(new ToolFilterHandler(tool, activateToolAfter: true))
- Else: ActivateTool(tool), HandlerStack.Pop()

ActivateTool (internal static):
- Find matching ToolMenu.ToolInfo by iterating ToolMenu.Instance.basicTools
- Call Traverse.Create(ToolMenu.Instance).Method("ChooseTool").GetValue(gameToolInfo)
- Fallback: PlayerController.Instance.ActivateTool(tool) + UISounds.PlaySound

HandleKeyDown: Consume Escape to close without activating.

#### ToolFilterHandler.cs

Namespace: `OniAccess.Handlers.Tools`
Extends BaseMenuHandler.

Two constructors:
1. ToolFilterHandler(ToolHandler owner) -- F key during tool mode
2. ToolFilterHandler(ModToolInfo toolToActivate, bool activateToolAfter) -- Harvest mode-pick

OnActivate: Read filter state from ToolParameterMenu (or hardcode Harvest modes). Set _currentIndex to currently-on filter.

ActivateCurrentItem:
- Apply filter via Traverse on ToolParameterMenu.ChangeToSetting(key)
- If activateToolAfter: ToolPickerHandler.ActivateTool(_pendingTool)
- If owner: owner.ClearSelection() (filter change clears rectangles)
- HandlerStack.Pop()

HandleKeyDown: Consume Escape to close without changing filter.

### Phase 6: Tool Sections (12 files)

All in namespace `OniAccess.Handlers.Tiles.Tools.Sections`. All implement ICellSection. All are stateless.

Each section combines order state + tool-specific physical state per the interaction design doc:

**Per-tool section readout (from tool-interaction-design.md):**

- **DigToolSection:** Order: dig order with priority. Physical: element name, material category (`element.GetMaterialCategoryTag().ProperName()`), hardness (`GameUtil.GetHardnessString`). Validity: foundation = not diggable.
- **MopToolSection:** Order: mop order with priority. Validity: too much liquid (>150kg, game string), not on floor (no solid below, game string).
- **DisinfectToolSection:** Order: "marked for disinfect" if any. Physical: per-object disease (walk all 45 layers for Disinfectable with DiseaseCount > 0, format as "name, disease, count").
- **SweepToolSection:** Order: "marked for sweep, priority N". Physical: list clearable pickupables (not dupes, not critters).
- **AttackToolSection:** Order: "marked for attack" if creature has attack mark. Physical: hostile creatures at cell (FactionAlignment.Disposition != Assist).
- **CaptureToolSection:** Physical: capturable creatures. Non-capturable get game string NOT_CAPTURABLE.
- **HarvestToolSection:** Physical: current harvest designation from ToolParameterMenu active key.
- **DeconstructToolSection:** Order: "marked for deconstruct, priority N" if marked. Physical: built objects with material prefix, scoped to active filter. Read filter from FilteredDragTool.
- **CancelToolSection:** Physical: cancellable orders scoped to active filter. Layer filters show matching built objects. CLEANANDCLEAR shows sweep/mop orders. DIGPLACER shows dig orders.
- **PrioritizeToolSection:** Physical: orders with current priorities scoped to errand filter. Construction: Constructable/Deconstructable. Digging: Diggable. Cleaning: Clearable/Moppable. Duties: everything else.
- **EmptyPipeToolSection:** Physical: conduit type + contents (element + mass), scoped to filter. Empty conduits say "type, empty".
- **DisconnectToolSection:** Physical: connection type and directions at cell, scoped to filter.

### Phase 7: Wire Into Registry

Update ToolProfileRegistry.Build() to create all 12 tool composers with correct section lists. Each tool: [Selection, ToolSection, GlanceComposer.Building, GlanceComposer.Element].

## Key Integration Points

### ToolMenu.ChooseTool activation
```csharp
// Find ToolInfo from basicTools
foreach (var row in ToolMenu.Instance.basicTools)
    foreach (var collection in row)
        foreach (var ti in collection.tools)
            if (ti.toolName == toolName) { found = ti; break; }

// Activate via Traverse (calls ChooseTool which handles toggle state + sound)
Traverse.Create(ToolMenu.Instance).Method("ChooseTool", typeof(ToolMenu.ToolInfo)).GetValue(found);
```

### Filter reading (for filtered tool sections)
```csharp
// Read active filter key from ToolParameterMenu
var menuTraverse = Traverse.Create(ToolMenu.Instance.toolParameterMenu);
var parameters = menuTraverse.Field<Dictionary<string, ToolParameterMenu.ToggleState>>("currentParameters").Value;
string activeFilter = "ALL";
foreach (var kv in parameters)
    if (kv.Value == ToolParameterMenu.ToggleState.On) { activeFilter = kv.Key; break; }
```

### Filter layer matching for objects
```csharp
// Use FilteredDragTool.GetFilterLayerFromGameObject (public method)
var tool = PlayerController.Instance.ActiveTool as FilteredDragTool;
string layer = FilteredDragTool.GetFilterLayerFromGameObject(go);
bool matches = tool.IsActiveLayer(layer);
```

Note: `GetFilterLayerFromGameObject` is a static method. `IsActiveLayer` is an instance method. Verify exact signatures in ONI-Decompiled during implementation.

### Priority reading and setting
```csharp
// Read current priority
var priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();

// Set priority
ToolMenu.Instance.PriorityScreen.SetScreenPriority(setting, play_sound: false);

// Play priority sound
PriorityScreen.PlayPriorityConfirmSound(setting);
```

### Rectangle submission
```csharp
var activeTool = PlayerController.Instance.ActiveTool as DragTool;
var pos1 = Grid.CellToPosCCC(rect.Cell1, Grid.SceneLayer.Move);
var pos2 = Grid.CellToPosCCC(rect.Cell2, Grid.SceneLayer.Move);
activeTool.OnLeftClickDown(pos1);
activeTool.OnLeftClickUp(pos2);
```

### Sound playing
```csharp
// UI sound (no position)
KFMOD.PlayUISound(GlobalAssets.GetSound("Tile_Cancel"));

// Sound with FMOD parameter
var instance = KFMOD.BeginOneShot(GlobalAssets.GetSound(soundName), worldPos);
instance.setParameterByName("tileCount", tileCount);
KFMOD.EndOneShot(instance);
```

## Risks and Mitigations

1. **ChooseTool is private** - Traverse with try/catch. Fallback: direct ActivateTool + manual sound.
2. **OnLeftClickDown disables event system** - Use try/finally with CancelDragging() on error.
3. **Entity tools use OnDragComplete** - Sections read creatures at cell. Summary walks Components.FactionAlignments/Capturables.
4. **Double-push guard** - Check `HandlerStack.ActiveHandler is ToolHandler` before pushing.
5. **GetConfirmSound is virtual** - Harmony prefix on base DragTool.GetConfirmSound may not catch overrides. Test with CancelTool and DisconnectTool. If needed, add per-tool patches.
6. **FilteredDragTool private fields** - Traverse reads. Live, not cached. Wrap in try/catch.
