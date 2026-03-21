namespace OniAccess.Audio {
	public readonly struct SoundBatch {
		public SoundSpec[] Specs { get; }
		public float Volume { get; }

		public SoundBatch(params SoundSpec[] specs) {
			Specs = specs;
			Volume = 1.0f;
		}

		public SoundBatch(float volume, SoundSpec[] specs) {
			Specs = specs;
			Volume = volume;
		}
	}
}
