// MonoBehaviour that drives per-frame Tick() on the active handler stack.
// Also owns the Ctrl+Shift+F12 toggle key, which must work even when the mod is off.
// All UnityEngine.Input calls are fully qualified to avoid resolving to this namespace.
class KeyPoller : UnityEngine.MonoBehaviour (line 13)
  static KeyPoller Instance { get; private set; } (line 14)
  private bool _startupDone (line 16)

  private void Awake() (line 18)

  // Per-frame update:
  // 1. Always check Ctrl+Shift+F12 toggle first.
  // 2. Skip rest when mod is off.
  // 3. One-time startup: find MainMenu if Harmony missed its Activate (fires before patches load).
  // 4. Remove stale handlers from the stack (destroyed or hidden without Deactivate).
  // 5. F12 (no modifiers): collect help entries from all reachable handlers and push HelpHandler.
  // 6. Walk stack top-to-bottom ticking each handler; stop at CapturesAllInput barrier or stack mutation.
  private void Update() (line 22)

  private void OnDestroy() (line 78)
