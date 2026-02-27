# BaselineHandler.cs

## File-level comment
Silent baseline handler that sits at the bottom of the handler stack.
Always present when the mod is active, ensuring the stack is never empty.
Announces "Loading" since this handler is typically active during phase
transitions before a real context handler takes over.

---

```
class BaselineHandler : IAccessHandler (line 8)

  // Properties
  string DisplayName { get; }       (line 9)
  // Returns STRINGS.ONIACCESS.HANDLERS.LOADING ("Loading").

  bool CapturesAllInput { get; }    (line 10)
  // Always false — lets all unhandled input pass through to the game.

  IReadOnlyList<HelpEntry> HelpEntries { get; }   (line 12)
  // Empty read-only list.

  IReadOnlyList<ConsumedKey> ConsumedKeys { get; } (line 15)
  // Empty array.

  // Methods
  bool Tick()                       (line 18)
  // Always returns false.

  bool HandleKeyDown(KButtonEvent e) (line 20)
  // Always returns false.

  void OnActivate()                 (line 22)
  // Speaks DisplayName via SpeechPipeline.SpeakInterrupt.

  void OnDeactivate()               (line 26)
  // No-op.
```
