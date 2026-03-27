namespace OniAccess.Speech {
	/// <summary>
	/// Static facade delegating to an ISpeechBackend instance.
	/// Say() passes text directly to the backend without filtering;
	/// filtering is handled by TextFilter via SpeechPipeline.
	/// </summary>
	public static class SpeechEngine {
		private static ISpeechBackend _backend;

		public static bool IsInitialized => _backend?.IsInitialized ?? false;
		public static bool IsAvailable => _backend?.IsAvailable ?? false;

		public static bool Initialize(ISpeechBackend backend) {
			_backend = backend;
			return _backend.Initialize();
		}

		public static void Shutdown() => _backend?.Shutdown();

		internal static void Say(string text, bool interrupt = true) => _backend?.Say(text, interrupt);

		public static void Stop() => _backend?.Stop();
	}
}
