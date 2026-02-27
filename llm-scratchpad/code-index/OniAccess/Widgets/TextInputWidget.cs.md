// A text input widget (KInputTextField, KNumberInputField, KInputField).
// Resolves the underlying input field for text editing.

namespace OniAccess.Widgets

class TextInputWidget : Widget (line 6)
  KInputTextField GetTextField() (line 11)
    // Resolves the underlying KInputTextField from the component, handling indirection:
    //   - Direct KInputTextField cast
    //   - KNumberInputField.field
    //   - KInputField.field
    // Returns null if none of the above match.
