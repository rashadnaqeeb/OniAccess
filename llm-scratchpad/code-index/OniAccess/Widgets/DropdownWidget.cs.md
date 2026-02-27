// A dropdown/radio-group widget. Supports Left/Right cycling.
// Cycling logic stays in handlers (too handler-specific).
// Used by SideScreenWalker when it collapses a set of KToggle radio buttons
// into a single navigable item. The Tag property holds List<RadioMember>.

namespace OniAccess.Widgets

class DropdownWidget : Widget (line 6)
  override bool IsAdjustable (line 7)
    // Always returns true. Handlers implement Adjust() by cycling through Tag (List<RadioMember>).
