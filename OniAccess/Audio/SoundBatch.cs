namespace OniAccess.Audio {
	public readonly struct SoundBatch {
		public SoundSpec[] Specs { get; }

		public SoundBatch(params SoundSpec[] specs) {
			Specs = specs;
		}
	}
}
