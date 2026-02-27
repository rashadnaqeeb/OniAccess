# TranslationHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `LanguageOptionsScreen` (language/translation selection from options menu). Reads language buttons from the screen's private `buttons` list (dynamically-created, not stale prefab templates). Action buttons (Workshop, Uninstall, Dismiss) are separate serialized fields.

Language buttons are created in `OnSpawn`, which runs after the Harmony postfix on `Activate` — `BaseWidgetHandler`'s deferred rediscovery handles the timing automatically.

The currently active language's button gets a `SpeechFunc` that prepends "selected" to the label. `GetSelectedButtonName` identifies the active language by inspecting `Localization.GetSelectedLanguageType()` and resolving preinstalled buttons as `"{code}_button"` or UGC buttons as `"{mod.title}_button"`.

---

## class TranslationHandler : BaseWidgetHandler (line 24)

  **Properties**
  - `public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.TRANSLATIONS` (line 25)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 27)

  **Constructor**
  - `public TranslationHandler(KScreen screen) : base(screen)` (line 29)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 33) — gets `selectedButtonName` from `GetSelectedButtonName()`; iterates the screen's `buttons` list; reads `HierarchyReferences "Title"` LocText for each button label; marks the selected button's widget with a `SpeechFunc` that prepends "selected"; appends `workshopButton`, `uninstallButton`, `dismissButton`
  - `private string GetSelectedButtonName()` (line 91) — returns the `go.name` of the active language button; for Preinstalled type uses `"{code}_button"`; for UGC type finds the matching mod by label ID and uses `"{mod.title}_button"`; returns null if None or unresolvable
