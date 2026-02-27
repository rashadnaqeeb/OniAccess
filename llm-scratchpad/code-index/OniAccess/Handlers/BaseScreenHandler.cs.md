# BaseScreenHandler.cs

## File-level comment
Abstract base for ALL screen handlers. Provides only the infrastructure
every screen type shares: a KScreen reference for ContextDetector matching,
a display name spoken on activation, and `CapturesAllInput` (all screens block
input fallthrough).

`BaseMenuHandler` extends this with widget lists, 1D navigation, type-ahead
search, tooltip reading, and widget interaction. Future 2D grid handlers extend
`BaseScreenHandler` directly.

---

```
abstract class BaseScreenHandler : IAccessHandler (line 20)

  // Fields
  protected KScreen _screen                               (line 21)

  // Properties
  KScreen Screen { get; }                                 (line 27)
  // The KScreen this handler manages. Used by ContextDetector to match a
  // deactivating screen to its handler for correct Pop behavior.

  abstract string DisplayName { get; }                    (line 33)
  abstract IReadOnlyList<HelpEntry> HelpEntries { get; }  (line 39)
  abstract bool CapturesAllInput { get; }                 (line 45)

  private static readonly IReadOnlyList<ConsumedKey> _noKeys (line 47)
  virtual IReadOnlyList<ConsumedKey> ConsumedKeys { get; }   (line 48)
  // Default: returns an empty array (no keys consumed at the base level).

  // Constructor
  protected BaseScreenHandler(KScreen screen = null)      (line 54)

  // IAccessHandler implementation
  virtual void OnActivate()                               (line 66)
  // Speaks DisplayName via SpeechPipeline.SpeakInterrupt.

  virtual void OnDeactivate()                             (line 73)
  // No-op default; subclasses override for teardown.

  virtual bool Tick()                                     (line 80)
  // No-op default; returns false. F12 help is handled centrally by KeyPoller.

  virtual bool HandleKeyDown(KButtonEvent e)              (line 87)
  // Default: returns false (pass through to game for screen close).
```
