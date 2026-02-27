// Central speech dispatch point. ALL speech output in the mod flows through here.
// No code calls SpeechEngine.Say() directly.
// Pipeline: Caller -> SpeechPipeline -> TextFilter -> SpeechEngine -> Tolk
static class SpeechPipeline (line 10)
  private static bool _enabled (line 15)              // when false, all methods are no-ops
  private static string _lastInterruptText (line 17)  // deduplication: last spoken interrupt text
  private static float _lastInterruptTime (line 18)   // deduplication: time of last interrupt
  private const float DeduplicateWindowSeconds = 0.05f (line 19)

  // Time source for deduplication. Defaults to Unity's unscaledTime.
  // Tests replace this to avoid native Unity calls.
  internal static System.Func<float> TimeSource (line 25)

  // Speech backend. Defaults to SpeechEngine.Say.
  // Tests replace this to capture output without P/Invoke.
  internal static System.Action<string, bool> SpeakAction (line 31)

  // Whether the pipeline is active (mod not toggled off).
  static bool IsActive => _enabled (line 37)

  // Enable or disable the pipeline. Called by ModToggle.
  internal static void SetEnabled(bool enabled) (line 43)

  // Resets deduplication state and re-enables the pipeline. Used for test isolation.
  internal static void Reset() (line 50)

  // Interrupt mode: stops current speech and speaks immediately.
  // Applies TextFilter. Deduplicates: ignores identical text within 50ms window.
  // Used for navigation and time-sensitive announcements.
  static void SpeakInterrupt(string text) (line 61)

  // Queue mode: speaks after current speech finishes (no interrupt).
  // Applies TextFilter. Used for sequential information (e.g., menu name then first item).
  static void SpeakQueued(string text) (line 80)
