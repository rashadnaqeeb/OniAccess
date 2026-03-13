namespace OniAccess.Audio {
	public readonly struct SoundSpec {
		public string ClipName { get; }
		public float Pitch { get; }
		public float Pan { get; }

		public SoundSpec(string clipName, float pitch = 1.0f, float pan = 0.0f) {
			ClipName = clipName;
			Pitch = pitch;
			Pan = pan;
		}
	}
}
