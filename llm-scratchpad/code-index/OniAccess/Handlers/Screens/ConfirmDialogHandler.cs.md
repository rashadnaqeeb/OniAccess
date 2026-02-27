# ConfirmDialogHandler.cs

## File-level / class-level comment

Handler for confirmation dialogs (`ConfirmDialogScreen`). Also covers `InfoDialogScreen` and
other dialog types that add buttons dynamically.

Per locked decision: confirmation dialogs are a vertical list. Focus starts on the dialog message
text (Label widget), then buttons below.

`ConfirmDialogScreen` inherits `KModalScreen` (not `KButtonMenu`), so buttons/messages are found
manually via Traverse and component walks.

Key quirk: `PopupConfirmDialog` sets `popupMessage.text` **after** `StartScreen` returns (which is
after our Activate postfix fires). On the first `DiscoverWidgets` call the LocText still holds stale
prefab text, so `_firstDiscovery` forces one-frame deferral.

---

```
public class ConfirmDialogHandler : BaseWidgetHandler (line 14)

  // Fields
  private string _dialogTitle (line 15)
  private string _displayNameOverride (line 16)                     -- set at construction time; takes priority over extracted title
  private bool _firstDiscovery = true (line 17)                     -- causes DiscoverWidgets to return false on first call so popupMessage.text is set by the time we read it

  // Properties
  public override string DisplayName (line 19)                      -- priority: _displayNameOverride > _dialogTitle > game's CONFIRMNAME string
  public override IReadOnlyList<HelpEntry> HelpEntries (line 22)

  // Constructor
  public ConfirmDialogHandler(KScreen screen, string displayNameOverride = null) (line 24)

  // Lifecycle
  public override void OnActivate() (line 29)                       -- resets _firstDiscovery; calls TryExtractTitle; calls base

  // Widget discovery
  public override bool DiscoverWidgets(KScreen screen) (line 35)    -- on first call returns false to defer; then reads popupMessage or first child LocText (skipping title); adds KButtons from confirmButton/cancelButton/configurableButton fields; falls back to walking all KButton children if no named buttons found
  private bool TryAddButtonField(Traverse screenTraverse, string fieldName, string fallback) (line 119) -- resolves named GameObject field, gets KButton component, adds ButtonWidget; returns true if successful; catches and logs exceptions
  private void TryExtractTitle(KScreen screen) (line 149)           -- tries titleText field (ConfirmDialogScreen) then header field (InfoDialogScreen); sets _dialogTitle if found
```
