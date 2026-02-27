# ToolFilterHandler.cs

## File-level notes
Modal menu for selecting a tool filter/mode. Two use cases:
1. Opened from ToolHandler (F key) to change filter for the active tool.
2. Opened from ActionMenuHandler/ToolPickerHandler for tools that require mode-pick before activation (e.g., Harvest: "when ready" vs "do not harvest").

---

```
class ToolFilterHandler : BaseMenuHandler (line 12)

  // Constants — hardcoded filter keys used when ToolParameterMenu has no live parameters (Harvest pre-activation)
  internal const string HarvestWhenReadyKey = "HARVEST_WHEN_READY" (line 13)
  internal const string DoNotHarvestKey = "DO_NOT_HARVEST" (line 14)

  // Fields
  private readonly ToolHandler _owner (line 16)           // non-null when changing filter for an active tool
  private readonly ModToolInfo _pendingTool (line 17)     // non-null when picking mode before activation
  private List<string> _filterKeys (line 18)
  private List<string> _filterNames (line 19)

  // Properties
  public override string DisplayName (line 21)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 23)   // => ToolPickerHandler.ModalMenuHelp

  // Constructor — change filter for an active tool (F key in tool mode)
  public ToolFilterHandler(ToolHandler owner) (line 28)

  // Constructor — pick mode before activating a tool (e.g., Harvest from tool picker)
  public ToolFilterHandler(ModToolInfo pendingTool) (line 36)

  public override int ItemCount (line 41)

  public override string GetItemLabel(int index) (line 43)

  // Speaks the current filter name directly; ignores parentContext
  public override void SpeakCurrentItem(string parentContext = null) (line 48)

  // Lifecycle: plays open sound, reads currentParameters from ToolParameterMenu via Traverse.
  // Special case: if no parameters and pendingTool is HarvestTool, uses hardcoded HARVEST_WHEN_READY / DO_NOT_HARVEST keys.
  // Positions cursor on the currently active filter (ToggleState.On). Pops immediately if no filters found.
  public override void OnActivate() (line 53)

  public override void OnDeactivate() (line 95)

  // Reflected method handles (lazily initialized)
  private static System.Reflection.MethodInfo _changeToSettingMethod (line 100)
  private static System.Reflection.MethodInfo _onChangeMethod (line 101)

  // Applies selected filter via reflected ChangeToSetting + OnChange on ToolParameterMenu.
  // If _pendingTool is set, activates it first via ToolPickerHandler.ActivateTool.
  // _owner path: clears selection, announces filter + "selection cleared" if had selection, pops.
  // _pendingTool path: replaces handler with new ToolHandler.
  protected override void ActivateCurrentItem() (line 103)

  // Intercepts Escape to speak "closed" and pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 135)

  private static void PlaySound(string name) (line 146)   // delegates to ToolPickerHandler.PlaySound
```
