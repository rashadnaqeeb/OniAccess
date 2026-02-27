// Installs Unity's Debug.Log as the logging backend for Log (LogHelper).
// Kept separate so UnityEngine types are never resolved unless this class is used,
// which allows the test suite to run standalone without Unity assemblies.
internal static class LogUnityBackend (line 7)
  // Replaces Log.LogFn/WarnFn/ErrorFn with UnityEngine.Debug.Log/LogWarning/LogError.
  static void Install() (line 8)
