# ToolPickerHandler.cs

## File-level notes
Modal menu for selecting a standard toolbar tool via type-ahead search. Lists all tools from ToolHandler.AllTools. Selecting a tool either activates it directly or opens ToolFilterHandler for mode-pick first (e.g., Harvest).

---

```
class ToolPickerHandler : BaseMenuHandler (line 12)

  // Properties
  public override string DisplayName (line 13)   // => STRINGS.ONIACCESS.TOOLS.PICKER_NAME

  // Shared help entry list used by ToolPickerHandler and ToolFilterHandler
  internal static readonly IReadOnlyList<HelpEntry> ModalMenuHelp (line 15)

  public override IReadOnlyList<HelpEntry> HelpEntries (line 24)   // => ModalMenuHelp

  public override int ItemCount (line 26)   // => ToolHandler.AllTools.Count

  public override string GetItemLabel(int index) (line 28)

  // Speaks the current tool's label directly; ignores parentContext
  public override void SpeakCurrentItem(string parentContext = null) (line 33)

  // Lifecycle: plays open sound, resets index to 0, clears search, speaks first tool name
  public override void OnActivate() (line 38)

  public override void OnDeactivate() (line 46)

  // If tool.RequiresModeFirst, replaces handler with ToolFilterHandler(tool).
  // Otherwise activates the tool and replaces handler with ToolHandler.
  protected override void ActivateCurrentItem() (line 51)

  // Intercepts Escape to speak "closed" and pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 64)

  // Lazily initialized reflected method handles
  private static System.Reflection.MethodInfo _chooseToolMethod (line 75)
  private static System.Reflection.MethodInfo _chooseCollectionMethod (line 76)

  // Activates a tool in the game's ToolMenu.
  // Primary path: finds the ToolMenu.ToolInfo by toolName, then calls ChooseCollection + ChooseTool via reflection.
  // Fallback: scans PlayerController.tools by type name and calls ActivateTool directly.
  internal static void ActivateTool(ModToolInfo tool) (line 78)

  // Plays a named KFMOD UI sound; logs on failure
  internal static void PlaySound(string name) (line 110)
```
