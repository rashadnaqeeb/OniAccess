namespace OniAccess.Speech {
	public interface ISpeechBackend {
		bool IsInitialized { get; }
		bool IsAvailable { get; }
		bool Initialize();
		void Shutdown();
		void Say(string text, bool interrupt);
		void Stop();
	}
}
