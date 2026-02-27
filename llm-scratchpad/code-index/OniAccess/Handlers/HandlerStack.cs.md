# HandlerStack.cs

## File-level comment
Manages a stack of `IAccessHandler` instances.

KeyPoller and ModInputRouter walk the stack top-to-bottom using `GetAt()` +
`Count`. Each handler receives `Tick()` / `HandleKeyDown()` until one consumes
the event or a `CapturesAllInput` barrier is reached. Barriers stop the walk
(inclusive).

Push/Pop/Replace manage handler lifecycle with OnActivate/OnDeactivate callbacks.
OnDeactivate only fires when a handler is popped off.

The stack represents the current input context hierarchy, e.g.:
- `[BaselineHandler]`
- `[BaselineHandler, BuildHandler]`
- `[BaselineHandler, BuildHandler, HelpHandler]`

All methods are safe for null/empty stack.

---

```
static class HandlerStack (line 19)

  // Private fields
  private static readonly List<IAccessHandler> _stack     (line 20)
  private static readonly List<int> _pushFrames           (line 21)
  // _pushFrames tracks the frame each handler was pushed. Used by
  // RemoveStaleHandlers to grant a grace period after a push.

  // Internal test seam
  internal static Func<int> FrameSource                   (line 27)
  // Defaults to () => UnityEngine.Time.frameCount.
  // Tests replace this to avoid native Unity calls.

  // Properties
  static IAccessHandler ActiveHandler { get; }            (line 32)
  // Top of stack, or null if empty.

  static int Count { get; }                               (line 38)

  // Methods
  static IAccessHandler GetAt(int index)                  (line 44)
  // Returns the handler at index (0 = bottom, Count-1 = top).
  // Returns null for out-of-range; safe if stack mutates mid-loop.

  static void Push(IAccessHandler handler)                (line 54)
  // Pushes handler onto stack and calls handler.OnActivate().
  // Does NOT call OnDeactivate on the previous handler.
  // If OnActivate throws, speaks a failure announcement and does NOT push.

  static void Pop()                                       (line 76)
  // Removes the top handler (calls its OnDeactivate), then calls OnActivate
  // on the newly exposed handler underneath (if any). Handles exceptions at
  // both steps without crashing.

  static void Replace(IAccessHandler handler)             (line 114)
  // Removes the current top handler (OnDeactivate) then pushes a new one
  // (OnActivate). Used when switching same-level handlers without nesting.
  // Equivalent to Push if the stack is empty.

  static void DeactivateAll()                             (line 149)
  // Calls OnDeactivate on all handlers top-to-bottom, then clears the stack.
  // Used when the mod is toggled OFF.

  static void Clear()                                     (line 166)
  // Clears the stack without calling any callbacks.
  // Emergency cleanup only.

  static bool RemoveByScreen(KScreen screen)              (line 178)
  // Searches entire stack for a BaseScreenHandler whose Screen matches the
  // given KScreen. Removes it and calls OnDeactivate regardless of stack
  // position. Used by ContextDetector for buried handlers.
  // Returns true if a match was found and removed.

  static void RemoveStaleHandlers()                       (line 202)
  // Scans entire stack for BaseScreenHandlers whose KScreen is destroyed or
  // inactive. Skips show-patched screens (they use Show() not SetActive).
  // Skips handlers pushed within the last 2 frames (grace period for modal
  // transitions). Calls OnDeactivate on each removed handler.
  // Called by ContextDetector.OnScreenActivated before pushing.

  static IReadOnlyList<HelpEntry> CollectHelpEntries()    (line 233)
  // Walks stack top-to-bottom, collecting HelpEntries from each reachable
  // handler (stops at a CapturesAllInput barrier, inclusive).
  // Deduplicates by KeyName — topmost handler wins.
```
