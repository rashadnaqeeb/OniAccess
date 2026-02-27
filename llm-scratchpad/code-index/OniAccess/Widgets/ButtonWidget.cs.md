// A clickable button widget (KButton or MultiToggle used as button).

namespace OniAccess.Widgets

class ButtonWidget : Widget (line 5)
  override bool IsValid() (line 6)
    // Returns false if inactive in hierarchy. For KButton, checks btn.isInteractable.
    // For MultiToggle, always returns true. Falls back to Component/GameObject non-null check.
  override bool Activate() (line 14)
    // Clicks KButton via WidgetOps.ClickButton, or MultiToggle via WidgetOps.ClickMultiToggle.
    // Returns true if either component was found and clicked; false otherwise.
