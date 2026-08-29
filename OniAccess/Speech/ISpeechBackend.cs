namespace OniAccess.Speech {
	public interface ISpeechBackend {
		bool IsInitialized { get; }
		bool IsAvailable { get; }
		bool Initialize();
		void Shutdown();
		void Say(string text, bool interrupt);
		void Stop();
		/// <summary>
		/// Once per frame, after the frame's announcements have been queued, for
		/// backends that pace speech themselves. Most do nothing.
		/// </summary>
		void Update();
	}
}
