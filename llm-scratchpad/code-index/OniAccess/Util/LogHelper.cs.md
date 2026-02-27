// Logging with swappable backend.
// Defaults to Console so tests run without Unity.
// Mod.OnLoad() installs LogUnityBackend to redirect to Unity's Debug.Log.
// Output format: "[OniAccess] [DEBUG] ..." / "[OniAccess] ..." etc.
static class Log (line 8)
  private const string Prefix = "[OniAccess]" (line 9)

  // Replaceable backend functions. Tests swap these to capture output.
  internal static Action<string> LogFn (line 11)    // defaults to Console.WriteLine
  internal static Action<string> WarnFn (line 12)   // defaults to Console.WriteLine
  internal static Action<string> ErrorFn (line 13)  // defaults to Console.Error.WriteLine

  static void Debug(string msg) (line 15)   // prefixes "[OniAccess] [DEBUG]"
  static void Info(string msg) (line 16)    // prefixes "[OniAccess]"
  static void Warn(string msg) (line 17)    // prefixes "[OniAccess]"
  static void Error(string msg) (line 18)   // prefixes "[OniAccess]"
