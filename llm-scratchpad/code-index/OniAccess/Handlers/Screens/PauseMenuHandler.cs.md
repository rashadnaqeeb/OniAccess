# PauseMenuHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for the pause menu (`PauseScreen`). `PauseScreen` inherits `KModalButtonMenu` (-> `KButtonMenu`), so the `buttons` array pattern is used: `KButtonMenu.buttons` provides `ButtonInfo` labels, `KButtonMenu.buttonObjects` provides the GameObjects with `KButton` components. `RefreshButtons()` destroys cached references, but `DiscoverWidgets` is called fresh on `OnActivate`, so references are always current.

---

## class PauseMenuHandler : BaseWidgetHandler (line 15)

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.PAUSE_MENU` (line 16)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 18)

  **Constructor**
  - `public PauseMenuHandler(KScreen screen) : base(screen)` (line 20)

  **Lifecycle**
  - `public override void OnActivate()` (line 24) — calls base, then reads `worldSeed` LocText from the screen and queues its text (displays game coordinates/seed info to the player)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 35) — reads `buttons` IList and `buttonObjects` array; pairs them by index; adds a `ButtonWidget` for each active, interactable `KButton` with a non-empty `ButtonInfo.text` label
