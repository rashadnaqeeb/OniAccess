// Tolk P/Invoke wrapper. Passes text directly to the screen reader without filtering.
// All filtering is done by TextFilter before reaching this class.
// Call order: SpeechPipeline -> TextFilter -> SpeechEngine -> Tolk -> screen reader.
static class SpeechEngine (line 12)
  // P/Invoke declarations for Tolk.dll (CallingConvention.Cdecl)
  [DllImport] private static extern void Tolk_Load() (line 15)
  [DllImport] private static extern void Tolk_Unload() (line 18)
  [DllImport] private static extern bool Tolk_Output(string str, bool interrupt) (line 21)
  [DllImport] private static extern bool Tolk_TrySAPI(bool trySAPI) (line 24)
  [DllImport] private static extern bool Tolk_HasSpeech() (line 27)
  [DllImport] private static extern IntPtr Tolk_DetectScreenReader() (line 30)
  [DllImport] private static extern bool Tolk_Silence() (line 33)

  private static bool _initialized (line 36)
  private static bool _available (line 37)

  // Whether Initialize() has been called (regardless of whether a screen reader was found).
  static bool IsInitialized => _initialized (line 42)

  // Whether a screen reader or SAPI is available and ready to speak.
  static bool IsAvailable => _available (line 47)

  // Loads Tolk, enables SAPI fallback, checks for speech output.
  // Must be called after SetDllDirectory points to Tolk's native DLL location.
  // Returns true if speech is available.
  static bool Initialize() (line 53)

  // Calls Tolk_Unload() and resets state flags.
  static void Shutdown() (line 87)

  // Passes text to Tolk_Output. Not filtered -- callers must use SpeechPipeline.
  // internal to prevent direct use from outside the Speech namespace.
  internal static void Say(string text, bool interrupt = true) (line 107)

  // Calls Tolk_Silence() to stop any current speech output.
  static void Stop() (line 120)
