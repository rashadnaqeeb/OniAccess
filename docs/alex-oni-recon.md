# Alex-Oni Scouting Report

Intel from reviewing alex-oni (C:\Users\rasha\Documents\alex-oni\), a parallel ONI accessibility mod. Their architecture is completely different (MonoBehaviour polling, no handler stack, manual per-screen wiring) so nothing is structurally transferable — but they've already built handlers for screens we haven't reached yet.

## Screen-Specific Intel

### CharacterContainer (Duplicant Selection)

CharacterContainer is a KScreen but is **never pushed to KScreenManager's stack**. Our ContextDetector's KScreen.Activate/Deactivate patches won't fire for it. Need a different detection mechanism (likely hook from MinionSelectScreen's OnSpawn or similar).

Each CharacterContainer has: name, interests, traits (with full descriptions), attributes, filter dropdown, reroll button. Alex uses Tab/Shift+Tab to switch between the 3 slots.

### AudioOptionsScreen

- Volume sliders are inside a SliderContainer (UIPool), not bare KSliders.
- Toggle controls (AlwaysPlayMusic, AlwaysPlayAutomation, MuteOnFocusLost) are **KButton + Checkmark child** — NOT KToggle or MultiToggle. The HierarchyReferences component holds references to the button and a checkmark indicator.
- Audio device selector is a **Unity Dropdown** (`UnityEngine.UI.Dropdown`), not any Klei widget.

### GameOptionsScreen

- The `buttons` array (from KModalButtonMenu) is **empty**. All controls are standalone serialized fields: `resetTutorialButton`, `controlsButton`, `sandboxButton`, `cameraSpeedSlider`, `doneButton`, `closeButton`.
- Temperature unit toggles use HierarchyReferences + MultiToggle checkbox pattern.
- Cloud saves toggle also uses HierarchyReferences + MultiToggle.

### GraphicsOptionsScreen

- Resolution selector is a Unity Dropdown.
- Fullscreen and low-res textures are MultiToggles.
- UI scale is a KSlider with a separate LocText label field.
- Has an Apply button (not just Done).

### CustomGameSettingWidget (Colony Setup)

Uses Left/Right value cycling — not a standard slider or toggle. Each widget has a current value label that updates on cycle.

### KInputTextField (Seed Input)

Requires a text edit mode. Enter confirms the value, Escape rolls back to previous value. Need to intercept both keys while in edit mode.

### WorldGenScreen

Zero interactive widgets. Pure progress display. Needs polling (our ITickable pattern handles this already).

### ColonyDestinationSelectScreen

Has a "customize" popup that's a child GameObject toggled via SetActive, not a separate KScreen. Escape needs to close the popup before closing the screen itself.

### ModeSelectScreen

Game mode buttons are MultiToggles. Each has a name and a longer description that should be spoken together on focus.

### ConfirmDialogScreen

Dialog title is extractable for the screen announcement. Body message should be the first navigable item (as a label). Then confirm/cancel buttons.

### LoadScreen

Two-level navigation: colony list (name, cycle, duplicant count, date) then individual saves within a colony (with auto-save / newest labels). Enter drills in, Escape backs out.

### ModsScreen

Mod entries are toggles. Also has Manage/Browse Workshop and Close buttons.

### LanguageOptionsScreen

Two sections: preinstalled languages and workshop languages. Has Uninstall/Workshop/Back/Close buttons.
