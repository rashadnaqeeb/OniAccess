# HelpHandler.cs

## File-level comment
Handler for F12 help mode. Extends `BaseMenuHandler` for 1D navigation with
type-ahead search, Home/End, and wrap sounds. No KScreen.

Speaks help entries one at a time with Up/Down arrow navigation. Escape or
F12 returns to the previous handler.

Per locked decision: F12 opens a navigable list (arrow keys step through
entries), not a speech dump. Shows only the active handler's keys.

---

```
class HelpHandler : BaseMenuHandler (line 13)

  // Private fields
  private readonly IReadOnlyList<HelpEntry> _entries      (line 14)
  // Combined list: caller-supplied entries + _commonEntries.

  // Properties
  override string DisplayName { get; }                    (line 16)
  // Returns STRINGS.ONIACCESS.HANDLERS.HELP.

  override IReadOnlyList<HelpEntry> HelpEntries { get; }  (line 21)
  // Self-describing entries: A-Z search, Up/Down, Home/End, Escape, F12.

  private static readonly List<HelpEntry> _commonEntries  (line 33)
  // Entries appended to every help list: "Ctrl+Shift+F12" -> toggle mod.

  // Constructor
  HelpHandler(IReadOnlyList<HelpEntry> entries)           (line 38)
  // Merges caller entries with _commonEntries into a single read-only list.

  // BaseMenuHandler implementation
  override int ItemCount { get; }                         (line 46)
  override string GetItemLabel(int index)                 (line 48)
  override void SpeakCurrentItem(string parentContext = null) (line 53)

  override void OnActivate()                              (line 58)
  // Plays "HUD_Click_Open" sound, calls base (speaks DisplayName + resets state),
  // then queues the first entry (or "no commands" if list is empty).

  override bool Tick()                                    (line 67)
  // Intercepts F12 to close. Falls through to base for Up/Down/etc.

  override bool HandleKeyDown(KButtonEvent e)             (line 75)
  // Intercepts Escape via TryConsume to close. Also calls base to clear
  // search on Escape when search is active.

  private void Close()                                    (line 85)
  // Speaks "closed", plays "HUD_Click_Close", calls HandlerStack.Pop.

  private static void PlaySound(string name)              (line 91)
  // Plays a named UI sound via KFMOD. Logs on failure.
```
