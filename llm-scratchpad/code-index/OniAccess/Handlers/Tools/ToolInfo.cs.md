# ToolInfo.cs

## File-level notes
Immutable descriptor for a standard toolbar tool. Carries all behavior flags and delegates that the handler infrastructure needs to drive the tool — no game state, no caching.

---

```
sealed class ModToolInfo (line 7)

  // Properties (all get-only, set in constructor)
  public string ToolName { get; }          (line 8)   // game's ToolMenu.ToolInfo.toolName, used to find the tool
  public string Label { get; }             (line 9)   // localized display name spoken in menus
  public Type ToolType { get; }            (line 10)  // runtime type of the game's InterfaceTool subclass
  public bool HasFilterMenu { get; }       (line 11)  // true if tool has a ToolParameterMenu filter
  public bool RequiresModeFirst { get; }   (line 12)  // true if filter must be chosen before tool activates (e.g., Harvest)
  public bool SupportsPriority { get; }    (line 13)  // true if priority keys 0-9 apply to this tool
  public bool IsLineMode { get; }          (line 14)  // true if second corner must be orthogonally adjacent (DisconnectTool)
  public string DragSound { get; }         (line 15)  // FMOD event name played when a corner/rectangle is set
  public string ConfirmSound { get; }      (line 16)  // FMOD event name played on confirm (currently unused by handler; game plays it)
  public string ConfirmFormat { get; }     (line 17)  // format string for the confirm announcement (uses count, priority, noun args)
  public Func<int, int> CountTargets { get; } (line 18) // delegate(cell) -> target count at that cell for valid-cell feedback

  public ModToolInfo(
      string toolName,
      string label,
      Type toolType,
      bool hasFilterMenu,
      bool requiresModeFirst,
      bool supportsPriority,
      bool isLineMode,
      string dragSound,
      string confirmSound,
      string confirmFormat,
      Func<int, int> countTargets
  ) (line 20)
```
