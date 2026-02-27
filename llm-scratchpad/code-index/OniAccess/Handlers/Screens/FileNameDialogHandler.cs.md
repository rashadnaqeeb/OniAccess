# FileNameDialogHandler.cs

## File-level / class-level comment

Handler for `FileNameDialog` (filename entry for new saves).

Presents a text field (Enter to begin editing, Enter to confirm, Escape to cancel) followed by OK
and Cancel buttons. Text editing handled by the base class `TextEdit` helper. This handler only
provides widget discovery and OnActivate deactivation logic.

---

```
public class FileNameDialogHandler : BaseWidgetHandler (line 14)

  // Properties
  public override string DisplayName (line 15)                      -- game's SAVENAMETITLE string
  public override IReadOnlyList<HelpEntry> HelpEntries (line 17)

  // Constructor
  public FileNameDialogHandler(KScreen screen) (line 19)

  // Lifecycle
  public override void OnActivate() (line 23)                       -- deactivates the input field (game auto-activates it; we want Enter to control editing); calls base

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 38)    -- adds TextInputWidget for inputField (SpeechFunc reads live text); adds ButtonWidget for confirmButton; adds ButtonWidget for cancelButton; each field access is in its own try/catch
```
