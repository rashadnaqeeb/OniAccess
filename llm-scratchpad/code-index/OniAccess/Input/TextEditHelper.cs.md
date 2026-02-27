// Helper that manages inline text field editing: activates the field, speaks state changes,
// and restores the original value on cancel.
class TextEditHelper (line 2)
  bool IsEditing { get; private set; } (line 3)
  private string _cachedValue (line 4)            // original value before editing began; restored on Cancel
  private System.Func<KInputTextField> _fieldAccessor (line 5)

  // Activates the given field directly and speaks "Editing, <current text>".
  void Begin(KInputTextField field) (line 7)

  // Activates a field via lazy accessor (re-evaluated each call), enables the GameObject if needed,
  // and speaks "Editing, <current text>".
  void Begin(System.Func<KInputTextField> accessor) (line 15)

  // Deactivates the field and speaks "Confirmed, <new text>".
  // If accessor returns null, falls back to speaking "Cancelled, <cached>" and warns.
  void Confirm() (line 28)

  // Restores _cachedValue to the field, deactivates, and speaks "Cancelled, <original text>".
  // If accessor returns null, warns and speaks just "Cancelled".
  void Cancel() (line 40)
