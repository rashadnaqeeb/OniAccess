// A read-only label widget (LocText, status info, section headers).
// Always valid as long as it exists (active in hierarchy).
// Also used as a drillable parent container widget when Children is populated.

namespace OniAccess.Widgets

class LabelWidget : Widget (line 6)
  override bool IsValid() (line 7)
    // Returns false if GameObject is inactive in hierarchy; otherwise always returns true.
    // Unlike ButtonWidget/SliderWidget, does not check interactability — labels are always valid.
