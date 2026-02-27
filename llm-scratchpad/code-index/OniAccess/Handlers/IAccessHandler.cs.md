# IAccessHandler.cs

## File-level comment
Interface for mod input handlers in the handler stack.

Each handler detects its own keys via `UnityEngine.Input.GetKeyDown()` in `Tick()`,
called once per frame by KeyPoller. `HandleKeyDown` processes KButtonEvents from
ModInputRouter (primarily Escape interception via `e.TryConsume`).

KeyPoller and ModInputRouter walk the stack top-to-bottom. Each handler gets
`Tick()` / `HandleKeyDown()` until one consumes the event or a `CapturesAllInput`
barrier is reached. Barriers stop the walk (inclusive): handlers below a barrier
receive nothing. Handlers with `CapturesAllInput=false` let unhandled keys fall
through to handlers below, and ultimately to the game.

---

```
interface IAccessHandler (line 19)
  string DisplayName { get; }                             (line 24)
  // Display name spoken on activation. Name-first, vary-early convention.

  bool CapturesAllInput { get; }                          (line 31)
  // When true, ModInputRouter consumes all keyboard events except mouse and Escape.

  IReadOnlyList<HelpEntry> HelpEntries { get; }           (line 36)
  // Help entries for the F12 navigable help list.

  IReadOnlyList<ConsumedKey> ConsumedKeys { get; }        (line 43)
  // Keys the handler uses in Tick(); router pre-consumes matching KButtonEvents.

  bool Tick()                                             (line 51)
  // Called once per frame. All key detection and mod logic lives here.
  // Returns true if input was consumed (stops the KeyPoller walk).

  bool HandleKeyDown(KButtonEvent e)                      (line 58)
  // Processes a KButtonEvent from ONI's input system.
  // Primarily used for Escape interception via e.TryConsume(Action.Escape).
  // Returns true to consume and stop the walk.

  void OnActivate()                                       (line 64)
  // Called when this handler becomes active on the stack.
  // Convention: speak DisplayName here (interrupts current speech).

  void OnDeactivate()                                     (line 69)
  // Called when this handler is removed from the active position.
```
