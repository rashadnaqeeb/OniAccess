namespace OniAccess.Util {
	/// <summary>
	/// Installs Unity's Debug.Log as the logging backend.
	/// Separated from Log so that UnityEngine types are never resolved
	/// unless this class is actually used (enables standalone test runs).
	/// </summary>
	internal static class LogUnityBackend {
		public static void Install() {
			Log.LogFn = msg => UnityEngine.Debug.Log(msg);
			Log.WarnFn = msg => UnityEngine.Debug.LogWarning(msg);
			Log.ErrorFn = msg => UnityEngine.Debug.LogError(msg);
		}
	}
}
