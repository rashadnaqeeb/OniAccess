using System.Collections.Generic;

namespace OniAccess.Audio {
	public class PassabilityEarconSet: EarconSet {
		public override int Priority => 1;
		public override bool IsEnabled => ConfigManager.Config.PassabilityEarcons;

		public override bool IsActive(HashedString overlayMode) => true;

		public override List<SoundBatch> GetBatches(int cell) {
			if ((Grid.Solid[cell] && !Grid.DupePassable[cell]) || Grid.DupeImpassable[cell])
				return new List<SoundBatch> { new SoundBatch(new SoundSpec("impassable")) };
			return new List<SoundBatch>();
		}
	}
}
