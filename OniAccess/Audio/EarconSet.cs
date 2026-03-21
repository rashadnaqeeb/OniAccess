using System.Collections.Generic;

namespace OniAccess.Audio {
	public abstract class EarconSet {
		public abstract int Priority { get; }
		public abstract bool IsEnabled { get; }
		public abstract float Volume { get; }
		public abstract bool IsActive(HashedString overlayMode);
		public abstract List<SoundBatch> GetBatches(int cell);
	}
}
