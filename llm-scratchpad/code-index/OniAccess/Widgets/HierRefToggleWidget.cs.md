// A toggle widget backed by a KButton + HierarchyReferences CheckMark pattern.
// Used by AudioOptionsScreen and GameOptionsScreen where on/off state is
// determined by a child "CheckMark"/"Checkmark" GameObject's visibility.

namespace OniAccess.Widgets

class HierRefToggleWidget : ToggleWidget (line 7)
  HierarchyReferences HierRef { get; set; } (line 8)
    // The HierarchyReferences component that holds the "CheckMark" or "Checkmark" reference.

  override bool IsValid() (line 10)
    // Returns false if inactive in hierarchy. For KButton component, checks isInteractable.
    // Falls back to Component/GameObject non-null check.
  override bool Activate() (line 17)
    // Clicks the KButton via WidgetOps.ClickButton. Returns false if Component is not a KButton.
  override string GetSpeechText() (line 26)
    // SpeechFunc short-circuits if set. Otherwise reads HierRef for "CheckMark" or "Checkmark"
    // child GameObject activeSelf to determine on/off state. Returns "{Label}, on/off".
    // Falls back to Label if HierRef or the reference is null.
