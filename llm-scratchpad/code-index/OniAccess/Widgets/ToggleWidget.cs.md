// A toggle widget (KToggle, MultiToggle used as toggle, or KButton acting as toggle).
// Speaks on/off/mixed/selected/disabled state, validates interactability, clicks on activation.

namespace OniAccess.Widgets

class ToggleWidget : Widget (line 6)
  override string GetSpeechText() (line 7)
    // SpeechFunc short-circuits if set. For KToggle: uses SideScreenWalker.IsToggleActive()
    // and returns "{Label}, on/off". For MultiToggle: uses WidgetOps.GetMultiToggleState()
    // and returns "{Label}, {state}". Falls back to Label.
  override bool IsValid() (line 26)
    // Returns false if inactive in hierarchy. For KToggle, checks toggle.IsInteractable().
    // For MultiToggle, always returns true. Falls back to Component/GameObject non-null check.
  override bool Activate() (line 38)
    // Post-interaction speech is the handler's responsibility.
    // Tries in order: KToggle.Click(), WidgetOps.ClickMultiToggle(mt), WidgetOps.ClickButton(btn).
    // Returns true if any was found and clicked; false otherwise.
