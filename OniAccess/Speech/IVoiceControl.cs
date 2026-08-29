namespace OniAccess.Speech {
	/// <summary>
	/// A speech backend whose voice, rate, and volume the mod chooses: the
	/// system TTS backends, not screen readers. Voices are addressed by index
	/// into the backend's current list; the settings store a voice by identifier
	/// (where the backend has one), name, and language instead, because the list
	/// reorders whenever the OS voice set changes.
	/// </summary>
	public interface IVoiceControl {
		/// <summary>Number of voices on offer, or 0 when none can be listed.</summary>
		int VoiceCount { get; }
		/// <summary>Display name of voice <paramref name="index"/>, or null on failure.</summary>
		string GetVoiceName(int index);
		/// <summary>Language tag of voice <paramref name="index"/> (typically BCP 47), or null when unknown.</summary>
		string GetVoiceLanguage(int index);
		/// <summary>A stable identifier for voice <paramref name="index"/>, or null when the backend has none.</summary>
		string GetVoiceIdentifier(int index);
		/// <summary>Index of the voice in use, or -1 when the backend is on its own default voice or it cannot be read.</summary>
		int CurrentVoice { get; }
		/// <summary>Switch to voice <paramref name="index"/>; -1 returns a backend that has one to its own default voice.</summary>
		bool SetVoice(int index);
		/// <param name="rate">Normalized rate in [0, 1]; 0.5 is the voice's default speed.</param>
		bool SetRate(float rate);
		/// <summary>The rate in use, normalized to [0, 1], or -1 on failure.</summary>
		float GetRate();
		/// <param name="volume">Normalized volume in [0, 1].</param>
		bool SetVolume(float volume);
	}
}
